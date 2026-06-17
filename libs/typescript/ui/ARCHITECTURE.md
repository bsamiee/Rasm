# [UI_ARCHITECTURE]

`ui` is the browser UI library as one folder: one `AtomBinding` is the sole sanctioned state binding, every render leaf subscribes through it, and the component vocabulary is one `InteractionRole` owner-block, not a `.tsx`-per-component sprawl. It holds no domain state and authors no decode. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the source tree and build order, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, boundaries, and prohibitions.

## [1]-[SOURCE_TREE]

The flat module layout IS the build order: the binding precedes every surface that subscribes through it, the component-system precedes nothing it does not read, and render-surfaces compose both. Each leaf is one transcription unit annotated with the owners it transcribes and the owning page#cluster.

```text codemap
ui/
├── binding.ts                      # AtomBinding, DeepLinkBinding, UndoStack, OfflineState, dev inspector — binding#BINDING
├── component-system/               # InteractionRole, RoleBehavior, ThemeTokens, CssVarSync — component-system#COMPONENT_SYSTEM
├── render-surfaces.ts              # EvidenceTimelineRoute/BenchmarkRoute/CollectorPanel, GeoSeriesSurface/GeoSeriesLayer, GlbViewport/RendererBackend/ViewportCamera — render-surfaces#RENDER_SURFACES, #GLB_VIEWPORT
└── index.ts                        # the single `./ui` subpath export
```

`binding.ts` lands first: `AtomBinding` is the spine every render leaf subscribes through and the `DeepLinkBinding`/`UndoStack`/`OfflineState` folds ride under it. `component-system/` lands next as the role vocabulary render-surfaces consume; the announce-arm dispatch is exhaustive at this leaf. `render-surfaces.ts` lands last: the observation routes subscribe through `AtomBinding` to the `projection` stores, `GeoSeriesSurface` reads the `interchange` `GeometryRail`, and `GlbViewport` reads the `ArtifactFrameRail` blob. `index.ts` exports the one `./ui` subpath.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the folder. A new feature is a row or case, never a new surface; `ui` owns no app-service. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual probe named in the page RESEARCH cluster, and `BLOCKED` where the owner's fence is authored but its seam is gated on an unmet cross-branch precondition. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]              | [OWNER]                                                    | [KIND]                  | [CASES]                                                                          | [PAGE#CLUSTER]                  |  [STATE]  |
| :-----: | :---------------------- | :--------------------------------------------------------- | :---------------------- | :------------------------------------------------------------------------------ | :------------------------------ | :-------: |
|   [1]   | sanctioned state binding | `AtomBinding`                                              | React binding + folds   | `useAtomValue`/`useAtomSet`/`useAtomSuspense` over the atom layer + dev inspector | binding#BINDING                 | FINALIZED |
|   [2]   | url-resident state      | `DeepLinkBinding`                                          | query-string fold       | one `Schema`-round-tripped key per surface, intents from stable string keys       | binding#BINDING                 | FINALIZED |
|   [3]   | client-state folds      | `UndoStack`/`OfflineState`                                 | Effect-native folds     | bounded undo/redo/push history + last-good offline cell from `LocalPersistence`    | binding#BINDING                 | FINALIZED |
|   [4]   | observation routes      | `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel`  | read-only leaves        | HLC-ordered evidence + `SkewBand`, fingerprint-gated benchmark, collector reader   | render-surfaces#RENDER_SURFACES | FINALIZED |
|   [5]   | cartographic surface    | `GeoSeriesSurface`/`GeoSeriesLayer`                        | Schema.Union owner      | base map style vs overlay (`featureKind` × `overlayMode` layer set) over the child | render-surfaces#RENDER_SURFACES | FINALIZED |
|   [6]   | component vocabulary     | `InteractionRole`/`RoleBehavior`/`ThemeTokens`            | Schema.Literal owner-block | eight roles × headless behavior + color-space tokens + `CssVarSync` runtime sync | component-system#COMPONENT_SYSTEM | FINALIZED |
|   [7]   | mesh render leaf        | `GlbViewport`/`RendererBackend`/`ViewportCamera`          | Schema.Literal backend axis | four backends × in-memory `MeshView` upload/draw fold + camera `RoleBehavior` row | render-surfaces#GLB_VIEWPORT     | BLOCKED   |

