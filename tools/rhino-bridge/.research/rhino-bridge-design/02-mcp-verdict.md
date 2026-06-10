# [02] McNeel Rhino MCP Platform — Build, Integrate, or Ignore

The operator's question: "are we re-creating it for no reason, or do we benefit from integrating it too?" Research date 2026-06-10/11. Method: full source read of `github.com/mcneel/RhinoMCP` at plugin-v0.1.5 (pushed 2026-06-05) via the GitHub API — plugin server, router, tools, cc-plugin, manifests — plus official docs site, announcement thread (discourse 216568), NuGet, and the prior corpus baseline (05 §5, 08 §1/§2.4). No live Rhino contact.

[VERDICT] — **(b) COMPLEMENT, in a precisely scoped two-lane form.** Riding their in-host server as our substrate (a) re-imports three of the four rebuild-only failure generators (D4 dialect, D5 one-shot wire, D6 string evidence — 09 §1) and regresses the one lifecycle decision the corpus marked [SOUND] (D3 quit ladder). Ignoring it (c) is also wrong: their plugin auto-starts an HTTP listener in *every* Rhino once yak-installed — including the rebuild's verification host — so coexistence must be designed regardless (11 G4 already names this), and ignoring forfeits a zero-cost ad-hoc agent lane plus an MIT-licensed reference implementation of crash forensics we are about to build. We are NOT re-creating their platform: theirs is conversational modeling for agents; ours is typed verification for agents. The concerns are disjoint; the host is shared.

## [1]-[GROUND_TRUTH] — what the platform actually is (source-verified)

Four components, MIT license, authors "Callum Sykes, Dan Cascaval, Claude":

| [COMPONENT] | [WHAT] | [KEY FACTS] |
| ----------- | ------ | ----------- |
| `rhino/plugin` (`.rhp`, yak `Rhino-MCP-Platform`) | In-host MCP server | net8.0, C# 12; Kestrel bundled INTO Rhino (per-OS ASP.NET Core runtime payload); one POST endpoint speaking hand-rolled MCP JSON-RPC 2.0; explicitly NO Streamable-HTTP/SSE — "the plugin only exposes request/response tools" (`McpEndpoint.cs`); in-house clones of the `ModelContextProtocol.*` attributes to keep the SDK out of Rhino's default ALC; `AtStartup` load; auto-starts a listener per document on `BeginOpenDocument` unless `RHINO_MCP_AUTOSTART_PORT` is set (`Plugin.cs`); ports allocated from 10500 |
| `rhino/router` (exe) | Out-of-host stdio MCP server | Official `ModelContextProtocol` SDK + `WithStdioServerTransport`; NativeAOT on mac (Apple Silicon only, no Intel binary); stateless process per agent, all state in SQLite (`SlotStore`) so concurrent agents do not race; proxies tool calls to the in-host Kestrel endpoint over localhost HTTP; tool proxies are build-time codegen from the plugin source tree |
| `connector` (`.mcpb`) | Claude Desktop one-click installer | Installs the yak plugin for the user |
| `cc-plugin` | Claude Code plugin | 8 agents (rhino-modeller, grasshopper-scripter, …), 5 commands (`/launch-rhino`, `/scene`, `/snapshot`, …), 2 skills; `.mcp.json` → router via stdio |

**It is NOT `rhinocode_remotepipe`.** The host-side server is the plugin's own Kestrel listener; the only RhinoCode touchpoint is that `run_python`/`run_csharp` write a temp file and shell `-ScriptEditor _Run "<tmp>"` via `RhinoApp.RunScript`, then scrape `CapturedCommandWindowStrings` (see §2).

**Tool inventory** (38 `[McpServerTool]`/`[McpServerResource]` sites, per their own source comment):

- Rhino (15): `run_python`, `run_csharp`, `run_command`, `get_viewport_image` (JPG, default 480×270, max 1280×720, "increase sparingly" — token economy by design; returns a camera/bounds/object-count metadata block to diagnose empty captures), `set_camera`, `list_objects`, `get_selection`/`set_selection`, `open_doc`/`save_doc`/`close_doc`, `get_commands`, `set_layer_material`, `zoom_to_layer`/`zoom_to_object`.
- GH1 (11) and GH2 (11), parallel families: `g1_`/`g2_` `start`, `solve_canvas`, `place_component`, `place_slider`, `connect`, `connect_many`, `apply_graph`, `get_canvas_graph`, `search_components`, `describe_component`, `clear_canvas`.
- Resources (3): command help, host environment, installed plugins.
- Router-side: slot management (launch/list/close Rhino instances, Rhino 8 AND 9 simultaneously), crash-report surfacing, plus the plugin-internal `_router_spawn_listener` (creates a new `RhinoDoc` + dedicated listener per slot — on mac, slots are documents inside ONE Rhino process, each with its own port).

