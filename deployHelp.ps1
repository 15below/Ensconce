$deployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if (Test-Path variable:\OctopusParameters)
{
    foreach($kp in $OctopusParameters.GetEnumerator())
    {
        Set-Content ("env:\" + $kp.Key) ($kp.Value) -Force
    }
}

if(!(Test-Path "env:\ConfigOnly"))
{
    Set-Content "env:\ConfigOnly" $false
}

function ensconce
{
    $results = & "$deployToolsDir\ensconce.exe" $args 2>&1
    if ($LASTEXITCODE -ne 0)
    {
        throw (
"Ensconce operation failed.
$results")
    }
    $results
}
