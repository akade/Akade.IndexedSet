Remove-Item Akade.IndexedSet.Tests\TestResults -Recurse
dotnet test --collect "XPlat Code Coverage"
$coverageReport = Get-Childitem -Path Akade.IndexedSet.Tests/TestResults -Include coverage.cobertura.xml -Recurse | select -expand FullName
dotnet reportgenerator -reports:"$coverageReport" -targetdir:"coveragereport" -reporttypes:Html
Invoke-Item coveragereport/index.html