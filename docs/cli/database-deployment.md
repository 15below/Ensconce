---
title: Database Deployment
description: How to deploy a database
---

# Database Deployment

## Overview

Ensconce has the [roundhousE](https://github.com/chucknorris/roundhouse) project embedded within it's codebase.

This means that you can deploy databases using the roundhousE database deployment tool with the Ensconce framework.

## Syntax

### Standard Deployment

`--deployFrom=[pathToSQLFiles] --connectionString=[sqlConnectionString]`

### Warn On One Time Script Changes

By default, Ensconce will error if you change a one time script, you can override this behavior

`--deployFrom=[pathToSQLFiles] --connectionString=[sqlConnectionString] --warnOnOneTimeScriptChanges=True`

### Run Outside A Transaction

By default, Ensconce will run all scripts within a transaction which rolls back upon a failure.
Not all database change scripts can run within a transaction, so it's possible to disable this functionality

`--deployFrom=[pathToSQLFiles] --connectionString=[sqlConnectionString] --withTransaction=False`

### roundhousE Output

Ensconce tries to prevent the default roundhousE output, however roundhousE will still create some in flight files.

The path for this by default is `E:\RH`.  This can be overridden.

`--deployFrom=[pathToSQLFiles] --connectionString=[sqlConnectionString] --roundhouseOutputPath=[pathToOutput]`

### roundhousE Timeout

This is the timeout for SQL queries run by roundhousE, there is a default of 30 seconds.

`--deployFrom=[pathToSQLFiles] --connectionString=[sqlConnectionString] --databaseCommandTimeout=[CustomTimeoutValue]`
