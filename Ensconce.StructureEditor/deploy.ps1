$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

$DeployDir = "$ToolsDir\Ensconce Structure Editor"

Write-Host "Deploying Editor To $DeployDir"

if(!(Test-Path $DeployDir))
{
    New-Item $DeployDir -Type container
}

Copy-Item "$scriptDir\..\Content\*.*" $DeployDir -Force