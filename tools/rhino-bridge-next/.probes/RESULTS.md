# Phase-0 Live-Host Probe Results

Run 2026-06-12 against RhinoWIP `9.0.26160.12306` (macOS, arm64) via the OLD frozen bridge tool
(`tools/rhino-bridge`) driven through `uv run python -m tools.assay bridge <verb>` and direct client
spawns serialized under the assay `bridge` flock lease (`.artifacts/assay/locks/bridge.lock`).

## Bracket lifecycle (all OK)

| Step | Mechanism | Outcome |
| ---- | --------- | ------- |
| build | `dotnet build` of protocol->plugin->client->testkit under `build-bridge-Release.lock` | GREEN (assay rail `bridge build` faults pre-spawn with `incoherent closure: 4 tails for one check` — a routing defect in `tools/assay/core/routing.py:place` Input.PROJECT fanning 4 projects to one `_BUILD_TOOL` check; bypassed by direct `dotnet build` per project under the same lease) |
| launch | `bridge launch` (needs `RHINO_WIP_APP_PATH=/Applications/RhinoWIP.app`; the resolver formerly lived in the archived quality operator) | OK, endpoint `rb-<pid>-<suffix>` written to `~/.rasm/rhino-bridge.json` |
| connect/doctor | client hello round-trip over `CoreFxPipe_rb-<pid>-*` UDS | OK (`launch/connect/doctor` all `ok`) |
| quit | `bridge quit` (Apple-Event terminate ladder) | OK, host down, no SIGTERM, no pkill |

Endpoint metadata confirms the OLD tool identity: `bridgeAssemblyName=rasm-bridge`, `version 0.0.1-dev4`.

## PROBE verdicts

### 0a — collectible ALC load/unload :: BLOCKED
Scenario file: `tests/csharp/_testkit/probes/probe-0a-alc-unload.verify.csx` (OLD C#10 dialect; loads
`LanguageExt.Core.dll` + `Rasm.dll` into a collectible `AssemblyLoadContext`, exercises a LanguageExt
`Seq<int>.Add` typeclass call, unloads, then `WeakReference` + 10x `GC.Collect`/`WaitForPendingFinalizers`).
The scenario BODY never runs: the bridge execute phase faults at `RhinoCode.CreateCode` with
`Rhino.Runtime.Code.Languages.LanguageMissingException: "Can not find language *.*.*@*.* for <guid>"`
BEFORE user code. Cargo-disposal contract is unverifiable on this host. NOT a fallback trigger by itself —
the block is host-side language registration (see Root cause), not an ALC-collection failure.

### 0b — GH2 headless split-phase :: BLOCKED (two independent causes)
Scenario files: `probe-0b-gh2-headless.verify.csx` (GH-aware wrapper route) and
`probe-0b-gh2-headless-reflect.verify.csx` (TestKit reflection route; pre-loads GH2 via
`PlugIn.LoadPlugIn`, drives `SolutionServer.Start(SolutionMode.Headless)` -> `Task<Solution>` then
UI-thread `RhinoApp.Wait()` pump-poll, reads `IDocumentObject.State.Data.Tree()`).
1. Execute lane blocked identically to 0a (LanguageMissingException) — neither variant's body runs.
2. The GH-aware compile route (`Rasm.Grasshopper.Tests.csproj`) is RED at HEAD: `Rasm.Grasshopper`
   fails with `CS8016` transparent-identifier + `CS0117 CanvasOp.ValueEditorCase/SparkleCase does not
   contain SelfOp` in `libs/csharp/Rasm.Grasshopper/UI/Canvas.cs:564,571,576` (a tree-wide analyzer/
   source-gen breakage, not bridge-owned). API shape for the probe WAS decompile-verified live:
   `Solution Start(SolutionMode)` returns `Task<Solution>`; `Solution.Phase/ComputableCount/Mode`,
   `ObjectSolutionState.{Phase,Data}`, `SolutionData.Tree()` all present in `9.0.26160`.

### 0c — EventPipe socket + dotnet-counters :: PASS
Live Rhino pid resolved from endpoint. Diagnostic socket PRESENT:
`$TMPDIR/dotnet-diagnostic-<pid>-<start>-socket`. `dotnet tool run dotnet-counters -- ps` (9.0.661903)
lists `<pid>  Rhinoceros  /Applications/RhinoWIP.app/Contents/MacOS/Rhinoceros`. Both conditions met ->
`Microsoft.Diagnostics.NETCore.Client` admission is supported; the EventPipe/counters lane is live.

### 0d — warm-loop benchmark :: PARTIAL
`doctor` spawn x10 (client-direct `--no-build`, warm existing-endpoint so launch is skipped, serialized
under lease), wall-clock ms: 792 775 773 741 792 745 781 771 726 779. Distribution min=726 median~775
max=792. This sizes SessionPolicy connect/round-trip deadlines and the cutover criterion-4 denominator.
trivial-verify x10 (`probe-0d-trivial.verify.csx`) is BLOCKED by the execute lane (LanguageMissingException),
so the verdict-path warm cost is not measurable on this host build.

