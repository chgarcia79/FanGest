---
tipo: moc
resumen: Epics, caracterÍsticas mayores y mÓdulos completos finalizados en producciÓn.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - implementaciones
---

# 000 Indice Implementaciones y Modulos

> [!NOTE]
> Epics, caracterÍsticas mayores y mÓdulos completos finalizados en producciÓn.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "implementacion" OR contains(tags, "modulo")
SORT fecha_modificacion DESC
``