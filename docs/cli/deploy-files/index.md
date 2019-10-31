# How To Deploy Files

## Overview

Ensconce has the ability to deploy files either performing a copy function to add files, or a replace option to completely remove all existing files and replace with new files.

It is also possible to use a [deployment substitution]({{ '/' | relative_url }}substitutions) as part of the deployment of files.

## Syntax

### Copy Files

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --copyTo`

### Replace Files

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --replace`

### Update Config

This can be used with a `--copyTo` or a `--replace`

`--deployFrom=[sourcePath] --deployTo=[destinationPath] --replace --updateConfig`

This will assume there is a file alongside your calling location called `substitutions.xml`.
