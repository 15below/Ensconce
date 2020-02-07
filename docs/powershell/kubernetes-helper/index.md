---
title: kubernetesHelper.ps1
description: Details about the kubernetesHelper.ps1 helper script
---

# kubernetesHelper.ps1

## Overview

The `kubernetesHelper.ps1` script has functionality to interact with a Kubernetes environment

## Functions

* ValidateK8sYaml([string]$yamlDirectory)
* SetK8sContext([string] $kubernetesConfigFile, [string] $kubernetesContext)
* DeployToK8s([string]$yamlDirectory, [string] $kubernetesConfigFile)
* DeployYamlFilesToK8sClusters([string]$yamlDirectory, [string] $kubernetesContext)
