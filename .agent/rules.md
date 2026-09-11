# Reglas Específicas del Proyecto FanGest

1. **Golden Template**: Seguir las directrices oficiales del Golden Template en la estructura de carpetas.
2. **Ubicación de Código**: El código fuente vive en `Src/DESKTOP/FanGest/`.
3. **Compilación Limpia**: Mantener `build.bat` sincronizado con el compilador nativo `csc.exe` x64 referenciando `LibreHardwareMonitorLib.dll`, `NvAPIWrapper.dll` y garantizar 0 errores de compilación antes de cualquier commit.
4. **Git Remoto Exclusivo**: Sincronización remota únicamente hacia la cuenta personal `chgarcia79` (`git@github.com-personal:chgarcia79/FanGest.git`).
5. **Política de Cero Latencia**: Preservar el principio de diseño de detención total de sondeo (0 polling) al minimizar a la bandeja del sistema (Systray).
6. **Documentación Viva**: Registrar lecciones técnicas y cambios en `Docs/Conocimiento/` (Zettelkasten MOC) y `Docs/Diario/`.
7. **Auto-Publicación en Producción**: Cada vez que se compile satisfactoriamente (0 errores) mediante `build.bat`, los binarios y dependencias (`FanGest.exe`, `FanGest.sys`, DLLs, `fan.ico` y herramientas de diagnóstico) deben desplegarse y publicarse automáticamente en `C:\soft\Utilidades\FanGest\`.