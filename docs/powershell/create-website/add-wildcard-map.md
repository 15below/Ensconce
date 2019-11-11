---
title: createWebsite.ps1 - AddWildcardMap
linkText: AddWildcardMap
description: Details about the AddWildcardMap function in createWebsite.ps1 helper script
---

# AddWildcardMap

```PowerShell
{% raw %}
AddWildcardMap
    [-websiteName] <String>
    [-subFolders] <String>
{% endraw %}
```

## Description

The `AddWildcardMap` function will add ASP.NET MVC application support to IIS6

## Examples

**Example 1: No sub-folders**

```PowerShell
{% raw %}
AddWildcardMap -websiteName "MySite" -subFolders ""
{% endraw %}
```

**Example 2: With sub-folder tree**

```PowerShell
{% raw %}
AddWildcardMap -websiteName "MySite" -subFolders "Some\Sub\Folder"
{% endraw %}
```
