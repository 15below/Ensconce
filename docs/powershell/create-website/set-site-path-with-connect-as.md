---
title: createWebsite.ps1 - SetSitePathWithConnectAs
linkText: SetSitePathWithConnectAs
description: Details about the SetSitePathWithConnectAs function in createWebsite.ps1 helper script
---

# StartAppPool

```PowerShell
{% raw %}
SetSitePathWithConnectAs
    [-name] <String>
    [-path] <String>
    [-username] <String>
    [-password] <String>
{% endraw %}
```

## Description

Sets the site physical path with connect as set to a dedicated user

## Example

```PowerShell
{% raw %}
SetSitePathWithConnectAs -name "MySite" -path "\\remoteshare\folder" -username "MyUser" -password "5ecureP@ssw0rd
{% endraw %}
```
