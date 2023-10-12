dotnet publish -p:PublishDir=..\..\build -r win-x64 -c Release -p:PublishSingleFile=true --self-contained false
cd ../../build
del winmap.exe
ren OTools.WinMap.exe winmap.exe
exit