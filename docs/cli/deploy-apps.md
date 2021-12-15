---
title: Deploying Applications
description: How to deploy applications with a configuration update
linkText: Deploying Applications
---

# Deploying Applications

## Overview

Whilst [deploying files]({{ '/' | relative_url }}cli/deploy-files){:.link-secondary} Ensconce has the ability to update the deployed application configuration using a  [deployment substitution]({{ '/' | relative_url }}substitutions){:.link-secondary}.

In order to trigger the application configuration update we add an extra parameter.

## Example

### Copy Files & Update Configuration

```cmd
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo --updateConfig
```

This will assume there is a file alongside your calling location called `substitutions.xml`.

### Replace Files & Update Configuration

```cmd
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig
```

This will assume there is a file alongside your calling location called `substitutions.xml`.

### Using A Different Substitution File

This can be used with a `--copyTo` or a `--replace`.

```cmd
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig --substitutionPath=[substitutionFilePath]
```

### Output Partially Applied Output On Failure

This can be used with a `--copyTo` or a `--replace`.

```cmd
ensconce --deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig --outputFailureContext
```
