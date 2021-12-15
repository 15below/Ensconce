---
title: dnsHelper.ps1 - CheckHostsEntry
linkText: CheckHostsEntry
description: Details about the CheckHostsEntry function in dnsHelper.ps1 helper script
---

# CheckHostsEntry

```PowerShell
{% raw %}
CheckHostsEntry
    [-Address] <String>
    [-FullyQualifiedName] <String>
{% endraw %}
```

## Description

Checks if the HOSTS file contains an entry with value

## Example

```PowerShell
{% raw %}
CheckHostsEntry -Address "127.0.0.1" -FullyQualifiedName "site.example.com"
{% endraw %}
```
