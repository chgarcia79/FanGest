---
tipo: patron
resumen: "Patrón de persistencia en silicio con System Tray: cómo FanGest retiene las órdenes PWM en el chip SuperIO mientras apaga los subprocesos de sondeo."
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: ["[[000_Indice_Arquitectura_y_Diseno]]", "[[000_Indice_Optimizaciones_y_Rendimiento]]"]
tecnologias: [C#, WinForms, NotifyIcon, ContextMenuStrip, Win32 API]
archivos_claves: ["T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Src/DESKTOP/FanGest/Program.cs"]
prioridad_busqueda: 8
tags: [patron, systray, notifyicon, persistencia-silicio, lifecycle]
criticidad: "Media"
resolucion: "Resuelto"
---

# Patrón de Persistencia en Silicio con System Tray

## 1. Fundamento Físico del Hardware
A diferencia de los procesadores o la memoria RAM, los circuitos controladores de ventiladores (SuperIO / ASIC de placa base) disponen de registros PWM dedicados que funcionan de forma autónoma en hardware:
- Cuando una aplicación escribe un valor en el registro de control de ciclo de trabajo (*Duty Cycle*) mediante la llamada `Control.SetSoftware(valor)`, el silicio mantiene dicho ciclo de conmutación de voltaje de forma continua.
- **No es necesario reiterar la orden**. El ventilador seguirá girando a esa velocidad exacta hasta que se escriba un nuevo valor o se ordene el reset al modo automático.

## 2. Ciclo de Vida en FanGest
El diseño de `FanGest` aprovecha esta propiedad física para eliminar por completo el consumo de CPU y la latencia:

```csharp
protected override void OnResize(EventArgs e)
{
    base.OnResize(e);
    if (WindowState == FormWindowState.Minimized)
    {
        Hide();
        _pollTimer.Stop(); // CERO LATENCIA: Detención total del sondeo
        _trayIcon.Visible = true;
    }
}
```

Al restaurar la ventana desde la bandeja del sistema:
```csharp
private void RestoreFromTray()
{
    Show();
    WindowState = FormWindowState.Normal;
    _pollTimer.Start(); // Reanuda lecturas solo para pintar la UI
    BringToFront();
}
```

## 3. Seguridad Térmica en la Salida
Para evitar que el usuario cierre la aplicación y los ventiladores queden fijados accidentalmente a baja velocidad cuando la máquina se calienta posteriormente, el evento `FormClosing` garantiza la seguridad térmica:
- Si el usuario selecciona **`❌ Salir y Restaurar BIOS`**, el método de desinicialización recorre todos los canales e invoca `Control.SetDefault()`, restituyendo las curvas térmicas nativas del firmware UEFI antes de cerrar el proceso.
