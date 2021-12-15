---
title: createWebsite.ps1 - CheckIfAppPoolExists
linkText: CheckIfAppPoolExists
description: Details about the CheckIfAppPoolExists function in createWebsite.ps1 helper script
---

# CheckIfAppPoolExists

```PowerShell
{% raw %}
CheckIfAppPoolExists
    [-name] <String>
{% endraw %}
```

## Description

Checks for the presence of an application pool matching the name.

## Examples

```PowerShell
{% raw %}
if(CheckIfAppPoolExists -name "MyAppPool")
{
    Write-Host "AppPool exists"
}
{% endraw %}
```
