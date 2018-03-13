# Using Microsoft .NET Core with Tick History REST API

## About the example

This example is console based application and it was created for demonstrates how to use Microsoft .NET Core SDK version 2.0 with TRTH (Thomson Reuters Tick History) REST API. The example also use the REST API with direct HTTP . It can use to retrieve Historical tick data(Time and sales data) and End of day data, with an On Demand extraction request.

## About the REST API

Tick History's REST API is a Representational State Transfer (REST)-compliant API that programmatically exposes Tick History functionality on the DataScope Select platform. Client applications can be written in most programming languages, such as C#, C++, Visual Basic, Java, Python, Objective-C, and Swift. 

Basically the REST API can be used with a [REST API toolkit](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning?content=8713&type=learning_material_item) or by communicating directly with the server using HTTPS. However using REST API toolkit has some limitation when user want to deploy .NET application on cross platform environment as the toolkit was created with .NET Framework 4.x which support windows platform only. Cloud environment such as AWS Lambda supports only function code in C# using the .NET Core SDK 2.x. As a result it causes the application which built with REST API toolkit unable to works on Linux or macOS and also the AWS.  If there are requirement to deploy .NET application on other platform, .NET developer has to re-implement the application with .NET Core SDK. And they might need to use .NET Core functionality to communicate directly over HTTPS. It just like working with other programming languages such as Java and Python. More details about issues when using the REST API toolkit on macOS or Linux  will be provided in the next section.


## Limitation of REST API Toolkit

This section describing about the issue when using the REST API Toolkit on macOS or Linux. At the time we write this article, we know that the Toolkit has the following limitation.

* It supports only Windows Platform because the Toolkit still building with .NET Framework 4.x. 

* It's unable to run on macOS and Linux.

* It’s not an Open Source library therefore we are unable to modify the codes or change anything in the library.


![REST API Toolkit Limitation](./DSSTRTHArchitecture_Mac.png)


Typically you can create a new project target .NET Core 2.x on Visual Studio 2017 and then add libraries from the REST API Toolkit to the project. You should be able to build the solution or project without any error and you should be able to run the application on Windows platform.

The following sample codes for Extraction request, it works very well on Windows with .NET Core 2.0 Project type.

```cs
 var ExtractionsContext = new ExtractionsContext(new Uri("https://hosted.datascopeapi.reuters.com/RestApi/v1/"), "<UserName>", "Password");
var availableFields = ExtractionsContext.GetValidContentFieldTypes(ReportTemplateTypes.TickHistoryTimeAndSales);

//Find fields that are trade or quote related
var fields = availableFields
            .Where(x => x.FieldGroup == "Quote" || x.FieldGroup == "Trade")
            .Select(x => x.Name).ToArray();

//Request an extraction
//Reduce the poll time for the purposes of the example (to trigger async processing and exhibit a polling step).
ExtractionsContext.Preferences.WaitSeconds = 5;
ExtractionsContext.Options.AutomaticDecompression = true; //Decompress gzip to plain text
var startDate = new DateTimeOffset(2016, 9, 29, 0, 0, 0, TimeSpan.FromHours(-5)); //Central Time Zone
var endDate = startDate.AddHours(12);
var result = ExtractionsContext.ExtractRaw(
            new TickHistoryTimeAndSalesExtractionRequest
            {
                Condition = new TickHistoryTimeAndSalesCondition
                {
                        ReportDateRangeType = ReportDateRangeType.Range,
                        QueryStartDate = startDate,
                        QueryEndDate = endDate,
                        ApplyCorrectionsAndCancellations = false,
                        ExtractBy = TickHistoryExtractByMode.Ric,
                        MessageTimeStampIn = TickHistoryTimeOptions.GmtUtc,
                        SortBy = TickHistorySort.SingleByRic,
                        DisplaySourceRIC = false
                },
                ContentFieldNames = fields,
                IdentifierList = new InstrumentIdentifierList
                {
                    InstrumentIdentifiers = new[]
                    {
                        InstrumentIdentifier.Create(IdentifierType.Ric, "IBM.N"),
                    }
                },
            });

            //Download the results
            using (var response = ExtractionsContext.RawExtractionResultOperations.GetReadStream(result))
            using (var stream = response.Stream)
            using (var reader = new StreamReader(stream))
            {
                var lineCount = 0;
                while (!reader.EndOfStream && lineCount++ < 10) //Limit to ten rows for the example
                    Console.WriteLine(reader.ReadLine());
            }
```

