$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

Write-Host "Deploying Tools to $DeployDir"

if(!(Test-Path $DeployDir))
{
    New-Item $DeployDir -Type container
}

Copy-Item "$scriptDir\..\Content\*.*" $DeployDir -Force