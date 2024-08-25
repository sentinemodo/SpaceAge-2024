cd "C:\Users\Kaczma10\Documents\Visual Studio 2015\Projects\SpaceAge"

c:\dev\opencover\OpenCover.Console.exe -target:"C:\dev\NUnit\bin\nunit-console.exe" -targetargs:"/noshadow /output=TestsConsoleOutput.txt Tests.dll" -output:coverage.xml -register:Kaczma10 -targetdir:".\Tests\bin\Debug" -filter:+[*]* 

c:\dev\ReportsGenerator\bin\ReportGenerator.exe -reports:coverage.xml -targetdir:"./.coveragereport"

c:\dev\sonar-scanner\bin\sonar-scanner.bat

pause