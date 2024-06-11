---
title: cloudflareHelper.ps1 - CheckCloudflareARecord
linkText: CheckCloudflareARecord
description: Details about the CheckCloudflareARecord function in cloudflareHelper.ps1 helper script
---

# CheckCloudflareARecord

```PowerShell
{% raw %}
CheckCloudflareARecord
    [-token] <String>
    [-domain] <String>
    [-record] <String>
    [-ipaddr] <String>
{% endraw %}
```

## Description

Used to ensure that a DNS A record is present within Cloudflare.

If the DNS entry already exists and if wrong, will write a warning and return false.

If the DNS entry does not exist, will write a warning and return false.

## Example

```PowerShell
{% raw %}
CheckCloudflareARecord -token "CF-TOKEN" -domain "example.com" -record "mywebsite" -ipaddr "7.7.7.7"
{% endraw %}
```
