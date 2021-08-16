Write-Host "Ensconce - Powershell4Polyfill Loading"

function Clear-DnsClientCache()
{
	Write-Host "Calling IPConfig /flushdns to simulate Powershell 5 function"

	$ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
	$ProcessInfo.FileName = "ipconfig.exe"
	$ProcessInfo.RedirectStandardError = $true
	$ProcessInfo.RedirectStandardOutput = $true
	$ProcessInfo.UseShellExecute = $false
	$ProcessInfo.Arguments = "/flushdns"

	$Process = New-Object System.Diagnostics.Process
	$Process.StartInfo = $ProcessInfo
	$Process.Start() | Out-Null
	$Process.WaitForExit()

	$output = $Process.StandardOutput.ReadToEnd()
	write-host $output
}

Write-Host "Ensconce - Powershell4Polyfill Loaded"
