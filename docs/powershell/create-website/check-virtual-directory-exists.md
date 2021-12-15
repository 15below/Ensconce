---
title: createWebsite.ps1 - CheckIfVirtualDirectoryExists
linkText: CheckIfVirtualDirectoryExists
description: Details about the CheckIfVirtualDirectoryExists function in createWebsite.ps1 helper script
---

# CheckIfVirtualDirectoryExists

```PowerShell
{% raw %}
CheckIfVirtualDirectoryExists
    [-webSite] <String>
    [-virtualDir] <String>
{% endraw %}
```

## Description

Checks if a virtual directory exists under a website.

## Examples

```PowerShell
{% raw %}
if(CheckIfVirtualDirectoryExists -webSite "MySite" -virtualDir "SubDir")
{
    Write-Host "Virtual directory exists"
}
{% endraw %}
```
