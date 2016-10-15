#Disable Telemetry
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1

function FindProjects($path)
{
    return gci -Filter project.json -Path $path -Recurse
}

#Restore packages
Write-Host "Restoring Packages" -ForegroundColor Green
. dotnet restore

#Build projects
function Build ($location)
{
    Write-Host "Building project '$location'..." -ForegroundColor Green
    . dotnet build $location
}

FindProjects "src" | % { Build $_.FullName }

#Test projects
function Test ($location)
{
    Write-Host "Testing project '$location'..." -ForegroundColor Green
    . dotnet test $location
}

FindProjects "tests" | % { Test $_.FullName }