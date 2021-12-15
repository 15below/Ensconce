---
title: scheduledTaskHelper.ps1 - ScheduledTask-Run
linkText: ScheduledTask-Run
description: Details about the ScheduledTask-Run function in scheduledTaskHelper.ps1 helper script
---

# ScheduledTask-Run

```PowerShell
{% raw %}
ScheduledTask-Run
    [-taskName] <String>
    [-taskPath] <String>
{% endraw %}
```

## Description

Runs a scheduled task.

## Example

```PowerShell
{% raw %}
ScheduledTask-Run -taskName "MyTask" -taskPath "Tasks"
{% endraw %}
```
