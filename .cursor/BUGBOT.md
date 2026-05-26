# Rasm Bugbot Guidance

Review Rasm PRs for behavioral regressions, runtime host mistakes, missing validation, and stale docs. Prefer concrete findings over style comments already covered by `tools/cs-analyzer`.

## High-Value Checks

- Rails stay separate: static C# uses `bash scripts/check-cs.sh check`, unit specs use `bash scripts/test.sh`, and Rhino runtime evidence uses `bash scripts/rhino.sh verify <scenario-or-glob>`.
- C# changes preserve polymorphic collapse, typed rails, boundary conversion, and existing category owners instead of adding helpers, wrappers, shims, or parallel APIs.
- Rhino/GH2/Eto code targets RhinoWIP on macOS, uses local API truth, marshals UI work, document-parents Eto UI, and avoids `RhinoDoc.ActiveDoc` when a document context exists.
- Bridge scenarios use `Scenario.Run` plus `facts.Add`, contain no `#r`, `#load`, or absolute paths, and do not emit per-fact `Console.WriteLine("key=value")` evidence.
- Dependencies follow central package management, app-bundle host references, catalog usage, and approved docs before local reinvention.

## Stale Guidance To Flag

Flag references to `BridgeMarker.EmitFact`, `EmitScenarioHeader`, `load-smoke`, `StartScriptServer`, job JSON, `tests/rhino`, `pnpm check:cs`, GH1, Rhino 8, Windows assumptions, or Rasm Cursor skills.
