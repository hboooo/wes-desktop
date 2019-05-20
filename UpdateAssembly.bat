echo on
%windir%\microsoft.net\framework\v4.0.30319\msbuild Tools\UpdateAssemblyInfo\UpdateAssemblyInfo.csproj /p:configuration=Debug,Platform=x86 /t:rebuild
cd setup\debug\
UpdateAssemblyInfo.exe 
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\AVNET --assName AvnetAssemblyInfo.cs --moduleName AVNET
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\FitiPower --assName FitiPowerAssemblyInfo.cs --moduleName FitiPower
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\Sinbon --assName SinbonAssemblyInfo.cs --moduleName Sinbon
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\Allsor --assName AllsorAssemblyInfo.cs --moduleName Allsor
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\Mcc --assName MccAssemblyInfo.cs --moduleName Mcc
UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\Promaster --assName PromasterAssemblyInfo.cs --moduleName Promaster

UpdateAssemblyInfo.exe --output AddinsAssemblyInfo --folder Components\Wes.Widgets\Wes.Component.Widgets --assName GeneralAddInAssemblyInfo.cs --moduleName General
cd ..\..\