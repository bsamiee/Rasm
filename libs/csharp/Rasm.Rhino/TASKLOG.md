# [RASM_RHINO_TASKLOG]

`Rasm.Rhino`'s open and closed work, distilled from ideas and design-page RESEARCH residuals. Each task is a card whose leader carries `[ID]-[STATUS]: thesis`, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` bullets.

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

[CUSTODY_CENSUS]-[ACTIVE]: Process-global custody census classifies every collision surface for multi-plugin arbitration.
- Capability: Complete roster of process-global state — static host event subscriptions, `ObjectsTelemetry` sink, `HostTap` mounts, named shell callbacks, panel/page/command registrations, application settings writers — each with collision class and seat-arbitration shape.
- Shape: Roster and arbitration rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md`, `libs/csharp/Rasm.Rhino/.planning/Document/events.md`, and `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` per owner.
- Unlocks: `[MULTI_PLUGIN_COEXISTENCE]` lands as verified rows, never a partial sweep.
- Anchors: single-subscription-per-process event law; `HostTap.Mount` detacher identity; `AppSettings.Commit` static families on `libs/csharp/Rasm.Rhino/.planning/Persistence/appsettings.md`.

[PLUGIN_LIFECYCLE_SPINES]-[ACTIVE]: Plugin lifecycle and census page spines transcribe the verified `Rhino.PlugIns` rosters.
- Capability: Staged lifecycle custody (`OnLoad`/`CreateCommands`/`OnShutdown`, ALC `Unloading` flush obligations, diagnostics capture window) and the installed-plugin census (id/path resolution, load protection) as page spines with exact member rosters.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/lifecycle.md` and `libs/csharp/Rasm.Rhino/.planning/Plugin/census.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Boundary's missing domain folder opens with its two spine pages grounded in catalog truth.
- Anchors: `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-plugins.md` lifecycle and census entrypoints; `SkinPhase` plugin-load phases on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md`.

[LICENSE_RAIL_PAGE]-[QUEUED]: License rail page pins the complete entitlement surface.
- Capability: Acquisition, checkout/checkin, CloudZoo login and lease facts, state-change events, and capability flags as one typed rail with detached evidence records.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/licensing.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Entitlement-gated capability rows for any Rasm plugin feature.
- Anchors: `LicenseUtils`/`LicenseData`/`LicenseStatus`/`LicenseLease` rosters on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-plugins.md`; `ZooClientParameters` and `LicenseStateChangedEventArgs` rows on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md`.

[DOCUMENT_PARTICIPATION_BRIDGE]-[QUEUED]: Per-plugin document participation bridges onto the archive and settings rails.
- Capability: `ReadDocument`/`WriteDocument`/`ShouldCallWriteDocument` custody expressed over `ArchiveIo` framing, and `GetPluginSettings`/`SavePluginSettings`/`SettingsSaved` expressed as settings-rail facts.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/document.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Plugin document data rides the same schema/integrity framing as every other archive crossing.
- Anchors: `ArchiveIo` spine on `libs/csharp/Rasm.Rhino/.planning/Persistence/userdata.md`; `Settings.Commit` rail on `libs/csharp/Rasm.Rhino/.planning/Persistence/settings.md`.

[PULSE_BEAT_RECOMPOSE]-[QUEUED]: Pulse beat evidence composes the kernel monotonic beat.
- Capability: cadence receipts project the kernel's temporal identity — ordinal and elapsed read off the composed evidence while cadence columns stay host-local, so drift semantics never fork from the timeline owner.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Eto/runtime.md` `[03]` — `PulseBeat` re-shapes to compose `MonotonicBeat` with `Interval`/`Drift`/`Missed` as extension columns.
- Unlocks: the branch host-beat composition row holds at both host boundaries.
- Anchors: kernel `Parametric/projections.md` `MonotonicBeat`; the Grasshopper `ClockBeat` composed form as the sibling discipline.
- Atomic: one receipt re-shape.

[HOST_PATH_VALUE_ADJUDICATION]-[QUEUED]: Adjudicate the `HostPath` app-root value — a distinct redaction class or a collapse into `UserContent`.
- Capability: the sensitivity taxonomy's classification classes and its app-root value set agree — either path payloads earn their own redactor-map value or the distinct `HostPathAttribute` classification retires into `UserContent`.
- Shape: one verdict on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md` `[02]` — a fourth app-root value widening the three-value roster, or the `HostPath` class and attribute deleted with its members reclassified.
- Unlocks: the app-root redactor map keyed on value strings distinguishes exactly the classes the sweep law distinguishes.
- Anchors: the classification sweep law and `HostSensitivity` roster on `Objects/authoring.md`; the app-root `DataClassification` value custody.
- Tension: path redaction granularity is a redactor-map intent question — the taxonomy currently separates what the value space cannot express.

[MARSHAL_STALL_GAUGE]-[QUEUED]: A stall watchdog on the Rhino marshal seam — hang evidence at parity with the GH dispatch pulse.
- Capability: UI-thread marshal stalls surface as typed pulse evidence with budgets and breach verdicts, beside the landed checkpoint-latency ledger, so a hung host thread is observable evidence rather than a silent freeze.
- Shape: a pulse/stall band on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` beside `MarshalLatency` — budgets, stall policy, and last-stall evidence at the `UiThread` seat.
- Unlocks: both host boundaries carry hang evidence; the app root reads one stall vocabulary across hosts.
- Anchors: `libs/csharp/.planning/RULINGS.md` host-twins plural row (a twin capability, never a shared owner); the GH `DispatchPulse` discipline as the sibling shape; the `MarshalLatency` one-seat law.
- Ripple: mirrors `Rasm.Grasshopper` `[DISPATCH_PULSE_WATCH]`.

