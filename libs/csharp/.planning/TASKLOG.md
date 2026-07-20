# [CSHARP_BRANCH_TASKLOG]

Cross-package C# work no single folder owns: the cross-folder seam arbitrations and the host-boundary seams the future app root composes. Per-folder work lives in each package's own task pool; cross-language amendments (the CRDT wire-vocabulary change, the SDK-codegen consumption) live at the cross-libs tier and are referenced as wire seams, never restated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[NPGSQL_ROOT_COMPOSITION]-[QUEUED]: AppHost-root Npgsql telemetry composition over the Persistence-owned driver.
- Capability: the PORT-peer arbitration — Persistence owns the driver and data-source builders; the app root alone registers `AddNpgsql()` and `AddNpgsqlInstrumentation()`, so no downward reference forms.
- Shape: two registration rows in the AppHost signal root; `NpgsqlDataSourceBuilder.Name` set per logical database on the Persistence side keys `db.client.connection.pool.name`.
- Unlocks: driver spans and db-semconv pool metrics joined to store receipts without a Persistence→AppHost edge.
- Anchors: `api-npgsql-opentelemetry` branch catalog; AppHost Observability telemetry; Persistence store observability.
- Atomic: two builder rows and one data-source name row.

[PLUGIN_ALC_PROVIDERS]-[QUEUED]: Per-ALC telemetry provider lifetime at the Rhino and Grasshopper host roots.
- Capability: each plugin load context owns its providers — explicit `Sdk.CreateTracerProviderBuilder()`/`Sdk.CreateMeterProviderBuilder()` handles, a minimal `ServiceCollection().AddMetrics()` yielding the per-ALC `IMeterFactory`, and `AssemblyLoadContext.Unloading` running `ForceFlush` then `Dispose`.
- Shape: one bootstrap capsule composed at each host-boundary app root, its resource stamping a plugin discriminator so co-resident plugins separate downstream.
- Unlocks: co-resident Rasm plugins in one Rhino process without Meter collision or dropped batch tails.
- Anchors: `api-opentelemetry` and `api-opentelemetry-exporter-otlp` branch catalogs; AppHost Observability telemetry; the two host-boundary roots.

[KAFKA_CONSUMER_INSTRUMENTATION]-[QUEUED]: AppHost-root Kafka consumer instrumentation over the Persistence-owned ingress leg.
- Capability: `AddKafkaConsumerInstrumentation<TKey,TValue>` registers at the signal root over the Persistence `InstrumentedConsumerBuilder` consume leg — the PORT-peer arbitration mirroring the Npgsql row, closing the producer-only Kafka telemetry asymmetry.
- Shape: one registration row in the AppHost signal root beside the producer row; the Persistence ingress owner (`libs/csharp/Rasm.Persistence/.planning/Version/ingress.md`) names the builder it hands.
- Unlocks: CDC ingress spans and consumer metrics join the drain trace end to end.
- Anchors: `libs/csharp/.api/api-otel-instrumentation-confluentkafka.md` `AddKafkaConsumerInstrumentation<TKey,TValue>`; Persistence `[PERS-V4]`/`[0007]`; the `[NPGSQL_ROOT_COMPOSITION]` precedent.
- Atomic: one registration-row arbitration.

[BATCH_SEAM_LEDGER]-[QUEUED]: Ledger the producer→landing record-batch seams with their custody split.
- Capability: one seam table naming each producer wire, its batch-shape owner, its landing slot, and its content-key metadata columns — the arbitration keeping four folder pairs on one discipline.
- Shape: seam rows on `libs/csharp/.planning/ARCHITECTURE.md#SEAMS`, each pair mirrored at BOTH endpoint registries with identical `[KIND]` and direction — `libs/csharp/Rasm/ARCHITECTURE.md`, `libs/csharp/Rasm.Compute/ARCHITECTURE.md`, `libs/csharp/Rasm.Element/ARCHITECTURE.md`, and `libs/csharp/Rasm.Materials/ARCHITECTURE.md` each against `libs/csharp/Rasm.Persistence/ARCHITECTURE.md` — closure holds only when all five endpoint pages carry their edges.
- Unlocks: `[ANALYTICS_LAKE_CONCERT]` holds as one discipline instead of four bespoke handoffs.
- Anchors: the kernel/Compute/Element/Materials egress cards, Persistence `[PERS-L1]`, the seam-registry grammar.

[CLAIM_FIELD_MAP]-[QUEUED]: Map each folder claim family onto the `BenchmarkReceipt` fields.
- Capability: one mapping table — family to suite/case/`HostEvidence`/duration/allocation/verdict columns, where `HostEvidence` binds as the whole typed projection (fingerprint, determinism tag, environment), never a bare host name — with divergent fields named for re-cut instead of tolerated as sibling grammars.
- Shape: mapping rows beside the AppHost gate ingestion rows at `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md#BENCHMARK_RECEIPT`, each folder family citing its origin card.
- Unlocks: `[BENCH_PROOF_CONCERT]` verdict-grammar law becomes checkable per family.
- Anchors: `BenchmarkReceipt` field roster, the folder claim-family cards, the Persistence claim custody.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[RHINO_TELEMETRY_ANCHOR]-[COMPLETE]: `Microsoft.Extensions.Telemetry.Abstractions` and `Microsoft.Extensions.Logging.Abstractions` anchored in the Rasm.Rhino manifest and README registry; the HostUtils cloud-log/exception tap card lives at the Rasm.Rhino folder tier.
