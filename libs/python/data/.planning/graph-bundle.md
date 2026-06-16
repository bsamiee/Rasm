# [PY_DATA_GRAPH_BUNDLE]

Graph and bundle ownership captures network-shaped data, mesh payloads, and exchange packages without turning data into a product state owner.

## [1]-[GRAPH_OWNER]

[GRAPH_PAYLOAD]:
- Owns: graph kind, node ids, edge ids, attributes, directionality, and algorithm result metadata.
- API route: `.api/api-networkx.md`.
- Output: graph payload record and algorithm receipt.
- Boundary: NetworkX never becomes repository ownership discovery, route mapping, or product collaboration state.

[GRAPH_EGRESS]:
- Owns: graph conversion to tabular, JSON, GraphML-like, or artifact bundle forms.
- Packages: `networkx`, `pyarrow`, `polars`.
- Output: exchange bundle with schema claim and optional artifact digest.
- Boundary: no product graph database or route graph.

## [2]-[MESH_OWNER]

[MESH_PAYLOAD]:
- Owns: mesh file identity, cell/block topology, units, metadata, and preview-export routing.
- API routes: `.api/api-meshio.md`, `.api/api-trimesh.md`, `.api/api-h5py.md`.
- Output: mesh exchange bundle and artifact route.
- Boundary: no simulation runtime, no C# geometry kernel replacement, no AppUi render surface.

[AEC_PENDING]:
- Owns: pending evidence for IFC, Topologic, Open3D, VTK, PyVista, Rasterio, COMPAS, and Speckle lanes.
- API routes: pending `.api/api-<distribution>.md` folders.
- Output: owner-pressure notes only.
- Boundary: pending packages do not enter source until the root manifest and package-local plan admit them.

## [3]-[SPLIT_PRESSURE]

[EXCHANGE_PACKAGE]:
- Split trigger: AEC/geospatial/document interchange gains independent receipts, multiple file families, and a named consumer outside `data`.
- Stay merged when: exchange remains dataset egress plus schema/geo/AEC claims.

## [4]-[RED_TEAM]

- Reject graph code that becomes product route discovery.
- Reject mesh code that becomes a renderer or solver runtime.
- Reject pending AEC packages in source without `.api` evidence and root manifest admission.
- Reject bundle emission without content identity and provenance fields.
