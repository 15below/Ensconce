if ($deployHelpLoaded -eq $null)
{
	$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
    . $DeployToolsDir\deployHelp.ps1
}

Write-Host "Ensconce - CreateWebsite Loading"

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

$iisInfo = Get-ItemProperty HKLM:\SOFTWARE\Microsoft\InetStp\
$iisVersion = [decimal]"$($iisInfo.MajorVersion).$($iisInfo.MinorVersion)"

Write-Host "IIS Version: $iisVersion"

function CheckIfAppPoolExists ([string]$name)
{
    Test-Path "IIS:\AppPools\$name"
}

function GetAppPoolState ([string]$name)
{
    return (Get-WebAppPoolState -Name $name).Value
}

function StopAppPool([string]$name)
{
    try
    {
        $status = GetAppPoolState $name

        if($status -eq $null){
            "Skipping stop AppPool as no current status: " + $name | Write-Host
            return
        }

        if ($status -ne "Stopped")
        {
            "Stopping AppPool: " + $name | Write-Host
            Retry-Command { Stop-WebAppPool "$name" } 5 250

            for($i=1; $i -le 30; $i++) {
                $status = GetAppPoolState $name

                if ($status -eq "Stopped")
                {
                    "AppPool stopped: " + $name | Write-Host
                    break
                } else {
                    if($i -ge 30) {
                        throw "AppPool not stopped (status=$status): " + $name
                    } else {
                        "AppPool not stopped (status=$status): " + $name | Write-Host
                        Write-Host "Sleep for 2 seconds..."
                        Start-Sleep -Milliseconds 2000
                    }
                }
            }
        }
        else
        {
            "AppPool already in Stopped state: " + $name | Write-Host
        }
    }
    catch
    {
        "Error Stopping AppPool " + $name | Write-Host
        throw
    }
}

function UpdateAppPoolRecycling([string]$name, [string]$periodicRestart="02:00:00", [int32]$privateMemory=1048576)
{
    Set-ItemProperty IIS:\AppPools\$name Recycling.periodicRestart.time -Value $periodicRestart
    Set-ItemProperty IIS:\AppPools\$name Recycling.periodicRestart.privateMemory -Value $privateMemory
}

function StartAppPool([string]$name)
{
    try
    {
        $status = GetAppPoolState $name

        if($status -eq $null){
            "Skipping start AppPool as no current status: " + $name | Write-Host
            return
        }

        if ($status -ne "Started")
        {
            "Starting AppPool: " + $name | Write-Host
            Retry-Command { Start-WebAppPool "$name" } 5 250

            for($i=1; $i -le 30; $i++) {
                $status = GetAppPoolState $name

                if ($status -eq "Started")
                {
                    "AppPool started: " + $name | Write-Host
                    break
                } else {
                    if($i -ge 30) {
                        throw "AppPool not started (status=$status): " + $name
                    } else {
                        "AppPool not started (status=$status): " + $name | Write-Host
                        Write-Host "Sleep for 2 seconds..."
                        Start-Sleep -Milliseconds 2000
                    }
                }
            }
        }
        else
        {
            "AppPool already in Started state: " + $name | Write-Host
        }
    }
    catch
    {
        "Error Starting AppPool " + $name | Write-Host
        throw
    }
}

function RestartAppPool([string]$name)
{
    try
    {
        $status = GetAppPoolState $name

        if($status -eq $null){
            "Skipping restart AppPool as no current status: " + $name | Write-Host
            return
        }

        if ($status -eq "Started")
        {
            "Restarting AppPool: " + $name | Write-Host
            Retry-Command { Restart-WebItem "IIS:\AppPools\$name" } 5 250

            for($i=1; $i -le 30; $i++) {
                $status = GetAppPoolState $name

                if ($status -eq "Started")
                {
                    "AppPool started: " + $name | Write-Host
                    break
                } else {
                    if($i -ge 30) {
                        throw "AppPool not started (status=$status): " + $name
                    } else {
                        "AppPool not started (status=$status): " + $name | Write-Host
                        Write-Host "Sleep for 2 seconds..."
                        Start-Sleep -Milliseconds 2000
                    }
                }
            }
        }
        else
        {
            "AppPool not in Started state ($status): " + $name | Write-Host
        }
    }
    catch
    {
        "Error Restarting AppPool: " + $name | Write-Host
        throw
    }
}

