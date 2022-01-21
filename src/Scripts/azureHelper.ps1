if($deployHelpLoaded -eq $null)
{
	$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
    . $DeployToolsDir\deployHelp.ps1
}

if($compressionHelperLoaded -eq $null)
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

function Azure-Logout()
{
    Write-Host "Checking if logged into account already"
    & az account show 2>&1 | Out-Null
    if($LASTEXITCODE -eq 0)
    {
        Write-Host "Logging out"
        & az logout

        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "Error logging out"
            exit $LASTEXITCODE
        }
    }
    else
    {
        Write-Host "Not logged in"
    }
}

function Azure-LoginServicePrincipal([string]$username, [string]$password, [string]$tenant)
{
    Azure-Logout
    Write-Host "Logging in as $username with tenant $tenant"
    & az login --service-principal --username $username --password $password --tenant $tenant

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error logging in as $username"
        exit $LASTEXITCODE
    }
}

function Azure-DeployWebApp([string]$resourceGroup, [string]$name, [string]$zipPath)
{
    & az webapp deployment source config-zip --resource-group $resourceGroup --name $name --src $zipPath
}

function Azure-DeployZipToWebApp([string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [string]$contentFolder)
{
    Azure-LoginServicePrincipal $username $password $tenant

    if(Test-Path "$contentFolder.zip")
    {
        Remove-Item "$contentFolder.zip" -Force
    }

    CreateZip $contentFolder "$contentFolder.zip"

    Azure-DeployWebApp $resourceGroup $name "$content.zip"

    Azure-Logout
}


Write-Host "Ensconce - AzureHelper Loaded"