Above sample codes working fine on Windows with .NET Core project type so you might need to know what exactly the problem is on macOS or Linux?

If you copy the same .NET Core project to macOS or Linux and rebuild it. You should see errors about unresolved reference like following errors.

```command
user1@ubuntudev1:~/workspaces/ConsoleApp8/ConsoleApp8$ dotnet build
Microsoft (R) Build Engine version 15.5.179.9764 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

Restore completed in 33.87 ms for /home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj.
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Microsoft.OData.Client". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Microsoft.OData.Core". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Microsoft.OData.Edm". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Microsoft.Spatial". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Newtonsoft.Json". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "ThomsonReuters.Dss.Core.RestApi.Common". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
/usr/share/dotnet/sdk/2.1.3/Microsoft.Common.CurrentVersion.targets(2041,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "ThomsonReuters.Dss.RestApi.Client". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
Program.cs(2,7): error CS0246: The type or namespace name 'ThomsonReuters' could not be found (are you missing a using directive or an assembly reference?) [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]
Program.cs(3,7): error CS0246: The type or namespace name 'ThomsonReuters' could not be found (are you missing a using directive or an assembly reference?) [/home/mcca/workspaces/ConsoleApp8/ConsoleApp8/ConsoleApp8.csproj]

Build FAILED.
```
At the time we write this article, it looks like the version of OData Client used by the REST API toolkit is not compatible with .NET Core 2.x. And this seems be the main problem prevent us from building the API Toolkit on macOS or Linux. There are some dependency or library which still not compatible with .NET Core SDK on macOS and Linux.

## Solution for .NET Core user

