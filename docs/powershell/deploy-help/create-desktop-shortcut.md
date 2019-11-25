---
title: deployHelp.ps1 - CreateDesktopShortcut
linkText: CreateDesktopShortcut
description: Details about the CreateDesktopShortcut function in deployHelp.ps1 helper script
---

# CreateDesktopShortcut

```PowerShell
{% raw %}
CreateDesktopShortcut
    [-exePath] <String>
    [-shortcutName] <String>
{% endraw %}
```

## Description

The `CreateDesktopShortcut` function creates a shortcut to an exe on the desktop.

## Examples

```PowerShell
{% raw %}
CreateDesktopShortcut -exePath "C:\App\StartApp.exe" -shortcutName "Start App"
{% endraw %}
```
