# [PY_DATA]

`data` owns portable dataset references, scan plans, schema claims, geospatial claims, graph payloads, query receipts, and exchange bundles for offline Python work. This package currently contains planning and API placeholders only; future source lands directly in this folder after the planning and API pages are filled.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: columnar/query, schema/geo/AEC, graph, mesh, and exchange-bundle planning.

[API]:
- Path: `.api/README.md`
- Owns: flat `api-*.md` placeholders for data dependencies and pending exchange candidates.

[BOUNDARY]:
- Data emits portable artifacts and evidence for downstream owners.
- Durable state, store profiles, schema rails, query rails, and Rhino/GH document mutation stay outside this package.
