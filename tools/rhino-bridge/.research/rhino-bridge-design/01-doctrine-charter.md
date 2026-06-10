# [01] Doctrine Charter — In-Host Code Law for the rhino-bridge Rebuild

Binding charter for the C1 SUPERVISOR build (prior corpus `tools/rhino-bridge/.research/rhino-bridge/`, verdict in 09 §4 and 10 §4). It answers one question: what do the thirteen laws of `docs/stacks/csharp/README.md` MEAN inside a Rhino plugin, a supervisor CLI, and repo-compiled scenario assemblies — and where does the host force a deviation. Every later design doc (02-08) must satisfy this charter; §8 is the red-team's enforcement list for docs 06-08. Deviations not argued here are not deviations; they are violations.

Operator directives internalized as charter axioms: agent-first (no human-facing flags or modes that exist only as declarations); no host-build pinning — version tolerance is a design property; no fragile logic, no stringiness, no flat spam; every capability designed trigger → behavior → evidence.

## [1]-[COMPONENT_LAW_MAP]

The five C1 components plus the scenario corpus, each with its doctrinal posture. "Rail" = LanguageExt carrier policy per `rails-and-effects.md`; "Shape" = generated-owner policy per `shapes.md`.

| [INDEX] | [COMPONENT] | [LIVES] | [RAIL POSTURE] | [SHAPE POSTURE] | [NAMED EXEMPTIONS] |
| :-----: | ----------- | ------- | -------------- | --------------- | ------------------ |
| [1] | `Rasm.Bridge.Contract` | both processes (one DLL) | NONE — pure data, no carriers, no `Error` base | sole owner of the wire vocabulary: status smart enum, fault union, evidence event union, scenario/endpoint value objects, version+capability records | none — Contract is expression-free declarations |
| [2] | Shell stub (`.rhp`, default ALC) | Rhino default plugin context | none (boot only) | none — zero wire types, zero dependencies | `PlugIn` override statements (host law) |
| [3] | Shell impl (shell-private non-collectible ALC) | in Rhino | `Fin<T>` internal; boundary capsules for UI marshal, pipe, ALC lifecycle | consumes Contract; one `ShellOp`-class dispatch owner if ≥3 operations share context | idle-queue drain loop; ALC unload GC-retry kernel; `using` capsules (§5 D-1, D-3) |
| [4] | `Rasm.Bridge.Cargo` (collectible ALC) | in Rhino, swapped per session | `Fin<T>` + `IO<T>` bracket for scenario lifetime; own private LanguageExt/Thinktecture copies | scenario kit owners: probe/fact/capture/document-scope capsules | `using DocumentScope`; disposal-before-unload enforcement (§5 D-2, D-3) |
| [5] | `Rasm.Bridge.Supervisor` (exe) | workstation | `Eff<SupervisorRuntime,T>` spine; `Schedule` owns all polling/retry/backoff; terminal collapse at `Main` only | CLI verb `[Union]` (port `ClientVerb` shape, 01 §1); `SessionState` `[Union]` state machine; policy values for every timeout/budget | kqueue/`kevent` P/Invoke kernel (11 §4 G3); subprocess boundary capsules |
| [6] | `*.Scenarios.csproj` corpus | repo, loaded into cargo ALC | full repo doctrine, verbatim — `coding-csharp` skill, CSP analyzers, IVT, C# 14 | ordinary repo shapes; `[RhinoScenario]` entrypoints as definition-time aspect | only the host-law exemptions production Rasm code already has |
| [7] | assay rail (python) | workstation | n/a | ONE msgspec decode of the supervisor's result envelope — a boundary admission, not a wire mirror | ~150 LOC ceiling (02 §7) |

The unit of design is the polymorphic dispatch surface, not the file (`DEEP_SURFACES`). Components [1]-[5] exist because process and ALC boundaries are real; nothing else justifies a new assembly.

## [2]-[RAILS_ACROSS_THE_RPC_BOUNDARY]

`Fin`/`Eff`/`Validation`/`Option`/`Seq` are in-process execution semantics. They do not serialize, and their LanguageExt assembly identity is ALC-local (07 §1 pitfall 7). The wire is a `BOUNDARY_ADMISSION` edge on both sides.

