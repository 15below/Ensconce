---
title: powershell4Polyfill.ps1 - Clear-DnsClientCache
linkText: Clear-DnsClientCache
description: Details about the Clear-DnsClientCache function in powershell4Polyfill.ps1 helper script
---

# Clear-DnsClientCache

```PowerShell
{% raw %}
Clear-DnsClientCache
{% endraw %}
```

## Description

Runs `ipconfig /flushdns` to replicate the function added in later versions of powershell - https://docs.microsoft.com/en-us/powershell/module/dnsclient/clear-dnsclientcache

## Examples

```PowerShell
{% raw %}
Clear-DnsClientCache
{% endraw %}
```
