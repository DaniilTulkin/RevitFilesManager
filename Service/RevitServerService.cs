using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace RevitFilesManager
{
    internal class RevitServerService
    {
        public string requestAdress;
        public WebRequest request;
        public RevitServerService(string rsVersion, string rsAdress)
        {
            requestAdress = $"http://{rsAdress}/RevitServerAdminRESTService{rsVersion}/AdminRESTService.svc";
        }

        internal ServerProperties GetServerProperties()
        {
            request = WebRequest.Create(requestAdress + "/serverProperties");
            request.Method = "GET";
            AddHeaders();

            return GetResponse<ServerProperties>(request);
        }

        internal Contents GetContents(string path)
        {
            request = WebRequest.Create(requestAdress + $"/{path}/contents");
            request.Method = "GET";
            AddHeaders();
            return GetResponse<Contents>(request);
        }

        private void AddHeaders()
        {
            request.Headers.Add("User-Name", Environment.UserName);
            request.Headers.Add("User-Machine-Name", Environment.MachineName);
            request.Headers.Add("Operation-GUID", Guid.NewGuid().ToString());
        }

        private T GetResponse<T>(WebRequest request)
        {
            try
            {
                Stream stream = request.GetResponse().GetResponseStream();
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    return new JsonSerializer().Deserialize<T>(jsonTextReader);
                }
            }
            catch { return default; }
        }
    }
}
