# [PY_COMPUTE_RULINGS]

`python/compute` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `jax` and its solver family stay interpreter-marker-gated in the root manifest — an assay `unsupported` probe against `jax` is the INTENDED gated state on the estate interpreter, never a missing admission to re-process; the gate is a wheel-absence fact for the manifest's interpreter floor, and it lifts only when the upstream `jaxlib` wheel for that floor publishes, never by dropping the marker.
- Markers seat on `jax` and each ecosystem sibling rather than on `jaxlib`, because `jax` pins `jaxlib` at an exact equality as a mandatory runtime dependency and four siblings depend on `jaxlib` directly, so no marker confined to the binary half is expressible; `jaxtyping` declares no jax dependency at all and therefore stays unmarked, and a sweep marking it mistakes a naming family for a dependency edge.
- `jaxlib` publishes wheels ONLY, with zero sdists at every release, so the Forge scientific-source-build lane can never lift this gate — the resolver has no source to hand the wrapper, a missing-native-library row buys nothing, and upstream builds it under bazel from its own repo outside any manifest-resolvable path; reading this gate as the `geopandas`/`shapely` source-build class proposes a build that cannot start.

## [02]-[SHAPE]

- `GEOMETRY_SUBJECTS` stays a hand-authored decode-end mirror of geometry's subject wire literals, never an import — S2 peers share no import edge, so the union crosses as wire data; decode-time `forbid_unknown_fields` refuses only an added subject, so a removed or renamed literal drifts silent until a set-equality gate against geometry's `SUBJECTS` export proves the mirror, and reconciling by import is the rejected move.

## [03]-[COLLAPSE]

- Solver meshes never route through the data `MeshPayload` interchange shape — `MeshExchange` reads and writes `meshio` directly, because the interchange projection deliberately discards the cell blocks, physical groups, and field data the weak-form fold keys on; a dedup sweep re-proposing the seam detour strips exactly what assembly needs.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
