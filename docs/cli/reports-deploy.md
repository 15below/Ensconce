---
title: Reports Deployment
description: How to deploy reports
linkText: Reports Deployment
---

# Reports Role

## Overview

Ensconce can configure reports being deployed to SQL Server Reporting Services (SSRS).

As part of the deployment subscriptions can be added using a Ensconce defined `SubInfo` file.

Whilst deploying the reports, any connection information for the report database will be updated to ensure reports run against the correct database.

## Syntax

`--dr --rsv reportingServicesUrl=[urlToReportingServices] --rsv networkDomain=[authenticationDomain] --rsv networkLogin=[authenticationUser] --rsv networkPassword=[authenticationPassword] --rsv parentFolder=[reportRootFolder] --rsv subFolder=[reportSubFolder] --rsv dataSourceName=[reportDataSource] --rsv  ataSourceConnectionString=[reportDataSourceConnectionString] --rsv dataSourceUserName=[reportDataSourceUserName] --rsv dataSourcePassword= [reportDataSourcePassword] --rsv reportSourceFolder=[pathToReports]`

## SubInfo Files

A `SubInfo` file is a text file with the `.subinfo` extension with a key/value store of subscription information.

The available sub info is as follows

* `subscriptionOn` - either `true` or `false`
* `eventType` - The SSRS event type, for example `TimedSubscription`
* `scheduleXml` - The report timed subscription schedule in XML format
* `subscriptionType` - The type of report, either `FileShare`, `CSV` or `Email` (the default is `Email` if not provided)
* `subjectPrefix` - A prefix to apply to email report subscriptions
* `emailBodyText` - The text to include in the body of email report subscriptions
* `subscriptionSendTo` - semi colon separated list of email report subscription recipients
* `subscriptionCCto` - semi colon separated list of email report subscription recipients who will be in CC
* `subscriptionBCCto` - semi colon separated list of email report subscription recipients who will be in BCC
* `subscriptionRenderFormat` - The report renderer format supported by your reporting services instance, defaults to `CSV`
* `subscriptionToFile_FilePath` - File path to write reports to
* `subscriptionToFile_UserName` - Username to run file save as
* `subscriptionToFile_Password` - Password for user to save files
* `subscriptionToFile_FileName` - Filename prefix on saved reports
* `reportParameters` - a semi colon separated string of report parameters in the format `Key=Value`
