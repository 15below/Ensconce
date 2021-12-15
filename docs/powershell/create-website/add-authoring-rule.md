---
title: createWebsite.ps1 - AddAuthoringRule
linkText: AddAuthoringRule
description: Details about the AddAuthoringRule function in createWebsite.ps1 helper script
---

# AddAuthoringRule

```PowerShell
{% raw %}
AddAuthoringRule
    [-websiteName] <String>
    [-userName] <String>
    [-access] <String>
{% endraw %}
```

## Description

Add authoring rules for a user on a WebDav store.

## Example

```PowerShell
{% raw %}
AddAuthoringRule -websiteName "MyWebDavStore" -user "WebDavUser" -access "Read"s
{% endraw %}
```
