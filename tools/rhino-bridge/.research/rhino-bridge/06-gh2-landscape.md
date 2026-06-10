# [06] Grasshopper 2 Automation Landscape (June 2026)

Scope: GH2's current state on macOS Rhino 9 WIP, solver architecture vs. the repo's hard-won findings, SDK/scripting maturity, document-level automation, and GH1-parity gaps a bridge cares about. Every claim is sourced either from the CURRENT installed assembly (RhinoWIP `9.0.26153.12416`, built 2026-06-02, decompiled via `tools.quality api query gh2|gh2-io` — no live-Rhino interaction) or from 2025-2026 McNeel discourse/yak sources. Repo-memory reconciliation is explicit per claim.

## 1. GH2 status and distribution (mid-2026)

| Fact | Evidence | Status |
| --- | --- | --- |
| GH2 still ALPHA; SDK freezes only at beta ("no more breaking changes") | Rutten, 2025-02-20, alpha thread; no beta announcement found through 2026-06 | [CURRENT] |
| Rhino 8: GH2 via PackageManager, yak `grasshopper2 2.0.9340-wip.16200` (2026-01-15, 63k downloads, `-wip` tag) | yak.rhino3d.com/packages/grasshopper2 | [CURRENT] |
| Rhino 9 WIP: GH2 ships IN-BOX as `ManagedPlugIns/Grasshopper2Plugin.rhp` (Grasshopper2.dll 27.4MB + GrasshopperIO.dll + FSharp.Core, MathNet.Numerics, Microsoft.Build in-folder) | local bundle inspection | [VERIFIED] |
| Root namespace is `Grasshopper2` (renamed from `Grasshopper` in Feb 2025 alpha) | Rutten, 2025-02-20 | [CURRENT] |
| Rutten active on GH2 daily-driver issues as of 2026-06-08 (Eto-based rendering, yak plugin dirs, UV components) | discourse search, posts 2026-03..06 | [CURRENT] |
| No GH2 support in Rhino.Compute/Hops; all headless-compute discourse traffic (2025-11..12) is GH1 | discourse search `grasshopper 2 headless after:2025-06-01` | [GAP] |
| No public release-notes stream for GH2; changes surface in the alpha megathread + WIP release posts | category scan | [GAP] |

macOS-specific signal (2026): users report GH2 "much cleaner and a lot faster" than GH1 but with component freezes (persistent blue-ellipsis state after rapid slider movement, manual recompute to clear), no canvas auto-scroll, missing Cap Holes / slider-typing / option-drag (Experience thread, 2026-02-12). June 2026 "broken preview" report self-resolved as a GH1→GH2 converter artifact, not a display regression (Petras_Vestartas, 2026-06-07). Net: macOS GH2 is usable-but-rough alpha; the freeze reports are consistent with solver/UI thread contention the bridge also fights.

## 2. Solver architecture — reconciliation against repo memory

Repo memory `reference_gh2_headless_solution_limits` (verified 2026-05-29) vs. the 2026-06-02 assembly:

| Memory claim | Current-build finding | Verdict |
| --- | --- | --- |
| Bridge scenarios run on the Rhino UI thread | Bridge architecture fact, not a GH2 fact | [CONFIRMED-STILL-TRUE] |
| `SolutionMode` = Regular, Headless, Validating, ValidatingSafe | Identical; `Headless` doc-comment now read in full: "This solution is running in a headless Rhino, or otherwise not while the document is loaded in the canvas" — headless solving is an EXPLICIT design target | [CONFIRMED-STILL-TRUE] |
| `StartWait` forbidden on UI thread | Doc-comment unchanged: "You should never call this function while on the UI thread." `StartWait` = `CreateSolution` + `StartSolution` synchronously on the calling thread | [CONFIRMED-STILL-TRUE] |
| `Start(SolutionMode)` is "deferred — schedules a solve that finishes on the canvas/UI idle loop, which the headless bridge never pumps" | CONTRADICTED by decompile: `Start` returns `Task<Solution>`; after sync short-circuits (cancelled / `ComputableCount == 0` / superseded id) it sets `SolutionPhase.Running` and runs the ENTIRE solve (`StartSolution` → `RunSolution`, a plain `for` over `solution._computable` calling `IDocumentObject.Compute`) inside `Task.Run` on a threadpool worker. No idle-loop dependency is visible in `SolutionServer` | [CHANGED-OR-CORRECTED] |
| "Headless solve never settles; ExpiredCount stays 2 for unconsumed toggles" | The empirical observation is compatible with the current code WITHOUT implying "never settles": unconsumed sources are plausibly not in `_computable` (a doc with `ComputableCount == 0` completes instantly), and `RunSolution` only computes objects in Phase Expired/Cancelled/Faulted with the primary flag set. `ExpiredCount` was the wrong oracle, not proof of a stuck solver | [REINTERPRETED] |
| "Reading a computed value needs raw `IParameter.VolatileData` + IGH_Goo casting — fragile" | GH1 vocabulary; no such members exist on GH2's `IParameter`. The real read path is structured: `IDocumentObject.State` → `ObjectSolutionState` (`Phase`, `Data`, `FaultException`, `CurrentId`) → `SolutionData` (`Tree()`, `Tree(Guid key)`, `KeyedTreeCount`, `Messages`, `Duration`, `CustomData`) → `Grasshopper2.Data.ITree` | [CHANGED-OR-CORRECTED] |

