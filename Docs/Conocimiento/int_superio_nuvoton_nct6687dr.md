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

---

## 4. Comunicación Ring 0 y Controlador `FanGest.sys`
- **Puertos de E/S Utilizados**: Puertos indexados de configuración SuperIO `0x2E` (Índice) y `0x2F` (Datos), alternativamente `0x4E/0x4F`.
- **Acceso en Modo Kernel**: En Windows x64 no es posible ejecutar instrucciones `IN`/`OUT` desde espacio de usuario (Ring 3). El binario carga y arranca temporalmente el driver de servicio `FanGest.sys`, el cual expone códigos IOCTL para leer y escribir registros de silicio protegidos.
- **Retención PWM**: Una vez que `FanGest.sys` escribe el valor de ciclo de trabajo en el registro correspondiente (ej. `Bank 0, Register 0x01` para el canal CPU), el circuito generador de señal PWM de la placa retiene la oscilación de forma autónoma.

---

## 5. Batería de Utilitarios de Diagnóstico e Inspección

| Utilitario Ejecutable | Código / Rol | Propósito Técnico |
|---|---|---|
| `DumpAllSensors.exe` | Herramienta CLI | Recorre todo el árbol de hardware y exporta a consola todos los sensores detectados con ID, tipo y valor actual. |
| `InspectControl.exe` | Herramienta CLI | Verifica qué sensores disponen de interfaz `IControl` activa y comprueba si admiten control de software. |
| `InspectNct.exe` | Herramienta CLI | Inspección de bajo nivel directa sobre los registros de hardware del chip Nuvoton NCT6687D. |
| `InspectSuperIo.exe` | Herramienta CLI | Detección y sondeo del bus ISA para validar el chip SuperIO presente en la placa base. |
| `TestBiosRestore.exe` | Herramienta Test | Envía la orden `SetDefault()` a todos los canales y comprueba si la BIOS recupera el control de las RPM. |
| `TestProbe.cs` / `.exe` | Código fuente / Test | Sonda de prueba para validar tiempos de respuesta y lectura de sensores individuales. |
| `GenerateFanIcon.exe` | Utilitario Gráfico | Generador procedimental que dibuja las aspas de ventilador vectoriales y las exporta a `fan.ico`. |

