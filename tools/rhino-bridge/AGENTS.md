# Rhino Bridge Agent Guide

Operator reference is `tools/rhino-bridge/README.md` (architecture, command catalog, output contract, reference policy, failure reading, update rules, validation gate). This file owns agent-specific routing — when to invoke, what NOT to invoke for, and the smallest validation ladder for bridge changes.

## When to invoke

Use bridge commands ONLY when static .NET gates cannot answer:
- Library scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/*.verify.csx` — `uv run python -m tools.quality bridge verify <path-or-glob>` is the agent-first rail; it resolves the owning project automatically.
- Real diagnostics on a `*.csproj` that targets Rhino or Grasshopper — `uv run python -m tools.quality bridge check <project>`. With no scenario, the internal smoke probe emits `returnValue.kind = "assemblyFreshness"` as assembly-load evidence.
- Source ownership/build proof on a `*.cs` file — `uv run python -m tools.quality bridge check <source.cs>` returns `unsupported` (exit 3) without a scenario, which is truthful build proof, not failure.
- Bridge health check before editing native-touching code — `uv run python -m tools.quality bridge doctor`.
- Local RhinoWIP API metadata lookup before relying on undocumented members — `uv run python -m tools.quality api doctor|path|xml|types|decompile`.

## When NOT to invoke

- Synthetic unit tests, mocked Rhino/Grasshopper, or coverage probes — use xUnit + `Rasm.TestKit` (`docs/testing-libs/`).
- C# analyzer/format failures already covered by `uv run python -m tools.quality static check`.
- Long-running UI-thread experiments — RhinoCode execution is not server-cancelable.
- Rhino settings / template / preference automation — owned by `LoadTime.AtStartup` plugin lifecycle.

## Scenario kit usage

`Rasm.TestKit.Scenarios` capsules (under `tests/csharp/_testkit/Scenarios/`) are bridge-staged automatically — no `#r` directive needed. Consume them via `using Rasm.TestKit.Scenarios;`. See `tests/csharp/AGENTS.md` for the catalog and migration patterns.

Scenarios are authored via the polymorphic `Scenario.Run(theme, capturePath, (key, facts) => { … })` harness — it builds the `Op key`, emits the `scenario=`/`capture=` plain header, runs the body, and serializes the populated `FactBag` to a single `facts={json}` plain line plus a `rasm.rhino-bridge.evidence=facts={json}` marker. Inside the body, scenarios call `facts.Add(string key, object value);` as a void statement (no return value, no chaining required).

Grasshopper-aware projects receive bridge-owned `BridgeWire.ScenarioHostUsings` (`Eto.Drawing`, `LanguageExt`) after the scenario preamble and before `LanguageExtBootstrap`. Host assemblies remain off `#r`; add explicit `using Grasshopper2.*` in scenario source only when a rail needs GH2 types.

## Parsing scenario evidence

Bridge markers are the structured wire contract. Use `Rasm.RhinoBridge.Protocol.BridgeMarker.Scan(string stdout) -> IReadOnlyList<BridgeMarker>` to extract them; filter on `BridgeMarker.Evidence` for the `facts` payload and `BridgeMarker.Returned` for `execute.data.returnValue`-equivalent envelopes. Do not parse individual `key=value` plain lines — the legacy per-fact emission was removed during the scenario migration. The plain `facts={json}` line is duplicate human-readable noise; rely on the prefixed Evidence marker.

## Validation ladder for bridge changes

Run serially in this order. `quality static` and focused `--target` test runs may run concurrently — they isolate MSBuild artifacts per invocation under `.artifacts/agents/<pid>/`. Default `tools.quality test run` executes VSTest then Stryker in one invocation; do not parallelize two default test runs. Bridge build/check/package/verify routes and live Rhino remain single-flight (one endpoint; Yak packaging writes project `bin/` outputs).

```bash
uv run python -m tools.quality self-test
pnpm check:py
uv run pytest tests/tools/quality/test_quality.py -q
git diff --check -- tools/rhino-bridge
uv run python -m tools.quality bridge build-bridge
uv run python -m tools.quality static check
uv run python -m tools.quality bridge doctor
uv run python -m tools.quality bridge check apps/grasshopper/Radyab/Radyab.csproj
rc=0
uv run python -m tools.quality bridge check apps/grasshopper/Radyab/Components/ExtractPoints.cs || rc=$?
[[ "${rc}" == 3 ]]
uv run python -m tools.quality bridge verify tests/csharp/libs/Rasm/Vectors/scenarios/vectors-space-projection.verify.csx
```

For packaging changes, add:

```bash
VERSION=0.0.0-ci.$(date -u +%Y%m%d%H%M%S)
uv run python -m tools.quality bridge package rasm-bridge "${VERSION}"
uv run python -m tools.quality bridge deploy rasm-bridge "${VERSION}"
```
