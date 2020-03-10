Write-Host "Ensconce - CompressionHelper Loading"

Add-Type -Assembly System.IO.Compression.FileSystem
$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal

Function CreateZip([string]$sourcePath, [string]$destinationFile)
{
	if($sourcePath -match "\\\*\*$")
	{
        Write-Warning "Path $sourcePath ends with '\**' which is not needed, removing from path"
		$sourcePath = $sourcePath.Substring(0, $sourcePath.Length - 3)
	}
	Write-Host "Compressing '$sourcePath' into '$destinationFile'"
	[System.IO.Compression.ZipFile]::CreateFromDirectory($sourcePath, $destinationFile, $compressionLevel, $false)
}

Function ExtractZip([string]$sourceFile, [string]$destinationFolder)
{
	Write-Host "Extracting '$sourceFile' into '$destinationFolder'"
	[System.IO.Compression.ZipFile]::ExtractToDirectory($sourceFile, $destinationFolder, $compressionLevel, $false)
}

Function Get7zip() {
	if ((Test-Path 'C:\Program Files\7-zip\7z.exe') -eq $true) {
		return ('C:\"Program Files"\7-zip\7z.exe')
	}
	elseif ((Test-Path 'C:\Program Files (x86)\7-zip\7z.exe') -eq $true) {
		return ('C:\"Program Files (x86)"\7-zip\7z.exe')
	}
	else{
		throw 'Could not find 7-zip location'
	}
}

Function Create7z([string]$sourcePath, [string]$destinationFile)
{
	$7Zip = Get7zip
	Write-Host "Compressing '$sourcePath' into '$destinationFile'"
	Invoke-Expression ("$7Zip a $destinationFile $sourcePath")
}

Function Extract7z([string]$sourceFile, [string]$destinationFolder)
{
	$7Zip = Get7zip
	Write-Host "Extracting '$sourceFile' into '$destinationFolder'"
	Invoke-Expression ("$7Zip x $sourceFile -y -o$destinationFolder")
}

Write-Host "Ensconce - CompressionHelper Loaded"
$compressionHelperLoaded = $true
