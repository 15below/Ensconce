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

Get-ChildItem -Path $scriptDir\Content\*.ps1 | ForEach-Object {
	$scriptName = $_.Name
	$scriptFullName = $_.FullName
	Write-Host "Deploying script $scriptName to $DeployPath"
	Copy-Item -Path $scriptFullName -Destination $DeployPath -Force
}

Get-ChildItem -Path $scriptDir\Content\Tools | ForEach-Object {
	$toolName = $_.Name
	$toolFullName = $_.FullName
	Write-Host "Deploying tool $toolName to $DeployPath\Tools"
	New-Item -Path "$DeployPath\Tools" -Name $toolName -ItemType "Directory" -Force | Out-Null
	Copy-Item -Path $toolFullName -Destination $DeployPath\Tools -Force -Recurse
}

Write-Host "Create releaseVersion.txt"
New-Item -Path $DeployPath -Name "releaseVersion.txt" -ItemType "file" -Value $VersionNumber