---
title: dnsHelper.ps1
description: Details about the dnsHelper.ps1 script
linkText: dnsHelper.ps1
---

# dnsHelper.ps1

## Overview

The `dnsHelper.ps1` script has functionality to assist creating and checking DNS entries

## Functions

{% include childPages.html %}

{% comment %}
GetDnsRecords ([string]$dnsServer, [string]$domain, [string]$lookupName)
GetAllSubValues ([string]$dnsServer, [string]$domain, [string]$lookupName)
CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
DeleteCName ([string]$dnsServer, [string]$domain, [string]$name)
CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [string]$ttl="3600")
UpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
CreateOrUpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [bool]$warnOnUpdate = $false)
CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
DeleteARecord ([string]$dnsServer, [string]$domain, [string]$name)
CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [string]$ttl="3600")
UpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
CreateOrUpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [bool]$warnOnUpdate = $false)
CreateOrUpdateDns ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddressOrServer, [bool]$warnOnUpdate = $false)
DeleteDns([string]$dnsServer, [string]$domain, [string]$name, [bool]$warnOnUpdate = $false)
CheckHostsEntry ([string]$Address, [string]$FullyQualifiedName)
AddHostsEntry ([string]$Address, [string]$FullyQualifiedName)
{% endcomment %}