`GlbViewport` is the row [7] owner with its backend/draw/camera owners authored against the in-memory `MeshView`, but its mesh-DECODE seam stays `BLOCKED` on the upstream mesh-wire-type promotion routed through the Tier-0 seam ledger; the WebGL packages are admitted-on-precondition. State stays `BLOCKED`, not `SPIKE`, until the upstream fence physically carries the promoted mesh wire type.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [FOLDER]      | [MAY_REFERENCE_UI] | [UI_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :------------ | :----------------: | :----------------: | :------------------------------------------------- |
|   [1]   | `interchange` |         no         |        yes         | geometry through `GeometryRail`, mutation through `CommandGateway` |
|   [2]   | `projection`  |         no         |        yes         | render leaves subscribe through the stores          |
|   [3]   | `platform`    |        yes         |         no         | the CompositionRoot composes this library           |
|   [4]   | `services`    |         no         |         no         | node tier is out of the browser stratum             |

`ui` is the lower browser-stratum library: `platform` imports `ui`, `ui` never imports `platform`. A `ui`->`platform` import is the named browser-internal coupling defect; `ui` reads only the neutral `interchange`/`projection` surfaces.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named owner cluster, consequences land at the consumer. Intra-TypeScript seams ride `pkg/page#CLUSTER`; the wire contracts the render leaves consume route through the Tier-0 seam ledger as the consequence end only.

| [INDEX] | [SEAM]               | [MECHANICS_AT]                                | [CONSEQUENCE_AT]                                                       |
| :-----: | :------------------- | :-------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | evidence + receipt   | projection/envelope-and-evidence#ENVELOPE_AND_EVIDENCE | render-surfaces#RENDER_SURFACES renders `EvidenceTimelineRoute`/`BenchmarkRoute` |
|   [2]   | geometry projection  | interchange/codec-rails#CODEC_RAILS            | render-surfaces#RENDER_SURFACES reads the `GeometryRail` GeoJSON        |
|   [3]   | artifact blob        | interchange/codec-rails#CODEC_RAILS            | render-surfaces#GLB_VIEWPORT reads the `ArtifactFrameRail` blob          |
|   [4]   | mutation egress      | interchange/gateway-and-quarantine#GATEWAY_AND_QUARANTINE | every `ui` mutation leaves through the `CommandGateway`        |
|   [5]   | offline cell         | platform/platform-substrate#PLATFORM_SUBSTRATE | binding#BINDING `OfflineState` reads `LocalPersistence`                 |
|   [6]   | mesh wire promotion  | the Tier-0 seam ledger                         | render-surfaces#GLB_VIEWPORT decode stays `BLOCKED` until the fence lands |

## [5]-[BOUNDARIES]

- `ui` is not a domain-state package, a decode package, or a host package; it owns the browser UI/UX/components concern and holds no domain state.
- Every domain read flows through a `projection` store and the one `AtomBinding`; local domain state in component state is the named defect.
- Every geometry read flows through the `interchange` `GeometryRail`; every mutation leaves only through the `interchange` `CommandGateway`.
- The atom inspector is a dev-build-only row stripped at the build edge, never in the shipped bundle.
- The GL context is one `Effect.acquireRelease` resource; a free React ref outside the `GlbViewport` resource is the named defect.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a second React state binding beside `AtomBinding`; NEVER domain data in local component state.
- NEVER a parallel client-state library where `DeepLinkBinding`/`UndoStack`/`OfflineState` fold under the one binding.
- NEVER a transport dialed directly; every mutation leaves only through the `interchange` `CommandGateway`.
- NEVER a benchmark claim displayed without the fingerprint gate.
- NEVER a second decode of geometry beside the `GeometryRail`, or of the GLB mesh bytes beside the `ArtifactFrameRail`.
- NEVER the four geo aliases restated as parallel const objects; they stay collapsed into one `GeoSeriesLayer` union.
- NEVER a per-component `.tsx` library beside the `InteractionRole` vocabulary + `RoleBehavior` contract.
- NEVER a `document.documentElement.style` write outside `CssVarSync`.
- NEVER a GL context held as a free React ref outside the `GlbViewport` `Effect.acquireRelease` resource, or a parallel viewport-camera gesture handler beside the `ViewportCamera` `RoleBehavior` row.
- NEVER a `ui/**` import of `platform/**`; the AppUi-analog never imports the AppHost-analog.
- NEVER a comment carrying task or process narration.
