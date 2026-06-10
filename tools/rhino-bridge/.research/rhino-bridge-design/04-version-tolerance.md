# [04] Version Tolerance — Engineering the No-Pinning Mandate

Design task D4 of the rhino-bridge DESIGN wave, 2026-06-10. Input: the full prior corpus (`tools/rhino-bridge/.research/rhino-bridge/`, cited by number), `docs/stacks/csharp/**` doctrine, and local bundle inspection this session. The operator rejected pinning RhinoWIP builds (research README Q2 / 11 G1 is hereby ANSWERED: no pin, ever). RhinoWIP updates monthly; the tool must run unchanged on WIP today and on Rhino 9 GA at launch. This doc converts that mandate into mechanism: a drift model, a churn classification of every host coupling in C1, four tolerance mechanisms designed end-to-end (trigger → behavior → evidence), an anti-fragility audit of each, and a reflection budget. Design only — no process mitigations, no "re-pin per parity phase" anywhere.

## [1]-[DRIFT_MODEL] — what actually changes, and in which regime

The host is an explicitly unfrozen platform: monthly WIP service releases (05 §5), GH2 under an explicit breaking-changes-until-beta license (06 §1), a bundled .NET runtime that has already moved within one WIP cycle (net9 → 10.0.2, 05 §5), and a vendor whose own bundled `rhinocode` CLI shipped mis-targeted (net8 binary on a .NET-10-only bundle, 05 §2) — the cautionary proof that even McNeel does not hold its own bundle internally consistent across updates. Three drift regimes, each needing a different mechanism:

| [REGIME] | [WHAT DRIFTS] | [SYMPTOM IF UNHANDLED] | [OWNING MECHANISM] |
| :------: | ------------- | ---------------------- | ------------------ |
| A — binary skew | Host updated under previously-compiled cargo/scenario assemblies (HintPath snapshot ≠ loaded host) | `MissingMethodException` / `TypeLoadException` at JIT time, mid-scenario, attributed to nothing | M4 fingerprint gate + drift fault |
| B — source drift | Host API surface changed; cargo or scenarios no longer compile, or compile against a renamed/moved member | Compile errors scattered across every file that touched the host | M2 choke points (one-file fix radius) |
| C — behavior drift | Member exists, semantics changed | Wrong results, silent ceilings, stale folklore | M1 behavior probes |

Regime C is the corpus's own case study: the "GH2 `Start(Headless)` never settles" constraint was live-host-verified 2026-05-29 and contradicted by the 2026-06-02 assembly decompile four days later (06 §2, 11 claim 14). A constraint verified in May was false by June — on a monthly-churn host, **any encoded constraint is folklore the moment it is written**. The engineering consequence drives M1: constraints must be re-derived per session by executable probes, never stored as static truth.

A fourth boundary is internal but created by the same mandate: the C1 shell is frozen by design while cargo and supervisor iterate freely (10 §1.2), so the wire itself is a version seam inside the tool. No-pinning therefore demands skew tolerance in three directions at once — host↔cargo, host↔supervisor, supervisor↔shell — and M3 handles the third with the same capability algebra M1 uses for the first two.

One verified GA-transition hazard found this session (local inspection, 2026-06-10): the crash-marker filenames the current tool deletes embed the app **display name**. `~/Library/Autosave Information/` on this machine holds `Unsaved Rhino 8 Document.3dm` (from `Rhino 8.app`, `CFBundleName` = "Rhino 8") beside the WIP's `Unsaved RhinoWIP Document.3dm` (`CFBundleName` = "RhinoWIP"); both bundles share `CFBundleExecutable` = `Rhinoceros`, which is why crash reports are `Rhinoceros-*.ips` for both. The current tool hard-codes the "RhinoWIP" literal (01 §5). At Rhino 9 GA the bundle name changes and marker reconciliation silently stops — the recovery-dialog roulette returns at exactly the transition the operator named. The fix is `SYMBOLIC_REFERENCE` applied to host paths: derive autosave names from the discovered bundle's `CFBundleName` and `.ips` globs from `CFBundleExecutable`, never from literals. The literal's accidental virtue — never deleting a human-operated Rhino 8's recovery state (04 §5 open gap) — is preserved deliberately: reconciliation scopes to the bundle the supervisor discovered and launched.

