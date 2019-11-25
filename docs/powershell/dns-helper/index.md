---
title: dnsHelper.ps1
description: Details about the dnsHelper.ps1 script
---

# dnsHelper.ps1

## Overview

The `dnsHelper.ps1` script has functionality to assist creating and checking DNS entries

## Functions

* AddHostsEntry ([string]$Address, [string]$FullyQualifiedName)
* CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
* CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
* CheckHostsEntry ([string]$Address, [string]$FullyQualifiedName)
* CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
* CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
* CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
