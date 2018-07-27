Write-Host "Ensconce - CreateIIS6App Loading"
$frameworkPath = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client").InstallPath
set-alias regIIS $frameworkPath\aspnet_regiis.exe

function TestInclude ([string]$name) {
	(get-host).ui.rawui.foregroundcolor= "Magenta"
	$name
	(get-host).ui.rawui.foregroundcolor= "Yellow"
}

function CheckIfAppPoolExists ([string]$name)
{
	$tempPool  = gwmi -namespace "root\MicrosoftIISv2" -class "IISApplicationPoolSetting" -filter "Name like '%$name%'"
	$tempPool -ne $NULL
}

function CheckIfWebSiteExists ([string]$name)
{
	$tempWebsite  = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$name%'"
	$tempWebsite -ne $NULL
}

function CheckIfWebApplicationExists ([string]$webSite, [string]$appName)
{
	$tempWebsite  = (gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$webSite%'")
	$tempApp = (gwmi -namespace "root\MicrosoftIISv2" -class "IISWebDirectory" | where {$_.name -like "$tempWebSite/*$appName" })
	if ($tempApp -ne $NULL)
	{
    $tempApp.AppGetStatus().returnvalue -ne 2
	}
	else
	{ $False }
}

function CheckIfVirtualDirectoryExists ([string]$webSite, [string]$virtualDir)
{
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$webServer = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer"	 -AND $_.ServerComment -eq $website }
	$webVirtualDir = ($webServer.children | where { $_.keyType -eq "IIsWebVirtualDir" }).children | where { $_.keyType -eq "IIsWebVirtualDir" -and $_.Name -eq $virtualDir}
	($webVirtualDir -ne $null)
}

function CheckIfSslBindingExists ([string]$webSite, [string]$hostHeader)
{
	$true
}
function CreateAppPool ([string]$name)
{
	# check if pool exists and delete it - for testing purposes
	"    Creating ISS app pool for " + $name
	$tempPool = gwmi -namespace "root\MicrosoftIISv2" -class "IISApplicationPoolSetting" -filter "Name like '%$name%'"
	if (($tempPool -eq $NULL)) {

		# create Application Pool
		$appPoolSettings = [wmiclass] "root\MicrosoftIISv2:IISApplicationPoolSetting"
		$newPool = $appPoolSettings.CreateInstance()

		$newPool.Name = "W3SVC/AppPools/" + $name

		$newPool.PeriodicRestartTime = 1740
		$newPool.IdleTimeout = 20
		$newPool.MaxProcesses = 1
		$newPool.AppPoolIdentityType = 3

		$newPool.Put()
	}
}

function SetAppPoolIdentity([string]$name, [string]$user, [string]$password)
{
	$appPool = gwmi -namespace "root\MicrosoftIISv2" -class "IISApplicationPoolSetting" -filter "Name like '%$name%'"

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
		$appPool.WAMUserName = $user
		$appPool.WAMUserPass = $password
		# Only add user to group if its a specific user, dont do it for networkservice
		AddUserToGroup $user "IIS_WPG"
	}

	$appPool.AppPoolIdentityType = $identityType
	$appPool.Put()
}

function StopAppPool([string]$name)
{
	$appPool = get-wmiobject -namespace "root\MicrosoftIISv2" -class "IIsApplicationPool" | where-object {$_.Name -eq "W3SVC/AppPools/$name"}
	if ($appPool)
	{
		"Stopping AppPool: " + $name | Write-Host
		$appPool.Stop()
	}
}

function StartAppPool([string]$name)
{
	$appPool = get-wmiobject -namespace "root\MicrosoftIISv2" -class "IIsApplicationPool" | where-object {$_.Name -eq "W3SVC/AppPools/$name"}
	if ($appPool)
	{
		"Starting AppPool: " + $name | Write-Host
		$appPool.Start()
	}
}

function RestartAppPool([string]$name)
{
	$appPool = get-wmiobject -namespace "root\MicrosoftIISv2" -class "IIsApplicationPool" | where-object {$_.Name -eq "W3SVC/AppPools/$name"}
	if ($appPool)
	{
		"Restarting AppPool: " + $name | Write-Host
		$appPool.Stop()
		$appPool.Start()
	}
}

function StopWebSite([string]$name)
{
	"Stopping WebSite: " + $name | Write-Host
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$website = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer" -AND $_.ServerComment -eq $name }
	if ($website -ne $NULL)
	{
		$website.serverState = 4
		$website.setInfo()
	}
}

