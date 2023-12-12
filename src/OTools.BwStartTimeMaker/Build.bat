dotnet publish -p:PublishDir=..\..\build -r win-x64 -c Release -p:PublishSingleFile=true --self-contained false
cd ../../build
del bwst.exe
ren OTools.BwStartTimeMaker.exe bwst.exe
exit