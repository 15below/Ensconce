function CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
{                                                                               
  $result = dnscmd $dnsServer /EnumRecords $domain $lookupName
	$outcome = $False
	foreach ($item in $result)
	{
		if ($item.Contains("3600 CNAME"))
		{
			$outcome = $True
		}
	}
	$outcome
	
}

function CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
	write-host "Creating dns CNAME record for $name.$domain pointing at $server"
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

function CheckHostsEntry ([string]$Address, [string]$FullyQualifiedName)
{
	write-host "Checking hosts file for $Address pointing at $FullyQualifiedName"
	$checkEntry = "^\s*$Address\s+$FullyQualifiedName\s*$"

	$matches = (Get-Content "$env:windir\System32\drivers\etc\hosts") -match $checkEntry 
	If ($matches.Count -gt 0)
	{ $True}
	else
	{ $False}
		
}

function AddHostsEntry ([string]$Address, [string]$FullyQualifiedName)
{
	write-host "Creating hosts file entry for $Address pointing at $FullyQualifiedName"
	$newEntry = "`n$Address`t$FullyQualifiedName"
	
	Add-Content "$env:windir\System32\drivers\etc\hosts" -Value $newEntry
	
	CheckHostsEntry $Address $FullyQualifiedName
}