Write-Host "Ensconce - CreateWebsite Loading"
$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

$ErrorActionPreference = 'SilentlyContinue'
$IISVersion = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\InetStp").SetupString

if ($IISVersion.Contains("6.0"))
{
	$IIS6 = $true
}
else
{
	$IIS6 = $false
}
$ErrorActionPreference = 'Continue'
if($IIS6 -eq $true)
{
	write-host "Using IIS6"
	. $DeployToolsDir\createiis6app.ps1
}
else
{
	write-host "Using IIS7"
	. $DeployToolsDir\createiis7app.ps1
}

Write-Host "Ensconce - CreateWebsite Loaded"
$createWebSiteLoaded = $true
