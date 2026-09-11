---
tipo: integracion
resumen: "Integración de hardware con el chip SuperIO Nuvoton NCT6687D/NCT6687DR y GPU NVIDIA GeForce RTX 5090 mediante LibreHardwareMonitor y NvAPIWrapper."
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: ["[[000_Indice_Hardware_y_Firmware]]", "[[000_Indice_Arquitectura_y_Diseno]]"]
tecnologias: [C#, LibreHardwareMonitor, NvAPIWrapper, Nuvoton, SuperIO, RTX 5090, FanGest.sys]
archivos_claves: ["T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/Program.cs", "T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/FanGest.sys"]
prioridad_busqueda: 9
tags: [integracion, hardware, nuvoton, nct6687dr, superio, rtx5090, driver]
criticidad: "Alta"
resolucion: "Resuelto"
---

# Integración con Chip SuperIO Nuvoton NCT6687DR & GPU RTX 5090

## 1. Topología del Hardware Identificado
En la placa base MSI MPG X870E CARBON WIFI / GODLIKE, el control térmico de los ventiladores de la caja y de la bomba de refrigeración líquida está gestionado por el circuito integrado **Nuvoton NCT6687D / NCT6687DR**:
- **Driver de Kernel**: `FanGest.sys` (proporciona acceso seguro a los puertos E/S `0x2E/0x2F` y `0x4E/0x4F` en modo privilegiado Ring 0).
- **Librería de Abstracción**: `LibreHardwareMonitorLib.dll` para mapear canales de hardware y exponer la interfaz `IControl`.

## 2. Mapeo de Canales y Sensores

| Canal en FanGest | Identificador Interno | Sensor Fan (RPM) | Control PWM Asignado |
|---|---|---|---|
| **CPU Fan** | `cpu_fan` | Sensor CPU Fan | Control 1 |
| **Pump Fan** | `pump_fan` | Sensor Pump Fan | Control 2 |
| **Trasero** | `rear_fan` | Sensor Sys Fan 1 | Control 3 |
| **Lateral** | `side_fan` | Sensor Sys Fan 2 | Control 4 |
| **Delantero** | `front_fan` | Sensor Sys Fan 3 | Control 5 |
| **RTX 5090 (Fan 1)** | `gpu_fan1` | GPU Fan 1 | NvAPI Fan Control 1 |
| **RTX 5090 (Fan 2)** | `gpu_fan2` | GPU Fan 2 | NvAPI Fan Control 2 |

## 3. Integración con NVIDIA GeForce RTX 5090
Para la tarjeta gráfica de última generación RTX 5090:
- Se utiliza la API nativa de NVIDIA mediante `NvAPIWrapper.dll`.
- Permite la lectura y el comando directo del porcentaje de rotación de cada ventilador independiente de la GPU sin necesidad de mantener abierto MSI Afterburner ni el panel de control de NVIDIA.