Completion observability that the memory did not record (all public, current build):

- `Task<Solution>` from `Start` completes when `RunSolution` finishes on the worker thread.
- `Solution.Phase` (public) + `Solution.StateChanged` event.
- `SolutionServer` events: `SolutionAboutToStart/Started/Stopped/Cancelled/Completed/Faulted` — `Stopped` is guaranteed paired with `Started`.
- `SolutionServer.State` (`ServerState`: `CurrentlyRunning`/`MostRecentlyCompleted` + phase) and a `CircularArray<SolutionRecord>` history (8192 entries).
- `SolutionServer.EnableSolutions` static gate; `_triggerViewportUpdatesOnCompletedSolution` (default false) is the ONLY viewport touch in the completion path.

Consequence for a rebuilt bridge: a UI-thread scenario can `doc.Solution.Start(SolutionMode.Headless)` then poll `Solution.Phase`/task status with a timeout while the worker thread computes independently — no `StartWait`, no message pump needed for the solve itself. The unverified hazard: individual `Compute` implementations (notably RhinoCode script components, UI-bound previews) may marshal to the UI thread and deadlock a blocking UI-thread wait; pure native components should not. This is the single highest-value EMPIRICAL probe for the rebuild: place slider→addition→panel, `Start(Headless)`, poll with deadline, read `param.State.Data.Tree()`. If it settles, the repo's "computed-value dataflow is unreachable" ceiling is GONE; if it deadlocks, the fallback is dispatching `StartWait` from a worker thread, which the doc-comments bless.

Repo memory `reference_gh2_bridge_isolated_loading`: [CONFIRMED-STILL-TRUE] structurally. GH2 remains a single in-bundle host plugin (27MB-class) loaded into the default ALC; nothing in 2025-2026 sources offers an isolated-ALC-loadable GH2, so the pre-load-via-`PlugIn.LoadPlugIn` + drive-through-wrappers pattern (and the "no raw `Grasshopper2.*` types in isolated scenario bodies" constraint) stands. The compile-vs-runtime tension is a bridge-resolver property, not something GH2 changed.

## 3. Document-level automation surface (current build, decompile-verified)

- `Grasshopper2.Doc.DocumentIO` — per-document IO capsule: `Open(string path, ...)` / `Save(string path, FileContents)` / `SaveCopy` / `Export(string path, ...)` overloads that take explicit paths (the parameterless variants raise Eto dialogs; `Window` property exists only for the dialog variants). `RetainRoot` keeps the raw archive `Node` after open/save — direct structural inspection of what was loaded.
- File formats: `.ghz` (GH2 zip archive), `.sml` (text SML), `.ghbackup`, `.ghautosave`; GH1 `.gh`/`.ghx`/`.ghdata` constants are present (a GH1→GH2 converter exists; the 2026-06-07 preview bug came from it — treat conversion as lossy/experimental).
- `GrasshopperIO.dll` — the GH2 analog of GH1's `GH_IO.dll`, shipped as a separate assembly with XML docs (`tools.quality api` key `gh2-io`): `IO`, `IReader`/`IWriter`/`IStorable`, `[IoId]` attribute typing, `DataBase.Archive` (+ `Compression`), `Node`/`Item`/`Value` dictionary model — and a built-in DIFF engine (`Difference`, `Differences`, `NodeMatcher`, `EqualityLevel`) that GH_IO never had. `DocumentIO.ObjectNodeMatcher` matches object nodes across reordered `.ghz` archives by `id`+`__ioid`.
- Solve + read: section 2's `Start(Headless)` → `ObjectSolutionState.Data` → `ITree` path is the programmatic equivalent of GH1's `GH_Document.NewSolution` + volatile-data read.

Bridge implication: open/mutate/save/diff of `.ghz` documents is fully scriptable today; GrasshopperIO is plausibly even usable OUT of process for archive-level assertions (pure managed serialization layer — worth one probe before relying on it; it ships only in the plugin folder, not as a NuGet).

## 4. Scripting / SDK maturity

