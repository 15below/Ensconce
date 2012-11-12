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
    if (@($input).Count -ne 0)
    {
        $input.Reset()
        $results = $input | & "$deployToolsDir\ensconce.exe" $args 2>&1
    }
    else {
        $results = & "$deployToolsDir\ensconce.exe" $args 2>&1
    }
    if ($LASTEXITCODE -ne 0)
    {
        if (Test-Path env:\TEAMCITY_VERSION)
        {
            Write-Host "##teamcity[buildStatus status='FAILURE' text='{build.status.text}; Ensconce failure']"
            $results
            exit $LASTEXITCODE
        }
        else {
        throw (
"Ensconce operation failed.
$results")
        }
    }
    $results
}

function EnsurePath([string]$name)
{
	$path = $name | ensconce -i
	
	if ((Test-Path $path) -eq $False)
	{
		md $path
	}
}

function is64bit() {
  return ([IntPtr]::Size -eq 8)
}