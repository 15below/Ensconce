---
title: backupHelper.ps1 - CreateDatedBackup
linkText: CreateDatedBackup
description: Details about the CreateDatedBackup function in backupHelper.ps1 helper script
---

# CreateDatedBackup

```PowerShell
{% raw %}
CreateDatedBackup
    [-destFolder] <String>
    [-baseName] <String>
    [-sources] <String[]>
{% endraw %}
```

## Description

Creates a dated zip file in the destination folder with all the files provided as sources.

## Examples

```PowerShell
{% raw %}
CreateDatedBackup -destFolder "Path\To\Backups" -baseName "MyBackup" -sources ("Path1","Path2")
{% endraw %}
```
