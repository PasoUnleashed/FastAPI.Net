using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastAPI.Net
{
    /// <summary>
    /// Configuration of an api server
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// Use https?
        /// </summary>
        public bool Https { get => https; }
        bool https;

        bool sendExceptions = false;
        public bool SendExceptions { get => sendExceptions; }
        /// <summary>
        /// Set https on/off
        /// </summary>
        /// <param name="u"></param>
        public ServerConfig UseHttps(bool u)
        {
            https = u;
            return this;
        }
        /// <summary>
        /// Send exceptions in response body?
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public ServerConfig VerboseExceptions(bool v)
        {
            this.sendExceptions = v;
            return this;
        }
    }
}
