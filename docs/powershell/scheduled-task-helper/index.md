---
title: scheduledTaskHelper.ps1
description: Details about the scheduledTaskHelper.ps1 helper script
---

# scheduledTaskHelper.ps1

## Overview

The `scheduledTaskHelper.ps1` script has functionality to check, create, delete and update scheduled tasks

## Functions

* ScheduledTask-CheckExists([string] $taskName, [string] $taskPath)
* ScheduledTask-Delete([string] $taskName, [string] $taskPath)
* ScheduledTask-CreateFromXml([string] $taskName, [string] $taskPath, [string] $taskXmlPath)
* ScheduledTask-CreateOrUpdateFromXml([string] $taskName, [string] $taskPath, [string] $taskXmlPath)
