# Contexto del Proyecto: FanGest

## Descripción
Gestor de ventiladores de alto rendimiento y ultra-baja latencia diseñado para controlar directamente los ventiladores de placa base (chip SuperIO Nuvoton NCT6687D/NCT6687DR) y tarjeta gráfica NVIDIA GeForce RTX 5090 con una arquitectura de CERO LATENCIA (Zero-Polling) en reposo mediante integración en la bandeja del sistema (System Tray).

## Stack Tecnológico Principal
- **Desktop / UI**: C# .NET Framework 4.8 / WinForms nativo x64.
- **Compilador**: `csc.exe` nativo de Windows (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`).
- **Hardware Abstraction**: `LibreHardwareMonitorLib.dll`, `NvAPIWrapper.dll`, `HidSharp.dll`.
- **Driver de Kernel**: `FanGest.sys` (acceso directo a puertos de E/S y registros de silicio).

## Arquitectura y Decisiones Clave
- **Ubicación del Código Desktop**: `Src/DESKTOP/FanGest/`
- **Zero-Polling en Reposo (Cero Latencia)**: Cuando la ventana se minimiza o envía al Systray (`NotifyIcon`), el temporizador de sondeo se DETIENE AL 100% (`0 lecturas SMBus/LPC, 0 bloqueos de bus, 0,00 ms de latencia DPC`). El chip Nuvoton de la placa base MSI retiene los valores en sus registros físicos de silicio.
- **Canales Controlados**: CPU Fan, Pump Fan, Trasero, Lateral, Delantero, GPU Fan 1 y GPU Fan 2.
- **Modos de Operación**: Modo Manual (sliders 0-100%), Modo Turbo 100% (`Control.SetSoftware(100)`), y Restauración BIOS Automático (`Control.SetDefault()`).

## Repositorio Oficial
- **GitHub Personal**: `git@github.com-personal:chgarcia79/FanGest.git`