# [PY_COMPUTE_RULINGS]

`python/compute` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `jax` and its solver family stay interpreter-marker-gated in the root manifest — an assay `unsupported` probe against `jax` is the INTENDED gated state on the estate interpreter, never a missing admission to re-process; the gate is a wheel-absence fact for the manifest's interpreter floor, and it lifts only when the upstream `jaxlib` wheel for that floor publishes, never by dropping the marker.
- Markers seat on `jax` and each ecosystem sibling rather than on `jaxlib`, because `jax` pins `jaxlib` at an exact equality as a mandatory runtime dependency and four siblings depend on `jaxlib` directly, so no marker confined to the binary half is expressible; `jaxtyping` declares no jax dependency at all and therefore stays unmarked, and a sweep marking it mistakes a naming family for a dependency edge.
- `jaxlib` publishes wheels ONLY, with zero sdists at every release, so the Forge scientific-source-build lane can never lift this gate — the resolver has no source to hand the wrapper, a missing-native-library row buys nothing, and upstream builds it under bazel from its own repo outside any manifest-resolvable path; reading this gate as the `geopandas`/`shapely` source-build class proposes a build that cannot start.
- `proxsuite` is refused at admission on the interpreter floor — it publishes no cp315 wheel and its `cmeel` build backend itself dies on py3.15 metadata (`KeyError: 'license'` importing `cmeel.cmeel`) before any compile starts, so the Forge source-build lane cannot lift it and the convex `Backend` axis carries no ProxQP row; re-opens on a cp315 `proxsuite` wheel or a `cmeel` release carrying py3.15 metadata handling.
- `cvxpy` canonicalization pins `canon_backend=cp.SCIPY_CANON_BACKEND` at every solve site — the floor's source-built CPP canon extension trips a fatal `ProblemData.hpp` assert on EVERY canonicalization, LP through SDP alike, an uncatchable process abort on the solve lane, while the SciPy path canonicalizes every family clean; re-opens on an upstream cp315 release wheel, never by dropping the pin against the source build.

## [02]-[SHAPE]

- The drift-envelope container layout is single-writer law at the C# ingest fence — `csharp:Rasm.Compute/Model/identity#MODEL_IDENTITY` `GraduationEnvelope.Admit(HdfHandle)` — and the python writer at `experiments/model#ENVELOPE` hand-copies it as a deliberate non-import mirror, seated at the model owner because only it holds the training columns the bands fit from; it rides neither the `[02.27]` field-container entry (different layout, different seam) nor a graduation-page owner, and the crossing owes its own `tests/contracts/MANIFEST.md` entry with the python side the producer; re-opens only if the ingest fence moves.
- The sparse exchange reads and writes BOTH containers — `.mtx` for SuiteSparse interop and the scipy-convention HDF5 archive whose attributes carry the reproduction policy `.mtx` drops — because the C# factor lane landed both directions of each at `Tensor/factor#SPARSE_SOLVE`; the group convention is that fence's law hand-copied at `solvers/linear#EXCHANGE`, the int32 index pin is the exchange law (the peer reader declares int32 dataset reads), and a sparse operator is solver currency that never routes through a gridded-plane page.
- `GEOMETRY_SUBJECTS` stays a hand-authored decode-end mirror of geometry's subject wire literals, never an import — S2 peers share no import edge, so the union crosses as wire data; decode-time `forbid_unknown_fields` refuses only an added subject, so a removed or renamed literal drifts silent until the `mirror_aligned` hub gate — fed geometry's `SUBJECTS`/`WIRE_FIELDS`/`LINK_KIND` exports by a composing root — proves the mirror, and reconciling by import is the rejected move.
- Classical coded designs, the `factorial` grid included, map LINEARLY onto each axis's `bounds` through the study `_unit`/`_box` fold, never through the marginal ppf — a coded design is box-geometric and the ppf's 0/1 tails are unbounded, so a NORM axis would seat corner runs at ±inf; the qmc and SALib sampling paths keep the ppf `rescale` because their draws are interior.
- The graduation-evidence reverse bundle decodes ONE wire — the peer's canonical UTF-8 JSON under the `ComputeWireContext` CamelCase policy with the six locked `kind` literals — and `bundle_key` crosses as the bare 32-hex render under the estate x32 content-key law, never a raw integer column; `codegen` re-mints a second `WireFormat` arm only behind a producer that emits it.
- A compute audit verb spells `<EVIDENCE_DOMAIN>.<operation>` off the handoff-derived segment, never a per-page literal, so a durable row greps against the series its evidence twin records under; `MeterFact` carries `REGULATORY` by constitution and every audit class is fixed by what its producer WRITES rather than by which axis produced it, so no `RETENTION` table keyed on `Domain`/`HandoffAxis` is admitted — it would carry one value per row and decide nothing. Recurring wrong move: a retention table minted for symmetry with the governed-ceiling precedent.

## [03]-[COLLAPSE]

- Solver meshes never route through the data `MeshPayload` interchange shape — `MeshExchange` reads and writes `meshio` directly, because the interchange projection deliberately discards the cell blocks, physical groups, and field data the weak-form fold keys on; a dedup sweep re-proposing the seam detour strips exactly what assembly needs.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
