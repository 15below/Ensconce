Write-Host "Ensconce - CreateIIS7App Loading"
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

function CheckIfAppPoolExists ([string]$name)
{
	Test-Path "IIS:\AppPools\$name"
}

function StopAppPool([string]$name)
{
	$status = (Get-WebAppPoolState -Name $name).Value

	if ($status -eq "Started")
	{
		"Stopping AppPool: " + $name | Write-Host
		Stop-WebAppPool "$name"
	}
	else
	{
		"AppPool not in Started state ($status): " + $name | Write-Host
	}
}

function UpdateAppPoolRecycling([string]$name, [string]$periodicRestart="02:00:00", [int32]$privateMemory=1048576)
{
	Set-ItemProperty IIS:\AppPools\$name Recycling.periodicRestart.time -Value $periodicRestart
	Set-ItemProperty IIS:\AppPools\$name Recycling.periodicRestart.privateMemory -Value $privateMemory
}

function StartAppPool([string]$name)
{
	$status = (Get-WebAppPoolState -Name $name).Value

	if ($status -ne "Started")
	{
		"Starting AppPool: " + $name | Write-Host
		Start-WebAppPool "$name"
	}
	else
	{
		"AppPool already in Started state: " + $name | Write-Host
	}
}

function RestartAppPool([string]$name)
{
	$status = (Get-WebAppPoolState -Name $name).Value

	if ($status -eq "Started")
	{
		"Restarting AppPool: " + $name | Write-Host
		Restart-WebItem "IIS:\AppPools\$name"
	}
	else
	{
		"AppPool not in Started state ($status): " + $name | Write-Host
	}
}

function StopWebSite([string]$name)
{
	$status = "Unknown"

	try
	{
		$status = (Get-WebsiteState -Name $name).Value

		if ($status -eq "Started")
		{
			"Stopping WebSite: " + $name | Write-Host
			Stop-WebSite -Name $name
		}
		else
		{
			"WebSite not in Started state ($status): " + $name | Write-Host
		}
	}
	catch
	{
		"Error Stopping WebSite: " + $name | Write-Host
	}
}

function StartWebSite([string]$name)
{
	$status = "Unknown"

	try
	{
		$status = (Get-WebsiteState -Name $name).Value

		if ($status -ne "Started")
		{
			"Starting WebSite: " + $name | Write-Host
			Start-WebSite -Name $name
		}
		else
		{
			"WebSite already in Started state: " + $name | Write-Host
		}
	}
	catch
	{
		"Error Starting WebSite: " + $name | Write-Host
	}
}

function CheckIfWebApplicationExists ([string]$webSite, [string]$appName)
{
	$tempApp = Get-WebApplication -Site $webSite | where-object {$_.path.contains($appName) }
	$tempApp -ne $NULL
}

function CheckIfVirtualDirectoryExists ([string]$webSite, [string]$virtualDir)
{
	$tempApp = Get-WebVirtualDirectory -Site $webSite | where-object {$_.path.contains($virtualDir) }
	$tempApp -ne $NULL
}

function CheckIfSslBindingExists ([string]$webSite, [string]$hostHeader)
{
	$tempApp = Get-WebBinding -Name $webSite -HostHeader $hostHeader -Protocol https
	$tempApp -ne $NULL
}

function CheckIfWebSiteExists ([string]$name)
{
	Test-Path "IIS:\Sites\$name"
}

function CreateAppPool ([string]$name)
{
	try
	{
	 $poolCreated = Get-WebAppPoolState $name -ErrorVariable myerrorvariable
	}
	catch
	{
	 # Assume it doesn't exist. Create it.
	 New-WebAppPool -Name $name
	 Set-ItemProperty IIS:\AppPools\$name managedRuntimeVersion v4.0
	}
}

function SetManagedPipelineModeClassic ([string]$name)
{
	Set-ItemProperty IIS:\AppPools\$name managedPipelineMode 1
}

