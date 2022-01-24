---
title: azureHelper.ps1 - Azure-DeployWebApp
linkText: Azure-DeployWebApp
description: Details about the Azure-DeployWebApp function in azureHelper.ps1 helper script
---

# Azure-DeployWebApp

```PowerShell
{% raw %}
Azure-DeployWebApp
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
Azure-DeployWebApp -username "[service-guid]" -password "PASSWORD-HERE" -tenant "[tenant-guid]" -resourceGroup "MyWebAppResource" -name "MyWebApp" -contentFolder "Path\To\Folder"
{% endraw %}
```
