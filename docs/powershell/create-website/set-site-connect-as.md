---
title: createWebsite.ps1 - SetSiteConnectAs
linkText: SetSiteConnectAs
description: Details about the SetSiteConnectAs function in createWebsite.ps1 helper script
---

# StartAppPool

```PowerShell
{% raw %}
SetSiteConnectAs
    [-name] <String>
    [-username] <String>
    [-password] <String>
{% endraw %}
```

## Description

Sets the site to connect to the file system as a dedicated user

## Example

```PowerShell
{% raw %}
SetSiteConnectAs -name "MySite" -username "MyUser" -password "5ecureP@ssw0rd
{% endraw %}
```
