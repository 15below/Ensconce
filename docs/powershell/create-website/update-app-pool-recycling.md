---
title: createWebsite.ps1 - UpdateAppPoolRecycling
linkText: UpdateAppPoolRecycling
description: Details about the UpdateAppPoolRecycling function in createWebsite.ps1 helper script
---

# UpdateAppPoolRecycling

```PowerShell
{% raw %}
UpdateAppPoolRecycling
    [-name] <String>
    [-periodicRestart] <String> = "02:00:00"
    [-privateMemory] <int> = 1048576
{% endraw %}
```

## Description

Update the settings for how frequently an application pool should restart.

The supported triggers for restart are time (default to 2 hours) and memory size (default to 1048576KB/1024MB/1GB)

## Examples

```PowerShell
{% raw %}
UpdateAppPoolRecycling -name "MyAppPool" -periodicRestart "00:30:00"
{% endraw %}
```

```PowerShell
{% raw %}
UpdateAppPoolRecycling -name "MyAppPool" -privateMemory 524288
{% endraw %}
```

```PowerShell
{% raw %}
UpdateAppPoolRecycling -name "MyAppPool" -periodicRestart "00:30:00" -privateMemory 524288
{% endraw %}
```
