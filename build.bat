@echo off
setlocal
REM TrackSpender build script. Needs: Visual Studio Build Tools (C/C++), CMake, .NET 8 SDK.

echo [1/3] Building the C/C++ core...
cmake -S native -B native\build -A x64 || goto :fail
cmake --build native\build --config Release || goto :fail
ctest --test-dir native\build -C Release --output-on-failure || goto :fail

echo [2/3] Building the C# app...
dotnet publish app\TrackSpender.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist || goto :fail

echo [3/3] Done. Run dist\TrackSpender.exe
exit /b 0

:fail
echo Build failed.
exit /b 1
