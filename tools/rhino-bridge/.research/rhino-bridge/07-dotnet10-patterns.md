# [07] .NET 10 In-Host Bridge Modernization Patterns

Research date: 2026-06-10. Scope: current-source (.NET 10 GA era, 2025-2026) patterns for a ground-up bridge rebuild. Evidence tiers: `[VERIFIED]` = confirmed against official source/docs or local bundle inspection this session; `[NEEDS_LIVE_PROOF]` = plausible per docs but requires a live-Rhino probe (sibling workflow owns the host today).

Decisive local facts that change the rebuild calculus:

| [INDEX] | [FACT]                                                                                  | [EVIDENCE]                                                                                              |
| :-----: | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
|   [1]   | RhinoWIP bundles .NET **10.0.2** (arm64)                                                 | `[VERIFIED]` `RhCore.framework/.../dotnet/arm64/shared/Microsoft.NETCore.App/10.0.2/`                   |
|   [2]   | RhinoWIP bundles Roslyn **5.x daily** (`2.25567.12`, ~Nov 2025)                          | `[VERIFIED]` `RhCore.framework/Versions/A/Resources/Microsoft.CodeAnalysis.CSharp.dll` version strings  |
|   [3]   | The C# 10 scenario ceiling is **RhinoCode policy**, not a compiler limit                 | `[VERIFIED]` `tools/rhino-bridge/plugin/Rhino/CodeEngine.cs:31` (RhinoCode pins `CSharpVersion`)        |
|   [4]   | Repo already builds `net10.0` / `LangVersion 14`; `Microsoft.CodeAnalysis.CSharp` 5.3.0 already central | `[VERIFIED]` `Directory.Build.props`, `Directory.Packages.props:75`                       |
|   [5]   | Current transport (named pipe) is **already** a unix domain socket on macOS              | `[VERIFIED]` corefx PR #6833 — `NamedPipe*Stream` implemented over UDS on Unix                          |

Consequence: every pattern below runs in-host today. No host upgrade, no TFM bridge, no downlevel compromise.

## [1]-[COLLECTIBLE_ALC_HOT_RELOAD]

### Maturity

Collectible `AssemblyLoadContext` is stable since .NET Core 3.0; no .NET-10-specific ALC changes surfaced in release notes or the runtime design docs (negative result — searched .NET 10 GA announcements and `dotnet/runtime` unloadability docs). The mechanism is mature; the discipline is the hard part.

### The proposal that kills the package-update -> relaunch cycle

Split the plugin into a **stable shell** and **swappable cargo**:

- Shell (`.rhp`, loaded once by Rhino into the default ALC): transport server, supervisor RPC, ALC lifecycle, diagnostics. Changes rarely; only shell changes require relaunch.
- Cargo (per verify run): Rasm libraries under test + the compiled scenario assembly, loaded into ONE collectible ALC via `AssemblyDependencyResolver` over content-hash-staged copies (the bridge already stages `refs/<content-hash>/` folders — this slots in directly).

A Rasm library rebuild then costs: stage new hash folder -> `Unload()` old ALC -> new ALC -> run. No Yak deploy, no Rhino quit, no crash-recovery dialog. Yak deploy remains only for shell/protocol changes and for host plugins (GH2 itself).

### Reference-direction law

