---
title: scheduledTaskHelper.ps1 - ScheduledTask-CheckExists
linkText: ScheduledTask-CheckExists
description: Details about the ScheduledTask-CheckExists function in scheduledTaskHelper.ps1 helper script
---

# ScheduledTask-CheckExists

```PowerShell
{% raw %}
ScheduledTask-CheckExists
    [-taskName] <String>
    [-taskPath] <String>
{% endraw %}
```

## Description

Checks if a task already exists

## Examples

```PowerShell
{% raw %}
if(ScheduledTask-CheckExists -taskName "MyTask" -taskPath "Tasks")
{
    Write-Host "Task Exists"
}
{% endraw %}
```