function SetAppPoolIdentity([string]$name, [string]$user, [string]$password)
{
	$appPool = Get-Item "IIS:\AppPools\$name"

	# LocalSystem = 0
	# LocalService = 1
	# NetworkService = 2
	# SpecificUser = 3
	# ApplicationPoolIdentity = 4
	$identityType = 3
	if($user -And $user.ToLower() -eq "networkservice")
	{
		$identityType = 2
	}
	else
	{
		$appPool.processModel.username = $user
		$appPool.processModel.password = $password
	}
	$appPool.processModel.identityType = $identityType
	$appPool | set-item
}

function CreateWebSite ([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation, [int32] $port=80)
{
	$site = Get-WebSite | where { $_.Name -eq $name }
	if($site -eq $null)
	{
		EnsurePath $localPath
		if ($port -eq 443) {
			New-WebSite $name -Port $port -HostHeader $hostName -PhysicalPath $localPath -ApplicationPool $appPoolName -ssl
		} else {
			New-WebSite $name -Port $port -HostHeader $hostName -PhysicalPath $localPath -ApplicationPool $appPoolName
		}
	}

	Set-ItemProperty IIS:\Sites\$name -name logFile.directory -value $logLocation
}

function AddHostHeader([string]$siteName, [string] $hostHeader, [int] $port, [string] $protocol)
{
	if($protocol -eq "" ) {
		$protocol = "http"
	}

	$site = Get-WebSite | where { $_.Name -eq $siteName }
	if($site -ne $null)
	{
		$webBinding = Get-WebBinding -Name $siteName -IPAddress "*" -Port $port -HostHeader $hostHeader -Protocol $protocol
		if($webBinding -eq $null) {
			if( $hostHeader -eq "" ) {
				"Host-header is empty, cannot add" | Write-Host
			}
			else {
				$supportedProtocols = "http", "https", "net.tcp", "net.pipe", "net.msmq", "msmq.formatname"
				if($supportedProtocols -contains $protocol) {
					"Adding additional host-header binding of: $hostHeader, port: $port, protocol: $protocol" | Write-Host
					New-WebBinding -Name $siteName -IPAddress "*" -Port $port -HostHeader $hostHeader -Protocol $protocol
				}
				else {
					"Error - cant add binding, protocol: $protocol is not supported in IIS7" | Write-Host
				}
			}
		}
		else {
			"Http host header already exists - no need to add" | Write-Host
		}
	}
}

function CreateWebApplication([string]$webSite, [string]$appName, [string] $appPool, [string]$InstallDir, [string]$SubFolders)
{
	EnsurePath $installDir
	if ($subFolders -eq $null -or $subFolders -eq "")
	{ New-WebApplication -Name $appName -Site $webSite -PhysicalPath $installDir -ApplicationPool $appPool }
	else
	{
		"$installDir\$SubFolders"
		New-WebApplication -Name "$SubFolders\$appName" -Site $webSite -PhysicalPath $installDir -ApplicationPool $appPool
	}
}

function CreateVirtualDirectory([string]$webSite, [string]$virtualDir, [string]$physicalPath)
{
	"Creating $virtualDir pointing at $physicalPath" | Write-Host
	EnsurePath $physicalPath
	New-WebVirtualDirectory -Site $webSite -Name $virtualDir -PhysicalPath $physicalPath
}

function AddSslCertificate ([string] $websiteName, [string] $friendlyName, [string] $hostHeader)
{
	$checkBinding = CheckIfSslBindingExists $instanceName $hostHeader
	if ( $checkBinding -eq $False) {
		New-WebBinding -Name $websiteName -IP "*" -Port 443 -Protocol https -HostHeader $hostHeader
	}

	try
	{
		# Avoid Error 234: https://stackoverflow.com/questions/21859308/failed-to-enumerate-ssl-bindings-error-code-234
		$bindings=Get-ChildItem IIS:\SslBindings
	}
	catch
	{
		$firstentry = Get-ChildItem HKLM:\SYSTEM\CurrentControlSet\services\HTTP\Parameters\SslBindingInfo | Select-Object -First 1
		$firstentry | New-ItemProperty -Name "SslCertStoreName" -Value "MY"
		$bindings=Get-ChildItem IIS:\SslBindings
	}

	if (($bindings | where-object {$_.port -eq "443" -and $_.IPAddress -eq "0.0.0.0"}) -eq $Null)
	{
		Set-Location IIS:\sslbindings
		get-childitem -Path cert:\LocalMachine -Recurse | Where-Object {$_.FriendlyName -eq $friendlyName} | Select-Object -first 1 | new-item 0.0.0.0!443
		Set-Location $scriptDir
	}
}

function EnableWebDav ([string] $websiteName)
{
	"Enabling WebDav authoring for $websiteName" | Write-Host
	Set-WebConfigurationProperty -filter "/system.WebServer/webdav/authoring" -name enabled -value true -PSPath "IIS:\" -location $websiteName
}

function AddAuthoringRule ([string] $websiteName, [string] $userName, [string] $access)
{
	$exists = Get-WebConfiguration -filter "system.WebServer/webdav/authoringRules/add[@users='$userName' and @path='*']" -PSPath "IIS:\Sites\$websiteName"
	if ($exists -eq $null)
	{
		"Giving $userName $access access for WebDav on $websiteName" | Write-Host
		Add-WebConfiguration -filter "/system.WebServer/webdav/authoringRules" -Value @{users=$userName;path="*";access=$access} -PSPath "IIS:\" -location $websiteName
	}
}

function AddWildcardMap ([string] $websiteName)
{
	"Don't need to do anything for IIS7"
}

function AddDefaultDocument ([string] $websiteName, [string] $defaultDocumentName)
{
	$exists = get-webconfiguration -filter "//defaultDocument/Files/Add[@value='$defaultDocumentName']" "IIS:\sites\$websiteName"
	if ($exists -eq $null)
	{
		"Adding $defaultDocumentName as a default document of $websiteName" | Write-Host
		Add-WebConfiguration //defaultDocument/files "IIS:\sites\$websiteName" -atIndex 0 -Value @{value=$defaultDocumentName}
	}
}

function EnableParentPaths ([string] $websiteName)
{
	$current = get-webconfigurationproperty -filter "//asp" -name enableParentPaths "IIS:\Sites\$websiteName"
	if ($current.Value -eq $False)
	{
		"Setting enableParentPaths to True for $websiteName" | Write-Host
		Set-WebConfigurationProperty -Filter "//asp" -name enableParentPaths -PSPath "IIS:\" -value true -location $websiteName
	}
}

function Enable32BitApps ([string] $appPoolName)
{
	$current = (get-itemProperty IIS:\AppPools\$appPoolName).enable32BitAppOnWin64

	if ($current -eq $False)
	{
		"Setting enable32BitAppOnWin64 to True for the $appPoolName AppPool" | Write-Host
		Set-ItemProperty IIS:\AppPools\$appPoolName enable32BitAppOnWin64 true
	}
}

function DefaultApplicationPoolGroup ()
{
	"IIS_IUSRS"
}

function EnableBasicAuthentication([string] $websiteName)
{
	Set-WebConfigurationProperty -filter "/system.webServer/security/authentication/basicAuthentication" -name enabled -value true -PSPath "IIS:\" -location $websiteName
}

function SetMaxRequestEntityAllowed([string] $websiteName, [int] $maxRequestEntityAllowedValue)
{
	Set-WebConfigurationProperty -Filter "//asp/limits" -name maxRequestEntityAllowed -PSPath "IIS:\" -value $maxRequestEntityAllowedValue -location $websiteName
}

function RequireClientCertificate([string] $websiteName)
{
	"Setting SSL Require Client Certs for $websiteName" | Write-Host
	Set-WebConfiguration -Location "$webSiteName" -filter 'system.webserver/security/access' -Value "Ssl, SslRequireCert"
}

function SetManagedRuntimeVersion([string] $appPoolName, [string] $runtimeVersion)
{
	# Valid runtime versions can be found here: https://msdn.microsoft.com/en-us/library/aa347554(v=VS.90).aspx
	Set-ItemProperty IIS:\AppPools\$appPoolName managedRuntimeVersion $runtimeVersion
}

function SetManagedRuntimeToNoManagedCode([string] $appPoolName, [string] $runtimeVersion)
{
	# "" is the same as "No Managed Code"
	Set-ItemProperty IIS:\AppPools\$appPoolName managedRuntimeVersion ""
}

Write-Host "Ensconce - CreateIIS7App Loaded"
$createIIS7AppLoaded = $true
