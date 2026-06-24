# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more of C# / Python / TypeScript at the wire and the companion/offline seams. A concept that couples packages within one language lives in that branch's `IDEAS.md`, never here. `[1]-[OPEN]` holds the live concert; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[THE_CONCERT]-[QUEUED]: one host-authored model computes across Python and surfaces through TypeScript without crossing runtime interiors.
- Capability: flagship tri-language product pipeline spanning C# authoring, Python offline compute, and TypeScript web/edge consumption.
- Shape: a model sealed in `csharp:Rasm.Persistence` emits canonical wire shapes once; the Python companion recomputes offline, and the TypeScript web platform renders the result after decoding the same C#-owned wire vocabulary and reproducing the same content-identity seed.
- Unlocks: a browser SPA, node backend, and offline companion that meet only at wire contracts and companion/offline seams, never through per-language hand-written clients.
- Anchors: content-address identity, proto wire vocabulary, suite wire law, op-log CRDT payload, capability descriptor + SDK source, tenant/causal identity, GLB tessellation rail, and graduation evidence each keep one cross-branch owner.

[ONE_MODEL_THREE_FACES]-[QUEUED]: one sealed model emits snapshot, op-log delta, and gRPC once for three runtime faces.
- Capability: shared model presentation through snapshot, delta, and service-call forms without a second wire mint.
- Shape: `csharp:Rasm.Compute` suite wire (`Runtime/channels#PROTO_VOCABULARY`/#FAULT_PROJECTION) and `csharp:Rasm.Persistence` snapshot/op-log surfaces form the one wire vocabulary; `typescript:interchange` decodes over grpc-web, and `python:runtime/transport/serve` decodes the same proto.
- Unlocks: a new runtime face as a decode row whose field-for-field re-decode matches the source wire.
- Anchors: C# owns the wire, peers decode it, and a second shared-shape mint in any branch is the cross-language drift defect.

[ONE_CONTENT_IDENTITY]-[QUEUED]: the C# `XxHash128` seed reproduces bit-identically in Python and TypeScript.
- Capability: one content-address identity for cross-runtime artifacts, sub-results, assembled blobs, and parity fixtures.
- Shape: `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` mints seed zero with the two-64-bit-half order; `python:runtime/evidence/identity` reproduces through `xxhash`, and `typescript:interchange` reproduces through a 128-bit wasm hash.
- Unlocks: fetch-by-content-key across runtimes, so Python-recomputed artifacts, C# sub-results, and TypeScript blobs share one byte-addressable identity.
- Anchors: content-addressed build graph over the one seed; HLC two-half encoding rides the same parity because a logical half-order error makes a fresh op fold as stale.
- Tension: TS admission moves to `hash-wasm` after retiring 128-bit-incapable `xxhash-wasm`; Python core parity waits on an upstream `xxhash` cp315/abi3 wheel while the sub-3.15 companion consumes `xxh3_128_intdigest`.

[ONE_TESSELLATION_RAIL]-[QUEUED]: one Python-native IFC/STEP companion produces the GLB every runtime consumes.
- Capability: shared tessellation rail for IFC and CAD geometry through content-keyed GLB output.
- Shape: `python:geometry` serves the IfcOpenShell GLB tessellation daemon; `csharp:Rasm.Bim/exchange/interchange` builds the tessellation request, `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` content-addresses the result, and `typescript:ui` renders the GLB viewport.
- Unlocks: one geometry-evaluation companion with no duplicate mesh pipeline; meshing keeps one owner per runtime, and IFC keeps one semantic owner per runtime while meeting only at the content-keyed GLB.
- Anchors: one-owner-per-runtime geometry law, IfcOpenShell/OpenCascade companion two-hop, `SharpGLTF.Ext.3DTiles` builders (`FeatureIDBuilder`, `PropertyTable`, `EXTStructuralMetadataRoot`), and per-tile `EXT_structural_metadata`/`EXT_mesh_features` carrying IFC class and field bands on the same key.
- Tension: the AP242 STEP arm rides the same OpenCascade companion shape but waits on `cadquery-ocp` companion admission.

[CRDT_COLLABORATION]-[QUEUED]: op-log CRDT becomes a breaking amendment to the one wire vocabulary.
- Capability: multi-runtime collaborative editing over a single op-log changefeed.
- Shape: `csharp:Rasm.Persistence/Version/commits#CRDT_ALGEBRA` owns the `MvRegister`/`opMerge` op union, `Sync/collaboration#TS_PROJECTION` projects the changefeed, and the `typescript:projection/convergence#LWW_MERGE` fold plus Python companion decode the amended payload as the one vocabulary.
- Unlocks: divergent-delivery folds of the same op-set converge to byte-identical state under one mechanically verified convergence law.
- Anchors: closed-CRDT prohibition; LWW survives only as the register arm, and a new op kind lands as one arm on the existing fold after it lands on the wire.
- Tension: a second composition surface already broke the closed vocabulary once and was dropped, so branch-side op authoring remains the named failure mode.

[FAULT_WIRE_ROUNDTRIP]-[QUEUED]: C# fault triples reconstruct losslessly in TypeScript. (C#↔TS only.)
- Capability: typed fault rail whose identity is `(package, code, case)`, not a status code plus string.
- Shape: `csharp:Rasm.Compute/Runtime/channels#FAULT_PROJECTION` packs typed faults into one `FaultDetail` family with disjoint code bands (`ComputeFault` 2200, `HopFault` 4500, `WireFault` 4520-4532, store/config at app roots); `typescript:interchange/Ingress/fault` reconstructs the literal-discriminated union through `faultTagOf`/`FAULT_CTOR` and folds unmapped packages to `Quarantine`.
- Unlocks: producer-side detection of misbanded codes before emit and downstream fault handling without wire ambiguity.
- Anchors: package-exclusive band law; neither `package` nor `code` alone is a total key, and the round-trip property is the gate any new fault family passes.
- Tension: Python mints `FaultDetail` outbound but is not a package-keyed decoder, so the round-trip scope stays C#↔TS.

[ONE_CAUSAL_TENANT_IDENTITY]-[QUEUED]: C# mints the tenant/causal frame once, and Python plus TypeScript read it.
- Capability: shared causal and tenant identity for receipt ordering, attribution, and isolation across runtimes.
- Shape: `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` mints the HLC two-half stamp on `ReceiptEnvelope` and threads `TenantContext`; `python:runtime/transport/serve#SERVE` decodes `hlc_physical`/`hlc_logical` plus `tenant` from the inbound receipt slot, and `typescript:projection` reads the HLC band for skew-aware ordering.
- Unlocks: receipts produced in any runtime carry stamps the others order against, and tenant isolation remains a wire fact instead of a per-runtime scheme.
- Anchors: HLC two-64-bit-half encoding, C#-owns-the-wire law, and content-seed parity fixtures that also prove HLC half order.
- Tension: the Python `runtime/observability/receipts#RECEIPT` page is not this consumer because it owns local evidence and disclaims the AppHost envelope; causal/tenant inbound belongs to the server decode leg and inherits the `hash-wasm`/upstream-`xxhash` wheel gates.

[ONE_GRADUATION_EVIDENCE]-[QUEUED]: Python offline results cross inward through one content-keyed graduation rail.
- Capability: single graduation contract for offline science results consumed by the C# determinism closure.
- Shape: `python:compute/graduation#GRADUATION` packs each result into one `HandoffAxis` `Literal`-discriminated evidence shape keyed by `ContentIdentity`; axes are `solver`, `symbolic`, `model-asset`, `array-layout`, `unit-law`, `uncertainty-law`, and `geometry`, with `geometry` sub-discriminated by `GeometrySubject` values `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, and `form-finding`; ONNX surrogate fit crosses as `model-asset` after `ModelAssetManifest` validation.
- Unlocks: every offline result graduates the same way regardless of producing Python package, so `csharp:Rasm.AppHost/Runtime/determinism#EVENT_LOG` consumes one rail, hash-chained replay references one key, and cross-machine replay-verify reads the same evidence.
- Anchors: one-graduation-rail-outward law, C# determinism ownership, content-keyed ONNX inference, `(checksum, OrtEpDevice)` provider-determinism fingerprint as the C# `SESSION_CAPSULE` key, and landed Vectors DDG adjoint coefficients (`T-DDG-ADJOINT-ROWS` closed).
- Tension: the remaining C# determinism leaf is the offline graduation handoff itself, not the gradient-adjoint substrate.

[CONTENT_REUSE_FEDERATION_WIRE]-[QUEUED]: cross-session content-keyed reuse resolves against the one C#-owned durable federation ledger.
- Capability: the Python companion's session-local content-keyed lane cache becomes a federation participant whose durable backing is the C# Persistence federated graph, so a by-reference reuse lookup the companion misses in-session resolves an `Ok` result a prior session — in any runtime — committed to the durable reuse ledger, the digest-parity-proven `ContentKey` the single reuse name across both tiers.
- Shape: `python:runtime/transport/serve#SERVE` answers a `ReuseFrame` `(ContentKey, ContentDescriptor)` by-reference lookup that `python:runtime/transport/wire#WIRE_PROTO_CODEC` transcodes as one more `(Struct, Message)` pair; on an in-session `LanePolicy.cached` `Map[ContentKey, T]` miss the companion resolves over the wire against `csharp:Rasm.Persistence/Query/federation#FEDERATED_PLAN`, which keys the durable reuse row by the `#ENTITY_GRAPH` `XxHash128` content-addressed identity and rides the `Version/commits` content-addressed commit-DAG/op-log substrate, never a second engine or a Python durable store.
- Unlocks: by-reference reuse across companion sessions, package boundaries, and runtimes — `compute`, `data`, `geometry`, and `artifacts` expensive outputs hit one federated ledger keyed by the same identity the in-session lane keys, durable hits derived from the proven key rather than re-declared per owner, with the in-session `Map` staying the hot path and the durable federation the cold-resolve tier.
- Anchors: the one `XxHash128` content-address seed and `ONE_CONTENT_IDENTITY` digest-parity gate the key trusts, `csharp:Rasm.Persistence/Query/federation#FEDERATED_PLAN`/`#ENTITY_GRAPH` source-agnostic federated graph, `Version/commits` content-addressed commit-DAG, `python:runtime/evidence/identity#IDENTITY` `ContentKey`/`ContentIdentity`, `execution/lanes` `LanePolicy.cached`/`Keyed[T]`/`Map[ContentKey, T]`, and the `transport/wire` `WireProtoCodec`/`transport/serve` invoke leg.
- Tension: the federation is split by tier — the Python participant is by-reference resolution over the wire and never a durable store, the C# `Query/Federation` is the sole durable-identity owner the architecture law assigns, and the lookup misses correctly on a settings-drift key exactly as the in-session cache does; this is distinct from `csharp:Rasm.Persistence` `[REUSE_WIRE]`, which consumes the `python:data`-stamped `ContentKey` to federate the data-lane reuse ledger, whereas this seam federates the runtime lane cache's cross-session by-reference resolution.
- Ripple: `python:runtime` `[CONTENT_REUSE_FEDERATION_WIRE]` and `csharp:Rasm.Persistence` `[REUSE_WIRE]` — the runtime card owns the by-reference wire participant and disclaims the durable store; this cross-language card binds that participant to the C# durable federation owner, distinct from the data-lane reuse-ledger pairing the Persistence `[REUSE_WIRE]` card owns.

[ONE_WIRE_FIXTURE_CORPUS]-[QUEUED]: one frozen golden-fixture corpus feeds every parity harness.
- Capability: content-addressed fixture corpus for cross-runtime identity, fault, CRDT, GLB, and HLC parity.
- Shape: `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and `csharp:Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` mint frozen bytes once: eight-primitive clash bytes, 52-byte `0x9462A71A5DD13DCFA3B1D6D225FCBE70` adjacency digest, `FaultDetail` `(package, code, case)` triples, CRDT op-set, GLB-by-key, and HLC two-half stamps.
- Unlocks: `CONTENT_IDENTITY_PARITY`, `FAULT_WIRE_ROUNDTRIP`, and `TRI_LANGUAGE_WIRE_PARITY` read one corpus rather than re-deriving fixtures in each runtime.
- Anchors: one `XxHash128` seed, `python:runtime/evidence/identity#IDENTITY`, `typescript:interchange/Codec/frame#CONTENT_HASHING`, `typescript:testing/`, the C# shared test corpus, C# `[BRANCH_TEST_NODE_PROVISIONING]`, Python `[PYTHON_TEST_INFRA_FLOOR]`, and TS `testing/`.

[WEB_GEOMETRY_RESIDENCY_WIRE]-[QUEUED]: WebGPU raster uses a second geometry wire shape rather than overloading GLB.
- Capability: content-keyed splat/meshlet residency manifest distinct from triangulated GLB.
- Shape: `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` mints the residency manifest; `typescript:platform/Transport/decode#RESIDENCY_DECODE` and `typescript:ui/render/glb#GLB_VIEWPORT` consume it, and the shape binds the Python splat-decode companion (`SPZ`/`SOG`).
- Unlocks: WebGPU residency without a second manifest mint; AppUi remains the producer, the TypeScript worker decodes, and `ui/glb-viewport` consumes by reference.
- Anchors: one-owner-per-wire-shape law, `ONE_CONTENT_IDENTITY` parity, `platform/worker/` decode leg, and `SPLAT_REALITY_CAPTURE` C# capability.
- Tension: manifest mint+decode is landed, but Python splat-payload decode and live WebGPU upload remain the residual gates.

[ONE_DISTRIBUTED_TRACE]-[QUEUED]: one W3C trace-context frame correlates browser, C#, and Python work.
- Capability: distributed trace propagation that joins browser interaction, C# solve, and Python tessellation under one trace id.
- Shape: `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` owns `traceparent`/`tracestate` propagation and registers the `TraceContextPropagator`/`BaggagePropagator` composite as `Propagators.DefaultTextMapPropagator`; `python:runtime/observability/receipts#RECEIPT` gains inbound `Extract`/`Continue`, and `typescript:platform/Observability/telemetry#METRIC_REGISTRY` propagates `traceparent` over grpc-web and continues the parent trace inbound.
- Unlocks: flame graphs and span trees that cross browser INP spans, C# `ActivitySource` solve spans, and Python tessellation spans without stitching collector silos by guesswork.
- Anchors: W3C Trace Context, OTel GenAI/MCP span conventions, C# propagation-fold ownership, and peers extracting-and-continuing at the wire.
- Tension: peers inject their own active spans today but do not extract the C# parent across the wire, so traces flatten per process until this seam lands.

[ONE_HEALTH_DEGRADATION_WIRE]-[QUEUED]: C# health and degradation state gate TypeScript availability.
- Capability: single capability-health frame for UI exposure, command dispatch, degraded serving, and operator visibility.
- Shape: `csharp:Rasm.AppHost/Observability/health#TS_PROJECTION` mints `HealthSnapshot`, five-level `DegradationLevel`, and alert shapes; `typescript:projection/evidence/availability#AVAILABILITY_GATE` consumes the level at `interchange:CommandGateway`, and `typescript:ui/render/dashboard#LIVE_BINDING_DASHBOARD` renders the level plus retained-capability set.
- Unlocks: usable failure across consumers: degraded capability keeps serving, browser commands disable, node routes around unhealthy peers, and a new health-gated surface becomes one availability-fold row.
- Anchors: C# usable-failure degradation law, `grpc.health.v1` mapping where degraded keeps serving, `WireHealth` projection, and the TS availability gate.
- Tension: the health wire exists as a C# `TS_PROJECTION` cluster, but no master seam binds it to the TS availability gate yet.

[ONE_AGENT_TOOL_PROJECTION]-[QUEUED]: C# capability descriptors project to one MCP tool catalog decoded by TypeScript.
- Capability: agent-tool catalog with effect hints, cost preview, progress, resume token, and structured tool results.
- Shape: `csharp:Rasm.AppHost/Agent/mcp#TS_PROJECTION` projects each `CapabilityDescriptor` to `Microsoft.Extensions.AI` `CommandAIFunction : AIFunction`; `McpServerTool.Create(AIFunction, McpServerToolCreateOptions)` adopts it into MCP `tools/list`, and `typescript:services/agent/mcp#MCP_TRANSPORT` decodes `McpToolWire` with descriptor schema, hint annotations, and `estimatedCost`, then decodes `CostPreviewWire`, `ProgressNotificationWire`, and `ResumeTokenWire` before lifting each tool into `@effect/ai` `Tool.make` with the host `payload` as the sole agent-facing parameter.
- Unlocks: MCP tool catalog, effect-hint gating, cost dry-run, resumable SSE progress, `ToolResult`, and `ReceiptEnvelopeWire<ToolResultWire>` all derive from the descriptor source that SDK codegen reads.
- Anchors: official MCP C# SDK protocol ownership, `@effect/ai` `Toolkit`/`Tool.make`, descriptor JSON Schema as validating authority, `EffectClass` hint projection, irreversible operations wrapped in `ApprovalRequiredAIFunction`, and Python serving the same tools over its server-host leg.
- Tension: `ONE_CAPABILITY_CATALOG` owns descriptor-to-SDK codegen; this idea owns the distinct agent-tool decode surface so TypeScript never re-mints branch-side tool definitions.

[ONE_UI_INTENT_WIRE]-[QUEUED]: C# mints the control and layout-constraint vocabulary once, and a web/remote head materializes the same surface.
- Capability: single control-intent plus constraint-layout wire so a browser/remote head renders the exact control vocabulary and solves the exact layout the desktop renders.
- Shape: `csharp:Rasm.AppUi/Controls#CONTROL_MATERIALIZATION_SYSTEM` mints `ControlIntentWire` over the `ControlIntent` family and `csharp:Rasm.AppUi/Layout#CONSTRAINT_LAYOUT_ENGINE` mints `LayoutProgramWire` — the full ordered `Kiwi` program (`constraints` over the `LayoutConstraintWire`/`LayoutExprWire` relation rows, `introduction` variable-order, `edits` edit-variable set, `suggestions` suggested-value sequence), never solved pixel positions; `typescript:interchange` decodes both, and `typescript:ui/render` materializes the control tree and re-solves the identical Cassowary system through `kiwi.js` by feeding the program in introduction order, so the web head reproduces the desktop layout from the ordered program rather than a baked frame.
- Unlocks: a web/remote head that adds a control kind or layout constraint as one decode row, never a hand-authored second control vocabulary, a second layout solver, or a position-baked snapshot that drifts under a different viewport.
- Anchors: one-owner-per-wire-shape law, `ONE_CONTENT_IDENTITY` parity over control/layout payloads, AppUi `Kiwi` Cassowary-simplex ownership and the `kiwi.js` strength/relation-parity invariant, control-materialization ownership, and the `typescript:ui/render` decode-and-resolve leg.
- Tension: an under-constrained Cassowary system admits many valid assignments, so the `Kiwi` and `kiwi.js` simplex tableaus converge to identical positions only when `LayoutConstraintWire` also carries the variable-introduction order, the edit-variable set, and the suggested-value sequence the desktop solver fed; the wire transmits the full ordered constraint program, not just the relation set, and an order-free constraint dump is the divergence defect a redteam reads as silent per-viewport drift.
- Ripple: `csharp:Rasm.AppUi` `CONTROL_MATERIALIZATION_SYSTEM` + `CONSTRAINT_LAYOUT_ENGINE`.

[ONE_FEATURE_FLAG_PROJECTION]-[QUEUED]: one OpenFeature evaluation contract drives host and browser/edge flag draws identically.
- Capability: single feature-flag evaluation contract so browser/edge flag draws match the host evaluation under one provider semantics.
- Shape: `csharp:Rasm.AppHost/Delivery#TARGETED_DELIVERY_EXPERIMENTATION` exposes the `OpenFeature` `FeatureProvider` projecting the flag-evaluation contract over the `FlagDefinition`/`TargetingRule`/`Variant`/`RolloutSegment` rows; `typescript:interchange` exposes the same OpenFeature client provider so browser/edge `getBooleanValue`/`getObjectValue` draws resolve against the host evaluation context and rules, and the deterministic sticky-bucketing hash of subject-plus-flag reproduces bit-identically so a variant assigned host-side draws the same variant browser-side.
- Unlocks: targeting rules, experiment buckets, sticky A/B variant assignment, and kill-switches that a browser/edge consumer reads as one provider draw rather than a second flag engine, a divergent bucketer, or a hardcoded toggle.
- Anchors: OpenFeature provider/evaluation-context contract, AppHost targeted-delivery `XxHash3` sticky-bucketing ownership, `ONE_CONTENT_IDENTITY` for the cross-runtime bit-identical bucketing seed that keeps variant assignment convergent, `ONE_CAUSAL_TENANT_IDENTITY` for tenant-scoped targeting keys, and the `typescript:interchange` client-provider decode leg.
- Tension: bit-identical bucketing holds only for the static `FlagDefinition`/`TargetingRule` set plus the subject key, so a rule keyed on a server-private attribute the browser context never holds, or on a wall-clock rollout window the two consumers evaluate at different instants, resolves the variant differently across heads; the host projects the resolved rule snapshot plus an `evaluationContext` field manifest the browser draw must satisfy, and a flag whose targeting reads a host-only attribute degrades to a host-evaluated `Variant` the wire carries rather than a browser-side re-evaluation that silently picks a second bucket.
- Ripple: `csharp:Rasm.AppHost` `TARGETED_DELIVERY_EXPERIMENTATION`.

[ONE_OIDC_AUTH_FLOW]-[QUEUED]: C# mints the OIDC token flow, and the web head runs authorization-code + PKCE against the same provider.
- Capability: single OIDC provider contract so the TypeScript web flow and the host token mint share one authorization server, scopes, and claims.
- Shape: `csharp:Rasm.AppHost/Identity#RUNTIME_IDENTITY_AUTHZ_PLANE` runs the `OpenIddict.Client` OIDC token flow minting the `TokenLease` (access/refresh/expiry/DPoP-thumbprint); `typescript` runs the authorization-code + PKCE browser flow against the same provider, exchanging the code for DPoP-bound tokens whose claims the host issues, so the web head holds proof-of-possession tokens the host-side `AuthPlane` validates rather than bearer tokens a second scheme would re-bind.
- Unlocks: a web head, native head, and host that authenticate against one provider under one DPoP proof-of-possession binding, so a new scope, claim, or token-binding lands once on the authorization server rather than per-consumer auth code.
- Anchors: `OpenIddict.Client` OIDC ownership, authorization-code + PKCE public-client flow with DPoP sender-constrained tokens matching the host `TokenLease` thumbprint, `ONE_CAUSAL_TENANT_IDENTITY` for tenant-scoped principals, and branch `ONE_IDENTITY_STORE` as the distinct store concern (flow versus store).
- Tension: `ONE_IDENTITY_STORE` owns the principal/refresh-token store; this seam owns only the OIDC token flow, so the web PKCE leg never re-mints store rows.
- Ripple: `csharp:Rasm.AppHost` `RUNTIME_IDENTITY_AUTHZ_PLANE`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
