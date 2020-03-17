$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if(Test-Path $DeployPath)
{
    Write-Host "Removing Deploy Tools Directory Found at $DeployPath"
    Remove-Item $DeployPath -Force -Recurse
    Start-Sleep -s 2
}

Write-Host "Creating Deploy Tools Directory at $DeployPath"
New-Item $DeployPath -Type container

if($includeK8s -eq "True")
{
	Write-Host "Ensconce deployed in Kubernetes mode as 'includeK8s' set to 'True'"
	Remove-Item "$scriptDir\Content\backupHelper.ps1" -Force
	Remove-Item "$scriptDir\Content\createWebSite.ps1" -Force
	Remove-Item "$scriptDir\Content\dnsHelper.ps1" -Force
	Remove-Item "$scriptDir\Content\serviceManagement.ps1" -Force
}
else
{
	Write-Host "Ensconce deployed in application deployment mode as 'includeK8s' NOT set to 'True'"
	Remove-Item "$scriptDir\Content\Tools\KubeCtl" -Force -Recurse
	Remove-Item "$scriptDir\Content\kubernetesHelper.ps1" -Force
}

Write-Host "Deploying Tools to $DeployPath"

Get-ChildItem -Path $scriptDir\Content\*.ps1 | ForEach-Object {
	$scriptName = $_.Name
	Write-Host "Deploying script $scriptName"
	Copy-Item $_ $DeployPath -Force
}

Get-ChildItem -Path $scriptDir\Content\Tools | ForEach-Object {
	$toolName = $_.Name
	Write-Host "Deploying tool $toolName"
	New-Item -Path "$DeployPath\Tools" -Name $toolName -ItemType "Directory"
	Copy-Item $_ $DeployPath\Tools -Force -Recurse
}