---
title: createWebsite.ps1 - GetAppPoolState
linkText: GetAppPoolState
description: Details about the GetAppPoolState function in createWebsite.ps1 helper script
---

# GetAppPoolState

```PowerShell
{% raw %}
GetAppPoolState
    [-name] <String>
{% endraw %}
```

## Description

Returns the current state of the app pool

## Example

```PowerShell
{% raw %}
$state = GetAppPoolState -name "MyAppPool"
{% endraw %}
```
