---
title: Deployment Config Path
description: How to specify the deployment config file path
linkText: Deployment Config Path
---

# Deployment Config Path

## Overview

Ensconce relies on a [deployment configuration file]({{ '/' | relative_url }}config) to specify variable contents to be used in the deployment interactions.

By default, Ensconce will use the environment variable `FixedPath` which should contain a path to the XML deployment configuration file.

It's possible to override environment variable, or use a path rather than the environment variable.

## Syntax

This example is a file copy, with the additional parameter

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo --fixedPath=[deployConfigurationPath]`
