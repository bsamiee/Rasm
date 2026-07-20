# [RASM_RHINO_TASKLOG]

`Rasm.Rhino`'s open and closed work, distilled from ideas and design-page RESEARCH residuals. Each task is a card whose leader carries `[ID]-[STATUS]: thesis`, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` bullets.

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

[0002]-[ACTIVE]: Hook-point census maps every detached fact stream in the boundary onto `rasm.rhino.<domain>.<point>` rows.
- Capability: Exact point roster — stream owner, page anchor, point name, payload type, and host-contract modality (observe, veto, replay) for every fact emission the folder owns.
- Shape: Census table lands on `libs/csharp/Rasm.Rhino/.planning/Document/events.md` as the registry spine's row source.
- Unlocks: `[HOST_HOOK_REGISTRY]` registry rows transcribe from verified census instead of re-derivation per page.
- Anchors: `DocumentStream` fact families; `PointerFact`/gumball/widget channels on `libs/csharp/Rasm.Rhino/.planning/Display/interaction.md`; panel facts on `libs/csharp/Rasm.Rhino/.planning/HostUi/panels.md`; content events on `libs/csharp/Rasm.Rhino/.planning/Render/registry.md`.

[0003]-[ACTIVE]: Modality ruling pins veto-capable seams from the host contract, point by point.
- Capability: Per-point verdict — cancelable host callback (veto row), post-hoc notification (observe row), retained-evidence replay eligibility — each verdict citing the exact host member.
- Shape: Verdicts fold into the registry rows on `libs/csharp/Rasm.Rhino/.planning/Document/events.md`.
- Unlocks: Registry rows never promise a veto the native seam cannot honor.
- Anchors: cancelable event args across `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-document.md` and `api-rhinocommon-commands.md`; `tools/assay` api query over the host assembly for ambiguous members.

[0004]-[ACTIVE]: Process-global custody census classifies every collision surface for multi-plugin arbitration.
- Capability: Complete roster of process-global state — static host event subscriptions, `ObjectsTelemetry` sink, `HostTap` mounts, named shell callbacks, panel/page/command registrations, application settings writers — each with collision class and seat-arbitration shape.
- Shape: Roster and arbitration rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md`, `libs/csharp/Rasm.Rhino/.planning/Document/events.md`, and `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` per owner.
- Unlocks: `[MULTI_PLUGIN_COEXISTENCE]` lands as verified rows, never a partial sweep.
- Anchors: single-subscription-per-process event law; `HostTap.Mount` detacher identity; `AppSettings.Commit` static families on `libs/csharp/Rasm.Rhino/.planning/Persistence/appsettings.md`.

[0005]-[ACTIVE]: Plugin lifecycle and census page spines transcribe the verified `Rhino.PlugIns` rosters.
- Capability: Staged lifecycle custody (`OnLoad`/`CreateCommands`/`OnShutdown`, ALC `Unloading` flush obligations, diagnostics capture window) and the installed-plugin census (id/path resolution, load protection) as page spines with exact member rosters.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/lifecycle.md` and `libs/csharp/Rasm.Rhino/.planning/Plugin/census.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Boundary's missing domain folder opens with its two spine pages grounded in catalog truth.
- Anchors: `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-plugins.md` lifecycle and census entrypoints; `SkinPhase` plugin-load phases on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md`.

[0006]-[QUEUED]: License rail page pins the complete entitlement surface.
- Capability: Acquisition, checkout/checkin, CloudZoo login and lease facts, state-change events, and capability flags as one typed rail with detached evidence records.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/licensing.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Entitlement-gated capability rows for any Rasm plugin feature.
- Anchors: `LicenseUtils`/`LicenseData`/`LicenseStatus`/`LicenseLease` rosters on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-plugins.md`; `ZooClientParameters` and `LicenseStateChangedEventArgs` rows on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md`.

[0007]-[QUEUED]: Per-plugin document participation bridges onto the archive and settings rails.
- Capability: `ReadDocument`/`WriteDocument`/`ShouldCallWriteDocument` custody expressed over `ArchiveIo` framing, and `GetPluginSettings`/`SavePluginSettings`/`SettingsSaved` expressed as settings-rail facts.
- Shape: `libs/csharp/Rasm.Rhino/.planning/Plugin/document.md` minted per `[PLUGIN_DOMAIN]`.
- Unlocks: Plugin document data rides the same schema/integrity framing as every other archive crossing.
- Anchors: `ArchiveIo` spine on `libs/csharp/Rasm.Rhino/.planning/Persistence/userdata.md`; `Settings.Commit` rail on `libs/csharp/Rasm.Rhino/.planning/Persistence/settings.md`.

[0008]-[QUEUED]: Instrument partition rows enumerate receipt families with attribution keys and latency checkpoints.
- Capability: Per-domain rows — receipt kind, instrument name and unit, source field, attribution tags (document key, command, op provenance, tenant) — and named `ILatencyContext` checkpoints across the `HostThread.Run` marshal seam.
- Shape: Rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md`; checkpoint names on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md`.
- Unlocks: `[HOST_INSTRUMENT_PARTITION]` projection executes at app root from declared data.
- Anchors: receipt families across the surface ledgers; `libs/csharp/.api/api-extensions-telemetry.md` latency rows.

