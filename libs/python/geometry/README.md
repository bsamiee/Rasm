# [PY_GEOMETRY]

`geometry` owns geometry + IFC/BIM interchange and is the load-bearing cross-boundary package of the branch: the IfcOpenShell tessellation companion daemon (IFC to mesh/GLB + semantic XML/JSON), IFC property/quantity/relationship analysis, point-cloud/3D-scan registration and reconstruction, non-manifold topological modeling, and AEC computational geometry. It consumes the runtime `ServerHost`, `ContentIdentity`, rails, lanes, and `ReceiptContributor`. This package currently contains planning and API evidence only; future source lands directly in this folder. The package is pinned under a SEPARATE companion interpreter floor (`python_version<'3.13'`) divorced from the `>=3.15` runtime floor.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: the four owner pages (ifc-companion, ifc-analysis, scan-processing, geometry-algebra) carrying transcription-complete signature fences.

[API]:
- Path: `.api/README.md`
- Owns: the cp312-verified `api-*.md` evidence for the five geometry distributions; the surface is verified spelling reflected on the companion interpreter floor.

[BOUNDARY]:
- C# owns IFC semantic in-process (GeometryGym) and glTF in-process (SharpGLTF); the companion is purely the tessellation hop the managed surface cannot perform.
- The companion speaks the EXISTING C# `ComputeService`/`ArtifactSync` gRPC contract through the runtime `ServerHost`; it mints no transport and no second wire vocabulary.
- LGPL-3.0-or-later is satisfied at the process boundary by the isolated companion environment keeping the copyleft wheel out of the MIT/Apache library lock.
- Mesh-file exchange and remote AEC streams stay in data/runtime; 3D scientific visualization stays in artifacts.
