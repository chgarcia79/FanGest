---
tipo: moc
resumen: Cementerio de errores crÃ³nicos resueltos, post-mortems y resoluciÃ³n de fallos conocidos.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - bugs
  - troubleshooting
---

# 000 Indice Bugs y Troubleshooting

> [!NOTE]
> Cementerio de errores crÃ³nicos resueltos, post-mortems y resoluciÃ³n de fallos conocidos.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "bug" OR contains(tags, "troubleshooting")
SORT fecha_modificacion DESC
``