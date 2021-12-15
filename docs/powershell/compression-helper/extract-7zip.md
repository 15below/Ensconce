---
title: compressionHelper.ps1 - Extract7z
linkText: Extract7z
description: Details about the Extract7z function in compressionHelper.ps1 helper script
---

# Extract7z

```PowerShell
{% raw %}
Extract7z
    [-sourceFile] <String>
    [-destinationFolder] <String>
{% endraw %}
```

## Description

Extracts a 7zip file to a provided location

## Example

```PowerShell
{% raw %}
Extract7z -sourceFile "MyZip.7z" -destinationFolder "Path\To\Folder"
{% endraw %}
```
