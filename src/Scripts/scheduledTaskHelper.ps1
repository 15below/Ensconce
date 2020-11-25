Write-Host "Ensconce - scheduledTaskHelper Loading"

function ScheduledTask-CheckExists([string] $taskName, [string] $taskPath)
{
    try
    {
        Write-Host "Checking if scheduled task '\$taskPath\$taskName' exists"

        $response = & "schtasks" "/query" "/TN" "\$taskPath\$taskName" 2>&1

        if($LASTEXITCODE -eq 0 -and $response -match ".*$taskName.*")
        {
            Write-Host "Task '\$taskPath\$taskName' does exist"
            $true
        }
        else
        {
            Write-Host "Task '\$taskPath\$taskName' does NOT exist"
            $false
        }
    }
    catch
    {
        Write-Host "Checking if task '\$taskPath\$taskName' exist errored - $_"
        $false
    }
    finally
    {
        #Reset last exit code, because otherwise deploys fail - the error is handled in this function
        $global:LASTEXITCODE = $null
    }
}

function ScheduledTask-Delete([string] $taskName, [string] $taskPath)
{
    if((ScheduledTask-CheckExists $taskName $taskPath) -eq $true)
    {
        try
        {
            Write-Host "Deleting scheduled task '\$taskPath\$taskName'"

            $response = & "schtasks" "/delete" "/TN" "\$taskPath\$taskName" "/f" 2>&1

            if($LASTEXITCODE -eq 0 -and $response -match ".*SUCCESS.*")
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
            Write-Error "Error deleting scheduled task '$taskPath\$taskName' - $_"
            $false
        }
        finally
        {
            #Reset last exit code, because otherwise deploys fail - the error is handled in this function
            $global:LASTEXITCODE = $null
        }
    }
}

function ScheduledTask-CreateFromXml([string] $taskName, [string] $taskPath, [string] $taskXmlPath)
{
    try
    {
        Write-Host "Creating scheduled task '\$taskPath\$taskName'"
        $response = & "schtasks" "/create" "/XML" $taskXmlPath "/TN" "\$taskPath\$taskName" "/F" 2>&1

        if($LASTEXITCODE -eq 0 -and $response -match ".*SUCCESS.*")
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
        Write-Error "Error creating scheduled task '$taskPath\$taskName' - $_"
        $false
    }
    finally
    {
        #Reset last exit code, because otherwise deploys fail - the error is handled in this function
        $global:LASTEXITCODE = $null
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

function ScheduledTask-Run([string] $taskName, [string] $taskPath)
{
    if((ScheduledTask-CheckExists $taskName $taskPath) -eq $true)
    {
        try
        {
            Write-Host "Running scheduled task '\$taskPath\$taskName'"

            $response = & "schtasks" "/run" "/TN" "\$taskPath\$taskName" 2>&1

            if($LASTEXITCODE -eq 0 -and $response -match ".*SUCCESS.*")
            {
                Write-Host "Scheduled task '\$taskPath\$taskName' ran"
                $true
            }
            else
            {
                Write-Error "Error running scheduled task '$taskPath\$taskName' - $response"
                $false
            }
        }
        catch
        {
            Write-Error "Error running scheduled task '$taskPath\$taskName' - $_"
            $false
        }
        finally
        {
            #Reset last exit code, because otherwise deploys fail - the error is handled in this function
            $global:LASTEXITCODE = $null
        }
    }
}

Write-Host "Ensconce - scheduledTaskHelper Loaded"
$scheduledTaskHelperLoaded = $true