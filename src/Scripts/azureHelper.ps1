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

function Azure-Logout([string]$currentDirectory)
{
    $azureProfilePath = [IO.Path]::Combine($currentDirectory, ".azure-profile")

    if($azureProfilePath -ne "")
    {
        Write-Host "Logging out"
        & az logout

        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "Error logging out"
            exit $LASTEXITCODE
        }

        Remove-Item -Recurse -Force $azureProfilePath
        $azureProfilePath = $null
        $env:AZURE_CONFIG_DIR = $null
    }
    else
    {
        Write-Host "Not logged in"
    }
}

function Azure-LoginServicePrincipal([string]$currentDirectory, [string]$username, [string]$password, [string]$tenant)
{
    if($azureProfilePath -ne "")
    {
        Azure-Logout $currentDirectory
    }

    $azureProfilePath = [IO.Path]::Combine($currentDirectory, ".azure-profile")
    $env:AZURE_CONFIG_DIR = $azureProfilePath
    Write-Host "Storing profile $env:AZURE_CONFIG_DIR"

    if($env:AZURE_CONFIG_DIR -eq $null -or $env:AZURE_CONFIG_DIR -eq "")
    {
        Write-Error "AZURE_CONFIG_DIR environment variable is empty, will not login"
        exit -1
    }

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

function Azure-DeployZipToWebApp([string]$currentDirectory, [string]$username, [string]$password, [string]$tenant, [string]$resourceGroup, [string]$name, [string]$contentFolder)
{
    Azure-LoginServicePrincipal $currentDirectory $username $password $tenant

    if(Test-Path "$contentFolder.zip")
    {
        Remove-Item "$contentFolder.zip" -Force
    }
    
    CreateZip $contentFolder "$contentFolder.zip"
    
    Azure-DeployWebApp $resourceGroup $name "$content.zip"

    Azure-Logout $currentDirectory
}


Write-Host "Ensconce - AzureHelper Loaded"
