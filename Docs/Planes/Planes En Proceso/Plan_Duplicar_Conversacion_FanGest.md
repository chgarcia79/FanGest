---
estado: en proceso
resolucion: 
fecha_creacion: 2026-09-11
fecha_finalizacion: 
trazabilidad: 
archivos_claves:
  - T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Docs/Planes/Planes En Proceso/Plan_Duplicar_Conversacion_FanGest.md
  - T:/DESARROLLO/GCLOUD/PROYECTOS/Scripts/duplicar_conversacion_fangest.cjs
  - T:/DESARROLLO/GCLOUD/PROYECTOS/Scripts/Duplicar_Conversacion_FanGest.bat
---

# Plan: Duplicar Conversación Histórica "FanGest & Debloat Drivers NVIDIA" hacia el Proyecto FanGest

## 0. Especificación y Alcance (Spec-Driven)
- **Objetivo funcional (El QUÉ y el PARA QUÉ):** Clonar de forma segura e independiente la conversación histórica original `dc2506bf-ca87-4f96-b481-bde7cfc3f185` (con título "FanGest & Debloat Drivers NVIDIA", 2.149 pasos y artefactos) para que aparezca disponible tanto en el proyecto FanGest (`file:///t:/DESARROLLO/GCLOUD/PROYECTOS/FanGest`) como en Optimizaciones NVIDIA (`file:///k:/LaterncyTestNVIDIA`), permitiendo acceder y continuar el hilo en ambos proyectos de forma autónoma.
- **Fuera de alcance (Non-Goals):**
  - No se modificará ni eliminará la conversación existente en `K:\LaterncyTestNVIDIA`.
  - No se ejecutarán modificaciones sobre los archivos de código fuente de FanGest ni de Optimizaciones NVIDIA.
  - No se ejecutará la clonación en caliente mientras Antigravity esté abierto para evitar corrupción de base de datos o sobreescritura de índices en memoria.
- **Supuestos y Decisiones Validadas:**
  - El usuario confirmó vincular la copia duplicada al proyecto FanGest (`T:\DESARROLLO\GCLOUD\PROYECTOS\FanGest`).
  - El usuario confirmó preservar el título original: `FanGest & Debloat Drivers NVIDIA`.
  - Se creará un script Node.js determinista (`duplicar_conversacion_fangest.cjs`) y su lanzador `.bat` (`Duplicar_Conversacion_FanGest.bat`) en `Scripts/` con verificación de procesos cerrados (`Antigravity.exe` y `language_server.exe`), idéntico al exitoso patrón de `Vincular_LaterncyTestNVIDIA.bat`.
- **Matriz de Dependencias Técnicas:**
  1. Generación de nuevo UUID para la conversación duplicada y asignación de ID de proyecto único para FanGest.
  2. Script Node.js con manipulación binaria Protobuf pura (`encodeProto`/`decodeProto`) y SQLite sincrónico (`node:sqlite`).
  3. Clonación de base de datos SQLite (`conversations/<newId>.db`) y actualización de tablas `trajectory_meta` y `trajectory_metadata_blob`.
  4. Clonación íntegra de artefactos y transcripts en disco (`brain/<newId>/`).
  5. Inserción de la nueva entrada en `agyhub_summaries_proto.pb` con backup preventivo (`.bak`).

---

## 1. Fases de Ejecución

### Fase 1: Creación del Script de Duplicación Node.js
- Archivo: [duplicar_conversacion_fangest.cjs](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/Scripts/duplicar_conversacion_fangest.cjs)
- Validaciones y pasos del script:
  1. Comprobación estricta de procesos activos (`Antigravity.exe`, `language_server.exe`).
  2. Generación determinista o dinámica de `newConvId` y `fangestProjectId`.
  3. Copia física de `C:\Users\Chgar\.gemini\antigravity\conversations\dc2506bf-ca87-4f96-b481-bde7cfc3f185.db` a `conversations\<newConvId>.db`.
  4. Modificación de `trajectory_meta` actualizando `cascade_id = <newConvId>`.
  5. Modificación de `trajectory_metadata_blob` actualizando `folderUri` a `file:///t:/DESARROLLO/GCLOUD/PROYECTOS/FanGest`, `encodedFolderUri` a `file:///t%3A/DESARROLLO/GCLOUD/PROYECTOS/FanGest`, `convId` a `<newConvId>`, y `projectId` a `fangestProjectId`.
  6. Copia recursiva de `brain\dc2506bf-ca87-4f96-b481-bde7cfc3f185\` a `brain\<newConvId>\`.
  7. Creación de backup `.bak_pre_duplicate` de `agyhub_summaries_proto.pb`.
  8. Inserción de la nueva entrada en `agyhub_summaries_proto.pb` preservando la existente de LatencyTestNVIDIA.

### Fase 2: Creación del Ejecutable Lanzador (.bat)
- Archivo: [Duplicar_Conversacion_FanGest.bat](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/Scripts/Duplicar_Conversacion_FanGest.bat)
- Codificación UTF-8 pura (`chcp 65001`), invocación transparente de Node.js y mensajes claros en terminal para el usuario.

### Fase 3: Réplica en la Carpeta de Scripts de FanGest
- Ubicación: [Duplicar_Conversacion_FanGest.bat](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Scripts/Utils/Duplicar_Conversacion_FanGest.bat) para acceso directo desde el propio repositorio de FanGest.

### Fase 4: Registro en el Diario y Zettelkasten
- Actualización de [2026-09-11.md](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Docs/Diario/2026/09/2026-09-11.md) registrando la creación del plan y la utilidad de duplicación.
- Documentación técnica en [Conversaciones/Historico_FanGest.md](file:///T:/DESARROLLO/GCLOUD/PROYECTOS/FanGest/Conversaciones/Historico_FanGest.md).

---

## 2. Plan de Verificación
- **Prueba en Seco**: Verificar la validez de las funciones de codificación y decodificación Protobuf sobre los blobs sin colisiones.
- **Validación Estructural**: Comprobar que los archivos generados en `Scripts/` respeten la política de codificación sin MojiBake.
- **Instrucciones de Ejecución**: Guiar al usuario paso a paso para cerrar Antigravity y ejecutar el `.bat`, confirmando la aparición de la conversación en ambos workspaces.
