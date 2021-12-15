---
title: compressionHelper.ps1 - ExtractZip
linkText: ExtractZip
description: Details about the ExtractZip function in compressionHelper.ps1 helper script
---

# ExtractZip

```PowerShell
{% raw %}
ExtractZip
    [-sourceFile] <String>
    [-destinationFolder] <String>
{% endraw %}
```

## Description

Extracts a zip file to a provided location

## Example

```PowerShell
{% raw %}
ExtractZip -sourceFile "MyZip.zip" -destinationFolder "Path\To\Folder"
{% endraw %}
```
