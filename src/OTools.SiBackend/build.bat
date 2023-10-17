dotnet publish -p:PublishDir=..\..\build -r win-x86 -c Release -p:PublishSingleFile=true --self-contained false
cd ../../build
del sib.exe
ren OTools.SiBackend.exe sib.exe