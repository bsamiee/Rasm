# [WEB_PLANNING]

`@rasm/web` owns the complete browser web/UI/UX platform — a browser concern distinct in kind from the C# Avalonia AppUi, owned ground-up: the one sanctioned `AtomBinding` spine, client-state folds (url-as-state, offline persistence, undo/redo), the leaf render routes and the 2D geo surface, the role-based headless interaction-role vocabulary, the SPA composition root with auth and config, and the telemetry/build/worker/persistence substrate. Zero consumers exist; implementation is full-capability with no holding back; pages are transcribed, not re-designed. It is the BROWSER publication entry (Nx tag `scope:browser`), consumes only grpc-web unary + server-stream, partitions all evidence by `TenantContextWire`, and emits intents only through the `@rasm/interchange` `CommandGateway`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                | [OWNS]                                                                       | [STATE]     |
| :-----: | :-------------------- | :-------------------------------------------------------------------------- | :---------- |
|   [1]   | binding.md            | AtomBinding + DeepLinkBinding + url-state + offline persistence + undo/redo + dev inspector | provisional |
|   [2]   | render-surfaces.md    | the leaf observation routes + GeoSeriesSurface/GeoSeriesLayer + the GlbViewport horizon | provisional |
|   [3]   | component-system.md   | the role-based headless interaction-role vocabulary owner-block + theme tokens + CSS-var sync | provisional |
|   [4]   | host-runtime.md       | CompositionRoot + BrowserPlatform + AuthSession (arctic) + RuntimeConfig + the SPA entry | provisional |
|   [5]   | platform-substrate.md | SelfTelemetry + MetricRegistry + BuildPipeline + DecodeWorkerPool + LocalPersistence | provisional |

## [2]-[WIRE_PAGES]

The domain authors no wire shape; it subscribes to the `@rasm/projection` folds and reads `@rasm/interchange` decoded shapes. The wire-relevant consumer pages:

- render-surfaces.md: Compute receipts-and-benchmarks (BenchmarkRoute fingerprint gate), AppUi diagnostics-evidence (EvidenceTimelineRoute), Persistence snapshot-codecs geometry (GeoSeriesSurface via GeometryRail).
- host-runtime.md: AppHost runtime-ports envelope read through the projection feeds; the credential stamp seam into `@rasm/interchange` `WireTransport`.

## [3]-[CATALOGUE_PENDING]

