---
title: webHelper.ps1 - UploadFileAndGetStringResponse
linkText: UploadFileAndGetStringResponse
description: Details about the UploadFileAndGetStringResponse function in webHelper.ps1 helper script
---

# UploadFileAndGetStringResponse

```PowerShell
{% raw %}
UploadFileAndGetStringResponse
    [-url] <String>
    [-file] <String>
{% endraw %}
```

## Description

Uploads a file to a URL

## Examples

```PowerShell
{% raw %}
UploadFileAndGetStringResponse -url "https://example.com/upload" -file "C:\Temp\File.txt"
{% endraw %}
```
