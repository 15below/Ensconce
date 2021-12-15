---
title: serviceManagement.ps1 - InstallService
linkText: InstallService
description: Details about the InstallService function in serviceManagement.ps1 helper script
---

# InstallService

```PowerShell
{% raw %}
InstallService
    [-serviceName] <String>
    [-exePath] <String>
    [-startupType] <String>
    [-serviceDisplayName] <String>
    [-serviceDescription] <String>
{% endraw %}
```

## Description

Install a service which will run an exe.

## Examples

```PowerShell
{% raw %}
InstallService -serviceName "MySvc" -exePath "/Path/To/exe.exe" -startupType "Automatic" -serviceDisplayName "My Service" -serviceDescription "Service that does stuff"
{% endraw %}
```
