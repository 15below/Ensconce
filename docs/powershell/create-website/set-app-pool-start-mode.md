---
title: createWebsite.ps1 - SetAppPoolStartMode
linkText: SetAppPoolStartMode
description: Details about the SetAppPoolStartMode function in createWebsite.ps1 helper script
---

# SetAppPoolStartMode

```PowerShell
{% raw %}
SetAppPoolStartMode
    [-name] <String>
    [-startMode] <String>
{% endraw %}
```

## Description

Set Application Pool start mode.

* Value "1" = AlwaysRunning
* Value "0" = OnDemand

## Example

```PowerShell
{% raw %}
SetAppPoolStartMode -name "MyAppPool" -startMode "1"
{% endraw %}
```
