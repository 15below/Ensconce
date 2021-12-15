---
title: dnsHelper.ps1 - CreateOrUpdateDns
linkText: CreateOrUpdateDns
description: Details about the CreateOrUpdateDns function in dnsHelper.ps1 helper script
---

# CreateOrUpdateDns

```PowerShell
{% raw %}
CreateOrUpdateDns
    [-dnsServer] <String>
    [-domain] <String>
    [-name] <String>
    [-ipAddressOrServer] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Adds or updates an `A` or `CNAME` record on a DNS sever.

Can optionally log a warning if an update occurs

## Examples

```PowerShell
{% raw %}
CreateOrUpdateARecord -dnsServer "192.168.0.100" -domain "myDomain.local" -name "server01" -ipAddressOrServer "192.168.0.10"
{% endraw %}
```

```PowerShell
{% raw %}
CreateOrUpdateARecord -dnsServer "192.168.0.100" -domain "myDomain.local" -name "server01" -ipAddressOrServer "server01.servers.local"
{% endraw %}
```
