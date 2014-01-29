$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

Write-Host "Deploying Tools to $DeployToolsDir"

if(!(Test-Path $DeployToolsDir))
{
    New-Item $DeployToolsDir -Type container
}

Copy-Item "$scriptDir\..\Content\*" $DeployToolsDir -Force