---
tipo: moc
resumen: Manuales de scripts PowerShell/Python de un solo uso, migraciones y mantenimiento.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - scripts
  - operaciones
---

# 000 Indice Operaciones y Scripts

> [!NOTE]
> Manuales de scripts PowerShell/Python de un solo uso, migraciones y mantenimiento.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "script" OR contains(tags, "operaciones")
SORT fecha_modificacion DESC
``