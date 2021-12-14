---
title: backupHelper.ps1
description: Details about the backupHelper.ps1 script
linkText: backupHelper.ps1
---

# backupHelper.ps1

## Overview

The `backupHelper.ps1` script has functionality to assist performing/maintaining backups

## Functions

{% include childPages.html %}

{% comment %}
CleanBackups([string]$backupDir, [int]$daysToKeep)
CreateDatedBackup([string]$destFolder, [string]$baseName, [string[]]$sources)
Create7DayRollingDatedBackup([string]$destFolder, [string]$baseName, [string[]]$sources)
{% endcomment %}
