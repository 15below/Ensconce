$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if(Test-Path $DeployPath)
{
    Write-Host "Removing Deploy Tools Directory Found at $DeployPath"
    Remove-Item $DeployPath -Force -Recurse
    Start-Sleep -s 2
}

Write-Host "Creating Deploy Tools Directory at $DeployPath"
New-Item $DeployPath -Type container

if($includeK8s -ne "True")
{
	Write-Host "Kubernetes items will not deploy...To deploy set 'includeK8s' to 'True'"
	Remove-Item "$scriptDir\Content\Tools\Kubernetes" -Force -Recurse
	Remove-Item "$scriptDir\Content\kubernetesHelper.ps1" -Force
}

Write-Host "Deploying Tools to $DeployPath"
Copy-Item "$scriptDir\Content\*" $DeployPath -Force -Recurse