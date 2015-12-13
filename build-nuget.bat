set projFile=%~dp0\Source\FormattableSql.Core\FormattableSql.Core.csproj
set outputDir=%~dp0\Package
set msbuild=C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe

"%msbuild%" "%projFile%" /p:Configuration=Release
mkdir "%outputDir%"
"%~dp0\.nuget\nuget.exe" pack "%projFile%" -Prop Configuration=Release -OutputDirectory "%outputDir%"
