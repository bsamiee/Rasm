# [AGENT_MANIFEST]

Scope: `tools/rhino-bridge/` only. Root `AGENTS.md` and `CLAUDE.md` own universal policy; this file is the canonical bridge-operator delta. Full operator reference (command catalog, output contract, reference policy, failure reading, update rules) is `tools/rhino-bridge/README.md` — point to its sections, do not restate them here.

## [1][ROUTING]
- Quality-rail selection and runtime-verification policy: `CLAUDE.md` §5.2; live-lease parallelism rules: `CLAUDE.md` §5.2 [10].
- Scenario authoring + testkit: `tests/csharp/AGENTS.md` §7 and `.claude/skills/testing-cs/references/bridge-runtime.md`; `.verify.csx` and testkit route to `testing-cs`.
- Architecture (operator→client→protocol→plugin), command catalog, env vars, status/exit policy: `README.md` §2-§4.
- Reference projection + isolation policy: `README.md` §5. Failure-signal table: `README.md` §6. Bridge change update rules: `README.md` §7.

## [2][WHEN_TO_INVOKE]
Use bridge commands ONLY when static .NET gates (`static fix`/`static build`) cannot answer — those rails own managed cleanup and compile/analyzer proof; never reach for the bridge for them.
- Library scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/*.verify.csx` — `bridge verify <path-or-glob>` is the agent-first rail; it resolves the owning project from the mirror convention automatically (no manifest, no catalog).
- Real diagnostics on a Rhino/GH `*.csproj` — `bridge check <project>`. With no scenario the internal smoke probe emits `returnValue.kind = "assemblyFreshness"` as assembly-load evidence.
- Build proof on a `*.cs` — `bridge check <source.cs>` returns `unsupported` (exit 3) without a scenario. Exit 3 here is truthful build proof, NOT failure; do not retry it as an error.
- Runtime host facts before editing native-touching code — `bridge doctor` (resolves `hostRuntime`, `scriptEngine` readiness, and host-assembly versions/paths from the newest installed RhinoWIP bundle, never NuGet).
- Undocumented host/package member reads — `api doctor|resolve|query|show`; `api query <key> <symbol>` (dotted `Type.Member`) owns member/type reads via `ilspycmd`, XML-optional. Compact JSON to stdout; full query/decompile evidence under `.artifacts/quality/api/<run-id>/`.

## [3][WHEN_NOT_TO_INVOKE]
- Synthetic/mocked unit tests or coverage probes — xUnit + `Rasm.TestKit` (`docs/testing-libs/`), never the bridge.
- Long-running UI-thread experiments — RhinoCode execution is not server-cancelable; `--timeout-ms` is a client transport deadline only.
- Rhino settings / template / preference automation — owned by `LoadTime.AtStartup` plugin lifecycle, never scripted from this repo.

## [4][BRIDGE_INVARIANTS]
Non-derivable constraints an agent cannot infer from the code:
- **Rebuild semantics:** `check`/`verify` REBUILD the owning project in Release under `RestoreLockedMode=true`. A stale `packages.lock.json` surfaces as the distinct fault category `nuget-lock-drift` (NU1004/NU1403) — remediate with `dotnet restore Workspace.slnx --force-evaluate`, NOT a code edit. It is not a generic compile error.
- **GH2 isolated-resolver trap:** Grasshopper-aware projects get bridge-owned `BridgeWire.ScenarioHostUsings` (`Eto.Drawing`) and GH2 is pre-loaded into the default ALC (`BridgeWire.GrasshopperPluginId`) before execution. Drive GH2 only through `Rasm.Grasshopper.UI` wrapper types. A raw `using Grasshopper2.*` plus direct GH2 types forces GH2 as a compile reference, which the isolated resolver binds as a SEPARATE GH2 instance, breaking the shared editor/canvas singletons — such scenarios cannot run on the isolated bridge.
- **HashMap key policy in bridge-hot assemblies:** use only primitive, `string`, `Guid`, `uint`/`ulong`, or built-in value-tuple keys inside `HashMap<,>`. Custom record-struct keys (e.g. `PreviewFingerprint`) fail under isolated-resolver trait warmup even with correct reference order.
- **No `#r`/`#load`/absolute paths in scenarios:** the bridge owns reference projection and injects `BridgeWire.ScenarioBaseUsings` into every scenario; host assemblies stay off `#r`. Authoring these in a scenario body is a hard error, not a convenience.

## [5][SCENARIO_KIT]
- `Rasm.TestKit.Scenarios` capsules (`tests/csharp/_testkit/Scenarios/`) are bridge-staged automatically — no `#r` directive. Catalog/migration patterns live there and in `.claude/skills/testing-cs/references/bridge-runtime.md`.
- Author via the polymorphic `Scenario.Run(theme, capturePath, (key, facts) => { … })` harness: it builds the `Op key`, emits the `scenario=`/`capture=` header, runs the body, and serializes the `FactBag` to one `facts={json}` plain line plus one `rasm.rhino-bridge.evidence=facts={json}` marker. Inside the body, call `facts.Add(string key, object value)` as a void statement (no return, no chaining).

## [6][PARSING_EVIDENCE]
- Bridge markers are the structured wire contract. Use `Rasm.RhinoBridge.Protocol.BridgeMarker.Scan(string stdout) -> IReadOnlyList<BridgeMarker>`; filter `BridgeMarker.Evidence` for the `facts` payload and `BridgeMarker.Returned` for `execute.data.returnValue` envelopes.
- Do NOT parse individual `key=value` plain lines — legacy per-fact emission was removed during the scenario migration. The plain `facts={json}` line is duplicate human-readable noise; rely on the prefixed Evidence marker.

## [7][ARTIFACTS]
- `bridge verify` writes JSON reports, shadow refs, `summary.json`, and PNG captures under `.artifacts/rhino/verify/<run-id>`. Treat it as ephemeral: default retention `300` s via `QUALITY_VERIFY_RETENTION_SECONDS`; each run prunes prior bundles past that window. Never persist references into it.

## [8][VALIDATION]
Select the touched rail; parallelism/lease policy is `CLAUDE.md` §5.2 [10] (concurrent static/test rails; fail-fast exclusive leases for live bridge/verify/package). Minimal ladder for bridge changes:

```bash
uv run python -m tools.quality self-test
pnpm check:py
uv run pytest tests/tools/quality/test_quality.py -q
uv run python -m tools.quality bridge build-bridge
uv run python -m tools.quality static fix
uv run python -m tools.quality static build
uv run python -m tools.quality bridge doctor
uv run python -m tools.quality bridge check apps/grasshopper/Radyab/Radyab.csproj
rc=0
uv run python -m tools.quality bridge check apps/grasshopper/Radyab/Components/ExtractPoints.cs || rc=$?
[[ "${rc}" == 3 ]]
uv run python -m tools.quality bridge verify tests/csharp/libs/Rasm/Vectors/scenarios
```

Packaging changes add:

```bash
VERSION=0.0.0-ci.$(date -u +%Y%m%d%H%M%S)
uv run python -m tools.quality bridge package rasm-bridge "${VERSION}"
uv run python -m tools.quality bridge deploy rasm-bridge "${VERSION}"
uv run python -m tools.quality bridge publish rasm-bridge "${VERSION}"
```
