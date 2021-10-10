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
        public bool TryGetHandler(HttpListenerContext context,out PathParsing.PathBinder<HandlerInfo>.PathBindingResult handler)
        {
            var path = context.Request.Url.LocalPath.Trim(' ').Trim('/');

            path += "/" + context.Request.HttpMethod.ToLower();
            return pathBinder.TryGet(path, out handler);
        }
        
        
        
    }
   
}
