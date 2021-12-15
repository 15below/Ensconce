---
title: scheduledTaskHelper.ps1 - ScheduledTask-CreateOrUpdateFromXml
linkText: ScheduledTask-CreateOrUpdateFromXml
description: Details about the ScheduledTask-CreateOrUpdateFromXml function in scheduledTaskHelper.ps1 helper script
---

# ScheduledTask-CreateOrUpdateFromXml

```PowerShell
{% raw %}
ScheduledTask-CreateOrUpdateFromXml
    [-taskName] <String>
    [-taskPath] <String>
    [-taskXmlPath] <String>
{% endraw %}
```

## Description

Creates or updates a scheduled task based on a task definition XML file

## Examples

```PowerShell
{% raw %}
ScheduledTask-CreateOrUpdateFromXml -taskName "MyTask" -taskPath "Tasks" -taskXmlPath "/Path/To/Xml.xml"
{% endraw %}
```
