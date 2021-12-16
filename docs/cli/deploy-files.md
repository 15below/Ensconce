---
title: Deploying Files
description: How to deploy files
linkText: Deploying Files
---

# Deploying Files

## Overview

Ensconce has the ability to deploy files either performing a copy function to add files, or a replace option to completely remove all existing files and replace with new files.

If the files you are deploying are applications, you should see the [Deploying Applications]({{ '/' | relative_url }}cli/deploy-apps){:.link-secondary} page.

## Example

### Copy Files

```powershell
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo
```

### Replace Files

```powershell
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --replace
```