function StopWebSite([string]$name)
{
    $status = "Unknown"

    try
    {
        $siteProtocol = "http"

        (Get-WebBinding -Name $name) | ForEach-Object{
            if ($_.Protocol -eq "ftp")
            {
                $siteProtocol = "ftp"
            }
        }

        $status = (Get-WebItemState -PSPath "IIS:\sites\$name" -Protocol $siteProtocol).Value

        if($status -eq $null){
            "Skipping stop website as no current status: " + $name | Write-Host
            return
        }

        if ($status -ne "Stopped")
        {
            "Stopping Website: " + $name + " Protocol " + $siteProtocol | Write-Host
            Retry-Command { Stop-WebItem -PsPath "IIS:\sites\$name" -Protocol $siteProtocol } 5 250

            for($i=1; $i -le 30; $i++) {
                $status = (Get-WebItemState -PSPath "IIS:\sites\$name" -Protocol $siteProtocol).Value

                if ($status -eq "Stopped")
                {
                    "Website Stopped: " + $name | Write-Host
                    break
                } else {
                    if($i -ge 30) {
                        throw "Website not stopped (status=$status): " + $name
                    } else {
                        "Website not stopped (status=$status): " + $name | Write-Host
                        Write-Host "Sleep for 2 seconds..."
                        Start-Sleep -Milliseconds 2000
                    }
                }
            }
        }
        else
        {
            "WebSite already in Stopped state: " + $name | Write-Host
        }
    }
    catch
    {
        "Error Stopping WebSite: " + $name | Write-Host
        throw
    }
}

