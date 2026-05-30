# Bridge PR Review

When this PR touches `tools/rhino-bridge/**`, bridge Python rails, or shared scenario infrastructure.

## Flag

- Wrong operator routes — canonical: [`AGENTS.md`](../AGENTS.md), [`README.md`](../README.md)
- `BridgeMarker.EmitFact`, per-fact console evidence, `#r`, `#load`, absolute paths in scenarios
- Missing `facts.Add` inside `Scenario.Run`
- Parallel `bridge verify` assumptions (single-flight live Rhino)
- Stale commands — [stale-rejections.md](../../../.cursor/bugbot/stale-rejections.md)

## Advisory

- Native-touching change without scenario or glob update when runtime proof is implied
- Packaging/version ladder drift for `rasm-bridge` Yak flow

## Owners

- [`.cursor/rules/rasm-rhino-bridge.mdc`](../../../.cursor/rules/rasm-rhino-bridge.mdc)
- [`.cursor/rules/rasm-bridge-scenarios.mdc`](../../../.cursor/rules/rasm-bridge-scenarios.mdc)
