Write-Host "Ensconce - UserManagement Loading"
# ADS_USER_FLAG_ENUM Enumeration
# http://msdn.microsoft.com/en-us/library/aa772300(VS.85).aspx
$ADS_UF_SCRIPT                                 = 1        # 0x1
$ADS_UF_ACCOUNTDISABLE                         = 2        # 0x2
$ADS_UF_HOMEDIR_REQUIRED                       = 8        # 0x8
$ADS_UF_LOCKOUT                                = 16       # 0x10
$ADS_UF_PASSWD_NOTREQD                         = 32       # 0x20
$ADS_UF_PASSWD_CANT_CHANGE                     = 64       # 0x40
$ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED        = 128      # 0x80
$ADS_UF_TEMP_DUPLICATE_ACCOUNT                 = 256      # 0x100
$ADS_UF_NORMAL_ACCOUNT                         = 512      # 0x200
$ADS_UF_INTERDOMAIN_TRUST_ACCOUNT              = 2048     # 0x800
$ADS_UF_WORKSTATION_TRUST_ACCOUNT              = 4096     # 0x1000
$ADS_UF_SERVER_TRUST_ACCOUNT                   = 8192     # 0x2000
$ADS_UF_DONT_EXPIRE_PASSWD                     = 65536    # 0x10000
$ADS_UF_MNS_LOGON_ACCOUNT                      = 131072   # 0x20000
$ADS_UF_SMARTCARD_REQUIRED                     = 262144   # 0x40000
$ADS_UF_TRUSTED_FOR_DELEGATION                 = 524288   # 0x80000
$ADS_UF_NOT_DELEGATED                          = 1048576  # 0x100000
$ADS_UF_USE_DES_KEY_ONLY                       = 2097152  # 0x200000
$ADS_UF_DONT_REQUIRE_PREAUTH                   = 4194304  # 0x400000
$ADS_UF_PASSWORD_EXPIRED                       = 8388608  # 0x800000
$ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 16777216 # 0x1000000

Function AddUser([string]$name, [string]$password)
{
    $computer = [ADSI]"WinNT://$env:computername,computer"
    $newuser = $computer.Create("user", $name)
    $newuser.SetPassword($password)
    $newuser.SetInfo()

    $newuser.UserFlags = $ADS_UF_DONT_EXPIRE_PASSWD
    $newuser.CommitChanges()
}

Function SetUserPassword([string]$name, [string]$password)
{
    $user = [ADSI]"WinNT://$env:computername/$name,user"
    $user.SetPassword($password)
    $user.UserFlags = $ADS_UF_DONT_EXPIRE_PASSWD
    $user.CommitChanges()
}

Function AddUserToGroup([string]$name, [string]$group)
{
    "AddUserToGroup: $name, $group" | Write-Host

    $objComputer = [ADSI]("WinNT://$env:computername,computer")

    $colUsers = $objComputer.psbase.children |
        Where-Object {$_.psBase.schemaClassName -eq "User" -and $_.psBase.Name -eq $name}

    if($colUsers -eq $null)
    {
        "Could not locate user: $user" | Write-Host
    }
    else
    {
        $searchgroup = ($objComputer.psbase.children |
            Where {$_.psbase.schemaClassName -eq "group" -and $_.psbase.Name -eq $group} |
            Select-Object -First 1)

        if($searchgroup -eq $null)
        {
            "Could not locate group: $group" | Write-Host
        }
        else
        {
            $blnFound = $False
            $members = @($searchgroup.psbase.Invoke("Members"))
            ForEach ($Member In $Members)
            {
                $class = $member.GetType().InvokeMember("Class", 'GetProperty', $Null, $member, $Null)
                $username = $member.GetType().InvokeMember("Name", 'GetProperty', $Null, $member, $Null)
                if ($class -eq "User" -and $username -eq $name)
                {
                    $blnFound = $True
                    "User '$name' already in group '$group'" | Write-Host
                }
            }

            if ($blnFound -eq $False) {
                try {
                    net localgroup $group $name /add
                }
                catch [Exception] {
                    "Error caught" | Write-Host
                }
            }
        }
    }
}

Function CheckAndCreateServiceAccount([string]$name, [string]$password)
{
    $objComputer = [ADSI]("WinNT://$env:computername,computer")

    $colUsers = ($objComputer.psbase.children |
        Where-Object {$_.psBase.schemaClassName -eq "User"} |
            Select-Object -expand Name)

    $blnFound = $colUsers -contains $name

    if ($blnFound -eq $False) {
        AddUser $name $password
    } else {
    	SetUserPassword $name $password
    }

    AddUserToGroup $name "administrators"

    $exe = "$EnsconceDir\Tools\Grant\Grant.exe"
    $osInfo = Get-WmiObject -Class Win32_OperatingSystem
    if($osInfo.ProductType -eq 2)
    {
        $userName = "$env:UserDomain\$name"
    }
    else
    {
        $userName = "$env:computername\$name"
    }
    &$exe ADD SeServiceLogonRight $userName
}

Function CheckAndCreateUserAccount([string]$name, [string]$password)
{
    $objComputer = [ADSI]("WinNT://$env:computername,computer")

    $colUsers = ($objComputer.psbase.children |
        Where-Object {$_.psBase.schemaClassName -eq "User"} |
            Select-Object -expand Name)

    $blnFound = $colUsers -contains $name

    if ($blnFound -eq $False) {
        AddUser $name $password
    } else {
    	SetUserPassword $name $password
    }

    AddUserToGroup $name "Users"
}

Function SetServiceAccount([string]$serviceName, [string]$account, [string]$password)
{
    $svc = gwmi win32_service -filter ("Name=""$serviceName""")

    if ($svc -eq $null)
    {
        "Could not locate service $serviceName" | Write-Host
    }
    else
    {
        CheckAndCreateServiceAccount $account $password

        $inParams = $svc.psbase.getMethodParameters("Change")
        $inParams["StartName"] = ".\$account"
        $inParams["StartPassword"] = $password
        $svc.invokeMethod("Change", $inParams, $null)

        "Service $serviceName set to use account $account" | Write-Host
    }
}

Write-Host "Ensconce - UserManagement Loaded"
$userManagementLoaded = $true
