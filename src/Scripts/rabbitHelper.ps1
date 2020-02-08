Write-Host "Ensconce - RabbitHelper Loading"

function CreateRabbitVHost([string]$deployUser, [string]$deployPassword, [string]$serverAddress, [string]$vHost)
{
	$deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
	$deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)
	$url = "http://${serverAddress}:15672/api"

	if (Invoke-RestMethod -Uri "$url/vhosts" -Credential $deployCreds | where {$_.name -eq $vHost}) 
	{
		Write-Host "The virtual host $vHost already exists."
	}
	else 
	{
		Write-Host "Creating virtual host $vHost."
		Invoke-RestMethod -Uri "$url/vhosts/$vHost" -Credential $deployCreds -DisableKeepAlive -Method Put -ContentType "application/json; charset=utf-8"
	}
}

function CreateRabbitUser([string]$deployUser, [string]$deployPassword, [string]$serverAddress, [string]$user, [string]$password)
{
	$deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
	$deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)
	$url = "http://${serverAddress}:15672/api"
	
	if (Invoke-RestMethod -Uri "$url/users" -Credential $deployCreds | where {$_.name -eq $user}) 
	{
		Write-Host "Updating user $user."
		$updatedUser = "{""password"":""$password"",""tags"":""management""}"
		$utf = [System.Text.Encoding]::Utf8.GetBytes($updatedUser)
		Invoke-RestMethod -Uri "$url/users/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $utf -ContentType "application/json; charset=utf-8"
	}
	else 
	{
		Write-Host "Creating user $user."
		$newUser = "{""password"":""$password"",""tags"":""management""}"
		$utf = [System.Text.Encoding]::Utf8.GetBytes($newUser)
		Invoke-RestMethod -Uri "$url/users/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $utf -ContentType "application/json; charset=utf-8"
	}
}

function AddUserToVHost([string]$deployUser, [string]$deployPassword, [string]$serverAddress, [string]$user, [string]$vHost)
{
	$deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
	$deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)
	$url = "http://${serverAddress}:15672/api"
	
	if (Invoke-RestMethod -Uri "$url/users" -Credential $deployCreds | where {$_.name -eq $user}) {
		if (Invoke-RestMethod -Uri "$url/vhosts" -Credential $deployCreds | where {$_.name -eq $vHost}) {				
			Write-Host "Giving $user access to $vHost."
			$newPermission = "{""configure"":"".*"",""write"":"".*"",""read"":"".*""}"
			Invoke-RestMethod -Uri "$url/permissions/$vHost/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $newPermission -ContentType "application/json; charset=utf-8"
		}
		else 
		{
			Write-Error "The virtual host $vHost does not exist."
			Exit 123
		}
	}
	else
	{
		Write-Error "The user $user does not exist."
		Exit 124
	}
}

function ValidateUserAccess([string]$serverAddress, [string]$user, [string]$password, [string]$vHost)
{
	$passwordSecure = ConvertTo-SecureString $password -AsPlainText -Force
	$creds = New-Object System.Management.Automation.PSCredential ($user, $passwordSecure)
	$url = "http://${serverAddress}:15672/api"
	
	try {
		Write-Host "Validating user $user has been set up correctly."
		$queues = Invoke-RestMethod -uri "$url/queues/$vHost" -Credential $creds
	}
	catch [Exception] {
		Write-Error $_.Exception 
		Exit 125
	}
}

function CreateRabbitUserAndVHost([string]$deployUser, [string]$deployPassword, [string]$serverAddress, [string]$user, [string]$password, [string]$vHost)
{
	CreateRabbitVHost $deployUser $deployPassword $serverAddress $vHost
	
	CreateRabbitUser $deployUser $deployPassword $serverAddress $user $password
	
	AddUserToVHost $deployUser $deployPassword $serverAddress $user $vHost
	
	ValidateUserAccess $serverAddress $user $password $vHost
}


Write-Host "Ensconce - RabbitHelper Loaded"
