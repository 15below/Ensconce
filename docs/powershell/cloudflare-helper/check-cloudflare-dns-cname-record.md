---
title: cloudflareHelper.ps1 - CheckCloudflareCNAMERecord
linkText: CheckCloudflareCNAMERecord
description: Details about the CheckCloudflareCNAMERecord function in cloudflareHelper.ps1 helper script
---

# CheckCloudflareCNAMERecord

```PowerShell
{% raw %}
CheckCloudflareCNAMERecord
    [-token] <String>
    [-domain] <String>
    [-record] <String>
    [-cnameValue] <String>
{% endraw %}
```

## Description

Used to ensure that a DNS CNAME record is present within Cloudflare.

If the DNS entry already exists and if wrong, will write a warning and return false.

If the DNS entry does not exist, will write a warning and return false.

## Example

```PowerShell
{% raw %}
CheckCloudflareCNAMERecord -token "CF-TOKEN" -domain "example.com" -record "mywebsite" -cnameValue "myserver.serverfarm.net"
{% endraw %}
```
