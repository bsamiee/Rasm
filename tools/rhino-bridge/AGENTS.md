# Rhino Bridge Agent Guide

## Purpose

`tools/rhino-bridge` is an agent-first RhinoWIP runtime diagnostic tool. It exists to let coding agents validate real C# behavior inside the already-installed RhinoWIP/RhinoCode host without driving the Rhino UI, copying ScriptEditor macro patterns, or inventing fake source execution.

## Architecture

- `scripts/rhino.sh` is the operator entrypoint. Keep it thin, table-driven, and variable-driven.
- `client/` owns target resolution, builds, report paths, JSON phase output, and named-pipe calls.
- `protocol/` owns wire DTOs, statuses, endpoint metadata, and exit-code policy.
- `plugin/` runs inside RhinoWIP, owns the pipe server, invokes RhinoCode on the Rhino UI thread, and captures stdout, stderr, diagnostics, document state, and structured returns.

## Commands

- `scripts/rhino.sh bridge build`: build protocol, plugin, and client.
- `scripts/rhino.sh bridge launch`: open RhinoWIP and verify the bridge endpoint.
- `scripts/rhino.sh bridge restart`: quit a safe Rhino session, relaunch, and reconnect.
- `scripts/rhino.sh bridge doctor`: inspect the live RhinoWIP bridge session.
- `scripts/rhino.sh bridge check <target> [scenario.csx]`: primary diagnostic command.
- `scripts/rhino.sh bridge clean <target>`: delete generated reports for one target.
- `scripts/rhino.sh bridge load-smoke <assembly.dll>`: lower-level assembly load/unload evidence.

## Check Targets

- `<project.csproj>` builds the project and runs the assembly freshness smoke in RhinoCode.
- `<source.cs>` resolves the owning SDK project and builds it. Without a scenario it returns `unsupported`; that is truth, not failure.
- `<source.cs> <scenario.verify.csx>` builds the owner, injects host-filtered runtime references, runs the scenario, and reads `rasm.rhino-bridge.return=<json>` when emitted.
- `<script.csx>` runs an explicit RhinoCode script as the target.

## Output

- `check` prints structured JSON to stdout for the agent and writes the same bytes to `reportPath`.
- Default reports live under `.artifacts/rhino/bridge/check/<target-path>/<timestamp>-<target-file>.json`.
- Do not require `--result` for normal agent work. Keep it only for explicit CI or one-off paths.
- Use `clean <target>` for report cleanup; never delete broad bridge artifact roots from a command path.

## Development Rules

- Prefer one intelligent command over mode/flag sprawl.
- Do not reintroduce public `check-source` or `script` routes.
- Do not automate the Rhino code panel, ScriptEditor macros, Rhino settings, or templates.
- Do not pretend arbitrary library `.cs` files are runnable. Runtime proof needs a scenario.
- Keep RhinoCode CLI and ScriptEditor docs as context only; in-process plugin execution is the authority.
- Preserve structured stdout/stderr, compile diagnostics, document metadata, return JSON, and assembly freshness evidence.
- Add capability by extending the existing client/protocol/plugin surfaces; avoid helper files, hardcoded app paths, and single-use wrappers.

## Validation

Run bridge changes serially:

```bash
bash -n scripts/rhino.sh
shellcheck scripts/rhino.sh
scripts/rhino.sh --self-test
scripts/rhino.sh bridge build
bash scripts/check-cs.sh check
scripts/rhino.sh bridge doctor
scripts/rhino.sh bridge check apps/grasshopper/Radyab/Radyab.csproj
rc=0
scripts/rhino.sh bridge check apps/grasshopper/Radyab/Components/ExtractPoints.cs || rc=$?
[[ "${rc}" == 3 ]]
scripts/rhino.sh bridge clean apps/grasshopper/Radyab/Radyab.csproj
git diff --check -- scripts/rhino.sh tools/rhino-bridge
```