**Session/state model**: doc-scoped — each listener binds one `RhinoDoc` via DI; scripts get `__rhino_doc__` injected by ScriptEditor. Slots persist across agent restarts (SQLite); user-started Rhinos announce via drop-files (`.../McNeel/rhino-mcp/listeners/{pid}-{port}.json`) and are ADOPTED — adopted slots are never killed (`AdoptedSlotCloseException`); router-owned slots are torn down on agent disconnect via `Process.Kill(entireProcessTree: true)` — no clean-quit ladder, no marker hygiene; crash cause is then reconstructed post-hoc by `RhinoCrashReportFinder` (552 LOC: `.ips` parse on macOS, `RhinoDotNetCrash.txt`/WER minidump on Windows, 5-minute fuzzy window, 12-frame extraction) "so we can surface the actual crash cause instead of just 'it died'".

**Extensibility**: none at runtime. `ToolRegistry.Scan` reflects over exactly one assembly (the plugin's own); router proxies are compile-time codegen scanning `/plugin/Tools/`; internal tools are excluded by folder convention. Third parties extend by forking. No registration API exists, none is on the stated roadmap.

**Maturity/cadence**: repo created 2026-04-25; announced 2026-05-07; 0.1.0 → 0.1.5 in ten days (2026-05-27 → 2026-06-05), ~350 commits, 13 open issues. Staff posture: "quite barebones", 0.2 in development, Anthropic connector-marketplace submission planned. Version posture: exact RhinoCommon pins per release (8.29.26063 / 9.0.26132-wip) — they absorb monthly WIP churn by re-releasing the plugin; riding them adds their release lag to ours.

## [2]-[WHAT_IT_DOES_NOT_HAVE] — against the rebuild's requirements

| [REQUIREMENT] (10 §1.2) | [THEIR PLATFORM] |
| ----------------------- | ---------------- |
| Typed evidence rail (facts, captures, status algebra, crash-durable spool) | Absent. `run_csharp`/`run_python` return `{stdout, error}` where `error` is found by string-scanning command-window lines for `"error CS"` / `"Exception:"` / `"Traceback"` — the exact FM6/D6 stringly-evidence pattern the failure dossier condemns, with less structure than even the current bridge's diagnostics phase |
| Repo-compiled C# 14 scenario assemblies (analyzers, IVT, PDBs) | Absent and unreachable. The only C# execution path is ScriptEditor — the same RhinoCode dialect pin as FM5/D4. No assembly-loading surface exists at all |
| Collectible-ALC cargo hot-swap | Absent. Their `.rhp` is `AtStartup` in the default ALC; every plugin release rides the same quit→yak→relaunch gauntlet as FM3 (six releases in ten days, each a relaunch) |
| Clean host lifecycle (AE-terminate ladder, marker reconcile) | Regression. Owned slots die by `Kill(entireProcessTree)` — manufacturing the crash-recovery state our D3 ladder exists to avoid, then paving over it with post-hoc crash reports |
| Streaming/progress/heartbeat during execution | Absent. Request/response only; no SSE; a UI-thread hang is an HTTP timeout (FM7 class unchanged). GH2 `solve_canvas` calls `Solution.StartWait()` under their default marshal-to-UI dispatch |
| Version-tolerant frozen contract | Opposite posture: per-release exact host pins |
| Full-resolution golden captures | Deliberately inverted: captures are token-economical and capped at 1280×720 JPG — correct for agent eyes, wrong for evidence |
| GH2 computed-value oracles | Absent. `g2_solve_canvas` returns per-object warning/error message strings only; no `Tree()`/value extraction |

## [3]-[THREE_WAY_VERDICT]

### [3.1] (a) SUBSTRATE — supervisor rides their in-host server: [REJECT]

Scored against the 04 dossier, riding `RhMcp` instead of building Shell/Cargo:

| [FM] | [OUTCOME ON THEIR SUBSTRATE] |
| :--: | ---------------------------- |
| FM1/FM2 | [REGRESS] — kill-based teardown, no ladder, no reconcile; crash-recovery dialogs become routine and are answered with post-hoc reports, not prevention |
| FM3 | [SAME/WORSE] — their default-ALC `.rhp` + their high release cadence; no cargo lane for OUR logic at all |
| FM4 | [PARTIAL] — crash finder names the cause post-hoc (5-min fuzzy window); no heartbeat, misattribution window remains |
| FM5 | [REGRESS] — ScriptEditor dialect plus string-scan error detection; worse than the current bridge's structured `diagnostics` phase |
| FM6 | [REGRESS] — `{stdout, error}` heuristics; no facts, no captures rail, no spool |
| FM7 | [SAME] — one-shot wire, no cancellation, no progress |
| FM8 | [SOLVED] — announce/adopt drop-files + SQLite slots (genuinely good) |
| FM9/FM10 | [SOLVED-IN-THEIR-DOMAIN] — persistent slots, multi-agent concurrency; but no build/assembly story to amortize, so "solved" by not having the capability that created the cost |

Net: it solves the one generator (D7 session ownership) the corpus already designed a better answer for, while re-importing D4 + D5 + D6 and regressing D3. Against the operator directives it fails directly: string-matched error rails are the named "stringiness"; `Stop()`'s disposal probe is commented "A bit messy, but it works" in their own source; exact host pins are the pinning posture the operator rejected. C1 stands.

### [3.2] (b) COMPLEMENT — two lanes, one shared host: [ACCEPT]

The operator's complement framing was "our shell ALSO exposes an MCP surface so agents drive Rhino through the standard protocol." The evidence sharpens that into three sub-decisions:

1. **Adopt their platform as-is for the exploratory lane — zero build cost.** Conversational modeling, viewport-for-agent-eyes, camera control, GH canvas authoring by component search, multi-Rhino fleet, Claude Code agents/skills: McNeel iterates this weekly with dedicated staff and an explicit dev-cycle pitch ("put an assistant in the driver's seat" — their Developers docs). Installing the yak package + cc-plugin when ad-hoc probing is wanted costs nothing and duplicates nothing we build.
2. **Do NOT ship our own MCP facade in v1 — and do not assume it is ever needed.** Our consumers are coding agents WITH shell access; the typed session CLI + one JSON evidence document IS the agent surface (same argument that demoted C2's MTP-from-v1 in 10 §1.3: never ship a second front end and a new substrate together). The session API is already MTP/MCP-shaped (enumerable scenario refs, per-scenario typed results), so a thin supervisor-side stdio MCP facade — official `ModelContextProtocol` SDK, now stable (1.4.0, 2026-06-04), exposing OUR verbs only (session status, run scenario, cargo C# 14 eval lane, fetch evidence, captures as resources) — remains a phase-5+ option that earns its place only when an agent without shell access becomes a real consumer. McNeel's in-host hand-roll of MCP is not a precedent against the SDK: they hand-rolled to keep dependencies out of Rhino's default ALC; our facade lives in the supervisor exe where no such constraint exists (`LIBRARY_DEPTH` applies cleanly).
3. **Crib under MIT instead of rebuilding what they proved.** Direct inputs to our build wave: `RhinoCrashReportFinder` (cross-platform `.ips`/minidump/`RhinoDotNetCrash.txt` parsing — our supervisor's planned `.ips` diff, already implemented and field-tested); the announce/adopt drop-file pattern (vendor-converged validation of our D10 endpoint discovery, plus the adoption semantics our dual-run plan needs); the capture-metadata-block idea (camera/bounds/on-screen-count returned WITH the image to diagnose empty captures without re-shooting — worth folding into our capture facts); the `_New`-not-`_-New` fact (dash-form creates a headless doc with no viewport plumbing); the `RHINO_MCP_AUTOSTART_PORT` suppression hook.

### [3.3] (c) IGNORE: [REJECT]

Two reasons ignoring is not available even if we wanted it. First, the coexistence hazard is unilateral: once `Rhino-MCP-Platform` is installed (one yak install, shared by every Rhino launch on the machine), their plugin auto-starts a listener in OUR verification host and announces it as adoptable — any concurrently-running agent router can then mutate the document mid-scenario. Second, their kill-based slot teardown writes the global crash-recovery markers (`.ips`, autosave, `.rhl`) that our pre-launch reconcile deletes blindly — 04 §5's "could the marker clear ever delete a second Rhino's state" gap stops being hypothetical the day both tools share a machine. Ignoring the platform means inheriting both hazards undesigned.

## [4]-[COEXISTENCE_PROTOCOL] — design inputs for the build wave

| [#] | [RULE] | [MECHANISM] |
| :-: | ------ | ----------- |
| 1 | The verification host must not expose their listener | Supervisor launches Rhino with `RHINO_MCP_AUTOSTART_PORT` set — `Plugin.Register` returns before `StartOrRestart` when the variable is present. Off-label use of a 0.1.x internal: verify per installed release at build time; fallback is the policy "no `Rhino-MCP-Platform` yak on the verification profile" |
| 2 | One lifecycle owner per Rhino instance, ever | Their router only kills slots it spawned; our supervisor's Rhino must never be router-spawned, and our supervisor must never manage a router-spawned Rhino. Extends the 10 §3 exclusive-ownership law from old-tool/new-tool to third-party automation |
| 3 | Marker reconcile becomes instance-scoped | Their unclean kills make global-marker deletion a real cross-tool data hazard: the rebuild's reconcile must match markers by PID/start-time window instead of clearing all three marker classes wholesale (tightens 04 FM2's current mitigation) |
| 4 | Transports are already disjoint | Their plane: localhost TCP from 10500 + `.../McNeel/rhino-mcp/` state. Ours: UDS pipe + `~/.rasm/`. No collision; keep it that way by never binding TCP |

## [5]-[ANSWER] — the operator's exact question

We are not re-creating it. The overlap is the topology (in-host listener + external driver + per-run payloads — the vendor-converged shape 05 §7 already blessed), not the capability: their platform has no typed evidence, no compiled-assembly execution, no hot-swap, no clean lifecycle, no streaming, and inverts our capture and version-tolerance requirements — while owning, permanently and well, the conversational-modeling lane we should never build (their tool catalog, fleet management, consumer connectors, cc-plugin agents). We benefit from integrating it as a PEER, not a substrate: install it for exploratory agent work when wanted, design the four coexistence rules into the supervisor now (one is a genuine new hazard their teardown model creates), port its MIT crash-forensics and discovery patterns into our supervisor, and keep an MCP facade over our session API as a deferred, demand-gated option rather than a v1 surface.

## [6]-[SOURCES]

| [SOURCE] | [DATE] |
| --- | --- |
| github.com/mcneel/RhinoMCP source @ plugin-v0.1.5 (`Plugin.cs`, `McpServer.cs`, `Server/{Attributes,ToolRegistry,McpEndpoint}.cs`, `Tools/**` incl. `RunCSharpTool`/`RunPythonTool`/`GetViewportImageTool`/`GH2_{Solve,Start}Tool`, `Internal/RouterControlTool.cs`, `router/{Program,RhinoManager,RhinoCrashReportFinder,RouterPaths}.cs`, `RhMcp.csproj`, `Directory.Build.props`, `manifest.yml`, `cc-plugin/**`, READMEs) | pushed 2026-06-05; read 2026-06-10/11 |
| github.com/mcneel/RhinoMCP releases/tags (plugin 0.1.0→0.1.5, connector 0.1.0→0.1.3) + repo metadata (MIT, created 2026-04-25, 13 open issues) | 2026-06-10 |
| mcneel.github.io/RhinoMCP/docs/ (landing + Developers section) | current |
| discourse.mcneel.com/t/rhino-mcp-server/216568 (Sykes announcement + replies: "quite barebones", 0.2 WIP, marketplace plan, Intel-mac gap, Claude Desktop config churn) | 2026-05-07 → 2026-06 |
| nuget.org `ModelContextProtocol` 1.4.0 (official C# SDK, stable line since 1.0.0) | 2026-06-04 |
| Prior corpus: 04 (failure dossier), 05 §5 (RhinoMCP baseline), 08 §1/§2.4 (prior-art row), 09 §1/§3 (decision register), 10 §1-§4 (C1), 11 G4 (default-ALC coexistence gap) | 2026-06-10 |
