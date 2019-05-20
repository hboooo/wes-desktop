echo on
call releasebuild.bat
MSBuild Main\Wes.Launcher\Wes.Launcher.csproj /p:configuration=Release,Platform=x86 /t:rebuild
MSBuild Main\Wes.Launcher\Wes.Launcher.csproj /p:configuration=Debug,Platform=x86 /t:rebuild
cd setup\debug\
Wes.Launcher.exe publish
cd ../../