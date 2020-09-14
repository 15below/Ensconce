Write-Host "Ensconce - scheduledTaskHelper Loading"

function ScheduledTask-CheckExists([string] $taskName, [string] $taskPath)
{
    $ErrorActionPreference = 'Stop'
    try 
    {
        Write-Host "Checking if scheduled task '\$taskPath\$taskName' exists"

        $response = & "schtasks" "/query" "/TN" "\$taskPath\$taskName"

        if($response -match ".*$taskName.*")
        {
            $true
        }
        else
        {
            $false
        }
    }
    catch
    {
        $false
    }
    finally
    {
        $ErrorActionPreference = 'Continue'
    }
}

function ScheduledTask-Delete([string] $taskName, [string] $taskPath)
{
    if((ScheduledTask-CheckExists $taskName $taskPath) -eq $true)
    {
        $ErrorActionPreference = 'Stop'
        try 
        {
            Write-Host "Deleting scheduled task '\$taskPath\$taskName'"

            $response = & "schtasks" "/delete" "/TN" "\$taskPath\$taskName" "/f"

            if($response -match ".*SUCCESS.*")
            {
                Write-Host "Scheduled task '\$taskPath\$taskName' deleted"
                $true
            }
            else
            {
                Write-Error "Error deleting scheduled task '$taskPath\$taskName' - $response"
                $false

            }
        }
        catch
        {
            Write-Error "Error deleting scheduled task '$taskPath\$taskName'"
            $false
        }
        finally
        {
            $ErrorActionPreference = 'Continue'
        }       
    }
    else
    {
        Write-Host "Scheduled task '\$taskPath\$taskName' does not exist"
        $true
    }
}

function ScheduledTask-CreateFromXml([string] $taskName, [string] $taskPath, [string] $taskXmlPath)
{
    $ErrorActionPreference = 'Continue'
    try 
    {
        Write-Host "Creating scheduled task '\$taskPath\$taskName'"
        $response = & "schtasks" "/create" "/XML" $taskXmlPath "/TN" "\$taskPath\$taskName" "/F"
        if($response -match ".*SUCCESS.*")
        {
            Write-Host "Scheduled task '\$taskPath\$taskName' created"
            $true
        }
        else
        {
            Write-Error "Error creating scheduled task '$taskPath\$taskName' - $response"
            $false
        }
    }
    catch
    {
        Write-Error "Error creating scheduled task '$taskPath\$taskName'"
        $false
    }
    finally
    {
        $ErrorActionPreference = 'Continue'
    }    
}

function ScheduledTask-CreateOrUpdateFromXml([string] $taskName, [string] $taskPath, [string] $taskXmlPath)
{
    if((ScheduledTask-Delete $taskName $taskPath) -eq $true)
    {
        ScheduledTask-CreateFromXml $taskName $taskPath $taskXmlPath
    }
    else
    {
        $false
    }
}

Write-Host "Ensconce - scheduledTaskHelper Loaded"
$scheduledTaskHelperLoaded = $true