`[VERIFIED]` Collectible assemblies may reference non-collectible ones; the reverse is forbidden by the runtime (`System.NotSupportedException`, surfaced in roslyn#72366). Therefore:

- RhinoCommon, GH2, Eto stay in the default ALC — cargo references them freely.
- Nothing host-rooted may hold cargo types: no cargo type in a host cache, no cargo delegate in a host event, no cargo Eto control left mounted.
- Scenario assembly and libs-under-test must share the SAME collectible ALC (scenario references libs).

### Unload pitfalls, calibrated to THIS repo

| [INDEX] | [PITFALL]                                                                 | [RASM_INSTANCE]                                                                | [MITIGATION]                                                                   |
| :-----: | ------------------------------------------------------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------ |
|   [1]   | Host event subscriptions pin the ALC                                       | `RhinoApp.Idle` MotionPump, doc events, `OnExceptionReport`                     | Scenario-scope disposal contract; repo's `IDisposable` Seq-of-detachers pattern is exactly this |
|   [2]   | Statics/`[ThreadStatic]` inside cargo                                      | process-static `ConcurrentDictionary` caches, Atom singletons                   | Safe — LoaderAllocator-bound, collected at unload IF no host root remains      |
|   [3]   | Native handles not disposed before unload                                  | GeometrySource capsules, `File3dm`, display conduits                            | Unload waits on finalizers; enforce capsule disposal at scenario end           |
|   [4]   | Timers/threads still running                                               | CADisplayLink Pacer, motion pumps                                               | Pause + Invalidate before `Unload()` (repo already documents Invalidate-before-Dispose) |
|   [5]   | Unload is async + silent on failure                                        | leaked file handles persist (runtime#44679)                                     | `WeakReference(alc)` + GC loop with bounded retries; report `unloadConfirmed` as a bridge fact |
|   [6]   | Debugger inspection of a cargo `Type` pins the ALC forever                 | any attached-debugger session (runtime#124876)                                  | Treat unload-confirmation as unreliable under debugger; gate the fact          |
|   [7]   | Shared dependency loaded in default ALC captures cargo types               | serializer caches, LanguageExt typeclass resolution caches                      | Load LanguageExt/Thinktecture runtime INTO the cargo ALC (duplicate copies, fresh per swap — also means dependency bumps hot-swap too) |

Pitfall [7] is double-edged in the right direction: per-ALC copies of LanguageExt/Thinktecture cost memory per swap but mean a `Directory.Packages.props` version bump is ALSO hot-swappable — the exact package-update pain the operator named.

### Hot Reload (EnC deltas) considered and rejected

`MetadataUpdateHandler`/EnC applies in-place deltas but enforces rude-edit rules (no signature changes, no new types in many cases) and assumes an IDE delta producer. The bridge's unit of change is a whole rebuilt assembly; ALC swap is the correct primitive. `[VERIFIED]` against ASP.NET Core 10 hot-reload docs scope.

## [2]-[SCENARIO_COMPILATION] — lifting the C# 10 ceiling

### Root cause

RhinoCode (`Rhino.Runtime.Code`) pins csx compilation to C# 10 internally; only resolver/compiler options are configurable via `RunContext.Options`. Lifting the ceiling means compiling scenarios ourselves and keeping RhinoCode out of the compile path entirely.

### Option A — `Microsoft.CodeAnalysis.CSharp.Scripting` (CSharpScript)

- `[VERIFIED]` Current main `CSharpScriptCompiler.DefaultParseOptions` is `languageVersion: LanguageVersion.Latest` — modern scripting is NOT language-limited; NuGet ships 5.3.0.
- Globals-object model (`ScriptOptions` + `globalsType`) is a clean way to inject a typed bridge context (doc handle, fact sink, capture sink) into scenarios — better than today's stdout-marker convention.
- `[X]` BLOCKER for a long-lived host: `InteractiveAssemblyLoader` loads each evaluation into non-collectible contexts — assemblies accumulate per run (roslyn#41722, #22219; collectible-loader PR #74915 exists but its merged/shipped status is unconfirmed). A bridge that runs hundreds of scenarios per session leaks the host.

### Option B — plain incremental `CSharpCompilation` (recommended)

~100-150 LOC compile service in the shell:

- Parse with `CSharpParseOptions(LanguageVersion.Latest)` — full C# 14 (extension members, `field`, null-conditional assignment) in scenarios, matching production `LangVersion 14.0`.
- References: host assemblies from the default ALC (RhinoCommon, GH2, Eto via `Assembly.Location` / `MetadataReference.CreateFromFile`) + cargo ALC staged assemblies.
- `Emit` to `MemoryStream` (+ PDB for real stack traces — a diagnostics upgrade over RhinoCode's opaque frames), load into the cargo ALC. Solves the leak (unload-with-cargo) and the reference-direction law simultaneously.
- Deterministic emit + content-hash keying gives a scenario compile cache for free.
- Full `Diagnostic` objects with spans replace the `CompileException.Diagnosis` projection — richer than today's `BridgeDiagnostic`.

### Roslyn collision policy

`[VERIFIED]` RhinoWIP ships its own `Microsoft.CodeAnalysis*.dll` (5.x daily) in RhCore resources for RhinoCode. Two safe postures: (a) load the bridge's pinned Roslyn 5.3.0 into a bridge-private ALC (never the default ALC); (b) bind to the host's bundled Roslyn — works today but version-drifts with every WIP update. Posture (a) is the robust one and costs one extra long-lived ALC.

### Authoring-UX borrowing from .NET 10 file-based apps

`[VERIFIED]` .NET 10 (GA 2025-11-11, LTS) ships `dotnet run app.cs` with `#:package`/`#:property` file-level directives. The pattern — front-matter directives in a single file, harness owns the project model — is exactly the scenario-authoring shape: `//#: plugins grasshopper` / `//#: capture viewport` directives parsed by the shell beat today's convention-by-documentation (`reference_bridge_csx_scenario_constraints` exists because the current rules are unguessable). The C# 10 constraint memory and its workarounds (no collection expressions, no `params` span) all evaporate with Option B.

## [3]-[IPC] — transport vs RPC layer

### Comparison

| [INDEX] | [OPTION]                                  | [FIT]                                                                                                       |
| :-----: | ------------------------------------------ | ----------------------------------------------------------------------------------------------------------- |
|   [1]   | Named pipe + hand-rolled JSON framing (today) | Works; on macOS it IS a UDS underneath. All RPC concerns (correlation, cancellation, events) are hand-rolled |
|   [2]   | Raw UDS + custom framing                   | No gain over [1] on macOS — same kernel object, same hand-rolling                                            |
|   [3]   | gRPC over UDS (Kestrel)                    | `[X]` UNFIT plugin-side: requires the ASP.NET Core shared framework, absent from Rhino's bundled runtime; self-contained Kestrel closure inside a plugin is large and collision-prone. Official UDS docs target standalone services |
|   [4]   | StreamJsonRpc 2.25.x over the existing pipe | Best fit: JSON-RPC 2.0 over any `Stream`/`IDuplexPipe`; bidirectional notifications, request cancellation, client proxy generation, `IProgress<T>` marshaling |
|   [5]   | [4] + `NerdbankMessagePackFormatter`       | [4] with binary payloads via Nerdbank.MessagePack 1.1.x (PolyType `GenerateShape` source-gen, NativeAOT-ready). The author's successor to MessagePack-CSharp |

### Judgment

Transport is NOT the bottleneck — UDS unary latency is O(100 µs) against a 3-8 s host handshake and UI-thread scheduling. The deficit is the RPC LAYER. StreamJsonRpc buys, over today's hand-rolled frame:

- Server -> client **notifications**: facts, progress, and capture events stream live instead of being scraped from stdout by `BridgeMarker.Scan` post-hoc. This is the single largest diagnostics upgrade available (stdout scraping today loses interleaving and dies with the process).
- Bidirectional contracts: the host can push `dialogShown` / `shutdownStarted` / `exceptionReported` to the supervisor — prerequisite for the supervisor-centric inversion the operator suspects is the right linkage direction.
- Typed proxies from one shared interface in `protocol/` — the 538-LOC `BridgeWire` envelope/dispatch code collapses; DTOs remain.
- Cancellation caveat stands regardless of library: Rhino UI-thread execution is not abortable. RPC-level cancellation buys transport hygiene (abandon the call, free the pipe), not UI-thread interruption — keep the README's `--timeout-ms` framing.
- Sizing note: MessagePack formatter is worthwhile only if captures/geometry payloads move inline; for JSON-sized control messages, the default `SystemTextJsonFormatter` is sufficient and keeps wire debuggability.

`[VERIFIED]` versions: StreamJsonRpc 2.25.25 on NuGet; Nerdbank.MessagePack 1.1.62. Also noted: .NET 11 hardens UDS-backed `CurrentUserOnly` named-pipe socket files to mode 0600 — single-user posture should set `CurrentUserOnly` today.

## [4]-[IN_HOST_DIAGNOSTICS]

### Layered shape

| [INDEX] | [LAYER]                          | [PATTERN]                                                                                                  |
| :-----: | --------------------------------- | ----------------------------------------------------------------------------------------------------------- |
|   [1]   | In-plugin instrumentation         | BCL `ActivitySource`/`Meter` — zero-dependency; spans per phase (connect/compile/load/run/unload/capture)    |
|   [2]   | Export                            | OTel SDK 1.15.3 (2026-04) optional; for a single-host dev tool, an OTLP-file or JSON-lines exporter into `.artifacts/rhino/` beats running a collector |
|   [3]   | Runtime-level attach              | EventPipe: `dotnet-counters` / `dotnet-trace` / `dotnet-gcdump` attach by PID to Rhino's CoreCLR via the `$TMPDIR/dotnet-diagnostic-<pid>-*` socket — no code in the plugin at all |
|   [4]   | Crash capture                     | macOS `.ips` reports in `~/Library/Logs/DiagnosticReports/` (JSON since macOS 12) — supervisor snapshots the set pre-run, diffs post-exit; `DOTNET_DbgEnableMiniDump=1` for managed dumps `[NEEDS_LIVE_PROOF]` on this host |
|   [5]   | Visual state                      | Keep in-host `ViewCapture` for viewport truth; ADD supervisor-side `screencapture -l <windowID>` for states the plugin cannot see (crash-recovery dialog, error sheets, beachball) — requires one-time Screen Recording TCC grant |

### EventPipe specifics

- `[NEEDS_LIVE_PROOF]` Attach presumes McNeel did not set `DOTNET_EnableDiagnostics=0` for the bundled runtime. One `dotnet-counters ps` against a running RhinoWIP settles it.
- `DOTNET_DiagnosticPorts` reverse-connect lets the SUPERVISOR listen and Rhino dial out at launch — env injectable because `open --env NAME=VALUE -b <bundle-id>` is supported (`[VERIFIED]` local `man open`). This gives per-session GC/alloc/exception counters as run artifacts with zero plugin code.
- `dotnet-gcdump` is the ALC-leak proof tool: after `Unload()`, a gcdump showing a surviving `LoaderAllocator` names the rooting path. Pairs with §1 pitfall [5] — `bridge verify` could emit `unloadConfirmed` + on failure an automatic gcdump artifact. This closes the loop the current bridge cannot: today an unload leak would be invisible until the host degrades.
- The existing `OnExceptionReport` capture (Transport.cs:231) — the only window into `InvokeOnUiThread`-swallowed faults — must survive any rebuild; EventPipe exception counters complement but do not replace it.

## [5]-[MACOS_HOST_SUPERVISION]

### Verified semantics

- `[VERIFIED]` `NSRunningApplication.terminate()` sends the quit Apple Event; returns `true` when the REQUEST is delivered, not when the app exits; observe `isTerminated`. Sandboxed callers cannot use it (the CLI supervisor is not sandboxed — fine). `forceTerminate()` is the AppKit-level kill.
- Repo-verified (live-host evidence in memory, re-stated as constraint): `RhinoApp.Exit(false)` AND `SIGTERM` both self-SIGABRT via `RhMacSignalHandler` — producing exactly the `.ips` + crash-recovery dialog the operator wants gone. **Signals are not a clean-quit channel for Rhino. The Apple Event is the only one.**
- kqueue `EVFILT_PROC`/`NOTE_EXIT` watches ANY PID (not just children) without polling; exposed in Python via `select.kqueue` — the supervisor needs no helper binary to know the instant Rhino exits. Stable BSD spec (freshness rules satisfied by stability clause).

### Robust supervisor shape (python-side, replaces ad-hoc launch/quit)

| [INDEX] | [PHASE]    | [ACTION]                                                                                                                          |
| :-----: | ---------- | ---------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | Pre-launch | Reconcile crash markers (`.rhl`, autosave doc, `.ips` diff baseline) — pre-empts the recovery dialog instead of clicking through it |
|   [2]   | Launch     | `open -b <bundle-id> --env DOTNET_DiagnosticPorts=...` (env reaches the app; LaunchServices direct-binary exec avoided)             |
|   [3]   | Monitor    | kqueue `NOTE_EXIT` on PID + RPC liveness (StreamJsonRpc heartbeat) — distinguishes "process died" from "UI thread wedged"           |
|   [4]   | Quit       | Apple Event quit (`NSRunningApplication.terminate()` via pyobjc, or `osascript -e 'tell app id "…" to quit'`) -> await `NOTE_EXIT` with deadline |
|   [5]   | Escalate   | `forceTerminate()` -> `SIGKILL` last; then phase [1] reconcile BEFORE the next cold open (kill guarantees markers exist)            |
|   [6]   | Post-exit  | `.ips` diff -> parse JSON -> attach crash thread + exception type as a structured run fact; window screenshot if a dialog survived  |

The host -> supervisor notification channel (§3) upgrades phase [3]: the plugin announces `shutdownStarted` on `RhinoApp.Closing`, letting the supervisor distinguish AE-quit-in-progress from a hang — today's quit logic can only time out blind.

### Modal-dialog watchdog

Crash-recovery and error-ignoring dialogs are detectable out-of-process (AX API / `CGWindowListCopyWindowInfo` window enumeration; TCC Accessibility grant required), but detection is the fallback — marker reconciliation in phase [1] makes the dialog not appear at all, and §1's hot-swap makes most relaunches unnecessary in the first place. Spend effort in that order.

## [6]-[SYNTHESIS] — pain -> pattern

| [INDEX] | [OPERATOR_PAIN]                                  | [PATTERN_THAT_KILLS_IT]                                                             |
| :-----: | ------------------------------------------------ | ------------------------------------------------------------------------------------ |
|   [1]   | Package update forces quit/relaunch + dialogs     | §1 shell/cargo collectible ALC swap; relaunch only for shell + GH2                    |
|   [2]   | Scenario authoring finicky, C# 10 traps           | §2 Option B in-shell Roslyn compile at C# 14 + typed globals + front-matter directives |
|   [3]   | Diagnostics thinner than needed                   | §3 live notification stream + §4 spans, EventPipe attach, gcdump, `.ips` diff, PDB stack traces |
|   [4]   | Forced-quit / crash-recovery churn                | §5 AE-quit-only ladder + kqueue watch + pre-launch marker reconcile                   |
|   [5]   | Linkage direction (bridge wired into python tool) | §3 bidirectional RPC makes a supervisor-centric inversion mechanically possible       |

## [SOURCES]

- .NET unloadability how-to + pitfalls — learn.microsoft.com/dotnet/standard/assembly/unloadability (current); dotnet/runtime `docs/design/features/unloadability.md`
- runtime#124876 (debugger pins collectible ALC, 2025); runtime#44679 (silent unload failure)
- roslyn#41722 / #22219 (CSharpScript assembly accumulation); roslyn PR #74915 (collectible InteractiveAssemblyLoader, status unconfirmed); roslyn#72366 (non-collectible -> collectible reference ban)
- `dotnet/roslyn` main `CSharpScriptCompiler.cs` — `DefaultParseOptions` = `LanguageVersion.Latest` (fetched 2026-06)
- Microsoft.CodeAnalysis.CSharp.Scripting 5.3.0 (NuGet, current)
- .NET 10 GA 2025-11-11 LTS; file-based apps `#:package` (devblogs .NET Conf 2025 recap; visualstudiomagazine 2025-11-17)
- StreamJsonRpc — github.com/microsoft/vs-streamjsonrpc (2.25.25, NuGet current); NerdbankMessagePackFormatter docs
- Nerdbank.MessagePack 1.1.62 (NuGet current); blog.nerdbank.net 2024-11 intro
- gRPC IPC w/ UDS — learn.microsoft.com/aspnet/core/grpc/interprocess-uds (aspnetcore-10.0, updated 2025-05)
- corefx PR #6833 — named pipes over UDS on Unix; .NET 11 `CurrentUserOnly` 0600 hardening note
- EventPipe / dotnet-counters / DOTNET_DiagnosticPorts — learn.microsoft.com/dotnet/core/diagnostics (current)
- OpenTelemetry .NET 1.15.3 (NuGet, 2026-04)
- NSRunningApplication terminate()/forceTerminate() — developer.apple.com AppKit (fetched 2026-06)
- Local bundle inspection 2026-06-10: RhinoWIP .NET 10.0.2 runtime path; RhCore Roslyn `2.25567.12`; `man open` `--env`

## [OPEN_QUESTIONS]

1. `[NEEDS_LIVE_PROOF]` Is EventPipe enabled on RhinoWIP's bundled CoreCLR (`dotnet-counters ps` while running), and does `open --env DOTNET_DiagnosticPorts` actually reach the managed runtime through Rhino's launch path?
2. `[NEEDS_LIVE_PROOF]` Do current Rasm libs unload cleanly from a collectible ALC, or do host-rooted statics (Idle pump, panels, token-gated singletons) pin it? One gcdump probe answers this and sizes the disposal-contract work.
3. Roslyn coexistence: does loading bridge-pinned Roslyn 5.3.0 into a private ALC conflict with RhinoCode's bundled Roslyn under `AssemblyLoadContext.Default` fallback resolution, or is full isolation clean?
4. PR roslyn#74915 shipped state — if collectible scripting landed in 5.3.x, Option A's blocker may be gone; re-check before committing to Option B's globals-replacement design.
5. GH2 remains default-ALC-only (isolated compile of GH2 types fails per repo evidence) — does the cargo-ALC design need a GH2 carve-out where GH2-touching scenarios reference default-ALC GH2 while still living in the cargo ALC (allowed direction), and does that hold under real GH2 object graphs?
