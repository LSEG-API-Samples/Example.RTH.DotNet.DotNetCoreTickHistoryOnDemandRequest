using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;  
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Authentication
{
    class Program
    {
        class TickHistoricalManager
        {
              public static async Task<string> GetToken(string credentialFilePath)
        {

            var returnToken="";
            var authenUri=new Uri("https://hosted.datascopeapi.reuters.com/RestApi/v1/Authentication/RequestToken");
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
        public static async Task<bool> SendRAWExtractionRequest(string dssToken, string extractionRequestContent, string outputFileName, bool autoDecompress = false,bool downloadFromAmzS3=false)
        {
             var rawExtractionUri = new Uri("https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/ExtractRaw");
            var handler = new HttpClientHandler() { AllowAutoRedirect = false, PreAuthenticate = false };
            if (autoDecompress)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpClient client = new HttpClient(handler))
            {
                // ***Step1 Send RawExtraction Request***
                Console.WriteLine("Sending RawExtraction Request");
                Console.WriteLine("Waiting for response from server...");
              
                // Create Http Request and set header and request content Set HttpMethod to Post request.
                var extractionRequest = new HttpRequestMessage(HttpMethod.Post, rawExtractionUri);
                extractionRequest.Headers.Add("Prefer", "respond-async");
                extractionRequest.Headers.Authorization = new AuthenticationHeaderValue(dssToken);
                extractionRequest.Content = new StringContent(extractionRequestContent);
                extractionRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Call SendAsync to send RAW Extraction
                var extractionResponse = await client.SendAsync(extractionRequest);
                
                Uri location = null;
                var statusResponseContent = String.Empty;
                if((extractionResponse.StatusCode != System.Net.HttpStatusCode.OK) && (extractionResponse.StatusCode != System.Net.HttpStatusCode.Accepted))
                {
                    Console.WriteLine("Request Failed Status Code:{0} Reason:{1}", extractionResponse.StatusCode, extractionResponse.ReasonPhrase);
                    return false;
                }

                if (extractionResponse.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    System.Console.WriteLine("Request Accepted");
                    location = extractionResponse.Headers.Location;
                    Console.WriteLine("Location: {0}", extractionResponse.Headers.Location);
                    Console.WriteLine("Polling Request status");
                    // *** Step2 Polling the status using the location provied with response from previous step.***
                    do
                    {
                        // Create a new HttpRequest and set HttpMethod to Get and pass location from previous steps to request Uri.
                        var extractionStatusRequest = new HttpRequestMessage(HttpMethod.Get, location);
                        extractionStatusRequest.Headers.Add("Prefer", "respond-async");
                        extractionStatusRequest.Headers.Authorization = new AuthenticationHeaderValue(dssToken);
                        var resp = await client.SendAsync(extractionStatusRequest);
                        // Show status 
                        IEnumerable<string> statusValue;
                        if (resp.Headers.TryGetValues("Status", out statusValue))
                        {
                            Console.WriteLine("Request Status:{0}", statusValue.First());
                        }
                        if (resp.StatusCode != HttpStatusCode.OK)
                            await Task.Delay(30000); // Wait for 30 sec and re-request the data accroding to TRTH Document.
                        else
                        {
                            statusResponseContent = await resp.Content.ReadAsStringAsync();
                            break;
                        }
                    } while (true);
                }
                else //200
                {
                    statusResponseContent = await extractionResponse.Content.ReadAsStringAsync();
                }

                Console.WriteLine("Request completed");
                // Deserialize response and get JobId and Notes from response message.

                var JobId = JObject.Parse(statusResponseContent)["JobId"];
                Console.WriteLine("Recevied JobID={0}\nNotes\n", JobId);
                Console.WriteLine("========================================");
                var notes = JObject.Parse(statusResponseContent)["Notes"];
                foreach (var note in notes)
                    Console.WriteLine(note);

                Console.WriteLine("========================================");

                //*** Step 3 retrieve the data from Tick Historical Server using the JobID received from previous steps.

                //Construct data retrieval Uri using the JobId
                Uri retrieveDataUri = new Uri(String.Format("https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/RawExtractionResults('{0}')/$value", (string)JobId));
                Console.WriteLine("Retreiving data from endpoint {0}", retrieveDataUri);

                // Create a new request and set HttpMethod to Get and set AcceptEncoding to gzip and defalte
                // The application will receive data as gzip stream with CSV format.
                var retrieveDataRequest = new HttpRequestMessage(HttpMethod.Get, retrieveDataUri);
                retrieveDataRequest.Headers.Authorization = new AuthenticationHeaderValue(dssToken);
    
                // Add custom header to HttpClient to download data from AWS server
                if(downloadFromAmzS3)
                    retrieveDataRequest.Headers.Add("X-Direct-Download", "True");

                retrieveDataRequest.Headers.Add("Prefer", "respond-async");
                retrieveDataRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                retrieveDataRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                Console.WriteLine("Here is request message\n {0}", retrieveDataRequest);


                var getDataResponse = await client.SendAsync(retrieveDataRequest);
                if (getDataResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Data retrieval completed\nWriting data to {0}", outputFileName);
                    using (var fileStream = File.Create(outputFileName))
                    {
                        await getDataResponse.Content.CopyToAsync(fileStream);
                    }
                    Console.WriteLine("Write data to {0} completed ", outputFileName);
                }
                else
                // Handle Redirect in case of application want to download data from amazon.
                if (getDataResponse.StatusCode == HttpStatusCode.Redirect)
                {
                    Console.WriteLine("Get Redirect, retrieving data from Amazon S3 Uri:{0}\n", getDataResponse.Headers.Location);
                    var retrieveAmzRequest = new HttpRequestMessage(HttpMethod.Get, getDataResponse.Headers.Location);
                    retrieveAmzRequest.Headers.Add("Prefer", "respond-async");
                    retrieveAmzRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    retrieveAmzRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                    var amzResponse = await client.SendAsync(retrieveAmzRequest);
                    Console.WriteLine("Amazon S3 Data retrival completed\nWriting data to {0}", outputFileName);
                    await WriteResponseToFile(amzResponse,outputFileName);
              
                }
                else
                {
                    Console.WriteLine("Unable to get data Status Code:{0} Reason:{1}", getDataResponse.StatusCode, getDataResponse.ReasonPhrase);
                    return false;
                }

            }

            return true;
        }
        public static async Task WriteResponseToFile(HttpResponseMessage responseMessage,string outputFileName)
        {
            using (var fileStream = File.Create(outputFileName))
            {
                await responseMessage.Content.CopyToAsync(fileStream);
            }
            Console.WriteLine("Write data to {0} completed ", outputFileName);
        }
        }
        static void Main(string[] args)
        {
            var dssToken="";
            try{
                dssToken=TickHistoricalManager.GetToken("./Credential.json").GetAwaiter().GetResult();
                System.Console.WriteLine("Token={0}",dssToken);
                var extractionRequestContent="";
                var requestModelPath="./TimeAndSalesRequest.json";
                if(!File.Exists(requestModelPath))
                {
                    throw new FileNotFoundException(string.Format("Unable to find {0} ",requestModelPath));
                }
                FileStream fileStream = new FileStream(requestModelPath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    extractionRequestContent = reader.ReadToEndAsync().GetAwaiter().GetResult();
                }
                System.Console.WriteLine(extractionRequestContent);
                if(TickHistoricalManager.SendRAWExtractionRequest(dssToken,extractionRequestContent,"output.csv.gz").GetAwaiter().GetResult())
                    System.Console.WriteLine("Request Completed Successful");
            }catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
      
    }
  
}
