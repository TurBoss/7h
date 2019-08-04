set SLN="7thWrapperSln.sln"
set BUILD=".dist\Build"


devenv /clean Debug %SLN%
devenv /build Debug %SLN%

devenv /clean Release %SLN%
devenv /build Release %SLN%

cd %BUILD%\Debug
7z a "Debug.zip" "*"
move Debug.zip ..\..

cd %BUILD%\Release
7z a "Release.zip" "*"
move Release.zip ..\..

pause
