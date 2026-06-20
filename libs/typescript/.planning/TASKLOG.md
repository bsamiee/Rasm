# [TYPESCRIPT_BRANCH_TASKLOG]

The cross-package open and closed work for the TypeScript branch — the wiring, guards, and seams no single folder owns, distilled from the branch concert in `IDEAS.md`. Per-folder work lives in each folder's `TASKLOG.md`. Each task is a card whose leader carries a status marker and thesis, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[WIRE_MUTATION_GATE_DESCRIPTOR_CODEGEN]-[QUEUED]: wire the mutation gate and the descriptor codegen rail into the root dev-tooling manifest.
- Capability: root dev-tooling admits the mutation gate and descriptor codegen rail that the TypeScript folders already resolve from the workspace catalog.
- Shape: `@stryker-mutator/core`, `@stryker-mutator/typescript-checker`, `@stryker-mutator/vitest-runner`, and `@bufbuild/buf` land in the root manifest; versions stay centralized in the workspace catalog, each package carries one `stryker.config.mjs`, and `interchange` consumes `buf` only at its codegen edge.
- Unlocks: per-package mutation spines and `interchange` descriptor `buf generate` transcription resolve without folder-local version pins or command restatement.
- Anchors: `libs/typescript/.planning/README.md` `[04]-[ROOT_DEV_TOOLING]`, the root workspace catalog, the package-local mutation configs, and the `interchange` descriptor-generation boundary.
- Tension: the current gap is catalog-present-but-root-absent, so this card precedes every transcription card that needs mutation proof or emitted `gen/` descriptors.

[AUTHOR_PER_PACKAGE_MANIFESTS_SUBPATH]-[QUEUED]: author the per-package manifests and the subpath publication surface.
- Capability: each TypeScript package resolves through the `libs/typescript/*` workspace glob with the publication subpaths its stratum owns.
- Shape: the five package manifests expose `interchange` and `projection` under `.`, `ui` under `./ui`, `platform` under `./web`, and `services` under `./node` plus `./provisioning`; centralized tsconfig solution references carry the build wiring without new runtime packages.
- Unlocks: browser bundles stay free of node and IaC closures, node bundles stay free of browser surfaces, and deploy-time `@pulumi/*` stays isolated behind `./provisioning`.
- Anchors: branch `ARCHITECTURE.md` dependency direction, the `libs/typescript/*` workspace glob, package `exports`, and centralized TypeScript project references.
- Tension: the manifests land at transcription time; authoring them earlier would create publication surfaces before their package cards settle.

[MATERIALIZE_MECHANICAL_STRATUM_IMPORT_GUARDS]-[QUEUED]: materialize the mechanical stratum and import guards through the centralized config.
- Capability: branch dependency direction becomes a centralized typecheck or lint failure instead of a prose review invariant.
- Shape: the monorepo config owns the folder-stratum module-boundary fence, the `projection/**` `@connectrpc/*` ban, the `ui/**` `../platform` ban, the `./provisioning` subpath isolation, and the `tsgo` strictness floor (`isolatedDeclarations`, `erasableSyntaxOnly`, `exactOptionalPropertyTypes`, `verbatimModuleSyntax`, `noUncheckedIndexedAccess`, `customConditions`).
- Unlocks: transport imports in the fold tier, `platform` imports in `ui`, and `@pulumi/*` hot-path types fail at one central config surface, never at runtime.
- Anchors: branch `ARCHITECTURE.md` `[03]-[DEPENDENCY_DIRECTION]`, centralized eslint module boundaries, and the TypeScript strictness floor.

[WIRE_FOLD_BINDING_READ_SEAM]-[QUEUED]: wire the one-fold-one-binding read seam.
- Capability: the read path stays one `projection` fold algebra and one `ui` reactive spine.
- Shape: every `projection`-derived `Subscribable` from the derived-query combinator binds through the one `ui` `AtomBinding`; `effect` `SubscriptionRef.changes`/`Subscribable` own the fold side, and `@effect-atom/atom` plus `@effect-atom/atom-react` own the render binding.
- Unlocks: composite reactive views grow as incremental fold rows over base change streams, not as view-state layers or component-local raw-store recombination.
- Anchors: `projection` `fold` and `evidence/cells`, the `ui` binding spine, `effect`, `@effect-atom/atom`, `@effect-atom/atom-react`, and `IDEAS.md` `ONE_FOLD_ONE_BINDING`.
- Tension: the `projection` derived-query combinator lands first so each composite view has one fold owner before `ui` binds it.

