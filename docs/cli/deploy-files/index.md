---
title: Deploying Files
description: File deployment details
---

# Deploying Files

## Overview

Ensconce has the ability to deploy files either performing a copy function to add files, or a replace option to completely remove all existing files and replace with new files.

If the files you are deploying are applications, you should see the [Deploying Applications]({{ '/' | relative_url }}cli/deploy-apps) page.

## Syntax

### Copy Files

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo`

### Replace Files

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --replace`
