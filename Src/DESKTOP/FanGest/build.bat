@echo off
echo ========================================================
echo  Compilando FanGest (Zero-Latency Hardware Fan Manager)...
echo ========================================================

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist %CSC% (
    echo [ERROR] No se encontro el compilador csc.exe en el sistema.
    exit /b 1
)

%CSC% /target:winexe /platform:x64 /optimize+ /win32icon:"%~dp0fan.ico" /reference:System.dll,System.Drawing.dll,System.Windows.Forms.dll,Microsoft.CSharp.dll,"%~dp0LibreHardwareMonitorLib.dll" /out:"%~dp0FanGest.exe" "%~dp0Program.cs"

if %ERRORLEVEL% equ 0 (
    echo.
    echo [EXITO] Compilacion completada con exito: FanGest.exe
) else (
    echo.
    echo [ERROR] Fallo la compilacion de FanGest.exe.
    exit /b 1
)
