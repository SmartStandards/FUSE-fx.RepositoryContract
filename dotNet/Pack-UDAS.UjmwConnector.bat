
nuget pack ./UDAS.Connector.nuspec -Symbols -OutputDirectory ".\(Stage)\Packages" -InstallPackageToOutputPath

IF NOT EXIST "..\..\..\(NuGetRepo)" GOTO NOCOPYTOGLOBALREPO
xcopy ".\(Stage)\Packages\udas.connector.nuspec" "..\..\..\(NuGetRepo)\" /d /r /y /s
xcopy ".\(Stage)\Packages\UDAS.Connector*.nupkg*" "..\..\..\(NuGetRepo)\" /d /r /y /s
:NOCOPYTOGLOBALREPO

PAUSE