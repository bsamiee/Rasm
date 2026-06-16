# [UI_PLANNING]

`ui` owns the browser UI/UX/components concern ONLY — the AppUi-analog library of the TS branch. It is the single sanctioned `AtomBinding` reactive-binding spine, the leaf render-surfaces over the decoded wire vocabulary, and the interaction-role component-system vocabulary. It holds no domain state and authors no decode: every domain read flows through a `projection` store and the one `AtomBinding`, every geometry read through the `interchange` `GeometryRail`, and every mutation leaves only through the `interchange` `CommandGateway`. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. `ui` is the LOWER browser-stratum library beneath the `platform` AppHost entry — `ui/**` must NEVER import `platform/**`; the `platform` CompositionRoot composes this library, never the reverse.

## [1]-[PAGE_INDEX]

Per the single-state-surface law, page-finalization state lives in exactly one cell — this column. A page flips to finalized only on a cold all-PASS sweep of the re-derived corpus.

| [INDEX] | [PAGE]              | [OWNS]                                                                                                                       | [STATE]     |
| :-----: | :------------------ | :-------------------------------------------------------------------------------------------------------------------------- | :---------- |
|   [1]   | binding.md          | AtomBinding + DeepLinkBinding + UndoStack + OfflineState + the dev-build atom inspector                                      | provisional |
|   [2]   | render-surfaces.md  | the EvidenceTimelineRoute/BenchmarkRoute/CollectorPanel observation routes + GeoSeriesSurface over one GeoSeriesLayer union + GlbViewport WebGL mesh render | provisional |
|   [3]   | component-system.md | the InteractionRole Schema.Literal owner-block + RoleBehavior + ThemeTokens + CssVarSync                                     | provisional |

## [2]-[WIRE_PAGES]

`ui` authors no wire shape and transcribes no `#TS_PROJECTION` fence directly; it consumes two C# wire pages only as the CONSEQUENCE end of a seam — the receipt and evidence shapes reach a render leaf through a `projection` store, and the geometry projection reaches `GeoSeriesSurface` through the `interchange` `GeometryRail`. The seam mechanics live in the [branch ledger](../../.planning/region-map/seam-splits.md).

