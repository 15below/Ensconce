---
title: webHelper.ps1 - UploadValuesAndGetStringResponse
linkText: UploadValuesAndGetStringResponse
description: Details about the UploadValuesAndGetStringResponse function in webHelper.ps1 helper script
---

# UploadValuesAndGetStringResponse

```PowerShell
{% raw %}
UploadValuesAndGetStringResponse
    [-url] <String>
    [-values] <System.Collections.Specialized.NameValueCollection>
{% endraw %}
```

## Description

Uploads a key value pairing of parameters to a url

## Examples

```PowerShell
{% raw %}
$data = New-Object System.Collections.Specialized.NameValueCollection
$data.Add("key","value")
UploadValuesAndGetStringResponse -url "https://example.com/data" -values $data
{% endraw %}
```
