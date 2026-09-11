# Registro Histórico de Conversación: FanGest (Zero-Latency Hardware Fan Manager)

- **ID de Conversación Origen**: `dc2506bf-ca87-4f96-b481-bde7cfc3f185` (Pasos 1290 a 1510)
- **Fecha de Sesión**: 27 de Agosto de 2026
- **Tecnologías Implicadas**: C# WinForms x64 (.NET Framework 4.8), LibreHardwareMonitor, NvAPIWrapper, Nuvoton NCT6687D/NCT6687DR, NVIDIA GeForce RTX 5090, System Tray P/Invoke.

---

## 1. El Problema de Arquitectura: Latencia Oculta de FanControl
El usuario planteó la siguiente limitación crítica presente en herramientas convencionales de control de ventiladores como **FanControl**:
1. **Sondeo Continuo Destructivo para la Latencia**: FanControl realiza lecturas periódicas continuas (cada 1 o 2 segundos) sobre los buses de hardware (SMBus, LPC o I2C) para refrescar temperaturas y RPM. Estas lecturas provocan bloqueos momentáneos a nivel de controlador de kernel, generando picos periódicos de latencia DPC en el sistema.
2. **Pérdida de Estado al Cerrar**: Al cerrar FanControl para eliminar el sondeo, los ventiladores regresaban inmediatamente a su estado automático predeterminado de la BIOS o quedaban en un estado no deseado.
3. **Requisito del Usuario**: Disponer de una aplicación ligera que permita fijar los ventiladores al 100% (Turbo) para sesiones de juego o devolverlos a la gestión de la BIOS a voluntad, pero con una **política estricta de CERO LATENCIA en segundo plano**.

---

## 2. Cronología de Requisitos y Decisiones Técnicas

### 2.1 Inspección y Detección de Hardware
- Mediante herramientas de sondeo preliminar (`DumpAllSensors.exe`, `InspectControl.exe`, `InspectSuperIo.exe`), se identificó la topología de la placa base (MSI MPG X870E) y su chip SuperIO:
  - **Chip SuperIO**: Nuvoton `nct6687dr` accesible mediante el controlador `FanGest.sys`.
  - **Canales de Placa**: `CPU Fan`, `Pump Fan`, `Trasero`, `Lateral`, `Delantero`.
  - **Tarjeta Gráfica**: NVIDIA GeForce RTX 5090 con control independiente de ventiladores (`GPU Fan 1`, `GPU Fan 2`) a través de `NvAPIWrapper.dll`.

### 2.2 Formulación del Patrón Zero-Polling (Cero Latencia en Systray)
- **Estado Ventana Visible**: Se activa un `System.Windows.Forms.Timer` con intervalo de 1.000 ms para actualizar etiquetas de RPM, porcentajes y sliders con el fin de proporcionar retroalimentación visual al usuario.
- **Estado Minimizado o Cerrado a Systray**: Al minimizar o pulsar la `X` de la ventana, esta se oculta en el área de notificación del reloj de Windows y **EL TEMPORIZADOR DE SONDEO SE DETIENE TOTALMENTE (`_pollTimer.Stop()`)**.
  - **Cero llamadas a sensores**.
  - **Cero accesos al bus SMBus/LPC**.
  - **0,00 ms de latencia DPC añadida**.
  - **Persistencia en Hardware**: Los registros del chip Nuvoton retienen en su silicio la orden PWM configurada sin necesidad de que ningún software esté corriendo en bucle.

### 2.3 Calibración de Comportamiento y Feedback del Usuario
- **Porcentaje Proporcional a RPM**: El usuario observó que al arrancar en modo BIOS los ventiladores no estaban al 100% sino en torno al 33%-70% según la temperatura. Se adaptó la lógica para reflejar el estado real de la placa base.
- **Modos Globales**:
  - `🌪️ TODOS AL 100% (TURBO)`: Invoca `Control.SetSoftware(100)` en todos los canales.
  - `🍃 MODO BIOS (CONTROL AUTOMÁTICO)`: Invoca `Control.SetDefault()` devolviendo la curva a la BIOS.
- **Icono en Bandeja del Sistema y Menú Contextual**:
  - Generación de icono de ventilador `fan.ico`.
  - Menú contextual con clic derecho: cambiar de modo con un clic, restaurar panel o salir restaurando la BIOS para seguridad térmica del equipo.

### 2.4 Validación de Latencia en Tiempo Real
- Con `FanGest` minimizado en Systray con ventiladores al 100%, el usuario ejecutó el analizador `NvidiaLatencyFrameTest.exe`:
  - **Resultado**: La latencia media del sistema se mantuvo en **~4,5 $\mu\text{s}$** y se eliminaron por completo los picos periódicos que antes causaba FanControl.

---

## 3. Trazabilidad de Archivos y Componentes
- **Código Fuente**: [Program.cs](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/Program.cs)
- **Compilador por Lotes**: [build.bat](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/build.bat)
- **Binario Resultante**: [FanGest.exe](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/FanGest.exe)
- **Driver de Acceso a Silicio**: [FanGest.sys](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/FanGest.sys)
- **Notas de Conocimiento Asociadas**:
  - [[arq_fangest_zero_latency]]
  - [[int_superio_nuvoton_nct6687dr]]
  - [[patron_systray_hardware_persistence]]
