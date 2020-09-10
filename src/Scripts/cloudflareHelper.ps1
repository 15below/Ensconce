Write-Host "Ensconce - cloudflare Loading"

function CallCloudflare([string]$token, [string]$urlPart, [Microsoft.PowerShell.Commands.WebRequestMethod]$method, [string]$body = $null) 
{
    $baseurl = "https://api.cloudflare.com/client/v4"

    $headers = @{
	    "Content-Type" = "application/json"
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
    $zoneurl = "zones/?name=$domain"
    $zone = CallCloudflare $token $zoneurl Get

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
    $recordurl = "zones/$zoneid/dns_records/?name=$record.$domain"
    $dnsRecord = CallCloudflare $token $recordurl Get

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
    $recordurl = "zones/$zoneid/dns_records/?name=$record.$domain"
    $dnsRecord = CallCloudflare $token $recordurl Get

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

function CreateCloudflareARecord([string]$token, [string]$zoneid, [string]$domain, [string]$record, [string]$ipaddr)
{
    $newDnsRecord = @{
        "type" = "A"
        "name" =  "$record.$domain"
        "content" = $ipaddr
    }

    $body = $newDnsRecord | ConvertTo-Json

    $newrecordurl = "zones/$zoneid/dns_records"
    $result = CallCloudflare $token $newrecordurl Post $body

    Write-Host "New record $record.$domain has been created with the ID $($result.result.id)"
}

function UpdateCloudflareARecord([string]$token, [string]$zoneid, [string]$recordid, [string]$domain, [string]$record, [string]$ipaddr, [bool]$warnOnUpdate = $false)
{
    $dnsRecord | Add-Member "content" $ipaddr -Force 
    $body = $dnsRecord | ConvertTo-Json 

    $updateurl = "zones/$zoneid/dns_records/$recordid/" 
    $result = CallCloudflare $token $updateurl Put $body
    if($warnOnUpdate)
    {
        Write-Warning "Record $record.$domain has been updated to the IP $($result.result.content)"
    }
    else
    {
        Write-Host "Record $record.$domain has been updated to the IP $($result.result.content)"
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
        if($dnsRecord.content -eq $ipaddr)
        {
            Write-Host "Record $record.$domain already has the value $ipaddr" 
        }
        else
        {
            UpdateCloudflareARecord $token $zoneid $recordid $domain $record $ipaddr $warnOnUpdate
        }        
    } 
    else 
    {
        CreateCloudflareARecord $token $zoneid $domain $record $ipaddr
    }
}

Write-Host "Ensconce - cloudflare Loaded"
$cloudflareHelperLoaded = $true
