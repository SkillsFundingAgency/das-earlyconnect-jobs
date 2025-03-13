## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Das-EarlyConnect-Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/_projectname_?branchName=master)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=_projectid_&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=_projectId_&metric=alert_status)](https://sonarcloud.io/dashboard?id=_projectId_)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=564&projectKey=_projectKey_)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/_pageurl_)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

## Description

The Early Connect Project aims to bridge the gap between young people in education and apprenticeship opportunities by connecting them with employers and providers at an earlier stage. This initiative, in collaboration with UCAS, is part of a broader program that includes employer and school engagement.

Key Highlights:
Provides a new way for students in schools and colleges to explore apprenticeship options.
Collaborates with UCAS to enhance accessibility to apprenticeship pathways. (As of Nov 2024 UCAS are no longer part of this system)
Includes a digital workstream alongside employer and school engagement efforts.
Pilot launch in Autumn 2023, targeting three regions: North East, Lancashire, and London.
This project is designed to create stronger connections between education and employment, helping young people make informed career choices at the right time.`

## How It Works

Early connect: GAA

A collection of functions, API’s and micro-site that supports a triage form where users can enter their details and get sent 

When survey data is sent to the LEP platform that has matched the postcode in the survey, a representative of the LEP will contact the student and fill in details about the candidate. On a weekly basis this data is sent to DfE via an email with the data attached in CSV format

This file is received by a member of the team and copied over to azure blob storage. The process of copying this file to blob storage fires an event which kicks off the ImportStudentFeedback webjob. This job will store the csv into a data table in SQL Server

## 🚀 Installation

### Pre-Requisites
```
* A clone of this repository
* A code editor that supports .Net8.0
* An Azure Service Bus instance
* The [das-earlyconnect-api](https://github.com/SkillsFundingAgency/das-earlyconnect-api) API available either running locally or accessible in an Azure tenancy    
```
### Config

This utility uses the standard Apprenticeship Service configuration. All configuration can be found in the [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config).


Azure Table Storage config

Row Key: SFA.DAS.EarlyConnect.Jobs_1.0

Partition Key: LOCAL

Data:

```json
{
   "OuterApiConfiguration": {
    "BaseUrl": "https://*********.apprenticeships.education.gov.uk/",
    "Key": "123456789abcdefg"
  },
  "Functions": {
    "ExportMetricsDataJobSchedule": "0 0 * * 0",
    "ReminderEmailJobSchedule": "0 0 * * *"
  },
  "Containers": {
    "MetricsDataSourceContainer": "import-metricsdata",
    "MetricsDataArchivedCompletedContainer": "archived-completed-metricsdata",
    "MetricsDataArchivedFailedContainer": "archived-error-metricsdata",
    "MetricsDataExportContainer": "export-metricsdata",
    "StudentFeedbackSourceContainer": "import-studentfeedback",
    "StudentFeedbackArchivedCompletedContainer": "archived-completed-studentfeedback",
    "StudentFeedbackArchivedFailedContainer": "archived-error-studentfeedback"
  }
}
```
Check [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config) for subscription keys and LEPS codes.
## 🔗 External Dependencies


## Technologies

```
* .NET 8.0  
* Azure Functions (v4)  
* Application Insights  
* Azure Storage (Blobs, Queues, Tables)  
* Azure Timer Trigger  
* Azure Table Storage Configuration  
* NUnit
* Moq
```

## 🐛 Known Issues


```

```
