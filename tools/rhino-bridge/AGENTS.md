# Rhino Bridge Agent Guide

Operator reference is `tools/rhino-bridge/README.md` (architecture, command catalog, output contract, reference policy, failure reading, update rules, validation gate). This file owns agent-specific routing — when to invoke, what NOT to invoke for, and the smallest validation ladder for bridge changes.

## When to invoke

Use bridge commands ONLY when static .NET gates cannot answer:
- Library scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/*.verify.csx` — `scripts/rhino.sh verify <path-or-glob>` is the agent-first rail; it resolves the owning project automatically.
- Real diagnostics on a `*.csproj` that targets Rhino or Grasshopper — `scripts/rhino.sh bridge check <project>`. With no scenario, the internal smoke probe emits `returnValue.kind = "assemblyFreshness"` as assembly-load evidence.
- Source ownership/build proof on a `*.cs` file — `scripts/rhino.sh bridge check <source.cs>` returns `unsupported` (exit 3) without a scenario, which is truthful build proof, not failure.
- Bridge health check before editing native-touching code — `scripts/rhino.sh bridge doctor`.
- Local RhinoWIP API metadata lookup before relying on undocumented members — `scripts/rhino.sh api doctor|path|xml|types|decompile`.

## When NOT to invoke

- Synthetic unit tests, mocked Rhino/Grasshopper, or coverage probes — use xUnit + `Rasm.TestKit` (`docs/testing-libs/`).
- C# analyzer/format failures already covered by `bash scripts/check-cs.sh check`.
- Long-running UI-thread experiments — RhinoCode execution is not server-cancelable.
- Rhino settings / template / preference automation — owned by `LoadTime.AtStartup` plugin lifecycle.

## Scenario kit usage

`Rasm.TestKit.Scenarios` capsules (under `tests/csharp/_testkit/Scenarios/`) are bridge-staged automatically — no `#r` directive needed. Consume them via `using Rasm.TestKit.Scenarios;`. See `tests/csharp/AGENTS.md` for the catalog and migration patterns.

Scenarios are authored via the polymorphic `Scenario.Run(theme, capturePath, (key, facts) => { … })` harness — it builds the `Op key`, emits the `scenario=`/`capture=` plain header, runs the body, and serializes the populated `FactBag` to a single `facts={json}` plain line plus a `rasm.rhino-bridge.evidence=facts={json}` marker. Inside the body, scenarios call `facts.Add(string key, object value);` as a void statement (no return value, no chaining required).

## Parsing scenario evidence

Bridge markers are the structured wire contract. Use `Rasm.RhinoBridge.Protocol.BridgeMarker.Scan(string stdout) -> Seq<BridgeMarker>` to extract them; filter on `BridgeMarker.Evidence` for the `facts` payload and `BridgeMarker.Returned` for `execute.data.returnValue`-equivalent envelopes. Do not parse individual `key=value` plain lines — the legacy per-fact emission was removed during the scenario migration. The plain `facts={json}` line is duplicate human-readable noise; rely on the prefixed Evidence marker.

## Validation ladder for bridge changes

Run serially in this order; bridge build/check/package commands and `scripts/check-cs.sh check` share build caches and one live Rhino endpoint:

```bash
bash -n scripts/rhino.sh
shellcheck scripts/rhino.sh
bash scripts/rhino.sh --self-test
bash scripts/rhino.sh bridge build
bash scripts/check-cs.sh check
git diff --check -- scripts/rhino.sh tools/rhino-bridge
bash scripts/rhino.sh bridge doctor
bash scripts/rhino.sh bridge check apps/grasshopper/Radyab/Radyab.csproj
rc=0
bash scripts/rhino.sh bridge check apps/grasshopper/Radyab/Components/ExtractPoints.cs || rc=$?
[[ "${rc}" == 3 ]]
bash scripts/rhino.sh verify tests/csharp/libs/Rasm/Vectors/scenarios/vectors-space-projection.verify.csx
```

For packaging changes, add:

```bash
VERSION=0.0.0-ci.$(date -u +%Y%m%d%H%M%S)
bash scripts/rhino.sh package rasm-bridge "${VERSION}"
bash scripts/rhino.sh deploy rasm-bridge "${VERSION}"
```
