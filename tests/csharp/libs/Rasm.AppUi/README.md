# [RASM_APPUI_TESTS]

`Rasm.AppUi.Tests` is the AppUI proof shell. It owns headless visual sessions and branch-local generated-wire goldens; production pages expose deterministic inputs, while assertions, snapshot artifacts, and test-only packages stay here.

## [01]-[ROUTER]

- [01]-[CONTRACT_WIRE](.planning/contract-wire.md): Generated AppUI contract output pinned through the shared ProtoJSON registry.

## [02]-[TEST_PACKAGES]

- `Avalonia.Headless.XUnit` — Headless Avalonia session runner for UI assertions.
- `Verify.DiffPlex` — Assembly-wide compact snapshot diff rendering; private to the test project.
- `Verify.XunitV3` — Durable generated-wire snapshots; private to the test project.

## [03]-[SUBSTRATE]

- `Rasm.AppUi` (project) — Deterministic branch producers and generated `ui.v1`/`render.v1` projections under test.
