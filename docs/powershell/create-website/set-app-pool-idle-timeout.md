---
title: createWebsite.ps1 - SetAppPoolIdleTimeout
linkText: SetAppPoolIdleTimeout
description: Details about the SetAppPoolIdleTimeout function in createWebsite.ps1 helper script
---

# SetAppPoolIdleTimeout

```PowerShell
{% raw %}
SetAppPoolIdleTimeout
    [-name] <String>
    [-timeout] <String>
{% endraw %}
```

## Description

Set Application Pool idle timeout. - Setting to "00:00:00" will set no timeout.

## Example

```PowerShell
{% raw %}
SetAppPoolIdleTimeout -name "MyAppPool" -startMode "01:00:00"
{% endraw %}
```
