$currentDirectory = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if($deployHelpLoaded -eq $null)
{	
	. $currentDirectory\deployHelp.ps1
}

Write-Host "Ensconce - KubernetesHelper Loading"
$KubeCtlExe = "$currentDirectory\Tools\Kubernetes\kubectl.exe"
$rootConfigPath = "$Home\.kube"

function ValidateK8sYaml([string]$yamlDirectory, [string]$kubernetesConfigFile)
{
	$kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
	
	Write-Host "Validating yaml in $yamlDirectory (local)"
	& $KubeCtlExe apply --dry-run -f $yamlDirectory --kubeconfig=$kubernetesConfigFilePath
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Invalid yaml in $yamlDirectory"
		exit $LASTEXITCODE
	}
	
	Write-Host "Validating yaml in $yamlDirectory (server-side)"
	& $KubeCtlExe apply --server-dry-run  -f $yamlDirectory --kubeconfig=$kubernetesConfigFilePath
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Invalid yaml in $yamlDirectory"
		exit $LASTEXITCODE
	}
}

function SetK8sContext([string]$kubernetesConfigFile)
{
	$kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
	
	Write-Host "Working with cluster 'k8s-cluster'"
	& $KubeCtlExe config use-context "k8s-cluster" --kubeconfig=$kubernetesConfigFilePath
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Error setting kubernetes context to 'k8s-cluster'"
		exit $LASTEXITCODE
	}
}

function GetResourceVersionsUsed([string]$kubernetesConfigFile, [string]$selector)
{
	Write-Host "Get Accessible Resources"
	$resourceVersions = @()
	$resources = @()
		
	$rawResources = & $KubeCtlExe api-resources --verbs=list --namespaced -o name --kubeconfig=$kubernetesConfigFilePath
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Error all api resources"
		exit $LASTEXITCODE
	}
	
	foreach($resource in $rawResources)
	{
		if($resource.Contains("."))
		{
			$resource = $resource.Substring(0, $resource.IndexOf("."))
		}

		if($resources -notcontains $resource)
		{
			Write-Host "  Checking $resource is accessible"
			$cani = & $KubeCtlExe auth can-i list $resource --kubeconfig=$kubernetesConfigFilePath
			
			if ($LASTEXITCODE -eq 0 -and $cani -eq "yes")
			{
				$resources += $resource
			}
		}
	}
	
	Write-Host "Accessible Server Resources: $resources"
	Write-Host "Getting Resources Used On Selector: $selector"
	
	foreach($resource in $resources)
	{
		Write-Host "  Getting: $resource"
		$output = & $KubeCtlExe get $resource -l $selector -o json --kubeconfig=$kubernetesConfigFilePath | ConvertFrom-Json	
			
		if ($LASTEXITCODE -ne 0)
		{
			Write-Error "Error getting $resource"
			exit $LASTEXITCODE
		}

		foreach($item in $output.items)
		{
			$groupVersion = $item.apiVersion
			if(-not($groupVersion.Contains("/")))
			{
				$groupVersion = "core/${groupVersion}"
			}
			$kind = $item.kind
			$GroupVersionKind = "${groupVersion}/${kind}"

			if($resourceVersions -notcontains $GroupVersionKind)
			{
				$resourceVersions += $GroupVersionKind
			}
		}
	}
	
	Write-Host "Used API Versions: $resourceVersions"
	$resourceVersions
}

function DeployToK8s([string]$yamlDirectory, [string]$kubernetesConfigFile, [string]$pruneSelector)
{
	$kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
	
	$prunableList = GetResourceVersionsUsed $kubernetesConfigFile $pruneSelector
	
	$deploymentName = ""
	Write-Host "Deploying yaml in $yamlDirectory"
	#Run using Invoke-Expression because of dynamic parameters	
	$pruneWhiteList = $prunableList -join " --prune-whitelist="
	Invoke-Expression "$KubeCtlExe apply -f $yamlDirectory --prune -l $pruneSelector --prune-whitelist=$pruneWhiteList --kubeconfig=$kubernetesConfigFilePath" | foreach-object {
		Write-Host $_
		if($_.StartsWith("deployment.apps/"))
		{
			$deploymentLineName = $_.Substring(0, $_.IndexOf(' '))
			if($deploymentName -eq "")
			{
				$deploymentName = $deploymentLineName
			}
			else
			{
				Write-Warning "Second deployment detected in package with name $deploymentLineName - This will not be checked for rollout"
			}			
		}
	}
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Error applying yaml in $yamlDirectory"
		exit $LASTEXITCODE
	}
	
	if ($deploymentName -eq "")
	{
		Write-Error "Unable to establish deployment name"
		exit -1
	}
	
	For ($i=0; $i -lt 5; $i++) {
		& $KubeCtlExe get $deploymentName --kubeconfig=$kubernetesConfigFilePath
		if ($LASTEXITCODE -eq 0) {
			break
		}
		Start-Sleep 5
	}
	
	& $KubeCtlExe rollout status $deploymentName --kubeconfig=$kubernetesConfigFilePath
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Rollout of $deploymentName was not successful"
		exit $LASTEXITCODE
	}
}

function DeployYamlFilesToK8sCluster([string]$yamlDirectory, [string]$kubernetesConfigFile, [string]$pruneSelector)
{
	Write-Host "Replace tags in yaml in $yamlDirectory"
	ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.yaml | Write-Host
	
	SetK8sContext $kubernetesConfigFile
	
	ValidateK8sYaml	$yamlDirectory $kubernetesConfigFile	
	
	DeployToK8s $yamlDirectory $kubernetesConfigFile $pruneSelector
}

Write-Host "Ensconce - KubernetesHelper Loaded"
