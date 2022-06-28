$currentDirectory = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)

if ($deployHelpLoaded -eq $null)
{
    . $currentDirectory\deployHelp.ps1
}

Write-Host "Ensconce - KubernetesHelper Loading"
if ([string]::IsNullOrWhiteSpace($KubeCtlExe))
{
	$KubeCtlExe = "$EnsconceDir\Tools\KubeCtl\kubectl.exe"
}
if ([string]::IsNullOrWhiteSpace($DatreeExe))
{
	$DatreeExe = "$EnsconceDir\Tools\Datree\datree.exe"
}
$rootConfigPath = "$Home\.kube"

if (Test-Path $KubeCtlExe)
{
    (& $KubeCtlExe version --client 2>&1) | ForEach-Object {
        if ($_ -match "^Client Version.*")
        {
            $data = ConvertFrom-Json ($_ -replace "Client Version: version.Info", "")
            $clientVersion = $data.GitVersion
            Write-Host "KubeCtl Version: $clientVersion"
        }
    }
}
else
{
    throw "'$KubeCtlExe' doesn't exist"
}

if (Test-Path $DatreeExe)
{
	(& $DatreeExe version 2>&1) | Select-Object -First 1 {
    	Write-Host "Datree Version: $_"
	}
    $DatreeExeFound = $true
}
else
{
    Write-Warning "Datree exe not found at $DatreeExe"
    $DatreeExeFound = $false
}

function PreProcessYaml([string]$yamlDirectory)
{
    if ((Test-Path -Path "$yamlDirectory\kustomization.yaml") -eq $false)
    {
        Write-Host "Creating kustomization.yaml"
        Add-Content -Path "$yamlDirectory\kustomization.yaml" -Value "resources:"
        Get-ChildItem "$yamlDirectory" -Filter *.yaml | Foreach-Object {
            if ($_.Name -ne "kustomization.yaml")
            {
                Add-Content -Path "$yamlDirectory\kustomization.yaml" -Value $_.Name
            }
        }
    }

    Write-Host "Replace tags in yaml in $yamlDirectory"
    ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.yaml | Write-Host

    Write-Host "Running kustomize in $yamlDirectory"
    $output = & $KubeCtlExe kustomize $yamlDirectory

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Kustomize error processing $kustomizationPath"
        exit $LASTEXITCODE
    }

    Out-File -FilePath "$yamlDirectory\kustomization-output.yaml" -InputObject $output

    "$yamlDirectory\kustomization-output.yaml"
}

function ValidateK8sYaml([string]$yamlFile, [string]$kubernetesConfigFile)
{
    if([string]::IsNullOrWhiteSpace($DatreeToken) -eq $false -and $DatreeExeFound)
    {
        & $DatreeExe config set token $DatreeToken
        if([string]::IsNullOrWhiteSpace($DatreePolicyYaml) -eq $false)
        {
        	$TempFile = New-TemporaryFile
        	$PolicyYamlFile = $TempFile.FullName.Replace(".tmp", ".yaml")
        	Rename-Item $TempFile $PolicyYamlFile
        	$DatreePolicyYaml | Out-File -FilePath $PolicyYamlFile
        	& $DatreeExe publish $PolicyYamlFile
        	Remove-Item $PolicyYamlFile -force
        }

        if($DatreeRecord -eq $true)
        {
            if([string]::IsNullOrWhiteSpace($DatreePolicy))
            {
                & $DatreeExe test $yamlFile --output simple
            }
            else
            {
                & $DatreeExe test $yamlFile --output simple --policy $DatreePolicy
            }
        }
        else
        {
            if([string]::IsNullOrWhiteSpace($DatreePolicy))
            {
                & $DatreeExe test $yamlFile --output simple --no-record
            }
            else
            {
                & $DatreeExe test $yamlFile --output simple --no-record --policy $DatreePolicy
            }
        }

        if ($LASTEXITCODE -ne 0)
        {
            if($DatreeFailOnError -eq $false)
            {
                Write-Warning "Datree errors for yaml file $yamlFile"
            }
            else
            {
                Write-Error "Datree errors for yaml file $yamlFile"
                exit $LASTEXITCODE
            }
        }
    }

    $kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"

    Write-Host "Validating yaml file $yamlFile (local)"
    & $KubeCtlExe apply --dry-run=client -f $yamlFile --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        if($KubeCtlFailOnDryRun -eq $false)
        {
            Write-Warning "Invalid yaml file $yamlFile"
        }
        else
        {
            Write-Error "Invalid yaml file $yamlFile"
            exit $LASTEXITCODE
        }
    }

    Write-Host "Validating yaml file $yamlFile (server-side)"
    & $KubeCtlExe apply --dry-run=server -f $yamlFile --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        if($KubeCtlFailOnDryRun -eq $false)
        {
            Write-Warning "Invalid yaml file $yamlFile"
        }
        else
        {
            Write-Error "Invalid yaml file $yamlFile"
            exit $LASTEXITCODE
        }
    }
}

