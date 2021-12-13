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
using FastAPI.Net.Authentication;

namespace FastAPI.Net
{
    /// <summary>
    /// Api Server. Automatically finds controllers and authentication types
    /// </summary>
    public class Server : IDisposable
    {
        System.Net.HttpListener listener;
        ServerConfig config;
        ConcurrentQueue<HttpListenerContext> contextQueue = new ConcurrentQueue<HttpListenerContext>();
        APIHandler apiHandler;
        Thread listenThread;
        Thread processorThread;
        Authentication.AuthenticationIdentityFactory authFact;
        bool started;
        public bool Running { get => started && listenThread.IsAlive && processorThread.IsAlive; }
        public ServerConfig Config { get => config;  }

        public Server(ServerConfig config, string host, int port)
        {
            this.config = config;
            apiHandler = new APIHandler(this,AppDomain.CurrentDomain);
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
                   
                    Authentication.AuthenticationIdentity auth = authFact.GetIdentity(x.Request.Headers);
                    try
                    {
                        if (!apiHandler.TryHandle(x, auth))
                        {
                            x.Response.StatusCode = 404;
                            using (System.IO.StreamWriter w = new System.IO.StreamWriter(x.Response.OutputStream))
                            {
                                w.WriteLine("Resource not found!");
                            }
                            Console.WriteLine($"404: Resource not found @{x.Request.Url}");
                        }
                        
                    }catch(Exception e)
                    {
                        x.Response.StatusCode = 500;
                        Console.WriteLine(e);
                        Console.WriteLine(e.InnerException);
                    }
                    finally
                    {
                        x.Response.OutputStream.Close();
                        x.Response.OutputStream.Dispose();
                    }
                        
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

        

        ~Server()
        {
            Dispose();
        }
    }      
    
}
