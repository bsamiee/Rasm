# [TYPESCRIPT_BRANCH]

One `pnpm-workspace.yaml` catalog pins versions; test infrastructure lives under `tests/`, never the branch.

## [01]-[ROUTER]

Folder roots in stratum order; each root `README.md` carries the folder identity, its sub-domain router, and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map and boundaries.

- [01]-[CORE](../core/README.md) — branch law every folder composes.
- [02]-[SECURITY](../security/README.md) — identity and custody, stateless behind ports.
- [03]-[DATA](../data/README.md) — durable persistence and the record of truth.
- [04]-[RUNTIME](../runtime/README.md) — execution substrate across both process planes and the browser condition.
- [05]-[UI](../ui/README.md) — browser product surface; `viewer` the spatial second Nx project.
- [06]-[IAC](../iac/README.md) — deploy plane; nothing depends on it at runtime.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate; a consuming folder names its substrate set in its README `[03]-[SUBSTRATE_PACKAGES]` section, and every other package is folder-local, registered in the owning folder README.

[CONTRACT_BINDINGS]:

[TYPING_CORE]:
- `effect` — Carries the effect, schema, and layer substrate every folder composes.

[PLATFORM]:
- `@effect/platform` — Platform service contracts each binding realizes.
- `@effect/platform-node` — node binding backing server exec and serve.
- `@effect/platform-bun` — bun binding for the same server plane.
- `@effect/platform-browser` — Browser condition binding.
- `@effect/experimental` — Durable overlays; never the record of truth.

[EVENT_FABRIC]:
- `cloudevents` — Core seats the one message-envelope mint, data projects the outbox, and runtime binds each transport.

[BENCH]:
- `mitata` — Mints the benchmark measurement shape the claim family folds; registration and render stay in the bench lane under `tests/`.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapsing to a dash): substrate-tier catalogues live at `libs/typescript/.api/`, folder-local tiers at `<folder>/.api/`, and the dev-tool tier at `tests/typescript/.api/`.