[WIRE_CARRIES]:
- Closed Contract vocabulary only: status cases, fault cases, evidence events, scenario refs, version/capability records, primitive payloads.
- Outcomes cross as data — a fault case on the wire, never a `Fin.Fail`, never an exception type name parsed from a string.

[WIRE_NEVER_CARRIES]:
- LanguageExt carriers or `Error` derivatives, live handles, `Type`/`Assembly` references, delegates, host-native types (RhinoCommon/GH2/Eto), or open `object` payloads.
- Rationale beyond serialization: any cargo-defined type that crosses into shell-held state pins the collectible ALC (§5 D-3). Keeping the wire Contract-only makes the unload contract structurally satisfiable instead of disciplinary.

[RAIL_BRIDGE_ONCE] — each process owns exactly one named conversion per direction:
- Supervisor → wire: terminal collapse of the `Eff` pipeline into a Contract request at the proxy call site.
- Wire → supervisor: StreamJsonRpc proxy exceptions (`ConnectionLostException`, `RemoteInvocationException`, timeout) capture via the doctrine's exception-capture form — `Try.lift<Fin<T>>(f).Run().MapFail(...)` flattened with `Bind(static r => r)` — into the supervisor's single `Error`-deriving fault lift. One type derives `Error` and wraps the Contract fault union; `MapFail` scatter and per-call-site bridges are rejected (`feedback_validation_error_typed_rail` generalizes).
- Shell: RPC target methods are boundary adapters — admit the Contract payload, dispatch, project the outcome back to Contract. No rail type appears in any RPC method signature.
- Cargo → evidence: scenario outcomes flow onto the evidence stream (§4); the stream record is Contract-shaped at birth so spool, notification, and envelope need no re-encoding.

[EFFECT_ALLOCATION]:
- Supervisor: `Eff<SupervisorRuntime,T>` where `SupervisorRuntime` carries lease cell (`Atom<T>` — boundary state per rails doctrine §6), clock/`TimeProvider`, cancellation, bundle policy, artifact roots. Constructed once at the composition edge; no service location.
- `Schedule` is the only polling/retry authority: connect polling (replaces the 250 ms hand loop, 01 F5) is `Schedule.spaced(...) | Schedule.upto(deadline)`; quit escalation rungs, restart budgets (08 §2.4 compute.frontend pattern), and redeploy retries are declared `Schedule` values. Ad-hoc delay loops are rejected (`SCHEDULE_POLICY`).
- One authoritative deadline: the supervisor owns it; shell and python derive (09 D5 [FIXABLE-IN-PLACE] half; the three-authority disagreement of FM7 dies by construction). Timeout values are policy rows, not scattered literals (`POLICY_VALUES`).
- Cancellation truth: JSON-RPC cancellation tokens cross the wire and buy transport hygiene — abandon the call, free the pipe — never UI-thread interruption (07 §3, host law). The design must not present cancellation as kill; the watchdog (heartbeat + kqueue `NOTE_EXIT`) is the external authority that converts "still executing" into a named state.

## [3]-[WIRE_VOCABULARY] — Thinktecture, declared once

The current tool declares one wire shape four times — protocol C#, client C# mirror, two python msgspec mirrors (02 §2, 09 §4.2) — plus a string marker grammar inside the JSON (01 F1). That is the named `SHAPE_BUDGET` violation this rebuild exists to retire.

[ONE_DECLARATION]:
- `Rasm.Bridge.Contract` declares every wire concept exactly once, as generated owners: `[SmartEnum<string>]` for the status algebra (wire string + severity rank + process exit code as case rows — the existing `PhaseStatus` triple ports into constructor-delegate rows, `POLICY_VALUES`); `[Union]` for faults and for evidence events; `[ValueObject<T>]`/`[ComplexValueObject]` for scenario refs, endpoint records, version/capability records. `SkipUnionOps` on result/evidence unions, `GenerateUnionOps` only where per-case operation identity is real (`shapes.md` §5).
- Supervisor, shell, and cargo consume Contract types directly. No mirror structs anywhere in C#.
- Python decodes ONE document — the supervisor's result envelope — through one msgspec boundary admission. That is the sanctioned domain-owner/boundary-DTO pair of `shapes.md` §3, not a mirror: it admits a foreign document at a language boundary; it does not re-declare the RPC wire.

