---
title: backupHelper.ps1 - CleanBackups
linkText: CleanBackups
description: Details about the CleanBackups function in backupHelper.ps1 helper script
---

# CleanBackups

```PowerShell
{% raw %}
CleanBackups
    [-backupDir] <String>
    [-daysToKeep] <int>
{% endraw %}
```

## Description

Removes any backup from directory which is older than the number of days to keep.

## Example

```PowerShell
{% raw %}
CleanBackups -backupDir "Path\To\Backups" -daysToKeep 7
{% endraw %}
```
