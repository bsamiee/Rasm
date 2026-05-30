# Rasm.Grasshopper PR Review

When this PR touches GH2 components, UI rails, or Grasshopper boundary code.

## Flag

- `GhEditor.ShowEditor` or live editor show in bridge scenarios — headless path uses `EditorOp.Show(visible:false)` → `GhEditor.Instance ?? new GhEditor()`
- Raw `Grasshopper2.*` in isolated `*.verify.csx` (separate GH2 instance / broken singletons)
- Removed `EditorOp.EnsureVisible` reintroduced in scenarios or docs

## Advisory

- GH2 UI change without wire-cache / paint subscription awareness when wire queries are involved ([`AGENTS.md`](../AGENTS.md))

## Owners

- [`AGENTS.md`](../AGENTS.md)
- [`docs/host/gh2.md`](../../../../docs/host/gh2.md)
- [`.cursor/rules/rasm-grasshopper.mdc`](../../../../.cursor/rules/rasm-grasshopper.mdc)
