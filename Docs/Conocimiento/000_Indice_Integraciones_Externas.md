---
tipo: moc
resumen: ComunicaciÃ³n con APIs y servicios de terceros (Vapi, Google Drive, NotebookLM, MCPs, etc.).
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - integraciones
---

# 000 Indice Integraciones Externas

> [!NOTE]
> ComunicaciÃ³n con APIs y servicios de terceros (Vapi, Google Drive, NotebookLM, MCPs, etc.).

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "integraciones" OR contains(tags, "mcp")
SORT fecha_modificacion DESC
``