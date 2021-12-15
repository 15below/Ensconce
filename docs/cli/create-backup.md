---
title: Creating Backups
description: How to create a backup zip file
linkText: Creating Backups
---

# Creating Backups

## Overview

In order to facilitate a rollback, Ensconce gives the ability to create an automated zip backup of your application prior to a deployment.

## Example

## Single Source

```cmd
ensconce --backupSource=[pathToBackup] --backupDestination=[pathToBackupZipFile]
```

## Multiple Sources

```cmd
ensconce --backupSource=[pathToBacker] --backupSource=[anotherPathToBackup] --backupDestination=[pathToBackupZipFile]
```

## Overwrite Existing Backups

```cmd
ensconce --backupSource=[pathToBackup] --backupDestination=[pathToBackupZipFile] --backupOverwrite
```
