# [TS_DATA_API_WATLAS]

`watlas` is the xatlas UV-atlas kernel compiled to wasm: `Atlas` ingests indexed mesh declarations, segments them into charts, and packs the charts into one or more atlas pages, answering per-vertex `uv`, per-vertex `xref` back-references into the source vertex stream, and per-chart provenance. It generates texture coordinates and nothing else — no raster byte, no container, no GPU handle leaves it.

Its one consumer seam on this branch is injection: `@gltf-transform/functions` `unwrap({ watlas, ... })` takes the whole initialized module as its engine instance, exactly as `meshoptimizer` instances inject into the transform rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `watlas`
- package: `watlas` (MIT)
- module: `"type": "module"` with `main: dist/watlas.js` beside `dist/watlas.wasm`; the manifest declares NO `types` field and NO `exports` map, so the hand-maintained `dist/watlas.d.ts` resolves as the sibling declaration of the main entry and no subpath is importable — the whole module arrives under one namespace binding
- runtime: both lanes — wasm module with no native binding; the loader resolves `watlas.wasm` beside the js entry
- rail: derivative-spine engine instance; every call is synchronous after the one async `Initialize()` readiness gate
- boundary: UV generation ONLY — geometry in, chart/pack coordinates out; the caller owns writing the result into its own vertex streams

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declaration records and result views

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :------------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `MeshDecl`     | input record       | positions + optional normals/uvs/indices/material ids per mesh   |
|  [02]   | `UvMeshDecl`   | input record       | uv-only ingest for repack of an already-parameterized mesh       |
|  [03]   | `ChartOptions` | policy record      | segmentation weights, cost/iteration bounds, `useInputMeshUvs`   |
|  [04]   | `PackOptions`  | policy record      | `resolution`, `padding`, `texelsPerUnit`, `bruteForce`, rotation |
|  [05]   | `Mesh`         | result view        | `getVertex(i) -> Vertex`, `getIndexArray`, `getChart`, counts    |
|  [06]   | `Vertex`       | result record      | `atlasIndex`, `chartIndex`, `uv: [number, number]`, `xref`       |
|  [07]   | `Chart`        | result view        | `getFaceArray`, `faceCount`, `atlasIndex`, `type`, `material`    |
|  [08]   | `ChartType`    | numeric vocabulary | `Planar \| Ortho \| LSCM \| Piecewise \| Invalid`                |

- `Vertex.xref` indexes the ORIGINAL vertex stream — atlasing splits vertices along chart seams, so the output vertex count exceeds the input's and every output attribute rebuilds by `xref` gather, never by positional copy.
- `MeshDecl` strides are BYTE strides (`vertexPositionStride` for a tight `Float32Array` position stream is 12), and `indexData` admits `Uint32Array | Uint16Array`.
- `ChartType.Invalid` marks a chart the segmenter could not parameterize; a result carrying one is evidence, not a throw.
- `Atlas` is declared with a `new(): Atlas` INSTANCE member beside its implicit zero-argument constructor — a hand-typing artifact of the manual declarations, not a factory; construction is `new Atlas()` and the member is never called.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one readiness gate, one atlas class

| [INDEX] | [SURFACE]                                           | [SHAPE]      | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `Initialize() -> Promise<void>`                     | module gate  | wasm instantiation; every other call is sync after |
|  [02]   | `new Atlas()` / `atlas.delete()`                    | wasm handle  | explicit lifetime — emscripten heap, no GC         |
|  [03]   | `atlas.addMesh(decl)` / `addUvMesh(decl)`           | ingest       | stage one mesh per call before compute             |
|  [04]   | `atlas.generate(chartOptions?, packOptions?)`       | one-shot     | `computeCharts` + `packCharts` fused               |
|  [05]   | `atlas.computeCharts(o)` / `atlas.packCharts(o)`    | staged pair  | re-pack without re-segmenting                      |
|  [06]   | `atlas.getMesh(index)` / `atlas.getUtilization`     | result reads | per-input-mesh results; per-page fill ratios       |
|  [07]   | `atlas.width`/`height`/`atlasCount`/`texelsPerUnit` | getters      | packed page extent, page count, resolved density   |

