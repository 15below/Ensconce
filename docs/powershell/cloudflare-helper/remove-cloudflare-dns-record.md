---
title: cloudflareHelper.ps1 - RemoveCloudflareDnsRecord 
linkText: RemoveCloudflareDnsRecord 
description: Details about the RemoveCloudflareDnsRecord  function in cloudflareHelper.ps1 helper script
---

# RemoveCloudflareDnsRecord 

```PowerShell
{% raw %}
RemoveCloudflareDnsRecord 
    [-token] <String>
    [-domain] <String>
    [-record] <String>
    [-warnOnDelete] <Boolean> = false
{% endraw %}
```

## Description

Removes DNS entry.

If an entry is removed, can log a warning when this happens

## Examples

```PowerShell
{% raw %}
CreateOrUpdateCloudflareCNAMERecord -token "CF-TOKEN" -domain "example.com" -record "mywebsite"
{% endraw %}
```
