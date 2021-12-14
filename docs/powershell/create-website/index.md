---
title: createWebsite.ps1
description: Details about the createWebsite.ps1 helper script
linkText: createWebsite.ps1
---

# createWebsite.ps1

## Overview

The `createWebsite.ps1` allows for interaction with IIS greater than or equal to version 7.0

This section will detail the functions that can be performed on IIS.

## Functions

* CheckIfAppPoolExists ([string]$name)
* StopAppPool([string]$name)
* UpdateAppPoolRecycling([string]$name, [string]$periodicRestart="02:00:00", [int32]$privateMemory=1048576)
* StartAppPool([string]$name)
* RestartAppPool([string]$name)
* StopWebSite([string]$name)
* StartWebSite([string]$name)
* CheckIfWebApplicationExists ([string]$webSite, [string]$appName)
* CheckIfVirtualDirectoryExists ([string]$webSite, [string]$virtualDir)
* CheckIfSslBindingExists ([string]$webSite, [string]$hostHeader, [string] $ipAddress="*")
* CheckIfWebSiteExists ([string]$name)
* CreateAppPool ([string]$name)
* SetManagedPipelineModeClassic ([string]$name)
* SetAppPoolIdentity([string]$name, [string]$user, [string]$password)
* CreateWebSite ([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation, [int32] $port=80, [string] $ipAddress="*")
* AddHostHeader([string]$siteName, [string] $hostHeader, [int] $port, [string] $protocol, [string] $ipAddress="*")
* CreateWebApplication([string]$webSite, [string]$appName, [string] $appPool, [string]$InstallDir, [string]$SubFolders)
* CreateVirtualDirectory([string]$webSite, [string]$virtualDir, [string]$physicalPath)
* AddSslCertificate ([string] $websiteName, [string] $friendlyName, [string] $hostHeader, [string] $ipAddress)
* GetSslCert([string] $friendlyName)
* EnableWebDav ([string] $websiteName)
* AddAuthoringRule ([string] $websiteName, [string] $userName, [string] $access)
* AddWildcardMap ([string] $websiteName)
* AddDefaultDocument ([string] $websiteName, [string] $defaultDocumentName)
* EnableParentPaths ([string] $websiteName)
* Enable32BitApps ([string] $appPoolName)
* DefaultApplicationPoolGroup ()
* EnableBasicAuthentication([string] $websiteName)
* SetMaxRequestEntityAllowed([string] $websiteName, [int] $maxRequestEntityAllowedValue)
* RequireClientCertificate([string] $websiteName)
* SetManagedRuntimeVersion([string] $appPoolName, [string] $runtimeVersion)
* SetManagedRuntimeToNoManagedCode([string] $appPoolName, [string] $runtimeVersion)
