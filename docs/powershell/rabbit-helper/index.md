---
title: rabbitHelper.ps1
description: Details about the rabbitHelper.ps1 helper script
---

# rabbitHelper.ps1

## Overview

The `rabbitHelper.ps1` script has functionality to interact with a RabbitMQ server instance

## Functions

* CreateRabbitVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$vHost)
* CreateRabbitUser([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$password)
* AddUserToVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$vHost)
* ValidateUserAccess([string]$rabbitApiUrl, [string]$user, [string]$password, [string]$vHost)
* CreateRabbitUserAndVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$password, [string]$vHost)
