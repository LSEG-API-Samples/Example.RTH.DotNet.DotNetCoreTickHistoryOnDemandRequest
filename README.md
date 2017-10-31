# Tick Historical On Demand Extraction Request with .NET Core

## Overview
This example was designed to demonstrates how to use .NET Core SDK to call Tick Historical REST API and send on demand extraction request to retrieve Times and Sales data from Tick Historical Server. Though TickHistorical REST API provides REST API Toolkit for .NET developer but it seems to not compatible with .NET Core.

## Limitation of REST API Toolkit

* It supports only Windows Platform because the Toolkit still using .NET Framework 4.x. 

* Unable to deploy and run on Mac OS and Linux.

* It’s not Open Source for now therefore client unable to rebuild the library using the new version of .NET framework and dependency libraries.

![REST API Toolkit Limitation](./DSSTRTHArchitecture_Mac.png)

## Solution for .NET Core user

* Install .NET Core 2.0 SDK using package and find installation instruction from https://www.microsoft.com/net/core/platform

* Using HttpClient class from .NET Core SDK to send request and get response from DSS Server.

* Using Json.Net to serialize/deserialize and parse the data from the Http response message. https://www.newtonsoft.com/json

![REST API Toolkit Limitation](./DSSTRTHArchitecture_Mac_Solution.png)


## Demo 

There are two console application we created to demonstrate the API Usage with .NET Core SDK. We follow the instruction to set request header and request from Tutorial Section on the [Developer Portal](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning) website.

### 1. Authentication
It's example to demonstrate how to get Authentication Token from TickHistorical Server. We follow instruction from [Tutorial 1](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning?content=8713&type=learning_material_item). 
This example just send Authentication request to TRTH Server and print Token to console output.

