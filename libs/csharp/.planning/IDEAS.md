# [CSHARP_BRANCH_IDEAS]

The cross-package C# concert: ideas that couple two or more C# packages into one capability, distilled from the eight folder pools. A concept that lives inside one folder stays in that folder's `IDEAS.md`; a concept that spans C#, Python, and TypeScript at once lives in `libs/.planning/IDEAS.md` and is referenced here as a wire seam, never restated. Each idea is a card — a bracketed slug leader plus the capability, what it unlocks, and the gap or technique it draws on.

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

[CAPABILITY_CONTROL_PLANE]-[QUEUED]: one self-describing op catalog spans the runtime spine, the execution lane, and the UI.
- Capability: A typed, effect-classed, cost-modeled, permission-gated `CapabilityDescriptor` source governs operation discovery, grant preview, invocation, rollback, MCP projection, SDK codegen, command-palette exposure, and in-process function calling.
- Shape: `AppHost/Agent/capability` mints descriptors from `Compute/Runtime/channels` proto op surfaces and the `AppUi/Shell/commands` intent table; a shape-discriminated discovery fold drives intent planning, the grant/cost broker previews cost, and the command algebra routes invocation through a commit-or-rollback transaction on `Compute/Runtime/admission`.
- Unlocks: A new operation becomes one descriptor row consumed by MCP tools, in-process functions, the command palette, and the polyglot SDK, with no parallel client, hand-authored tool, or second op-metadata owner.
- Anchors: The dependency policy admits the official MCP C# SDK as the protocol owner; the registry is the single metadata source, and cross-language SDK-codegen consumption remains the `libs/.planning` capability-catalog seam.

[ONE_CONTENT_IDENTITY_FABRIC]-[QUEUED]: one `XxHash128` content-address seed threads cache, blob residence, diff, model-result reuse, and codecs.
- Capability: A single identity law covers sealed snapshot hash, Compute interchange identity, incremental topology naming hash, per-node reactive graph key, artifact bytes, deflection, tolerance, and metadata.
- Shape: `Rasm.Geometry/Spatial/reconciliation` emits canonical geometry bytes; `Compute/Runtime/codecs` folds artifact bytes plus deflection and tolerance into the key; `Persistence/Query/cache` content-addresses blob residence and the model-result horizon; `Compute/Model/inference` and the reactive scheduler reuse sub-results by key.
- Unlocks: Warm-start blobs, result-cache hits, deduped snapshot chunks, clean reactive-graph nodes, and retessellation at a changed deflection all resolve through one key family instead of package-local identity schemes.
- Anchors: The content-addressed build graph and self-adjusting computation run over the one seed; the cross-runtime bit-identical reproduction of this seed stays in the `libs/.planning` content-address seam.

[INCREMENTAL_COMPUTE_PIPELINE]-[QUEUED]: a content-keyed reactive recompute engine spans the execution lane, durable store, and UI.
- Capability: Demand-driven recomputation limits a parametric edit to the transitive dirty closure and replays every clean node from the deterministic result cache.
- Shape: `Compute/Runtime/scheduling` keys each job-graph node on input content digests, `Compute/Model/inference` supplies result-cache replay, `Compute/Solver/sweep` bounds the dirty closure per frame, `Persistence/Sync/collaboration` records moved nodes through the op-log changefeed, and `AppUi` receives only genuine deltas.
- Unlocks: Interactive AEC edits rerun only affected tessellate, solve, encode, and field chains while unchanged branches return from cache and coupled geometry/field iterations transmit true deltas without a second versioning scheme.
- Anchors: Demand-driven incremental computation composes the content identity, receipt, result-cache, and scheduler owners already adjacent to the lanes instead of minting a memoization or dependency tracker.

[DETERMINISTIC_REPLAY_OBSERVATORY]-[QUEUED]: a reproducibility kernel binds the runtime spine, durable op-log, and notebook surface.
- Capability: Deterministic execution records, proves, exports, and replays capability work as a hash-chained, content-addressed event log with signed authorization context.
- Shape: `AppHost/Runtime/determinism` pins RNG, float mode, and environment fingerprint; the event log rides `Persistence/Sync/collaboration`; replay-verify proves each step by content hash; the macro engine records and replays units; `AppUi/Editing/notebook` pins capabilities and exports replay bundles with cell-edit ops projected onto the op-log CRDT delta.
- Unlocks: A notebook reruns to an identical result, a grant decision derives from the chain alone, and a reviewer reconstructs who computed what and when from durable receipts instead of process-local state.
- Anchors: Chained-event-log and verifiable-credential techniques ride the one content-address seed; cross-machine replay-verify consumes the Compute provider-determinism fingerprint, and signed grant attestation closes the in-process-only authorization gap.

