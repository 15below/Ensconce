Write-Host "Ensconce - WebHelper Loading"

Function UploadFileAndGetStringResponse([string]$url, [string]$file)
{
	Write-Host "Uploading file '$file' to $url"
	$responseBytes = $webClient.UploadFile($url, $file)
	$response = [System.Text.Encoding]::ASCII.GetString($responseBytes)
	Write-Host "Response: $response"
	$response
}

Function UploadFolderAsZipAndGetStringResponse([string]$url, [string]$sourcePath)
{
	$file = "Temp.zip"

	Write-Host "Compressing '$sourcePath' into '$file'"
	Compress-Archive -Path $sourcePath -DestinationPath $file

	UploadFileAndGetStringResponse($url, $file)
}

Function UploadValuesAndGetStringResponse([string]$url, [System.Collections.Specialized.NameValueCollection]$values)
{
	Write-Host "Uploading values to $url"
	Write-Host "Values:"
	foreach ($key in $values) {
		$value = $values[$key]
		Write-Host "$key : $value"
	}
	$responseBytes = $webClient.UploadValues($url, $values)
	$response = [System.Text.Encoding]::ASCII.GetString($responseBytes)
	Write-Host "Response: $response"
	$response
}

Write-Host "Ensconce - WebHelper Loaded"
