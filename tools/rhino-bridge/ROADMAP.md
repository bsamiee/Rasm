# [H1][RHINO_BRIDGE_ROADMAP]
>**Dictum:** *Roadmap work upgrades diagnostic truth before convenience.*

<br>

[IMPORTANT] Treat this file as the bridge professionalization backlog. Current behavior lives in `README.md`; future work below requires implementation and validation before it becomes documented contract.

[CRITICAL] Do not add fake probes, tracked sample projects, synthetic test suites, or convenience wrappers that make Rhino evidence look stronger than it is.

---
## [1][INVARIANTS]
>**Dictum:** *Runtime evidence stays valuable only when provenance stays explicit.*

<br>

- Bridge output is real RhinoWIP/RhinoCode evidence, not a unit-test framework.
- JSON phases are the machine interface for agents and scripts.
- Ownership remains layered: `scripts/rhino.sh` routes, client orchestrates, protocol owns DTOs, plugin executes inside Rhino.
- `check-source <source.cs>` without `--script` proves source ownership and owner build only.
- In-process `execute` is authoritative Rhino evidence; `rhinoCodeCli` is supplemental environment evidence.
- Ignored `.artifacts/rhino/**` files remain transient evidence, not tracked fixtures.

---
## [2][TRUTH_FIXES]
>**Dictum:** *Professional diagnostics report uncertainty instead of hiding it.*

<br>

| [INDEX] | [WORK] | [ACCEPTANCE] |
| :-----: | ------ | ------------ |
| **1** | Surface MSBuild/source-owner evaluation failures instead of collapsing them into `No tracked SDK project owns source file`. | Broken project evaluation returns a failed `resolve` phase with command, project, and stderr evidence. |
| **2** | Add required-vs-optional phase semantics. | Optional `rhinoCodeCli` or `load` skips cannot make required runtime work appear successful. |
| **3** | Fix smoke output identity after `Assembly.LoadFrom`. | Smoke stdout reports post-load assembly identity and path, not pre-load `none`. |
| **4** | Preserve source ownership/build vs runtime execution distinction. | `check-source` without `--script` remains `unsupported`; runtime docs and examples never imply execution. |
| **5** | Make reference provenance honest. | Either plugin applies `BridgeExecuteRequest.References`, or wire field is renamed/removed as report-only metadata. |

---
## [3][ARCHITECTURE]
>**Dictum:** *One coherent execution model beats adjacent partial proofs.*

<br>

| [INDEX] | [WORK] | [ACCEPTANCE] |
| :-----: | ------ | ------------ |
| **1** | Unify load/session and execute provenance. | Result JSON states whether code ran through RhinoCode default context, bridge load context, or explicit session. |
| **2** | Explore target-isolated source diagnostics through collectible `AssemblyLoadContext`. | A source diagnostic can exercise target code while sharing RhinoCommon/GH host assemblies and isolating package dependencies. |
| **3** | Report dependency identity collisions and unresolved closure references. | `load` and `load-smoke` expose assembly name, version, path, and resolver source for conflicts. |
| **4** | Add endpoint/session identity checks. | Commands can reject wrong RhinoWIP instance, app path drift, plugin version drift, or stale long-lived sessions. |
| **5** | Add active-request metadata for `busy` replies. | Busy response includes command, start time, timeout value, and script path when available. |
| **6** | Evaluate stdout/stderr spill or chunking. | Large diagnostics preserve complete output through artifacts or chunks instead of only `truncated: true`. |
| **7** | Add GH/Radyab app proof lane. | A future command proves package install/load/catalog/component discovery, not only project build plus RhinoCode smoke. |

---
## [4][CLI_AND_DEPENDENCIES]
>**Dictum:** *Small operator surfaces beat broad framework adoption.*

<br>

- Add compact `help` and invalid-route messaging for `scripts/rhino.sh`.
- Add `--help` and environment knob usage for `scripts/check-cs.sh`.
- Keep Bash as the operator wrapper; keep option parsing in the C# client.
- Consider `CliWrap` only if process execution policy grows beyond current `dotnet`, `git`, `open`, and `rhinocode` needs.
- Consider `System.CommandLine` only if command count, completions, response files, or parser validation outgrow the current table.
- Keep `LanguageExt` and `Thinktecture` out of `tools/rhino-bridge`; validate target projects that use them instead.
- Consider protocol-owned `System.Text.Json` source generation only if it reduces Rhino plugin reflection/runtime risk without DTO churn.
- Do not add Spectre, Serilog, StreamJsonRpc, Polly, or JSON Schema without a concrete bridge failure they uniquely solve.

---
## [5][VALIDATION_LADDER]
>**Dictum:** *Bridge code changes require static gates plus live Rhino evidence.*

<br>

Run after bridge code changes:

```bash
scripts/rhino.sh bridge build
pnpm check:cs
scripts/rhino.sh bridge doctor --result .artifacts/rhino/e2e/doctor.json
scripts/rhino.sh bridge check apps/grasshopper/Radyab/Radyab.csproj --result .artifacts/rhino/e2e/radyab.json
scripts/rhino.sh bridge check-source libs/csharp/Rasm/Analysis/Mesh.cs --result .artifacts/rhino/e2e/mesh-source-owner.json
scripts/rhino.sh bridge check-source libs/csharp/Rasm/Analysis/Mesh.cs --script .artifacts/rhino/e2e/mesh-runtime-diagnostic.csx --result .artifacts/rhino/e2e/mesh-runtime.json
```

Expected source-owner result: `unsupported` with `resolve` and `build` phases `ok`. Expected runtime result: `ok` with real `execute` stdout from Rhino.

---
## [6][EVIDENCE_SOURCES]
>**Dictum:** *Roadmap choices cite live platform contracts.*

<br>

| [INDEX] | [SOURCE] | [RELEVANCE] |
| :-----: | -------- | ----------- |
| **1** | [RhinoCode CLI](https://developer.rhino3d.com/en/guides/scripting/advanced-cli/) | `rhinocode` can inspect instances, run scripts, and build Rhino script-editor projects; script server startup is separate from Rhino launch. |
| **2** | [Yak CLI reference](https://developer.rhino3d.com/en/guides/yak/yak-cli-reference/) | Local package build/install command behavior. |
| **3** | [Yak package anatomy](https://developer.rhino3d.com/en/guides/yak/the-anatomy-of-a-package/) | Package layout, plugin placement, distribution tag rules. |
| **4** | [PlugInLoadTime](https://developer.rhino3d.com/api/rhinocommon/rhino.plugins.pluginloadtime) | `AtStartup` bridge plugin loading behavior. |
| **5** | [Rhino.PlugIns.PlugIn](https://developer.rhino3d.com/api/rhinocommon/rhino.plugins.plugin) | Installed plugin query and load surface. |
| **6** | [System.Text.Json source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation) | Protocol-owned source-generation option. |
| **7** | [AssemblyLoadContext unloadability](https://learn.microsoft.com/th-th/Dotnet/standard/assembly/unloadability) | Collectible load context and `AssemblyDependencyResolver` behavior. |
| **8** | [UnixDomainSocketEndPoint](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.unixdomainsocketendpoint) | Future transport comparison point; not a current replacement mandate. |
