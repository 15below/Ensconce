$currentDirectory = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if($deployHelpLoaded -eq $null)
{	
	. $currentDirectory\deployHelp.ps1
}

Write-Host "Ensconce - KubernetesHelper Loading"
$KubeCtlExe = "$currentDirectory\Tools\Kubernetes\kubectl.exe"
$KubeValExe = "$currentDirectory\Tools\Kubernetes\kubeval.exe"

function DeployYamlFilesToK8sClusters([string]$yamlDirectory)
{
	Write-Host "Replace tags in yaml in $yamlDirectory"
	ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.yaml | Write-Host

	Write-Host "Validating yaml in $yamlDirectory"
	& $KubeValExe -d $yamlDirectory
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Invalid yaml in $yamlDirectory"
		exit $LASTEXITCODE
	}

	Write-Host "Working with cluster $KubernetesContext"
	& $KubeCtlExe config use-context $KubernetesContext
	
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Error setting kubernetes context to $KubernetesContext"
		exit $LASTEXITCODE
	}
	
	$deploymentName = ""
	& $KubeCtlExe apply -f $yamlDirectory | foreach-object {
		write-host $_
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
		& $KubeCtlExe get $deploymentName
		if ($LASTEXITCODE -eq 0) {
			break
		}
		Start-Sleep 5
	}
	
	& $KubeCtlExe rollout status $deploymentName
	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Rollout of $deploymentName was not successful"
		exit $LASTEXITCODE
	}
}

Write-Host "Ensconce - KubernetesHelper Loaded"