[SERIALIZATION_POSTURE]:
- Codec policy lives at the edge with the Contract (wire law, README §5 roadmap): generated owners declare their serialization explicitly. System.Text.Json integration for Thinktecture owners requires the companion package `Thinktecture.Runtime.Extensions.Json` (verified against the maintainer repo 2026-06-10; runtime 10.2.0 already central). Neither it nor StreamJsonRpc (2.25.25, verified current in 11 §5) is in `Directory.Packages.props` yet — both are Phase-1 central-manifest admissions under the repo's C# dependency workflow, and `shapes.md` §6's serialization gate ("integration packages stay inactive until the package graph and an accepted owner prove adoption") is satisfied by exactly this owner.
- StreamJsonRpc's `SystemTextJsonFormatter` carries the Contract types; the hand-rolled 538-LOC envelope/framing/dispatch retires (09 §2.2). The DTO vocabulary and rank-max status fold port; the transport layer is the library's.
- Union-on-the-wire discriminator shape (how `[Union]` cases round-trip under STJ source-gen vs generated converters) is a Phase-1 verification item — docs 06-08 must show the serialized form, not assume it.

[ALC_PLACEMENT] (binding posture for 11 §4 G4):
- The default ALC contains the `.rhp` stub ONLY — no Contract, no Thinktecture runtime, no StreamJsonRpc. The stub's whole job is creating the shell-private ALC and handing off; it needs zero wire types.
- Contract + Thinktecture runtime + StreamJsonRpc closure load into the shell-private non-collectible ALC. Cargo's resolver forwards Contract (and the shell's RPC closure where referenced) to the shell-private instances so type identity is single. Docs 06-08 own the resolver design; the charter binds the invariant: one loaded Contract per host process, zero bridge dependencies in the default ALC.

## [4]-[COLLAPSE_PRESSURE_POINTS] — verbs, sessions, evidence

The collapse scan (`README` §3) applied to the three concept families where today's tool shows ≥3 parallel declarations. Calibration from repo memory binds the method: question SHAPE before collapsing (`feedback_bridge_shape_question_first` — the 2026-05 bridge refactor's headline wins were deletions, not merges) and respect break-even math (`feedback_op_result_collapse_break_even`). The wins below are all deletions of parallels, not micro-merges.

[VERBS]:
- Today: four verb vocabularies — assay verb → rail fn → client CLI verb → wire command — each with its own parse, status mirror, and failure taxonomy (02 §1). `ONE_HOP_RESOLUTION` violation at system scale.
- Charter: at most TWO vocabularies. (a) The supervisor CLI verb `[Union]` — the current `ClientVerb`/`CheckTarget` extension-dispatched union is already doctrinal; port the shape, extend the cases. (b) The RPC interface pair (`IBridgeShell` + events). Assay invokes supervisor verbs 1:1 with no renaming; the wire is invisible to python.
- `MODAL_ARITY` at the wire: JSON-RPC dispatch is name-based — that is the protocol's law, and the interface is the boundary where names are allowed. But no method siblings differing by arity or mode: scenario selection is one union/collection-shaped parameter (one theme, many, all discriminated by value shape), not `runScenario`/`runScenarios`/`runAll`, and no boolean mode knobs beside a payload.
- `LIBRARY_DEPTH` ruling: do NOT collapse the RPC interface to a single `Invoke(BridgeOp)` method. StreamJsonRpc's intended power is typed proxies, per-method cancellation tokens, `IProgress<T>` marshaling, and notification contracts; a single union endpoint would hand-roll the dispatch and correlation the library owns. Interface methods stay one-hop projections — admit payload, dispatch to the owning union/state machine, project outcome; zero logic in method bodies.

[SESSIONS]:
- Today: the session has no owner — sliced across the plugin gate, five client verbs, and two python leases (09 §2.5). The state machine the supervisor owns (idle → reconcile → launch → connect → session → quit → reconcile, 10 §1.2) is a closed vocabulary with payload-carrying states.
- Charter: one `SessionState` `[Union]`; cases carry their evidence (PID, endpoint record, lease token, cargo hash, restart budget remaining). Transitions are one total state-threaded `Switch` dispatch (`DERIVED_LOGIC`); ≥3 arms sharing context use the state-parameter overload (`feedback_polymorphic_state_threaded_collapse`). No boolean phase flags, no nullable payload bags (`shapes.md` §7).
- The lease lives at the resource: supervisor-owned, token-gated (`feedback_token_gated_singleton` pattern for the host singleton), replacing both python flocks (09 D11). Busy remains a typed state with exit code 5 derived from the status smart enum.