#### How run the example
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
[REST API Tutorial 4: On Demand tick data extraction](https://developers.thomsonreuters.com/thomson-reuters-tick-history-trth/thomson-reuters-tick-history-trth-rest-api/learning?content=11220&type=learning_material_item). Anyway we will skip the steps to request filed list from server and set everything in JSON file instead.

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
2) Open Command line and move to directory RawExtraction
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
3) Run dotnet core command 
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
You should see output like the following one.
```

C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction>dotnet run
Token=Token_NxdIxil3zHVePYZgYUXYMjj6_TBvKB1Xabboav9Evf_XDe5R7WHF6ju6sy4q1VYcIZADTUQKv9mP1Z-cy1AGNXzafXUuXoLSu4pcG6wmzNJBXU-bhfq4Y90lBZmqHiaHqCP3Gq7x11hVgtMCcnbKujuHiOHrlKl1kFoX_5JR0IaNaSDwHw5eIqIvy7NK
pUnHWdilWZ7KhcCo3Ah9CsEpFpUGoOg3ZVOOWEUu5zBo3C0KnORBrUGuza5NXEIsQGiWZD6-puh5jLgafDpWBL_89gNCe8DUbjReWmhbkB3q3P4
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
          "Identifier": "CARR.PA",
          "IdentifierType": "Ric"
        }
      ]
    },
    "Condition": {
      "MessageTimeStampIn": "GmtUtc",
      "ApplyCorrectionsAndCancellations": false,
      "ReportDateRangeType": "Range",
      "QueryStartDate": "2016-09-29T00:00:00.000Z",
      "QueryEndDate": "2016-09-29T12:00:00.000Z",
      "DisplaySourceRIC": true
    }
  }
}
Sending RawExtraction Request
Waiting for response from server...
Request Accepted
Location: https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/ExtractRawResult(ExtractionId='0x05ee4a1a05fb2f96')
Polling Request status
Request completed
Recevied JobID=0x05ee4a1a05fb2f96
Notes

========================================
Extraction Services Version 11.2.37632 (2ea496e1b3fa), Built Oct 12 2017 19:50:52
User ID: 9009975
Extraction ID: 2000000004299610
Schedule: 0x05ee4a1a05fb2f96 (ID = 0x0000000000000000)
Input List (1 items):  (ID = 0x05ee4a1a05fb2f96) Created: 31-10-2017 10:52:39 Last Modified: 31-10-2017 10:52:39
Report Template (3 fields): _OnD_0x05ee4a1a05fb2f96 (ID = 0x05ee4a1a0bcb2f96) Created: 31-10-2017 10:52:03 Last Modified: 31-10-2017 10:52:03
Schedule dispatched via message queue (0x05ee4a1a05fb2f96), Data source identifier (F1C34C7A4B094D1795B71E52AB5BF966)
Schedule Time: 31-10-2017 10:52:04
Processing started at 31-10-2017 10:52:04
Processing completed successfully at 31-10-2017 10:52:41
Extraction finished at 31-10-2017 03:52:41 UTC, with servers: tm04n01
Instrument <RIC,CARR.PA> expanded to 1 RIC: CARR.PA.
Quota Message: INFO: Tick History Cash Quota Count Before Extraction: 1057; Instruments Approved for Extraction: 1; Tick History Cash Quota Count After Extraction: 1057, 211.4% of Limit; Tick History
Cash Quota Limit: 500
Manifest: #RIC,Domain,Start,End,Status,Count
Manifest: CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,2016-09-29T11:59:46.542132243Z,Active,3620

========================================
Retreiving data from endpoint https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/RawExtractionResults('0x05ee4a1a05fb2f96')/$value
Here is request message
 Method: GET, RequestUri: 'https://hosted.datascopeapi.reuters.com/RestApi/v1/Extractions/RawExtractionResults('0x05ee4a1a05fb2f96')/$value', Version: 1.1, Content: <null>, Headers:
{
  Authorization: Token_NxdIxil3zHVePYZgYUXYMjj6_TBvKB1Xabboav9Evf_XDe5R7WHF6ju6sy4q1VYcIZADTUQKv9mP1Z-cy1AGNXzafXUuXoLSu4pcG6wmzNJBXU-bhfq4Y90lBZmqHiaHqCP3Gq7x11hVgtMCcnbKujuHiOHrlKl1kFoX_5JR0IaNaSDwH
w5eIqIvy7NKpUnHWdilWZ7KhcCo3Ah9CsEpFpUGoOg3ZVOOWEUu5zBo3C0KnORBrUGuza5NXEIsQGiWZD6-puh5jLgafDpWBL_89gNCe8DUbjReWmhbkB3q3P4
  Prefer: respond-async
  Accept-Encoding: gzip
  Accept-Encoding: deflate
}
Data retrieval completed
Writing data to output.csv.gz
Write data to output.csv.gz completed
Request Completed Successful

C:\dotnetcore\TickHistoryOnDemandRequest_001\RawExtraction>

```
Unpack output.csv.gz and open output.csv you will see the output like this.
```
#RIC,Domain,Date-Time,GMT Offset,Type,Price,Volume,Exch Time
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,63,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,64,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,27,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,2115,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,21,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,21,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.663334842Z,+2,Trade,23.25,11,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.664006387Z,+2,Trade,23.25,61,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.664006387Z,+2,Trade,23.25,235,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.664006387Z,+2,Trade,23.25,459,07:00:11.000000000
CARR.PA,Market Price,2016-09-29T07:00:11.667347943Z,+2,Trade,23.25,566,07:00:11.000000000
...
...
CARR.PA,Market Price,2016-09-29T11:53:40.647678437Z,+2,Trade,23.255,517,11:53:40.000000000
CARR.PA,Market Price,2016-09-29T11:54:18.830999286Z,+2,Trade,23.26,227,11:54:18.000000000
CARR.PA,Market Price,2016-09-29T11:54:18.830999286Z,+2,Trade,23.26,66,11:54:18.000000000
CARR.PA,Market Price,2016-09-29T11:54:19.286308486Z,+2,Trade,23.255,66,11:54:19.000000000
CARR.PA,Market Price,2016-09-29T11:59:46.342822841Z,+2,Trade,23.25,8,11:59:46.000000000
CARR.PA,Market Price,2016-09-29T11:59:46.342822841Z,+2,Trade,23.25,207,11:59:46.000000000
CARR.PA,Market Price,2016-09-29T11:59:46.542132243Z,+2,Trade,23.245,182,11:59:46.000000000
```
