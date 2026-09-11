# FanGest

Gestor de ventiladores de alto rendimiento y ultra-baja latencia diseñado para el control directo de ventiladores de placa base (chip SuperIO Nuvoton NCT6687D/NCT6687DR) y tarjeta gráfica NVIDIA GeForce RTX 5090 con una arquitectura de CERO LATENCIA (Zero-Polling) en reposo mediante integración en la bandeja del sistema (System Tray).

---

## 🚀 Componentes Principales

### 🖥️ Desktop: FanGest (`Src/DESKTOP/FanGest/`)
- **Política Zero-Polling en Reposo**: Al minimizar a la bandeja del sistema (`NotifyIcon`), el temporizador de sondeo se detiene al 100% (`0 llamadas a sensores, 0 bloqueos de bus, 0,00 ms de latencia DPC`).
- **Retención en Silicio**: Los registros PWM del chip Nuvoton retienen la velocidad en hardware sin requerir ciclos de CPU.
- **Canales Soportados**: CPU Fan, Pump Fan, Ventilador Trasero, Lateral, Delantero, GPU Fan 1 y GPU Fan 2 (RTX 5090).
- **Acciones Rápidas**:
  - `🌪️ TODOS AL 100% (TURBO)`: Modo turbo instantáneo para sesiones intensivas de juego.
  - `🍃 MODO BIOS (CONTROL AUTOMÁTICO)`: Restitución de las curvas automáticas del firmware UEFI de la placa base (`SetDefault()`).
  - `❌ Salir y Restaurar BIOS`: Garantía de seguridad térmica al cerrar el proceso.

### 🛠️ Compilación Nativa
```cmd
cd Src\DESKTOP\FanGest
build.bat
```
Compila con `csc.exe` x64 enlazando `LibreHardwareMonitorLib.dll` y `NvAPIWrapper.dll`.

---

## 🏛️ Estructura Golden Template

| Directorio | Propósito |
|---|---|
| `.agent/` | Contexto de IA, reglas de gobernanza y arquitectura |
| `.vscode/` | Configuración de entorno y Material Icon Theme para `DESKTOP` |
| `.obsidian/` | Configuración del Vault Obsidian y plugins |
| `Conversaciones/` | Registro de la conversación histórica original |
| `Docs/` | MOCs Zettelkasten, arquitectura y diario de desarrollo |
| `Src/DESKTOP/` | Código fuente, DLLs de hardware, driver de kernel y utilitarios |