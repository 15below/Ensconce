---
title: azureHelper.ps1 - Azure-CreateOrUpdateDnsRecord
linkText: Azure-CreateOrUpdateDnsRecord
description: Details about the Azure-CreateOrUpdateDnsRecord function in azureHelper.ps1
---

# Azure-CreateOrUpdateDnsRecord

Ensures an Azure DNS A or CNAME record exists with the requested value. Existing records are left unchanged when they already match. A record type is replaced safely when the requested type differs.

```PowerShell
Azure-CreateOrUpdateDnsRecord -username $servicePrincipal -password $password -tenant $tenant `
    -resourceGroup $resourceGroup -zoneName "example.com" -recordName "www" `
    -recordType "A" -value "192.0.2.10" -subscription $subscription
```

Use `Azure-CreateOrUpdateDnsARecord` or `Azure-CreateOrUpdateDnsCNameRecord` when a type-specific function is clearer. Credentials and environment-specific values should be supplied at runtime; do not store them in scripts or documentation.
