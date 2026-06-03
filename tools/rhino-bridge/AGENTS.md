# Rhino Bridge Agent Guide

Scope: `tools/rhino-bridge/` only. Root `AGENTS.md` and `CLAUDE.md` own universal policy; this file is the canonical bridge operator delta.

Operator reference is `tools/rhino-bridge/README.md` (architecture, command catalog, output contract, reference policy, failure reading, update rules, validation gate). This file owns agent-specific routing — when to invoke, what NOT to invoke for, and the smallest validation ladder for bridge changes.

## When to invoke

Use bridge commands ONLY when static .NET gates cannot answer:
- Library scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/*.verify.csx` — `uv run python -m tools.quality bridge verify <path-or-glob>` is the agent-first rail; it resolves the owning project automatically.
- Real diagnostics on a `*.csproj` that targets Rhino or Grasshopper — `uv run python -m tools.quality bridge check <project>`. With no scenario, the internal smoke probe emits `returnValue.kind = "assemblyFreshness"` as assembly-load evidence.
- Source ownership/build proof on a `*.cs` file — `uv run python -m tools.quality bridge check <source.cs>` returns `unsupported` (exit 3) without a scenario, which is truthful build proof, not failure.
- Bridge `check`/`verify` REBUILD the owning project in Release under `RestoreLockedMode=true`. A stale `packages.lock.json` surfaces as the distinct fault category `nuget-lock-drift` (NU1004/NU1403) — fix it with `dotnet restore Workspace.slnx --force-evaluate`, not a code edit; it is no longer reported as a generic compile error.
- Bridge health check before editing native-touching code — `uv run python -m tools.quality bridge doctor`. Reports `hostRuntime`, RhinoCode `scriptEngine` readiness, and resolved host-assembly versions/paths from the newest installed RhinoWIP bundle (never NuGet).
- Local RhinoWIP API metadata lookup before relying on undocumented members — `uv run python -m tools.quality api doctor|path|xml|types|decompile`.

## When NOT to invoke

- Synthetic unit tests, mocked Rhino/Grasshopper, or coverage probes — use xUnit + `Rasm.TestKit` (`docs/testing-libs/`).
- Managed cleanup is covered by `uv run python -m tools.quality static fix`; compile/analyzer proof is covered by `uv run python -m tools.quality static build`.
- Long-running UI-thread experiments — RhinoCode execution is not server-cancelable.
- Rhino settings / template / preference automation — owned by `LoadTime.AtStartup` plugin lifecycle.

## Scenario kit usage

`Rasm.TestKit.Scenarios` capsules (under `tests/csharp/_testkit/Scenarios/`) are bridge-staged automatically — no `#r` directive needed. The bridge injects `BridgeWire.ScenarioBaseUsings` into every scenario. Catalog and migration patterns: `tests/csharp/_testkit/Scenarios/` and `.claude/skills/cs-testing/references/bridge-runtime.md`.

Scenarios are authored via the polymorphic `Scenario.Run(theme, capturePath, (key, facts) => { … })` harness — it builds the `Op key`, emits the `scenario=`/`capture=` plain header, runs the body, and serializes the populated `FactBag` to a single `facts={json}` plain line plus a `rasm.rhino-bridge.evidence=facts={json}` marker. Inside the body, scenarios call `facts.Add(string key, object value);` as a void statement (no return value, no chaining required).

Grasshopper-aware projects receive bridge-owned `BridgeWire.ScenarioHostUsings` (`Eto.Drawing`) in addition to the base set, after the scenario preamble and before `LanguageExtBootstrap`, and the bridge pre-loads Grasshopper2 into the default ALC before execution so GH2-backed rails resolve at runtime. Host assemblies remain off `#r`. Drive GH2 through `Rasm.Grasshopper.UI` wrapper types — a raw `using Grasshopper2.*` plus direct GH2 types in the scenario body needs GH2 as a compile reference, which under the isolated resolver binds a separate GH2 instance and breaks the shared editor/canvas singletons; such scenarios cannot run on the isolated bridge.

`bridge verify` writes JSON reports, shadow refs, `summary.json`, and PNG captures under `.artifacts/rhino/verify/<run-id>`. Treat that directory as ephemeral: default retention is `300` seconds via `QUALITY_VERIFY_RETENTION_SECONDS`, and each verify run prunes prior bundles older than that window.

## Parsing scenario evidence

Bridge markers are the structured wire contract. Use `Rasm.RhinoBridge.Protocol.BridgeMarker.Scan(string stdout) -> IReadOnlyList<BridgeMarker>` to extract them; filter on `BridgeMarker.Evidence` for the `facts` payload and `BridgeMarker.Returned` for `execute.data.returnValue`-equivalent envelopes. Do not parse individual `key=value` plain lines — the legacy per-fact emission was removed during the scenario migration. The plain `facts={json}` line is duplicate human-readable noise; rely on the prefixed Evidence marker.

## Validation for bridge changes

Select the touched rail. `quality static fix`, `quality static report`, `quality static build`, and MTP test runs may run concurrently — they isolate MSBuild artifacts per invocation under `.artifacts/quality/<rail>/<run-id>/`. Live Rhino bridge commands, bridge verify, bridge package live steps, and package stage commit use fail-fast leases.

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

For packaging changes, add:

```bash
VERSION=0.0.0-ci.$(date -u +%Y%m%d%H%M%S)
uv run python -m tools.quality bridge package rasm-bridge "${VERSION}"
uv run python -m tools.quality bridge deploy rasm-bridge "${VERSION}"
```
