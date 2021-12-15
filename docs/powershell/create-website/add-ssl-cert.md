---
title: createWebsite.ps1 - AddSslCertificate
linkText: AddSslCertificate
description: Details about the AddSslCertificate function in createWebsite.ps1 helper script
---

# AddSslCertificate

```PowerShell
{% raw %}
AddSslCertificate
    [-websiteName] <String>
    [-friendlyName] <String>
    [-hostHeader] <String>
    [-ipAddress] <String>
{% endraw %}
```

## Description

Adds a `https` binding to an existing website using a certificate located by friendly name.

## Examples

```PowerShell
{% raw %}
AddSslCertificate -websiteName "MySite" -friendlyName "MyCert" -hostHeader "site.example.com"
{% endraw %}
```
