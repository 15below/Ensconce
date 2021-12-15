---
title: serviceManagement.ps1 - SetServiceRestartAlways
linkText: SetServiceRestartAlways
description: Details about the SetServiceRestartAlways function in serviceManagement.ps1 helper script
---

# SetServiceRestartAlways

```PowerShell
{% raw %}
SetServiceRestartAlways
    [-serviceName] <String>
{% endraw %}
```

## Description

Set service to restart on failure indefinitely

## Example

```PowerShell
{% raw %}
SetServiceRestartAlways -serviceName "MySvc"
{% endraw %}
```
