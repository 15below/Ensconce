$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if(Test-Path $DeployToolsDir)
{
    Write-Host "Removing Deploy Tools Directory Found at $DeployToolsDir"
    Remove-Item $DeployToolsDir
}

Write-Host "Creating Deploy Tools Directory at $DeployToolsDir"
New-Item $DeployToolsDir -Type container

Write-Host "Deploying Tools to $DeployToolsDir"
Copy-Item "$scriptDir\Content\*" $DeployToolsDir -Force -Recurse