## [2]-[CHURN_CLASSIFICATION] — every host coupling in C1, tiered

Tiers, by churn exposure and the mechanism each tier earns:

| [TIER] | [DEFINITION] | [CHURN EVIDENCE] | [BINDING RULE] |
| :----: | ------------ | ---------------- | -------------- |
| T0 | BCL / stable OS spec: named pipes (UDS on macOS), collectible ALC, STJ, kqueue `NOTE_EXIT` | LTS runtime; BSD spec stability (07 §3, §5) | Direct use anywhere |
| T1 | RhinoCommon stable core: `PlugIn`, `PlugInLoadTime`, `RhinoApp.Idle`/`.Closing`, `PlugIn.LoadPlugIn`, `RhinoDoc`, `ViewCapture` | Years-stable SDK surface; survived every WIP bump in the tool's life unchanged (01, 04) | Direct compile-time binding; the only host tier the shell may load-bear on |
| T2 | Host process/platform behavior contracts: AE-quit semantics, marker paths, bundle layout, bundled runtime version, EventPipe enablement, Eto platform behaviors, `HostUtils.OnExceptionReport` semantics, command-history text | Not APIs but observed behaviors; the quit ladder was shipped wrong once and corrected empirically (04 FM1); `rhinocode` mis-target (05 §2) | Evidence-bearing and fail-open; never load-bearing in the shell |
| T3 | GH2 alpha public surface + GrasshopperIO: `SolutionServer`, `DocumentIO`, `GhEditor`, `Canvas`, `.ghz` diff engine | Explicit vendor license to break until beta (06 §1); root namespace already renamed once (Feb 2025); the regime-C case study lives here | Cargo only, behind ONE owner type, capability-gated |
| T4 | Non-public surfaces reachable only by reflection: GH2 internal types, undocumented member shapes | The current tool's 5-hop `ReflectGrasshopperSolution` already degraded silently (01 F9) | Probe-gated, optional-only, never load-bearing; see §5 budget |

The placement law that makes the tiers structural rather than advisory: **a coupling's tier must not exceed its layer's swap cost.** The shell is the relaunch-expensive layer, so it binds T0/T1 only, with T2 touches strictly fail-open; cargo hot-swaps, so it may own T3; the supervisor never links a host assembly at all, so its entire host coupling is process-level T2; the Contract assembly carries zero host types — every host datum crosses the wire as a bridge-owned DTO projected at the cargo boundary, or host churn would propagate into the frozen layer.

Full coupling register for C1 (components per 10 §1.2):

