---
title: createWebsite.ps1 - CreateVirtualDirectory
linkText: CreateVirtualDirectory
description: Details about the CreateVirtualDirectory function in createWebsite.ps1 helper script
---

# CreateVirtualDirectory

```PowerShell
{% raw %}
CreateVirtualDirectory
    [-webSite] <String>
    [-virtualDir] <String>
    [-physicalPath] <String>    
{% endraw %}
```

## Description

Create new virtual directory under an existing website

## Example

```PowerShell
{% raw %}
CreateVirtualDirectory -webSite "MySite" -virtualDir "SubDir" -physicalPath "C:\Site\SubDir"
{% endraw %}
```