As explained earlier .NET developer can use alternative way by communicate directly over HTTPS like the other programming languages. So they can control everything including the way to send Http request and process the Http response. However the REST API user has to know what are the message they need to send and what are the Http Header required by the server. We provide document about getting started using REST API with Direct HTTP on [the DSS server](https://hosted.datascopeapi.reuters.com/RestApi.Help/Home/GettingStartedHttp) and you can find more information about the REST API specification from [REST API Reference Page](https://hosted.datascopeapi.reuters.com/RestApi.Help/Home/ExampleAppDownload?Id=1)

Currently .NET Core 2.x providing HTTPClient class which you can use to send Http request and process the HTTP response message. Also there is a new version of NewtonSoft JSON.NET library which support .NET Core target platform and fully compatible with .NET Core 2.x so that .NET developer can use the JSON library to serialize/deserialize and parsing the JSON data. 


![REST API Toolkit Solution](./DSSTRTHArchitecture_Mac_Solution.png)

In conclusion this example use the following SDK and library for implementing .NET Core application.

* .NET Core 2.0 SDK. You can find installation instruction from https://www.microsoft.com/net/core/platform

* Using [HttpClient](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient) class from .NET Core SDK to send request and get response from DSS Server.

* Using Json.Net to parse data from the Http response message. https://www.newtonsoft.com/json

### Send and Receive Http request and response
This example use the following sample codes to send Http request(Post/Get) and process Http response message using JSon.NET.

Send Http Post
```cs
  var rawExtractionUri = new Uri("https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/ExtractRaw");
  // Disable AutoRedirect in case that we need to download completed file from Amazon S3 instead. 
  // Application has to handle the HTTP Redirect itself.
  var handler = new HttpClientHandler() { AllowAutoRedirect = false, PreAuthenticate = false };
  // I
  if (autoDecompress) 
    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
    
  using (HttpClient client = new HttpClient(handler))
  {

              
    // Create Http Request and set header and request content Set HttpMethod to Post request.
    var extractionRequest = new HttpRequestMessage(HttpMethod.Post, rawExtractionUri);
    extractionRequest.Headers.Add("Prefer", "respond-async");
    extractionRequest.Headers.Authorization = new AuthenticationHeaderValue(dssToken);
    extractionRequest.Content = new StringContent(extractionRequestContent);
    extractionRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

    // Call SendAsync to send RAW Extraction
    var extractionResponse = await client.SendAsync(extractionRequest);
    ...
    // Get location and send a new request to check Request status.
    var location = extractionResponse.Headers.Location;
    ...
  }
```
Send Http Get
```cs
// Create a new HttpRequest and set HttpMethod to Get and pass location from previous steps to request Uri.
var extractionStatusRequest = new HttpRequestMessage(HttpMethod.Get, location);
extractionStatusRequest.Headers.Add("Prefer", "respond-async");
extractionStatusRequest.Headers.Authorization = new AuthenticationHeaderValue(dssToken);
var resp = await client.SendAsync(extractionStatusRequest);
...
```

Get the data from response message using JObject.Parse from NewtonSoft JSon.NET library.
```cs
//Convert response content to Json String as we know it return Content in Json format.
responseContent = await extractionResponse.Content.ReadAsStringAsync();
// Get the JobID from response message.
var JobId = JObject.Parse(statusResponseContent)["JobId"];
// Get the Note from response message and print to console
var notes = JObject.Parse(statusResponseContent)["Notes"];
foreach (var note in notes)
     Console.WriteLine(note);
```
There are an option in the example to allow user download data from Amazon S3 instead. It has to add custom header and set X-Direct-Download to "True" in Http request message in order to get HTTP Redirect to a new location on Amazon S3. Below is sample codes from the example.

```cs
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

var getDataResponse = await client.SendAsync(retrieveDataRequest);
if (getDataResponse.StatusCode == HttpStatusCode.OK)
{
    Console.WriteLine("Data retrieval completed\nWriting data to {0}", outputFileName);
    await WriteResponseToFile(getDataResponse, outputFileName);
}
else
// Handle Redirect in case of application want to download data from amazon.
if (getDataResponse.StatusCode == HttpStatusCode.Redirect)
{
    var retrieveAmzRequest = new HttpRequestMessage(HttpMethod.Get, getDataResponse.Headers.Location);
    retrieveAmzRequest.Headers.Add("Prefer", "respond-async");
    retrieveAmzRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
    retrieveAmzRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    var amzResponse = await client.SendAsync(retrieveAmzRequest);
    Console.WriteLine("Amazon S3 Data retrival completed\nWriting data to {0}", outputFileName);
    await WriteResponseToFile(amzResponse,outputFileName);
              
}
```
## Demo 

There are two console application we created to demonstrate the API Usage with .NET Core SDK. We follow the instruction to set request header and request from Tutorial Section on the [Developer Portal](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning) website.

### 1. Authentication
It's example to demonstrate how to get Authentication Token from TickHistorical Server. We follow instruction from [Tutorial 1](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning?content=8713&type=learning_material_item) to create the request message and process the response from server.
This example just send Authentication request to TRTH Server and print Token to console output.

#### How to run the example
1) Modify Credential.json in Authentication folder. Set DSS User name and DSS Password and save.
```JSON
{
    "Credentials": {
        "Username":  "<Your DSS Username>",
        "Password":  "<Your DSS Password>"
    }
}
```
2) Open Command line and move to directory Authentication
```

C:\dotnetcore\TickHistoryOnDemandRequest>cd Authentication

C:\dotnetcore\TickHistoryOnDemandRequest\Authentication>dir
 Volume in drive C is Windows
 Volume Serial Number is 5217-07DB

 Directory of C:\dotnetcore\TickHistoryOnDemandRequest\Authentication

10/18/2017  03:10 PM    <DIR>          .
10/18/2017  03:10 PM    <DIR>          ..
10/18/2017  02:32 PM               278 Authentication.csproj
10/18/2017  02:22 PM    <DIR>          bin
10/18/2017  03:42 PM               106 Credential.json
10/18/2017  02:26 PM    <DIR>          obj
10/18/2017  04:15 PM             2,241 Program.cs
               3 File(s)          2,625 bytes
               4 Dir(s)   7,452,020,736 bytes free

C:\dotnetcore\TickHistoryOnDemandRequest\Authentication>
```
3) Run dotnet core command 
```
> dotnet restore
> dotnet build
> dotnet run
```
You should see output like the following example
```

C:\dotnetcore\TickHistoryOnDemandRequest\Authentication>dotnet restore
  C:\dotnetcore\TickHistoryOnDemandRequest_001\Authentication>dotnet restore
  Restoring packages for C:\dotnetcore\TickHistoryOnDemandRequest_001\Authentication\Authentication.csproj...
  Generating MSBuild file C:\dotnetcore\TickHistoryOnDemandRequest_001\Authentication\obj\Authentication.csproj.nuget.g.props.
  Generating MSBuild file C:\dotnetcore\TickHistoryOnDemandRequest_001\Authentication\obj\Authentication.csproj.nuget.g.targets.
  Restore completed in 725.9 ms for C:\dotnetcore\TickHistoryOnDemandRequest_001\Authentication\Authentication.csproj.

C:\dotnetcore\TickHistoryOnDemandRequest\Authentication>dotnet run
Token=Token_MQN_j0vDQPXySKXx4ytFLr3cdJJU5S22LVpZauUJ-ubNdpkL1LFGsl9HIuHD2gqu059x7Kcg4RSMU4MPYlUFZyo0uGvA-QeeWLCFBN8twTMwwFVBN8UUepMR3QY49vgpHqZoVg8wy9N8wWViakdUkQZTomcM4Y7CQK_2QAzUT0Ck9JplFXvBxD8ImVEZ
agL3Kt4kK6TCm9nofqcMVdw1jYbpsWL9h8IKzB-virVhOKbzquttE9A4Ptu4bFKN7-2F3Q94x_aS-P2okKhagP4xWVzdU5u0beuwNAJIWaaw5LY

C:\dotnetcore\TickHistoryOnDemandRequest\Authentication>
```

