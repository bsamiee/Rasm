# [TYPESCRIPT_BRANCH]

The TypeScript branch charter — the web/edge platform of the workspace, five package folders meeting only at the wire contracts and the offline/companion seams. The branch consumes the upstream wire surface as settled vocabulary and authors no wire shape; every cross-language consumption routes through the Tier-0 seam ledger and is named there, never restated here. This charter absorbs the altitude grammar every package obeys and the verification policy every package is held to; per-folder owner state, source trees, and capability lists live in each folder's `ARCHITECTURE.md`, `README.md`, and `FEATURES.md`. The branch ARCHITECTURE draws the cross-folder source tree, dependency DAG, and intra-TypeScript seams; the branch IDEAS pool carries the forward concepts and the folder-tagged refinement horizons.

## [1]-[PACKAGE_TOPOLOGY]

Five packages in three reachability strata, each a real bounded concern. The branch is the web/edge platform; the package catalog resolves under `catalogMode: strict` with Effect held on the 3.x line, and the decompile-grounded `.api/` catalogue set is branch-centralized at `libs/typescript/.api/` (consumed by every folder, owned by none). The strata are the platform-neutral interior (`interchange` + `projection`), the browser publication (`ui` + `platform`), and the node publication (`services`); the bundle fence is a folder-scoped import stratum plus a subpath-export attribute, enforced by the monorepo's centralized config, never a runtime guard.

| [INDEX] | [PACKAGE]     | [STRATUM] | [ROLE]                                                                                                           |
| :-----: | :------------ | :-------- | :------------------------------------------------------------------------------------------------------------- |
|   [1]   | `interchange` | neutral   | the byte-to-typed-and-back wire boundary — single grpc-web transport, the polymorphic codec rail family, content-addressed artifact frames, exhaustive fault reconstruction, outbound command gateway |
|   [2]   | `projection`  | neutral   | the SPA host and runtime fold algebra — the unified key-discriminated transport-free fold over decoded wire vocabulary, the standing-query horizon, the evidence/availability folds |
|   [3]   | `services`    | node      | the durable-execution, RPC, persistence, and provisioning tier — durable workflows/activities/cluster, hybrid search, internal RPC, multi-tenant Postgres, two-mode IaC |
|   [4]   | `ui`          | browser   | the browser UI/UX/components library — reactive binding, leaf render-surfaces, the interaction-role component-system; holds no domain state |
|   [5]   | `platform`    | browser   | the SPA application infrastructure and browser entry — composition root, host substrate, routing, service-worker, crash telemetry, remote config, web-vitals |

The neutral interior both browser and node publications compose; the browser publication carries two folders where `platform` composes `ui` and `ui` never imports `platform`; the node publication keeps the deploy-time closure off the durable runtime hot path behind a `./provisioning` subpath. `interchange` is the inbound dependency root — it dials the wire and imports no sibling; `projection`, `ui`, `platform`, and `services` each consume the decoded shapes and the outbound gateway as settled vocabulary.

## [2]-[ALTITUDE_GRAMMAR]

Each altitude admits exactly one form; a second form on the same altitude is the named defect the grammar deletes. The grammar is closed — a new boundary concept lands as one fold owner and one app-service method, never a new altitude or a sixth app-service. The closed app-service budget is five: `WireClients`, `CommandGateway`, `SnapshotFeed`, `RuntimeFeed`, `EvidenceFeed`; the five strengthened infrastructure owners (`AppRouter`/`ServiceWorkerHost`/`CrashTelemetry`/`RemoteConfig`/`PerformanceBudget`) are host-substrate owners, not app-services, and never count against the budget.

| [INDEX] | [ALTITUDE]     | [ONE FORM]                                            | [DELETED FORM]                           |
| :-----: | :------------- | :---------------------------------------------------- | :--------------------------------------- |
|   [1]   | vocabulary     | `Schema.Literal` union + behavior `Record`            | parallel const enum, string switch       |
|   [2]   | domain shape   | one `Schema.Class`/`Model.Class`, projections derived | parallel struct per projection           |
|   [3]   | error rail     | `Data.TaggedError` family, one per rail               | thrown exception, untyped reject         |
|   [4]   | effect carrier | `Effect` for effects, `Either` pure, `Stream` server  | promise-in-domain, callback              |
|   [5]   | service        | `Effect.Service` class, one owner per axis            | sibling service, free-function module     |
|   [6]   | composition    | one `Layer` graph per entry, one runtime              | scattered `Layer.provide`, second root   |
|   [7]   | boundary       | `Schema.decodeUnknown` at the edge                    | interior re-validate, `null` in interior |

