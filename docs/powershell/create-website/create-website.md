---
title: createWebsite.ps1 - CreateWebSite
linkText: CreateWebSite
description: Details about the CreateWebSite function in createWebsite.ps1 helper script
---

# CreateWebSite

```PowerShell
{% raw %}
CreateWebSite
    [-name] <String>
    [-localPath] <String>
    [-appPoolName] <String>
    [-applicationName] <String>
    [-hostName] <String>
    [-logLocation] <String>
    [-port] <int> = 80
    [-ipAddress] <String> = "*"
{% endraw %}
```

## Description

Creates a website.

## Example

```PowerShell
{% raw %}
CreateWebSite -name "MySite" -localPath "C:\Site" -appPool "MyAppPool" -applicationName "MySite" -hostName "site.example.com" -logLocation "C:\logs"
{% endraw %}
```
