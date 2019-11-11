---
title: deployHelp.ps1 - is64Bit
linkText: is64Bit
description: Details about the is64Bit function in deployHelp.ps1 helper script
---

# is64Bit

```PowerShell
{% raw %}
is64Bit
{% endraw %}
```

## Description

The `is64Bit` function will check the current machine is a 64bit installation

## Examples

```PowerShell
{% raw %}
if(is64Bit)
{
    Write-Host "Running 64bit Windows"
}
{% endraw %}
```
