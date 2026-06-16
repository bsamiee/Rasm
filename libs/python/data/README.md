# [PY_DATA]

`data` owns portable data interchange: typed dataset refs, columnar lazy/streaming scan + egress, query plans across engines, schema claims + a data-contract validation gate, the vector AND raster geospatial axes, graph payloads, and mesh-file exchange. Content identity and the egress bundle spine are consumed from runtime `ContentIdentity`, never re-minted. This package currently contains planning and API evidence only; future source lands directly in this folder.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: the three owner pages (columnar-query, schema-geo, graph-mesh) carrying transcription-complete signature fences.

[API]:
- Path: `.api/README.md`
- Owns: the `api-*.md` evidence for data dependencies; `networkx` is cp315-reflected, the rest carry the wheel-floor capture gap.

[BOUNDARY]:
- Durable stores, schema migrations, product repositories, and query rails stay in `Rasm.Persistence`; data emits portable import/export bundles.
- IFC tessellation, registration, topology, and AEC geometry belong to `geometry`; the numeric trio and labelled-array compute belong to `compute`; remote-stream transport is a runtime `TransportResource` row.
- All emitted bundles carry one runtime content key.
