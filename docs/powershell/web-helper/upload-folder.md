---
title: webHelper.ps1 - UploadFolderAsZipAndGetStringResponse
linkText: UploadFolderAsZipAndGetStringResponse
description: Details about the UploadFolderAsZipAndGetStringResponse function in webHelper.ps1 helper script
---

# UploadFolderAsZipAndGetStringResponse

```PowerShell
{% raw %}
UploadFolderAsZipAndGetStringResponse
    [-url] <String>
    [-sourcePath] <String>
{% endraw %}
```

## Description

Uploads a folder as a zip file to a URL

## Example

```PowerShell
{% raw %}
UploadFolderAsZipAndGetStringResponse -url "https://example.com/upload" -file "C:\Temp\Files"
{% endraw %}
```