- `Initialize()` must resolve before ANY `Atlas` construction — the constructor reaches into the uninstantiated wasm table otherwise; the readiness gate is the same `ready`-style proof `_CODECS` already runs for the meshopt instances.
- `atlas.delete()` is mandatory: the handle owns emscripten-heap allocations no GC reclaims, so construction brackets `Effect.acquireRelease`.
- `getMesh(index)` answers by ingest order — result mesh `i` is declaration `i`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The kernel is synchronous and CPU-bound after readiness — a large assembly parameterization stalls its thread, so the server lane runs it inside the derivative spine's existing budget bracket and never on a request path.
- Input is declaration-shaped, not document-shaped: the caller flattens each primitive to `MeshDecl` streams and rebuilds attributes from `xref` after packing; `watlas` never sees a glTF document.
- `packCharts` re-runs without `computeCharts` — resolution and padding sweeps re-pack existing charts, so a texel-density retune never pays segmentation again.

[STACKING]:
- `@gltf-transform/functions`(`.api/gltf-transform-functions.md`): `unwrap({ watlas, texcoord, overwrite, groupBy })` consumes the whole initialized module as its injected instance — `UnwrapOptions.watlas` — and owns the document-to-declaration flatten and the `xref` rebuild; the branch composes `unwrap` and never drives `Atlas` by hand against a glTF document.
- `meshoptimizer`(`.api/meshoptimizer.md`): the injected-instance lineage twin — one wasm readiness proof per module at `_io`/`_CODECS` construction, the instance published beside the consumer that binds it. The two kernels DISAGREE on stride units and share one fence file: `MeshDecl` strides count BYTES (a tight `Float32Array` position stream is 12) while every meshopt stride counts FLOAT32 ELEMENTS (the same stream is 3), so a stride copied between the two reads every fourth vertex and answers a plausible, wrong result with no error anywhere.
- `watlas` publishes readiness through `Initialize()` ALONE and carries no `supported` flag, so it stands outside a `ready`+`supported` codec roster and takes its own construction-time await; folding it into that roster reads a property the module never declares.
- `effect`(`.api/effect.md`): `Initialize()` lifts through `Effect.tryPromise` once at engine construction; `new Atlas()`/`delete()` bracket under `Effect.acquireRelease`, so a segmentation defect cannot leak the wasm heap allocation.
- `object/asset.md` `[03]-[TRANSFORM_ROWS]`: `unwrap` is one `_TRANSFORMS` row carrying `ChartOptions`/`PackOptions` as its option record per the whole-knob-surface law; the emitted document re-proves against `_ROSTER` exactly as every fold does.

[LOCAL_ADMISSION]:
- UV generation only — the atlas extent (`width`/`height`) sizes a bake TARGET; the raster plane mints the bytes, and `watlas` never touches a texel.
- One readiness proof per process: `Initialize()` races safely but the branch proves it once at engine construction beside the meshopt `ready` awaits, never per transform call.
- The result's split-vertex census (`vertexCount` growth) rides the result; a consumer assuming input-count parity mis-gathers every attribute after the first seam split.

[RAIL_LAW]:
- Package: `watlas`
- Owns: xatlas chart segmentation and packing as an injectable wasm engine — mesh/uv-mesh ingest, chart and pack policy records, per-vertex `uv`/`xref` results, per-chart provenance, page utilization
- Accept: injection into `@gltf-transform/functions` `unwrap` as the whole initialized module, `Effect.tryPromise`-gated `Initialize()`, `acquireRelease`-bracketed `Atlas` lifetime, staged `packCharts` re-runs for density sweeps
- Reject: hand-driving `Atlas` against a glTF document the `unwrap` transform already flattens, an un-deleted `Atlas` handle, a bake or raster byte minted here, per-call `Initialize()` re-proof, positional attribute copy that ignores `xref`
