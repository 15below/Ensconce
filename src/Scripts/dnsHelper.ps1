Write-Host "Ensconce - dnsHelper Loading"

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
	$result = dnscmd $dnsServer /EnumRecords $domain $name /node
    
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

function DeleteCName ([string]$dnsServer, [string]$domain, [string]$name)
{
	write-host "Deleting DNS CNAME records for $name.$domain"
	$result = dnscmd $dnsServer /recordDelete $domain $name CNAME /f
	$outcome = $false
	foreach ($item in $result)
	{
		if ($item.Contains("Command completed successfully"))
		{
			$outcome = $true
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
		if ($item.Contains("Command completed successfully"))
		{
			$outcome = $true
		}
	}
	$outcome
}

function UpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
	DeleteCName $dnsServer $domain $name
	DeleteARecord $dnsServer $domain $name
	CreateCName $dnsServer $domain $name $server
}

function CreateOrUpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [bool]$warnOnUpdate = $false)
{
	$outcome = $false
	if(CheckName $dnsServer $domain $name)
	{
		if(CheckCNameValue $dnsServer $domain $name $server)
		{
			write-host "DNS CNAME record for $name.$domain already pointing at $server"
			$outcome = $true
		}
		else
		{
			if(UpdateCName $dnsServer $domain $name $server)
			{
				$outcome = $true
				if($warnOnUpdate)
				{
					write-warning "DNS CNAME record for $name.$domain updated to point at $ipAddress"
				}
				else
				{
					write-host "DNS CNAME record for $name.$domain updated to point at $server"
				}				
			}
			else
			{
				write-error "Failed to update DNS CNAME record for $name.$domain"
			}
		}
	}
	else
	{
		if(CreateCName $dnsServer $domain $name $server)
		{
			$outcome = $true
			write-host "DNS CNAME record for $name.$domain created pointing at $server"
        }
		else
		{
			write-error "Failed to create DNS CNAME record for $name.$domain"
		}
	}
	$outcome
}

function CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
	$result = dnscmd $dnsServer /EnumRecords $domain $name /node
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

function DeleteARecord ([string]$dnsServer, [string]$domain, [string]$name)
{
	write-host "Deleting DNS A records for $name.$domain"
	$result = dnscmd $dnsServer /recordDelete $domain $name A /f
	$outcome = $false
	foreach ($item in $result)
	{
		if ($item.Contains("Command completed successfully"))
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
		if ($item.Contains("Command completed successfully"))
		{
			$outcome = $true
		}
	}
	$outcome
}

function UpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
	DeleteCName $dnsServer $domain $name
	DeleteARecord $dnsServer $domain $name
	CreateARecord $dnsServer $domain $name $ipAddress
}

function CreateOrUpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [bool]$warnOnUpdate = $false)
{
	$outcome = $false
	if(CheckName $dnsServer $domain $name)
	{
		if(CheckARecordValue $dnsServer $domain $name $ipAddress)
		{
			write-host "DNS A record for $name.$domain already pointing at $ipAddress"
			$outcome = $true
		}
		else
		{
			if(UpdateARecord $dnsServer $domain $name $ipAddress)
			{
				$outcome = $true
				if($warnOnUpdate)
				{
					write-warning "DNS A record for $name.$domain updated to point at $ipAddress"
				}
				else
				{
					write-host "DNS A record for $name.$domain updated to point at $ipAddress"
				}				
			}
			else
			{
				write-error "Failed to update DNS A record for $name.$domain"
			}
		}
	}
	else
	{
		if(CreateARecord $dnsServer $domain $name $ipAddress)
		{
			$outcome = $true
			write-host "DNS A record for $name.$domain created pointing at $ipAddress"
		}
		else
		{
			write-error "Failed to create DNS A record for $name.$domain"
		}
	}
	$outcome
}

function CreateOrUpdateDns ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddressOrServer)
{
	$isIp = $ipAddressOrServer -match "^\d+\.\d+\.\d+\.\d+$";
	if($isIp)
	{
		CreateOrUpdateARecord $dnsServer $domain $name $ipAddressOrServer
	}
	else
	{
		CreateOrUpdateCNAME $dnsServer $domain $name $ipAddressOrServer
	}
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

Write-Host "Ensconce - dnsHelper Loaded"
$dnsHelperLoaded = $true
