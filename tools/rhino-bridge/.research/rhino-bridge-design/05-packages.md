# [D5] Package Plan — Validated for the In-Host Reality

Design-wave task D5 for the C1 SUPERVISOR rebuild (prior corpus `tools/rhino-bridge/.research/rhino-bridge/10` §4, README §3). Date: 2026-06-10. Every version below was re-verified against the NuGet v3 API (flatcontainer index + nuspec + registration published-dates) and the installed `RhinoWIP.app` 9.0.26153.12416 bundle this session; nothing is quoted from memory. Doctrine inputs: `LIBRARY_DEPTH` and manifest-truth law (`docs/stacks/csharp/README.md` §2/§4), central-package-management rules (`docs/stacks/csharp/platform/build-and-packages.md` §1-§3), wire law (`README.md` §5 BOUNDARIES tail), JSON/system-API ownership (`platform/system-apis.md` §2/§7).

Headline: the rebuild needs exactly ONE new runtime package family in-host (StreamJsonRpc and its closure, isolated in a shell-private ALC), ONE supervisor-side diagnostics library, ONE dev-time tool manifest row, and ZERO scenario-compilation packages. Everything else the design needs is already central, already host-bundled, or already in the .NET 10 shared framework.

## [1]-[DECISION_FRAME]

Placement is the unit of analysis, not the package. The C1 component map (10 §1.2) creates five distinct load environments with different conflict physics:

| [INDEX] | [ENVIRONMENT] | [COMPONENT] | [CONFLICT PHYSICS] |
| :-----: | ------------- | ----------- | ------------------ |
| [1] | Rhino default ALC | `.rhp` stub only | shares resolution with RhinoCommon, GH2, Eto, host Newtonsoft/Roslyn, and every operator-installed plugin (RhinoMCP class); anything placed here can collide with anything, forever |
| [2] | shell-private ALC (non-collectible) | `Rasm.Bridge.Shell` + `Rasm.Bridge.Contract` + RPC closure | isolated by construction; collides with nothing; loads once per Rhino session |
| [3] | cargo ALC (collectible) | `Rasm.Bridge.Cargo` + scenario kit + libs-under-test + per-swap LanguageExt/Thinktecture copies | swapped per session; per-ALC copies make dependency bumps hot (07 §1 pitfall 7) |
| [4] | workstation exe | `Rasm.Bridge.Supervisor` | ordinary .NET 10 process; zero in-host constraints |
| [5] | dev-time tool manifest | `.config/dotnet-tools.json` | never loads in any process the bridge owns |

Two operator directives bind the plan directly. NO PINNING: the shell must survive monthly RhinoWIP churn, so environment [1] must hold zero packages — the stub cannot break when the host's bundled assemblies move, because it references none of them beyond RhinoCommon itself. NO FRAGILE LOGIC: every package must be load-bearing for a complete trigger→behavior→evidence story; "restored but never loaded" is acceptable only when the nuspec forces it (see §3.2), never as a chosen capability.

## [2]-[HOST_BUNDLE_CONFLICT_SURFACE] — what RhinoWIP actually ships

Full-bundle `find` over `/Applications/RhinoWIP.app` (9.0.26153.12416), executed 2026-06-10:

