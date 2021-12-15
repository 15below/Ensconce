---
title: dnsHelper.ps1 - CreateOrUpdateCName
linkText: CreateOrUpdateCName
description: Details about the CreateOrUpdateCName function in dnsHelper.ps1 helper script
---

# CreateOrUpdateCName

```PowerShell
{% raw %}
CreateOrUpdateCName
    [-dnsServer] <String>
    [-domain] <String>
    [-name] <String>
    [-server] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Adds or updates an `CNAME` record on a DNS sever.

Can optionally log a warning if an update occurs

## Examples

```PowerShell
{% raw %}
CreateOrUpdateCName -dnsServer "192.168.0.100" -domain "myDomain.local" -name "server01" -server "server01.servers.local"
{% endraw %}
```
