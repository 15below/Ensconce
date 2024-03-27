Write-Host "Setting SecurityProtocol to TLS 1.1 or TLS 1.2"
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12;

$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

### Check Current Installed Version ###
if(Test-Path "$DeployPath\releaseVersion.txt")
{
	$currentVersion = Get-Content -Path "$DeployPath\releaseVersion.txt"

	if($currentVersion -eq $VersionNumber)
	{
		Write-Host "Version already installed on this machine"
		Exit
	}
}

### Download External Tools ###
if ([string]::IsNullOrWhiteSpace($ExternalToolDownloadUrl))
{
	Write-Warning "No External Tools Downloaded!`nYou Should Download`n* AZ CLI (and install it)`n* Kube-Linter`n* Grant`n* Handle`n* KubeCtl"
}
else
{
	$tools = @(
		[pscustomobject]@{Tool='Az-Installer';Version='2.54.0';ExeName='azure-cli.msi';RunExe=$true;RunArgs="/quiet /norestart";OctoAgent=$true}
		[pscustomobject]@{Tool='Kube-Linter';Version='0.6.7';ExeName='kube-linter.exe';RunExe=$false;RunArgs="";OctoAgent=$true}
		[pscustomobject]@{Tool='Grant';Version='1.01';ExeName='Grant.exe';RunExe=$false;RunArgs="";OctoAgent=$false}
		[pscustomobject]@{Tool='Handle';Version='5.0';ExeName='handle.exe';RunExe=$false;RunArgs="";OctoAgent=$false}
		[pscustomobject]@{Tool='Handle';Version='5.0';ExeName='handle64.exe';RunExe=$false;RunArgs="";OctoAgent=$false}
		[pscustomobject]@{Tool='KubeCtl';Version='1.27.12';ExeName='kubectl.exe';RunExe=$false;RunArgs="";OctoAgent=$true}
	)

	$tools | ForEach-Object {
		if($_.OctoAgent -eq $false -or $OctoAgent -eq $true)
		{
			$Tool = $_.Tool
			$ExeName = $_.ExeName
			$Version = $_.Version
			$DownloadUrl = "$ExternalToolDownloadUrl/$Tool/$Version/$ExeName"
			$DownloadPath = "$scriptDir\Content\Tools\$Tool\$ExeName"
			Write-Host "Downloading $DownloadUrl to $DownloadPath"
			New-Item -Path "$scriptDir\Content\Tools" -Name $Tool -Type container -Force | Out-Null
			Invoke-WebRequest -Uri $DownloadUrl -OutFile $DownloadPath | Write-Host

			if($_.RunExe)
			{
				Write-Host "Running $DownloadPath"
				Start-Process -FilePath "$scriptDir\Content\Tools\$Tool\$ExeName" -ArgumentList $_.RunArgs -Wait
				Remove-Item "$scriptDir\Content\Tools\$Tool" -Force -Recurse
			}
		}
	}
}

if(Test-Path $DeployPath)
{
    Write-Host "Removing Deploy Tools Directory Found at $DeployPath"
    Remove-Item $DeployPath -Force -Recurse | Out-Null
    Start-Sleep -s 2
}

Write-Host "Creating Deploy Tools Directory at $DeployPath"
New-Item -Path $DeployPath -Type container -Force | Out-Null

Get-ChildItem -Path $scriptDir\Content\*.ps1 | ForEach-Object {
	$scriptName = $_.Name
	$scriptFullName = $_.FullName
	$deployScript = $true
	if($OctoAgent -eq $false)
    {
        if($scriptName -eq "azureHelper.ps1" -or
           $scriptName -eq "kubernetesHelper.ps1")
        {
        	$deployScript = $false
        }
    }

	if($deployScript)
    {
        Write-Host "Deploying script $scriptName to $DeployPath"
        Copy-Item -Path $scriptFullName -Destination $DeployPath -Force
    }
    else
    {
    	Write-Host "Skipping script $scriptName as not applicable for machine"
    }
}

Get-ChildItem -Path $scriptDir\Content\Tools | ForEach-Object {
	$toolName = $_.Name
	$toolFullName = $_.FullName
	Write-Host "Deploying tool $toolName to $DeployPath\Tools"
	New-Item -Path $DeployPath\Tools -Name $toolName -Type container -Force | Out-Null
	Copy-Item -Path $toolFullName -Destination $DeployPath\Tools -Force -Recurse
}

Write-Host "Create releaseVersion.txt"
New-Item -Path $DeployPath -Name "releaseVersion.txt" -ItemType "file" -Value $VersionNumber | Out-Null

Write-Host "Loading All PowerShell Scripts"

Get-ChildItem -Path $DeployPath -Filter "*.ps1" | ForEach-Object {
    . $_.FullName
}

Write-Host "Testing Ensconce Works For Text"

$env:FixedPath = "$scriptDir\Test-Config.xml"

function DoEnsconceTest([string]$InputValue, [string]$ExpectedValue)
{
    $test = "$InputValue" | ensconce -i

    if($test -ne $ExpectedValue)
    {
        throw "'$InputValue' does not equate to '$ExpectedValue' but '$test'"
    }
    else
    {
        Write-Host "Checked '$InputValue' returned '$ExpectedValue' as expected"
    }
}

DoEnsconceTest -InputValue "{{ ClientCode }}" -ExpectedValue "ENSCONCE"
DoEnsconceTest -InputValue "{{ Environment }}" -ExpectedValue $OctopusEnvironmentName
DoEnsconceTest -InputValue "{{ SingleConfigItem }}" -ExpectedValue "ConfigItemValue"
DoEnsconceTest -InputValue "{{ SingleConfigItem|lower }}" -ExpectedValue "configitemvalue"
DoEnsconceTest -InputValue "{{ NonExistantValue|exists }}" -ExpectedValue "False"
DoEnsconceTest -InputValue "{{ NonExistantValue|empty }}" -ExpectedValue "True"
DoEnsconceTest -InputValue "{{ NonExistantValue|default:'default' }}" -ExpectedValue "default"
DoEnsconceTest -InputValue "{{ PropGroup.First.GroupItem }}" -ExpectedValue "Group1"
DoEnsconceTest -InputValue "{{ PropGroup.Second.GroupItem }}" -ExpectedValue "Group2"
DoEnsconceTest -InputValue "{% for instance in PropGroup %}{{ instance.identity }}+{{ instance.GroupItem }};{% endfor %}" -ExpectedValue "First+Group1;Second+Group2;"
DoEnsconceTest -InputValue "{{ DbLogins.DBKey.ConnectionString }}" -ExpectedValue "Data Source=Require-DB-Server; Initial Catalog=DB; User ID=DBName; Password=random-string-that-is-password;"
