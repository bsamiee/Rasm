# [RASM_RHINO_TASKLOG]

`Rasm.Rhino`'s open and closed work, distilled from ideas and design-page RESEARCH residuals. Each task is a card whose leader carries `[ID]-[STATUS]: thesis`, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` bullets.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
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

[FACT_STREAM_GENERIC_OWNER]-[QUEUED]: One generic receipt/slot/body/fact stream owner serves every host-mutation folder.
- Capability: the receipt-slot-body-fact quartet becomes one parameterized S0 owner, so a mutation folder contributes vocabularies instead of re-minting the stream machinery and the cross-product gate is written once.
- Shape: new owner at libs/csharp/Rasm.Rhino/.planning/Document/tables.md (the lowest stratum both consumers reach), collapsing `DraftReceipt`/`DraftSlot`/`DraftBody`/`DraftFact` (Annotation) and `BlockReceipt`/`BlockSlot`/`BlockBody`/`BlockFact` (Blocks/operations.md) onto it, the `Admits` predicate column carried as the one cross-product gate.
- Unlocks: a third mutation folder joins by declaring its slot and body vocabularies alone.
- Anchors: `[STRATA_TWIN]` seating law; the `Admits` column already landed on `BlockSlot`; both ends move in ONE pass — landing one half leaves two generics.
- Tension: the two S1 vocabularies must prove they genuinely share payload timing before the collapse; a forced merge of distinct evidence shapes is the rejected form.

[QUIET_WRITE_VOCABULARY]-[QUEUED]: One quiet-write posture vocabulary and one commit entry shape across the host-mutation folders.
- Capability: quiet-versus-loud host writes and transaction commit entries read as one posture vocabulary, so a consumer learns one grammar and a new folder cannot fork a third spelling.
- Shape: settle `WriteMode` (Annotation/style.md), `HostInteraction` (Blocks/operations.md), and `ObjectSignal Quiet` (Objects/lights.md) onto one owner; unify `DraftPlan<TOp>`, `BlockTransaction`, and the bare varargs commit entries onto one entry shape.
- Unlocks: `[FACT_STREAM_GENERIC_OWNER]` — the generic stream assumes one commit shape.
- Anchors: the three vocabularies verified live at their pages; the Annotation folder roster is `{dimension,hatch,linetype,style,text,typeface}.md` (no operations.md).
- Ripple: follows `[FACT_STREAM_GENERIC_OWNER]`.

[ATTRIBUTE_SOURCE_VOCABULARIES]-[QUEUED]: Objects/attributes.md admits its host enums and colors through keyed owners.
- Capability: every attribute-source axis dispatches on a keyed vocabulary and every color crosses as `PerceptualColor`, so raw host enum members and `System.Drawing.Color` stop leaking past the admission seam.
- Shape: libs/csharp/Rasm.Rhino/.planning/Objects/attributes.md — keyed `[SmartEnum]` owners over `ObjectColorSource`/`ObjectPlotColorSource`/`ObjectPlotWeightSource`/`ObjectMaterialSource`/`ObjectSectionAttributesSource`/`ObjectDecoration`/`SectionLabelStyle`/`ItemColorSource`/`DecalMapping`/`DecalProjection`; compose Annotation/linetype.md's landed `LinetypeSource` for `ObjectLinetypeSource`; the eight `System.Drawing.Color` fields to `PerceptualColor.OfRgb`/`ToRgb`; touch points `AttributeEdit.{Paint,Plot,PlotWeight,LinePattern,MaterialBind,Decorate,SectionSource,SectionLabel,HatchFill,HatchBoundary}`, the `Admit` and `Apply` arms, `AttributeSnapshot`, `DecalSnapshot`, `MaterialBinding`, `ObjectPiece`'s two colour columns.
- Unlocks: the attributes page joins the vocabulary discipline every sibling Objects page already holds beside the landed `ActiveSpaceUse` re-seat.
- Anchors: ~40 coupled touch points in one 900-line page — the half-landed-vocabulary-is-worse-than-none rule is why this is one focused pass; `Objects/lights.md` is the color precedent.

