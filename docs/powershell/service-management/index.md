---
title: serviceManagement.ps1
description: Details about the serviceManagement.ps1 helper script
---

# serviceManagement.ps1

## Overview

The `serviceManagement.ps1` script has functionality to interact and setup Windows services

## Functions

* InstallDotNetCoreService([string]$serviceName, [string]$dllPath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
* InstallService([string]$serviceName, [string]$exePath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
* InstallTopshelfService([string]$serviceName, [string]$exePath)
* InstallTopshelfServiceWithInstance([string]$serviceName, [string]$exePath, [string]$instance)
* RemoveService([string]$serviceName)
* SetServiceRestartAlways([string]$serviceName)
* SetServiceRestarts([string]$serviceName)
* SetServiceRunAs([string]$serviceName, [string]$serviceUser, [string]$servicePassword)
* StartService([string]$serviceName)
* StopService([string]$serviceName)
