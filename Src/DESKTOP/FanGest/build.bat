@echo off
echo ========================================================
echo  Compilando FanGest (Zero-Latency Hardware Fan Manager)...
echo ========================================================

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set DEST=C:\soft\Utilidades\FanGest

if not exist %CSC% (
    echo [ERROR] No se encontro el compilador csc.exe en el sistema.
    exit /b 1
)

%CSC% /target:winexe /platform:x64 /optimize+ /win32icon:"%~dp0fan.ico" /reference:System.dll,System.Drawing.dll,System.Windows.Forms.dll,Microsoft.CSharp.dll,"%~dp0LibreHardwareMonitorLib.dll" /out:"%~dp0FanGest.exe" "%~dp0Program.cs"

if %ERRORLEVEL% equ 0 (
    echo.
    echo [EXITO] Compilacion completada con exito: FanGest.exe
    if not exist "%DEST%" mkdir "%DEST%"
    copy /y "%~dp0FanGest.exe" "%DEST%\" >nul
    copy /y "%~dp0FanGest.sys" "%DEST%\" >nul
    copy /y "%~dp0fan.ico" "%DEST%\" >nul
    copy /y "%~dp0*.dll" "%DEST%\" >nul
    copy /y "%~dp0DumpAllSensors.exe" "%DEST%\" >nul
    copy /y "%~dp0InspectControl.exe" "%DEST%\" >nul
    copy /y "%~dp0InspectNct.exe" "%DEST%\" >nul
    copy /y "%~dp0InspectSuperIo.exe" "%DEST%\" >nul
    copy /y "%~dp0TestBiosRestore.exe" "%DEST%\" >nul
    copy /y "%~dp0TestProbe.exe" "%DEST%\" >nul
    echo [PUBLICACION] Archivos desplegados con exito en: %DEST%
) else (
    echo.
    echo [ERROR] Fallo la compilacion de FanGest.exe.
    exit /b 1
)
