---
title: deployHelp.ps1 - Retry-Command
linkText: Retry-Command
description: Details about the Retry-Command function in deployHelp.ps1 helper script
---

# Retry-Command

```PowerShell
{% raw %}
Retry-Command
    [-ScriptBlock] <ScriptBlock>
    [-Maximum] <int> = 5
    [-Delay] <int> = 100
{% endraw %}
```

## Description

The `Retry-Command` function is a helper to run the contents of the script block a number of times if there is an error with a delay between.

By default, it will retry 5 times, with a delay of 100ms.

## Example

```PowerShell
{% raw %}
Retry-Command -ScriptBlock { Write-Host "Test" } -Maximum 5 -Delay 500
{% endraw %}
```
