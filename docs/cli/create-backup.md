---
title: Creating Backups
description: How to create a backup zip file
---

# Creating Backups

## Overview

In order to facilitate a rollback, Ensconce gives the ability to create an automated zip backup of your application prior to a deployment.

## Syntax

## Single Source

`--backupSource=[pathToBackup] --backupDestination=[pathToBackupZipFile]`

## Multiple Sources

`--backupSource=[pathToBacker] --backupSource=[anotherPathToBackup] --backupDestination=[pathToBackupZipFile]`

## Overwrite Existing Backups

`--backupSource=[pathToBackup] --backupDestination=[pathToBackupZipFile] --backupOverwrite`
