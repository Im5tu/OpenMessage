#Skip the NuGet package cache generation
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1

function Log($message, $project)
{
    Write-Host ("{0}: {1}" -f $message, $project) -ForegroundColor Cyan
}

(gci -Filter *.sln) | % { 
    Log "Restoring" $_.FullName
    & dotnet restore $_.FullName

    Log "Building" $_.FullName
    & dotnet build $_.FullName 
}

(gci -Filter "*.csproj" -Path "tests" -Recurse) | % { 
    Log "Testing" $_.FullName
    & dotnet test $_.FullName --no-build
}