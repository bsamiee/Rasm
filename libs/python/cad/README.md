# [PY_CAD]

`cad` owns exact solid modeling and neutral CAD exchange: ISO 10303 admission and sealing, the OCCT boundary-representation algebra, measurement of exact shape, and the budgeted glTF projection. Generated `CadService` is its only boundary, so every caller reaches that domain through typed requests and reference-carrying receipts rather than an import.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/faults.md): Delivers one typed refusal every owner returns and one detail every peer decodes.

[EXCHANGE]:
- [02]-[IDENTITY](.planning/exchange/identity.md): Makes two runs over identical geometry emit byte-identical STEP.
- [03]-[STEP](.planning/exchange/step.md): Reads a foreign exact file and writes one back that any peer re-reads.
- [04]-[ASSEMBLY](.planning/exchange/assembly.md): Turns a foreign product file into one document the emitter and mesher read.

[BREP]:
- [05]-[PLACEMENT](.planning/brep/placement.md): Puts wire geometry into the kernel's exact basis under one convention.
- [06]-[PROFILE](.planning/brep/profile.md): Turns a closed planar boundary into a face the generators build on.
- [07]-[SOLID](.planning/brep/solid.md): Answers every arm needing no source body with a fresh exact volume.
- [08]-[BOOLEAN](.planning/brep/boolean.md): Combines sealed bodies exactly and reports what the kernel changed.
- [09]-[FEATURE](.planning/brep/feature.md): Names edges on a decoded body and rounds or bevels exactly those.
- [10]-[PROVENANCE](.planning/brep/provenance.md): Keeps a sub-shape addressable after an operation reseals its body.
- [11]-[OPERATION](.planning/brep/operation.md): Answers one execute request with one sealed body and its evidence.

[METROLOGY]:
- [12]-[PROPERTIES](.planning/metrology/properties.md): Measures an exact shape once and refuses a non-finite result.
- [13]-[CENSUS](.planning/metrology/census.md): Counts what the provider wrote, never what it meant to write.

[TESSELLATION]:
- [14]-[MESH](.planning/tessellation/mesh.md): Turns an exact body into drawable triangles, refusing an oversized result.
- [15]-[EMISSION](.planning/tessellation/emission.md): Writes the discrete result as a bounded artifact a caller can fetch.

[SERVICE]:
- [16]-[PROVIDER](.planning/service/provider.md): Answers a caller's request with a reference or a typed refusal.
- [17]-[LANE](.planning/service/lane.md): Keeps kernel work off the event loop and stops it when a caller walks away.
- [18]-[SPOOL](.planning/service/spool.md): Fetches what a call reads, publishes what it wrote, and cleans up either way.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`.

[CAD_KERNEL]:
- `cadquery-ocp` — OVERLAY; flat `OCP.*` topology, construction, STEP and IGES exchange, and meshing behind `service/lane#LANE`.
- `trimesh` — Decodes the emitted GLB for the placement, triangle, closure, and volume census.

[WIRE_COMPRESSION]:
- `zstandard` — Implements the zstd content coding Connect negotiates ahead of gzip for large bodies.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression` — `CadRail` carrier, `Block` traversal, and the `pipeline` fold every owner sequences through.
- `msgspec` — Frozen fault rows and evidence records pickling by reference across the worker seam.

[CONCURRENCY]:
- `anyio` — One-slot cancellable process lane and the request-deadline scope around each native fold.

[GRAPH_SUBSTRATE]:
- `networkx` — Component engine `trimesh.graph.split` dispatches to for the per-body closure verdict.

[WIRE_CODEGEN]:
- `protobuf-py` — Generated message, oneof, and binary surfaces every owner reads through the contract vocabulary.
- `connectrpc` — Generated asynchronous service protocol, ASGI application, status codes, and compression codecs.
- `rasm.contracts` — Generated CAD and artifact vocabulary, body admission, and the verified artifact lifecycle.
