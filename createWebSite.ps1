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
	. .\createiis6app.ps1
}
else
{
	write-host "Using IIS7"
	. .\createiis7app.ps1
}