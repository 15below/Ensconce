---
title: createWebsite.ps1 - SetAppPoolAlwaysRunning
linkText: SetAppPoolAlwaysRunning
description: Details about the SetAppPoolAlwaysRunning function in createWebsite.ps1 helper script
---

# SetAppPoolAlwaysRunning

```PowerShell
{% raw %}
SetAppPoolIdleTimeout
    [-name] <String>
{% endraw %}
```

## Description

Set Application Pool start mode to "AlwaysRunning" and idle timeout to "00:00:00"

## Example

```PowerShell
{% raw %}
SetAppPoolAlwaysRunning -name "MyAppPool"
{% endraw %}
```
