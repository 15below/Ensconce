---
title: dnsHelper.ps1 - AddHostsEntry
linkText: AddHostsEntry
description: Details about the AddHostsEntry function in dnsHelper.ps1 helper script
---

# AddHostsEntry

```PowerShell
{% raw %}
AddHostsEntry
    [-Address] <String>
    [-FullyQualifiedName] <String>
{% endraw %}
```

## Description

Adds a value into the machine HOSTS file

## Examples

```PowerShell
{% raw %}
AddHostsEntry -Address "127.0.0.1" -FullyQualifiedName "site.example.com"
{% endraw %}
```
