Write-Host "Ensconce - BackupHelper Loading"

Function CleanBackups([string]$backupDir, [int]$daysToKeep)
{
	$backupDate = Get-Date -Format yyyy-MM-dd_HH-mm-ss
	$backupPurgedate = (get-date).AddDays($daysToKeep * -1)

	if (@(Get-ChildItem $backupDir -Recurse | Where-Object { (-Not ($_.PSIsContainer)) }).Count -gt 0) {
		Write-Host "Remove backups in $backupDir older than $daysToKeep days"
		Get-ChildItem  -Path $backupDir -Recurse -Force | Where-Object { $_.CreationTime -lt $backupPurgedate -and (-Not ($_.PSIsContainer)) } | Remove-Item -Force
	}
}

Write-Host "Ensconce - BackupHelper Loaded"
