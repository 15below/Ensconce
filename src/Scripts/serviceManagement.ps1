Write-Host "Ensconce - ServiceManagement Loading"

$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
. $DeployToolsDir\userManagement.ps1

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

Function RemoveService([string]$serviceName)
{
	If (Get-Service $serviceName -ErrorAction SilentlyContinue) {
		StopService $serviceName
		"Removing $serviceName" | Write-Host
		& "sc.exe" delete $serviceName
	}
}

Function InstallAutomaticStartServiceWithCredential([string]$serviceName, [string]$exePath, [string]$serviceDisplayName, [string]$serviceDescription, [string]$serviceUser, [string]$servicePassword)
{
	RemoveService $serviceName
	"Installing $serviceName with exe '$exePath' to run as $serviceUser" | Write-Host
	$passwordSecure = ConvertTo-SecureString -String $servicePassword -AsPlainText -Force
	$credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $serviceUser, $passwordSecure
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription -Credential $credential | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function InstallAutomaticStartService([string]$serviceName, [string]$exePath, [string]$serviceDisplayName, [string]$serviceDescription)
{
	RemoveService $serviceName
	"Installing $serviceName with exe '$exePath'" | Write-Host
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function InstallAutomaticStartDotNetCoreServiceWithCredential([string]$serviceName, [string]$dllPath, [string]$serviceDisplayName, [string]$serviceDescription, [string]$serviceUser, [string]$servicePassword)
{
	$exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
	InstallAutomaticStartServiceWithCredential $serviceName $exePath $serviceDisplayName $serviceDescription $serviceUser $servicePassword
}

Function InstallAutomaticStartDotNetCoreService([string]$serviceName, [string]$dllPath, [string]$serviceDisplayName, [string]$serviceDescription)
{
	$exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
	InstallAutomaticStartService $serviceName $exePath $serviceDisplayName $serviceDescription
}

Function InstallTopshelfService([string]$exePath)
{
	"Installing $exePath using topshelf" | Write-Host
	& "$exePath install"
}

Function InstallTopshelfServiceWithInstance([string]$exePath, [string]$instance)
{
	"Installing $exePath with instance $instance using topshelf" | Write-Host
	& "$exePath install /instance:$instance"
}

Function InstallTopshelfServiceWithCredential([string]$serviceName, [string]$exePath, [string]$serviceUser, [string]$servicePassword)
{
	"Installing $exePath to run as $serviceUser using topshelf" | Write-Host
	& "$exePath install"
	SetServiceAccount $serviceName $serviceUser $servicePassword
}

Function InstallTopshelfServiceWithInstanceAndCredential([string]$serviceName, [string]$exePath, [string]$instance, [string]$serviceUser, [string]$servicePassword)
{
	"Installing $exePath with instance $instance to run as $serviceUser using topshelf" | Write-Host
	& "$exePath install /instance:$instance"
	SetServiceAccount $serviceName $serviceUser $servicePassword
}

Write-Host "Ensconce - ServiceManagement Loaded"
