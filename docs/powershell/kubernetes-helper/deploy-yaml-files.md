---
title: kubernetesHelper.ps1 - DeployYamlFilesToK8sCluster
linkText: DeployYamlFilesToK8sCluster
description: Details about the DeployYamlFilesToK8sCluster function in kubernetesHelper.ps1 helper script
---

# DeployYamlFilesToK8sCluster

```PowerShell
{% raw %}
DeployYamlFilesToK8sCluster
    [-yamlDirectory] <String>
    [-kubernetesConfigFile] <String>
    [-pruneSelector] <String>
{% endraw %}
```

## Description

Deploys a set of yaml files to a Kubernetes instance using a specific config file.

For prune to work correctly it needs to be provided with a selector

## Examples

```PowerShell
{% raw %}
DeployYamlFilesToK8sCluster -yamlDirectory "/Path/To/YAML" -kubernetesConfigFile "/.kube/instance1.yaml" -pruneSelector "app=myapp"
{% endraw %}
```
