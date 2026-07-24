# [CSHARP_BRANCH_TASKLOG]

Cross-package C# work no single folder owns: the cross-folder seam arbitrations and the host-boundary seams the future app root composes. Per-folder work lives in each package's own task pool; cross-language amendments (the CRDT wire-vocabulary change, the SDK-codegen consumption) live at the cross-libs tier and are referenced as wire seams, never restated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[BATCH_SEAM_LEDGER]-[QUEUED]: Ledger the producer→landing record-batch seams with their custody split.
- Capability: one seam table naming each producer wire, its batch-shape owner, its landing slot, and its content-key metadata columns — the arbitration keeping four folder pairs on one discipline.
- Shape: seam rows on `libs/csharp/.planning/ARCHITECTURE.md#SEAMS`, each pair mirrored at BOTH endpoint registries with identical `[KIND]` and direction — `libs/csharp/Rasm/ARCHITECTURE.md`, `libs/csharp/Rasm.Compute/ARCHITECTURE.md`, `libs/csharp/Rasm.Element/ARCHITECTURE.md`, and `libs/csharp/Rasm.Materials/ARCHITECTURE.md` each against `libs/csharp/Rasm.Persistence/ARCHITECTURE.md` — closure holds only when all five endpoint pages carry their edges.
- Unlocks: `[ANALYTICS_LAKE_CONCERT]` holds as one discipline instead of four bespoke handoffs.
- Anchors: the kernel/Compute/Element/Materials egress cards, Persistence `[PERS_L1]`, the seam-registry grammar.

[SUBSTRATE_TIER_PROMOTION]-[QUEUED]: Promote the four multi-folder packages to branch substrate — registry rows, one shared catalog each, folder tiers collapse to registration.
- Capability: a package consumed by two or more folders registers once at the branch tier with one member-truth catalog, so folder catalogs stop duplicating substrate truth and a new consumer registers instead of re-cataloging.
- Shape: branch README `[02]` rows and `libs/csharp/.api/` catalogs for `PollinationSDK`, `Microsoft.Data.Sqlite`, `MQTTnet`, and `CloudNative.CloudEvents.Mqtt` (new label clusters where none fits); the folder-tier catalogs re-cut to genuine domain overlays or registrations; the Compute MQTT subscribe rows land at the substrate catalog.
- Unlocks: the AppHost and Compute `[MQTT_RECEIVE_ARM]`/`[BROKER_INGEST_ROWS]` arms gain their second catalog route; the two-or-more-folders registry law holds bidirectionally for all four.
- Anchors: the branch README earn-a-row law; the catalog-alignment touch-point set; the true-substrate catalog doctrine.
- Atomic: four packages, one touch-point sweep each.

[L3_REFERENCE_PROSE_RECONCILE]-[QUEUED]: Branch L3 reference prose reconciles with the manifest reality — one stated intra-stratum order.
- Capability: the strata law states the realized intra-L3 reference order, so a ledger reader and a manifest reader reach one graph.
- Shape: `libs/csharp/.planning/ARCHITECTURE.md` `[02]` L3 bullet and the import-strata mermaid; the `libs/csharp/Rasm.AppUi/ARCHITECTURE.md` lead's peers-by-contract clause in the same pass.
- Unlocks: strata-law citations stop contradicting `Rasm.AppUi.csproj` and `Rasm.Compute.csproj`.
- Anchors: `Rasm.AppUi.csproj` referencing `{Rasm, Rasm.AppHost, Rasm.Compute, Rasm.Persistence}`; `Rasm.Compute.csproj` referencing `{Rasm, Rasm.AppHost, Rasm.Element, Rasm.Persistence}`; `Rasm.AppHost.csproj` referencing `{Rasm}` alone.
- Tension: the manifests realize an acyclic intra-L3 order (`Rasm.AppHost` below `Rasm.Persistence`/`Rasm.Compute` below `Rasm.AppUi`) while the prose states contract-only peer alignment; one of the two is the law and the other re-cuts.

[GRPC_TOOLS_ROW_RECUT]-[QUEUED]: Branch README `Grpc.Tools` row states the per-consumer codegen truth.
- Capability: the registry row carries the real codegen split, so a reader never infers message-only compilation for the client-codegen consumer.
- Shape: `libs/csharp/.planning/README.md` `Grpc.Tools` row — the message-only `GrpcServices=None` characterization re-cuts against the Compute client codegen.
- Unlocks: the registry row survives a codegen audit without a false message-only claim.
- Anchors: `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md` `GrpcServices=Client` + `PrivateAssets=all`; `libs/csharp/Rasm.Persistence/.planning/Query/federation.md` zero-codegen posture.
- Atomic: one registry row.

[CLAIM_FIELD_MAP]-[BLOCKED]: Task-named claim families map onto `BenchmarkReceipt` at the AppHost benchmarks owner.
- Capability: each task-named family fills `BenchmarkReceipt` through the claim-field adapter, the held-median side sourced from capture truth.
- Shape: map rows on `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md` `#CLAIM_FIELD_MAP`.
- Unlocks: `[BENCH_PROOF_CONCERT]` — one verdict grammar closes over every claim family.
- Anchors: `Observability/benchmarks.md` claim rows; `libs/csharp/Rasm.Grasshopper/.planning/Platform/capture.md` `CaptureBreach`.
- Tension: armed when `Platform/capture.md` adds the producing bound to `CaptureBreach`, allowing the adapter to fill the held-median side without policy re-derivation.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[NPGSQL_ROOT_COMPOSITION]-[COMPLETE]: `SignalGovernance.StoreDriver` registers `AddNpgsql` + `AddEntityFrameworkCoreInstrumentation` tracing and `AddNpgsqlInstrumentation` metrics at the store-composing root (Rasm.AppHost `Observability/telemetry#SIGNAL_GOVERNANCE`); `NpgsqlDataSourceBuilder.Name` assigns per logical database in the Persistence `Store/provisioning` `Source` fence.
[PLUGIN_ALC_PROVIDERS]-[COMPLETE]: `PluginTelemetryHost.Open` realizes the per-ALC trace-and-metric capsule — `AddMetrics` `IMeterFactory`, explicit provider handles, idempotent unload-ordered `ForceFlush` then `Dispose`; exporter disk retry remains process-scoped (Rasm.AppHost `Observability/instruments#PROVIDER_LIFETIME`).
[KAFKA_CONSUMER_INSTRUMENTATION]-[COMPLETE]: `StoreWire<TKey,TValue>` registers `AddKafkaConsumerInstrumentation` on both providers at the signal root; the Persistence ingress owner landed at `Rasm.Persistence/.planning/Version/ingress.md` handing the `CdcIngress.Bind` instrumented builder.
[RHINO_TELEMETRY_ANCHOR]-[COMPLETE]: `Microsoft.Extensions.Telemetry.Abstractions` and `Microsoft.Extensions.Logging.Abstractions` anchored in the Rasm.Rhino manifest and README registry; the HostUtils cloud-log/exception tap card lives at the Rasm.Rhino folder tier.
