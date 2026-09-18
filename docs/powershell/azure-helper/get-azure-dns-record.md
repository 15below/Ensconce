---
title: azureHelper.ps1 - Azure-GetDnsRecord
linkText: Azure-GetDnsRecord
description: Details about the Azure-GetDnsRecord function in azureHelper.ps1
---

# Azure-GetDnsRecord

Looks up A and CNAME records in an Azure DNS zone using the Azure CLI and a service principal.

```PowerShell
Azure-GetDnsRecord -username $servicePrincipal -password $password -tenant $tenant `
    -resourceGroup $resourceGroup -zoneName "example.com" -recordName "www" `
    -subscription $subscription
```

The function returns Azure CLI record-set objects. Only A and CNAME record sets matching `recordName` are returned.

The lookup does not create or modify records.
