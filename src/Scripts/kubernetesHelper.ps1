$currentDirectory = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if($deployHelpLoaded -eq $null)
{	
	. $currentDirectory\deployHelp.ps1
}

Write-Host "Ensconce - KubernetesHelper Loading"
$KubeCtlExe = "$currentDirectory\Tools\Kubernetes\kubectl.exe"
$KubeValExe = "$currentDirectory\Tools\Kubernetes\kubeval.exe"
$kubernetesConfigFilePath = "$Home\.kube"

function ValidateK8sYaml([string]$yamlDirectory)
{
	Write-Host "Validating yaml in $yamlDirectory"
	& $KubeValExe -d $yamlDirectory
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Invalid yaml in $yamlDirectory"
		exit $LASTEXITCODE
	}
}

function SetK8sContext([string] $kubernetesConfigFile, [string] $kubernetesContext)
{
	Write-Host "Working with cluster $kubernetesContext"
	$kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
	& $KubeCtlExe config use-context $kubernetesContext --kubeconfig=$kubernetesConfigFilePath
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Error setting kubernetes context to $kubernetesContext"
		exit $LASTEXITCODE
	}
}

function DeployToK8s([string]$yamlDirectory, [string] $kubernetesConfigFile)
{
	$kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
	$deploymentName = ""
	& $KubeCtlExe apply -f $yamlDirectory --kubeconfig=$kubernetesConfigFilePath | foreach-object {
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

function DeployYamlFilesToK8sCluster([string]$yamlDirectory, [string] $kubernetesConfigFile, [string] $kubernetesContext)
{
	Write-Host "Replace tags in yaml in $yamlDirectory"
	ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.yaml | Write-Host
	
	ValidateK8sYaml	$yamlDirectory

	SetK8sContext $kubernetesConfigFile $kubernetesContext
	
	DeployToK8s $kubernetesConfigFile $kubernetesContext 
}

Write-Host "Ensconce - KubernetesHelper Loaded"