[GEOMETRY_FLOW_SEAM]-[QUEUED]: one geometry and IFC flow runs from the kernel through codecs, store, and renderer.
- Capability: The C# geometry source-of-truth, IFC semantic graph, content-addressed tessellation, blob residence, federated graph, clash compute, structural diff, material shading, and viewport rendering meet only through owned seams.
- Shape: The kernel `Rasm.Geometry` owns geometry; `Compute/Runtime/codecs` content-addresses artifacts and runs the two-hop IFC-to-geometry tessellation bridge; `Rasm.Bim/Exchange/tessellation` builds requests; `Rasm.Bim/Exchange/import` owns IFC semantics; `Persistence/Query/cache` owns blob residence; `Persistence/federation` ingests the model; `AppUi/Render/viewport` renders field receipts.
- Unlocks: The spatial index feeds Compute clash detection, canonical adjacency bytes feed Persistence structural diff, and Materials BSDF evaluation shades the viewport path trace without duplicate geometry, IFC, or material owners.
- Anchors: The one-owner-per-runtime geometry law governs the seam; the kernel clash-seam node-link contract and Compute clash decode are the frozen contract, with the residual two-sided golden fixture carried by folder tasks.

[FEDERATED_COORDINATION_COCKPIT]-[QUEUED]: an openBIM coordination surface composes BIM semantics, durable annotation, and the UI viewport.
- Capability: BCF topics, GlobalId-stable diffs, anchored annotations, CDE sync, federated entity rules, viewpoint view state, CRDT comment threads, and board snapshots operate as one coordination workflow.
- Shape: `Rasm.Bim/coordination` owns BCF 3.0 topic, component, comment, and exchange semantics plus the GlobalId-stable diff; `Persistence/annotation` owns anchored annotation and CDE sync; `Persistence/federation` owns the federated graph and rule engine; `AppUi/coordination` owns the board projection over viewpoint state, comments, and tiles.
- Unlocks: In-app clash and issue coordination lands with no new persistence owner and no second BCF model; the diff joins by GlobalId plus content key, IDS audit and rules ride one graph, and issues round-trip through the op-log changefeed.
- Anchors: BCF 3.0, IDS 1.0, bSDD dictionary binding, and the anchor algebra unify comments, conflicts, presence, and blame around the one Bim-owned semantic model.

[TELEMETRY_LAKE_ANALYTICS]-[QUEUED]: a four-signal observability lake folds live signals and durable receipts into one analytics surface.
- Capability: Traces, metrics, logs, profiles, HLC-ordered receipts, in-store classifications, cost catalog rows, and dashboard series share one correlation primitive and one analytical engine.
- Shape: `AppHost/Observability/telemetry` governs telemetry through minted identities and the redaction taxonomy; `AppHost/Runtime/ports` orders cross-process receipts through HLC fan-in; `Persistence/Store/profiles` classifies durable receipts into a 5D cost catalog and DuckDB/DuckLake columnar tier; `AppUi/Charts/dashboards` reads aggregates as cross-filtered chart tiles.
- Unlocks: Production flame graphs scoped to a span, TimescaleDB hypertable time-series telemetry, and dashboards fed by N solve nodes become row additions on the metrics axis rather than parallel surfaces.
- Anchors: OTel profiles, GenAI semantic conventions, DuckLake over object-store Parquet, and Arrow Flight zero-copy egress ride the existing correlation primitive.

