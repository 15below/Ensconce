---
title: Tag Replacement In Files
description: How to update files which have Ensconce template tags
linkText: Tag Replacement In Files
---

# Tag Replacement In Files

## Overview

With Ensconce you can perform [template tag]({{ '/' | relative_url }}templates){:.link-secondary} replacement in any file without the need for any substitution files.

Providing a wildcard pattern for searching files, Ensconce will parse each of those files & update the contents.

## Example

```cmd
ensconce --deployFrom=[sourcePath] --treatAsTemplateFilter=[searchPattern]
```
