---
tipo: moc
resumen: ConfiguraciÓn de Docker, Dokploy, CI/CD, Kestrel, Proxies Inversos y paso a producciÓn.
fecha_creacion: 2026-09-11
fecha_modificacion: 2026-09-11
related: []
estado: activo
tags:
  - moc
  - despliegues
  - devops
---

# 000 Indice Publicacion y Despliegues

> [!NOTE]
> ConfiguraciÓn de Docker, Dokploy, CI/CD, Kestrel, Proxies Inversos y paso a producciÓn.

---

## Notas Vinculadas

``dataview
TABLE resumen as "Resumen", fecha_modificacion as "Modificado"
FROM "Docs/Conocimiento"
WHERE tipo = "despliegues" OR contains(tags, "dokploy")
SORT fecha_modificacion DESC
``