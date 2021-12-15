---
title: scheduledTaskHelper.ps1 - ScheduledTask-Delete
linkText: ScheduledTask-Delete
description: Details about the ScheduledTask-Delete function in scheduledTaskHelper.ps1 helper script
---

# ScheduledTask-Delete

```PowerShell
{% raw %}
ScheduledTask-Delete
    [-taskName] <String>
    [-taskPath] <String>
{% endraw %}
```

## Description

Will delete an existing scheduled task

## Examples

```PowerShell
{% raw %}
ScheduledTask-Delete -taskName "MyTask" -taskPath "Tasks"
{% endraw %}
```
