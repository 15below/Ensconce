---
title: deployHelp.ps1 - retry-command
linkText: retry-command
description: Details about the retry-command function in deployHelp.ps1 helper script
---

# retry-command

```PowerShell
{% raw %}
Retry-Command([scriptblock]$ScriptBlock, [int]$Maximum = 5, [int]$Delay = 100)
{% endraw %}
```

## Description

The `retry-command` function is a helper to run the contents of the script block a number of times if there is an error with a delay between.

By default, it will retry 5 times, with a delay of 100ms.

## Examples

```PowerShell
{% raw %}
retry-command { Write-Host "Test" } 5 500
{% endraw %}
```