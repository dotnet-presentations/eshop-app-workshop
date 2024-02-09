@ECHO OFF

:: Install the latest version of the Aspire workload from the release branches
powershell -ExecutionPolicy Bypass -NoProfile -Command "& '.\build\installLatestFromReleaseBranch.ps1'"

dotnet build .\build\Build.proj