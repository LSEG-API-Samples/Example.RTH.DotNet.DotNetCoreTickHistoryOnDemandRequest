using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;  
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Authentication
{
    class Program
    {
        static void Main(string[] args)
        {
            try{
                System.Console.WriteLine("Token={0}",GetToken("./Credential.json").GetAwaiter().GetResult());
            }catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        public static async Task<string> GetToken(string credentialFilePath)
        {
            var returnToken="";
            var authenUri=new Uri("https://selectapi.datascope.refinitiv.com/RestApi/v1/Authentication/RequestToken");
            var content="";
            if(!File.Exists(credentialFilePath))
            {
                throw new FileNotFoundException(string.Format("Unable to find {0} ",credentialFilePath));
            }
            FileStream fileStream = new FileStream(credentialFilePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                content = await reader.ReadToEndAsync();
            }
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post,authenUri);
                request.Headers.Add("Prefer", "respond-async");
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    returnToken =  (string)JObject.Parse(jsonData)["value"];
     
                }
                else
                {
                    throw new Exception(String.Format("Unable to Login to Tick Historical Server\n {0}", response.ToString()));
                }
                return String.Format("Token{0}",returnToken);
            }
        }
    }
}
