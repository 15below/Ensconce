# Resource For Looking Up IIS Powershell Snap In Commands/Functions
# http://learn.iis.net/page.aspx/447/managing-iis-with-the-iis-powershell-snap-in/

$ModuleName = "WebAdministration"
$ModuleLoaded = $false
$LoadAsSnapin = $false

if ($PSVersionTable.PSVersion.Major -ge 2) {
    if ((Get-Module -ListAvailable | ForEach-Object {$_.Name}) -contains $ModuleName) {
        write-host "Module $ModuleName Is Avaliable!"
		write-host "Running: Import-Module $ModuleName"
		Import-Module $ModuleName
        if ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName) {
            $ModuleLoaded = $true
			write-host "Loaded $ModuleName As Module"
        } else {
            $LoadAsSnapin = $true
			write-host "Didn't Load Module For $ModuleName Will Try Snap In"
        }
    } elseif ((Get-Module | ForEach-Object {$_.Name}) -contains $ModuleName) {
        $ModuleLoaded = $true
		write-host "The Module $ModuleName Is Already Available & Imported"
    } else {
        $LoadAsSnapin = $true
		write-host "Module $ModuleName Not Found Will Try Snap In"
    }
} else {
    $LoadAsSnapin = $true
	write-host "Not Powershell >= 2, Will Try Snap In"
}

if ($LoadAsSnapin) {
    if ((Get-PSSnapin -Registered | ForEach-Object {$_.Name}) -contains $ModuleName) {
        write-host "Snap-In $ModuleName Is Avaliable!"
		write-host "Add-PSSnapin $ModuleName"
		Add-PSSnapin $ModuleName
        if ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName) {
            $ModuleLoaded = $true
			write-host "Loaded $ModuleName As Snap In"	
        }
    } elseif ((Get-PSSnapin | ForEach-Object {$_.Name}) -contains $ModuleName) {
        $ModuleLoaded = $true
		write-host "The Snap In $ModuleName Is Already Available & Imported"
    } else
	{
		write-host "Couldn't Load Module Or Snap In For $ModuleName"
		write-host "Returning Error As Cannot Process Any Functions"
		exit 1
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