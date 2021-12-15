---
title: createWebsite.ps1 - CheckIfSslBindingExists
linkText: CheckIfSslBindingExists
description: Details about the CheckIfSslBindingExists function in createWebsite.ps1 helper script
---

# CheckIfSslBindingExists

```PowerShell
{% raw %}
CheckIfSslBindingExists
    [-webSite] <String>
    [-hostHeader] <String>
    [-ipAddress] <String> = "*"
{% endraw %}
```

## Description

Checks if there is a binding with `https` protocol with the provided host header and optional IP Address on a website.

## Example

```PowerShell
{% raw %}
if(CheckIfSslBindingExists -webSite "MySite" -hostHeader "site.example.com")
{
    Write-Host "SSL binding exists"
}
{% endraw %}
```
