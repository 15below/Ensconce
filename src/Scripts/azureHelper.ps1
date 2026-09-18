if ($deployHelpLoaded -eq $null)
{
	$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
    . $DeployToolsDir\deployHelp.ps1
}

if ($compressionHelperLoaded -eq $null)
{
    . $DeployToolsDir\compressionHelper.ps1
}

Write-Host "Ensconce - AzureHelper Loading"
if (Get-Command "az" -ErrorAction SilentlyContinue)
{
    $raw = (az version 2>&1) -join ""
    $data = ConvertFrom-Json $raw
    $cliVersion = $data.'azure-cli'
    write-host "Azure CLI Version: $cliVersion"
}
else
{
    throw "azure CLI not installed"
}

$rootProfilePath = "$Home\.azure-ensconce-profiles"

function Azure-EnsureProfileActive([string]$username, [string]$tenant)
{
    $azureProfileId = "$username`_$tenant"
    $azureProfilePath = [IO.Path]::Combine($rootProfilePath, $azureProfileId)
    if ($env:AZURE_CONFIG_DIR -ne $azureProfilePath)
    {
        $env:AZURE_CONFIG_DIR = $azureProfilePath
        Write-Host "Profile config set to $env:AZURE_CONFIG_DIR"
    }
}

function Azure-CheckLoggedIn
{
    $originalErrorPref = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'

    & az account show --only-show-errors 2>&1 | Out-Null

    $ErrorActionPreference = $originalErrorPref

    return $LASTEXITCODE -eq 0    
}

function Azure-LoginServicePrincipal([string]$username, [string]$password, [string]$tenant)
{
    Azure-EnsureProfileActive $username $tenant
    if ($env:AZURE_CONFIG_DIR -eq $null -or $env:AZURE_CONFIG_DIR -eq "")
    {
        Write-Error "AZURE_CONFIG_DIR environment variable is empty, will not login"
        exit -1
    }

    $loggedIn = Azure-CheckLoggedIn

    if ($loggedIn)
    {
        Write-Host "Already Logged In"
    }
    else
    {
        Write-Host "Logging in as $username with tenant $tenant"
        & az login --service-principal --username $username --password $password --tenant $tenant --only-show-errors

        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "Error logging in as $username"
            exit $LASTEXITCODE
        }
    }
}

function Azure-DeployZipToWebApp([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [string]$slot, [string]$zipPath, [string]$subscription)
{
    Azure-LoginServicePrincipal $username $password $tenant
    
    if ($slot -eq $null -or $slot -eq "")
    {
        Write-Host "Deploying $name in resource group $resourceGroup into production slot"

        & az webapp deployment source config-zip --resource-group $resourceGroup --name $name --src $zipPath --only-show-errors --subscription $subscription
    }
    else
    {
        Write-Host "Deploying $name in resource group $resourceGroup into slot $slot"

        & az webapp deployment source config-zip --resource-group $resourceGroup --name $name --src $zipPath --slot $slot --only-show-errors --subscription $subscription
    }    

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error deploying in as $username"
        exit $LASTEXITCODE
    }
}

function Azure-WebAppSlotSwapStagingToProduction([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [string]$subscription)
{
    Azure-LoginServicePrincipal $username $password $tenant
    
    Write-Host "Swapping slot staging to production for $name in resource group $resourceGroup"
    
    & az webapp deployment slot swap --resource-group $resourceGroup --name $name --slot "staging" --target-slot "production" --only-show-errors --subscription $subscription
    
    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error swapping slot $stagingSlot to $productionSlot"
        exit $LASTEXITCODE
    }
}

