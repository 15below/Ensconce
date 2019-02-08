Write-Host "Ensconce - ServiceManagement Loading"

Function StopService([string]$serviceName)
{
	If (Get-Service $serviceName -ErrorAction SilentlyContinue) {
		If ((Get-Service $serviceName).Status -eq 'Running') {
			"Stopping $serviceName" | Write-Host
			Stop-Service $serviceName | Write-Host
		} Else {
		   "$serviceName found, but it is not running." | Write-Host
		}
	} Else {
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
	$passwordSecure = ConvertTo-SecureString -String $servicePassword -AsPlainText -Force
	$credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $serviceUser, $passwordSecure
	"Setting service $serviceName to run as $serviceUser"
	Set-Service -Name $serviceName -Credential $credential | Write-Host
}

Function SetServiceRestarts([string]$serviceName){
	"Setting service restarts for $serviceName"
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function RemoveService([string]$serviceName)
{
	If (Get-Service $serviceName -ErrorAction SilentlyContinue) {
		StopService $serviceName
		"Removing $serviceName" | Write-Host
		& "sc.exe" delete $serviceName
	}
}

Function InstallService([string]$serviceName, [string]$exePath, [ServiceStartupType]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
{
	RemoveService $serviceName
	"Installing $serviceName with exe '$exePath'" | Write-Host
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType $startupType -DisplayName $serviceDisplayName -Description $serviceDescription | Write-Host
	SetServiceRestarts $serviceName
}

Function InstallServiceWithCredential([string]$serviceName, [string]$exePath, [ServiceStartupType]$startupType, [string]$serviceDisplayName, [string]$serviceDescription, [string]$serviceUser, [string]$servicePassword)
{
	InstallService $serviceName $exePath $startupType $serviceDisplayName $serviceDescription
	SetServiceRunAs $serviceName $serviceUser $servicePassword
}

Function InstallDotNetCoreService([string]$serviceName, [string]$dllPath, [ServiceStartupType]$startupType, [string]$serviceDisplayName, [string]$serviceDescription)
{
	$exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
	InstallService $serviceName $exePath $startupType $serviceDisplayName $serviceDescription
}

Function InstallDotNetCoreServiceWithCredential([string]$serviceName, [string]$dllPath, [ServiceStartupType]$startupType, [string]$serviceDisplayName, [string]$serviceDescription, [string]$serviceUser, [string]$servicePassword)
{
	InstallDotNetCoreService $serviceName $dllPath $startupType $serviceDisplayName $serviceDescription
	SetServiceRunAs $serviceName $serviceUser $servicePassword
}

Function InstallTopshelfService([string]$serviceName, [string]$exePath)
{
	RemoveService $serviceName
	"Installing $exePath using topshelf" | Write-Host
	& "$exePath install"
	SetServiceRestarts $serviceName
}

Function InstallTopshelfServiceWithCredential([string]$serviceName, [string]$exePath, [string]$serviceUser, [string]$servicePassword)
{
	InstallTopshelfService $serviceName $exePath
	SetServiceRunAs $serviceName $serviceUser $servicePassword
}

Function InstallTopshelfServiceWithInstance([string]$serviceName, [string]$exePath, [string]$instance)
{
	RemoveService $serviceName
	"Installing $exePath with instance $instance using topshelf" | Write-Host
	& "$exePath install /instance:$instance"
	SetServiceRestarts $serviceName
}

Function InstallTopshelfServiceWithInstanceAndCredential([string]$serviceName, [string]$exePath, [string]$instance, [string]$serviceUser, [string]$servicePassword)
{
	InstallTopshelfServiceWithInstance $serviceName $exePath $instance
	SetServiceRunAs $serviceName $serviceUser $servicePassword
}

Write-Host "Ensconce - ServiceManagement Loaded"
