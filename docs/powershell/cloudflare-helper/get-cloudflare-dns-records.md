---
title: cloudflareHelper.ps1 - GetCloudflareDnsRecords
linkText: GetCloudflareDnsRecords
description: Details about the GetCloudflareDnsRecords function in cloudflareHelper.ps1 helper script
---

# GetCloudflareDnsRecords

```PowerShell
{% raw %}
GetCloudflareDnsRecords
    [-token] <String>
    [-domain] <String>
    [-filter] <String> = ""
{% endraw %}
```

## Description

Returns a collection of strings representing all the entries that exist.

## Example

```PowerShell
{% raw %}
$records = [Collections.Generic.List[string]](GetCloudflareDnsRecords -token "CF-TOKEN" -domain "example.com")
{% endraw %}
```
