if ($deployHelpLoaded -eq $null)
{
    $DeployToolsDir = Split-Path ((Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path)
    . $DeployToolsDir\deployHelp.ps1
}

Write-Host "Ensconce - KubernetesHelper Loading"
if ([string]::IsNullOrWhiteSpace($KubeCtlExe))
{
    $KubeCtlExe = "$DeployToolsDir\Tools\KubeCtl\kubectl.exe"
}
if ([string]::IsNullOrWhiteSpace($KubeLinterExe))
{
    $KubeLinterExe = "$DeployToolsDir\Tools\Kube-Linter\kube-linter.exe"
}
$rootConfigPath = "$Home\.kube"

if (Test-Path $KubeCtlExe)
{
    (& $KubeCtlExe version --output=json --client 2>&1) | ForEach-Object {
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

if (Test-Path $KubeLinterExe)
{
    $kubeLinterVersion = (& $KubeLinterExe version 2>&1)
    Write-Host "Kube-Linter Version: $kubeLinterVersion"
    $KubeLinterExeFound = $true
}
else
{
    Write-Warning "Kube-Linter exe not found at $KubeLinterExe"
    $KubeLinterExeFound = $false
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
                Add-Content -Path "$yamlDirectory\kustomization.yaml" -Value "  - $($_.Name)"
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($KustomizeTemplatesFolder) -eq $false -and (Test-Path $KustomizeTemplatesFolder))
    {
        Write-Host "Copying templates from $KustomizeTemplatesFolder into $yamlDirectory\templates"
        Copy-Item -Path $KustomizeTemplatesFolder -Destination "$yamlDirectory\templates" -Recurse | Out-Null
    }

    Get-ChildItem -Path $yamlDirectory -Filter "*substitution*.xml" -Recurse | ForEach-Object {
        Write-Host "Processing Subsitution: $($_.FullName)"
        ensconce --deployFrom $yamlDirectory --updateConfig --substitutionPath $_.FullName | Write-Host
    }

    if ([string]::IsNullOrWhiteSpace($ScriptDir) -eq $false)
    {
	    Get-ChildItem -Path $ScriptDir -Filter "*substitution*.xml" | ForEach-Object {
	        Write-Host "Processing Subsitution: $($_.FullName)"
	        ensconce --deployFrom $ScriptDir --updateConfig --substitutionPath $_.FullName | Write-Host
	    }
    }

    Write-Host "Replace tags in yaml in $yamlDirectory"
    ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.yaml | Write-Host

    Write-Host "Replace tags in json in $yamlDirectory"
    ensconce --deployFrom $yamlDirectory --treatAsTemplateFilter=*.json | Write-Host

    Get-ChildItem -Path $yamlDirectory -Filter "*.*" -File -Recurse | ForEach-Object {
        Write-Host "Update Encoding To UTF-8 Without BOM: $($_.FullName)"
        $MyRawString = Get-Content -Raw $_.FullName
        $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
        [System.IO.File]::WriteAllLines($_.FullName, $MyRawString, $Utf8NoBomEncoding)
    }

    Write-Host "Running kustomize in $yamlDirectory"
    $outputFile = "$yamlDirectory\kustomization-output.yaml"
    & $KubeCtlExe kustomize $yamlDirectory --output $outputFile

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Kustomize error processing $kustomizationPath"
        exit $LASTEXITCODE
    }

    $outputFile
}

function ValidateK8sYaml([string]$yamlFile, [string]$kubernetesConfigFile)
{
    if($KubeLinterExeFound)
    {
        $KubeLinterConfigYamlFile = [IO.Path]::Combine((Split-Path -Path $yamlFile), "kube-linter-config.yaml")
        if(Test-Path $KubeLinterConfigYamlFile)
        {
            Remove-Item $KubeLinterConfigYamlFile -Force
        }

        if([string]::IsNullOrWhiteSpace($KubeLinterConfigYaml) -eq $false)
        {
            $KubeLinterConfigYaml | Out-File -FilePath $KubeLinterConfigYamlFile
        }

        Write-Host "Validating yaml file $yamlFile (kube-linter)"

        $ErrorActionPreferenceOrig = $ErrorActionPreference
        $ErrorActionPreference = "SilentlyContinue"
        if(Test-Path $KubeLinterConfigYamlFile)
        {
            $kubeLinterOutput = & $KubeLinterExe lint $yamlFile --fail-if-no-objects-found --fail-on-invalid-resource --format json --config $KubeLinterConfigYamlFile 2>&1
        }
        else
        {
            $kubeLinterOutput = & $KubeLinterExe lint $yamlFile --fail-if-no-objects-found --fail-on-invalid-resource --format json 2>&1
        }
        $ErrorActionPreference = $ErrorActionPreferenceOrig

        $kubeLinterData = ConvertFrom-Json $kubeLinterOutput

        if ($LASTEXITCODE -ne 0 -or $kubeLinterData.Summary.ChecksStatus -eq "Failed")
        {
            $FailureCount = 0
            $ErrorTable = @()
            $kubeLinterData.Reports | ForEach-Object {
                $ErrorTable += [PSCustomObject] @{
                    Name = $_.Object.K8sObject.Name
                    Kind = $_.Object.K8sObject.GroupVersionKind.Kind
                    Check = $_.Check
                    Message = $_.Diagnostic.Message
                    Remediation = $_.Remediation
                    Docs = "https://docs.kubelinter.io/#/generated/checks?id=$($_.Check)"
                    Full = $_
                }
                $FailureCount++
            }
            Write-Host
            Write-Host "--------------------------------------"
            Write-Host "Failures Report"
            Write-Host "_______________"
            $ErrorTable | Format-Table -Wrap -AutoSize -GroupBy @{name = "Failure Object"; expr = {"$($_.Name) ($($_.Kind))"}} -Property Check, Message, Docs
            Write-Host "--------------------------------------"
            Write-Host
            $KubeLinterFailureMessage = "$FailureCount Errors for yaml file $yamlFile (kube-linter)"
            if($KubeLinterFailureMode -eq "LOG")
            {
                $KubeLinterFailureMessage | Write-Host
            }
            else
            {
                if($KubeLinterFailureMode -eq "WARN")
                {
                    $KubeLinterFailureMessage | Write-Warning
                }
                else
                {
                    $KubeLinterFailureMessage | Write-Error
                    exit $LASTEXITCODE
                }
            }
        }
        else
        {
            Write-Host "No errors for yaml file $yamlFile (kube-linter)"
        }
    }

    $kubernetesConfigFilePath = "$rootConfigPath\$kubernetesConfigFile"

    Write-Host "Validating yaml file $yamlFile (local)"
    & $KubeCtlExe apply --dry-run=client -f $yamlFile --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        if($KubeCtlFailOnDryRun -eq $false)
        {
            Write-Warning "Invalid yaml file $yamlFile (local)"
        }
        else
        {
            Write-Error "Invalid yaml file $yamlFile (local)"
            exit $LASTEXITCODE
        }
    }
    else
    {
        Write-Host "Valid yaml file $yamlFile (local)"
    }

    Write-Host "Validating yaml file $yamlFile (server-side)"
    & $KubeCtlExe apply --dry-run=server -f $yamlFile --kubeconfig=$kubernetesConfigFilePath

    if ($LASTEXITCODE -ne 0)
    {
        if($KubeCtlFailOnDryRun -eq $false)
        {
            Write-Warning "Invalid yaml file $yamlFile (server-side)"
        }
        else
        {
            Write-Error "Invalid yaml file $yamlFile (server-side)"
            exit $LASTEXITCODE
        }
    }
    else
    {
        Write-Host "Valid yaml file $yamlFile (server-side)"
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