- No official SDK or NuGet package; `Grasshopper2.dll` reference = copy-from-bundle, exactly what this repo already does. Breaking changes remain licensed until beta (Rutten policy, reaffirmed Feb 2025; unanswered "is it stable yet" question in the dev-support thread, 2026-05-14).
- GH2 script components (C#/Python3/IronPython via RhinoCode, eirannejad) exist in Rhino 9 WIP since 2025-05-30: multi-threaded execution, `IPear` data+metadata access, per-iteration debug. Known-rough as of late 2025 (terminal output broken Oct 2025; editor UI being rewritten Nov 2025). They REQUIRE Rhino 9 — one reason GH2-in-R9 is the right host for this repo.
- Plugin ecosystem: GH2 plugins distribute via PackageManager/yak (Rutten, 2026-03-10); plugin authoring is possible but on a moving SDK.
- Community automation appetite is real and unmet: "If Grasshopper was rebuilt for 2026" (2025-10/11) carries repeated headless-execution wishes; staff pointed at GH1 Player/Compute answers only.

## 5. GH1-parity matrix for a bridge

| Bridge need | GH1 had | GH2 now (June 2026) | Verdict |
| --- | --- | --- | --- |
| Standalone file IO library | `GH_IO.dll` (.gh/.ghx) | `GrasshopperIO.dll` (.ghz/.sml) + archive diffing | [PARITY-PLUS] |
| Programmatic open/save w/o dialogs | `GH_DocumentIO` | `DocumentIO.Open/Save(path)` | [PARITY] |
| Headless solution + completion signal | `NewSolution(true)` + `SolutionEnd` (powers Compute) | `Start(Headless)` → `Task<Solution>` + `Solution*` events + `Phase` | [PARITY-AT-API-LEVEL] — empirically unproven in this bridge |
| Read computed values | volatile data / `IGH_Goo` | `ObjectSolutionState.Data` → `SolutionData.Tree()` → `ITree` | [PARITY] — richer (Messages, Duration, keyed trees) |
| Headless Rhino host | Windows-only headless Rhino / Compute | unchanged; macOS still requires live `RhinoWIP.app` | [GAP] — the bridge's raison d'etre, unchanged |
| Compute/Hops server route | yes | no GH2 support found | [GAP] |
| Name→GUID component proxy lookup | `GH_ComponentServer` | GH2 `Framework.PluginServer`/proxy registry exists but unwrapped in Rasm (memory claim, not re-verified) | [OPEN] |
| Stable SDK | yes | alpha until beta freeze | [GAP] |

## 6. Actionable conclusions for the bridge rebuild

1. The repo's "GH2 dataflow cannot be verified headless" ceiling rests on a solver model the CURRENT assembly contradicts. Before baking that limit into a new bridge design, run the one-scenario probe in section 2 (cost: one scenario file; payoff: computed-value oracles for every GH2 test).
2. Design the GH2 lane around `SolutionServer` events + `Task<Solution>` rather than canvas repaint: `SolutionCompleted/Faulted` are exactly the fact-stream hooks the bridge's phase/fact model wants, and `SolutionRecord` history gives post-hoc evidence without racing the solve.
3. Treat `GrasshopperIO` + `DocumentIO(path)` as a NEW capability class: golden-file `.ghz` round-trip and archive-diff assertions (the built-in `Differences` engine) work without solving at all — cheap regression rail that GH1-era tooling never had.
4. Keep the isolated-ALC discipline unchanged (pre-load via `HostPlugins`, wrappers-only in scenario bodies); nothing in 2025-2026 GH2 changes it.
5. Do not wait for GH2 stability: alpha + in-box R9 WIP + active daily fixes means the bridge MUST tolerate GH2 surface drift; pin assertions to decompile-verified members and re-run `api query gh2` on each WIP bump (the bundled assembly changes under the app-update cycle that already forces bridge relaunches).

## Sources

- Installed assembly decompiles (authoritative): `.artifacts/quality/api/.../query-gh2-Grasshopper2.Doc.{SolutionServer,SolutionMode,Solution,SolutionData,ObjectSolutionState,DocumentIO}/decompile.cs`; `gh2-io` surface index. RhinoWIP `9.0.26153.12416`.
- [New Grasshopper 2.0 alpha 2025/Feb](https://discourse.mcneel.com/t/new-grasshopper-2-0-alpha-2025-feb/199833) — Rutten on alpha/SDK policy, namespace rename; active through 2026-05-13.
- [Grasshopper2 Plugin Development Support](https://discourse.mcneel.com/t/grasshopper2-plugin-development-support/158578) — unanswered production-readiness ask, 2026-05-14.
- [Experience with Grasshopper 2](https://discourse.mcneel.com/t/experience-with-grasshopper-2/215845) — macOS field report, 2026-02-12.
- [Grasshopper 2 Broken Preview](https://discourse.mcneel.com/t/grasshopper-2-broken-preview/219702) — GH1→GH2 converter artifact, 2026-06-07.
- [Rhino WIP Feature: Grasshopper 2 Script Components](https://discourse.mcneel.com/t/rhino-wip-feature-grasshopper-2-script-components/205260) — eirannejad, 2025-05-30 onward.
- [If Grasshopper was rebuilt for 2026](https://discourse.mcneel.com/t/if-grasshopper-was-rebuilt-for-2026-what-should-change-or-stay/211626) — community headless demand, 2025-10/11.
- [yak grasshopper2 package](https://yak.rhino3d.com/packages/grasshopper2) — `2.0.9340-wip.16200`, 2026-01-15.
- DavidRutten 2026 posts: [Yak Grasshopper 2 and 1](https://discourse.mcneel.com/t/yak-grasshopper-2-and-1/219727) (2026-06-08), [GH2 plugin directories](https://discourse.mcneel.com/t/can-the-plugin-file-directory-of-grasshopper-2-recognize/216687) (2026-03-10).
