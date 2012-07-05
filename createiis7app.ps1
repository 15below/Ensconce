# Resource For Looking Up IIS Powershell Snap In Commands/Functions
# http://learn.iis.net/page.aspx/447/managing-iis-with-the-iis-powershell-snap-in/

$ModuleName = "WebAdministration"
$ModuleLoaded = $false
$LoadAsSnapin = $false

if ($PSVersionTable.PSVersion.Major -ge 2) {
    if ((Get-Module -ListAvailable | ForEach-Object {$_.Name}) -contains $ModuleName) {
        write-host "Import-Module $ModuleName"
		Import-Module $ModuleName
        if ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName) {
            $ModuleLoaded = $true
			write-host "Loaded As Module"
        } else {
            $LoadAsSnapin = $true
			write-host "Need To Load Snap In"
        }
    } elseif ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName) {
        $ModuleLoaded = $true
		write-host "The Module Is Already Available"
    } else {
        $LoadAsSnapin = $true
		write-host "Need To Load Snap In"
    }
} else {
    $LoadAsSnapin = $true
	write-host "Need To Load Snap In"
}

if ($LoadAsSnapin) {
    if ((Get-PSSnapin -Registered | ForEach-Object {$_.Name}) -contains $ModuleName) {
        write-host "Add-PSSnapin $ModuleName"
		Add-PSSnapin $ModuleName
        if ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName) {
            $ModuleLoaded = $true
			write-host "Loaded As Snap In"	
        }
    } elseif ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName) {
        $ModuleLoaded = $true
		write-host "The Snap In Is Already Available"
    }
}

function CreateAppPool ([string]$name)
{
	try
	{
	 $poolCreated = Get-WebAppPoolState $name –errorvariable myerrorvariable
	}
	catch
	{
	 # Assume it doesn't exist. Create it.
	 New-WebAppPool -Name $name
	 Set-ItemProperty IIS:\AppPools\$name managedRuntimeVersion v4.0
	}
}

function CreateWebSite ([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation)
{
	$site = Get-WebSite | where { $_.Name -eq $name }
	if($site -eq $null)
	{
	 New-WebSite $name -Port 80 -HostHeader $hostName -PhysicalPath $localPath -ApplicationPool $appPoolName
	}
	
	Set-ItemProperty IIS:\Sites\$name -name logFile.directory -value $logLocation
}

function AddSslCertificate ([string] $websiteName, [string] $certificateCommonName)
{
	write-host "You Will Need To Write PowerShell For This - Or Do It Manually"
}