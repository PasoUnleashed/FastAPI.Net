using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    public class HttpResponse
    {
        internal bool IsErroneouss { get; set; }
        internal Exception Exception { get; set; }
        internal string Message { get; set; }
        internal int Status { get; set; }
        public static HttpResponse CreateError(int status, Exception e)
        {
            return new HttpResponse() { Status = status, Exception = e, Message = e.ToString(), IsErroneouss = true };
            
        }
        public static HttpResponse CreateError(int status, string message)
        {
            return new HttpResponse() { Status = status, Message = message.ToString(), IsErroneouss = true };
        }
        public static HttpResponse CreateSuccess(string message)
        {
            return new HttpResponse() { Message = message, Status = 200 };
        }
    }
}