### 2.RawExtraction
It's the example to demonstrate how to call TRTH REST API to retrieve Time and Sales data from Tick Historical Server. The example will read request query from JSON file and then pass the it to Http Request content. We will apply instruction and steps from 
[REST API Tutorial 4: On Demand tick data extraction](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning?content=11220&type=learning_material_item). Anyway we will skip the steps to request field list from server and manually set required fields in JSON file instead.

This example will retrieve data and write to file.csv.gz. You have to unpack .gz file and then find output in csv format.

#### How run the example
1) Modify Credential.json in RawExtraction folder. Set DSS User name and DSS Password and save.
```JSON
{
    "Credentials": {
        "Username":  "<Your DSS Username>",
        "Password":  "<Your DSS Password>"
    }
}
```
2) Open Command line and change directory to RawExtraction
```
 Directory of C:\dotnetcore\TickHistoryOnDemandRequest\RawExtraction

10/19/2017  08:16 AM    <DIR>          .
10/19/2017  08:16 AM    <DIR>          ..
10/18/2017  04:25 PM    <DIR>          bin
10/18/2017  03:42 PM               106 Credential.json
10/18/2017  04:25 PM    <DIR>          obj
10/19/2017  01:39 PM            11,988 Program.cs
10/18/2017  04:24 PM               276 RawExtraction.csproj
10/19/2017  10:38 AM            56,956 scb.csv.gz
10/19/2017  08:16 AM               677 TimeAndSalesRequest.json
               5 File(s)         70,003 bytes
               4 Dir(s)   7,453,138,944 bytes free

C:\dotnetcore\TickHistoryOnDemandRequest\RawExtraction>

```
3) This example read JSON request body from ExtractionReqeust.json file so you can modify ExtractionReqeust.json to request item and fields you want. The default value is for Time and Sales Tick Historical data.

