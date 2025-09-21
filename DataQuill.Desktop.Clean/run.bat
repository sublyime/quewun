@echo off
cd /d "c:\code\quewun\DataQuill.Desktop.Clean"
dotnet build
if %errorlevel% equ 0 (
    echo Build successful, running application...
    dotnet run
) else (
    echo Build failed!
    pause
)