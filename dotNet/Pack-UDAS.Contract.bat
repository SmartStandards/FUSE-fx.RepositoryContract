
nuget pack ./UDAS.Contract.nuspec -Symbols -OutputDirectory ".\(Stage)\Packages" -InstallPackageToOutputPath

IF NOT EXIST "..\..\..\(NuGetRepo)" GOTO NOCOPYTOGLOBALREPO
xcopy ".\(Stage)\Packages\udas.contract.nuspec" "..\..\..\(NuGetRepo)\" /d /r /y /s
xcopy ".\(Stage)\Packages\UDAS.Contract*.nupkg*" "..\..\..\(NuGetRepo)\" /d /r /y /s
:NOCOPYTOGLOBALREPO

PAUSE