| [#] | [COUPLING] | [COMPONENT] | [TIER] | [TOLERANCE ROUTE] |
| :-: | ---------- | ----------- | :----: | ----------------- |
| 1 | `PlugIn` base, `AtStartup` load, `RhinoApp.Idle` marshal, `RhinoApp.Closing` | Shell | T1 | Direct binding; a break here is a shell redeploy — accepted (§4) |
| 2 | `PlugIn.LoadPlugIn(Guid)` for GH2 preload | Shell | T1 API, **T3 payload** | GUID arrives as RPC data from the supervisor, never a shell constant — a GH2 GUID/packaging change at GA is a config fix, zero shell rebuild |
| 3 | `HostUtils.OnExceptionReport` tap | Shell | T2 | Fail-open: subscribe inside its own probe/catch; absence degrades the diagnostic, never the transport; reported as a capability fact at hello |
| 4 | Named pipe server, collectible ALC machinery | Shell | T0 | Direct |
| 5 | StreamJsonRpc + STJ closure | Shell | T0, coexistence hazard | Shell-private ALC per 11 G4 — insulates against OTHER plugins' dependency closures (RhinoMCP is now a standard operator install, 05 §5), which is version tolerance against the plugin ecosystem, not just the host |
| 6 | Bundled .NET runtime version (10.0.2 today) | Shell/Cargo TFM | T2 | `net10.0` assemblies load on any ≥10 bundled runtime; the runtime version is recorded in the host fingerprint (M4) so a bundle jump is visible evidence, not a mystery |
| 7 | `RhinoDoc` lifecycle (`DocumentScope`), geometry, `ViewCapture`/display pipeline | Cargo | T1 | Direct binding; recompiled per fingerprint change (M4) |
| 8 | Eto drawing/platform behaviors | Cargo | T2 | Behind the capture/UI owner; failures are typed facts, not crashes |
| 9 | ALL `Grasshopper2.*` + `GrasshopperIO` | Cargo | T3 | M2 choke point `Gh2Lane` — the sole assembly-referencing owner; M1 probes gate every GH2 capability |
| 10 | LanguageExt/Thinktecture under collectible ALC | Cargo | T0 package, T2 runtime behavior | Per-ALC copies (07 §1 pitfall 7); the trait-resolution-under-ALC behavior (01 F11) is a Phase-0a probe outcome, not an inherited incantation |
| 11 | Command-history window text | Cargo | T2 | Secondary, fail-open evidence channel only; typed facts are primary (the F8 diff heuristic never becomes load-bearing again) |
| 12 | Scenario assemblies compiled against bundle HintPaths | Cargo | regime A | M4 fingerprint gate: host update → automatic rebuild+restage+hot-swap |
| 13 | Bundle discovery (`Rhino*.app`, `CFBundleVersion`) | Supervisor | T2 | Discovery by bundle metadata pattern; no "WIP" literal anywhere load-bearing — the same discovery finds `Rhino 9.app` at GA with zero adjustment; env override optional, not required |
| 14 | `open -b/--args/--env` launch semantics | Supervisor | T2 | Verified semantics (07 §5); failures are typed launch faults |
| 15 | AE-quit via `NSRunningApplication.terminate`, SIGKILL escalation | Supervisor | T2 | Port-verbatim ladder (09 §4); every escalation step's outcome is a typed phase fact (no swallow points) |
| 16 | Crash-marker paths (autosave name, `.ips` glob) | Supervisor | T2 | **Derived from `CFBundleName`/`CFBundleExecutable` of the discovered bundle** (§1 finding); scoped to the supervised bundle only |
| 17 | `.ips` JSON content parse | Supervisor | T2 | Fail-open: parse failure degrades to "crash report present, unparsed" fact with the raw path |
| 18 | kqueue `NOTE_EXIT` P/Invoke | Supervisor | T0 | Direct (11 G3 prices it); poll fallback is degraded-mode, also evidence-bearing |
| 19 | EventPipe / `DOTNET_DiagnosticPorts` | Supervisor | T2 | Capability row gated on Phase-0c; absence = diagnostics lane `unsupported`, never an error |
| 20 | yak CLI for shell deploys | Supervisor | T2 | Inside the `redeploy` transaction; CLI output drift is a transaction fault with captured output |
| 21 | Wire DTOs, status algebra, version negotiation | Contract | none | Zero host types by law; the frozen layer is host-blind |

Reading of the register: the shell's load-bearing host surface is rows 1–4 — five T1 members and two T0 facilities. That is the entire compile-time contract the frozen layer has with a monthly-moving host, and it is the oldest, most stable slice of RhinoCommon. Everything that historically churned (GH2, markers, quit behavior, evidence channels) sits in hot-swappable or out-of-host layers. This is the no-pinning mandate satisfied structurally: the layer that cannot be cheaply redeployed is coupled only to the surface that does not move.

## [3]-[TOLERANCE_MECHANISMS] — four mechanisms, designed end-to-end

### [M1]-[CAPABILITY_PROBES] — probe the member's behavior, never the version string

- Trigger: session start (once), immediately after cargo load, before any scenario. Re-triggered only by host-fingerprint change.
- Behavior: a closed `HostCapability` smart-enum — one row per optional lane — where each row carries its probe as a constructor delegate (`POLICY_VALUES`: the row owns its behavior; no parallel probe table). A probe is an executable assertion of the capability's actual contract, not a presence check and never a version comparison: the GH2 dataflow row solves a trivial three-component document via `Start(SolutionMode.Headless)` under a bounded deadline and reads `SolutionData.Tree()` back; the `.ghz` diff row round-trips a two-node archive through `Differences`; the exception-tap row verifies the subscription took. Probes run inside the same crash-durable evidence spool as scenarios (10 §1.2) with the heartbeat active, so a probe that hangs hits the session deadline with a named owner and a probe that crashes the host is attributed to the probe, not to the first scenario after it (the FM4 misattribution class, pre-closed for probes).
- Evidence: a typed `CapabilityReport` — `Seq` of (capability key, outcome, probe receipt) — streamed as session facts and folded into the session envelope. Admitted once at the boundary (`BOUNDARY_ADMISSION`): interior code reads the report; nothing re-probes mid-session.
- Why this kills regime C: the constraint's only durable encoding IS the probe. "Never settles" folklore cannot drift, because nothing stores "never settles" — each session derives settles-or-not from the host it is actually talking to, and the derivation cost is one trivial solve.

### [M2]-[CHOKE_POINT_ADAPTERS] — one owner type per churny surface, placed in the hottest layer

- Trigger: any T3 surface use; permanently structural.
- Behavior: `Gh2Lane` (cargo) is the single type — `DEEP_SURFACES`, one deep owner — that references `Grasshopper2.*`/`GrasshopperIO` types. Scenarios and the rest of cargo speak `Gh2Lane`'s bridge-owned vocabulary (solve receipts, tree projections, archive diffs as typed values), never raw GH2 types; the shell is GH2-blind (row 2: GUID as data). The repo's existing wrappers-only discipline for GH2 scenario bodies (06 §6.4, FM11 root fix) generalizes into this owner. Same pattern at smaller scale: marker-path derivation owns rows 16–17; the capture owner owns rows 8/11.
- Evidence: a GH2 break under this design has a measurable fix radius — recompile fails (regime B) or a probe fails (regime C) **in exactly one file**, the fix is a cargo edit, and the redeploy is an ALC hot-swap with `unloadConfirmed`. No relaunch, no yak, no dialog gauntlet. The Feb-2025 root-namespace rename — the worst GH2 break on record — would have been a one-file, zero-relaunch event.
- Doctrine note: this is the planned `boundaries.md` resource/wire law instantiated; the named deviation is that the owner exists for *churn isolation*, not only resource lifetime — justified because GH2's breaking-changes license is a host-forced fact (06 §1).

### [M3]-[VERSIONED_WIRE] — additive-only contract, unknown-tolerant by construction

- Trigger: every connect (hello) and every contract evolution.
- Behavior: the Contract assembly declares one frozen-forever negotiation method: hello carries `(contractVersion, capabilityKeys)` both ways, alongside the host fingerprint (M4) and the shell's capability facts (rows 3, 19). Everything else evolves additively: fields are never removed, renamed, retyped, or semantically reused — new meaning is a new field; new operations are new RPC methods. Tolerance is then structural, not policed: STJ's default deserialization ignores unmapped members (an older shell reading a newer supervisor's DTO drops the new fields harmlessly), and calling a method an older shell lacks surfaces as JSON-RPC `-32601` (StreamJsonRpc `RemoteMethodNotFoundException`), which the supervisor maps to capability-absent — the SAME degradation lattice as a failed host probe. One capability algebra spans both skew directions: "this host cannot do X" and "this shell does not yet do X" are one typed outcome, not two error vocabularies.
- Evidence: negotiated version + capability intersection is the first session fact; a skewed pair operates on the intersection and says so, instead of today's silent `JsonException`/null-field corruption (01 F2, D9). The contract is generated source-of-truth for both code and docs (09 §5's drift lesson: the README documented a `schema` field the code never had — impossible when the Contract assembly is the only declaration).
- Scope honesty: M3 protects supervisor↔shell skew (the seam no-pinning creates inside the tool). It does not protect against host churn — that is M1/M2/M4's job; keeping these concerns separate is what lets the shell stay frozen while the host moves.

### [M4]-[FINGERPRINT_GATE_AND_TYPED_DEGRADATION] — self-healing on host update, named faults at the floor

- Trigger: hello returns the host fingerprint — `(CFBundleVersion, RhinoCommon AssemblyFileVersion, Grasshopper2 AssemblyFileVersion, bundled runtime version)`. The supervisor compares it to the fingerprint recorded in the cargo/scenario staging manifest at compile time.
- Behavior, in degradation order: (a) fingerprint match → proceed; (b) mismatch → automatic rebuild of cargo + scenario projects (HintPaths re-resolve into the updated bundle — the build self-heals with zero operator action), restage by content hash, hot-swap, re-probe (M1 re-derives every capability against the new host); (c) rebuild fails in a choke point → regime-B compile fault naming the owner file — the one-file fix radius; (d) the floor: a `MissingMethodException`/`TypeLoadException` escaping at invoke (gate bypassed or partial bundle update) is caught at the cargo invoke boundary — the `EXCEPTION_CAPTURE` pattern of rails-and-effects §2, a named boundary exemption — and classified as a `host-drift` fault carrying the missing member name plus both fingerprints. Scenario-level degradation rides the same lattice: `[RhinoScenario]` entries declare required capability keys; the runner derives run/skip per scenario from the `CapabilityReport` (`DERIVED_LOGIC` — one fold, no per-scenario conditionals), and an unmet requirement reports the existing `unsupported` status (the ported `PhaseStatus` algebra already owns this rank, 01 §2) with the probe receipt attached: `dataflow lane unavailable on this host: Start(Headless) probe faulted — <probe receipt>`.
- Evidence: host updates appear in session output as a first-class event — fingerprint delta, rebuild receipt, fresh `CapabilityReport` — so "the host moved" becomes the most legible event in the system instead of the least. `unsupported` ≠ `failed` is the load-bearing distinction: a capability the host lacks never reddens a run, and a capability the host claims but flunks does.

## [4]-[ANTI_FRAGILITY_AUDIT] — what still breaks each mechanism, and whether that is acceptable

| [MECHANISM] | [HOST CHANGE THAT STILL BREAKS IT] | [RESIDUAL BLAST RADIUS] | [ACCEPTABLE?] |
| :---------: | ---------------------------------- | ----------------------- | :-----------: |
| M1 probes | Semantics drift the probe does not exercise (probe passes, deeper behavior differs) | Wrong scenario verdicts until a scenario contradicts the probe; bounded by writing probes as behavior assertions, not presence checks | [YES] — irreducible on an unfrozen host; the alternative (broader probes) converges on running the test suite twice |
| M1 probes | A probe that crashes the host natively (the FM4 deferred-crash class) | One session loss, correctly attributed to the named probe via the crash-durable spool + kqueue exit watch | [YES] — attribution is the design goal; host-native crashes are host law (09 §3) |
| M2 choke points | GH2 break so deep the lane's *vocabulary* no longer maps (e.g. solution model redesign at beta) | One-file rewrite + scenario expectations touched; still zero relaunch | [YES] — fix radius, not fix absence, is the promise |
| M2 placement law | A T1 member the shell load-bears on is removed or reshaped (e.g. `RhinoApp.Idle` semantics change) | Shell recompile + one supervised `redeploy` (relaunch); the rarest event class by tier construction | [YES] — accepted explicitly; tolerance here would mean reflection in the shell, which §5 bans as worse than the disease |
| M2 GUID-as-data | GA changes GH2 packaging such that `LoadPlugIn(Guid)` is no longer the load path | Config + possibly one cargo edit; shell untouched (it only forwards the load request) | [YES] — verify failure mode (return-false vs throw) in the build wave |
| M3 wire | A change that must alter hello itself | Breaks negotiation — the one frozen method | [YES] — by construction hello's parameters are a single extensible record; only a transport-level redesign breaks it, which is a new tool |
| M3 wire | STJ changes its unmapped-member default | None in practice (documented stable default); pin the behavior explicitly via serializer options at the Contract boundary so the tolerance is declared, not inherited | [YES] |
| M4 fingerprint gate | Host updated mid-session | Impossible — macOS cannot replace a running bundle's loaded images; next connect sees the new fingerprint | [YES] |
| M4 self-heal rebuild | Host update breaks the *production libs under test* (not the bridge) | Ordinary static-rail compile failure, correctly attributed to the lib — outside bridge scope by design; the bridge reports, never repairs | [YES] |
| M4 floor catch | Drift inside a scenario's own host calls (scenario compiled fresh, host honest) | Cannot occur in regime A (fresh compile = current surface); regime C residue handled by M1 | [YES] |
| Marker derivation | McNeel changes the autosave/`.ips` *scheme* (paths, not names) at GA | Reconcile misses markers → recovery dialog reappears → but the supervisor's alive-but-silent watchdog names it (`dialog-suspected`, 10 §1.2) instead of a blind 90s timeout; scheme re-derivation is one supervisor edit | [YES] — degraded-but-diagnosed is the contract for all T2 behavior facts |

The audit's summary judgment: every residual break is either (a) attributed by name at the moment it happens, or (b) confined to a one-file fix in a hot-swappable layer, or (c) the explicitly-accepted rare case (shell T1 break → one supervised redeploy). Nothing silently degrades, and nothing requires version pinning — the host moves and the tool's response is rebuild + re-probe + report.

## [5]-[REFLECTION_BUDGET]

Doctrine baseline (language.md §5): reflection-based dispatch is rejected as a domain surface. The bridge earns two narrow, named exemptions and bans everything else — and the net result is LESS reflection than the current tool, not more.

[BANNED]:
- Reflection against stable RhinoCommon (T1): the compiler is the drift detector there; reflection would convert loud compile-time breaks into silent runtime nulls — the exact F9 failure shape.
- Reflection as a steady-state call path anywhere: probes may reflect once; per-call dispatch goes through compiled typed code (cargo recompiles per fingerprint, so the typed path is always current — there is no standing reason to late-bind).
- Reflection to dodge a compile error after a host break: the break IS the signal; fix the choke point.
- `[UnsafeAccessor]` against host members: it binds a signature shape with reflection's runtime fragility and none of its probe-ability — worst of both; if ever considered for a T4 surface it follows the same probe-gate rules as raw reflection.
- The current tool's 5-hop `ReflectGrasshopperSolution` chain (01 F9): DELETED, not ported — 06 §2 found public equivalents (`SolutionServer.State`, `ObjectSolutionState.Data`, `SolutionData.Tree()`); the rebuild's GH2 solution reads are compiled typed calls inside `Gh2Lane`.

[JUSTIFIED] — two cases, both probe-shaped, both degradable:
- Probe primitives for members that legitimately may not exist on a given host: forward-tolerance (opportunistically use a Rhino-9-GA-only API while still running on WIP builds that lack it) requires `Type.GetType`/`GetMethod` presence checks as the capability row's first step, followed by behavior assertion through the typed path when present. This is reflection as *admission evidence*, executed once per fingerprint, never as dispatch.
- T4 internals where no public path exists AND the capability is optional: permitted only inside a `HostCapability` row + `Gh2Lane`-style owner, with the probe gating it and `unsupported` as the absent outcome. (Today's corpus has zero such needs for the bridge after F9's deletion; the budget exists so that when one appears it lands in the lattice instead of as a bare `GetField` somewhere.) Production-library reflection (e.g. `WireShape` internals in `Rasm.Grasshopper`) is out of scope — that is the lib's own boundary, tested BY the bridge, not part of it.

Budget accounting: rebuilt-bridge reflection = capability-probe presence checks only, each one paired with a typed steady-state path and a typed degradation. Current-tool reflection = a silent 5-hop diagnostic chain with hard-coded nulls. The no-pinning mandate is satisfied with a net reflection decrease.

## [6]-[BUILD_WAVE_HOOKS] — what this design asks the implementation wave to verify

1. `LoadPlugIn` failure mode for an absent/repackaged GH2 (return-false vs throw) — the capability row handles either, but the probe's catch shape should match reality (one live check).
2. GA marker naming: the `CFBundleName`-derivation rule reproduces `Unsaved Rhino 8 Document.3dm` for Rhino 8 on this machine (verified 2026-06-10); confirm the same derivation against the first Rhino 9 GA candidate bundle.
3. Probe cost ceiling: the GH2 dataflow probe is one trivial solve per session; if measured cost exceeds ~1s, key the `CapabilityReport` cache by host fingerprint (probe once per host build, not per session) — the design permits this without change.
4. Re-verify STJ unmapped-member default and StreamJsonRpc `-32601` → `RemoteMethodNotFoundException` mapping against the pinned StreamJsonRpc 2.25.x at contract-authoring time, and declare the serializer tolerance explicitly in Contract serializer options rather than inheriting the default.
5. Coordination with the contract/evidence design doc (this wave): shell fail-open T2 taps (rows 3, 19) should surface as capability facts in hello so the supervisor never infers diagnostic availability — recommended here, owned there.