| [C# WIRE PAGE]                  | [CODEC]  | [UI CONSUMER]                                                                                |
| :------------------------------ | :------- | :------------------------------------------------------------------------------------------ |
| Compute/receipts-and-benchmarks | json-stj | render-surfaces#RENDER_SURFACES (`BenchmarkRoute`, fingerprint-gated, over `ReceiptStore`)   |
| AppUi/diagnostics-evidence      | json-stj | render-surfaces#RENDER_SURFACES (`EvidenceTimelineRoute` + `SkewBand`, over `EvidenceFeed`)  |
| Persistence/snapshot-codecs     | messagepack | render-surfaces#RENDER_SURFACES (`GeoSeriesSurface` over the `GeometryRail` GeoJSON projection) |
| Compute/remote-lane             | proto       | render-surfaces#GLB_VIEWPORT (`GlbViewport` over the `ArtifactFrameRail` blob; mesh-decode seam BLOCKED on the not-yet-promoted `GeometryPayload(mesh)`/`MeshTensor` shape) |

## [3]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                                  | [CLOSED_BY (page#cluster)]                                                              |
| :-----: | :--------------------------------------------------------------------- | :-------------------------------------------------------------------------------------- |
|   [1]   | a second React state binding proliferating beside the atom layer       | binding#BINDING (one `AtomBinding`, the sole sanctioned binding under the collapse scan) |
|   [2]   | domain data held in local component state instead of a fold            | binding#BINDING (`AtomBinding` leaf subscription; local domain state is the named defect) |
|   [3]   | url/undo/offline hand-rolled as a parallel client-state library        | binding#BINDING (`DeepLinkBinding`/`UndoStack`/`OfflineState` folds under the one binding) |
|   [4]   | an unverifiable benchmark claim displayed as verified                  | render-surfaces#RENDER_SURFACES (`BenchmarkRoute` fingerprint gate)                      |
|   [5]   | the geo surface re-decoding geometry the rail already admitted         | render-surfaces#RENDER_SURFACES (`GeoSeriesSurface` reads only the `interchange` `GeometryRail`) |
|   [6]   | four loose geo aliases relocated instead of collapsed                  | render-surfaces#RENDER_SURFACES (one `GeoSeriesLayer` union owner)                       |
|   [7]   | a `.tsx`-per-component library instead of a role vocabulary            | component-system#COMPONENT_SYSTEM (one `InteractionRole` owner-block + `RoleBehavior`)   |
|   [8]   | a theme written direct to `document.documentElement.style`             | component-system#COMPONENT_SYSTEM (`CssVarSync` the single theme-to-runtime path)        |
|   [9]   | a `ui->platform` import inverting the AppUi/AppHost direction           | the `ui/**`-specific import-ban rule in the centralized config (RULE_ENFORCEMENT row [7]) |
|  [10]   | the GLB viewport re-decoding the mesh bytes beside the rail / a free GL ref | render-surfaces#GLB_VIEWPORT (`GlbViewport` reads only the `ArtifactFrameRail` blob; the GL context is one `Effect.acquireRelease` resource) |
|  [11]   | a parallel viewport-camera gesture handler beside the role vocabulary    | render-surfaces#GLB_VIEWPORT (`ViewportCamera` is one `RoleBehavior` row on `core`/`gesture`; the `CameraGesture` `Data.TaggedEnum` folds total) |

## [4]-[DENSITY_BAR]

A new feature is a row or case, never a new surface. The owner-finalization axis (`FINALIZED|SPIKE|BLOCKED`) lives only in this column per the single-state-surface law; `BLOCKED` marks an owner whose fence is authored but whose seam is gated on an unmet cross-branch precondition. `ui` owns NO app-service — the closed five-app-service budget (`WireClients`/`CommandGateway` in `interchange`; `SnapshotFeed`/`RuntimeFeed`/`EvidenceFeed` in `projection`) holds none of these owners; every `ui` owner is a browser-stratum render/binding/vocabulary owner.

| [INDEX] | [AXIS/CONCERN]            | [OWNER]                            | [KIND]                  | [CASES]                                                                                  | [STATE]   |
| :-----: | :------------------------ | :--------------------------------- | :---------------------- | :--------------------------------------------------------------------------------------- | :-------- |
|   [1]   | sanctioned state binding  | `AtomBinding`                      | React binding + folds   | `useAtomValue`/`useAtomSet`/`useAtomSuspense` over `@effect-atom` + the dev-build atom inspector as one row | FINALIZED |
|   [2]   | url-resident state        | `DeepLinkBinding`                  | query-string fold       | one `Schema`-round-tripped key per surface, intents from stable string keys, survives reload | FINALIZED |
|   [3]   | client-state folds        | `UndoStack` + `OfflineState`       | Effect-native folds     | bounded undo/redo/push history fold + the last-good offline cell read from `LocalPersistence` | FINALIZED |
|   [4]   | observation routes        | `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel` | read-only leaves | HLC-ordered evidence + `SkewBand`, fingerprint-gated benchmark, telemetry collector reader | FINALIZED |
|   [5]   | cartographic surface      | `GeoSeriesSurface` / `GeoSeriesLayer` | Schema.Union owner   | base (maplibre style-spec) vs overlay (`featureKind` × `overlayMode` `@deck.gl/react` `DeckGL` layer set over the maplibre child) | FINALIZED |
|   [6]   | component vocabulary      | `InteractionRole` + `RoleBehavior` + `ThemeTokens` | Schema.Literal owner-block | eight roles × headless behavior + color-space tokens + `CssVarSync` runtime sync         | FINALIZED |
|   [7]   | mesh render leaf          | `GlbViewport` / `RendererBackend`  | Schema.Literal backend axis | three/babylon/model-viewer/webgpu backend × in-memory `MeshView` upload/draw fold over the `ArtifactFrameRail` blob + the `ViewportCamera` `RoleBehavior` row on `core`/`gesture`; mesh-decode seam BLOCKED on upstream `GeometryPayload` promotion | BLOCKED |

`GlbViewport` is the row [7] DENSITY_BAR owner with its backend/draw/camera owners authored against the in-memory `MeshView`, but its mesh-DECODE seam is BLOCKED: the upstream C# `remote-lane#TS_PROJECTION` promotion of `GeometryPayload(mesh)`/`MeshTensor` out of `[PROTO_VOCABULARY]` (the single true blocker per `region-map/seam-splits.md` line 50) has NOT landed, so the decode consumes the generated descriptor by reference only once that fence exists. State stays BLOCKED, not SPIKE, until `(a)` physically promotes the mesh wire type; the WebGL packages are admitted-on-precondition.

## [5]-[BUILD_ORDER]

The binding precedes every surface that subscribes through it; the component-system precedes nothing it does not read; render-surfaces compose both.

| [INDEX] | [FILE]                  | [TRANSCRIBES]                        | [GATE]                          |
| :-----: | :---------------------- | :----------------------------------- | :------------------------------ |
|   [1]   | binding.ts              | binding#BINDING                      | tsgo `--noEmit` + unit-pbt      |
|   [2]   | component-system/       | component-system#COMPONENT_SYSTEM    | tsgo + announce-arm exhaustive  |
|   [3]   | render-surfaces.ts      | render-surfaces#RENDER_SURFACES + #GLB_VIEWPORT | tsgo + BrowserE2E DOM/navigation + GL-context acquireRelease teardown |
|   [4]   | index.ts                | the single `./ui` subpath export     | exports resolve                 |

## [6]-[PROOF_GATES]

| [GATE]          | [COMMAND]                              | [EVIDENCE]                                                  |
| :-------------- | :------------------------------------- | :--------------------------------------------------------- |
| catalog resolve | `pnpm install`                         | catalogMode strict resolves the browser-stratum admissions |
| typecheck       | tsgo `--noEmit` over the domain        | zero diagnostics                                           |
| stratum lint    | centralized config over `ui/**`        | no `ui->platform` import, no `services`/node-platform import |
| unit-pbt        | vitest project `browser`               | the `UndoStack` fold and `announceFor` total-dispatch laws pass |
| browser-e2e     | vitest browser-mode (playwright)       | leaf subscriber renders the projection fold via `AtomBinding` |

## [7]-[PROHIBITIONS]

No second React state binding beside `AtomBinding`; no domain data in local component state; no parallel client-state library where `DeepLinkBinding`/`UndoStack`/`OfflineState` fold under the one binding; the atom inspector is a dev-build-only row stripped by `BuildPipeline` and never in the shipped bundle; no transport dialed directly — every mutation leaves only through the `interchange` `CommandGateway`; no benchmark claim displayed without the fingerprint gate; no second decode of geometry beside the `interchange` `GeometryRail`; the four geo aliases stay collapsed into one `GeoSeriesLayer` union and never restate as parallel const objects; no per-component `.tsx` library beside the `InteractionRole` vocabulary + `RoleBehavior` contract; no `document.documentElement.style` write outside `CssVarSync`; no second decode of the GLB mesh bytes beside the `interchange` `ArtifactFrameRail` and no GL context held as a free React ref outside the `GlbViewport` `Effect.acquireRelease` resource; no parallel viewport-camera gesture handler beside the `ViewportCamera` `RoleBehavior` row on `core`/`gesture`; no `ui/**` import of `platform/**` (the AppUi-analog never imports the AppHost-analog); no comment carrying task or process narration.

## [8]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                                       | [PAGE]              | [CATALOGUE]    | [STATUS] |
| :-----: | :---------------------------------------------- | :------------------ | :------------- | :------- |
|   [1]   | react + react-dom                               | binding, render, component-system | api-ui-stack | admitted |
|   [2]   | @effect-atom/atom + @effect-atom/atom-react     | binding.md          | api-effect     | admitted |
|   [3]   | maplibre-gl                                     | render-surfaces.md  | api-infra-data | admitted |
|   [4]   | @deck.gl/core + @deck.gl/layers + @deck.gl/react | render-surfaces.md  | api-infra-data | admitted |
|   [5]   | @tanstack/react-virtual                         | render-surfaces.md  | api-ui-stack   | admitted |
|   [6]   | @effect/opentelemetry                           | render-surfaces.md  | api-effect-opentelemetry | admitted (collector reader only) |
|   [7]   | react-aria + react-aria-components + react-stately | component-system.md | api-ui-stack | admitted |
|   [8]   | colorjs.io                                      | component-system.md | api-ui-stack   | admitted |
|   [9]   | tailwindcss                                     | component-system.md | api-ui-stack   | admitted |
|  [10]   | isomorphic-dompurify                            | component-system.md | api-ui-stack   | admitted |
|  [11]   | effect                                          | all pages           | api-effect     | admitted |
|  [12]   | three / @babylonjs/core / @google/model-viewer / @webgpu/types | render-surfaces.md  | api-ui-stack   | admitted-on-precondition — the four `RendererBackend` upload/draw rows over the in-memory `MeshView`; the mesh-DECODE seam stays BLOCKED until the upstream `remote-lane#TS_PROJECTION` mesh promotion `(a)` lands |

## [9]-[REFINEMENT_HORIZON]

The `GlbViewport` WebGL mesh render is a DENSITY_BAR owner (row [7], BLOCKED) whose backend/draw/camera owners are authored against the in-memory `MeshView` but whose mesh-DECODE seam is gated on an unmet cross-branch precondition. The four-end DAG (branch [seam-splits](../../.planning/region-map/seam-splits.md), line 50) resolves bottom-up: (a) the C# `remote-lane#TS_PROJECTION` must promote `GeometryPayload(mesh)`/`MeshTensor` from `[PROTO_VOCABULARY]` into the projection fence — the single true blocker, NOT YET LANDED upstream; (b) C# `interchange.md` SharpGLTF/GeometryGym pinning DISCHARGED; (c) the Python `libs/python/compute` IFC->GLB two-hop companion authors, observed not forced. The `ui` viewer is fenced with a `Schema.Literal` renderer-backend axis over three/babylon/model-viewer plus the webgpu literal, and the four WebGL packages are admitted-on-precondition. The blocking residual is precondition `(a)`: until the upstream cluster physically carries the promoted mesh wire type, `decodeMeshView` consumes only a blocked-on-upstream reference to the generated `GeometryPayload` descriptor and this owner stays BLOCKED, never a discharged spike (region-map `owner-symbols.md` records `GlbViewport` as `[REFINEMENT_HORIZON; no leaf this turn]`). Once `(a)` lands, the `GeometryPayload` `point_cloud`/`voxel` oneof decode cases land as sibling `Match` arms on the same `decodeMeshView` fold, each a row on its owner, never a parallel surface.
