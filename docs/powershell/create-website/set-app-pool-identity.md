---
title: createWebsite.ps1 - SetAppPoolIdentity
linkText: SetAppPoolIdentity
description: Details about the SetAppPoolIdentity function in createWebsite.ps1 helper script
---

# SetAppPoolIdentity

```PowerShell
{% raw %}
SetAppPoolIdentity
    [-name] <String>
    [-user] <String>
    [-password] <String>
{% endraw %}
```

## Description

Set Application Pool to run as a specific user

## Example

```PowerShell
{% raw %}
SetAppPoolIdentity -name "MyAppPool" -user "MyUser" -password "ThisIsASecurePassword!"
{% endraw %}
```
