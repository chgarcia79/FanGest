---
tipo: moc
resumen: Archivo de cÓdigo o documentaciÓn obsoleta mantenida por valor histÓrico.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - legacy
---

# 000 Indice Legacy y Archivo

> [!NOTE]
> Archivo de cÓdigo o documentaciÓn obsoleta mantenida por valor histÓrico.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "legacy" OR contains(tags, "archivo")
SORT fecha_modificacion DESC
``