[EVIDENCE]:
- Today: five parallel evidence channels — `BridgeMarker` stdout grammar (4 string variants), `BridgePhase.data` JSON, RhinoCode diagnostics, command-window capture, `OnExceptionReport` taps — decoded by three layers, truncation-lossy, dead on crash (01 F1, 04 FM6, 09 D6).
- Charter: ONE evidence event family in Contract — fact, capture, phase-transition, progress, host-exception, lifecycle-transition as cases of one closed union with scenario/session/timestamp envelope. The three consumers are FOLDS of that one stream (`DERIVED_LOGIC`; operational-receipts law, `rails-and-effects` §6): (a) live `IBridgeEvents` notifications, (b) the cargo-side crash-durable JSONL spool, (c) the supervisor's result envelope. Any second schema for the same information is a violation. Counts, summaries, and first-failure are derived projections, never independently maintained fields.
- The status fold is the ported rank-max monoid (`Worst`); first-non-ok taxonomy is a projection of the phase stream. Fact KEYS remain author-chosen strings — the open vocabulary is the domain here — but the record around them is typed, and assertion+evidence fuse in the scenario kit (03 §5.3) so the key is written once.

## [5]-[HOST_FORCED_DEVIATIONS] — argued, each cited

Doctrine deviations are admitted only where Rhino, AppKit, or the CLR forces them. Each is named, bounded, and carries its evidence obligation.

[D-1_UI_THREAD_LAW]:
- Force: all host mutation executes on `RhinoApp.Idle` frames; execution is synchronous and non-cancelable; a blocked UI thread cannot answer the pipe (01 §4, 04 FM7). Every prior-art system accepts this (08 §4[9]); 09 D5 rules the non-cancelable work itself [SOUND].
- Deviation: the idle-queue drain is a statement loop in the shell (named kernel exemption per `EXPRESSION_SPINE`); in-host execution is modeled as an uncancelable boundary effect. UI marshaling is an explicit boundary effect with captured failure (thread law, README §5) — the `OnUiThread` + catch posture from `reference_rhinocommon_invoke_swallows`; raw `InvokeOnUiThread` is forbidden because it swallows.
- Obligation: the supervisor heartbeat + kqueue watch is the cancellation story; any design that claims in-host abort is wrong by host law.

[D-2_NATIVE_HANDLES]:
- Force: RhinoCommon objects wrap native lifetime; finalizer-pending handles block ALC unload (07 §1 pitfall 3); display conduits, doc events, and `File3dm` must close before swap.
- Deviation: `using` capsules and bracket acquisition inside cargo's scenario scope are the named statement exemption (resource law, `rails-and-effects` §5). The capsule patterns are already repo law (`feedback_geometry_source_capsule_pattern`, `feedback_native_resource_projection_patterns`); the scenario kit re-houses them, it does not reinvent them.
- Obligation: cargo enforces disposal at scenario end as an unload PREcondition, and the unload receipt records it.

