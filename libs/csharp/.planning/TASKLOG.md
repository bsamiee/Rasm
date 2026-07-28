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

[SUBSTRATE_TIER_PROMOTION]-[QUEUED]: Promote every proven multi-folder package to branch substrate — registry rows, one shared catalog each, folder tiers collapse to registration.
- Capability: a package consumed by two or more folders registers once at the branch tier with one member-truth catalog, so folder catalogs stop duplicating substrate truth and a new consumer registers instead of re-cataloging.
- Shape: branch README `[02]` rows and `libs/csharp/.api/` catalogs for `ACadSharp`, `Alimer.Bindings.MeshOptimizer`, `Apache.Arrow`, the three `CloudNative.CloudEvents` rows, `MessagePack`, `Microsoft.Data.Sqlite`, `NATS.Net`, `NREL.OpenStudio.macOS-arm64`, the two `NetTopologySuite.IO` rows, `PeterO.Numbers`, `PollinationSDK`, the four `SharpGLTF` rows, the three `Silk.NET.WebGPU` rows, the two `Speckle` rows, `geometry3Sharp`, and `pocketken.H3`; each folder-tier catalog re-cuts to a genuine domain overlay or a registration.
- Unlocks: the two-or-more-folders registry law holds bidirectionally across the branch, and a new consumer of any of them registers instead of minting a third copy.
- Anchors: the branch README earn-a-row law; `RULINGS.md` `[04]-[STRUCTURE]` tier and dual-home rows; the catalog-alignment touch-point set.
- Tension: the roster is the CSPROJ-consumption census, not a guess — each entry has two-or-more consuming `.csproj` rows, two-or-more folder-tier catalogs, and no branch catalog; host-SDK partitions (`Eto`, `Grasshopper2`) and multi-package files are excluded by the same ruling and are not defects.
- Atomic: one package, one touch-point sweep each.

[GRPC_TOOLS_ROW_RECUT]-[QUEUED]: Branch README `Grpc.Tools` row states the per-consumer codegen truth.
- Capability: the registry row carries the real codegen split, so a reader never infers message-only compilation for the client-codegen consumer.
- Shape: `libs/csharp/.planning/README.md` `Grpc.Tools` row — the message-only `GrpcServices=None` characterization re-cuts against the Compute client codegen.
- Unlocks: the registry row survives a codegen audit without a false message-only claim.
- Anchors: `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md` `GrpcServices=Client` + `PrivateAssets=all`; `libs/csharp/Rasm.Persistence/.planning/Query/federation.md` zero-codegen posture.
- Atomic: one registry row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[L3_REFERENCE_PROSE_RECONCILE]-[COMPLETE]: the branch `[02]-[STRATA]` rows state the realized app-platform order and verify exactly against the live `ProjectReference` set — `Rasm.AppHost` holds `{Rasm}`, `Rasm.Persistence` `{Rasm, Rasm.Element}`, `Rasm.Compute` `{Rasm, Rasm.Element, Rasm.AppHost, Rasm.Persistence}`, `Rasm.AppUi` `{Rasm, Rasm.AppHost, Rasm.Compute, Rasm.Persistence}`. `Rasm.AppUi/ARCHITECTURE.md`'s lead carried the surviving half — a peers-by-contract clause its own csproj refutes — and now names the S4 consuming-leaf reference set.
[BATCH_SEAM_LEDGER]-[COMPLETE]: every producer→landing pair mirrors at both endpoint `ARCHITECTURE#SEAMS` registries — `Rasm.Element` and `Rasm.Materials` carry `[WIRE]: AnalyticsSchema` against `Rasm.Persistence` for the relational residence handoff, `Rasm.Compute` carries `[WIRE]: LakeGeneration` for the lake landing. Both open claims refuted on disk: the Compute counterpart had landed already, and it was MISLABELLED `AnalyticsSchema`, a contract no Compute fence composes. No kernel pair exists or may exist — `Rasm` sits at S0 and reaches no landing coordinate.
[CLAIM_FIELD_MAP]-[COMPLETE]: `#CLAIM_FIELD_MAP` carries rows `[08]` `BudgetBreach` and `[09]` `CaptureBreach` beside the seven standing families, each filling median and p95 from its one measured cost and its producing bound as `ReferenceEvidence.Median`; a single-sample law seats that projection so no adapter fabricates a distribution one judgment never produced.
[NPGSQL_ROOT_COMPOSITION]-[COMPLETE]: `SignalGovernance.StoreDriver` registers `AddNpgsql` + `AddEntityFrameworkCoreInstrumentation` tracing and `AddNpgsqlInstrumentation` metrics at the store-composing root (Rasm.AppHost `Observability/telemetry#SIGNAL_GOVERNANCE`); `NpgsqlDataSourceBuilder.Name` assigns per logical database in the Persistence `Store/provisioning` `Source` fence.
[PLUGIN_ALC_PROVIDERS]-[COMPLETE]: `PluginTelemetryHost.Open` realizes the per-ALC trace-and-metric capsule — `AddMetrics` `IMeterFactory`, explicit provider handles, idempotent unload-ordered `ForceFlush` then `Dispose`; exporter disk retry remains process-scoped (Rasm.AppHost `Observability/instruments#PROVIDER_LIFETIME`).
[KAFKA_CONSUMER_INSTRUMENTATION]-[COMPLETE]: `StoreWire<TKey,TValue>` registers `AddKafkaConsumerInstrumentation` on both providers at the signal root; the Persistence ingress owner landed at `Rasm.Persistence/.planning/Version/ingress.md` handing the `CdcIngress.Bind` instrumented builder.
[RHINO_TELEMETRY_ANCHOR]-[COMPLETE]: `Microsoft.Extensions.Telemetry.Abstractions` and `Microsoft.Extensions.Logging.Abstractions` anchored in the Rasm.Rhino manifest and README registry; the HostUtils cloud-log/exception tap card lives at the Rasm.Rhino folder tier.
