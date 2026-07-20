# [TYPESCRIPT_BRANCH]

`libs/typescript` is the platform's host-free web/edge runtime: capability domains composing into whole products, shipped as one npm package `@rasm/ts` with per-domain subpath exports and `server`/`browser`/`wasm` conditions. Sibling coupling is decode-only: C# owns every wire vocabulary and the tessellation rail, the branch decodes each family once at one keyed registry, owns no geometry or IFC semantics, and re-derives content identity bit-identically from the C#-owned seed — aligned by wire bytes, frozen corpora, and the descriptor drift gate, never by import.

Topology, wave order, and the permitted-edge table are `ARCHITECTURE.md`; the data spine is `dataflow-system.md`; the `pnpm-workspace.yaml` catalog pins versions. Test infrastructure lives under `tests/`, never the branch: the frozen corpus is `tests/contracts/`, the `@rasm/ts-testkit` kit is `tests/typescript/_testkit`, and the gauge audits are the `tests/typescript/_architecture` suite.

## [01]-[ROUTER]

Folder roots in build-wave order; each root `README.md` carries the folder identity, its sub-domain router, and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map and seams.

- [01]-[CORE](../core/README.md) — W0 branch law every folder composes.
- [02]-[SECURITY](../security/README.md) — W1 identity and custody, stateless behind ports.
- [03]-[DATA](../data/README.md) — W2 durable persistence and the record of truth.
- [04]-[RUNTIME](../runtime/README.md) — W3 execution substrate across both process planes and the browser condition.
- [05]-[UI](../ui/README.md) — W4 browser product surface; `viewer` the spatial second Nx project.
- [06]-[IAC](../iac/README.md) — W4 deploy plane; nothing depends on it at runtime.

Branch-level page beside this router: [dataflow-system.md](dataflow-system.md) — the data spine binding content identity, the interchange plane, the journal, tenancy, and the cross-language invariants.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate, catalogued once at `libs/typescript/.api/` and never duplicated into a folder `.api/`. A consuming folder names its substrate set in its README `[03]-[SUBSTRATE_PACKAGES]` section; every other package is folder-local, registered in the owning folder README and catalogued at that folder's `.api/`. Observability emit is `effect`-native: every folder instruments through `Metric`/`Effect.withSpan`/`Effect.log*` under the core `Convention` vocabulary, and the runtime otel sub-domain alone owns the OTLP wire.

- `effect` — carries the rail, `Schema`, `Layer`, `Match`, `Stream`, and `STM` substrate.
- `@effect/platform` — platform service contracts each binding realizes.
- `@effect/platform-node` — node binding backing server exec and serve.
- `@effect/platform-bun` — bun binding for the same server plane.
- `@effect/platform-browser` — browser condition binding.
- `@effect/experimental` — durable overlays: `DurableQueue`, `PersistedCache`, `EventLog`, serializable `Machine`; never the record of truth.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapsing to a dash). Substrate-tier catalogues live at `libs/typescript/.api/`, folder-local tiers at `<folder>/.api/`, the dev-tool tier at `tests/typescript/.api/`. A substrate package is catalogued at exactly one tier; a domain-seam package shared across folders carries one folder-scoped catalogue per consuming folder. A design fence transcribes an external member only at a spelling its owning catalogue verifies; an unverifiable member is a RESEARCH item, never settled code.
