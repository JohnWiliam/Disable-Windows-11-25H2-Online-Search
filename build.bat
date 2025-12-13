@echo off
echo ========================================================
echo  Disable Windows 11 Online Search - Build Script
echo ========================================================
echo.
echo Building portable executable for Windows x64...
echo.

cd DisableWin11Search
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ../Build
cd ..

echo.
echo ========================================================
echo  Build Complete!
echo  You can find the executable in the 'Build' folder.
echo ========================================================
pause
