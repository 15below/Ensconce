---
title: Inline Tag Replacement
description: How to perform tag replacement on an input string
linkText: Inline Tag Replacement
---

# Inline Tag Replacement

## Overview

There are situations were you may require text replacement within a powershell deployment script.

Ensconce supports the ability to pass some text in and it will return the replaced text.

## Example

```powershell
$replacedText = "{{ Client }}-{{ Environment }}" | ensconce -i
```