## Root cause of the execute-lane block (host-version churn; rebuild-relevant)

On RhinoWIP `9.0.26160`, RhinoCode does NOT pre-register scripting languages at startup, and the FROZEN
tool's one-time registration call short-circuits on the empty registry. Decompiled
`RhinoCodePlatform.Rhino3D.Registrar` (current bundle):
- `StartScriptingLanguages(LanguageSpec spec, bool startServer)` opens with
  `if (IsScriptingLanguageStarted(spec)) return;` where
  `IsScriptingLanguageStarted(spec) => !RhinoCode.Languages.QueryRegistered().QueryAwaiting(spec).Any();`.
  On an EMPTY registry `QueryAwaiting(spec)` is empty -> `.Any()==false` -> the guard returns vacuous-TRUE
  -> the method returns WITHOUT registering. `PrepareLanguages()` (which `Register`s `mcneel.roslyn.csharp`
  v10.0) is therefore never reached.
- The OLD tool's `tools/rhino-bridge/plugin/Rhino/CodeEngine.cs:17-22` lazy `Language` calls exactly this
  vacuous path (`Registrar.StartScriptingLanguages(LanguageSpec.CSharp, startServer:false)`), so
  `Readiness()` is falsely green while no C# language exists; the next `RhinoCode.CreateCode` throws.
- Only `Registrar.StartScripting`/`StartScriptEditing` (and `ScriptEditorCommand`'s ctor, which calls
  `StartScripting(true)`) reach `PrepareLanguages` unconditionally (gated solely on `_registeredLanguages`).
  `StartScriptServer` and the plugin `OnLoad` call only `StartPlatform` (resolver, NOT languages).

Non-invasive warm attempts that did NOT clear it on this build:
- Pre-loading `RhinoCodePlugin` (`c9cba87a-23ce-4f15-a918-97645c05cde7`) via the bridge's own
  `HostPlugins` mechanism (protocol-conformant Execute, `tools/rhino-bridge-next/.probes/wire_bootstrap.py`)
  — runs only `OnLoad`->`StartPlatform`, not `PrepareLanguages`.
- Launching with `-runscript=_-ScriptEditor _Edit _Enter` — the command did not construct synchronously
  before the bridge served the request; languages stayed unregistered.
- (Patching the global Rhino `StartupCommands` settings XML and AppleScript keystroke injection were both
  correctly out of scope and not performed.)

REBUILD IMPLICATION: the new CargoHost/CodeEngine equivalent MUST register languages via
`Registrar.StartScripting(startServer)` / `StartScriptEditing` (unconditional `PrepareLanguages`), NOT
`StartScriptingLanguages(spec)` which is a no-op on a cold registry. This is a charter-grade fix the
rebuild owns; it also explains why prior `9.0.26153` sessions ran scenarios green (that build pre-warmed
languages at startup). Re-probe per WIP bump is exactly the M1-M4 tolerance lane.

## CORRECTION (verified 2026-06-12): StartScripting is NOT sufficient; three independent gates

The REBUILD IMPLICATION above is correct that `StartScriptingLanguages(spec)` is a cold-registry no-op,
but `StartScripting` alone does NOT make `RhinoCode.CreateCode` resolve C#. The OLD `tools/rhino-bridge`
execute lane was restored to `execute:ok` on `9.0.26160.12306` (cold + warm, twice consecutive) only after
fixing THREE distinct gates, each decompile-verified against the live bundle and each newly exposed by the
26160 host churn. The new `CargoHost`/`CodeEngine` must replicate all three.

1. LANGUAGE LOAD (not just register). `Registrar.StartScripting` -> `PrepareLanguages` registers the C#
   `LoadLanguageDelegate` as an AWAITING `LanguageEntry` (`HasLoader=true`); it never invokes the loader.
   `RhinoCode.CreateCode` resolves via `DefaultLanguageResolver.TryResolve` (which matches the awaiting
   ENTRY) then `OfIdentity(entry.Id)` against the LOADED `m_registry` — which is empty until a loader runs,
   so it returns null and `CreateCode` throws `LanguageMissingException`. The loader runs only inside
   `LanguageRegistry.InvokeLoaders`, reached by `RhinoCode.Languages.WaitStatusComplete(spec, responder)` /
   `WaitLoadComplete(spec)`. Drive it SYNCHRONOUSLY and INLINE: do NOT use `Registrar.StartScriptingLanguages(spec)`
   on a live host — when `Application.Instance != null` it queues `WaitStatusComplete` onto
   `Application.Instance.Invoke` (a DEFERRED UI callback that has not run when `CreateCode` fires inside the
   same UI-thread handler). Canonical drive: `RhinoCode.ReportProgressToConsole = true;`
   `Registrar.StartScripting(startServer:false);`
   `RhinoCode.Languages.WaitStatusComplete(LanguageSpec.CSharp, new RhinoCodePlatform.Rhino3D.Languages.RhinoWriteQueryResponder())`.
   `RhinoWriteQueryResponder` is public + UI-free (`RhinoApp.WriteLine` only); `RhinoProgressBarLanguageQueryResponder`
   is UI-bound (`StatusBar` + `Application.Instance.RunIteration`) and must NOT be used headless. On a cold
   first run `WaitStatusComplete`'s `Continue()->Start()` may install net48 reference assemblies (NuGet or the
   embedded fallback under `RHINOCODE_BLOCK_INTERNET`), so pre-warm once at host start to amortize.

