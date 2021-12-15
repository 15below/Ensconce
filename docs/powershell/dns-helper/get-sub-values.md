---
title: dnsHelper.ps1 - GetAllSubValues
linkText: GetAllSubValues
description: Details about the GetAllSubValues function in dnsHelper.ps1 helper script
---

# GetAllSubValues

```PowerShell
{% raw %}
GetAllSubValues
    [-dnsServer] <String>
    [-domain] <String>
    [-lookupName] <String>
{% endraw %}
```

## Description

Gets a collection of all DNS entries based within a section of your DNS server

## Example

```PowerShell
{% raw %}
GetAllSubValues -dnsServer "192.168.0.100" -domain "myDomain.local" -lookupName "servers"
{% endraw %}
```
