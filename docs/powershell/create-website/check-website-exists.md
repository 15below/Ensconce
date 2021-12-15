---
title: createWebsite.ps1 - CheckIfWebSiteExists
linkText: CheckIfWebSiteExists
description: Details about the CheckIfWebSiteExists function in createWebsite.ps1 helper script
---

# CheckIfWebSiteExists

```PowerShell
{% raw %}
CheckIfWebSiteExists
    [-webSite] <String>
{% endraw %}
```

## Description

Checks if website exists

## Examples

```PowerShell
{% raw %}
if(CheckIfWebSiteExists -webSite "MySite")
{
    Write-Host "Website exists"
}
{% endraw %}
```
