---
title: webHelper.ps1 - DownloadFile
linkText: DownloadFile
description: Details about the DownloadFile function in webHelper.ps1 helper script
---

# DownloadFile

```PowerShell
{% raw %}
DownloadFile
    [-url] <String>
    [-destinationPath] <String>
{% endraw %}
```

## Description

Downloads a file from a URL and saves to disk

## Example

```PowerShell
{% raw %}
DownloadFile -url "https://example.com/myFile.txt" -destination "C:\Temp\myFile.txt"
{% endraw %}
```
