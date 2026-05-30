# Stale Guidance — Canonical Reject List

Flag any reintroduction of these patterns in code, docs, or comments. Do not duplicate this list in nested BUGBOT files — link here from the root hub.

## Bridge operator (removed)

- `scripts/rhino.sh`, `scripts/check-cs.sh`, `scripts/test.sh`, `scripts/mutate-cs.sh`
- `pnpm check:cs`, `pnpm run check:cs:runtime`
- `load-smoke`, `StartScriptServer`, `bridge check-source`
- Job JSON under `tools/rhino-bridge/jobs/`
- `tests/rhino` scenario paths
- `BridgeMarker.EmitFact`, `EmitScenarioHeader`
- Per-fact `Console.WriteLine("key=value")` in scenarios (use batched `facts.Add` + `BridgeMarker.Scan`)

## Host platform (out of scope)

- GH1, Grasshopper 1, Rhino 7, Rhino 8 as targets
- Windows-only Rhino assumptions
- `RhinoDoc.ActiveDoc` when command or UI context supplies a document

## Tooling surface (wrong product)

- Rasm Cursor skills under `.cursor/skills/`
- Merged static + test + bridge in one quality invocation

## Docs (reference leaves)

- Agent routing strings: `read CLAUDE`, `read AGENTS`, `validation gates`, `skill eval`, `agents should`
