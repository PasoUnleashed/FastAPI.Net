using System;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Linq;
using Json.Net;
using System.Web;
using HttpMultipartParser;
using Json;
using FastAPI.Net.Authentication;

namespace FastAPI.Net
{



    /// <summary>
    /// A class to be inherited by a url controller
    /// </summary>
    public abstract class Controller
    {
        /// <summary>
        /// The request this controller is currently handling
        /// </summary>
        HttpListenerRequest request;

        Authentication.AuthenticationIdentity identity;
        IEnumerable<FileParameter> files;
        /// <summary>
        /// The authentication identity of the client (if no identity is found) this property will be null
        /// </summary>
        protected AuthenticationIdentity Identity { get => identity; }
        /// <summary>
        /// The original http request
        /// </summary>
        protected HttpListenerRequest Request { get => request; }
        /// <summary>
        /// If the request body is a multipart form, this will contain any files sent by the client.
        /// </summary>
        protected IEnumerable<FileParameter> Files { get => files; }
        /// <summary>
        /// Set the data of the controller after creating an instnace.
        /// </summary>
        /// <param name="request">the request</param>
        /// <param name="auth">auth identity if found (or null)</param>
        /// <param name="files">the list of files received</param>
        internal void SetData(HttpListenerRequest request, AuthenticationIdentity auth, IEnumerable<FileParameter> files)
        {
            if (this.request == null)
            {
                this.request = request;
                identity = auth;
                this.files = files;
            }

        }
    }

    /// <summary>
    /// Stores the information of a handler
    /// </summary>
    public class HandlerInfo
    {


        MethodInfo method;
        Type controller;
        string path;
        public HandlerInfo(MethodInfo info, Type controller,string path)
        {
            this.controller = controller;
            this.method = info;
            this.path = path;
        }
        /// <summary>
        /// Parse the parameters from a dictionary of string args
        /// </summary>
        /// <param name="args">the args where f(x,...,z) where any args can be named {x} or x in the dictioanry</param>
        /// <returns></returns>
        public object[] GetArgs(Dictionary<string,string> args)
        {
            var param = method.GetParameters();
            //try to get parameter from args array
            string getParam(string name)
            {
                var whr = args.Where((i) => i.Key == name || $"{{{i.Key}}}" == i.Key);
                if (whr.Count() > 0)
                {
                    return whr.First().Value;
                }
                return "";
            }
            object[] ret = new object[param.Count()]; 
            for(int i = 0; i < ret.Length; i++)
            {
                ret[i] = getParam(param[i].Name);
                if (!param[i].ParameterType.IsValueType)
                {
                    if (param.Count((p) => !p.ParameterType.IsValueType) == 1 && ((ret[i].ToString()) == ""))
                    {
                        ret[i] = args["json"];
                    }
                    var convtype = typeof(JsonNet).GetMethods().First((m)=>m.Name== nameof(JsonNet.Deserialize)&&m.GetParameters().Count((x)=>!x.IsOptional)==1&&m.GetParameters()[0].ParameterType==typeof(string));
                    var a=convtype.MakeGenericMethod(param[i].ParameterType);
                    Console.WriteLine($"ret:{ret[i]}");
                   
                    ret[i] = a.Invoke(null,new object[] { ((string)ret[i]),null });
                }
                else
                {
                    if (param[i].ParameterType == typeof(int))
                    {
                        try
                        {
                            ret[i] = int.Parse((string)ret[i]);
                        }
                        catch { ret[i] = 0; }
                    }
                    else if (param[i].ParameterType == typeof(float))
                    {
                        try
                        {
                            ret[i] = float.Parse((string)ret[i]);
                        }
                        catch { ret[i] = 0; }
                    }
                    else if (param[i].ParameterType == typeof(double))
                    {
                        try
                        {
                            ret[i] = double.Parse((string)ret[i]);
                        }
                        catch { ret[i] = 0; }
                    }
                    if (param[i].ParameterType == typeof(string))
                    {
                        if (ret[i].ToString() == "" && param.Count((j) => j.ParameterType == typeof(string)) == 1 && args.ContainsKey("text") && args["text"] != "") 
                        {
                            ret[i] = args["text"];
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// The method (the handler)
        /// </summary>
        public MethodInfo Method { get => method; set => method = value; }
        /// <summary>
        /// The controller type that defines it
        /// </summary>
        public Type Controller { get => controller; set => controller = value; }
        /// <summary>
        /// The path to this handler
        /// </summary>
        public string Path { get => path; set => path = value; }
    }
    
    
}
