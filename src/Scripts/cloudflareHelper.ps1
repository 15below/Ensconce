Write-Host "Ensconce - cloudflare helper Loading"

function CallCloudflare([string]$token, [string]$urlPart, [Microsoft.PowerShell.Commands.WebRequestMethod]$method, [string]$body = $null)
{
    $baseurl = "https://api.cloudflare.com/client/v4"

    $headers = @{
        "Authorization" = "Bearer $token"
    }

    $url = "$baseurl/$urlPart"

    if($body -eq $null -or $body -eq "")
    {
        Invoke-RestMethod -Uri $url -Method $method -Headers $headers
    }
    else
    {
        Invoke-RestMethod -Uri $url -Method $method -Headers $headers -Body $body
    }
}

function GetCloudflareDnsZone([string]$token, [string]$domain)
{
    $zone = CallCloudflare $token "zones/?name=$domain" Get

    if($zone.result.Count -gt 0){
        $zone.result
    }
    else
    {
        throw "Unable to locate zone $domain"
    }
}

function GetCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$domain, [string]$record)
{
    $dnsRecord = CallCloudflare $token "zones/$zoneid/dns_records/?name=$record.$domain" Get

    if($dnsRecord.result.Count -gt 0)
    {
        $dnsRecord.result
    }
    else
    {
        throw "Unable to locate record $record.$domain"
    }
}

function CheckCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$domain, [string]$record)
{
    $dnsRecord = CallCloudflare $token "zones/$zoneid/dns_records/?name=$record.$domain" Get

    if($dnsRecord.result.Count -gt 0)
    {
        $true
    }
    else
    {
        $false
    }
}

function GetCloudflareDnsIp([string]$token, [string]$domain, [string]$record)
{
    $zone = GetCloudflareDnsZone $token $domain

    $zoneid = $zone.id

    $dnsRecord = GetCloudflareDnsRecord $token $zoneid $domain $record

    $dnsRecord.content
}

function CreateCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$domain, [string]$record, [string]$content, [string]$type)
{
    $name = "$record.$domain"
    $newDnsRecord = @{
        "type" = $type
        "name" =  $name
        "content" = $content
    }

    $body = $newDnsRecord | ConvertTo-Json
	
	Write-Host "Create new DNS record '$name' in zone '$zoneid': $body"
	
    $result = CallCloudflare $token "zones/$zoneid/dns_records" Post $body

    if($result.success)
    {
        Write-Host "New record $name (type: $($result.result.type), content: $($result.result.content)) has been created with the ID $($result.result.id)"
        $true
    }
    else
    {
        Write-Error "Error creating record $name"
        $false
    }
}

function UpdateCloudflareDnsRecord([string]$token, [string]$zoneid, [string]$recordid, [string]$domain, [string]$record, [string]$type, [string]$content, [bool]$warnOnUpdate = $false)
{
    $name = "$record.$domain"
    $dnsRecord | Add-Member "type" $type -Force
    $dnsRecord | Add-Member "content" $content -Force
    $body = $dnsRecord | ConvertTo-Json
    	
	Write-Host "Update dns record '$recordid' / named '$name' new DNS record in zone '$zoneid': $body"
	
    $result = CallCloudflare $token "zones/$zoneid/dns_records/$recordid/" Put $body
	
    if($result.success)
    {
        if($warnOnUpdate)
        {
            Write-Warning "Record $name has been updated to type: $($result.result.type), content: $($result.result.content)"
        }
        else
        {
            Write-Host "Record $name has been updated to type: $($result.result.type), content: $($result.result.content)"
        }
        $true
    }
    else
    {
        Write-Error "Error updating record $name"
        $false
    }
}

function CreateOrUpdateCloudflareARecord([string]$token, [string]$domain, [string]$record, [string]$ipaddr, [bool]$warnOnUpdate = $false)
{
    $zone = GetCloudflareDnsZone $token $domain
    $zoneid = $zone.id

    $recordExists = CheckCloudflareDnsRecord $token $zoneid $domain $record

    if($recordExists -eq $true)
    {
        $dnsRecord = GetCloudflareDnsRecord $token $zoneid $domain $record
        $recordid = $dnsRecord.id
        if($dnsRecord.content -eq $ipaddr -and $dnsRecord.type -eq "A")
        {
            Write-Host "Record '$record.$domain' already has an 'A' record with the value '$ipaddr'"
            $true
        }
        else
        {
            UpdateCloudflareDnsRecord $token $zoneid $recordid $domain $record "A" $ipaddr $warnOnUpdate
        }
    }
    else
    {
        CreateCloudflareDnsRecord $token $zoneid $domain $record $ipaddr "A"
    }
}

function CreateOrUpdateCloudflareCNAMERecord([string]$token, [string]$domain, [string]$record, [string]$cnameValue, [bool]$warnOnUpdate = $false)
{
    $zone = GetCloudflareDnsZone $token $domain
    $zoneid = $zone.id

    $recordExists = CheckCloudflareDnsRecord $token $zoneid $domain $record

    if($recordExists -eq $true)
    {
        $dnsRecord = GetCloudflareDnsRecord $token $zoneid $domain $record
        $recordid = $dnsRecord.id
        if($dnsRecord.content -eq $cnameValue -and $dnsRecord.type -eq "CNAME")
        {
            Write-Host "Record '$record.$domain' already has an 'CNAME' record with the value '$cnameValue'"
            $true
        }
        else
        {
            UpdateCloudflareDnsRecord $token $zoneid $recordid $domain $record "CNAME" $cnameValue $warnOnUpdate
        }
    }
    else
    {
        CreateCloudflareDnsRecord $token $zoneid $domain $record $cnameValue "CNAME"
    }
}

Write-Host "Ensconce - cloudflare helper Loaded"
$cloudflareHelperLoaded = $true
