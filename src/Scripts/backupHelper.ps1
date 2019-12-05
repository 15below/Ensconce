if($deployHelpLoaded -eq $null)
{
	$currentPath = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
	. $currentPath\deployHelp.ps1
}

Write-Host "Ensconce - BackupHelper Loading"

Function CleanBackups([string]$backupDir, [int]$daysToKeep)
{
	if($daysToKeep > 0)
	{
		$backupPurgedate = (get-date).AddDays($daysToKeep * -1)
	}
	else
	{
		$backupPurgedate = (get-date).AddDays($daysToKeep)
	}

	if (@(Get-ChildItem $backupDir -Recurse | Where-Object { (-Not ($_.PSIsContainer)) }).Count -gt 0) {
		Write-Host "Remove backups in $backupDir older than $daysToKeep days"
		Get-ChildItem  -Path $backupDir -Recurse -Force | Where-Object { $_.CreationTime -lt $backupPurgedate -and (-Not ($_.PSIsContainer)) } | Remove-Item -Force
	}
}

Function CreateDatedBackup([string]$destFolder, [string]$baseName, [string[]]$sources)
{
	$backupDate = Get-Date -Format yyyy-MM-dd_HH-mm-ss

	foreach($source in $sources)
	{
		$args += "--backupSource"
		$args += $source
	}

	$args += "--backupDestination"
	$args += "$destFolder\$baseName-$backupDate.zip"
	ensconceWithArgs $args
}

Function Create7DayRollingDatedBackup([string]$destFolder, [string]$baseName, [string[]]$sources)
{
	CleanBackupsWithDate $destFolder 7
	CreateDatedBackup $destFolder $baseName $sources
}

Write-Host "Ensconce - BackupHelper Loaded"
$backupHelperLoaded = $true
