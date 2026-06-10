# [05] Rhino 9 WIP / macOS Automation Landscape

Research date: 2026-06-10. Scope: external landscape facts that constrain or inform a ground-up re-imagining of `tools/rhino-bridge`. Sources are McNeel official docs, discourse.mcneel.com staff posts, McNeel GitHub/NuGet/yak, plus local inspection of the installed `RhinoWIP.app` 9.0.26153.12416 bundle (no live Rhino contact). Each claim carries source + date.

## [1] Headless Rhino: still no macOS path

| [OPTION] | [PLATFORM] | [STATUS 2026-06] | [SOURCE] |
| --- | --- | --- | --- |
| Rhino.Inside (in-process RhinoCore) | Windows only | No mac support; "We (as in McNeel) will need to adjust how Rhino itself works" (S. Baer) | discourse 173311 (2024-01); Rhino.Inside NuGet 9.0.x-beta TFMs are `net*-windows` |
| compute.rhino3d (headless Rhino server) | Windows; Linux NEW (WIP) | "Rhino.Compute on Linux ... Ubuntu Server 24.04 and AmazonLinux2023", WIP, "do not recommend using this for production" (announced 2026-03-18) | discourse 217111, 216155 (2026-02/03) |
| Headless on macOS | None | "The standard Rhino.Compute server will not run on macOS"; mac Hops workaround is python `ghhops_server` + rhino3dm (geometry-only) | developer.rhino3d.com Hops guide (current) |

- [CONFIRMED] The Rhino 9 era changed the headless story on Linux, not macOS. The headless core demonstrably runs on a non-Windows OS now (Linux compute, 9.x branch), so the "requires core Rhino changes" blocker is being worked through — but no macOS headless announcement, roadmap item, or WIP feature exists as of 2026-06.
- [CONFIRMED] Rhino 9 WIP for Mac is Apple Silicon only; Intel builds discontinued (discourse 207740, 2025).
- [IMPLICATION] The operator's premise holds: on macOS, an in-host bridge inside live `RhinoWIP.app` remains the only way to execute RhinoCommon + GH2 + doc + UI code. Nothing on the 6-12 month horizon replaces it. Linux compute is the first credible future alternative for *doc-level headless CI* (geometry + GH solve, no UI) — worth tracking, not waiting for.

## [2] RhinoCode CLI (`rhinocode`): precedent, not replacement

Surface (verified against both `Rhino 8.app` 8.30.26103 and `RhinoWIP.app` 9.0.26153 bundled binaries, `--help` only):

| [COMMAND] | [CAPABILITY] | [LIMIT] |
| --- | --- | --- |
| `list [--json]` | Enumerate running Rhino instances (PID, ID, doc) | JSON only here, nowhere else |
| `command <CMD>` | Run Rhino command(s) in an instance | No output/exit-code contract documented |
| `script <PATH>` | Run `.py`, `.py2`, `.cs` script in an instance | Single positional arg; no args passing, no stdout capture, no structured results, no artifact channel documented (developer.rhino3d.com advanced-cli guide, current) |
| `project` | Build/publish ScriptEditor projects into Rhino/GH plugins | Authoring pipeline, not execution rail |

- [CONFIRMED] Architecture: the CLI talks to a "script server that is running inside of Rhino"; "For performance reasons, the script server is not started on Rhino launch" — requires manual `StartScriptServer` (developer.rhino3d.com advanced-cli, Rhino >= 8.11).
- [LOCAL FACT] The current WIP build's CLI is broken out of the box: `RhinoCode.dll` targets net8.0 while the WIP bundle ships only .NET 10.0.2 in `RhCore.framework`; it launches only with `DOTNET_ROLL_FORWARD=LatestMajor`. Evidence of low maintenance priority on this surface during the WIP cycle.
- [VERDICT] `rhinocode` cannot replace the bridge: no typed result rail, no fact/capture artifacts, no exit-code taxonomy, no GH2-isolated loading, server off by default, and a currently mis-targeted binary. But it is McNeel's own validation of the bridge's topology — thin out-of-process CLI + persistent script server inside the host, scripts recompiled per run. A rebuilt bridge could optionally use `rhinocode list --json` for instance discovery instead of hand-rolled endpoint files, and should treat "script-server-in-host" as the blessed shape.

## [3] Rhino.Testing and the in-app test runner

