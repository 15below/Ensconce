$deployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

$ErrorActionPreference = 'SilentlyContinue'
get-wmiobject -namespace "root\MicrosoftIISv2" -class "IISApplicationPoolSetting" | out-null
if ($?)
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
	. $deployToolsDir\createiis6app.ps1
}
else
{
	write-host "Using IIS7"
	. $deployToolsDir\createiis7app.ps1
}