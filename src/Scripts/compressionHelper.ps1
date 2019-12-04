Write-Host "Ensconce - CompressionHelper Loading"

Function CreateZip([string]$sourcePath, [string]$destinationFile)
{
	Write-Host "Compressing '$sourcePath' into '$destinationFile'"
	Compress-Archive -Path $sourcePath -DestinationPath $destinationFile
}

Write-Host "Ensconce - CompressionHelper Loaded"