[0009]-[QUEUED]: Bench evidence band pins its shape on the Modeling spine and capture run rail.
- Capability: Operation identity, input scale, duration, allocation, and host fingerprint as one evidence band normalized to the corpus-gate benchmark-receipt shape.
- Shape: Band lands on `libs/csharp/Rasm.Rhino/.planning/Modeling/solids.md` and `libs/csharp/Rasm.Rhino/.planning/Viewport/capture.md` per `[HOST_BENCH_HARVEST]`.
- Unlocks: Bridge-run harvest sessions produce corpus-comparable rows without a second measurement path.
- Anchors: `Built` spine receipts; capture run-rail timing; host-fingerprint evidence precedent.

[0010]-[QUEUED]: Census receipt pins its dimensions and composition across the snapshot, closure, and table owners.
- Capability: Exact census dimension set — object kinds, layer-tree shape, block closure metrics, material and annotation usage, archive size — each dimension naming its composing owner and detachment path.
- Shape: Receipt shape lands on `libs/csharp/Rasm.Rhino/.planning/Objects/state.md` per `[DOCUMENT_ANALYTICS_CENSUS]`.
- Unlocks: Analytics egress lands one stable shape into the data plane.
- Anchors: `Objects.Ask` snapshot; `BlockGraph.Ask` closure; `libs/csharp/Rasm.Rhino/.planning/Document/tables.md` table vocabulary.

[0011]-[QUEUED]: Classification sweep annotates every log-egress payload member.
- Capability: Taxonomy verdict per payload member — public, internal, private — with redactor registration named as the app-root contract.
- Shape: Annotated rows land on `libs/csharp/Rasm.Rhino/.planning/Objects/authoring.md` per `[REDACTED_HOST_EGRESS]`.
- Unlocks: Cloud-bound host logs carry enforceable classification.
- Anchors: `libs/csharp/.api/api-redaction.md` taxonomy and redactor rosters; `ObjectsTelemetry` generated events.
- Atomic: one-page annotation sweep.

[0012]-[QUEUED]: Script-engine rows verify member truth then land on the shell runtime.
- Capability: Verified compile/execute/census member spellings and result shapes for the host scripting engine, landed as guarded capability rows.
- Shape: Rows land on `libs/csharp/Rasm.Rhino/.planning/HostUi/shell.md` `[06]-[RUNTIME]` per `[HOST_SCRIPT_ENGINE]`.
- Unlocks: In-host scripting reachable through the boundary.
- Anchors: `tools/assay` api query over the host assembly for `PythonScript`/`PythonCompiledCode`; `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md` rows.
- Atomic: one runtime-section extension.

[0013]-[BLOCKED]: Accounts rail arming question — can entitlement probe without interactive login?
- Capability: Verdict on whether `CloudHostUtils.IsEntitled`/`DenyReason` and cached-token reads answer headless, which fixes how much of `[RHINO_ACCOUNTS_TOKEN_RAIL]` designs before a consuming feature exists.
- Shape: Verdict folds into the blocked idea's Tension on `libs/csharp/Rasm.Rhino/IDEAS.md`.
- Unlocks: Accounts card re-arms or stays parked on evidence instead of assumption.
- Anchors: `tools/assay` api query over the host assembly for `RhinoAccountsManager` and `CloudHostUtils` members; live bridge probe for headless behavior.
- Atomic: single blocker verdict.

[0014]-[BLOCKED]: Headless boot arming question — does macOS WIP permit `RhinoCore` boot outside the bridge launch custody?
- Capability: Verdict on in-process boot viability under macOS launch constraints, the fact `[INPROCESS_HEADLESS_BOOT]` needs before an app-stratum shell is worth designing.
- Shape: Verdict folds into the blocked idea's Tension on `libs/csharp/Rasm.Rhino/IDEAS.md`.
- Unlocks: Headless boot card re-arms with a real boot-environment contract.
- Anchors: `Rhino.Runtime.InProcess` rows on `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md`; bridge launch-custody facts in the estate memory route; live bridge probe.
- Atomic: single blocker verdict.

[0015]-[BLOCKED]: Compute endpoint arming question — what payload contract does `HostUtils.RegisterComputeEndpoint` bind?
- Capability: Verified endpoint delegate signature, routing shape, and census behavior, the facts `[COMPUTE_ENDPOINT_ROWS]` needs to type its rows.
- Shape: Verdict folds into the blocked idea's Anchors on `libs/csharp/Rasm.Rhino/IDEAS.md`.
- Unlocks: Compute rows type against decompiled truth the moment a compute shell exists.
- Anchors: `tools/assay` api query over the host assembly for `HostUtils.RegisterComputeEndpoint`/`GetCustomComputeEndpoints`; `libs/csharp/Rasm.Rhino/.api/api-rhinocommon-runtime.md` rows.
- Atomic: single blocker verdict.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: host exception and cloud-log taps land as `HostTap` on `Objects/authoring.md` — severity projected onto `LogLevel`, classified payload, one detacher, one egress.
