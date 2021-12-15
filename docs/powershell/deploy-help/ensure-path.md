---
title: deployHelp.ps1 - EnsurePath
linkText: EnsurePath
description: Details about the EnsurePath function in deployHelp.ps1 helper script
---

# EnsurePath

```PowerShell
{% raw %}
EnsurePath
    [-name] <String>
{% endraw %}
```

## Description

The `EnsurePath` function with check if a directory exists and if it does not exist will create it

## Example

```PowerShell
{% raw %}
EnsurePath -name "C:\Temp"
{% endraw %}
```
