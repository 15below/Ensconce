---
title: serviceManagement.ps1 - InstallTopshelfService
linkText: InstallTopshelfService
description: Details about the InstallTopshelfService function in serviceManagement.ps1 helper script
---

# InstallTopshelfService

```PowerShell
{% raw %}
InstallTopshelfService
    [-serviceName] <String>
    [-exePath] <String>
{% endraw %}
```

## Description

Install a Topshelf service

## Examples

```PowerShell
{% raw %}
InstallTopshelfService -serviceName "MySvc" -exePath "/Path/To/exe.exe"
{% endraw %}
```
