Write-Host "Ensconce - RabbitHelper Loading"

function CreateRabbitVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$vHost)
{
    $deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
    $deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)

    if (Invoke-RestMethod -Uri "$rabbitApiUrl/vhosts" -Credential $deployCreds -DisableKeepAlive | where {$_.name -eq $vHost})
    {
        Write-Host "The virtual host $vHost already exists."
    }
    else
    {
        Write-Host "Creating virtual host $vHost."
        Invoke-RestMethod -Uri "$rabbitApiUrl/vhosts/$vHost" -Credential $deployCreds -DisableKeepAlive -Method Put -ContentType "application/json; charset=utf-8"
    }
}

function CreateRabbitUser([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$password)
{
    $deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
    $deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)

    if (Invoke-RestMethod -Uri "$rabbitApiUrl/users" -Credential $deployCreds -DisableKeepAlive | where {$_.name -eq $user})
    {
        Write-Host "Updating user $user."
        $updatedUser = "{""password"":""$password"",""tags"":""management""}"
        $utf = [System.Text.Encoding]::Utf8.GetBytes($updatedUser)
        Invoke-RestMethod -Uri "$rabbitApiUrl/users/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $utf -ContentType "application/json; charset=utf-8"
    }
    else
    {
        Write-Host "Creating user $user."
        $newUser = "{""password"":""$password"",""tags"":""management""}"
        $utf = [System.Text.Encoding]::Utf8.GetBytes($newUser)
        Invoke-RestMethod -Uri "$rabbitApiUrl/users/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $utf -ContentType "application/json; charset=utf-8"
    }
}

function AddUserToVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$vHost)
{
    $deployPasswordSecure = ConvertTo-SecureString $deployPassword -AsPlainText -Force
    $deployCreds = New-Object System.Management.Automation.PSCredential ($deployUser, $deployPasswordSecure)

    if (Invoke-RestMethod -Uri "$rabbitApiUrl/users" -Credential $deployCreds -DisableKeepAlive | where {$_.name -eq $user}) {
        if (Invoke-RestMethod -Uri "$rabbitApiUrl/vhosts" -Credential $deployCreds -DisableKeepAlive | where {$_.name -eq $vHost}) {
            Write-Host "Giving $user access to $vHost."
            $newPermission = "{""configure"":"".*"",""write"":"".*"",""read"":"".*""}"
            Invoke-RestMethod -Uri "$rabbitApiUrl/permissions/$vHost/$user" -Credential $deployCreds -DisableKeepAlive -Method Put -Body $newPermission -ContentType "application/json; charset=utf-8"
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

function CheckUserAccess([string]$rabbitApiUrl, [string]$user, [string]$password, [string]$vHost)
{
    $passwordSecure = ConvertTo-SecureString $password -AsPlainText -Force
    $creds = New-Object System.Management.Automation.PSCredential ($user, $passwordSecure)

    try {
        Write-Host "Checking user $user access to $vHost."
        $queues = Invoke-RestMethod -uri "$rabbitApiUrl/queues/$vHost" -Credential $creds -DisableKeepAlive
    }
    catch [Exception] {
        return $false
    }

    return $true
}

function ValidateUserAccess([string]$rabbitApiUrl, [string]$user, [string]$password, [string]$vHost)
{
    $passwordSecure = ConvertTo-SecureString $password -AsPlainText -Force
    $creds = New-Object System.Management.Automation.PSCredential ($user, $passwordSecure)

    try {
        Write-Host "Validating user $user has been set up correctly."
        $queues = Invoke-RestMethod -uri "$rabbitApiUrl/queues/$vHost" -Credential $creds -DisableKeepAlive
    }
    catch [Exception] {
        Write-Error $_.Exception
        Exit 125
    }
}

function CreateRabbitUserAndVHost([string]$deployUser, [string]$deployPassword, [string]$rabbitApiUrl, [string]$user, [string]$password, [string]$vHost)
{
    $UserAccess = CheckUserAccess $rabbitApiUrl $user $password $vHost

    if($UserAccess) {
        Write-Host "$user has access to $vHost already"
        return
    }      

    CreateRabbitVHost $deployUser $deployPassword $rabbitApiUrl $vHost

    CreateRabbitUser $deployUser $deployPassword $rabbitApiUrl $user $password

    AddUserToVHost $deployUser $deployPassword $rabbitApiUrl $user $vHost

    ValidateUserAccess $rabbitApiUrl $user $password $vHost
}

Write-Host "Ensconce - RabbitHelper Loaded"
