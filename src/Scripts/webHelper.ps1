if($compressionHelperLoaded -eq $null)
{
	$currentPath = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
	. $currentPath\compressionHelper.ps1
}

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
	$basePath = Get-Location
	$file = "$basePath\Temp.zip"
		
	CreateZip $sourcePath $file

	UploadFileAndGetStringResponse $url $file
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

Function DownloadStringUntilOK([string]$url, [int] $maxChecks, [int] $sleepSeconds, [String[]] $okText, [String[]] $failText)
{
	$responseText = ""
	$checkedTimes = 0
	$webClient = New-Object System.Net.WebClient
	$webClient.UseDefaultCredentials = $true

	while($checkedTimes -lt $maxChecks)
	{
		$checkedTimes++
		Write-Host "Downloading $url on attempt $checkedTimes"
		$responseText = $webClient.DownloadString($url)

		if($failText.Contains($responseText))
		{
			throw "Got '$responseText' from API on attempt $checkedTimes, this indicates a failure"
		}
		elseif($okText.Contains($runStatus))
		{
			Write-Host "Got '$responseText' from API on attempt $checkedTimes, this indicates a success"
			break
		}
		else
		{
			Write-Host "Got '$responseText' from API on attempt $checkedTimes, this is unknown - sleeping for $sleepSeconds seconds"
			Start-Sleep -s $sleepSeconds
		}
	}

	if($checkedTimes -ge $maxChecks)
	{
		throw "API text checking timed out after the maxiumum $maxChecks checks"
	}
}

Write-Host "Ensconce - WebHelper Loaded"
$webHelperLoaded = $true
