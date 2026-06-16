# [PY_ARTIFACTS]

`artifacts` owns artifact production: one polymorphic document/PDF/Office/structured-text plan, one VisualSpec to ExportPlan axis spanning 2D charts and 3D scientific visualization, one report-templating composition owner, one preview owner, and one compression owner. Content identity and the bundle spine are consumed from runtime `ContentIdentity`, never re-minted. This package currently contains planning and API evidence only; future source lands directly in this folder.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: the two owner pages (documents, visual-export) carrying transcription-complete signature fences.

[API]:
- Path: `.api/README.md`
- Owns: the `api-*.md` evidence for artifacts dependencies; 19 are cp315-reflected, the pillow-blocked six and the native-floor `pyvista`/`vtk` carry the capture gap.

[BOUNDARY]:
- Live UI controls, dashboard runtime, browser state, product artifact stores, and AppUi evidence timelines stay outside this package.
- IFC tessellation/GLB belongs to `geometry`; geospatial/mesh-file/columnar interchange belongs to `data`.
- `pyvista`/`vtk` are 3D scientific viz feeding the visualization surface, reassigned here from the former data catalogue.
- All emitted files carry one runtime content key.
