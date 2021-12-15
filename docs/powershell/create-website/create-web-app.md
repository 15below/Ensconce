---
title: createWebsite.ps1 - CreateWebApplication
linkText: CreateWebApplication
description: Details about the CreateWebApplication function in createWebsite.ps1 helper script
---

# CreateWebApplication

```PowerShell
{% raw %}
CreateWebApplication
    [-webSite] <String>
    [-appName] <String>
    [-appPool] <String>
    [-InstallDir] <String>    
    [-SubFolders] <String>
{% endraw %}
```

## Description

Creates webapp into existing website

## Example

```PowerShell
{% raw %}
CreateWebApplication -webSite "MySite" -appName "SubApp" -appPool "MyAppPool" -InstallDir "C:\Site\SubApp"
{% endraw %}
```
