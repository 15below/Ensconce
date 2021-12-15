---
title: cloudflareHelper.ps1 - GetCloudflareDnsIp
linkText: GetCloudflareDnsIp
description: Details about the GetCloudflareDnsIp function in cloudflareHelper.ps1 helper script
---

# GetCloudflareDnsIp

```PowerShell
{% raw %}
GetCloudflareDnsIp
    [-token] <String>
    [-domain] <String>
    [-record] <String>
{% endraw %}
```

## Description

Returns the IP address of the requested record

## Example

```PowerShell
{% raw %}
$ipValue = GetCloudflareDnsIp -token "CF-TOKEN" -domain "example.com" -record "mywebsite"
{% endraw %}
```