- [CONFIRMED] `Rhino.Testing` 9.0.8-beta (NuGet, 2026-04-13) is actively maintained for Rhino 9 but is Windows-only by construction: NUnit + `Rhino.Inside >= 9.0.26084.13070-beta` dependency, TFMs `net48` / `net8.0-windows7.0` / `net9.0-windows7.0` (nuget.org/packages/Rhino.Testing). Offers headless-doc init, plugin + GHA loading, GH-definition assertions via Context Bake. Irrelevant on mac today; the natural choice if a Windows or Linux CI lane ever materializes.
- [CONFIRMED] Rhino 9 WIP ships an in-app Unit & Regression Testing panel (announced 2025-06-10, E. Iran-Nejad, discourse 205713): `test_*` scripts in any ScriptEditor language plus `.gh` files; hierarchical by directory; "Tests are executed synchronously and on the Rhino main (UI) thread"; "Tests pass if they do not throw an exception"; mac screenshots in the announcement. No CLI/CI driver documented yet.
- [IMPLICATION] McNeel's own mac-capable test runner accepts exactly the constraints the bridge lives with (main-UI-thread, in-host, exception-as-failure). It does not emit machine-readable evidence and cannot be driven externally today, so it does not displace `bridge verify` — but a rebuilt bridge should expect this surface to grow a `rhinocode`-driven runner eventually and avoid inventing scenario semantics that conflict with `test_*` conventions.

## [4] compute.rhino3d / rhino3dm: geometry-only ceilings

| [OPTION] | [WHAT IT GIVES] | [CEILING VS BRIDGE] |
| --- | --- | --- |
| rhino3dm (py/js/.NET, cross-platform incl. mac) | openNURBS geometry create/interrogate/store, `.3dm` I/O, "RhinoCommon style" API | No doc runtime, no UI, no compute functions (meshing, intersections, booleans route to a remote compute server) — github.com/mcneel/rhino3dm (current) |
| compute.rhino3d local/remote | Full RhinoCommon SDK over REST on headless Rhino | Windows (Rhino 8) + Linux (Rhino 9 WIP); no mac server; no UI, no viewport capture, no Eto, no display pipeline |

- [VERDICT] Neither covers the repo's actual test surface (RhinoCommon + doc + display/viewport + Eto + GH2 canvas on mac). They are complements (pure-geometry checks could run anywhere via rhino3dm), not bridge substitutes.

## [5] Rhino 9 WIP changes relevant to an in-host bridge

- [RUNTIME] WIP bundles a self-contained .NET that moved within the WIP cycle: a 2025 WIP thread documents `SetDotNetRuntime ... NetCoreVersion=v9` (discourse 200697); the installed 2026-06 build ships exactly `Microsoft.NETCore.App 10.0.2` (local: `RhCore.framework/.../dotnet/arm64/shared`). Repo already targets `net10.0` (`Directory.Build.props:61`) — aligned. Treat host-runtime drift across WIP service releases as a first-class compatibility hazard for the plugin (the broken net8-targeted `rhinocode` CLI inside the same bundle is the cautionary example).
- [THREADING] No relaxation of UI-thread rules announced for Rhino 9; the official in-app test runner is explicitly synchronous-on-main-thread (discourse 205713, 2025-06-10). A rebuilt bridge keeps the UI-thread execution model; GH2 headless-solve limits (idle-driven solver) remain as documented in repo memory.
- [RHINOMCP] Official McNeel "Rhino MCP Platform" (announced 2026-05-07 by C. Sykes, discourse 216568; repo github.com/mcneel/rhinomcp, active through 2026-06-05): yak package `Rhino-MCP-Platform` 0.1.5 (yak API, updated 2026-06-04; authors "Callum Sykes, Dan Cascaval, Claude"); supports Rhino 8 and Rhino 9 WIP (`MCPConnect` command generates connector config; docs cc-plugin page); Windows x64/arm64 + mac Apple Silicon only; executes Python scripts and Rhino commands, captures viewport images "at lower resolution to conserve tokens", camera control, "Access to Grasshopper 1 and 2"; roadmap: "I'll add more capabilities to it in the future". Note the yak naming collision: index entry `rhinomcp` 0.3.1 is the community package by Jingcheng Chen, not McNeel's.
- [SCRIPTING] Rhino 9 WIP scripting investments are real but editor-centric: Python 3.13, matplotlib support, in-editor examples, publish-yak-from-ScriptEditor, C++ SDK for Mac (WIP features index, discourse 196699). "Command line reboot" (discourse 210508) is command-prompt UI unification, not automation.
- [IMPLICATION] RhinoMCP is the most consequential new fact for a re-imagining: McNeel now ships and maintains an in-host automation channel on mac whose capability set (script exec, command exec, viewport capture, GH1/GH2 reach) overlaps the bridge plugin's core. It validates the architecture and offers a concrete benchmark; its evidence model (token-economical captures for agents) is exactly the "beyond parity" direction the operator asked about. It is not sufficient as-is: no typed protocol, no compile-against-repo-assemblies story, no fact/receipt stream, no exit-code policy, alpha-stage (0.1.5).

