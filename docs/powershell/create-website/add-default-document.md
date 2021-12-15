---
title: createWebsite.ps1 - AddDefaultDocument
linkText: AddDefaultDocument
description: Details about the AddDefaultDocument function in createWebsite.ps1 helper script
---

# AddDefaultDocument

```PowerShell
{% raw %}
AddDefaultDocument
    [-websiteName] <String>
    [-defaultDocumentName] <String>
{% endraw %}
```

## Description

Adds a default document to existing website.

## Example

```PowerShell
{% raw %}
AddDefaultDocument -websiteName "MySite" -defaultDocumentName "welcome.htm"
{% endraw %}
```