[D-3_COLLECTIBLE_ALC_UNLOAD_CONTRACT]:
- Force: collectible assemblies may reference non-collectible, never the reverse; any host-rooted cargo type pins the ALC forever; unload is async and silent on failure (07 §1, roslyn#72366, runtime#44679).
- Mostly NOT a deviation: the repo's event law (subscription = disposable value, symmetric attach/detach — `reference_event_subscription_idisposable`) and resource law ARE the unload contract. Doctrine-conformant code unloads.
- Real deviations: (a) the unload-confirm loop — `WeakReference` + bounded GC retries — is a statement kernel, named; (b) LanguageExt/Thinktecture load privately INTO the cargo ALC because default-context trait/serializer caches capture cargo types (07 pitfall 7; the `LanguageExtBootstrap` string hack F11 dies with this); (c) the shell may never hold cargo types in events, caches, or generic instantiations — including `Fin<CargoType>` materialized by shell-held LanguageExt — so the shell↔cargo seam exchanges Contract types and primitives only; (d) `unloadConfirmed` is a typed algorithm receipt, gated as unreliable under an attached debugger (07 pitfall 6).
- Obligation: Phase-0 probe (a) sizes this contract for real Rasm libs; if statics pin hard, the fallback is per-session host recycling (10 §4) — the charter's rules above still apply unchanged.

[D-4_HOST_OWNED_ASSEMBLIES]:
- Force: RhinoCommon, GH2, Eto, Microsoft.macOS resolve from the app bundle with `Private=false`, never NuGet (`build-and-packages` §11); GH2 is default-ALC-only and preloaded by GUID (09 D12 [SOUND], 06 §2).
- Deviation: cargo's resolver must forward every host-owned assembly to the default-ALC loaded instance — duplicating a host assembly into cargo splits singletons and crashes (04 FM11's root cause, already root-fixed; the rule survives the rebuild). Scenario projects MAY name GH2 types (the RhinoCode-era "wrappers only" rule was an isolated-resolver artifact, not host law) — but 07 open question 5 requires the live probe before docs 06-08 rely on raw GH2 object graphs in cargo.
- Obligation: the resolver's forwarding table is data (one declaration, `SYMBOLIC_REFERENCE` — assembly names from one vocabulary, not scattered literals; the current `HostAssemblyNames` deny-list ports as that table).

[D-5_DEFAULT_ALC_HYGIENE] (11 §4 G4):
- Force: the default plugin context is a shared global namespace across all installed plugins (RhinoMCP is now commonly co-resident, 05 §5); version unification conflicts there are the classic Rhino plugin failure.
- Deviation: the stub/shell split is a host-forced two-assembly shape, not file spam — the boundary aligns to ALC physics and rates of change, exactly like the shell/cargo boundary. A single-assembly shell would be denser and wrong.

