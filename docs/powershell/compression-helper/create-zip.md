---
title: compressionHelper.ps1 - CreateZip
linkText: CreateZip
description: Details about the CreateZip function in compressionHelper.ps1 helper script
---

# CreateZip

```PowerShell
{% raw %}
CreateZip
    [-sourcePath] <String>
    [-destinationFile] <String>
{% endraw %}
```

## Description

Creates a zip file with the contents of the source path

## Examples

```PowerShell
{% raw %}
CreateZip -sourcePath "Path\To\Folder" -destinationFile "MyZip.zip"
{% endraw %}
```
