using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    /// <summary>
    /// Http method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false)]
    public abstract class HttpMethodAttribute:System.Attribute
    {
       
    }
    /// <summary>
    /// this handler uses GET
    /// </summary>
    public class HttpGetAttribute : HttpMethodAttribute
    {

    }
    /// <summary>
    /// this handler uses POST
    /// </summary>
    public class HttpPostAttribute : HttpMethodAttribute
    {

    }
    /// <summary>
    /// this handler uses POST
    /// </summary>
    public class HttpPutAttribute : HttpMethodAttribute
    {

    }
    /// <summary>
    /// This handler uses DELETE
    /// </summary>
    public class HttpDeleteAttribute : HttpMethodAttribute
    {

    }
    /// <summary>
    /// thus HANDLER uses UPDATE
    /// </summary>
    public class HttpUpdateAttribute : HttpMethodAttribute
    {

    }

}
