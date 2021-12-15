---
title: serviceManagement.ps1 - InstallDotNetCoreService
linkText: InstallDotNetCoreService
description: Details about the InstallDotNetCoreService function in serviceManagement.ps1 helper script
---

# InstallDotNetCoreService

```PowerShell
{% raw %}
InstallDotNetCoreService
    [-serviceName] <String>
    [-dllPath] <String>
    [-startupType] <String>
    [-serviceDisplayName] <String>
    [-serviceDescription] <String>
{% endraw %}
```

## Description

Install a dotnet service (application which has a .dll file being passed to the dotnet runtime).

## Examples

```PowerShell
{% raw %}
InstallDotNetCoreService -serviceName "MySvc" -dllPath "/Path/To/Dll.dll" -startupType "Automatic" -serviceDisplayName "My Service" -serviceDescription "Service that does stuff"
{% endraw %}
```
