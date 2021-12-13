---
title: webHelper.ps1
description: Details about the webHelper.ps1 helper script
---

# webHelper.ps1

## Overview

The `webHelper.ps1` script has functionality to upload and download from web endpoints

## Functions

* UploadFileAndGetStringResponse([string]$url, [string]$file)
* UploadFolderAsZipAndGetStringResponse([string]$url, [string]$sourcePath)
* UploadValuesAndGetStringResponse([string]$url, [System.Collections.Specialized.NameValueCollection]$values)
* DownloadFile([string]$url, [string]$destinationPath)
* DownloadString([string]$url)
* DownloadStringUntilOK([string]$url, [int] $maxChecks, [int] $sleepSeconds, [String[]] $okText, [String[]] $failText)
