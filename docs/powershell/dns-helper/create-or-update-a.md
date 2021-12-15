---
title: dnsHelper.ps1 - CreateOrUpdateARecord
linkText: CreateOrUpdateARecord
description: Details about the CreateOrUpdateARecord function in dnsHelper.ps1 helper script
---

# CreateOrUpdateARecord

```PowerShell
{% raw %}
CreateOrUpdateARecord
    [-dnsServer] <String>
    [-domain] <String>
    [-name] <String>
    [-ipAddress] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Adds or updates an `A` record on a DNS sever.

Can optionally log a warning if an update occurs

## Example

```PowerShell
{% raw %}
CreateOrUpdateARecord -dnsServer "192.168.0.100" -domain "myDomain.local" -name "server01" -ipAddress "192.168.0.10"
{% endraw %}
```
