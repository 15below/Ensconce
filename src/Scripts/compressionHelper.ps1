Write-Host "Ensconce - CompressionHelper Loading"

Function CreateZip([string]$sourcePath, [string]$destinationFile)
{
	Write-Host "Compressing '$sourcePath' into '$destinationFile'"
	Compress-Archive -Path $sourcePath -DestinationPath $destinationFile
}

Function Get7zip() {
  if ((Test-Path 'C:\Program Files\7-zip\7z.exe') -eq $true) {
    return ('C:\"Program Files"\7-zip\7z.exe')
  }
  elseif ((Test-Path 'C:\Program Files (x86)\7-zip\7z.exe') -eq $true) {
    return ('C:\"Program Files (x86)"\7-zip\7z.exe')
  }
}

Function Extract7z([string]$sourceFile, [string]$destinationFolder)
{
	$7Zip = Get7zip
	if ($7Zip -eq $null) {
		throw 'Could not find 7-zip location'
	}

	Write-Host "Extracting '$sourceFile' into '$destinationFolder'"

	Invoke-Expression ("$7Zip x $sourceFile -y -o$destinationFolder")
}

Write-Host "Ensconce - CompressionHelper Loaded"
