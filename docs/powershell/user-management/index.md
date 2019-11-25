---
title: userManagement.ps1
description: Details about the userManagement.ps1 helper script
---

# userManagement.ps1

## Overview

The `userManagement.ps1` script has functionality to setup and manage user accounts on Windows

## Functions

* AddUser([string]$name, [string]$password)
* AddUserToGroup([string]$name, [string]$group)
* CheckAndCreateServiceAccount([string]$name, [string]$password)
* CheckAndCreateUserAccount([string]$name, [string]$password)
* SetServiceAccount([string]$serviceName, [string]$account, [string]$password)