| [ASSEMBLY FAMILY] | [IN BUNDLE?] | [WHERE / VERSION] | [CONSEQUENCE] |
| ----------------- | :----------: | ----------------- | ------------- |
| `Microsoft.NETCore.App` shared framework | YES | `RhCore.framework/.../dotnet/arm64/shared/Microsoft.NETCore.App/10.0.2/` incl. `System.Text.Json.dll`, `System.IO.Pipelines.dll` | STJ and Pipelines are framework-owned on net10 — no package ships for either |
| `Newtonsoft.Json.dll` | YES | `RhCore.framework/Versions/A/Resources/`, FileVersion `13.0.4.30916` | the one real overlap with the RPC closure; same version family as the NuGet pin (§3.2) |
| `Microsoft.CodeAnalysis.*` (full Roslyn, C# + VB + Scripting) | YES | RhCore Resources, 5.x daily | irrelevant to C1 — no Roslyn ships in-host (10 §1.2); becomes relevant only for the deferred C3 cargo REPL lane |
| `Microsoft.Bcl.AsyncInterfaces.dll`, `Microsoft.Bcl.Cryptography.dll` | YES | RhCore Resources | no-op on net10 targets (netstandard shim) |
| `StreamJsonRpc.dll` | NO | — | zero hits bundle-wide |
| `Nerdbank.Streams.dll` | NO | — | zero hits |
| `MessagePack.dll` | NO | — | zero hits |
| `Microsoft.VisualStudio.Threading.dll` / `.Validation.dll` | NO | — | zero hits |
| `Microsoft.Diagnostics.*` | NO | — | zero hits |

Operator-installed yak packages (`~/Library/Application Support/McNeel/Rhinoceros/packages/9.0/`): only `rasm-bridge` today. RhinoMCP (05 §5) is the live hazard — McNeel's own automation plugin, alpha, monthly-moving, installable at any time, loading into the default ALC with whatever closure it chooses.

The conflict verdict: the RPC closure has NO collision in the bundle TODAY, but the design must not depend on that absence persisting — a WIP service release or a RhinoMCP install can introduce any of these assemblies into default-ALC resolution without notice. That is exactly why corpus 11 §4 G4 demanded the shell-private ALC, and why this plan resolves it structurally (§4) instead of version-matching against a moving host.

## [3]-[PACKAGE_REGISTER]

### [3.1] StreamJsonRpc — the RPC layer (the only new in-host package)

| [FIELD] | [VALUE] |
| ------- | ------- |
| NuGet id / version | `StreamJsonRpc` **2.25.25** (latest stable, published 2026-06-02) |
| License | MIT (expression, nuspec-verified) |
| Maintenance signal | Microsoft-owned (`microsoft/vs-streamjsonrpc`); release cadence within the last 5 months: 2.24.84 (2026-01-21), 2.24.92 (2026-05-13), 2.25.25 (2026-06-02) |
| Direct consumers | `Rasm.Bridge.Contract`, `Rasm.Bridge.Shell`, `Rasm.Bridge.Supervisor` |
| Placement | shell-private ALC (in-host); ordinary process (supervisor) |
| What it buys (07 §3, 09 §2.2) | bidirectional notifications (`IBridgeEvents` fact/progress/`shutdownStarted` stream), request cancellation, typed proxies from the Contract interface pair, `IProgress<T>` marshaling — retires the 538-LOC hand-rolled `BridgeWire` envelope/dispatch |
| Source-gen surface | analyzers ship IN-PACKAGE (`analyzers/cs/StreamJsonRpc.Analyzers.dll` — verified in the 2.25.25 nupkg): `JsonRpcContractAttribute` on partial interfaces drives the `ProxyGenerator` source generator (verified present in the shipped assemblies). Contract interfaces get compile-time proxies — no runtime emit, no extra analyzer package |
| Formatter decision | `SystemTextJsonFormatter` (type name verified in the 2.25.25 assembly) over the existing UDS-backed named pipe. STJ comes from the shared framework; the wire stays human-debuggable JSON (07 §3 sizing note). The Contract DTO family gets one `JsonSerializerContext` (source-gen, `platform/system-apis.md` §2 JSON owner) — serialization stays protocol-shaped at the edge per the wire law |

Dependency closure on net10.0 (NuGet selects the net9.0 group; nuspec-verified):

| [PACKAGE] | [NUSPEC FLOOR] | [CENTRAL PIN] | [LOADS AT RUNTIME?] |
| --------- | -------------- | ------------- | :-----------------: |
| `Nerdbank.Streams` | 2.13.16 | **2.13.16** (latest stable, 2025-08-31) | YES — `IDuplexPipe`/stream plumbing |
| `Microsoft.VisualStudio.Threading.Only` | 17.14.15 | **17.14.15** (latest stable, 2025-05-14) | YES — JoinableTaskFactory internals |
| `Microsoft.VisualStudio.Validation` | 17.13.22 | **17.13.22** (latest stable) | YES — argument validation |
| `Newtonsoft.Json` | 13.0.3 | **13.0.4** (latest stable) | NO under `SystemTextJsonFormatter` — only `JsonMessageFormatter` touches it |
| `MessagePack` (+ `MessagePack.Annotations`) | 2.5.198 | **2.5.198** (pin the floor — see below) | NO — only the legacy `MessagePackFormatter` touches it |
| `Nerdbank.MessagePack` (+ `PolyType`) | 1.2.4 / 1.3.1 | **1.2.4** / **1.3.1** | NO — only `NerdbankMessagePackFormatter` touches it |
| `Microsoft.NET.StringTools` | 17.6.3 (via MessagePack) / 18.4.0 (via Nerdbank.MessagePack) | **18.4.0** (unify at the higher floor) | NO — rides the unused formatter branches |
| `System.IO.Pipelines` | 8.0.0 (via Nerdbank.Streams net8 group) | **10.0.9** (repo System.* family) | framework copy wins on net10; the bundle ships it inside `Microsoft.NETCore.App/10.0.2` (§2) — the package node exists only to keep the lock graph coherent |

Closure size honestly stated: 1 package loaded nowhere (`.rhp` stub), 5 assemblies actually mapped in the shell ALC (`StreamJsonRpc`, `Nerdbank.Streams`, `Microsoft.VisualStudio.Threading`, `Microsoft.VisualStudio.Validation`, plus `Rasm.Bridge.Contract`), 5 more restored-but-never-loaded because the nuspec hard-depends on all three formatter backends (assemblies load lazily; choosing `SystemTextJsonFormatter` means the Newtonsoft/MessagePack/Nerdbank.MessagePack branches never JIT). Pin `MessagePack` at its 2.5.198 floor deliberately: lifting it to the 3.1.7 line would version-bump a never-loaded assembly against a formatter binary-bound to the 2.x API — risk with zero capability.

Version-coherence dividend: the repo already pins `Microsoft.VisualStudio.Threading.Analyzers` **17.14.15** globally (`Directory.Packages.props:73`); the `.Only` runtime library lands at the identical version — one VS-Threading family, analyzers and runtime in lockstep, no new version axis.

### [3.2] Newtonsoft.Json — the one host overlap, neutralized twice

The bundle ships `Newtonsoft.Json` 13.0.4.30916 in default-ALC-reachable RhCore Resources (§2); the StreamJsonRpc nuspec forces a Newtonsoft node into the graph. The collision is neutralized by two independent layers: (a) the shell-private ALC means the bridge's copy and the host's copy can never unify in one resolution scope; (b) even in the worst case — a future stub-level leak into default-ALC resolution — the central pin 13.0.4 is the SAME version family the host ships. Layer (a) is the design guarantee; layer (b) is free insurance. Never select `JsonMessageFormatter`, which would make a never-loaded assembly load-bearing.

### [3.3] Microsoft.Diagnostics.NETCore.Client — supervisor-side EventPipe

| [FIELD] | [VALUE] |
| ------- | ------- |
| NuGet id / version | `Microsoft.Diagnostics.NETCore.Client` **0.2.661903** (latest stable, published 2026-01-06) |
| License | MIT |
| Maintenance | `dotnet/diagnostics` repo; versioned with the diagnostics-tools train (same 661903 build as the 9.0.x tools) |
| Consumer | `Rasm.Bridge.Supervisor` ONLY — never in-host, so zero conflict analysis applies |
| Closure | `Microsoft.Extensions.Logging.Abstractions` ≥8.0.3 — already central at **10.0.9**; transitive pinning satisfies it with no new node |
| Story (trigger→behavior→evidence) | session start → `DiagnosticsClient(pid)` EventPipe counter session against Rhino's CoreCLR via the `$TMPDIR/dotnet-diagnostic-<pid>-*` socket → GC/alloc/exception counters land as per-session artifacts in the result envelope (10 §1.2 diagnostics row). Gated on Phase-0 probe (c) (`DOTNET_EnableDiagnostics` not disabled — 07 §4); if the probe fails, this package is DROPPED, not shipped dormant |

### [3.4] dotnet-gcdump — the unload-leak proof tool (dev-time manifest)

| [FIELD] | [VALUE] |
| ------- | ------- |
| Tool id / version | `dotnet-gcdump` **9.0.661903** (latest stable, 2026-01-06), local tool in `.config/dotnet-tools.json` beside `dotnet-outdated-tool`/`dotnet-stryker`/`ilspycmd` |
| License | MIT |
| Story | cargo `Unload()` → `WeakReference` GC-loop fails to confirm within budget → supervisor shells `dotnet tool run dotnet-gcdump -- collect -p <rhino-pid>` → `.gcdump` artifact attached to the `unloadConfirmed:false` fact (07 §1 pitfall 5 + §4; 10 §1.2). This is the automatic behavior the AGENT-FIRST directive demands: a leak names its own rooting path without anyone asking |
| Companion | `dotnet-counters` **9.0.661903** earns a manifest row only for the Phase-0 probe (`dotnet-counters ps` settles EventPipe enablement) and live triage; if the build wave prefers, it can run one-shot via the .NET 10 SDK's `dotnet tool exec` instead of a manifest entry — decide there, both are dev-time-only either way |

### [3.5] Already-central packages the bridge reuses (no admission needed)

| [PACKAGE] | [CENTRAL VERSION] | [BRIDGE USE] |
| --------- | ----------------- | ------------ |
| `System.IO.Hashing` | 10.0.9 | supervisor content-hash staging (`XxHash3` per `system-apis.md` §7 INTEGRITY — replaces the current SHA256-for-cache-keys shape in `refs/<hash>/` staging) |
| `LanguageExt.Core` / `Thinktecture.Runtime.Extensions` | 5.0.0-beta-77 / 10.2.0 | cargo + supervisor + scenario projects via the existing `UseWorkspaceLibraries` injection; in-host these load CARGO-SIDE ONLY as per-swap copies (07 §1 pitfall 7) — which is precisely what makes a `Directory.Packages.props` bump hot-swappable, and what kills the F11 `LanguageExtBootstrap` warmup hack (typeclass caches die with the ALC instead of leaking across swaps) |
| `Microsoft.Testing.Platform` (+ MSBuild/Telemetry/Trx) | 2.2.3 | NOT referenced by any bridge project in v1. The session API is MTP-SHAPED (enumerable scenario refs, per-scenario typed results, stream-friendly — 10 §1.3 verdict) so the phase-5 C2 adapter bolts on with zero substrate change; the central rows already exist for the test graph, so even phase 5 adds no `Directory.Packages.props` edit |
| Host assemblies (RhinoCommon, GH2, Eto, Microsoft.macOS) | host-bundled, `Directory.Build.props` HintPaths, `Private=false` | unchanged — shell references RhinoCommon only; GH2 stays default-ALC preloaded (D12 port-verbatim) |

### [3.6] Scenario compilation — zero packages, by design

Scenarios are ordinary repo C# 14 in per-lib `*.Scenarios.csproj` (10 §1.2): `ProjectReference` to the owning lib + testkit, workspace globals injected by `Directory.Build.props`, host refs via the existing HintPath infrastructure, compiled by the repo toolchain with analyzers/IVT/PDBs. No xunit reference (scenarios are `[RhinoScenario]` static entrypoints, not test-framework tests — the 08 §4[5] discovery anti-pattern stays structurally impossible). No MTP reference (§3.5). No Roslyn reference (no in-host compile in C1). The entire "scenario-assembly compile/test package" category is EMPTY, and that emptiness is the design: every package that would have lived here is what made the old dialect (04 FM5) possible.

### [3.7] macOS process lifecycle — zero packages, with one priced gap

The supervisor's host-lifecycle spine needs no NuGet at all: Apple-Event quit rides the port-verbatim `osascript` JXA ladder (09 §4 port-verbatim list), launch rides `open --env` (07 §5), `.ips` forensics is JSON file reading (framework STJ). The one gap corpus 11 §4 G3 priced: kqueue `NOTE_EXIT` has no BCL surface and no acceptable package — `Mono.Unix`'s last release predates the freshness window and does not wrap `kevent`; `Tmds.*` is Linux-epoll-shaped. Resolution: ~50-100 LOC of `kqueue`/`kevent` P/Invoke inside the supervisor (a named boundary capsule per the boundaries law), with 250 ms PID polling as the degraded fallback. A package would be a thin wrapper over two syscalls — exactly what `LIBRARY_DEPTH`'s wrapper rejection forbids in reverse.

## [4]-[ALC_PLACEMENT_MAP] — resolving 11 §4 G4 / operator question 6

The prior corpus left "shell-private ALC for the RPC closure" as a one-paragraph design rule. Resolved shape:

| [LOAD CONTEXT] | [CONTENTS] | [PACKAGES] |
| -------------- | ---------- | ---------- |
| default ALC | `RasmBridge.rhp` stub: plugin registration, one `AssemblyDependencyResolver`-backed custom ALC construction, one `Shell.Start()` activation call | ZERO. References RhinoCommon (host, `Private=false`) and nothing else. This is what makes the shell layer immune to host bundle churn AND to other plugins' closures — the NO-PINNING requirement met structurally |
| shell-private ALC (custom, non-collectible, one per Rhino session) | `Rasm.Bridge.Shell.dll`, `Rasm.Bridge.Contract.dll`, StreamJsonRpc + the 4 runtime closure assemblies (§3.1), the 5 never-loaded formatter-branch assemblies staged for resolver completeness | the full §3.1 register |
| cargo ALC (collectible, swapped per session) | `Rasm.Bridge.Cargo.dll`, scenario assemblies, libs-under-test, per-swap LanguageExt/Thinktecture copies | already-central packages only (§3.5) |
| supervisor process | `Rasm.Bridge.Supervisor` | StreamJsonRpc closure + `Microsoft.Diagnostics.NETCore.Client` + `System.IO.Hashing` |

Three laws make this hold:

1. **Type-identity law**: exactly one copy of `Rasm.Bridge.Contract` + StreamJsonRpc may exist host-side. The cargo ALC's `Load` override delegates `Rasm.Bridge.Contract`, `StreamJsonRpc`, and closure names to the shell ALC instance (parent-first for bridge-owned names, cargo-first for everything else). Collectible→non-collectible reference direction is the allowed one (07 §1 reference-direction law).
2. **Contract placement correction to 11 §4 G4**: the corpus sketch put "stub + Contract DTOs" in the default context. Wrong half: Contract carries `[JsonRpcContract]` attributes referencing StreamJsonRpc types, so default-ALC placement would drag RPC metadata resolution into the host context — the exact leak the private ALC exists to prevent. Contract lives in the shell ALC; the stub is dependency-zero and contains no bridge types at all. Nothing in the design requires the default ALC to see a single Contract type: Rhino calls the stub, the stub calls one entry method resolved THROUGH the shell ALC.
3. **No-LanguageExt-in-shell law**: Shell and Contract carry no LanguageExt/Thinktecture (set `UseWorkspaceLibraries=false`). Today's protocol project references them unused so the `.rhp` can ship dependency-free (01 §1) — the rebuild makes the posture explicit instead of accidental. Fin rails begin cargo-side and supervisor-side; the wire carries DTOs, version negotiation, and the ported `PhaseStatus` algebra, all plain C#. This is a deliberate, argued doctrine deviation: the frozen-by-design shell minimizes its closure below the repo's functional-core default because every assembly in the shell ALC is a relaunch-priced dependency (host law, 05 §6).

Transport note with package consequence: the existing `NamedPipeServerStream` is already a UDS on macOS (07 §3) — no socket package, no Kestrel, no transport change. Set `CurrentUserOnly` now; .NET 11 hardens the backing socket file to 0600 (07 §3), and the option is free today.

## [5]-[WIRING] — exact central-package + csproj shape

`Directory.Packages.props` — one new labeled group (CPM rules: versions ONLY here; `CentralPackageTransitivePinningEnabled=true` means every closure entry below actively pins the transitive graph, which is why the never-loaded formatter branch is pinned explicitly rather than left floating):

```xml
<ItemGroup Label="Bridge">
    <!-- direct -->
    <PackageVersion Include="StreamJsonRpc" Version="2.25.25" />
    <PackageVersion Include="Microsoft.Diagnostics.NETCore.Client" Version="0.2.661903" />
    <!-- StreamJsonRpc closure: central-reserved, transitively pinned (MathNet-closure precedent,
         build-and-packages.md §5). MessagePack family pinned at the nuspec floor: never loaded
         under SystemTextJsonFormatter; lifting it buys nothing and unpins a binary-bound branch. -->
    <PackageVersion Include="Nerdbank.Streams" Version="2.13.16" />
    <PackageVersion Include="Microsoft.VisualStudio.Threading.Only" Version="17.14.15" />
    <PackageVersion Include="Microsoft.VisualStudio.Validation" Version="17.13.22" />
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.4" />
    <PackageVersion Include="MessagePack" Version="2.5.198" />
    <PackageVersion Include="MessagePack.Annotations" Version="2.5.198" />
    <PackageVersion Include="Nerdbank.MessagePack" Version="1.2.4" />
    <PackageVersion Include="PolyType" Version="1.3.1" />
    <PackageVersion Include="Microsoft.NET.StringTools" Version="18.4.0" />
    <PackageVersion Include="System.IO.Pipelines" Version="10.0.9" />
</ItemGroup>
```

Per-project references (versionless under CPM; lock files regenerate per project because `RestorePackagesWithLockFile=true` is repo-global — after the central edit, expect NU1004 until `dotnet restore --force-evaluate`):

| [PROJECT] | [PACKAGE REFERENCES] | [PROJECT REFERENCES] | [POSTURE] |
| --------- | -------------------- | -------------------- | --------- |
| `Rasm.Bridge.Contract` | `StreamJsonRpc` (for `[JsonRpcContract]` + DTO attributes; in-package analyzers arrive with it) | — | `UseWorkspaceLibraries=false`; STJ `JsonSerializerContext` for the DTO family; frozen after v1 |
| `Rasm.Bridge.Shell` (`.rhp` = stub + shell assembly, one project, two-assembly output or stub-as-second-tiny-csproj — build-wave choice) | `StreamJsonRpc` | Contract | `UseWorkspaceLibraries=false`; RhinoCommon via existing HintPath, `Private=false`; publishes the shell-ALC closure as its deploy set |
| `Rasm.Bridge.Cargo` | — (workspace globals flow) | Contract, `Rasm.TestKit` | collectible-ALC resident; LanguageExt/Thinktecture arrive as per-swap copies via normal restore output |
| `Rasm.Bridge.Supervisor` | `StreamJsonRpc`, `Microsoft.Diagnostics.NETCore.Client`, `System.IO.Hashing` | Contract | plain exe; workspace globals flow (Fin rails for the session state machine) |
| `*.Scenarios.csproj` (per lib) | — | owning lib, `Rasm.TestKit` | ordinary repo project; analyzers + IVT + PDBs by default |

`.config/dotnet-tools.json` — add:

```json
"dotnet-gcdump":   { "version": "9.0.661903", "commands": ["dotnet-gcdump"] },
"dotnet-counters": { "version": "9.0.661903", "commands": ["dotnet-counters"] }
```

Graph-proof route per the repo's own rules: this central edit is a trigger-file change → `uv run python -m tools.quality static full` (or the `.archive/quality` fallback path if the rail has moved) at the build wave's first commit.

## [6]-[REJECTED] — considered and cut

| [PACKAGE / FAMILY] | [ROLE CONSIDERED] | [WHY CUT] |
| ------------------ | ----------------- | --------- |
| `Grpc.AspNetCore` / Kestrel-over-UDS | RPC layer | requires the ASP.NET Core shared framework, absent from Rhino's bundled runtime; self-contained Kestrel inside a plugin is large and collision-prone (07 §3 — UNFIT plugin-side) |
| `Nerdbank.MessagePack` as ACTIVE formatter (vs restored-only) | binary wire payloads | control-plane messages are JSON-sized; captures/evidence are disk-spooled JSONL + PNG artifacts by design (10 §1.2), so nothing big rides the wire; JSON keeps the wire debuggable (07 §3 sizing note). The package stays in the graph solely because the nuspec demands it |
| `Newtonsoft.Json` via `JsonMessageFormatter` | default formatter | would make the one host-overlapping assembly load-bearing; STJ is framework-owned and source-generated (§3.2) |
| `Microsoft.CodeAnalysis.CSharp` / `.Scripting` in-host | scenario compile | C1's defining property is NO Roslyn in the host (10 §1.2 — the dialect dies by moving compilation to the repo toolchain, not by re-hosting a compiler); CSharpScript additionally leaks via `InteractiveAssemblyLoader` (07 §2 Option A blocker). Deferred to the C3 cargo REPL lane, in cargo, post-v1 |
| `Microsoft.Testing.Platform` (+ adapter packages) as v1 dependency | test front end | two unknowns at once (session core + MTP discovery protocol) was C2's demotion rationale (10 §1.3); the session API is MTP-shaped so phase 5 adds the adapter with zero substrate change; rows already central |
| `xunit.v3.*` in scenario projects | scenario authoring | scenarios are not xunit tests; framework discovery of host-native types is the documented Rhino.Testing failure class (08 §4[5]) |
| `Rhino.Testing` 9.0.8-beta | host harness | Windows-only by construction (`Rhino.Inside` dep, `net*-windows` TFMs — 05 §3); bookmark for a future Windows/Linux CI lane |
| `OpenTelemetry` SDK + exporters | diagnostics export | single-host dev tool; BCL `ActivitySource`/`Meter` are zero-dependency and the evidence rail is the bridge's own typed fact stream + JSONL spool (07 §4 layer 2 — a collector is strictly worse here) |
| `Microsoft.Extensions.Hosting` / DI container | supervisor composition | the supervisor is one state machine with policy values, not a service graph; Generic Host imports config/DI/logging ceremony the doctrine rejects (`POLICY_VALUES`, `ONE_HOP_RESOLUTION`) |
| `System.CommandLine` | supervisor argv | the repo's `[Union]`-verb parse pattern (today's `ClientVerb`) covers a closed verb set with zero dependencies; port it |
| `Polly` | retry/backoff | LanguageExt `Schedule` owns retry policy repo-wide (`LIBRARY_DEPTH`; rails-and-effects §5 SCHEDULE_POLICY) |
| `Mono.Unix` / `Tmds.LinuxAsync` | kqueue NOTE_EXIT | stale or wrong-kernel; neither wraps `kevent` on macOS; 50-100 LOC P/Invoke capsule instead (§3.7, 11 §4 G3) |
| `Microsoft.Diagnostics.Tracing.TraceEvent` | nettrace/gcdump parsing | heavyweight, Windows-leaning surface; the supervisor only needs to TRIGGER collection (MDNC) and ATTACH artifacts (`dotnet-gcdump` tool owns the format) — parsing dumps is a human/agent activity on the artifact, not a runtime concern |
| `protobuf-net` / `Google.Protobuf` | wire contract | no `.proto` boundary exists; Contract DTOs + STJ source-gen own the wire; gRPC already rejected above |
| `Serilog` / `Microsoft.Extensions.Logging` (runtime) | bridge logging | the bridge's observability IS the typed fact/phase stream + JSONL spool; a logging framework would be a second, weaker evidence rail (`docs/stacks/csharp/testing/README.md` §4 — one rail per concern). `M.E.L.Abstractions` stays central for AppHost, untouched |
| `H.Pipes` and named-pipe helper libraries | transport sugar | `NamedPipeServerStream` is already the UDS transport, and StreamJsonRpc consumes the raw stream directly — a helper would be a wrapper between two surfaces that already compose |
| SQLite/`Microsoft.Data.Sqlite` for the evidence spool | crash-durable evidence | append-only JSONL files per scenario are crash-durable by construction, diffable, and zero-dependency; a database adds a writer lifecycle exactly where crash-durability matters most (08 §3 lesson 3) |
| `rhino3dm` | out-of-host geometry checks | geometry-only ceiling, no doc/UI/GH2 surface (05 §4); the bridge exists precisely for the surface rhino3dm cannot reach |