Enforcement is mechanical, never a prose review check: each defect class fails the build or typecheck at exactly one surface. The `projection`-folder `@connectrpc/*` import ban keeps the fold interior transport-free; the `./provisioning` subpath isolates the deploy-time closure; the `ui`-folder `../platform` import ban enforces the one-way intra-browser direction; `tsgo` `isolatedDeclarations` makes a parallel-schema-per-entity a typecheck failure; `Match.tagsExhaustive` makes a hand-rolled fault rendering a compile error. The held Effect-3.x core is the single design constant under the entire owner set; the native `tsgo` `--noEmit` is the canonical typecheck and stable `typescript` runs as the peer-bridge smoke, with the strictness floor (`isolatedDeclarations`, `erasableSyntaxOnly`, `exactOptionalPropertyTypes`, `verbatimModuleSyntax`, `noUncheckedIndexedAccess`, `customConditions`) carried in the centralized root tsconfig every folder inherits.

## [3]-[TEST_POLICY]

The verification law across the five packages: an algebraic property-testing spine, a browser-mode DOM/navigation runner, a mutation kill-ratio gate, and an ephemeral-container durable harness. The neutral and node strata each contribute a node-mode vitest project; the two browser folders share ONE browser-mode project `browser` because the playwright provider is folder-agnostic and the browser publication is one runtime — a second runner config per browser folder is the named test-config defect. Each package carries one `stryker.config.mjs`; the test-config contract shapes (`SpecBudget`, `TestCategory`, `AlgebraicLaw`, `MutationThresholds`, `DurableHarness.engines`, `WebProject.proves`) are runner-configuration vocabularies, exempt from the domain `Schema.Literal`-plus-`Record` form because they decode no wire value.

| [INDEX] | [GATE]            | [RAIL]                       | [PROVES]                                                                                          |
| :-----: | :---------------- | :--------------------------- | :----------------------------------------------------------------------------------------------- |
|   [1]   | `UnitProperty`    | `@effect/vitest` + fast-check | algebraic law (identity/inverse/idempotence/commutativity/associativity/monotonicity/determinism) as the external oracle; the `it.prop` arbitrary is the boundary `Schema.Class` itself; `fc.commands`/`fc.asyncModelRun` grade stateful folds; 175 LOC cap, 95% per-file floor |
|   [2]   | `BrowserE2E`      | playwright browser provider + driver | DOM-load-bearing subscription, virtualized table, geo composition, auth leaf, offline restore over the real React renderer; the deep-link/intent-routing flow as a real navigation; one `browser` project globbing both `ui/**` and `platform/**` |
|   [3]   | `MutationHarness` | `@stryker-mutator/core` (typescript-checker + vitest-runner) | the mutation kill-ratio over each package's unit spine, break/low/high thresholds, related-test scoping |
|   [4]   | `DurableHarness`  | `testcontainers` + `@effect/vitest` | exactly-once workflow, durable replay, runner-restart recovery, singleton pin, RLS tenant scope, hybrid-search rank against ephemeral Postgres + Redis, one container set per `Effect.acquireRelease` scope |

A spec that re-derives expected values from the source it grades is the circular-test defect the mutation gate deletes; an example-only assertion where an algebraic law exists is the named defect; a `vi.fn` transport mock standing where a layer-provided test service exists is the named defect; a mock standing in for a real engine in the `services` tier is the named harness defect.

## [4]-[ENTRY]

The branch authoring method and loop are the cross-language [campaign method](../../.planning/campaign-method.md); the doc-set tiers, schemas, page#cluster notation, signature law, language law, and review law are the [planning standard](../../.planning/README.md). Per-folder entry points live in each `<package>/README.md`; cross-folder seams in [ARCHITECTURE](ARCHITECTURE.md) and [region-map/seam-splits](region-map/seam-splits.md); the forward pool and folder horizons in [IDEAS](IDEAS.md); open work in [TASKLOG](TASKLOG.md); the catalogue index and evidence protocol in [api-catalogues](api-catalogues.md). Cross-language facts — the wire-consumption seam, the CrdtOp wire decode, the SDK codegen consumption, and the content-address read — live only at the Tier-0 seam ledger and are referenced as seams, never restated.