function StartWebSite([string]$name)
{
	"Starting WebSite: " + $name | Write-Host
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$website = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer" -AND $_.ServerComment -eq $name }
	if ($website -ne $NULL)
	{
		$website.serverState = 2
		$website.setInfo()
	}
}

function CreateWebSite ([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation)
{
	# check if web site exists and delete it - for testing purposes
	"    Creating IIS website for " + $name
	$tempWebsite  = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$name%'"
	if (($tempWebsite -eq $NULL)) {

		$iisWebService  = gwmi -namespace "root\MicrosoftIISv2" -class "IIsWebService"

		$bindingClass = [wmiclass]'root\MicrosoftIISv2:ServerBinding'
		$bindings = $bindingClass.CreateInstance()
		$bindings.Port = "80"
		$bindings.Hostname = $hostname

		EnsurePath $localPath
		$NewSite = $iisWebService.CreateNewSite($name, $bindings, $localPath)

		$webServerSettings  = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$name%'"

		$iis = [ADSI]"IIS://localhost/W3SVC"
		$webServer = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer"	 -AND $_.ServerComment -eq $name }

		$webserver.AspEnableParentPaths = $True
		$webserver.LogFileDirectory = $logLocation
		$webServer.Properties["AccessFlags"].Value = 513
		$webServer.Properties["AuthFlags"].Value = 1
		$webServer.DefaultDoc = "index.asp," + $webServer.DefaultDoc
		$webServer.AppPoolID = $appPoolName

		$webserver.SetInfo()

		$webVirtualDir = $webServer.children | where { $_.keyType -eq "IIsWebVirtualDir" }

		# Set Application name
		$webVirtualDir.AppFriendlyName = $applicationName

		# Save changes
		$webServer.CommitChanges()
		$webVirtualDir.CommitChanges()

		# Switch the Website to .NET 4.0
		$webServerSettings.Name
		regiis -s $webServerSettings.Name

		# Start the newly created web site
		if (!($webServer -eq $NULL)) {$webServer.start()}
	}
}

function AddHostHeader([string]$siteName, [string] $hostHeader, [int] $port, [string] $protocol)
{
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$site = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer" -AND $_.ServerComment -eq $siteName }
	if($site -ne $null)
	{
		$webBinding = $site.ServerBindings | where { $_.Port -eq $port -AND $_.Hostname -eq $hostHeader -AND $_.IP -eq "*" }
		if($webBinding -eq $null) {
			if( $hostHeader -eq "" ) {
				"Host-header is empty, cannot add" | Write-Host
			}
			else {
				"Adding additional host-header binding of: $hostHeader, port: $port" | Write-Host
				"Ignoring protocol of: '$protocol' - IIS6 does not have this concept "| Write-Host
				$site.ServerBindings.Insert($site.ServerBindings.Count, ":$port:$hostHeader")
			}
		}
		else {
			"Http host header already exists - no need to add" | Write-Host
		}
	}
}


function CreateWebApplication([string]$webSite, [string]$appName, [string] $appPool, [string]$InstallDir ,[string]$subFolders)
{
	EnsurePath $InstallDir
	$webServerSettings  = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$webSite%'"

	if ($subFolders -eq $null -or $subFolders -eq "" )
	{

		$dirSettings = [wmiclass] "root\MicrosoftIISv2:IIsWebDirectory"
		$newDir = $dirSettings.CreateInstance()
		$newDir.Name = ($webServerSettings.Name + '/ROOT/' + $appName)
		$newDir.Description = $appPool
		$newDir.Put()
	} else {
		$virtualDirName = $subFolders +"\" + $appName
		CreateVirtualDirectory $webSite $virtualDirName $installDir
		$nvdir = ($webServerSettings.Name + '/ROOT/' + $virtualDirName)

		$nvdir = $nvdir.Replace("\", "/")

		$newDir = gwmi -namespace "root\microsoftiisv2" -Class "IIsWebVirtualDir" -filter "Name='$nvdir'"
	}

	$newDir.AppCreate3(2, $appPool, $True)
}

function CreateVirtualDirectory([string]$webSite, [string]$virtualDir, [string]$installDir)
{
  "Creating $virtualDir pointing at $installDir" | Write-Host
  $webServerSettings  = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '%$webSite%'"
  $virtualDirSettings = [wmiclass] "root\MicrosoftIISv2:IIsWebVirtualDirSetting"
  $virtualDirName = $virtualDir
  if ($virtualDirName.StartsWith("\")) {
    $virtualDirName = $VirtualDirName.substring(1)
  }
  $newVDir = $virtualDirSettings.CreateInstance()
  $newVDir.Name = ($webServerSettings.Name + '/ROOT/' + $virtualDirName)
  $newVDir.Path = $installDir

  $newVDir.Put();
}

function AddSslCertificate ([string] $websiteName, [string] $friendlyName, [string] $hostHeader)
{
	# This method requires for you to have selfssl on your machine
	$selfSslPath = "\program files\iis resources\selfssl"
	if (Test-Path $selfSslPath)
	{
		$certificateCommonName = "/N:cn=" + $friendlyName

		$certificateValidityDays = "/V:3650"
		$websitePort = "/P:443"
		$addToTrusted = "/T"
		$quietMode = "/Q"


		$webServerSetting = gwmi -namespace "root\MicrosoftIISv2" -class "IISWebServerSetting" -filter "ServerComment like '$websiteName'"
		$websiteId ="/S:" + $webServerSetting.name.substring($webServerSetting.name.lastindexof('/')+1)

		cd -path $selfSslPath
		.\selfssl.exe $addToTrusted $friendlyName $certificateValidityDays $websitePort $websiteId $quietMode
	}
	else
	{
		"Didn't add SSL ass SelfSSL was not installed on this machine!" | Write-Host
	}
}

function EnableWebDav ([string] $websiteName)
{
	"EnableWebDav is not supported for IIS6" | Write-Host
}

function AddAuthoringRule ([string] $websiteName, [string] $userName, [string] $access)
{
	"AddAutoringRole is not supported for IIS6" | Write-Host
}

function AddWildcardMap ([string] $websiteName, [string]$subFolders)
{
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$webServer = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer"	 -AND $_.ServerComment -eq $websiteName }
	$webVirtualDir = $webServer.children | where { $_.keyType -eq "IIsWebVirtualDir" }
	if ($subFolders -ne $null -and $subFolders -ne "" )
	{
		$folders = $subFolders.split("\")
		foreach ($folder in $Folders)
		{
			$webVirtualDir = ($webVirtualDir.Children | where {$_.name -like $Folder })
		}
	}

	if ($webVirtualDir -ne $null)
	{
		$check = $webVirtualDir.ScriptMaps | where {$_.EndsWith(", All")}
		if ($check -eq $null)
		{
			# Add wildcard map
			$wildcardMap = "*,C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll, 0, All"
			$webVirtualDir.ScriptMaps.Add($wildcardMap)
			$webVirtualDir.CommitChanges()
		}
	}
}

function AddDefaultDocument ([string] $websiteName, [string] $defaultDocumentName)
{
	$iis = [ADSI]"IIS://localhost/W3SVC"
	$webServer = $iis.psbase.children | where { $_.keyType -eq "IIsWebServer"	 -AND $_.ServerComment -eq $websiteName }
	if ($webServer.DefaultDoc.Contains($defaultDocumentName) -eq $false)
	{
		"Adding $defaultDocumentName as a default document of $websiteName" | Write-Host
		$webServer.DefaultDoc = "index.asp," + $webServer.DefaultDoc
		$webServer.CommitChanges()
	}
}

function EnableParentPaths ([string] $websiteName)
{
	"EnableParentPaths is set during site creation for IIS6" | Write-Host
}

function Enable32BitApps ([string] $appPoolName)
{
	"The Enable32BitApps function does nothing in IIS6" | Write-Host
}

function DefaultApplicationPoolGroup ()
{
	"IIS_WPG"
}

function SetMaxRequestEntityAllowed([string] $websiteName, [int] $maxRequestEntityAllowedValue)
{
	"The SetMaxRequestEntityAllowed function does nothing in IIS6" | Write-Host
}

function RequireClientCertificate([string] $websiteName)
{
	throw [System.NotImplementedException] "Only implemented for IIS7 Apps"
}

function SetManagedRuntimeVersion([string] $appPoolName, [string] $runtimeVersion)
{
	throw [System.NotImplementedException] "Only implemented for IIS7 Apps"
}

function SetManagedRuntimeToNoManagedCode([string] $appPoolName, [string] $runtimeVersion)
{
	throw [System.NotImplementedException] "Only implemented for IIS7 Apps"
}

Write-Host "Ensconce - CreateIIS6App Loaded"