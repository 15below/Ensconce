---
title: deployHelp.ps1 - ensconce
linkText: ensconce
description: Details about the ensconce function in deployHelp.ps1 helper script
---

# ensconce

```PowerShell
{% raw %}
ensconce
{% endraw %}
```

## Description

The `ensconce` function is a helper to call into the Ensconce executable without needing to know the exact executable name.

This is the recommended way to call Ensconce so as to avoid issues should the internal name or file structure change.

## Example

**Example 1: Pipe text into ensconce**

```PowerShell
{% raw %}
"{ ClientCode }" | ensconce -i
{% endraw %}
```

**Example 2: Copy files between 2 configured paths with the replace option**

```PowerShell
{% raw %}
ensconce --deployFrom="{{ SourcePath }}" --deployTo="{{ DestinationPath }}" --replace
{% endraw %}
```