function SetK8sContext([string]$kubernetesConfigFile)
{
    $kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"
    $targetContext = "k8s-cluster"

    Write-Host "Getting current context"
    $currentContext = & $KubeCtlExe config current-context --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error getting current kubernetes context"
        exit $LASTEXITCODE
    }

    if ($currentContext -ne $targetContext)
    {
        Write-Host "Switching from '$currentContext' context to '$targetContext' context"
        & $KubeCtlExe config use-context "$targetContext" --kubeconfig=$kubernetesConfigFilePath

        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "Error setting kubernetes context to '$targetContext'"
            exit $LASTEXITCODE
        }
    }
    else
    {
        Write-Host "Already working with '$targetContext' context"
    }
}

function GetResourceVersionsUsed([string]$kubernetesConfigFile, [string]$selector)
{
    Write-Host "Get Accessible Resources"
    $resourceVersions = @()
    $resources = @()

    $rawResources = & $KubeCtlExe api-resources --verbs=list --namespaced -o name --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error all api resources"
        exit $LASTEXITCODE
    }

    foreach($resource in $rawResources)
    {
        if ($resource.Contains("."))
        {
            $resource = $resource.Substring(0, $resource.IndexOf("."))
        }

        if ($resources -notcontains $resource)
        {
            Write-Host "  Checking $resource is accessible"
            $cani = & $KubeCtlExe auth can-i list $resource --kubeconfig=$kubernetesConfigFilePath

            if ($LASTEXITCODE -eq 0 -and $cani -eq "yes")
            {
                $resources += $resource
            }
        }
    }

    if ($resources.Count -gt 0)
    {
        Write-Host "Accessible Server Resources: $resources"
        Write-Host "Getting Resources Used On Selector: $selector"

        foreach($resource in $resources)
        {
            Write-Host "  Getting: $resource"
            $output = & $KubeCtlExe get $resource -l $selector -o json --kubeconfig=$kubernetesConfigFilePath | ConvertFrom-Json

            if ($LASTEXITCODE -ne 0)
            {
                Write-Error "Error getting $resource"
                exit $LASTEXITCODE
            }

            foreach($item in $output.items)
            {
                $groupVersion = $item.apiVersion
                if (-not($groupVersion.Contains("/")))
                {
                    $groupVersion = "core/${groupVersion}"
                }
                $kind = $item.kind
                $GroupVersionKind = "${groupVersion}/${kind}"

                if ($resourceVersions -notcontains $GroupVersionKind)
                {
                    $resourceVersions += $GroupVersionKind
                }
            }
        }

        if ($resourceVersions.Count -gt 0)
        {
            Write-Host "Used API Versions: $resourceVersions"
        }
        else
        {
            Write-Host "No User API Versions"
        }
    }
    else
    {
        Write-Host "No Accessible Server Resources"
    }

    $resourceVersions
}

function DeployToK8s([string]$yamlFile, [string]$kubernetesConfigFile, [string]$pruneSelector)
{
    $kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"

    $prunableList = GetResourceVersionsUsed $kubernetesConfigFile $pruneSelector

    $deploymentName = ""
    Write-Host "Deploying yaml file $yamlFile"
    #Run using Invoke-Expression because of dynamic parameters
    if ($prunableList.Count -gt 0)
    {
        $pruneWhiteList = $prunableList -join " --prune-whitelist="
        $command = "$KubeCtlExe apply -f $yamlFile --prune -l $pruneSelector --prune-whitelist=$pruneWhiteList --kubeconfig=$kubernetesConfigFilePath"
    }
    else
    {
        $command = "$KubeCtlExe apply -f $yamlFile --kubeconfig=$kubernetesConfigFilePath"
    }

    Invoke-Expression $command | foreach-object {
        Write-Host $_
        if ($_.StartsWith("deployment.apps/"))
        {
            $deploymentLineName = $_.Substring(0, $_.IndexOf(' '))
            if ($deploymentName -eq "")
            {
                $deploymentName = $deploymentLineName
            }
            else
            {
                Write-Warning "Second deployment detected in package with name $deploymentLineName - This will not be checked for rollout"
            }
        }
    }

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Error applying yaml file $yamlFile"
        exit $LASTEXITCODE
    }

    if ($deploymentName -eq "")
    {
        Write-Error "Unable to establish deployment name"
        exit -1
    }

    For ($i=0; $i -lt 5; $i++) {
        & $KubeCtlExe get $deploymentName --kubeconfig=$kubernetesConfigFilePath
        if ($LASTEXITCODE -eq 0) {
            break
        }
        Start-Sleep 5
    }

    & $KubeCtlExe rollout status $deploymentName --kubeconfig=$kubernetesConfigFilePath
    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Rollout of $deploymentName was not successful"
        exit $LASTEXITCODE
    }
}

function DeployYamlFilesToK8sCluster([string]$yamlDirectory, [string]$kubernetesConfigFile, [string]$pruneSelector)
{
    $yamlFile = PreProcessYaml $yamlDirectory

    SetK8sContext $kubernetesConfigFile

    ValidateK8sYaml $yamlFile $kubernetesConfigFile

    DeployToK8s $yamlFile $kubernetesConfigFile $pruneSelector
}

Write-Host "Ensconce - KubernetesHelper Loaded"
