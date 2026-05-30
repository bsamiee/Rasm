# Rasm.Rhino PR Review

When this PR touches Rhino command, UI, capture, exchange, or category folders under this library.

## Flag

- `RhinoDoc.ActiveDoc` when document or command context exists
- Eto UI off UI thread or without document parenting on macOS
- GH1, Rhino 8, or Windows-only assumptions
- Host API claims without local XML/decompile justification

## Advisory

- New native surface without `api doctor` / `api xml` cross-check when public API expands

## Owners

- [`AGENTS.md`](../AGENTS.md)
- [`docs/host/rhino.md`](../../../../docs/host/rhino.md)
- [`.cursor/rules/rasm-rhino-categories.mdc`](../../../../.cursor/rules/rasm-rhino-categories.mdc)
- [`.cursor/rules/rasm-rhino-ui.mdc`](../../../../.cursor/rules/rasm-rhino-ui.mdc)
