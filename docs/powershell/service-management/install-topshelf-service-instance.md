---
title: serviceManagement.ps1 - InstallTopshelfServiceWithInstance
linkText: InstallTopshelfServiceWithInstance
description: Details about the InstallTopshelfServiceWithInstance function in serviceManagement.ps1 helper script
---

# InstallTopshelfServiceWithInstance

```PowerShell
{% raw %}
InstallTopshelfServiceWithInstance
    [-serviceName] <String>
    [-exePath] <String>
    [-instance] <String>
{% endraw %}
```

## Description

Install a Topshelf service with a specific instance identifier

## Examples

```PowerShell
{% raw %}
InstallTopshelfServiceWithInstance -serviceName "MySvc" -exePath "/Path/To/exe.exe" -instance "1"
{% endraw %}
```
