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
    [-iconPath] <String> = ""
    [-arguments] <String> = ""
{% endraw %}
```

## Description

The `CreateDesktopShortcut` function creates a shortcut to an exe on the desktop.

## Example

```PowerShell
{% raw %}
CreateDesktopShortcut -exePath "C:\App\StartApp.exe" -shortcutName "Start App"
{% endraw %}
```

```PowerShell
{% raw %}
CreateDesktopShortcut -exePath "C:\App\StartApp.bat" -shortcutName "Start App" -iconPath "C:\App\StartApp.exe" -arguments "arg1"
{% endraw %}
```
