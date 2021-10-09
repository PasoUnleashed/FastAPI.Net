using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FastAPI.Net
{
    /// <summary>
    /// Container object for parsing bodies
    /// </summary>
    public class RequestArgs
    {

        List<FileParameter> fileParameters;
        Dictionary<string, string> stringParameters;
        public RequestArgs(Dictionary<string, string> stringParams, IEnumerable<FileParameter> files)
        {
            this.fileParameters = files.ToList();
            this.stringParameters = stringParams;
        }
        /// <summary>
        /// The files in the parsed body
        /// </summary>
        public List<FileParameter> FileParameters { get => fileParameters; set => fileParameters = value; }
        /// <summary>
        /// the value parameters parsed from a the body
        /// </summary>
        public Dictionary<string, string> StringParameters { get => stringParameters; set => stringParameters = value; }
    }

    public class BodyParser
    {
        /// <summary>
        /// Parses the body of a request, raw text will be called "text", raw json will be called "json"
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RequestArgs ParseArgs(HttpListenerRequest request)
        {
            List<FileParameter> files = new List<FileParameter>();
            Dictionary<string, string> ret = new Dictionary<string, string>();
            var content = request.ContentType;
            var paramsurl = GetUrlData(request);
            foreach (var i in paramsurl)
            {
                ret[i.Key] = i.Value;
            }
            if (content != null)
            {
                if (content.StartsWith("multipart/form-data"))
                {
                    (var f, var p) = GetMultipartForm(request);
                    files = f;
                    foreach (var i in p)
                    {
                        ret[i.Key] = i.Value;
                    }

                }
                else if (content == "application/x-www-form-urlencoded")
                {
                    var p = GetURLEncoded(request);
                    foreach (var i in p)
                    {
                        ret[i.Key] = i.Value;
                    }
                }
                else if (content == "application/json")
                {
                    ret["json"] = GetRawBody(request);

                }
                else if (content.StartsWith("text/"))
                {
                    ret["text"] = GetRawBody(request);
                }
            }
            return new RequestArgs(ret, files);
        }
        /// <summary>
        /// Parse body url-endcoded parameters
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static Dictionary<string, string> GetURLEncoded(HttpListenerRequest request)
        {
            var ret = new Dictionary<string, string>();
            using (System.IO.StreamReader r = new System.IO.StreamReader(request.InputStream))
            {
                var qs = HttpUtility.ParseQueryString(r.ReadToEnd());
                foreach (var i in qs.AllKeys)
                {
                    ret[i] = qs[i];
                }
            }
            return ret;
        }
        /// <summary>
        /// Get multipart files and args
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static (List<FileParameter>, Dictionary<string, string>) GetMultipartForm(HttpListenerRequest request)
        {

            var ret = new Dictionary<string, string>();
            var o = MultipartFormDataParser.Parse(request.InputStream);
            foreach (var i in o.Parameters)
            {
                ret[i.Name] = i.Data;
            }
            return (GetFiles(o).ToList(), ret);
        }
        /// <summary>
        /// Get the files in multipart form
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static IEnumerable<FileParameter> GetFiles(MultipartFormDataParser p)
        {
            foreach (var i in p.Files)
            {
                yield return new FileParameter(i.Data, i.Name, i.ContentType);
            }
        }
        /// <summary>
        /// Gets the raw body string
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static string GetRawBody(HttpListenerRequest request)
        {

            using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream))
            {
                return reader.ReadToEnd();

            }
        }
        /// <summary>
        /// Get url args
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static Dictionary<string, string> GetUrlData(HttpListenerRequest request)
        {
            var requeststring = request.Url.Query;
            var data = HttpUtility.ParseQueryString(requeststring);
            var ret = new Dictionary<string, string>();
            foreach (var i in data.AllKeys)
            {
                ret[i] = data[i];
            }
            return ret;
        }
    }

    public class FileParameter : IDisposable
    {
        string name;
        string id;
        string path;
        bool saved = false;
        /// <summary>
        /// Reads a file parameter and temporarily stores it until saved or disposed
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="name"></param>
        /// <param name="contentType"></param>
        public FileParameter(System.IO.Stream stream, string name, string contentType)
        {
            id = RandomString(20);
            path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), id + System.IO.Path.GetExtension(name));
            this.name = name;
            using (var f = System.IO.File.OpenWrite(path))
            {
                stream.CopyTo(f);
            }

        }
        private static Random random = new Random();
        /// <summary>
        /// Name of the file in parameter
        /// </summary>
        public string Name { get => name; }
        /// <summary>
        /// the random id generated for this file
        /// </summary>
        public string Id { get => id; }
        /// <summary>
        /// the path of the file currently
        /// </summary>
        public string Path { get => path; }
        /// <summary>
        /// Generate a random name for the temporary file
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {

            System.IO.File.Move(this.path, path);
            this.path = path;
            saved = true;
        }
        /// <summary>
        /// Delete the temporary file
        /// </summary>
        ~FileParameter()
        {
            Dispose();
        }
        /// <summary>
        /// Delete the temporary file
        /// </summary>
        public void Dispose()
        {
            if (!saved)
            {
                System.IO.File.Delete(path);
                Console.WriteLine("file deleted");
            }
        }
    }
}
