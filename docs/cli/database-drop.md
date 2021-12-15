---
title: Database Drop
description: How to drop a database
linkText: Database Drop
---

# Database Drop

## Overview

In situations where you require your database to be dropped prior to deployment (in the case of a testing system) Ensconce has the ability to do this.

## Example

```cmd
ensconce --connectionString=[sqlConnectionString] --dropDatabase --dropDatabaseConfirm
```
