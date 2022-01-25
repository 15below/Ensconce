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

    & az account show 2>&1 | Out-Null

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
        & az login --service-principal --username $username --password $password --tenant $tenant

        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "Error logging in as $username"
            exit $LASTEXITCODE
        }
    }
}

function Azure-DeployZipToWebApp([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [string]$slot, [string]$zipPath)
{
    Azure-LoginServicePrincipal $username $password $tenant
    
    if ($slot -eq $null -or $slot -eq "")
    {
        Write-Host "Deploying $name in resource group $resourceGroup into production slot"

        & az webapp deployment source config-zip --resource-group $resourceGroup --name $name --src $zipPath
    }
    else
    {
        Write-Host "Deploying $name in resource group $resourceGroup into slot $slot"

        & az webapp deployment source config-zip --resource-group $resourceGroup --name $name --src $zipPath --slot $slot
    }    

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error deploying in as $username"
        exit $LASTEXITCODE
    }
}

function Azure-WebAppSlotSwapStagingToProduction([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name)
{
    Azure-LoginServicePrincipal $username $password $tenant
    
    Write-Host "Swapping slot staging to production for $name in resource group $resourceGroup"
    
    & az webapp deployment slot swap --resource-group $resourceGroup --name $name --slot "staging" --target-slot "production"
    
    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error swapping slot $stagingSlot to $productionSlot"
        exit $LASTEXITCODE
    }
}

function Azure-DeployWebApp([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [bool]$useStagingSlot, [string]$contentFolder)
{
    if (Test-Path "$contentFolder.zip")
    {
        Remove-Item "$contentFolder.zip" -Force
    }
    
    CreateZip $contentFolder "$contentFolder.zip"

    if ($useStagingSlot -eq $true)
    {
        Azure-DeployZipToWebApp $username $password $tenant $resourceGroup $name "staging" "$contentFolder.zip"

        Azure-WebAppSlotSwapStagingToProduction $username $password $tenant $resourceGroup $name
    }
    else
    {
        Azure-DeployZipToWebApp $username $password $tenant $resourceGroup $name $null "$contentFolder.zip"
    }
}

Write-Host "Ensconce - AzureHelper Loaded"
