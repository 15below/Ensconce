---
title: createWebsite.ps1 - AddHostHeader
linkText: AddHostHeader
description: Details about the AddHostHeader function in createWebsite.ps1 helper script
---

# AddHostHeader

```PowerShell
{% raw %}
AddHostHeader
    [-siteName] <String>
    [-hostHeader] <String>
    [-port] <int>
    [-protocol] <String>    
    [-ipAddress] <String> = "*"
{% endraw %}
```

## Description

Adds a host header value to an existing website.

## Examples

```PowerShell
{% raw %}
AddHostHeader -siteName "MySite" -hostHeader "site.example.com" -port 80 -protocol "http"
{% endraw %}
```
