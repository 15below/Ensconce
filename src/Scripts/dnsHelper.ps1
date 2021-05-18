Write-Host "Ensconce - dnsHelper Loading"

function GetDnsRecords ([string]$dnsServer, [string]$domain, [string]$lookupName)
{
    $result = dnscmd $dnsServer /EnumRecords $domain $lookupName

    $keys = New-Object Collections.Generic.List[pscustomobject]
    foreach ($item in $result)
    {
        $match = [regex]::Match($item, "^(?<key>\S*)\s+(?<ttl>\d+)\s+(?<type>\S+)\s+(?<value>.+)$")
        if($match.Success)
        {
            $key = $match.Groups["key"].Value
            $ttl = $match.Groups["ttl"].Value
            $type = $match.Groups["type"].Value.ToUpper()
            $value = $match.Groups["value"].Value.ToLower()

            if($key -eq "@")
            {
                $record = "$lookupName".ToLower()
            }
            else
            {
                $record = "$key.$lookupName".ToLower()
            }

            if($value.EndsWith("."))
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
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $lookupName.ToLower()} | measure).Count -eq 1

    if($checkResult -eq $false)
    {
        Write-Host "CheckName is false records found:"
        Write-Host ($dnsRecords | Out-String)
    }

    $checkResult
}

function CheckCNameValue ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
    $dnsRecords = GetDnsRecords $dnsServer $domain $name
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $name.ToLower() -and $_.Type -eq "CNAME" -and $_.Value -eq $server.ToLower()} | measure).Count -eq 1

    if($checkResult -eq $false)
    {
        Write-Host "CheckCNameValue is false records found:"
        Write-Host ($dnsRecords | Out-String)
    }

    $checkResult
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

    if($outcome -eq $false)
    {
        write-host "dnscmd: $result"
    }

    $outcome
}

function CreateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server, [string]$ttl="3600")
{
    write-host "Creating DNS CNAME record for $name.$domain pointing at $server with TTL $ttl"
    $result = dnscmd $dnsServer /recordAdd $domain $name $ttl CNAME $server
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if($outcome -eq $false)
    {
        write-host "dnscmd: $result"
    }

    $outcome
}

function UpdateCName ([string]$dnsServer, [string]$domain, [string]$name, [string]$server)
{
    $currentRecord = (GetDnsRecords $dnsServer $domain $name | Where-Object {$_.Record -eq $name.ToLower()})
    $currentTtl = ($currentRecord | Select-Object -ExpandProperty Ttl -First 1)
    $currentType = ($currentRecord | Select-Object -ExpandProperty Type -First 1)
    if($currentType -eq "CNAME")
    {
        DeleteCName $dnsServer $domain $name
    }
    if($currentType -eq "A")
    {
        DeleteARecord $dnsServer $domain $name
    }
    CreateCName $dnsServer $domain $name $server $currentTtl
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
                    write-warning "DNS CNAME record for $name.$domain updated to point at $server"
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
    $dnsRecords = GetDnsRecords $dnsServer $domain $name
    $checkResult = ($dnsRecords | Where-Object {$_.Record -eq $name.ToLower() -and $_.Type -eq "A" -and $_.Value -eq $ipAddress} | measure).Count -eq 1

    if($checkResult -eq $false)
    {
        Write-Host "CheckARecordValue is false records found:"
        Write-Host ($dnsRecords | Out-String)
    }

    $checkResult
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

    if($outcome -eq $false)
    {
        write-host "dnscmd: $result"
    }

    $outcome
}

function CreateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress, [string]$ttl="3600")
{
    write-host "Creating DNS A record for $name.$domain pointing at $ipAddress with TTL $ttl"
    $result = dnscmd $dnsServer /recordAdd $domain $name A $ipAddress
    $outcome = $false
    foreach ($item in $result)
    {
        if ($item.Contains("Command completed successfully"))
        {
            $outcome = $true
        }
    }

    if($outcome -eq $false)
    {
        write-host "dnscmd: $result"
    }

    $outcome
}

function UpdateARecord ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddress)
{
    $currentRecord = (GetDnsRecords $dnsServer $domain $name | Where-Object {$_.Record -eq $name.ToLower()})
    $currentTtl = ($currentRecord | Select-Object -ExpandProperty Ttl -First 1)
    $currentType = ($currentRecord | Select-Object -ExpandProperty Type -First 1)
    if($currentType -eq "CNAME")
    {
        DeleteCName $dnsServer $domain $name
    }
    if($currentType -eq "A")
    {
        DeleteARecord $dnsServer $domain $name
    }
    CreateARecord $dnsServer $domain $name $ipAddress $currentTtl
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

function CreateOrUpdateDns ([string]$dnsServer, [string]$domain, [string]$name, [string]$ipAddressOrServer, [bool]$warnOnUpdate = $false)
{
    $isIp = $ipAddressOrServer -match "^\d+\.\d+\.\d+\.\d+$";
    if($isIp)
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
    if(CheckName $dnsServer $domain $name)
    {
        $cnameResult = DeleteCName $dnsServer $domain $name
        $aResult = DeleteARecord $dnsServer $domain $name
        if($cnameResult -or $aResult)
        {
            $outcome = $true
            if($warnOnUpdate)
            {
                write-warning "DNS A record for $name.$domain has been removed"
            }
            else
            {
                write-host "DNS A record for $name.$domain has been removed"
            }
        }
        else
        {
            write-error "Failed to remove DNS A record for $name.$domain"
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
