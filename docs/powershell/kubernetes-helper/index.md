---
title: kubernetesHelper.ps1
description: Details about the kubernetesHelper.ps1 helper script
---

# kubernetesHelper.ps1

## Overview

The `kubernetesHelper.ps1` script has functionality to interact with a Kubernetes environment

*NOTE: This will only be available if `includeK8s` variable is set to `True` on deploy*

## Functions

* ValidateK8sYaml([string]$yamlDirectory)
* SetK8sContext([string] $kubernetesConfigFile, [string] $kubernetesContext)
* DeployToK8s([string]$yamlDirectory, [string] $kubernetesConfigFile)
* DeployYamlFilesToK8sClusters([string]$yamlDirectory, [string] $kubernetesContext)
