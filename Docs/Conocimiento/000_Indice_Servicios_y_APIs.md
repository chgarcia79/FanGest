---
tipo: moc
resumen: Endpoints internos propios, controladores backend, microservicios y background workers.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - apis
  - servicios
---

# 000 Indice Servicios y APIs

> [!NOTE]
> Endpoints internos propios, controladores backend, microservicios y background workers.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "apis" OR contains(tags, "servicios")
SORT fecha_modificacion DESC
``