## [6] Plugin install/update mechanics: relaunch is structural

- [CONFIRMED] Yak: "Rhino will load new packages the next time it starts" and uninstalls register "the next time it starts" (developer.rhino3d.com yak guide, current). The 2026-05-21 "Improved Package Manager" announcement (discourse 219247) covers discovery/search/perf only — no hot-update mechanics.
- [CONFIRMED] Plugins are "not unloadable by design ... if these plug-ins were to be unloaded, Rhino would crash" (developer.rhino3d.com plugin-loading guide); on .NET there is additionally no collectible-ALC story for `.rhp` assemblies. The only reload affordance is GH1's `Instances.ReloadMemoryAssemblies` (COFF-loaded GHA only).
- [CONFIRMED] Load timing is controllable (load-on-demand, `PlugIn.LoadPlugIn` — already used by the repo for GH2), but load-once is irreversible: an updated `.rhp` on disk cannot replace the loaded one within a session, on any platform.
- [IMPLICATION] The package-update relaunch cycle that trips crash-recovery dialogs cannot be engineered away at the plugin layer; it can only be amortized. The landscape-blessed answer (rhinocode script server, RhinoMCP plugin, in-app test runner) is identical: keep the in-host binary a thin, rarely-changing host (pipe/server + compiler + capture), and push all churn into per-run-compiled payloads that never require a relaunch. The current bridge already compiles `.verify.csx` per run; the redesign lever is shrinking what else lives in the `.rhp` and versioning the wire protocol so client and plugin tolerate skew instead of forcing lockstep redeploys.

## [7] Net assessment for the re-imagining

| [QUESTION] | [ANSWER] |
| --- | --- |
| Is in-host-on-mac still forced? | [YES] No mac headless exists or is announced; Linux compute (2026-03) is the only headless platform expansion. |
| Can `rhinocode` CLI replace the bridge? | [NO] No structured output/evidence rail; but reuse its topology and optionally its `list --json` discovery. |
| Does Rhino.Testing help on mac? | [NO] Windows-only via Rhino.Inside; bookmark for a future Windows/Linux CI lane. |
| Do compute/rhino3dm cover the need? | [NO] Geometry-only; no doc/UI/display/GH2-canvas surface. |
| What changed that the redesign must absorb? | .NET 10 host runtime; RhinoMCP as official in-host automation peer; in-app `test_*` runner conventions; Apple-Silicon-only mac WIP. |
| Can plugin updates avoid relaunch? | [NO] Structural. Minimize `.rhp` churn; version the protocol; move logic to per-run payloads. |

## Sources

| [SOURCE] | [DATE] |
| --- | --- |
| discourse.mcneel.com/t/rhino-wip-feature-rhino-compute-on-linux/217111 | 2026-03-18 |
| discourse.mcneel.com/t/rhino-compute-rh9-on-linux/216155 | 2026-02-20 |
| discourse.mcneel.com/t/rhino-inside-on-macos/173311 (staff: Fugier, Baer) | 2024-01 (historical context) |
| developer.rhino3d.com/guides/compute/hops-component/ (mac compute statement) | current |
| developer.rhino3d.com/guides/scripting/advanced-cli/ | current (Rhino >= 8.11) |
| nuget.org/packages/Rhino.Testing (9.0.8-beta, deps, TFMs) | 2026-04-13 |
| discourse.mcneel.com/t/rhino-wip-feature-unit-regression-testing/205713 | 2025-06-10 |
| github.com/mcneel/rhino3dm | current |
| github.com/mcneel/compute.rhino3d | current |
| discourse.mcneel.com/t/rhino-wip-features/196699 (feature index) | current |
| discourse.mcneel.com/t/rhino-mcp-server/216568 (Sykes announcement) | 2026-05-07 |
| github.com/mcneel/rhinomcp + mcneel.github.io/RhinoMCP/docs/ | active through 2026-06-05 |
| yak.rhino3d.com/packages/Rhino-MCP-Platform (0.1.5) | 2026-06-04 |
| discourse.mcneel.com/t/rhino-wip-feature-improved-package-manager/219247 | 2026-05-21 |
| developer.rhino3d.com/guides/yak/installing-and-managing-packages/ | current |
| developer.rhino3d.com plugin-loading guide (unloadable-by-design) | current |
| discourse.mcneel.com/t/in-rhino9-wip-runtime-netcore-netcoreversion-v9-only-v9/200697 | 2025 |
| discourse.mcneel.com/t/rhino-for-mac-wip-users-important-update-on-intel-support/207740 | 2025 |
| Local: `RhinoWIP.app` 9.0.26153.12416 bundle (dotnet 10.0.2; rhinocode net8 mis-target; yak CLI surface) | 2026-06-10 |