- maplibre-gl + @deck.gl/core + @deck.gl/layers + @deck.gl/mapbox: admitted; the geo render needs the `core` engine, the `layers` `GeoJsonLayer` constructor, and the `mapbox` `MapboxOverlay` interleave.
- @opentelemetry/sdk-trace-web: admitted; the browser OTel exporter the `WebSdk` layer binds — the node SDK cannot serve the browser stratum.
- arctic: admitted; the single OAuth/OIDC owner — the `oauth4webapi` pending row is void.
- idb-keyval: admitted; the offline IndexedDB backing store under the Effect `KeyValueStore` abstraction, never called directly.

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP]                                                              | [CLOSED_BY (page#cluster)]                          |
| :-----: | :---------------------------------------------------------------- | :-------------------------------------------------- |
|   [1]   | a second React state binding beside the atom layer                | binding#BINDING (one AtomBinding spine)             |
|   [2]   | a hand-rolled idb-keyval call outside the Effect KV abstraction   | platform-substrate#PLATFORM_SUBSTRATE (LocalPersistence over KeyValueStore) |
|   [3]   | the four loose geo aliases as parallel const objects              | render-surfaces#RENDER_SURFACES (one GeoSeriesLayer union) |
|   [4]   | a benchmark claim displayed without the fingerprint gate          | render-surfaces#RENDER_SURFACES (BenchmarkRoute gate) |
|   [5]   | the node OTel SDK leaking into the browser telemetry edge         | platform-substrate#PLATFORM_SUBSTRATE (WebSdk over sdk-trace-web) |
|   [6]   | a sixth app-service or a parallel AuthStore                       | host-runtime#HOST_RUNTIME (closed five-service budget, AuthSession single owner) |
|   [7]   | per-component react-aria .tsx patterns proliferating              | component-system#COMPONENT_SYSTEM (one interaction-role vocabulary owner-block) |

## [5]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]            | [OWNER]                  | [KIND]            | [CASES]                                              | [STATE]    |
| :-----: | :------------------------ | :----------------------- | :---------------- | :--------------------------------------------------- | :--------- |
|   [1]   | React state binding       | `AtomBinding`            | Effect.Service     | use/useSet leaf subscription + dev inspector row      | FINALIZED  |
|   [2]   | client-state folds        | `DeepLinkBinding`/`UndoStack`/`OfflineState` | fold rows | url-state, undo/redo, offline persistence over the binding | FINALIZED  |
|   [3]   | leaf observation routes    | `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel` | route subscribers | HLC timeline, fingerprint-gated benchmark, collector | FINALIZED  |
|   [4]   | 2D geo surface            | `GeoSeriesSurface`       | one union owner    | GeoSeriesLayer base/overlay × featureKind × overlayMode | FINALIZED  |
|   [5]   | interaction-role vocabulary | `InteractionRole`      | one owner-block    | actions/collections/inputs/overlays/navigation/feedback/pickers/core | FINALIZED  |
|   [6]   | theme tokens              | `ThemeTokens`            | token owner        | color-space tokens + runtime CSS-var sync             | FINALIZED  |
|   [7]   | SPA composition root      | `CompositionRoot`        | one Layer graph    | provides the closed five app-services                 | FINALIZED  |
|   [8]   | browser platform          | `BrowserPlatform`        | Layer              | HTTP / key-value / worker bindings                    | FINALIZED  |
|   [9]   | browser credential        | `AuthSession`            | Effect.Service     | arctic OIDC/PKCE + SubscriptionRef session fold       | FINALIZED  |
|  [10]   | typed config              | `RuntimeConfig`          | one Config schema  | browser ConfigProvider.fromJson variant               | FINALIZED  |
|  [11]   | self-telemetry edge       | `SelfTelemetry`/`MetricRegistry` | Layer + vocab | WebSdk export + bounded instrument set                | FINALIZED  |
|  [12]   | build + offload + persist  | `BuildPipeline`/`DecodeWorkerPool`/`LocalPersistence` | owners | PWA build, Worker.makePool, KeyValueStore over idb-keyval | FINALIZED  |
`GlbViewport` is a refinement-horizon entry, not a present owner, so it carries no DENSITY_BAR row and no FINALIZED/SPIKE state until the upstream mesh `#TS_PROJECTION` exists; its route lives in the REFINEMENT_HORIZON section and the `owner-symbols.md` `#RESEARCH` registry, never a third state value in this two-state surface.

## [6]-[BUILD_ORDER]

| [INDEX] | [FILE]                    | [TRANSCRIBES]                                   | [GATE]                          |
| :-----: | :------------------------ | :---------------------------------------------- | :------------------------------ |
|   [1]   | src/binding.ts            | binding#BINDING                                 | tsgo --noEmit clean             |
|   [2]   | src/component-system/     | component-system#COMPONENT_SYSTEM               | tsgo + token sync               |
|   [3]   | src/render-surfaces.ts    | render-surfaces#RENDER_SURFACES                 | tsgo + browser-mode spec        |
|   [4]   | src/platform-substrate.ts | platform-substrate#PLATFORM_SUBSTRATE           | tsgo + worker pool spec         |
|   [5]   | src/host-runtime.ts       | host-runtime#HOST_RUNTIME                       | tsgo + e2e composition          |
|   [6]   | src/browser.ts            | the browser entry composing interchange+projection+web | bundle builds            |
|   [7]   | src/index.ts              | the "." export                                  | exports resolve                 |

## [7]-[PROOF_GATES]

| [GATE]          | [COMMAND]                  | [EVIDENCE]                                        |
| :-------------- | :------------------------- | :------------------------------------------------ |
| catalog resolve | `pnpm install`             | catalogMode strict resolves @rasm/web             |
| typecheck       | tsgo `--noEmit`            | zero diagnostics                                  |
| browser-pbt     | vitest project `browser` (playwright) | atom-subscription, geo-composition, auth-leaf prove |
| e2e             | playwright driver          | DeepLinkBinding->IntentRegistry->CommandGateway survives reload |
| bundle          | vite build                 | dev atom inspector stripped from production       |

## [8]-[PROHIBITIONS]

No second React state binding beside `AtomBinding`; no production-shipped atom inspector; no direct `localStorage`/`IndexedDB`/`idb-keyval` access outside `LocalPersistence`; no transport dial — intents emit only through the `@rasm/interchange` `CommandGateway`; no sixth app-service or parallel `AuthStore`; no node OTel SDK in the browser edge; no parallel const objects for the geo aliases; no per-component react-aria `.tsx` implementation pattern beside the one interaction-role vocabulary; no direct `import.meta.env` read outside `RuntimeConfig`; no WebGL package admitted before the upstream mesh fence; no comment carrying task or process narration.

## [9]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                  | [PAGE]                 | [CATALOGUE]   | [STATUS]   |
| :-----: | :------------------------- | :--------------------- | :------------ | :--------- |
|   [1]   | react / react-dom          | binding.md             | api-ui-stack  | admitted   |
|   [2]   | @effect-atom/atom + atom-react | binding.md         | api-effect-atom + atom-react | admitted |
|   [3]   | react-aria / -components / -stately | component-system.md | api-ui-stack | admitted   |
|   [4]   | nuqs                       | binding.md             | api-ui-stack  | admitted   |
|   [5]   | idb-keyval                 | platform-substrate.md  | api-ui-stack  | admitted   |
|   [6]   | maplibre-gl + @deck.gl/*   | render-surfaces.md     | api-ui-stack  | admitted   |
|   [7]   | arctic / otplib / @simplewebauthn/server | host-runtime.md | api-ui-stack | admitted |
|   [8]   | @effect/opentelemetry + @opentelemetry/sdk-trace-web | platform-substrate.md | api-effect-opentelemetry | admitted |
|   [9]   | @effect/platform-browser   | host-runtime.md, platform-substrate.md | api-effect-platform-browser | admitted |
|  [10]   | tailwindcss / colorjs.io / isomorphic-dompurify | component-system.md | api-ui-stack | admitted |
|  [11]   | vite                       | platform-substrate.md  | api-ui-stack  | admitted   |

## [10]-[REFINEMENT_HORIZON]

The single render refinement-horizon owner is `GlbViewport`, the WebGL mesh render with a `Schema.Literal` renderer-backend axis admitting three/babylon/model-viewer and a webgpu literal for the meshlet/cluster-LOD ambition; admitted only when the upstream C# `remote-lane#TS_PROJECTION` promotes the mesh shape and the Python `libs/python/compute` IFC->GLB companion authors — the four-end DAG the branch observes. The BCF anchor-algebra render-surface waits on the C# `annotation#ANCHOR_ALGEBRA`. The next deepening drives deeper render GPU paths and the full interaction-role breadth on the component-system owner-block. Closed by the bar: every leaf surface is one subscriber, every interaction role is one vocabulary row, and the geo aliases are one union owner.
