---
title: createWebsite.ps1 - CheckIfWebApplicationExists
linkText: CheckIfWebApplicationExists
description: Details about the CheckIfWebApplicationExists function in createWebsite.ps1 helper script
---

# CheckIfWebApplicationExists

```PowerShell
{% raw %}
CheckIfWebApplicationExists
    [-webSite] <String>
    [-appName] <String>
{% endraw %}
```

## Description

Checks if a web application exists under a website.

## Example

```PowerShell
{% raw %}
if(CheckIfWebApplicationExists -webSite "MySite" -appName "SubApp")
{
    Write-Host "WebApp exists"
}
{% endraw %}
```
