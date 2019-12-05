Write-Host "Ensconce - WebHelper Loading"

Function UploadFileAndGetStringResponse([string]$url, [string]$file)
{
	Write-Host "Uploading file '$file' to $url"
	$webClient = New-Object System.Net.WebClient
	$webClient.UseDefaultCredentials = $true
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

	$webClient = New-Object System.Net.WebClient
	$webClient.UseDefaultCredentials = $true
	$responseBytes = $webClient.UploadValues($url, $values)
	$response = [System.Text.Encoding]::ASCII.GetString($responseBytes)
	Write-Host "Response: $response"
	$response
}

Function DownloadFile([string]$url, [string]$destinationPath)
{
	Write-Host "Downloading $url to $destinationPath"
	$webClient = New-Object System.Net.WebClient
	$webClient.UseDefaultCredentials = $true
	$webClient.DownloadFile($url, $destinationPath)
}

Function DownloadString([string]$url)
{
	Write-Host "Downloading $url"
	$webClient = New-Object System.Net.WebClient
	$webClient.UseDefaultCredentials = $true
	$response = $webClient.DownloadString($url)
	Write-Host "Response: $response"
	$response
}

Write-Host "Ensconce - WebHelper Loaded"
$webHelperLoaded = $true
