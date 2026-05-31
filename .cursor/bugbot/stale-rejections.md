# Stale Guidance — Canonical Reject List

Flag any reintroduction of these patterns in code, docs, or comments. Do not duplicate this list in nested BUGBOT files — link here from the root hub.

## Bridge operator

- Use `uv run python -m tools.quality bridge verify <path-or-glob>` for runtime proof.
- Use `uv run python -m tools.quality static check` for managed cleanup.
- Use `uv run python -m tools.quality static build|full` for compile/analyzer proof.
- Use `uv run python -m tools.quality test run [<filter>]` for managed MTP unit tests.
- Use `uv run python -m tools.quality test run --mutation changed|full` for explicit mutation.
- Keep bridge scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios`.
- Emit scenario facts through `facts.Add(...)` inside `Scenario.Run`.

## Host platform (out of scope)

- GH1, Grasshopper 1, Rhino 7, Rhino 8 as targets
- Windows-only Rhino assumptions
- `RhinoDoc.ActiveDoc` when command or UI context supplies a document

## Tooling surface (wrong product)

- Rasm Cursor skills under `.cursor/skills/`
- Merged static + test + bridge in one quality invocation

## Docs (reference leaves)

- Agent routing strings: `read CLAUDE`, `read AGENTS`, `validation gates`, `skill eval`, `agents should`