[WIRE_CONTENT_ADDRESSED_OFF_THREAD]-[QUEUED]: wire the content-addressed off-thread artifact seam.
- Capability: large artifact frames reassemble, verify, stitch, and digest off the main thread into one reusable `ContentKey` blob.
- Shape: `interchange` pipes transferable-stream reassembly into the `platform` `DecodeWorkerPool`; `effect` `Stream`, transferable `ReadableStream`/BYOB worker primitives from `@effect/platform` and `@effect/platform-browser`, and the resolved 128-bit wasm hash run inside the worker.
- Unlocks: the `ui` viewport renders the same `ContentKey`-addressed blob that the `projection` evidence fold correlates, and a future WebTransport byte leg feeds one existing worker seam.
- Anchors: `interchange` artifacts, `platform` worker pool, `ui` viewport, `projection` evidence correlation, C# `XxHash128` fixtures, and `IDEAS.md` `CONTENT_ADDRESSED_OFF_THREAD`.
- Tension: content-key parity must close first; until the 128-bit provider and upstream fixture question is proven, the off-thread digest remains local-only rather than trusted cross-runtime.

[WIRE_NATIVE_ROUTE_TRANSITION_PER]-[QUEUED]: wire the native route-transition and per-route vital seam.
- Capability: native route transitions and per-route vital attribution share one admitted location cell.
- Shape: the `platform` Navigation API router and `document.startViewTransition` fold drive `ui` `<ViewTransition>`/`<Activity>` wrappers around the guard-admitted commit; `platform` web-vitals reset and flush use the same location cell for per-soft-navigation INP and CLS attribution.
- Unlocks: the `ui` viewport preserves GL context and tab state across route swaps, reduced-motion becomes one policy row, and no parallel route tracker or hand-authored FLIP choreography appears.
- Anchors: Navigation API, View Transitions API, React `<ViewTransition>`/`<Activity>`/`useEffectEvent`, `effect` `Stream.asyncScoped`/`Effect.acquireRelease`, and `IDEAS.md` `NATIVE_TRANSITION_PAIR`.

[WIRE_CAUSAL_ORDERING_READ_OFF]-[QUEUED]: wire the causal-ordering read off the C# HLC band — wire touchpoint for `libs/.planning` CAUSAL_TENANT_IDENTITY_WIRE.
- Capability: TypeScript reads C# HLC uncertainty as a load-bearing ordering input, never as a render-only decoration or second clock.
- Shape: the `projection` convergence fold decodes the HLC two-half causal stamp from the upstream wire, promotes `SkewBand` `{ midpointMs, radiusMs }` into the concurrent-uncertain decision, and exposes the ordering through an `effect` `Subscribable`; the `ui` evidence timeline renders the same band.
- Unlocks: causally ambiguous events stay honest in the fold and in the rendered evidence route, and the cross-`libs` `CAUSAL_TENANT_IDENTITY_WIRE` seam gets one TypeScript anchor.
- Anchors: C#-minted HLC stamp, `projection` convergence and skew-ordering, `ui` observation route, content/causal parity fixtures, and `IDEAS.md` `HONEST_CLOCK_UNCERTAINTY`.
- Tension: the parity fixture lands before this seam becomes trusted; a second TypeScript clock or causal scheme is the named cross-language drift defect.

[EXTEND_BRANCH_ORDERED_COLLECTION_STREAM]-[QUEUED]: extend the branch `.api/effect.md` ordered-collection, `Subscribable`, and stream-combinator coverage.
- Capability: the branch-owned `effect.md` catalogue carries the ordered-collection, `Subscribable`, value-equality, and stream-combinator members that `projection` transcribes.
- Shape: `libs/typescript/.api/effect.md` adds `RedBlackTree` range cursors (`empty`/`make`/`insert`/`removeFirst`/`greaterThanEqual`/`at`/`headOption`/`lastOption`), `SortedMap` keyed maps (`empty`/`make`/`fromIterable`/`set`/`get`/`remove`/`headOption`/`lastOption`/`entries`/`keys`/`values`/`reduce`/`getOrder`), `SortedSet`, `Subscribable` `{ get, changes }` plus `Subscribable.make`/`isSubscribable`, `Equivalence`/`Hash`, and `Stream` `scan`/`mapAccum`/`broadcast`/`share`/`aggregateWithin`/`changes`.
- Unlocks: `projection` ordered-index, projection-face, and causal-delivery pages transcribe from one verified branch catalogue instead of folder-side RESEARCH fences.
- Anchors: installed `effect@3.21.3` `dist/dts`, branch `.api/effect.md`, `projection` README `[CROSS_CUTTING]`, and the `projection` `ORDERED_INDEX_COLLAPSE` / `SUBSCRIBABLE_PROJECTION_FACE` folder ideas.
- Tension: exact module ownership is load-bearing: `greaterThanEqual`/`at`/`insert`/`removeFirst` are `RedBlackTree`, and keyed-map range walking uses `SortedMap` in-order `entries`.

