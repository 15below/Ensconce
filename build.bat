.paket\paket.exe restore
echo f | xcopy /f /y "Core.fsx" "paket-files/15below/Build.Tools/Core.fsx"
packages\FAKE\tools\FAKE.exe paket-files\15below\Build.Tools\Core.fsx "solution=src\Ensconce.sln"