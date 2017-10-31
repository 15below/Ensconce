Write-Host "Ensconce - dnsHelper Loading"
Write-Host "Ensconce - dnsHelper Loaded"

function CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
{                                                                               
	$result = dnscmd $dnsServer /EnumRecords $domain $lookupName
	$outcome = $False
	foreach ($item in $result)
	{
		if ($item.Contains("3600 CNAME") -or ($item.Contains("3600 A")))
		{
			$outcome = $True
		}
	}
	$outcome	
}

function CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{                                                                               
	$result = dnscmd $dnsServer /EnumRecords $domain $name
	$outcome = $False
	foreach ($item in $result)
	{
		if ($item.Contains("3600 CNAME") -and ($item.ToLower().Contains($server.ToLower())))
		{
			$outcome = $True
		}
	}
	$outcome
}

function CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{                                                                               
	$result = dnscmd $dnsServer /EnumRecords $domain $name
	$outcome = $False
	foreach ($item in $result)
	{
		if ($item.Contains("3600 A") -and ($item.Contains($ipAddress)))
		{
			$outcome = $True
		}
	}
	$outcome
}

function CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
	write-host "Creating DNS CNAME record for $name.$domain pointing at $server"
	$result = dnscmd $dnsServer /recordAdd $domain $name CNAME $server 
	$outcome = $false
	foreach ($item in $result)
	{
		if ($item.Contains("3600 CNAME") -And $item.Contains("Command completed successfully"))
		{
			$outcome = $true
		}
	}
	$outcome
}

function CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
	write-host "Creating DNS A record for $name.$domain pointing at $ipAddress"
	$result = dnscmd $dnsServer /recordAdd $domain $name A $ipAddress 
	$outcome = $false
	foreach ($item in $result)
	{
		if ($item.Contains("3600 A") -And $item.Contains("Command completed successfully"))
		{
			$outcome = $true
		}
	}
	$outcome
}

function CheckHostsEntry ([string]$Address, [string]$FullyQualifiedName)
{
	write-host "Checking hosts file for $Address pointing at $FullyQualifiedName"
	$checkEntry = "^\s*$Address\s+$FullyQualifiedName\s*$"

	$matches = (Get-Content "$env:windir\System32\drivers\etc\hosts") -match $checkEntry 
	if ($matches.Count -gt 0)
	{
		$true
	}
	else
	{
		$false
	}		
}

function AddHostsEntry ([string]$Address, [string]$FullyQualifiedName)
{
	write-host "Creating hosts file entry for $Address pointing at $FullyQualifiedName"
	$newEntry = "`n$Address`t$FullyQualifiedName"
	
	Add-Content "$env:windir\System32\drivers\etc\hosts" -Value $newEntry
	
	CheckHostsEntry $Address $FullyQualifiedName
}