[PROMOTE_CLOSED_FAMILY_UNION_LINT]-[QUEUED]: promote the closed-family `_tag`-union lint rule onto the centralized lint surface.
- Capability: closed-family `_tag` unions become mechanically enforced as generated owners, not hand-reviewed prose discipline.
- Shape: one centralized eslint/`tsgo` rule flags any `type`/`interface` union of two or more `{ readonly _tag: <literal>; ... }` arms unless the family is produced by `Data.taggedEnum`, `Schema.TaggedClass`, or `Data.TaggedError`.
- Unlocks: `shapes.md` `[5]` becomes a build error; generated families and boundary runtime `Schema` codecs remain legal without per-symbol allowlists.
- Anchors: centralized module-boundary rule surface, `projection/**` `@connectrpc/*` and `ui/**` `../platform` import bans, the regression corpus positive span, the non-flagged `Data.taggedEnum` form, the `Schema.Union(Schema.Struct({ _tag: Schema.Literal(...) }))` wire codec, and `IDEAS.md` `CLOSED_FAMILY_LINT_FENCE`.

[EXTRACT_EVENT_STREAM_CATALOGUES_RESEARCH]-[QUEUED]: extract the `@pulumi/pulumi/automation` event-stream and `rfc6902` `.api/` catalogues so the `services` RESEARCH flags drop.
- Capability: `services` drops RESEARCH gates for Pulumi preview events and JSON-patch activity diffs by extracting their real `.api` surfaces.
- Shape: the `@pulumi/pulumi/automation` catalogue captures `Stack.previewRefresh({ onEvent })`, `EngineEvent.resourcePreEvent.metadata`, `StepEventMetadata.op`/`.detailedDiff`, `OpType`, `PropertyDiff.diffKind`/`.inputDiff`, `DiffKind`, and `PreviewResult.changeSummary`; `rfc6902.md` captures `createPatch(before, after) -> Operation[]`, both verified against `services/node_modules`.
- Unlocks: `provisioning/drift.md` and `execution/ai.md` transcribe settled fences from `.api`, the provisioning drift fold consumes verified Pulumi event members, and `ai-activity.md` composes the verified `Operation[]` diff shape.
- Anchors: workspace catalog, `services` README roster, `services` folder `.api/`, drift page `[3]-[RESEARCH]`, and `execution/ai.md`.
- Tension: the extraction lands after the catalogue split so the Pulumi automation file is per-package from the start.

[PUBLIC_EDGE_INGRESS]-[QUEUED]: stand up the `libs/typescript/edge/` public-ingress library folder and its four index docs.
- Capability: `libs/typescript/edge/` becomes the one public HTTP ingress package above the node durable interior.
- Shape: the new package lands with `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`, and `.planning/` sub-domains for `api/` (`HttpApiGroup`/`HttpApiEndpoint` family plus `HttpApiClient` SDK), `middleware/` (`FaultDetail` to RFC 9457 problem-detail fold, CORS, body/rate caps, `traceparent`, auth-context `HttpApiMiddleware`), `runtime/` (`NodeHttpServer.layer` and `toWebHandler` as deployment rows on one `HttpApp`), and `spec/` (`OpenApi.fromApi` emit).
- Unlocks: the branch gains one externally reachable API surface, `services` durable workflows and `messaging/rpc` proxy stay handler internals, `interchange`/`projection` provide decode and availability gating, and `ARCHITECTURE.md` fault ownership gains the edge problem-detail altitude.
- Anchors: `@effect/platform` `HttpApi`/`HttpApiBuilder`/`HttpApiGroup`/`HttpApiEndpoint`/`HttpApiMiddleware`/`HttpApiClient`/`OpenApi`/`HttpServer`/`Multipart`, `@effect/platform-node` `NodeHttpServer.layer`/`toWebHandler`, branch `ARCHITECTURE.md`, and `IDEAS.md` `PUBLIC_EDGE_INGRESS`.
- Tension: the index docs land at this branch card; `api`/`middleware`/`runtime`/`spec` design pages follow as folder-local cards after the folder owner settles.

[REVERSE_HTTPAPI_OUT_SCOPE_NOTE]-[QUEUED]: reverse the `interchange/.api/effect-platform.md` HttpApi-out-of-scope note for the `edge` folder catalogue — coverage finding.
- Capability: the `edge` folder owns the verified HttpApi catalogue surface that the branch catalogue previously marked out of scope.
- Shape: `edge/.api/effect-platform.md` captures `HttpApi.make`, `HttpApiGroup.make`, `HttpApiEndpoint.{get,post,...}`, `HttpApiBuilder.{api,group,handler,toWebHandler}`, `HttpApiClient.make`, `HttpApiMiddleware.Tag`, `OpenApi.fromApi`, and `HttpServer.serve` against `node_modules`.
- Unlocks: `edge` `api`/`middleware`/`runtime`/`spec` design pages transcribe from verified `.api` entries, and the branch `effect-platform.md` note narrows to browser/neutral non-ownership while naming `edge/.api/effect-platform.md` as the HttpApi owner.
- Anchors: `interchange/.api/effect-platform.md`, `edge/.api/effect-platform.md`, `@effect/platform` HttpApi modules, `@effect/platform-node`, and the `PUBLIC_EDGE_INGRESS` stand-up card.
- Tension: this lands with the `edge` folder stand-up; before that point the `edge` HttpApi fences remain RESEARCH-gated.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
