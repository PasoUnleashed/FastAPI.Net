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
    public class APIConfig
    {
        /// <summary>
        /// Use https?
        /// </summary>
        public bool Https { get => https; }
        bool https;
        /// <summary>
        /// Set https on/off
        /// </summary>
        /// <param name="u"></param>
        public APIConfig UseHttps(bool u)
        {
            https = u;
            return this;
        }
    }
}
