Write-Host "Ensconce - dnsHelper Loading"

function GetDnsRecords ([string]$dnsServer, [string]$domain, [string]$lookupName)
{
    $result = dnscmd $dnsServer /EnumRecords $domain $lookupName

    $keys = New-Object Collections.Generic.List[pscustomobject]
    foreach ($item in $result)
    {
        $match = [regex]::Match($item, "^(?<key>\S*)\s+(?<ttl>\d+)\s+(?<type>\S+)\s+(?<value>.+)$")
        if ($match.Success)
        {
            $key = $match.Groups["key"].Value
            $ttl = $match.Groups["ttl"].Value
            $type = $match.Groups["type"].Value.ToUpper()
            $value = $match.Groups["value"].Value.ToLower()

            if ($key -eq "@")
            {
                $record = "$lookupName".ToLower()
            }
            else
            {
                $record = "$key.$lookupName".ToLower()
            }

            if ($value.EndsWith("."))
            {
                $value = $value.Substring(0,$value.Length-1)
            }

            $keys.Add([pscustomobject]@{Record=$record;Ttl=$ttl;Type=$type;Value=$value})
        }
    }

    $keys
}

function GetAllSubValues ([string]$dnsServer, [string]$domain, [string]$lookupName)
{
    GetDnsRecords $dnsServer $domain $lookupName | Select -ExpandProperty Record
}

function CheckName ([string]$dnsServer, [string]$domain, [string]$lookupName)
{
    $dnsRecords = GetDnsRecords $dnsServer $domain $lookupName
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $lookupName.ToLower()} | measure).Count -ge 1

    if ($checkResult -eq $false)
    {
        Write-Host "$dnsServer : CheckName for $lookupName.$domain is false records found:"
        $dnsRecords  | Format-Table | Out-String |% { Write-Host $_.Trim() }
    }

    $checkResult
}

function CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
    $dnsRecords = GetDnsRecords $dnsServer $domain $name
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $name.ToLower() -and $_.Type -eq "CNAME" -and $_.Value -eq $server.ToLower()} | measure).Count -ge 1

    if ($checkResult -eq $false)
    {
        Write-Host "$dnsServer : CheckCNameValue for $lookupName.$domain value $server is false records found:"
        $dnsRecords  | Format-Table | Out-String |% { Write-Host $_.Trim() }
    }

    $checkResult
}

function DeleteCName ([string]$dnsServer, [string]$domain, [string]$name)
{
    write-host "$dnsServer : Deleting DNS CNAME records for $name.$domain"
    $result = dnscmd $dnsServer /recordDelete $domain $name CNAME /f
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if ($outcome -eq $false)
    {
        write-host "$dnsServer : $result"
    }

    $outcome
}

function CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [string]$ttl="3600")
{
    write-host "$dnsServer : Creating DNS CNAME record for $name.$domain pointing at $server with TTL $ttl"
    $result = dnscmd $dnsServer /recordAdd $domain $name $ttl CNAME $server
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if ($outcome -eq $false)
    {
        write-host "$dnsServer : $result"
    }

    $outcome
}

function UpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
    $currentRecords = (GetDnsRecords $dnsServer $domain $name | Where-Object {$_.Record -eq $name.ToLower()})
    $currentTtl = ($currentRecords | Select-Object -ExpandProperty Ttl -First 1)
    $currentHasA =  ($currentRecords | Where-Object {$_.Type -eq "A"} | measure).Count -ge 1
    $currentHasCNAME =  ($currentRecords | Where-Object {$_.Type -eq "CNAME"} | measure).Count -ge 1
    if ($currentHasCNAME -eq $true)
    {
        DeleteCName $dnsServer $domain $name
    }
    if ($currentHasA -eq $true)
    {
        DeleteARecord $dnsServer $domain $name
    }
    CreateCName $dnsServer $domain $name $server $currentTtl
}

function CreateOrUpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [bool]$warnOnUpdate = $false)
{
    $outcome = $false
    if (CheckName $dnsServer $domain $name)
    {
        if (CheckCNameValue $dnsServer $domain $name $server)
        {
            write-host "$dnsServer : DNS CNAME record for $name.$domain already pointing at $server"
            $outcome = $true
        }
        else
        {
            if (UpdateCName $dnsServer $domain $name $server)
            {
                $outcome = $true
                if ($warnOnUpdate)
                {
                    write-warning "$dnsServer : DNS CNAME record for $name.$domain updated to point at $server"
                }
                else
                {
                    write-host "$dnsServer : DNS CNAME record for $name.$domain updated to point at $server"
                }
            }
            else
            {
                write-error "$dnsServer : Failed to update DNS CNAME record for $name.$domain"
            }
        }
    }
    else
    {
        if (CreateCName $dnsServer $domain $name $server)
        {
            $outcome = $true
            write-host "$dnsServer : DNS CNAME record for $name.$domain created pointing at $server"
        }
        else
        {
            write-error "$dnsServer : Failed to create DNS CNAME record for $name.$domain"
        }
    }

    $outcome
}

function CheckARecordValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
    $dnsRecords = GetDnsRecords $dnsServer $domain $name
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $name.ToLower() -and $_.Type -eq "A" -and $_.Value -eq $ipAddress} | measure).Count -ge 1

    if ($checkResult -eq $false)
    {
        Write-Host "$dnsServer : CheckARecordValue for $lookupName.$domain value $ipAddress is false records found:"
        $dnsRecords  | Format-Table | Out-String |% { Write-Host $_.Trim() }
    }

    $checkResult
}

function DeleteARecord ([string]$dnsServer, [string]$domain, [string]$name)
{
    write-host "$dnsServer : Deleting DNS A records for $name.$domain"
    $result = dnscmd $dnsServer /recordDelete $domain $name A /f
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if ($outcome -eq $false)
    {
        write-host "$dnsServer : $result"
    }

    $outcome
}

function CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [string]$ttl="3600")
{
    write-host "$dnsServer : Creating DNS A record for $name.$domain pointing at $ipAddress with TTL $ttl"
    $result = dnscmd $dnsServer /recordAdd $domain $name $ttl A $ipAddress
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if ($outcome -eq $false)
    {
        write-host "$dnsServer : $result"
    }

    $outcome
}

function UpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
    $currentRecords = (GetDnsRecords $dnsServer $domain $name | Where-Object {$_.Record -eq $name.ToLower()})
    $currentTtl = ($currentRecords | Select-Object -ExpandProperty Ttl -First 1)
    $currentHasA =  ($currentRecords | Where-Object {$_.Type -eq "A"} | measure).Count -ge 1
    $currentHasCNAME =  ($currentRecords | Where-Object {$_.Type -eq "CNAME"} | measure).Count -ge 1
    if ($currentHasCNAME -eq $true)
    {
        DeleteCName $dnsServer $domain $name
    }
    if ($currentHasA -eq $true)
    {
        DeleteARecord $dnsServer $domain $name
    }
    CreateARecord $dnsServer $domain $name $ipAddress $currentTtl
}

function CreateOrUpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [bool]$warnOnUpdate = $false)
{
    $outcome = $false
    if (CheckName $dnsServer $domain $name)
    {
        if (CheckARecordValue $dnsServer $domain $name $ipAddress)
        {
            write-host "$dnsServer : DNS A record for $name.$domain already pointing at $ipAddress"
            $outcome = $true
        }
        else
        {
            if (UpdateARecord $dnsServer $domain $name $ipAddress)
            {
                $outcome = $true
                if ($warnOnUpdate)
                {
                    write-warning "$dnsServer : DNS A record for $name.$domain updated to point at $ipAddress"
                }
                else
                {
                    write-host "$dnsServer : DNS A record for $name.$domain updated to point at $ipAddress"
                }
            }
            else
            {
                write-error "$dnsServer : Failed to update DNS A record for $name.$domain"
            }
        }
    }
    else
    {
        if (CreateARecord $dnsServer $domain $name $ipAddress)
        {
            $outcome = $true
            write-host "$dnsServer : DNS A record for $name.$domain created pointing at $ipAddress"
        }
        else
        {
            write-error "$dnsServer : Failed to create DNS A record for $name.$domain"
        }
    }
    $outcome
}

function CreateOrUpdateDns ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddressOrServer, [bool]$warnOnUpdate = $false)
{
    $isIp = $ipAddressOrServer -match "^\d+\.\d+\.\d+\.\d+$";
    if ($isIp)
    {
        CreateOrUpdateARecord $dnsServer $domain $name $ipAddressOrServer $warnOnUpdate
    }
    else
    {
        CreateOrUpdateCNAME $dnsServer $domain $name $ipAddressOrServer $warnOnUpdate
    }
}

function DeleteDns([string]$dnsServer, [string]$domain, [string]$name, [bool]$warnOnUpdate = $false)
{
    $outcome = $false
    if (CheckName $dnsServer $domain $name)
    {
        $cnameResult = DeleteCName $dnsServer $domain $name
        $aResult = DeleteARecord $dnsServer $domain $name
        if ($cnameResult -or $aResult)
        {
            $outcome = $true
            if ($warnOnUpdate)
            {
                write-warning "$dnsServer : DNS A record for $name.$domain has been removed"
            }
            else
            {
                write-host "$dnsServer : DNS A record for $name.$domain has been removed"
            }
        }
        else
        {
            write-error "$dnsServer : Failed to remove DNS A record for $name.$domain"
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

Write-Host "Ensconce - dnsHelper Loaded"
$dnsHelperLoaded = $true
