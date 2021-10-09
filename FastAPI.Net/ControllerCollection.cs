using FastAPI.Net.Authentication;
using Json.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    public class ControllerCollection
    {
        List<Type> controllers;
        List<HandlerInfo> handlers;
        PathParsing.PathBinder<HandlerInfo> pathBinder;
        /// <summary>
        /// The list of request handlers
        /// </summary>
        public List<HandlerInfo> Handlers { get => handlers; set => handlers = value; }
        /// <summary>
        /// Initiate a controller collection that contains the handlers defined in the domain
        /// </summary>
        /// <param name="domain">the domain containing the loaded assemblies where the controllers are defined</param>
        public ControllerCollection(AppDomain domain)
        {
            this.controllers = GetControllers(domain).ToList();
            var handlers = GetPaths(controllers);
            this.handlers = handlers.Select((i) => i.Item2).ToList();
            pathBinder = new PathParsing.PathBinder<HandlerInfo>(handlers.Select((i) => new KeyValuePair<string, HandlerInfo>(i.Item1, i.Item2)));


        }
        /// <summary>
        /// Get all the possible request paths
        /// </summary>
        /// <param name="controllers">list of controller types</param>
        /// <returns></returns>
        internal static IEnumerable<(string, HandlerInfo)> GetPaths(IEnumerable<Type> controllers)
        {
            List<(string, HandlerInfo)> paths = new List<(string, HandlerInfo)>();
            foreach (var i in controllers)
            {

                var robj = System.Attribute.GetCustomAttribute(i, typeof(RouteAttribute)) as RouteAttribute;
                string cpath = robj.Route.Trim('/');
                foreach (var j in i.GetMethods())
                {
                    if (j.DeclaringType != i)
                    {
                        continue;
                    }
                    string jroute;
                    TryGetHandlerRoute(j, cpath, out jroute);
                    string method = GetHandlerMethod(j);

                    string path = jroute + "/" + method;
                    paths.Add((path, new HandlerInfo(j, i, path)));
                }
            }
            return paths;
        }
        /// <summary>
        /// Try to get handler properties of method
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="root">the path of the controller</param>
        /// <param name="route"></param>
        /// <returns></returns>
        internal static bool TryGetHandlerRoute(MethodInfo handler, string root, out string route)
        {
            if (handler.GetCustomAttributes<RouteAttribute>().Count() > 0)
            {
                var r = handler.GetCustomAttribute<RouteAttribute>();
                route = r.Route;
                if (route.StartsWith("."))
                {
                    route = new String(route.Skip(1).ToArray());
                    route = root + route;
                }
                return true;
            }
            route = root;
            return false;
        }
        /// <summary>
        /// Get the http method the handler uses
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        internal static string GetHandlerMethod(MethodInfo handler)
        {
            string method = "get";
            if (handler.GetCustomAttributes().Any((i) => i is HttpMethodAttribute))
            {
                foreach (var methodatt in handler.GetCustomAttributes())
                {
                    if (methodatt is HttpGetAttribute)
                    {
                        method = "get";
                    }
                    else if (methodatt is HttpPutAttribute)
                    {
                        method = "put";
                    }
                    else if (methodatt is HttpPostAttribute)
                    {
                        method = "post";
                    }
                    else if (methodatt is HttpDeleteAttribute)
                    {
                        method = "delete";
                    }
                    else if (methodatt is HttpUpdateAttribute)
                    {
                        method = "update";
                    }
                }
            }
            return method;
        }
        /// <summary>
        /// Get all controllers defined in an appdomain
        /// </summary>
        /// <param name="domain">the domain where the controller is defined</param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetControllers(AppDomain domain)
        {
            foreach (var i in domain.GetAssemblies())
            {
                foreach (var j in i.GetTypes())
                {

                    if (typeof(Controller).IsAssignableFrom(j))
                    {
                        if (!j.IsAbstract)
                        {
                            yield return j;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Handle an incoming request using the controllers registered
        /// </summary>
        /// <param name="request"></param>
        public void HandleRequest(HttpListenerContext request, Authentication.AuthenticationIdentity auth, Dictionary<string, string> args, IEnumerable<FileParameter> files)
        {
            var path = request.Request.Url.LocalPath.Trim(' ').Trim('/');
            Console.WriteLine($"Handling request to /{path} ");
            path += "/" + request.Request.HttpMethod.ToLower();
            if (pathBinder.TryGet(path, out var res))
            {
                var args1 = res.Args;
                foreach (var i in args)
                {
                    args1[i.Key] = args[i.Key];

                }
                var rets = res.Result.GetArgs(args1);
                Controller obj = (Controller)Activator.CreateInstance(res.Result.Controller);
                obj.SetData(request.Request, auth, files);
                var response = res.Result.Method.Invoke(obj, rets);
                SubmitResponse(request, response);
                Console.WriteLine($"using {res.Result.Method.Name} in class {res.Result.Controller.Name}");
                request.Response.OutputStream.Close();
                request.Response.OutputStream.Dispose();
            }
            else
            {
                Console.WriteLine($"Unable to handle request");
                request.Response.StatusCode = 500;
                request.Response.OutputStream.Close();
                request.Response.OutputStream.Dispose();
            }
        }
        /// <summary>
        /// Submit a response returned by a handler
        /// </summary>
        /// <param name="context">the context to respond to</param>
        /// <param name="res">the response of the handler</param>
        void SubmitResponse(HttpListenerContext context, object res)
        {
            string responseText = "";
            if (res != null)
            {
                if (res is string s || res.GetType().IsValueType)
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
                        responseText = "unable to serialize";
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
            context.Response.OutputStream.Close();
            context.Response.OutputStream.Dispose();

        }
    }
   
}
