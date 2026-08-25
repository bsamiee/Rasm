# [PY_CAD]

`cad` owns exact solid modeling and neutral CAD exchange: ISO 10303 and IGES admission and sealing, the OCCT boundary-representation algebra with its healing and local-feature families, measurement of exact shape down to the inertia tensor, and the budgeted glTF projection with its per-placement identity roster. Generated `CadService` is its only boundary, so every caller reaches that domain through typed requests and reference-carrying receipts rather than an import.

## [01]-[ROUTER]

- [01]-[FAULTS](.planning/faults.md): Delivers one typed refusal every owner returns and one detail every peer decodes.

[EXCHANGE]:
- [02]-[IDENTITY](.planning/exchange/identity.md): Makes two runs over identical geometry emit byte-identical STEP and IGES.
- [03]-[STEP](.planning/exchange/step.md): Reads a foreign exact file and writes one back that any peer re-reads.
- [04]-[IGES](.planning/exchange/iges.md): Reads and writes the surface-form neutral file under the metre regime the writer ignores.
- [05]-[ASSEMBLY](.planning/exchange/assembly.md): Turns a foreign product file into one document and one identity roster.

[BREP]:
- [06]-[REGIME](.planning/brep/regime.md): Fixes the kernel tolerance vocabulary and the parallel-custody grant every kernel reads.
- [07]-[PLACEMENT](.planning/brep/placement.md): Puts wire geometry into the kernel's exact basis under one convention.
- [08]-[PROFILE](.planning/brep/profile.md): Turns a closed planar boundary into a face the generators build on.
- [09]-[SOLID](.planning/brep/solid.md): Answers every arm needing no source body with a fresh exact volume.
- [10]-[BOOLEAN](.planning/brep/boolean.md): Combines sealed bodies exactly and reports what the kernel changed.
- [11]-[FEATURE](.planning/brep/feature.md): Names edges or faces on a decoded body and reshapes exactly those.
- [12]-[HEALING](.planning/brep/healing.md): Repairs an imported body under stated steps and reports what moved.
- [13]-[PROVENANCE](.planning/brep/provenance.md): Keeps a sub-shape addressable after an operation reseals its body.
- [14]-[OPERATION](.planning/brep/operation.md): Answers one execute request with one sealed body and its evidence.

[METROLOGY]:
- [15]-[PROPERTIES](.planning/metrology/properties.md): Measures an exact shape once, inertia included, and refuses a non-finite result.
- [16]-[CENSUS](.planning/metrology/census.md): Counts what the provider wrote, never what it meant to write.

[TESSELLATION]:
- [17]-[MESH](.planning/tessellation/mesh.md): Turns an exact body into drawable triangles, refusing an oversized result.
- [18]-[EMISSION](.planning/tessellation/emission.md): Writes the discrete result as a deterministic artifact a caller can fetch.

[SERVICE]:
- [19]-[PROVIDER](.planning/service/provider.md): Answers a caller's request with a reference or a typed refusal.
- [20]-[LANE](.planning/service/lane.md): Keeps kernel work off the event loop and stops it when a caller walks away.
- [21]-[SPOOL](.planning/service/spool.md): Fetches what a call reads, publishes what it wrote, and cleans up either way.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; admission rows ride the workspace manifests as bare names, `uv.lock` fixes every version, and this folder's `.api/` corroborates.

[CAD_KERNEL]:
- `cadquery-ocp` — OVERLAY; flat `OCP.*` topology, construction, healing, STEP and IGES exchange, and meshing behind `service/lane#LANE`.
- `trimesh` — Decodes the emitted GLB for the placement, triangle, closure, volume, and node-name census.

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
- `networkx` — Component engine the census pins under `trimesh.graph.split` for the per-body closure verdict.

[WIRE_CODEGEN]:
- `protobuf-py` — Generated message, oneof, descriptor, and binary surfaces every owner reads through the contract vocabulary.
- `connectrpc` — Generated asynchronous service protocol, ASGI application, status codes, detail decode, and compression codecs.
- `rasm.contracts` — Generated CAD, fault, and artifact vocabulary; runtime `transport/artifact` owns its custody.
