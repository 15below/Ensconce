---
title: cloudflareHelper.ps1 - CreateOrUpdateCloudflareCNAMERecord
linkText: CreateOrUpdateCloudflareCNAMERecord
description: Details about the CreateOrUpdateCloudflareCNAMERecord function in cloudflareHelper.ps1 helper script
---

# CreateOrUpdateCloudflareCNAMERecord

```PowerShell
{% raw %}
CreateOrUpdateCloudflareCNAMERecord
    [-token] <String>
    [-domain] <String>
    [-record] <String>
    [-cnameValue] <String>
    [-warnOnUpdate] <Boolean> = false
{% endraw %}
```

## Description

Used to ensure that a DNS CNAME record is present within Cloudflare.

If the DNS entry already exists, it's value will be updated. - This can optionally log a warning.

If the DNS entry does not exist, it will be created.

## Example

```PowerShell
{% raw %}
CreateOrUpdateCloudflareCNAMERecord -token "CF-TOKEN" -domain "example.com" -record "mywebsite" -cnameValue "myserver.serverfarm.net"
{% endraw %}
```
