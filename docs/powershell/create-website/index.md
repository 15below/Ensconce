---
title: createWebsite.ps1
description: Details about the createWebsite.ps1 helper script
---

# createWebsite.ps1

## Overview

The `createWebsite.ps1` script does not actually contain any functionality.

It is a wrapper to load the correct version of the IIS scripts.

This section will detail the functions and which version of IIS the function is supported in.

## Functions

### IIS6 Only

* [AddWildcardMap [-websiteName] &lt;String&gt; [-subFolders]  &lt;String&gt;](add-wildcard-map)

### IIS7 Only

* AddAuthoringRule([string] $websiteName, [string] $userName, [string] $access)
* CheckIfSslBindingExists([string]$webSite, [string]$hostHeader)
* Enable32BitApps([string] $appPoolName)
* EnableBasicAuthentication([string] $websiteName)
* EnableParentPaths([string] $websiteName)
* EnableWebDav([string] $websiteName)
* RequireClientCertificate([string] $websiteName)
* SetManagedPipelineModeClassic([string]$name)
* SetManagedRuntimeToNoManagedCode([string] $appPoolName, [string] $runtimeVersion)
* SetManagedRuntimeVersion([string] $appPoolName, [string] $runtimeVersion)
* SetMaxRequestEntityAllowed([string] $websiteName, [int] $maxRequestEntityAllowedValue)
* UpdateAppPoolRecycling([string]$name, [string]$periodicRestart="02:00:00", [int32]$privateMemory=1048576)

### IIS6 & IIS7

* AddDefaultDocument([string] $websiteName, [string] $defaultDocumentName)
* AddHostHeader([string]$siteName, [string] $hostHeader, [int] $port, [string] $protocol)
* AddSslCertificate([string] $websiteName, [string] $friendlyName, [string] $hostHeader)
* CheckIfAppPoolExists([string]$name)
* CheckIfVirtualDirectoryExists([string]$webSite, [string]$virtualDir)
* CheckIfWebApplicationExists([string]$webSite, [string]$appName)
* CheckIfWebSiteExists([string]$name)
* CreateAppPool([string]$name)
* CreateWebApplication([string]$webSite, [string]$appName, [string] $appPool, [string]$InstallDir, [string]$SubFolders)
* DefaultApplicationPoolGroup()
* RestartAppPool([string]$name)
* SetAppPoolIdentity([string]$name, [string]$user, [string]$password)
* StartAppPool([string]$name)
* StartWebSite([string]$name)
* StopAppPool([string]$name)
* StopWebSite([string]$name)

### IIS6 & IIS7 (Differing Signatures)

#### IIS6

* CreateVirtualDirectory([string]$webSite, [string]$virtualDir, [string]$installDir)
* CreateWebSite([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation)

#### IIS7

* CreateVirtualDirectory([string]$webSite, [string]$virtualDir, [string]$physicalPath)
* CreateWebSite([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation, [int32] $port=80)