## [7]-[OPEN_QUESTIONS] — for the build wave

1. **Deploy-set trim probe (Phase 1)**: ship the full restored closure into the shell ALC staging dir, or trim to the 5 actually-loaded assemblies? One session with `AssemblyLoadContext.Default.Resolving` logging under `SystemTextJsonFormatter` settles the never-loads claim empirically; trim only with that evidence (resolver-missing failures are silent-by-default — 07 §1 pitfall 5 territory).
2. **Source-gen proxy fidelity**: `[JsonRpcContract]` + `ProxyGenerator` are verified present in 2.25.25, but the generated-proxy path under net10/C#14 with the repo's analyzer set needs one compile proof before the Contract interface shape freezes (fallback is runtime `Attach<T>` proxies — same interfaces, zero wiring change).
3. **EventPipe gate**: Phase-0 probe (c) (`dotnet-counters ps` against live RhinoWIP) decides `Microsoft.Diagnostics.NETCore.Client` — confirmed → supervisor counters land as session artifacts; refused (`DOTNET_EnableDiagnostics=0`) → drop the package and the manifest `dotnet-counters` row, keep `dotnet-gcdump` only if the diagnostic socket exists at all (same gate).
4. **StreamJsonRpc minor-version skew across the pipe**: supervisor and shell both reference 2.25.25 at build; after a central bump the shell keeps running the old version until its rare redeploy. JSON-RPC 2.0 framing is version-stable so this should be a non-event, but the hello/capability exchange (D9 successor) should carry the StreamJsonRpc assembly version as a fact so a wire-level incompatibility ever observed is attributable in one read.
