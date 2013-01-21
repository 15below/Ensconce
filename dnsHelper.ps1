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
