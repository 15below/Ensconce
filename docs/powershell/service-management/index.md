---
title: serviceManagement.ps1
description: Details about the serviceManagement.ps1 helper script
---

# serviceManagement.ps1

## Overview

The `serviceManagement.ps1` script has functionality to interact and setup Windows services

## Functions

* StopService([string]$serviceName)
* StartService([string]$serviceName)
* SetServiceRunAs([string]$serviceName, [string]$serviceUser, [string]$servicePassword)
* SetServiceRestarts([string]$serviceName)
* SetServiceRestartAlways([string]$serviceName)
* RemoveService([string]$serviceName)
* InstallService([string]$serviceName, [string]$exePath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
* InstallDotNetCoreService([string]$serviceName, [string]$dllPath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
* InstallTopshelfService([string]$serviceName, [string]$exePath)
* InstallTopshelfServiceWithInstance([string]$serviceName, [string]$exePath, [string]$instance)
