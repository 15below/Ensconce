---
title: serviceManagement.ps1 - SetServiceRunAs
linkText: SetServiceRunAs
description: Details about the SetServiceRunAs function in serviceManagement.ps1 helper script
---

# SetServiceRunAs

```PowerShell
{% raw %}
SetServiceRunAs
    [-serviceName] <String>
    [-serviceUser] <String>
    [-servicePassword] <String>
{% endraw %}
```

## Description

Set the startup user for a service

## Example

```PowerShell
{% raw %}
SetServiceRunAs -serviceName "MySvc" -serviceUser "MyUser" -servicePassword "Secure_User_Password"
{% endraw %}
```
