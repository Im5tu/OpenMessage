#Disable Telemetry
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1

#Skip the NuGet package cache generation
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1

function FindProjects($path)
{
    return gci -Filter project.json -Path $path -Recurse
}

function GetProjectName($path)
{
    return Split-Path (Split-Path $path -Parent) -Leaf
}

#Restore packages
Write-Host "Restoring Packages" -ForegroundColor Green
. dotnet restore

#Build projects
function Build ($location)
{
    $project = GetProjectName $location
    Write-Host "Building project '$project'..." -ForegroundColor Green
    . dotnet build $location
}

(Get-Content "global.json" | ConvertFrom-Json).projects | FindProjects $_ | % { Build $_.FullName }

#Test projects
if((Test-Path "testResults") -eq 0)
{
    $dir = mkdir "testResults"
}
else
{
    Remove-Item "testResults/*.xml" -Force    
}
function Test ($location)
{  
    $project = GetProjectName $location
    Write-Host "Testing project '$project'..." -ForegroundColor Green
    . dotnet test $location --no-build -xml ("testResults\{0}.TestResults.xml" -f $project) 
}

FindProjects "tests" | % { Test $_.FullName }