[D-6_VERSION_TOLERANCE — operator override of G1]:
- Force: RhinoWIP updates monthly; the operator rejects pinning as process-papering (overriding 11 §4 G1's pin-per-parity-phase recommendation). The host runtime itself drifted net9 → net10 inside one WIP cycle (05 §5); McNeel's own bundled `rhinocode` CLI broke against its own runtime — the cautionary case.
- Design consequences, binding: the shell binds only to the minimal stable host surface (`PlugIn`, `RhinoApp.Idle`, `RhinoApp.WriteLine`, `HostUtils.OnExceptionReport`, `RhinoApp.Closing` — APIs stable across 8/9); anything version-sensitive lives in cargo where recompile is free. The hello exchange carries host version + capability set; behavior differences are capability flags resolved at runtime, never compile-time branches. Wire tolerance: one-minor skew between supervisor and shell is legal (09 D9). Parity phases treat a WIP bump as an expected event — re-run `api doctor` + probes and continue — not a baseline freeze.

[D-7_SUPERVISION_PRIMITIVES] (11 §4 G3):
- Force: .NET has no BCL kqueue surface; instant `NOTE_EXIT` detection needs ~50-100 LOC of `kqueue`/`kevent` interop.
- Deviation: one P/Invoke kernel inside the supervisor's process-watch capsule (named exemption, `system-apis` §8 interop gate); 250 ms PID polling via `Schedule` is the degraded fallback and must be a swap-in policy value, not a rewrite.

## [6]-[LOC_AND_FILE_BUDGET]

The trigger for shape work is concept density, never byte count (CLAUDE.md §4). The rebuild's budget is therefore expressed in OWNER BLOCKS, with LOC as a sanity envelope only.

- Baseline: 2,158 LOC C# across 7 files, plus ~1,983 LOC of duplicated python (01 §1, 02 §2). G5's order-of-magnitude for C1 is 1.5-2.5× the C# body (11 §4) — accepted, PROVIDED the file count stays the same order: target ≤14 source files across the four bridge projects (Contract ≤2, shell stub 1, shell impl ≤3, Cargo ≤4, Supervisor ≤4), each file one owner block under the §6 file-organization law. A design doc that adds a file must name the owner block that file is.
- No helper/util files, no single-call indirections, no wrapper layers between Contract and its consumers (`ONE_HOP_RESOLUTION`). Capability growth lands as deeper owners — new cases on existing unions, new rows on existing policy tables, new folds on the one evidence stream — never as sibling files (`ROOT_REBUILD`).
- The deletion dividend is part of the budget and must be enumerated per design doc: both msgspec mirrors, both fact/capture regex scrapers, both python step-policy tables, the client's wire mirror, the four-verb vocabulary chain, the `BridgeMarker` grammar, the `SmokeTemplate` string-codegen (F12 — replaced by one compiled smoke scenario in the corpus), the `LanguageExtBootstrap` incantation (F11), and the 15-rule private-memory dialect contract (03 §2.3 rules 1-8, 11 cease to exist).
- Scenario corpus: grouping by handshake amortization is obsolete once the session owns batching (FM10 [ELIM]); files regroup by FIXTURE AFFINITY and ownership instead. The shared fixture module enters the testkit (03 §5.3); per-file duplicate builders die.
- Python: the assay rail's ~150 LOC ceiling (02 §7) is binding. Every python line beyond decode-and-fold is capability leaking back to the wrong side of the seam.

## [7]-[PORT_VERBATIM_OBLIGATIONS]

The eight hard-won, empirically corrected behaviors from 09 §4 are obligations, not suggestions. Each ports with semantics intact and is re-housed in doctrine-conformant shape. Re-deriving any of them from scratch is a red-team failure.

| [INDEX] | [OBLIGATION] | [EMPIRICAL CORE — DO NOT RELITIGATE] | [DOCTRINE-CONFORMANT HOUSING] |
| :-----: | ------------ | ------------------------------------ | ----------------------------- |
| [1] | AE-terminate → SIGKILL quit ladder, in that order | `RhinoApp.Exit(false)` and SIGTERM both self-SIGABRT; the Apple Event is the only clean exit (04 FM1; corrected 2026-05-29) | Supervisor lifecycle capsule: `Schedule`-sequenced escalation rungs; each rung emits a typed lifecycle event; `closed:false` FAILS the phase (kills 02 §4's swallow points 13a-13e) |
| [2] | Pre-launch crash-marker reconcile placement | macOS writes `.ips` asynchronously after kill; only the before-launch clear reliably beats the race (04 FM2) | `Reconcile` is a named `SessionState` always traversed before cold launch; markers found/cleared are evidence events; deletion stays a boundary IO effect |
| [3] | Endpoint liveness validation | PID match + start-time tolerance + `rb-` prefix + length cap guard PID recycling and stale files (01 §2, 04 FM8) | `EndpointRecord` as `[ComplexValueObject]` with the validation partial owning admission; staleness is a typed rejection on the `Fin` bridge, not a bool |
| [4] | GH2 preload discipline | `PlugIn.LoadPlugIn` by GUID into the default ALC before any GH2 touch; took UI scenarios 1/8 → 6/8 (04 FM11, 09 D12) | Shell owns preload as a session-start effect; the RhinoCode-era `FromAssembly` injection is succeeded by the cargo resolver's host-assembly forwarding table (§5 D-4) |
| [5] | `HostUtils.OnExceptionReport` capture | The only window into UI-thread-swallowed faults (09 §2.6); a swallowed report flips an otherwise-green scenario to failed | Shell-owned disposable subscription (event law), session-scoped; reports become host-exception evidence events; the status fold consumes them — the flip becomes derivation, not patching |
| [6] | NU1004/NU1403 lock-drift classification | Misread as compile errors it burns agent hours; the taxonomy + refusal to auto-regenerate is correct (04 FM9) | Lives with the SINGLE build-truth owner (supervisor consumes rail-proven outputs, owns no build pipeline — 09 D8); the typed `nuget-lock-drift` fault and remediation text port into that owner's admission |
| [7] | Try/finally evidence flush | Partial facts must survive a throw (04 FM6 fix history) | Cargo `Scenario.Run` as `IO` bracket/`Finally`; with the JSONL spool writing continuously, the finally degrades gracefully to footer+flush — crash-durability is structural, the bracket is belt |
| [8] | Phase vocabulary + first-non-ok taxonomy | The status algebra and remedy routing are the tool's best diagnostics asset (01 §2, 03 §4.1) | `[SmartEnum<string>]` status rows carrying severity rank + exit code; `Worst` as the monoid fold; first-non-ok and remedy route as derived projections of the phase stream (`DERIVED_LOGIC`) |

## [8]-[RED_TEAM_CHECKLIST] — enforced on docs 06-08

Fifteen rules. Each is checkable against a design doc's text; "show me" means the doc must exhibit the shape, not assert compliance.

| [#] | [RULE] | [CHECK] |
| :-: | ------ | ------- |
| R1 | WIRE_IS_DATA | No LanguageExt carrier, `Error` derivative, live handle, host-native type, delegate, or open `object` in any RPC signature, notification payload, spool record, or envelope field. Show every RPC signature. |
| R2 | ONE_WIRE_DECLARATION | Every wire concept declared exactly once in Contract as a generated owner; zero mirror structs in supervisor/shell/cargo; python decodes only the supervisor envelope. Grep-equivalent: one definition site per shape name. |
| R3 | RAIL_BRIDGE_ONCE | Exactly one named foreign-outcome → typed-fault conversion per process per direction, using the flattened `Try.lift` capture form; no `MapFail` scatter, no thrown control flow inland. |
| R4 | ONE_EVENT_STREAM | Facts, captures, phases, progress, host exceptions, lifecycle transitions are cases of one closed evidence family; notify/spool/envelope are folds of it; no field that a fold could derive is stored independently. |
| R5 | SESSION_IS_A_UNION | One `SessionState` owner, payload-carrying cases, one total state-threaded transition dispatch; no boolean phase flags; lease and restart budget live inside states. |
| R6 | VERB_BUDGET | ≤2 verb vocabularies system-wide (CLI union, RPC interface), 1:1 mapping, no renames; no method siblings differing by arity/mode; selection discriminates on value shape; no boolean knob parameters. |
| R7 | POLICY_VALUES_ONLY | Every timeout, retry, poll cadence, restart budget, retention window, and load-order rule is a declared policy value (`Schedule`, smart-enum row, frozen table) deriving from ONE authoritative deadline; zero scattered duration literals. |
| R8 | HOST_LAW_NAMED | Every statement-shaped region (idle drain, unload GC loop, `using` capsules, kqueue kernel) cites its §5 deviation by ID; any unnamed statement form in domain flow is a violation. |
| R9 | UNLOAD_CONTRACT | Shell holds no cargo types (events, caches, generic instantiations); subscriptions are disposable values with symmetric detach; LanguageExt/Thinktecture are cargo-private; host assemblies forward to default-ALC instances, never duplicate; `unloadConfirmed` is a typed receipt with a debugger gate. |
| R10 | NO_STRING_PROTOCOLS | No stdout marker grammar, no regex decode, no string-assembled codegen, no string-typed verb/state names where `nameof`/keys/vocabulary tables exist (`SYMBOLIC_REFERENCE`); the smoke probe is a compiled scenario. |
| R11 | LIBRARY_DEPTH | StreamJsonRpc used at native depth (typed proxies, notifications, `IProgress<T>`, cancellation tokens) with no envelope re-implementation and no rename wrappers; rejected-by-charter alternatives (single `Invoke(union)` endpoint, hand-rolled framing) absent. |
| R12 | SCENARIOS_ARE_REPO_CODE | Scenario projects compile under full repo doctrine (C# 14, CSP analyzers, IVT, `coding-csharp`); zero scenario-only language rules; `[RhinoScenario]` is the only discovery aspect; no host-native types on any out-of-host discovery path (08 §4[5]). |
| R13 | VERSION_TOLERANCE | No host-build pin anywhere; hello exchanges version + capability set; one-minor wire skew tolerated; shell binds only the minimal stable host surface; version-sensitive behavior is a runtime capability flag, never a compile-time branch. |
| R14 | PORT_VERBATIM_HONORED | All eight §7 obligations present with empirical semantics intact (ladder order, reconcile placement, liveness validation, GH2 preload, exception-report capture, lock-drift taxonomy, evidence flush, status algebra) — re-housed, never re-derived or weakened. |
| R15 | DENSITY_BUDGET | Every file maps to a named owner block; file counts within §6 ceilings; no helper/util/wrapper files; the doc enumerates its deletion dividend; new capability lands as cases/rows/folds on existing owners, with trigger → behavior → evidence stated end-to-end. |
