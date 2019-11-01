---
title: Deploying Apps
description: Application deployment details
---

# Deploying Apps

## Overview

Whilst [deploying files]({{ '/' | relative_url }}cli/deploy-files) Ensconce has the ability to update the deployed application configuration using a  [deployment substitution]({{ '/' | relative_url }}substitutions).

In order to trigger the application configuration update we add an extra parameter.

## Syntax

### Copy Files & Update Configuration

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo --updateConfig`

This will assume there is a file alongside your calling location called `substitutions.xml`.

### Replace Files & Update Configuration

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig`

This will assume there is a file alongside your calling location called `substitutions.xml`.

### Using A Different Substitution File

This can be used with a `--copyTo` or a `--replace`.

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig --substitutionPath=[substitutionFilePath]`
