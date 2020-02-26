---
title: kubernetesHelper.ps1
description: Details about the kubernetesHelper.ps1 helper script
---

# kubernetesHelper.ps1

## Overview

The `kubernetesHelper.ps1` script has functionality to interact with a Kubernetes environment

*NOTE: This will only be available if `includeK8s` variable is set to `True` on deploy*

## Functions

* ValidateK8sYaml([string]$yamlDirectory, [string]$kubernetesConfigFile)
* SetK8sContext([string]$kubernetesConfigFile)
* GetResourceVersionsUsed([string]$kubernetesConfigFile, [string]$selector)
* DeployToK8s([string]$yamlDirectory, [string]$kubernetesConfigFile, [string]$pruneSelector)
* DeployYamlFilesToK8sCluster([string]$yamlDirectory, [string]$kubernetesConfigFile, [string]$pruneSelector)
