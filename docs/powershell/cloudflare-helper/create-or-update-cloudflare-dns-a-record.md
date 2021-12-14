---
title: cloudflareHelper.ps1 - CreateOrUpdateCloudflareARecord
linkText: CreateOrUpdateCloudflareARecord
description: Details about the CreateOrUpdateCloudflareARecord function in cloudflareHelper.ps1 helper script
---

# CreateOrUpdateCloudflareARecord

```PowerShell
{% raw %}
CreateOrUpdateCloudflareARecord
    [-token] <String>
    [-domain] <String>
    [-record] <String>
    [-ipaddr] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Used to ensure that a DNS A record is present within Cloudflare.

If the DNS entry already exists, it's value will be updated. - This can optionally log a warning.

If the DNS entry does not exist, it will be created.

## Examples

```PowerShell
{% raw %}
CreateOrUpdateCloudflareARecord -token "CF-TOKEN" -domain "example.com" -record "mywebsite" -ipaddr "7.7.7.7"
{% endraw %}
```
