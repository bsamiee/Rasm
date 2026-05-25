# Rhino Bridge Agent Guide

## Purpose

`tools/rhino-bridge` is the agent-first runtime diagnostic rail for Rasm RhinoWIP and GH2 work. It exists to answer one practical question: what does the current code do inside the real RhinoWIP/RhinoCode host, with the same RhinoCommon, Grasshopper2, NuGet, and repo assemblies Rhino sees?

The bridge is not a unit-test framework, ScriptEditor automation layer, Rhino settings manager, or fake source runner. Static gates prove managed code builds; this tool supplies host-owned runtime evidence when static proof is not enough.

## Architecture

- `scripts/rhino.sh` is the operator entrypoint. Keep it thin, dispatch-table driven, and backed by evaluated MSBuild metadata.
- `client/` owns target resolution, builds, reference staging, phase JSON, report paths, and named-pipe calls.
- `protocol/` owns wire DTOs, status vocabulary, host assembly names, endpoint metadata, and exit-code policy.
- `plugin/` runs in RhinoWIP, owns the pipe server, executes RhinoCode in-process, and reports stdout, stderr, diagnostics, document facts, RhinoCode policy, and structured returns.

## Commands

- `scripts/rhino.sh bridge build`: build protocol, plugin, and client.
- `scripts/rhino.sh bridge launch`: open RhinoWIP and verify the bridge endpoint.
- `scripts/rhino.sh bridge restart`: restart RhinoWIP when a newly deployed bridge package must be loaded.
- `scripts/rhino.sh bridge doctor`: inspect the live RhinoWIP process, bridge identity, required assemblies, and sessions.
- `scripts/rhino.sh bridge check <target> [scenario.csx]`: primary diagnostic command.
- `scripts/rhino.sh bridge clean <target>`: delete generated reports for one target.
- `scripts/rhino.sh bridge load-smoke <assembly.dll>`: lower-level collectible load/unload proof.
- `scripts/rhino.sh package rasm-bridge <version>`: build and stage the local Yak package.
- `scripts/rhino.sh deploy rasm-bridge <version>`: install the Yak package, refresh RhinoWIP, and verify bridge health.

## Target Semantics

- `<project.csproj>` builds the project and runs the assembly-freshness smoke in RhinoCode.
- `<source.cs>` resolves and builds the owning SDK project, then returns `unsupported` unless a scenario is supplied. That is truthful build proof, not a failure.
- `<source.cs> <scenario.verify.csx>` builds the owner, rejects scenario-owned `#r` or `#load`, stages fresh bridge-owned references, injects `SCENARIO_NAME` and `CAPTURE_PATH`, and executes the scenario in RhinoCode.
- `<script.csx>` runs an explicit standalone RhinoCode script. Prefer project/source checks for repo diagnostics.

## Output

- `check` prints structured JSON to stdout and writes the same report under `.artifacts/rhino/bridge/check/<target-path>/` unless `--result` is explicitly supplied.
- `verify` is a convenience wrapper over `bridge check <owning.csproj> <scenario.verify.csx>` and writes `.artifacts/rhino/verify/<scenario>.json`.
- Runtime checks force isolated C# reference resolution and `CachePolicy.NeverCache`; reports include `execute.data.rhinoCode` evidence.
- Project/source scenario references are shadow-copied to per-run artifact `refs/<content-hash>/` directories. Scenarios must not own `#r` blocks.
- `rasm.rhino-bridge.return=<json>` on stdout is optional structured evidence. Raw stdout/stderr and RhinoCode diagnostics remain authoritative when it is absent.

## Development Direction

- Prefer one intelligent command over flags, modes, or alternate routes.
- Extend existing script/client/protocol/plugin ownership; do not add helper-file sprawl or one-off wrappers.
- Do not hardcode app, package, output, or reference paths. Use evaluated MSBuild properties such as `YakPackageSlug`, `YakPath`, `YakStageRoot`, and project build output.
- Do not reintroduce former source-specific routes, job JSON flows, bridge-specific package/install commands, or ScriptEditor/Code Panel automation.
- Do not weaken diagnostics into pass/fail text. Preserve phase JSON, raw output, compile diagnostics, document metadata, RhinoCode policy, and assembly freshness evidence.
- Keep bridge commands serial in validation. The live Rhino endpoint and shared build outputs are intentional single-client surfaces.
- Treat scenarios as disposable diagnostic scripts for current implementation truth. Refactor stale scenarios freely, but do not move product fixes into `libs/` during bridge/tooling work unless explicitly asked.

## Validation

Run the smallest truthful ladder for bridge changes:

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
bash scripts/rhino.sh verify apps/grasshopper/Radyab/Scenarios/vectors-space-projection.verify.csx
```

For packaging changes, add:

```bash
VERSION=0.0.0-ci.$(date -u +%Y%m%d%H%M%S)
bash scripts/rhino.sh package rasm-bridge "${VERSION}"
bash scripts/rhino.sh deploy rasm-bridge "${VERSION}"
```