4) Run dotnet core command 
```
> dotnet restore
> dotnet build
```
You should see output like the following example
```

C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction>dotnet restore
  Restoring packages for C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction\RawExtraction.csproj...
  Generating MSBuild file C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction\obj\RawExtraction.csproj.nuget.g.props.
  Generating MSBuild file C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction\obj\RawExtraction.csproj.nuget.g.targets.
  Restore completed in 1.6 sec for C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction\RawExtraction.csproj.


C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction>dotnet build
Microsoft (R) Build Engine version 15.3.409.57025 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  RawExtraction -> C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction\bin\Debug\netcoreapp2.0\RawExtraction.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.58

C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction>

```
Net step run command 
```
> dotnet run 
```
Then you should see below console output.
```
Token=Token_UwHzo7d_yfhd3TMfAJvaECUpWttHdh0pvVrWbJtwqkSANnmMAnhD4BPI3peti8CzPlDlGdjTSO12GWjaSez584qSEIhtJT4nhxwAaZx38S7BZUK-ASFgFGE5NXTV4cFT4nK4cu62-6w1fM1oZDfni0k-YQHLuDXjEK7ZrG-ES_DFjZoHfHBYK885cKdoo0-QWUFXO_TZ6AfHUrt96U9zvuQNKg2cufT4vlnnxnvWfbEHD17ojlmOo1TKnk_dqXVX3qapUuRtoRYJnbMcPKNELumKNjnGWP4RkJZLOCTg1aA
{
  "ExtractionRequest": {
    "@odata.type": "#ThomsonReuters.Dss.Api.Extractions.ExtractionRequests.TickHistoryTimeAndSalesExtractionRequest",
    "ContentFieldNames": [
      "Trade - Price",
      "Trade - Volume",
      "Trade - Exchange Time"
    ],
    "IdentifierList": {
      "@odata.type": "#ThomsonReuters.Dss.Api.Extractions.ExtractionRequests.InstrumentIdentifierList",
      "InstrumentIdentifiers": [
        {
          "Identifier": "TRI.N",
          "IdentifierType": "Ric"
        }
      ]
    },
    "Condition": {
      "MessageTimeStampIn": "GmtUtc",
      "ApplyCorrectionsAndCancellations": false,
      "ReportDateRangeType": "Range",
      "QueryStartDate": "2018-01-05T00:00:00.000Z",
      "QueryEndDate": "2018-01-05T12:00:00.000Z",
      "DisplaySourceRIC": true
    }
  }
}
Sending RawExtraction Request
Waiting for response from server...
Request Accepted
Location: https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/ExtractRawResult(ExtractionId='0x0619201e3b9b3026')
Polling Request status
Request Status:InProgress
Request Status:InProgress
Request completed
Recevied JobID=0x0619201e3b9b3026
Notes

========================================
Extraction Services Version 11.3.38375 (21f5d0209263), Built Feb 12 2018 18:41:02
User ID: 9009975
Extraction ID: 2000000020035936
Schedule: 0x0619201e3b9b3026 (ID = 0x0000000000000000)
Input List (1 items):  (ID = 0x0619201e3b9b3026) Created: 13-03-2018 12:59:13 Last Modified: 13-03-2018 12:59:13
Report Template (3 fields): _OnD_0x0619201e3b9b3026 (ID = 0x0619201e723b3026) Created: 13-03-2018 12:57:06 Last Modified: 13-03-2018 12:57:06
Schedule dispatched via message queue (0x0619201e3b9b3026), Data source identifier (D30D547FE6A4408AB1CA2A7A3ED3BF67)
Schedule Time: 13-03-2018 12:57:07
Processing started at 13-03-2018 12:57:07
Processing completed successfully at 13-03-2018 12:59:15
Extraction finished at 13-03-2018 05:59:15 UTC, with servers: tm04n01, TRTH (76.22 secs)
Instrument <RIC,TRI.N> expanded to 1 RIC: TRI.N.
Quota Message: INFO: Tick History Cash Quota Count Before Extraction: 5168; Instruments Approved for Extraction: 1; Tick History Cash Quota Count After Extraction: 5168, 1033.6% of Limit; Tick History Cash Quota Limit: 500
Manifest: #RIC,Domain,Start,End,Status,Count
Manifest: TRI.N,Market Price,2018-01-04T17:01:15.402881806Z,2018-01-04T21:01:47.604267631Z,Active,447

========================================
Retreiving data from endpoint https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/RawExtractionResults('0x0619201e3b9b3026')/$value
Here is request message
 Method: GET, RequestUri: 'https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/RawExtractionResults('0x0619201e3b9b3026')/$value', Version: 1.1, Content: <null>, Headers:
{
  Authorization: Token_UwHzo7d_yfhd3TMfAJvaECUpWttHdh0pvVrWbJtwqkSANnmMAnhD4BPI3peti8CzPlDlGdjTSO12GWjaSez584qSEIhtJT4nhxwAaZx38S7BZUK-ASFgFGE5NXTV4cFT4nK4cu62-6w1fM1oZDfni0k-YQHLuDXjEK7ZrG-ES_DFjZoHfHBYK885cKdoo0-QWUFXO_TZ6AfHUrt96U9zvuQNKg2cufT4vlnnxnvWfbEHD17ojlmOo1TKnk_dqXVX3qapUuRtoRYJnbMcPKNELumKNjnGWP4RkJZLOCTg1aA
  Prefer: respond-async
  Accept-Encoding: gzip
  Accept-Encoding: deflate
}
Data retrieval completed
Writing data to output.csv.gz
Write data to output.csv.gz completed 
Request Completed Successful


```
Unpack output.csv.gz and open output.csv you will see the output like this.
```
#RIC,Domain,Date-Time,GMT Offset,Type,Price,Volume,Exch Time
TRI.N,Market Price,2018-01-04T17:01:15.402881806Z,-5,Trade,43.97,400,17:01:15.378000000
TRI.N,Market Price,2018-01-04T17:01:15.402881806Z,-5,Trade,43.97,10,17:01:15.378000000
TRI.N,Market Price,2018-01-04T17:01:15.402916326Z,-5,Trade,43.97,3,17:01:15.378000000
TRI.N,Market Price,2018-01-04T17:01:45.134713133Z,-5,Trade,43.97,10,17:01:45.107000000
TRI.N,Market Price,2018-01-04T17:03:05.372659867Z,-5,Trade,43.96,8,17:03:05.344000000
TRI.N,Market Price,2018-01-04T17:05:57.028864686Z,-5,Trade,43.97,200,17:05:57.004000000
TRI.N,Market Price,2018-01-04T17:06:02.972656277Z,-5,Trade,43.97,195,17:06:02.944000000
TRI.N,Market Price,2018-01-04T17:07:17.044540435Z,-5,Trade,43.98,100,17:07:17.018000000
TRI.N,Market Price,2018-01-04T17:07:19.324705967Z,-5,Trade,43.98,100,17:07:19.296000000
TRI.N,Market Price,2018-01-04T17:07:19.324705967Z,-5,Trade,43.98,100,17:07:19.296000000
...
...
TRI.N,Market Price,2018-01-04T20:59:49.454807291Z,-5,Trade,44.02,200,20:59:49.430000000
TRI.N,Market Price,2018-01-04T20:59:50.386605035Z,-5,Trade,44.02,100,20:59:50.359000000
TRI.N,Market Price,2018-01-04T20:59:53.571130346Z,-5,Trade,44.03,100,20:59:53.543000000
TRI.N,Market Price,2018-01-04T20:59:55.139125865Z,-5,Trade,44.03,69,20:59:55.099000000
TRI.N,Market Price,2018-01-04T20:59:55.151372203Z,-5,Trade,44.03,12,20:59:55.115000000
TRI.N,Market Price,2018-01-04T21:01:47.603737654Z,-5,Trade,44.03,26850,21:01:47.578000000
TRI.N,Market Price,2018-01-04T21:01:47.604267631Z,-5,Trade,44.03,,21:01:47.578000000

```
The RAWExtraction example also provide EODDataExtraction.json which is sample request for End of Day data. You can just modify EODDataExtraction.json and then replace ExtractionRequest.json with the file.