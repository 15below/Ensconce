Write-Host "Ensconce - ServiceManagement Loading"

Function StopService([string]$serviceName)
{
    If (Get-Service $serviceName -ErrorAction SilentlyContinue)
    {
        If ((Get-Service $serviceName).Status -eq 'Running')
        {
            "Stopping $serviceName" | Write-Host
            Stop-Service $serviceName | Write-Host
        }
        Else
        {
            "$serviceName found, but it is not running." | Write-Host
        }
    }
    Else
    {
        "$serviceName not found to stop" | Write-Host
    }
}

Function StartService([string]$serviceName)
{
    "Starting $serviceName" | Write-Host
    Start-Service $serviceName | Write-Host
}

Function SetServiceRunAs([string]$serviceName, [string]$serviceUser, [string]$servicePassword)
{
    "Setting service $serviceName to run as $serviceUser"
    & "sc.exe" config $serviceName obj= $serviceUser password= $servicePassword | Write-Host
}

Function SetServiceRestarts([string]$serviceName)
{
    "Setting service restarts for $serviceName"
    & "sc.exe" failure $serviceName reset= 86400 actions= restart/60000/restart/60000// | Write-Host
}

Function SetServiceRestartAlways([string]$serviceName)
{
    "Setting service restarts for $serviceName"
    & "sc.exe" failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Write-Host
}

Function RemoveService([string]$serviceName)
{
    If (Get-Service $serviceName -ErrorAction SilentlyContinue)
    {
        StopService $serviceName
        "Removing $serviceName" | Write-Host
        & "sc.exe" delete $serviceName
    }
}

Function InstallService([string]$serviceName, [string]$exePath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
{
    RemoveService $serviceName
    "Installing $serviceName with exe '$exePath'" | Write-Host
    New-Service -Name $serviceName -BinaryPathName $exePath -StartupType $startupType -DisplayName $serviceDisplayName -Description $serviceDescription | Write-Host
}

Function InstallDotNetCoreService([string]$serviceName, [string]$dllPath, [string]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
{
    $exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
    InstallService $serviceName $exePath $startupType $serviceDisplayName $serviceDescription
}

Function InstallTopshelfService([string]$serviceName, [string]$exePath)
{
    RemoveService $serviceName
    "Installing $exePath using topshelf" | Write-Host
    & "$exePath install"
}

Function InstallTopshelfServiceWithInstance([string]$serviceName, [string]$exePath, [string]$instance)
{
    RemoveService $serviceName
    "Installing $exePath with instance $instance using topshelf" | Write-Host
    & "$exePath install /instance:$instance"
}

Write-Host "Ensconce - ServiceManagement Loaded"
$serviceManagementLoaded = $true
