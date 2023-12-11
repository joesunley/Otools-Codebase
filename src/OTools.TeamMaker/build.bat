dotnet publish -p:PublishDir=..\..\build -r win-x64 -c Release -p:PublishSingleFile=true --self-contained false
cd ../../build
del teammaker.exe
ren OTools.TeamMaker.exe teammaker.exe
exit