[CROSS_PROCESS_TOPOLOGY_FABRIC]-[QUEUED]: one host-modality switch drives in-process, paired, companion, sidecar, hub, and service topologies.
- Capability: Universal-standalone and Rhino/GH2 host modes share one topology axis covering placement, surface host, companion farms, object-graph sync hubs, sidecar ingest, guarded writes, secret leases, tenancy, OS binding, and multi-peer session rosters.
- Shape: `AppHost/Runtime/profiles` owns the host-profile axis, `Persistence/Store/profiles` owns the placement fold, and `AppUi/Shell/hosts` owns the surface-host axis; companion compute farms are substrate rows with warm-affinity reordering, object-graph sync hubs are sync-transport rows over the op-log HLC changefeed, sidecar ingest is capture-direction placement, and `AppHost/Runtime/time#FENCING_TOKEN` owns monotone guarded-write tokens.
- Unlocks: Paired editing, self-updating farms, health-gated rolling updates, cross-process result reuse by content address, live-wire studio bindings, and multi-peer rosters land as rows or columns on settled axes.
- Anchors: Fencing-token correctness, workload-identity short-lived credentials in `AppHost/Runtime/config#SECRET_LEASE`, certified OPC-UA and MQTTnet bindings, seven host-free port records, and Unix/macOS launchd/systemd binding beside `Wire/companion#SERVICE_HOST` make a new OS adapter-row set or transport `BindAddress` case.
- Tension: The browser-consumer leg of any web-fed topology remains the `libs/.planning` web-fed wire seam, and secrets, coordination, and tenancy stay row data on existing axes rather than new ports or sub-domains.

[ENCODED_GEOMETRY_PLUGIN_CONTRACT]-[QUEUED]: one geometry-encoding vocabulary binds sandboxed solver plugins to the tensor lane and kernel.
- Capability: Solver, mesher, optimizer, CAM-post, and field-codec plugins negotiate their input and output representations through the same `GeometryEncoding` vocabulary used by the tensor lane, remote proto geometry family, and model-zoo conformance triad.
- Shape: `AppHost/Sandbox/solver#SOLVER_KIND` declares each extension kind's representations as `EncodingKind` rows that project onto `Compute/Tensor/residency#GEOMETRY_ENCODING`; hosting `Negotiate` proves lossless round-trip through `EncodedTensor`; plugin ops project into `AppHost/Agent/capability`; `GeometryPacking` materializes per-case `EncodingChannel` rows from kernel geometry.
- Unlocks: A third-party extension becomes one kind contract over the shared representation axis and one capability descriptor, never a plugin-private representation or solver-page literal.
- Anchors: The finalized `GeometryEncoding` cases, eight-row `EncodingChannel` lattice, kind-contract effect defaults, `Compute/Solver/discretization#DISCRETIZATION_MESH` `FieldSpace`, and `Rasm.Fabrication/Toolpath/motion#CAM_MOTION` `Motion` stream carry the `Field` and `Toolpath` representation extensions.

[CROSS_PACKAGE_TRANSPORT_CONCERT]-[QUEUED]: one gRPC tuning algebra spans three disjoint transport axes that meet only at proto vocabulary and content identity.
- Capability: Intra-suite channels, external live-wire transports, durable CRDT/object-graph sync, agent integration, compression, retry, and content identity stay on their owning axes.
- Shape: `Compute/Runtime/channels#GrpcChannelPolicy` centralizes channel tuning; `AppHost/live-wire#ExternalTransport` owns OPC-UA and MQTT policy rows; `Persistence/Sync/collaboration#SyncTransport` owns durable peer sync, CRDT sync, and the Speckle-like diff case; all three align only at `#PROTO_VOCABULARY` and `ContentIdentity`.
- Unlocks: A new transport is one `[SmartEnum]` row on the correct axis, a new external system is one case on its axis, and a new agent integration is one `CapabilityDescriptor` row that projects to MCP and C#/TypeScript/Python SDKs.
- Anchors: `DisableResolverServiceConfig=true` leaves hop retry to the AppHost keyed Polly pipeline; inbox-only `Gzip`/`Deflate` compression keeps zstd or brotli as a `CompressionProviders` row; the C#-terminates-external law makes a TypeScript Speckle or MQTT client the drift defect.

[ROBUST_ARRANGEMENT_SUBSTRATE]-[QUEUED]: the kernel exact-arithmetic constrained-Delaunay and straight-skeleton arrangement is the single planar and volumetric substrate every AEC-domain geometry consumer composes.
- Capability: Managed exact predicates, constrained triangulation and tetrahedralization, straight skeleton wavefronts, generalized winding classification, nesting offsets, hidden-line projection, medial-axis construction, boolean classification, and solver meshing share one kernel arrangement.
- Shape: `Rasm.Geometry/tessellation#BOWYER_WATSON_DELAUNAY` and `Rasm.Geometry/offsetting#STRAIGHT_SKELETON` build on `Numerics/predicates#INDIRECT_PREDICATES` LPI/TPI exact signs and `Spatial/index#GENERALIZED_WINDING`; Fabrication nesting and projection, Materials construction layout, Bim exchange, and Compute solver discretization consume the arrangement through settled wires.
- Unlocks: Fully managed exact boolean, offset, medial, and volumetric geometry retire native CSG deploy pressure, land Fabrication and Materials consumers on the same arrangement, and seed the solver mesh from constrained tetrahedralization.
- Anchors: Cherchi/Attene-style exact mesh booleans over implicit points, the landed LPI/TPI family, and the one-owner-per-runtime geometry law keep AEC-domain consumers aligned to `Build`, `ToMesh`, `Apply`, and `SkeletonGraph` without coupling to wavefront or `SimplexStore` interiors.
- Tension: `Processing/repair#BOOLEAN_NATIVE_ASSET` remains the upstream-blocked native row until the managed companion body carries equivalent boolean classification.

