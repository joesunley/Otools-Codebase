dotnet publish -p:PublishDir=..\..\build -r win-x64 -c Release -p:PublishSingleFile=true --self-contained false
cd ../../build
del boflevee.exe
ren OTools.BofLevee.exe boflevee.exe
exit