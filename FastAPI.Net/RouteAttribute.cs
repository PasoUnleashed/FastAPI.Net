using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    /// <summary>
    /// Route to a controller or handler. (if it's for a handler the path can be relative using ./`path`
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple =false)]
    public class RouteAttribute :System.Attribute
    {
        public string Route { get; set; }
        /// <summary>
        /// Route to a controller or handler. (if it's for a handler the path can be relative using ./`path`
        /// </summary>
        /// <param name="route">if it's for a handler the path can be relative using ./`path`</param>
        public RouteAttribute(string route)
        {
            Route = route.Trim('/');
        
        }
    }
}
