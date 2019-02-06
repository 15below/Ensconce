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
	$passwordSecure = ConvertTo-SecureString -String $servicePassword -AsPlainText -Force
	$credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $serviceUser, $passwordSecure
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription -Credential $credential | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function InstallAutomaticStartService([string]$serviceName, [string]$exePath, [string]$serviceDisplayName, [string]$serviceDescription)
{
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function InstallAutomaticStartDotNetCoreServiceWithCredential([string]$serviceName, [string]$dllPath, [string]$serviceDisplayName, [string]$serviceDescription, [string]$serviceUser, [string]$servicePassword)
{
	$exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
	$passwordSecure = ConvertTo-SecureString -String $servicePassword -AsPlainText -Force
	$credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $serviceUser, $passwordSecure
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription -Credential $credential | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Function InstallAutomaticStartDotNetCoreService([string]$serviceName, [string]$dllPath, [string]$serviceDisplayName, [string]$serviceDescription)
{
	$exePath = "C:\Program Files\dotnet\dotnet.exe $dllPath"
	New-Service -Name $serviceName -BinaryPathName $exePath -StartupType Automatic -DisplayName $serviceDisplayName -Description $serviceDescription | Write-Host
	& "sc.exe" failure $serviceName reset= 30 actions= restart/5000 | Write-Host
}

Write-Host "Ensconce - ServiceManagement Loaded"
