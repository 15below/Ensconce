---
title: dnsHelper.ps1
description: Details about the dnsHelper.ps1 script
---

# dnsHelper.ps1

## Overview

The `dnsHelper.ps1` script has functionality to assist creating and checking DNS entries

## Functions

* CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
* CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
* DeleteCName ([string]$dnsServer, [string]$domain, [string]$name)
* CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
* UpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
* CreateOrUpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [bool]$warnOnUpdate = $false)
* CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
* DeleteARecord ([string]$dnsServer, [string]$domain, [string]$name)
* CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
* UpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
* CreateOrUpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [bool]$warnOnUpdate = $false)
