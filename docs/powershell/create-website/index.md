---
title: createWebsite.ps1
description: Details about the createWebsite.ps1 helper script
---

# createWebsite.ps1

## Overview

The `createWebsite.ps1` allows for interaction with IIS greater than or equal to version 7.0

This section will detail the functions that can be performed on IIS.

## Functions

* AddAuthoringRule([string] $websiteName,[int32] $port=80,[int32]$privateMemory=1048576)
* AddDefaultDocument([string] $websiteName,[int] $maxRequestEntityAllowedValue)
* UpdateAppPoolRecycling([string]$name,[int] $port,[string] $access)
* AddSslCertificate([string] $websiteName,[string] $appPool,[string] $appPoolName,[string] $applicationName,[string] $defaultDocumentName)
* AddHostHeader([string]$siteName,[string] $friendlyName,[string] $hostHeader,[string] $hostHeader,[string] $hostName,[string] $ipAddress="*")
* CheckIfSslBindingExists([string]$webSite,[string] $ipAddress="*")
* CreateWebSite([string]$name,[string] $ipAddress="*")
* Enable32BitApps([string] $appPoolName)
* EnableBasicAuthentication([string] $websiteName)
* EnableParentPaths([string] $websiteName)
* EnableWebDav([string] $websiteName)
* RequireClientCertificate([string] $websiteName)
* SetManagedPipelineModeClassic([string]$name)
* SetManagedRuntimeToNoManagedCode([string] $appPoolName,[string] $logLocation,[string] $protocol)
* CheckIfAppPoolExists([string]$name)
* CheckIfVirtualDirectoryExists([string]$webSite,[string] $runtimeVersion)
* SetManagedRuntimeVersion([string] $appPoolName,[string] $runtimeVersion)
* SetMaxRequestEntityAllowed([string] $websiteName,[string] $userName,[string]$InstallDir,[string]$SubFolders)
* CreateVirtualDirectory([string]$webSite,[string]$appName,[string]$appName)
* CheckIfWebSiteExists([string]$name)
* CreateAppPool([string]$name)
* CreateWebApplication([string]$webSite,[string]$hostHeader,[string]$hostHeader,[string]$localPath,[string]$password)
* StartAppPool([string]$name)
* StartWebSite([string]$name)
* StopAppPool([string]$name)
* StopWebSite([string]$name),[string]$periodicRestart="02:00:00",[string]$physicalPath)
* DefaultApplicationPoolGroup()
* RestartAppPool([string]$name)
* SetAppPoolIdentity([string]$name,[string]$user,[string]$virtualDir,[string]$virtualDir)
* CheckIfWebApplicationExists([string]$webSite)
* GetSslCert([string] $friendlyName)
