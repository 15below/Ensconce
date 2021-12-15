---
title: compressionHelper.ps1 - Create7z
linkText: Create7z
description: Details about the Create7z function in compressionHelper.ps1 helper script
---

# Create7z

```PowerShell
{% raw %}
Create7z
    [-sourcePath] <String>
    [-destinationFile] <String>
{% endraw %}
```

## Description

Creates a 7zip file with the contents of the source path

## Example

```PowerShell
{% raw %}
Create7z -sourcePath "Path\To\Folder" -destinationFile "MyZip.7z"
{% endraw %}
```
