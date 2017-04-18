#Skip the NuGet package cache generation
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1

function FindProjects($path)
{
    return gci -Filter *.csproj -Path $path -Recurse
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
    $outputXml = ("testResults\{0}.TestResults.xml" -f $project) # TODO :: Find out how to output xml results
    . dotnet test $location --no-build
}

FindProjects "tests" | % { Test $_.FullName }