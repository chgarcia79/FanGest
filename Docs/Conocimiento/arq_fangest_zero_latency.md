---
tipo: arquitectura
resumen: "Arquitectura de FanGest para la gestión de ventiladores con política de Cero Latencia (Zero-Polling) en segundo plano mediante System Tray."
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: ["[[000_Indice_Arquitectura_y_Diseno]]", "[[000_Indice_Hardware_y_Firmware]]"]
tecnologias: [C#, .NET Framework 4.8, WinForms, LibreHardwareMonitor, System Tray, SuperIO]
archivos_claves: ["T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/Program.cs"]
prioridad_busqueda: 9
tags: [arquitectura, fangest, cero-latencia, zero-polling, systray, ventiladores]
criticidad: "Alta"
resolucion: "Resuelto"
---

# Arquitectura de Cero Latencia: FanGest

## 1. El Paradigma "Zero-Polling" frente al Monitoreo Tradicional
Herramientas populares como FanControl o HWInfo aplican un bucle continuo de sondeo (*polling loop*) que lee los sensores de la placa base cada 1.000 ms. 
- Cada lectura requiere consultar el bus LPC/SMBus mediante llamadas al controlador de kernel.
- En sistemas de alta gama orientados a latencias de microsegundos, estos accesos generan interrupciones recurrentes que se manifiestan como micro-tirones (*spikes* de DPC).

**FanGest** rompe este paradigma desacoplando la **orden de velocidad** de la **monitorización continua**.

```
+-------------------------------------------------------------+
|                     ESTADO VISIBLE (UI)                     |
|  Timer de Polling ACTIVO (1.000 ms)                         |
|  Actualiza Sliders, % y RPM en pantalla                     |
+-------------------------------------------------------------+
                              |
                              v  [Minimizar a Systray o pulsar X]
+-------------------------------------------------------------+
|                  ESTADO SEGUNDO PLANO (Systray)             |
|  Timer.Stop() -> PARADA TOTAL DE SONDEO                     |
|  0 lecturas, 0 llamadas a bus, 0,00 ms de latencia          |
|  El chip SuperIO Nuvoton mantiene físicamente el PWM        |
+-------------------------------------------------------------+
```

## 2. Modos de Operación

### 2.1 Modo 100% Turbo Global
- Invoca `Control.SetSoftware(100)` sobre todos los canales registrados (CPU Fan, Pump, Trasero, Lateral, Delantero y GPU Fans).
- Maximiza el flujo de aire previo al inicio de una sesión intensiva de juego o computación.

### 2.2 Modo Automático BIOS (`SetDefault()`)
- Invoca `Control.SetDefault()` sobre los canales.
- Devuelve inmediatamente la modulación PWM a las curvas configuradas en la BIOS UEFI de la placa base sin necesidad de reiniciar el equipo.

### 2.3 Modo Manual por Canal
- Cada tarjeta en la interfaz expone un deslizador (`TrackBar`) de 0 a 100% y un interruptor de modo manual, permitiendo curvas estáticas personalizadas por ventilador.