function StartWebSite([string]$name)
{
    $status = "Unknown"

    try
    {
        $siteProtocol = "http"

        (Get-WebBinding -Name $name) | ForEach-Object{
            if ($_.Protocol -eq "ftp")
            {
                $siteProtocol = "ftp"
            }
        }

        $status = (Get-WebItemState -PSPath "IIS:\sites\$name" -Protocol $siteProtocol).Value

        if($status -eq $null){
            "Skipping start website as no current status: " + $name | Write-Host
            return
        }

        if ($status -ne "Started")
        {
            "Starting WebSite: " + $name + " Protocol " + $siteProtocol | Write-Host
            Retry-Command { Start-WebItem -PsPath "IIS:\sites\$name" -Protocol $siteProtocol } 5 250

            for($i=1; $i -le 30; $i++) {
                $status = (Get-WebItemState -PSPath "IIS:\sites\$name" -Protocol $siteProtocol).Value

                if ($status -eq "Started")
                {
                    "Website Started: " + $name | Write-Host
                    break
                } else {
                    if($i -ge 30) {
                        throw "Website not started (status=$status): " + $name
                    } else {
                        "Website not started (status=$status): " + $name | Write-Host
                        Write-Host "Sleep for 2 seconds..."
                        Start-Sleep -Milliseconds 2000
                    }
                }
            }
        }
        else
        {
            "WebSite already in Started state: " + $name | Write-Host
        }
    }
    catch
    {
        "Error Starting WebSite " + $name | Write-Host
        throw
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

function CheckIfSslBindingExists ([string]$webSite, [string]$hostHeader, [string] $ipAddress="*")
{
    $tempApp = Get-WebBinding -Name $webSite -IPAddress $ipAddress -HostHeader $hostHeader -Protocol https
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
    if ($user -And $user.ToLower() -eq "networkservice")
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

function CreateWebSite ([string]$name, [string]$localPath, [string] $appPoolName, [string] $applicationName, [string] $hostName, [string] $logLocation, [int32] $port=80, [string] $ipAddress="*")
{
    # accounts for possible empty strings
    if (!$ipAddress)
    {
        $ipAddress = "*"
    }

    $site = Get-WebSite | where { $_.Name -eq $name }
    if ($site -eq $null)
    {
        EnsurePath $localPath
        if ($port -eq 443) {
            New-WebSite $name -IPAddress $ipAddress -Port $port -HostHeader $hostName -PhysicalPath $localPath -ApplicationPool $appPoolName -ssl
        } else {
            New-WebSite $name -IPAddress $ipAddress -Port $port -HostHeader $hostName -PhysicalPath $localPath -ApplicationPool $appPoolName
        }
    }

    Set-ItemProperty IIS:\Sites\$name -name logFile.directory -value $logLocation
}

function AddHostHeader([string]$siteName, [string] $hostHeader, [int] $port, [string] $protocol, [string] $ipAddress="*")
{
    if ($protocol -eq "" ) {
        $protocol = "http"
    }

    # accounts for possible empty strings
    if (!$ipAddress)
    {
        $ipAddress = "*"
    }

    $site = Get-WebSite | where { $_.Name -eq $siteName }
    if ($site -ne $null)
    {
        $webBinding = Get-WebBinding -Name $siteName -IPAddress $ipAddress -Port $port -HostHeader $hostHeader -Protocol $protocol
        if ($webBinding -eq $null) {
            if ( $hostHeader -eq "" ) {
                "Host-header is empty, cannot add" | Write-Host
            }
            else {
                $supportedProtocols = "http", "https", "net.tcp", "net.pipe", "net.msmq", "msmq.formatname"
                if ($supportedProtocols -contains $protocol) {
                    "Adding additional host-header binding of: $hostHeader, port: $port, protocol: $protocol" | Write-Host
                    if ($iisVersion -gt 8 -and $protocol -eq "https")
                    {
                        New-WebBinding -Name $siteName -IPAddress $ipAddress -Port $port -HostHeader $hostHeader -Protocol $protocol -SslFlags 1
                    }
                    else
                    {
                        New-WebBinding -Name $siteName -IPAddress $ipAddress -Port $port -HostHeader $hostHeader -Protocol $protocol
                    }
                }
                else {
                    "Error - cant add binding, protocol: $protocol is not supported in IIS" | Write-Host
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

function AddSslCertificate ([string] $websiteName, [string] $friendlyName, [string] $hostHeader, [string] $ipAddress)
{
    # accounts for possible empty strings
    if (!$ipAddress)
    {
        $ipAddress = "*"
    }

    $checkBinding = CheckIfSslBindingExists $instanceName $hostHeader
    if ($checkBinding -eq $True) {
        write-host "SSL binding of $hostHeader on $instanceName already exists, skipping."
        return
    }

    $cert = GetSslCert $friendlyName
    $certThumbprint = $cert.Thumbprint

    if($certThumbprint -eq $null -or $certThumbprint -eq "") {
        throw "SSL Cert $friendlyName has no thumbprint"
    }

    if ($iisVersion -gt 8)
    {
        New-WebBinding -Name $websiteName -IP $ipAddress -Port 443 -Protocol https -HostHeader $hostHeader -SslFlags 1
    }
    else
    {
        New-WebBinding -Name $websiteName -IP $ipAddress -Port 443 -Protocol https -HostHeader $hostHeader
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

    Set-Location IIS:\sslbindings

    if ($iisVersion -gt 8)
    {
        if ($ipAddress -eq "*")
        {
            if (($bindings | where-object {$_.port -eq "443" -and $_.Host -eq $hostHeader}) -eq $Null)
            {
                new-item *!443!$hostHeader -Thumbprint $certThumbprint -SSLFlags 1
            }
        }
        else
        {
            if (($bindings | where-object {$_.port -eq "443" -and $_.IPAddress -eq $ipAddress -and $_.Host -eq $hostHeader}) -eq $Null)
            {
                new-item $ipAddress!443!$hostHeader -Thumbprint $certThumbprint -SSLFlags 1
            }
        }
    }
    else
    {
        if ($ipAddress -eq "*")
        {
            $ipAddress = "0.0.0.0"
        }

        if (($bindings | where-object {$_.port -eq "443" -and $_.IPAddress -eq $ipAddress}) -eq $Null)
        {
            new-item $ipAddress!443 -Thumbprint $certThumbprint
        }
    }

    Set-Location $scriptDir
}

function GetSslCert([string] $friendlyName)
{
    $cert = get-childitem -Path cert:\LocalMachine -Recurse | Where-Object {$_.FriendlyName -eq $friendlyName} | Select-Object -first 1

    if ($cert -eq $null)
    {
        throw "SSL Cert $friendlyName not found"
    }

    $cert
}

function EnableWebDav ([string] $websiteName)
{
    "Enabling WebDav authoring for $websiteName" | Write-Host
    Set-WebConfigurationProperty -filter "/system.WebServer/webdav/authoring" -name enabled -value true -PSPath "IIS:\" -location $websiteName
}

function AddAuthoringRule ([string] $websiteName, [string] $userName, [string] $access)
{
    $exists = $false
    Get-WebConfiguration -filter "system.WebServer/webdav/authoringRules/add[@path='*']" -PSPath "IIS:\Sites\$websiteName" | ForEach-Object {
        if($_.users.ToLower() -eq $userName.ToLower())
        {
            $exists = $true
        }
    }

    if ($exists -eq $false)
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

Write-Host "Ensconce - CreateWebsite Loaded"
$createWebSiteLoaded = $true