function Azure-DeployWebApp([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [bool]$useStagingSlot, [string]$contentFolder, [string]$subscription)
{
    if (Test-Path "$contentFolder.zip")
    {
        Remove-Item "$contentFolder.zip" -Force
    }
    
    CreateZip $contentFolder "$contentFolder.zip"

    if ($useStagingSlot -eq $true)
    {
        Azure-DeployZipToWebApp $username $password $tenant $resourceGroup $name "staging" "$contentFolder.zip" $subscription

        Azure-WebAppSlotSwapStagingToProduction $username $password $tenant $resourceGroup $name $subscription
    }
    else
    {
        Azure-DeployZipToWebApp $username $password $tenant $resourceGroup $name $null "$contentFolder.zip" $subscription
    }
}

function Azure-GetDnsRecord([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$zoneName, [string]$recordName, [string]$subscription)
{
    Azure-LoginServicePrincipal $username $password $tenant

    $recordsJson = & az network dns record-set list --resource-group $resourceGroup --zone-name $zoneName --subscription $subscription --only-show-errors --output json
    if ($LASTEXITCODE -ne 0)
    {
        throw "Error looking up DNS records in zone $zoneName"
    }

    $records = @($recordsJson | ConvertFrom-Json | Where-Object {
        $_.name -eq $recordName -and $_.type -in @("Microsoft.Network/dnszones/A", "Microsoft.Network/dnszones/CNAME")
    })

    $records
}

function Azure-CreateOrUpdateDnsRecord([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$zoneName, [string]$recordName, [ValidateSet("A", "CNAME")][string]$recordType, [string]$value, [string]$subscription, [int]$ttl = 3600, [bool]$warnOnUpdate = $false)
{
    if ($ttl -lt 1)
    {
        throw "TTL must be greater than zero"
    }

    if ($recordType -eq "A")
    {
        $parsedAddress = $null
        if (-not [System.Net.IPAddress]::TryParse($value, [ref]$parsedAddress) -or $parsedAddress.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork)
        {
            throw "An A record requires an IPv4 address"
        }
    }
    elseif ([string]::IsNullOrWhiteSpace($value))
    {
        throw "A CNAME record requires a target"
    }

    Azure-LoginServicePrincipal $username $password $tenant

    $recordsJson = & az network dns record-set list --resource-group $resourceGroup --zone-name $zoneName --subscription $subscription --only-show-errors --output json
    if ($LASTEXITCODE -ne 0)
    {
        throw "Error looking up DNS records in zone $zoneName"
    }

    $records = @($recordsJson | ConvertFrom-Json | Where-Object {
        $_.name -eq $recordName -and $_.type -in @("Microsoft.Network/dnszones/A", "Microsoft.Network/dnszones/CNAME")
    })
    $desiredType = "Microsoft.Network/dnszones/$recordType"
    $existingRecord = $records | Select-Object -First 1

    if ($null -ne $existingRecord -and $existingRecord.type -ne $desiredType)
    {
        & az network dns record-set delete --resource-group $resourceGroup --zone-name $zoneName --name $recordName --type ($existingRecord.type -split "/")[-1] --subscription $subscription --yes --only-show-errors
        if ($LASTEXITCODE -ne 0)
        {
            throw "Error replacing DNS record $recordName"
        }
        $existingRecord = $null
    }

    if ($recordType -eq "A")
    {
        if ($null -eq $existingRecord)
        {
            & az network dns record-set a create --resource-group $resourceGroup --zone-name $zoneName --name $recordName --ttl $ttl --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error creating DNS A record $recordName"
            }
        }
        elseif ($existingRecord.ttl -ne $ttl)
        {
            & az network dns record-set a update --resource-group $resourceGroup --zone-name $zoneName --record-set-name $recordName --set ttl=$ttl --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error updating DNS A record $recordName TTL"
            }
        }

        $currentAddresses = @($existingRecord | Select-Object -ExpandProperty aRecords -ErrorAction SilentlyContinue | Select-Object -ExpandProperty ipv4Address)
        $addressesToRemove = @($currentAddresses | Where-Object { $_ -ne $value })
        foreach ($currentAddress in $addressesToRemove)
        {
            & az network dns record-set a remove-record --resource-group $resourceGroup --zone-name $zoneName --record-set-name $recordName --ipv4-address $currentAddress --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error updating DNS A record $recordName"
            }
        }

        if ($currentAddresses -notcontains $value)
        {
            & az network dns record-set a add-record --resource-group $resourceGroup --zone-name $zoneName --record-set-name $recordName --ipv4-address $value --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error updating DNS A record $recordName"
            }
            if ($warnOnUpdate)
            {
                Write-Warning "DNS A record $recordName.$zoneName updated to $value"
            }
        }
    }
    else
    {
        if ($null -eq $existingRecord)
        {
            & az network dns record-set cname create --resource-group $resourceGroup --zone-name $zoneName --name $recordName --ttl $ttl --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error creating DNS CNAME record $recordName"
            }
        }
        elseif ($existingRecord.ttl -ne $ttl)
        {
            & az network dns record-set cname update --resource-group $resourceGroup --zone-name $zoneName --record-set-name $recordName --set ttl=$ttl --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error updating DNS CNAME record $recordName TTL"
            }
        }

        $currentTarget = $existingRecord | Select-Object -ExpandProperty cnameRecord -ErrorAction SilentlyContinue | Select-Object -ExpandProperty cname
        if ($null -ne $currentTarget)
        {
            $currentTarget = $currentTarget.TrimEnd(".")
        }
        $target = $value.TrimEnd(".")
        if ($currentTarget -ne $target)
        {
            & az network dns record-set cname set-record --resource-group $resourceGroup --zone-name $zoneName --record-set-name $recordName --cname $target --subscription $subscription --only-show-errors
            if ($LASTEXITCODE -ne 0)
            {
                throw "Error updating DNS CNAME record $recordName"
            }
            if ($warnOnUpdate)
            {
                Write-Warning "DNS CNAME record $recordName.$zoneName updated to $value"
            }
        }
    }

    $true
}

function Azure-CreateOrUpdateDnsARecord([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$zoneName, [string]$recordName, [string]$ipAddress, [string]$subscription, [int]$ttl = 3600, [bool]$warnOnUpdate = $false)
{
    Azure-CreateOrUpdateDnsRecord $username $password $tenant $resourceGroup $zoneName $recordName "A" $ipAddress $subscription $ttl $warnOnUpdate
}

function Azure-CreateOrUpdateDnsCNameRecord([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$zoneName, [string]$recordName, [string]$target, [string]$subscription, [int]$ttl = 3600, [bool]$warnOnUpdate = $false)
{
    Azure-CreateOrUpdateDnsRecord $username $password $tenant $resourceGroup $zoneName $recordName "CNAME" $target $subscription $ttl $warnOnUpdate
}

Write-Host "Ensconce - AzureHelper Loaded"
