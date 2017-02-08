set SLN="C:\cygwin64\home\Jauria\Proyectos\Final\7H\Git\7H\7thWrapperSln.sln"
set BUILD="C:\cygwin64\home\Jauria\Proyectos\Final\7H\Git\Build"


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
