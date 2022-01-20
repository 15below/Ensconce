---
title: azureHelper.ps1 - Azure-DeployZipToWebApp
linkText: Azure-DeployZipToWebApp
description: Details about the Azure-DeployZipToWebApp function in azureHelper.ps1 helper script
---

# Azure-DeployZipToWebApp

```PowerShell
{% raw %}
Azure-DeployZipToWebApp
    [-username] <String>
    [-password] <String>
    [-tenant] <String>
    [-resourceGroup] <String>
    [-name] <String>
    [-contentFolder] <String>
{% endraw %}
```

## Description

Will deploy the contents of the folder provided into a named Azure WebApp within a resource group using an Azure service principal to authenticate

## Example

```PowerShell
{% raw %}
Azure-DeployZipToWebApp -username "[service-guid]" -password "PASSWORD-HERE" -tenant "[tenant-guid]" -resourceGroup "MyWebAppResource" -name "MyWebApp" -contentFolder "Path\To\Folder"
{% endraw %}
```
