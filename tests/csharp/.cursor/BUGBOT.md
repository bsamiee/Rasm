# Tests PR Review

When this PR touches `*.spec.cs`, `_testkit/**`, or `*.verify.csx`.

## Flag

- Implementation-as-oracle or Grade-D mirror specs (assert what the test just wrote)
- Static xUnit for behavior that belongs on the bridge rail
- Raw `Grasshopper2.*` in isolated bridge scenarios — use `Rasm.Grasshopper.UI` wrappers
- `#r`, `#load`, absolute paths, per-fact emitters in scenarios
- Stale test commands — [stale-rejections.md](../../../.cursor/bugbot/stale-rejections.md)

## Advisory

- feat/fix without regression law, spec, or scenario where behavior changed
- `Gen.Select` + `throw` instead of `Where` + `Select` (CsCheck where-limit exhaustion)

## Leave alone

- Stryker zero-test diagnostic output — tooling evidence, not product weakness ([`AGENTS.md`](../AGENTS.md) §4)

## Owners

- [`tests/csharp/AGENTS.md`](../AGENTS.md)
- [`.cursor/rules/rasm-csharp-specs.mdc`](../../../.cursor/rules/rasm-csharp-specs.mdc)
- [`.cursor/rules/rasm-bridge-scenarios.mdc`](../../../.cursor/rules/rasm-bridge-scenarios.mdc)
- [`docs/testing-libs/README.md`](../../../docs/testing-libs/README.md)
