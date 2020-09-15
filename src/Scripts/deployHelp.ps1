Write-Host "Ensconce - DeployHelp Loading"
$DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

$ensconceVersion = "Unknown"
if((Test-Path $DeployToolsDir\releaseVersion.txt) -eq $true)
{
	$ensconceVersion = Get-Content -Path $DeployToolsDir\releaseVersion.txt -TotalCount 1
}
Write-Host "Ensconce - Ensconce Version: $ensconceVersion"

$psVersion = (Get-Host).Version
Write-Host "Ensconce - Powershell Version: $psVersion"

Write-Host "Ensconce - Setting SecurityProtocol to TLS 1.1 or TLS 1.2"
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12;

if (Test-Path variable:\OctopusParameters)
{
	foreach($kp in $OctopusParameters.GetEnumerator())
	{
		if (!($kp.Key.Contains("Octopus.Step[")) -and !($kp.Key.Contains("Octopus.Action[")) -and !($kp.Key.Contains("Octopus.Only")) -and !($kp.Key.StartsWith("env:")))
		{
			Set-Content ("env:\" + $kp.Key.replace("[","-").replace("]","")) ($kp.Value) -Force
		}
	}
}

if(!(Test-Path "env:\ConfigOnly"))
{
	Set-Content "env:\ConfigOnly" $false
}

function ensconceWithArgs($passedArgs)
{
	if (@($input).Count -ne 0)
	{
		$input.Reset()
		$results = $input | & "$DeployToolsDir\Tools\Ensconce\Ensconce.Console.exe" $passedArgs
	}
	else
	{
		$results = & "$DeployToolsDir\Tools\Ensconce\Ensconce.Console.exe" $passedArgs | Write-Host
	}

	if ($LASTEXITCODE -ne 0)
	{
		Write-Error "Ensconce failure"
		$results
		exit $LASTEXITCODE
	}
	else
	{
		$results
	}
}

function ensconce
{
	if (@($input).Count -ne 0)
	{
		$input.Reset()
		$input | ensconceWithArgs $args
	}
	else
	{
		ensconceWithArgs $args
	}
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
  return ( (Get-WmiObject Win32_OperatingSystem).OSArchitecture -eq "64-bit")
}

function CreateDesktopShortcut([string]$exePath, [string]$shortcutName, [string]$iconPath = "", [string]$arguments = "")
{
	$WshShell = New-Object -comObject WScript.Shell
	$Shortcut = $WshShell.CreateShortcut((Join-Path $wshShell.SpecialFolders.Item("AllUsersDesktop") "$shortcutName.lnk"))
	$Shortcut.TargetPath = $exePath
	if($iconPath -ne "")
	{
		$Shortcut.IconLocation = $iconPath
	}
	if($arguments -ne "")
	{
		$Shortcut.Arguments = $arguments
	}
	$Shortcut.Save()
}

# https://stackoverflow.com/questions/45470999/powershell-try-catch-and-retry
function Retry-Command {
	[CmdletBinding()]
	Param(
		[Parameter(Position=0, Mandatory=$true)]
		[scriptblock]$ScriptBlock,

		[Parameter(Position=1, Mandatory=$false)]
		[int]$Maximum = 5,

		[Parameter(Position=2, Mandatory=$false)]
		[int]$Delay = 100
	)

	Begin {
		$count = 0
	}

	Process {
		do {
			$count++
			try {
				$ScriptBlock.Invoke()
				return
			}
			catch {
				Write-Error $_.Exception.Message -ErrorAction Continue
				Write-Host "Retrying in $Delay ms..."
				Start-Sleep -Milliseconds $Delay
			}
		} while ($count -lt $Maximum)

		# Throw an error after $Maximum unsuccessful invocations. Doesn't need
		# a condition, since the function returns upon successful invocation.
		throw 'Execution failed.'
	}
}

Write-Host "Ensconce - DeployHelp Loaded"
$deployHelpLoaded = $true
