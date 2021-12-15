---
title: createWebsite.ps1 - Enable32BitApps
linkText: Enable32BitApps
description: Details about the Enable32BitApps function in createWebsite.ps1 helper script
---

# Enable32BitApps

```PowerShell
{% raw %}
Enable32BitApps
    [-appPoolName] <String>
{% endraw %}
```

## Description

Sets application pool to enable 32bit apps to run on 64bit version of Windows

## Example

```PowerShell
{% raw %}
Enable32BitApps -appPoolName "MyAppPool"
{% endraw %}
```