[AEC_SIMULATION_BRIDGE]-[QUEUED]: structural, thermal, and energy analysis solve over the federated model rather than a re-modeled mesh.
- Capability: Compute physics solves, BIM element semantics, Materials engineering properties, field substrates, tensor factorization, neural-field surrogates, DOE sweeps, optimization, and Persistence federation receipts operate as one analysis owner.
- Shape: `Solver/contract#SOLVE_FOLD` owns physics, boundary condition, and element assembly over `Solver/discretization`; `Solver/optimizer` owns design-space search; `Solver/sweep` owns N-dimensional DOE under the frame-budget governor; the field substrate reads `Rasm/Vectors`, numeric factorization reads `Compute/Tensor/blas`, inference reads `Compute/Model/inference`, materials read `Rasm.Materials/physical-properties#PROPERTY_SETS`, and analysis domains read `Rasm.Bim/Model/query#ELEMENT_SET`.
- Unlocks: Structural frames, thermal envelopes, surrogate-accelerated DOE, and digital-twin clash scoring all resolve by federated element identity; a new physics is one solve-contract row reading the same material and element vocabulary.
- Anchors: IFC element semantics, EN 1993 and ASHRAE constitutive tables, neural operators, implicit neural fields, the content-keyed result cache, the `libs/.planning` graduation-evidence `model-asset` ONNX seam, and C# inference/harvest ownership avoid an in-process training loop.

[OPEN_MATERIAL_STANDARD_WIRE]-[QUEUED]: OpenPBR and MaterialX convergence becomes one branch-wide appearance interchange minted once in Materials, carried on the BIM exchange wire, and shaded in the viewport.
- Capability: OpenPBR vectors, MaterialX node graphs, conductor IOR data, measured spectral libraries, tile structural metadata, tile-pyramid payloads, and path-trace shading share one Materials-owned wire.
- Shape: `Rasm.Materials/Appearance/bsdf#OPENPBR_SLAB_LAYERING` lowers the OpenPBR Surface 1.1 stack-of-slabs lobe family; `Appearance/interchange#WIRE_PROJECTION` mints `MaterialWire` and `MtlxDocument`; `Rasm.Bim/exchange#TILE_METADATA` carries material ids on `EXT_structural_metadata`; `Rasm.Compute/Runtime/codecs#TILE_PARTITION` content-addresses material payloads; `Rasm.AppUi/Render/viewport#PATH_TRACE` shades ReSTIR from the decoded vector.
- Unlocks: A measured material round-trips to OpenPBR/MaterialX DCC tools and renders consistently across the app viewport, BIM exchange, and web peer; a new measured material is one `MaterialLibrary` row carried to IFC/glTF and path tracing.
- Anchors: OpenPBR Surface 1.1, MaterialX 1.39.4, EPFL RGL goniophotometer measured-BRDF capture, and the C#-mints/peers-decode law make a second OpenPBR or MaterialX mint the drift defect, with Python and TypeScript decode legs held by the `libs/.planning` cross-language seam.

