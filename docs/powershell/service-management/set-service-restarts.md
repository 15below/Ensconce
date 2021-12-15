---
title: serviceManagement.ps1 - SetServiceRestarts
linkText: SetServiceRestarts
description: Details about the SetServiceRestarts function in serviceManagement.ps1 helper script
---

# SetServiceRestarts

```PowerShell
{% raw %}
SetServiceRestarts
    [-serviceName] <String>
{% endraw %}
```

## Description

Set service to restart on failure (2 restart attempts)

## Example

```PowerShell
{% raw %}
SetServiceRestartAlways -serviceName "MySvc"
{% endraw %}
```