[HEADLESS_BOOT_PROBE]-[BLOCKED]: Headless boot arming question — does macOS WIP permit `RhinoCore` boot outside the bridge launch custody?
- Capability: Verdict on in-process boot viability under macOS launch constraints, the fact `[INPROCESS_HEADLESS_BOOT]` needs before an app-stratum shell is worth designing.
- Shape: Verdict folds into the blocked idea's Tension on `libs/csharp/Rasm.Rhino/IDEAS.md`.
- Unlocks: Headless boot card re-arms with a real boot-environment contract.
- Anchors: `Rhino.Runtime.InProcess` rows on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md`; bridge launch-custody facts in the estate memory route; live bridge probe.
- Arms: the boot-viability verdict — a live bridge probe or `Rhino.Runtime.InProcess` evidence answering whether macOS WIP permits `RhinoCore` boot outside the bridge launch custody.
- Atomic: single blocker verdict.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOST_TAP_EGRESS]-[COMPLETE]: host exception and cloud-log taps land as `HostTap` on `Objects/authoring.md` — severity projected onto `LogLevel`, classified payload, one detacher, one egress.
[HOOK_POINT_CENSUS]-[COMPLETE]: hook-point census table with owner entries and failure-symmetric `MountRegistry.MountAll` custody landed on `Document/events.md` `[06]-[HOOK_REGISTRY]`; every row names its payload, kernel-ruled modality, and owning mount.
[MODALITY_VERDICT_FOLD]-[COMPLETE]: per-point modality verdicts folded into the census — veto rows cite `CullObjectEventArgs.CullObject`, `DrawObjectEventArgs.DrawObject`, `RhinoObject.IsActiveInViewport`, `RhinoObject.OnPick`, and `CustomObjectGrips.NewGeometry`; all other points observe, panel adds replay.
[SCRIPT_ENGINE_ROWS]-[COMPLETE]: script-engine rows landed on `HostUi/shell.md` `[06]-[RUNTIME]` — `HostScripts` compile/run custody, `ScriptRun`/`ScriptUnit`/`ScriptOutcome` family, engine census as `HostProbe.Scripting`; member truth decompile-verified via `tools.assay api query --key rhino-common`.
[HEADLESS_ACCOUNTS_VERDICT]-[COMPLETE]: headless-accounts verdict folded into `[RHINO_ACCOUNTS_TOKEN_RAIL]` Tension — entitlement and cached-token reads answer headless at the API surface, interactive login confines to first acquisition; the residual live-provider fact rides the idea's bridge-probe obligation.
[COMPUTE_ENDPOINT_VERDICT]-[COMPLETE]: compute-endpoint contract verdict folded into `[COMPUTE_ENDPOINT_ROWS]` Anchors — registration binds `(string endpointPath, Type t)` on an append-only roster, census is `GetCustomComputeEndpoints()`, no delegate or unregister surface exists; the idea stays blocked on the app-stratum compute shell alone.
[INSTRUMENT_PARTITION_ROWS]-[COMPLETE]: `RhinoInstrumentPartition.Rows` partition (fault, host-log, stream-loss, pointer, panel, content, marshal, census, bench kinds) on `Objects/authoring.md`; `MarshalLatency` checkpoint and tag names with the `DurationInstrument` mirror on `HostUi/shell.md`; `RhinoInstruments` contributed rows land beside the partition as the adjudicated twin — kind partition and contributed meter rows stay separate concerns.
[BENCH_EVIDENCE_SHAPE]-[COMPLETE]: `BenchEvidence` shape (operation-family identity, input scale, duration, allocation, `HostFingerprint`) with the `BenchBand.Measured` bracket landed on `Modeling/solids.md`; `Captures.Run` measures each request case inside `HostThread.Run` and stamps each artifact.
[DOCUMENT_CENSUS_DIMENSIONS]-[COMPLETE]: `DocumentCensus` dimensions pinned on `Objects/state.md` — canonical `Objects.Ask` snapshot window, `Layers.Ask` tree shape, `BlockGraph.Ask` closure triple, `CountBy` histograms, `RandomAccess.GetLength` archive extent.
[CLASSIFICATION_SWEEP]-[COMPLETE]: classification sweep landed on `Objects/authoring.md` — app-root-aligned `HostSensitivity` rows, member annotations on `HostFaultFact`, `HostLogFact`, and cached `HostStaticFact`, with unclassified-public site keys and codes.