[SYMBOLIC_PARAMETRIC_ALGEBRA]-[QUEUED]: a closed symbolic-expression algebra owner turns parametric formulas, unit laws, and analytic gradients into one CAS surface.
- Capability: The unfilled `Rasm.Compute/Symbolic/` sub-domain internalizes CAS parsing, simplification, differentiation, compilation, dimensional checking, formula DSL admission, symbolic quadrature, solver gradients, Materials constitutive curves, and inward symbolic graduation.
- Shape: `Rasm.Compute/Symbolic/expression` wraps `MathNet.Symbolics` `Expression`, `Rational`, `Algebraic`, `Trigonometric`, `Calculus.differentiate`, `Compile.compileExpression`, `Infix`, and `LaTeX` surfaces; `FParsec` owns infix parse; Persistence cost catalog formulas, federation QTO formulas, Tensor quadrature, Solver optimizer gradients, Symbolic units, and Materials physical-property curves compose `SymbolicExpr` instead of string `eval` or hand-bound delegates.
- Unlocks: Exact analytic gradients, parsed and typed cost/QTO formula DSLs, symbolic dimensional-consistency proof, compiled delegate reuse by content key, and inward Python `sympy`/`symengine` graduation onto a C# `SymbolicExpr` all land through one expression owner.
- Anchors: `MathNet.Symbolics`, `MathNet.Numerics.FSharp`, and `FParsec` are manifest-present, README-registered under `[SYMBOLIC]`, and catalogued at `Rasm.Compute/.api/api-mathnet-symbolics.md`; the content-addressed build graph keys `SymbolicExpr` by canonical `XxHash128` normal form; the `libs/.planning` `ONE_GRADUATION_EVIDENCE` symbolic `HandoffAxis` carries the inward seam.
- Tension: The catalogue `[4]-[RESEARCH]` flags for exact `Compile.compileExpression{,1,2,3}` and `compileComplexExpression` arity, `Func<>` return shape, and `SymbolicExpression` operator/conversion surface close before any compile fence transcribes.

[REALITY_CAPTURE_TO_BIM]-[QUEUED]: a scan-to-model pipeline binds kernel registration, Compute splat payloads, BIM semantics, Persistence lineage, and AppUi capture into one as-built reconstruction flow.
- Capability: Point cloud registration, segmentation, splat residency, primitive fitting, BIM element classification, property confidence, federated lineage, and LiDAR-anchored viewport playback produce classified BIM elements from captured reality.
- Shape: `Rasm/Vectors` `Align` and cloud-ICP register captures, greenfield `Rasm/Geometry/spatial` indexes and segments clouds, `Rasm.Compute/Runtime/codecs` content-addresses splat and point tiles, planned `Rasm.Bim/reconstruction` folds planar/cylindrical/torus/freeform primitives into `BimElement` rows, `Rasm.Persistence/Query/federation` joins reconstructed elements to source-cloud lineage by `(GeometryHash, content-key)`, and `Rasm.AppUi/Render/reality` overlays capture against the reconstructed model.
- Unlocks: An as-built workflow lands with no second geometry owner; a new primitive fit is one `ReconstructionPrimitive` row reading kernel cloud algebra, and offline RANSAC or learned segmentation returns through the `libs/.planning` graduation `reconstructed-mesh` or `topology-graph` geometry axis.
- Anchors: Scan-to-BIM and Gaussian-splat reconstruction maturity, one-owner-per-runtime geometry law, Python `open3d` as offline peer producer, existing AppUi `realitycapture` and `SPLAT_REALITY_CAPTURE` owners, and the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest identify `Rasm.Bim/reconstruction` as the missing cloud-to-semantics arm.

[DRAFTING_DOCUMENT_PRODUCTION]-[QUEUED]: one branch-wide construction-documentation rail emits drawings, schedules, and reports from the federated model.
- Capability: Hidden-line projection, 2D drawing geometry, sheet sets, dimensioning, schedule and quantity tables, OOXML/DWG/DXF export, and model-backed reporting compose existing owners instead of per-format document writers.
- Shape: `Rasm.Fabrication/Posting/projection` BSP visibility and kernel arrangements produce drawing geometry; `Rasm.AppUi/Render/drafting` owns ISO/ANSI/JIS sheet sets and ASME Y14.5 dimensioning; `Rasm.Persistence/Query/federation` `ElementSet` and `Rasm.Persistence/Sync/schedule` feed schedules and quantities; `Rasm.AppUi/Render/capture` emits OOXML, DWG, and DXF through ACadSharp, netDxf, and DocumentFormat.OpenXml.
- Unlocks: Coordinated drawing sets, live dimensions, quantity takeoff, and 4D construction-state reports become views over the federated model; a new sheet standard is one title-block row and a new export format is one writer row.
- Anchors: The admitted and catalogued drafting-export surface, AppUi drafting and capture owners, Fabrication projection, the viewport camera basis, and federated-model-as-single-source-of-truth law keep the drawing a model view rather than a re-modeled document.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
