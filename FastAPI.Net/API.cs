using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;

namespace FastAPI.Net
{
    /// <summary>
    /// Api Server. Automatically finds controllers and authentication types
    /// </summary>
    public class API : IDisposable
    {
        System.Net.HttpListener listener;
        APIConfig config;
        ConcurrentQueue<HttpListenerContext> contextQueue = new ConcurrentQueue<HttpListenerContext>();
        ControllerCollection controllers;
        Thread listenThread;
        Thread processorThread;
        Authentication.AuthenticationIdentityFactory authFact;
        bool started;
        public bool Running { get => started && listenThread.IsAlive && processorThread.IsAlive; }
        public API(APIConfig config, string host, int port)
        {
            controllers = new ControllerCollection(AppDomain.CurrentDomain);
            listener = new System.Net.HttpListener();
            authFact = new Authentication.AuthenticationIdentityFactory(AppDomain.CurrentDomain);
            listener.Prefixes.Add((config.Https ? "https://" : "http://") + host +$":{port}/");
            listenThread = new Thread(ListenLoop);
            processorThread = new Thread(ProcessorLoop);
            
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            started = true;
            listenThread = new Thread(ListenLoop);
            processorThread = new Thread(ProcessorLoop);
            
            listener.Start();
            listenThread.Start();
            processorThread.Start();
           
           
        }
        public void Stop()
        {
            started = false;
            processorThread.Join();
            listenThread.Join();
        }
        void ListenLoop()
        {
            while (true)
            {
                var context = listener.GetContext();
                contextQueue.Enqueue(context);
            }
        }

        void ProcessorLoop()
        {
            while (true)
            {
                if (contextQueue.TryDequeue(out var x))
                {
                    var args = BodyParser.ParseArgs(x.Request);
                    Authentication.AuthenticationIdentity auth = authFact.GetIdentity(x.Request.Headers);
                    controllers.HandleRequest(x,auth,args.StringParameters,args.FileParameters);
                        
                }
            }

        }

        public void Dispose()
        {
            if (started)
            {
                Stop();
                listener.Stop();

            }


        }

        ~API()
        {
            Dispose();
        }
    }      
    
}
