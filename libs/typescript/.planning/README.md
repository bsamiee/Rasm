# [TYPESCRIPT_BRANCH]

`libs/typescript` is the platform's host-free web/edge runtime: capability domains composing into whole products, shipped as one npm package `@rasm/ts` with per-domain subpath exports and `server`/`browser`/`wasm` conditions. Sibling coupling is decode-only: C# owns every wire vocabulary and the tessellation rail, the branch decodes each family once at one keyed registry, owns no geometry or IFC semantics, and re-derives content identity bit-identically from the C#-owned seed — aligned by wire bytes, frozen corpora, and the descriptor drift gate, never by import.

`ARCHITECTURE.md` carries the branch topology and seams; the `pnpm-workspace.yaml` catalog pins versions. Test infrastructure lives under `tests/`, never the branch: the frozen corpus is `tests/contracts/`, the `@rasm/ts-testkit` kit is `tests/typescript/_testkit`, and the gauge audits are the `tests/typescript/_architecture` suite.

## [01]-[ROUTER]

Folder roots in stratum order; each root `README.md` carries the folder identity, its sub-domain router, and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map and seams.

- [01]-[CORE](../core/README.md) — S0 branch law every folder composes.
- [02]-[SECURITY](../security/README.md) — S1 identity and custody, stateless behind ports.
- [03]-[DATA](../data/README.md) — S2 durable persistence and the record of truth.
- [04]-[RUNTIME](../runtime/README.md) — S3 execution substrate across both process planes and the browser condition.
- [05]-[UI](../ui/README.md) — S4 browser product surface; `viewer` the spatial second Nx project.
- [06]-[IAC](../iac/README.md) — S4 deploy plane; nothing depends on it at runtime.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate; a consuming folder names its substrate set in its README `[03]-[SUBSTRATE_PACKAGES]` section, and every other package is folder-local, registered in the owning folder README.

- `effect` — carries the rail, schema, and layer substrate every folder composes.
- `@effect/platform` — platform service contracts each binding realizes.
- `@effect/platform-node` — node binding backing server exec and serve.
- `@effect/platform-bun` — bun binding for the same server plane.
- `@effect/platform-browser` — browser condition binding.
- `@effect/experimental` — durable overlays; never the record of truth.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapsing to a dash). Substrate-tier catalogues live at `libs/typescript/.api/`, folder-local tiers at `<folder>/.api/`, the dev-tool tier at `tests/typescript/.api/`. A substrate package is catalogued at exactly one tier; a domain-seam package shared across folders carries one folder-scoped catalogue per consuming folder. A design fence transcribes an external member only at a spelling its owning catalogue verifies; an unverifiable member is a RESEARCH item, never settled code.
