using FastAPI.Net.Authentication;
using Json.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    public abstract class Handler
    {
        Server server;
        public Handler(Server server)
        {
            this.server = server;
        }

        protected Server Server { get => server;  }

        public abstract bool TryHandle(HttpListenerContext context, AuthenticationIdentity identity);

        /// <summary>
        /// Submit a response returned by a handler
        /// </summary>
        /// <param name="context">the context to respond to</param>
        /// <param name="res">the response of the handler</param>
        protected void SubmitResponse(HttpListenerContext context, object res)
        {
            string responseText = "";
            if (res != null)
            {
                if(res is HttpResponse response)
                {
                    context.Response.StatusCode = response.Status;
                    if (!response.IsErroneouss||this.server.Config.SendExceptions)
                    {
                        using (System.IO.StreamWriter w = new System.IO.StreamWriter(context.Response.OutputStream))
                        {
                            w.Write(response.Message);
                        }
                    }
                    if (response.Exception != null)
                    {
                        throw response.Exception;
                    }
                }else if (res is string s || res.GetType().IsValueType)
                {

                    responseText = res.ToString();
                }
                else if (!res.GetType().IsValueType)
                {
                    try
                    {
                        responseText = JsonNet.Serialize(res);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                
                if (responseText != "")
                {
                    using (System.IO.StreamWriter r = new System.IO.StreamWriter(context.Response.OutputStream))
                    {
                        r.Write(responseText);
                    }
                }
            }
          
        }
    }
    public class APIHandler : Handler
    {
        ControllerCollection collection;
        
        public APIHandler(Server server, AppDomain domain) : base(server)
        {
            
            collection = new ControllerCollection(domain);
        }
        public override bool TryHandle(HttpListenerContext context, AuthenticationIdentity identity)
        {
            
            if (collection.TryGetHandler(context, out var res))
            {
                Console.WriteLine($"Handling request to {context.Request.Url.ToString().Trim().Trim('\n')} using {res.Result.Method.Name} in class {res.Result.Controller.Name}");
                var args = res.Args;
                foreach(var i in args.ToArray())
                {
                    
                    args[i.Key.Trim('}').Trim('{')] = i.Value;
                    args.Remove(i.Key);
                }
                var bodyArgs = BodyParser.ParseArgs(context.Request);
                foreach (var i in bodyArgs.StringParameters)
                {
                    args[i.Key] = i.Value;
                }
                var rets = res.Result.GetArgs(args);
                Controller obj = (Controller)Activator.CreateInstance(res.Result.Controller);
                obj.SetData(context.Request, identity, bodyArgs.FileParameters);
                try
                {
                    
                    var methodAttributes = System.Attribute.GetCustomAttributes(res.Result.Method).Where((i)=>i is PermissionAtrribute);
                    bool passedRAuth = false, passedWAuth = false;
                    if (methodAttributes.Count() > 0 && identity != null)
                    {

                        var level = identity.Level;

                        foreach (var i in methodAttributes)
                        {
                            if (i is ReadAttribute r && level.ReadLevel >= r.Level)
                            {

                                passedRAuth = true;

                            }
                            else if (i is WriteAttribute w && level.WriteLevel >= w.Level)
                            {
                                passedWAuth = true;
                            }
                        }
                        if (!passedRAuth)
                        {
                            passedRAuth = !methodAttributes.Any((i) => i is ReadAttribute);

                        }
                        if (!passedWAuth)
                        {
                            passedWAuth = !methodAttributes.Any((i) => i is WriteAttribute);
                        }

                    }
                    else
                    {
                        passedRAuth = true;
                        passedWAuth = true;
                    }
                    if (passedWAuth && passedRAuth)
                    {
                        var response = res.Result.Method.Invoke(obj, rets);
                        SubmitResponse(context, response);
                    }
                    else
                    {
                        SubmitResponse(context, HttpResponse.CreateError(403,"Unauthorized access level"));
                    }
                }
                catch(Exception e)
                {
                    if (this.Server.Config.SendExceptions)
                    {
                        
                    }
                    Console.WriteLine(e);
                    SubmitResponse(context, HttpResponse.CreateError(500, e.InnerException));
                }
                return true;

            }
            return false;
        }
    }
}
