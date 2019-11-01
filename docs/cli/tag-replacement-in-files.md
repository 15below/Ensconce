---
title: Tag Replacement In Files
description: How to update files which have Ensconce template tags
---

# Tag Replacement In Files

## Overview

With Ensconce you can perform template tag replacement in any file without the need for any substitution files.

Providing a wildcard pattern for searching files, Ensconce will parse each of those files & update the contents.

## Syntax

`--deployFrom=[sourcePath] --treatAsTemplateFilter=[searchPattern]`
