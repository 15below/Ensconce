---
title: dnsHelper.ps1 - DeleteDns
linkText: DeleteDns
description: Details about the DeleteDns function in dnsHelper.ps1 helper script
---

# DeleteDns

```PowerShell
{% raw %}
DeleteDns
    [-dnsServer] <String>
    [-domain] <String>
    [-name] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Delete an existing DNS entry

Can optionally log a warning if a delete occurs

## Examples

```PowerShell
{% raw %}
DeleteDns -dnsServer "192.168.0.100" -domain "myDomain.local" -name "server01"
{% endraw %}
```
