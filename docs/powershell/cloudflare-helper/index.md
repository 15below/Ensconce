---
title: cloudflareHelper.ps1
description: Details about the cloudflareHelper.ps1 script
linkText: cloudflareHelper.ps1
---

# cloudflareHelper.ps1

## Overview

The `cloudflareHelper.ps1` script has functionality to assist interacting with the cloudflare API in order to create DNS entries

## Functions

* [CallCloudflare](call-cloudflare)
* [GetCloudflareDnsZone](get-cloudflare-dns-zone)
* [GetCloudflareDnsRecord](get-cloudflare-dns-record)
* CheckCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$domain, [string]$record)
* GetCloudflareDnsIp([string]$token, [string]$domain, [string]$record)
* ExportDnsRecords([string]$token, [string]$zoneid, [string]$domain)
* GetCloudflareDnsRecords([string]$token, [string]$domain, [string]$filter = "")
* CreateCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$domain, [string]$record, [string]$content, [string]$type)
* UpdateCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$recordid, [string]$domain, [string]$record, [string]$type, [string]$content, [bool]$warnOnUpdate = $false)
* CreateOrUpdateCloudflareARecord([string]$token, [string]$domain, [string]$record, [string]$ipaddr, [bool]$warnOnUpdate = $false)
* CreateOrUpdateCloudflareCNAMERecord([string]$token, [string]$domain, [string]$record, [string]$cnameValue, [bool]$warnOnUpdate = $false)
* RemoveCloudflareDnsRecord([string]$token, [string]$domain, [string]$record, [bool]$warnOnDelete = $false)
