---
title: backupHelper.ps1 - Create7DayRollingDatedBackup
linkText: Create7DayRollingDatedBackup
description: Details about the Create7DayRollingDatedBackup function in backupHelper.ps1 helper script
---

# Create7DayRollingDatedBackup

```PowerShell
{% raw %}
Create7DayRollingDatedBackup
    [-destFolder] <String>
    [-baseName] <String>
    [-sources] <String[]>
{% endraw %}
```

## Description

Creates a dated zip file in the destination folder with all the files provided as sources.

Will automatically clean any backups in the destination which are older than 7 days.

## Example

```PowerShell
{% raw %}
Create7DayRollingDatedBackup -destFolder "Path\To\Backups" -baseName "MyBackup" -sources ("Path1","Path2")
{% endraw %}
```