2. SPEC BINDING (never CreateCode(text/uri) for C#). Even with C# loaded, `RhinoCode.CreateCode(string text)`
   resolves the spec from text content; plain C# source has no discriminator (the only C# text specifier is a
   shebang; `.cs`/`.csx` is an EXTENSION specifier), so spec auto-detection yields `LanguageSpec.Any`, which
   `DefaultLanguageResolver.TryResolve` HARD-REJECTS (`if (LanguageSpec.Any == spec) return false`) ->
   `LanguageMissingException "*.*.*@*.*"`. The `.csx`-uri path resolves by extension ONLY when the file exists
   on the HOST filesystem. Bind the language explicitly instead: resolve once via
   `RhinoCode.Languages.QueryLatest(LanguageSpec.CSharp)` (returns the loaded `ILanguage`, or null if the
   loader never ran) then call `ilanguage.CreateCode(text)` / `ilanguage.CreateCode(uri)`. This binds the
   language directly and is path-independent.

3. REFERENCE PATHS + `#r` IS DEAD. RhinoCode's C# transformer (`CSharpCodeScriptTransformer.TransformCompilerDirectives`)
   comments out EVERY `#r`/`#load` line before Roslyn parses — script-embedded `#r` directives NEVER reach the
   compilation in any construction mode. References must be added PROGRAMMATICALLY:
   `code.References.Add(CompileReference.FromPath(absPath))` per reference; `RoslynCode.TryCompile` reads
   `code.References` (gated on `PathExists`) regardless of text-vs-uri construction. Dedupe by simple assembly
   name (`RoslynLoadContext` keys by filename-without-extension, first wins). CRITICAL PATH-ANCHOR FACT: the
   OLD client emits reference and `ScriptPath` values RELATIVE to its own cwd (the repo root), but the macOS app
   bundle resets the Rhino process `Directory.GetCurrentDirectory()` to `/`, so a raw `File.Exists` silently
   drops every relative reference (`resolved=0` -> CS0246 on every scenario type). The launching client is
   Rhino's PARENT process, so the client cwd survives as the inherited `PWD` environment variable even though
   `GetCurrentDirectory` does not. Resolve relative paths against `Path.GetFullPath(path, basePath: PWD)`. The
   new tool should prefer fixing this at the protocol (carry the client worktree/cwd as an explicit
   `BridgeExecuteRequest` field, or send absolute reference paths) rather than relying on `PWD` inheritance,
   which is a launch-topology assumption, not a contract.

Verified bracket (rasm-bridge `1.0.3-langreg`, `9.0.26160.12306`, `probe-0d-trivial.verify.csx`):

| Pass | launch | build | connect | execute | liveness | cmd |
| ---- | ------ | ----- | ------- | ------- | -------- | --- |
| cold | ok     | ok    | ok      | ok      | ok       | ok  |
| warm | skipped| ok    | ok      | ok      | ok       | ok  |

Doctor on a healthy host carries NO `rhinocode`/`create-probe` fault noise; the language-registry census
(entry count + identities + `IsAwaiting`) and ALC topology dump fire ONLY on the readiness create-probe
failure path, which discriminates "loader never registered" (count==0) from "loader never invoked" (all
`IsAwaiting`) from a resolver/spec change without a fresh decompile session.

## G5 sizing (corpus default, needs operator sign-off)

10-distill.md C1 §3 and §7 (review 00 op-3 / G5): order-of-magnitude effort = 1.5-2.5x the 2,158-LOC C#
body + the 21-file `.verify.csx` corpus migration; operator sign-off gates Phase 1. Working default
recorded; not re-derived.

## Artifacts
- Scenario sources: `tests/csharp/_testkit/probes/probe-0a-alc-unload.verify.csx`,
  `probe-0b-gh2-headless.verify.csx`, `probe-0b-gh2-headless-reflect.verify.csx`, `probe-0d-trivial.verify.csx`.
- Bridge result JSON (evidence, retained): `tools/rhino-bridge-next/.probes/results/*.json`
  (`probe-0a.json` carries the full failed execute phase + LanguageMissingException stack).
- Lease/wire helpers: `tools/rhino-bridge-next/.probes/with_lease.py`, `wire_bootstrap.py`.
