$scriptDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if(Test-Path $DeployPath)
{
    Write-Host "Removing Deploy Tools Directory Found at $DeployPath"
    Remove-Item $DeployPath -Force -Recurse | Out-Null
    Start-Sleep -s 2
}

Write-Host "Creating Deploy Tools Directory at $DeployPath"
New-Item -Path $DeployPath -Type container -Force | Out-Null

if($IncludeK8s -eq "True")
{
	Write-Host "Ensconce deployed in Kubernetes mode as 'IncludeK8s' set to 'True'"
	Remove-Item "$scriptDir\Content\backupHelper.ps1" -Force
	Remove-Item "$scriptDir\Content\createWebSite.ps1" -Force
	Remove-Item "$scriptDir\Content\dnsHelper.ps1" -Force
	Remove-Item "$scriptDir\Content\serviceManagement.ps1" -Force
}
else
{
	Write-Host "Ensconce deployed in application deployment mode as 'IncludeK8s' NOT set to 'True'"
	Remove-Item "$scriptDir\Content\Tools\KubeCtl" -Force -Recurse
	Remove-Item "$scriptDir\Content\kubernetesHelper.ps1" -Force
}

Get-ChildItem -Path $scriptDir\Content\*.ps1 | ForEach-Object {
	$scriptName = $_.Name
	$scriptFullName = $_.FullName
	Write-Host "Deploying script $scriptName to $DeployPath"
	Copy-Item -Path $scriptFullName -Destination $DeployPath -Force
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

Write-Host "Testing Ensconce Works For Text"

. $DeployPath\deployHelp.ps1

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