[COMMANDS_RUNTIME_PRELUDE]-[QUEUED]: The four Commands pages carry their runtime preludes.
- Capability: every Commands fence resolves its composed names through a declared prelude, so the folder meets the architecture's prelude law and a cold reader compiles the imports instead of inferring them.
- Shape: libs/csharp/Rasm.Rhino/.planning/Commands/{acquisition,command,options,selection}.md — one `[RUNTIME_PRELUDE]` block + `namespace` declaration per page, derived from the members each names (`RhinoGet`, `Rhino.Input.Custom`, `Rhino.Display`, `Rhino.DocObjects`, `Rasm.Domain`, `Rasm.Rhino.Document`).
- Unlocks: the Commands folder passes the same prelude conformance every other Rasm.Rhino folder holds.
- Anchors: Rasm.Rhino/ARCHITECTURE.md:238 prelude law; folder-wide gap verified on all four pages.
- Atomic: one prelude block per page, four pages.

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

[MARSHAL_STALL_GAUGE]-[QUEUED]: Rhino's marshal seam watches its own stalls, raising hang evidence at parity with the GH dispatch pulse.
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
[POST_EFFECT_PIPELINE_PORTS]-[COMPLETE]: `EffectPass` ports (`Read`/`Write` with commit, `Handle`, `CopyDown`, `Advance`) and `EffectHost`'s eight overrides landed on `Display/render.md`; `PostEffectPipeline`, `PostEffectChannel`, and the `ChannelGPU` producer chain landed in `.api/api-rhinocommon-render.md`, which previously named the GPU path with no producer.
[EFFECT_GATE_ARMING]-[COMPLETE]: the inert-gate defect closed — `Display/render.md` `[04]` now states that `WindowOp.Gate` is consulted only for a `[CustomPostEffect]` declaring `UseExecutionControl`, and the catalog enum row carries the same fact.
[HATCH_GENERATOR_EDITS]-[COMPLETE]: `LineEdit` append/replace/remove/clear over `AddHatchLine`/`HatchLineAt`/`RemoveHatchLine`/`RemoveAllHatchLines` with the `Revised` copy-then-`Modify` fold landed on `Annotation/hatch.md`; replace is a bounded remove-then-append because the host publishes no in-place setter.
[SHELL_REGISTRY_ROWS]-[COMPLETE]: `RenderPanels`/`RenderTabs` registration landed as instance calls on host-handed registrars behind `RenderShell.Drain`; `.api/api-rhinocommon-render-ui.md` corrected from static-shaped rows to the three panel and two tab instance overloads and the one-shot override window.
[SUBD_INTERPOLATION_EVIDENCE]-[COMPLETE]: the interpolate arm on `Modeling/subd.md` now emits `FixedVertexCount()`, `ContextId`, and `VertexIdList()` into `Built<SubDSlot>.Evidence`, and the `InterpolatedVertexCount` property-vs-method spellings corrected at both read sites.
[BACKING_SCALE_CONSUMER]-[COMPLETE]: `FrameTick.DevicePixels`/`HairlineWidth` landed on `Viewport/motion.md`, giving the promised device-pixel consumer the tick's own scale instead of a per-frame host read.
[MESH_SLOT_SPLIT_STATED]-[COMPLETE]: the `MeshSlot` name split stated at both ends — `Modeling/meshing.md` names the `Rasm.Rhino.Modeling` build-product vocabulary and `Objects/history.md` names the nested `SlotValue.MeshSlot` payload case under its union's `<Type>Slot` convention.
[GEOMETRY_KNN_TABLE]-[COMPLETE]: the neighbourhood family split into its own `.api/api-rhinocommon-geometry.md` scope with the shared `IEnumerable<int[]>` return hoisted to the scope line, closing the two rows that carried no return clause and the prose that re-listed the roster.
