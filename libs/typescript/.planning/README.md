# [TYPESCRIPT_BRANCH]

The TypeScript branch is a first-class platform library: six capability domains shipping composable `Layer`/`Service` families under one npm package `@rasm/ts` with per-domain subpath exports and `server`/`browser`/`wasm` conditions — the lib-grade substrate for building hundreds of complex apps, complete in isolation and aligned, never coupled, with the C# and Python branches. This file routes the branch and registers the substrate packages; topology, wave order, and the permitted-edge table are `ARCHITECTURE.md`; the data spine is `dataflow-system.md`; versions live only in the `pnpm-workspace.yaml` catalog. Test infrastructure lives under `tests/`, never in the branch: the frozen corpus is `tests/contracts/`, the TS kit is `@rasm/ts-testkit` at `tests/typescript/_testkit`, and the gauge audits are the `tests/typescript/_architecture` suite.

## [01]-[ROUTER]

Six folder roots in build-wave order; each root `README.md` carries the folder page router and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map and seams.

- [01]-[CORE](../core/README.md) — W0 branch law: the cross-language value floor, host-free state algebra, the ONE keyed-decode interchange plane, observability vocabulary and derivation.
- [02]-[SECURITY](../security/README.md) — W1 identity and custody: authn ceremonies, authorization, the crypto authority, leased secrets — state behind ports the data wave satisfies.
- [03]-[DATA](../data/README.md) — W2 durable persistence: the append-only journal, the guarantee-lane matrix, the content-addressed object plane, the typed read side.
- [04]-[RUNTIME](../runtime/README.md) — W3 execution on both process planes: process substrate, transport, fanout and coordination, the OTLP wire, the one front door, durable work, the intelligence spine, the browser runtime.
- [05]-[UI](../ui/README.md) — W4 interface: the component system and view plane, with `viewer` as a second Nx project carrying the spatial tier.
- [06]-[IAC](../iac/README.md) — W4 deploy plane: Pulumi typed programs over one `StackSpec`, arm dispatch, the self-hosted Kubernetes tiers, secrets, observability realization, policy.

Branch-level page beside this router: [dataflow-system.md](dataflow-system.md) — the data spine: content identity, interchange plane, journal spine, time/order, tenancy, cross-language invariants.

## [02]-[SUBSTRATE_PACKAGES]

The branch substrate every folder builds on, catalogued at `libs/typescript/.api/` and never duplicated into a folder `.api/`. A consuming folder names its substrate set in its README `[03]-[SUBSTRATE_PACKAGES]` section; every other package is folder-local, registered in the owning folder README and catalogued at that folder's `.api/`.

- `effect` — every folder: rails, `Schema`, `Layer`, `Match`, `Stream`, `STM`, vocabulary substrate.
- `@effect/platform` — the platform service contracts; the `-node`/`-bun` bindings back the runtime exec and serve rows, `-browser` backs the browser condition and the ui folder.
- `@effect/experimental` — overlay lanes only (`DurableQueue`, `PersistedCache`, `RateLimiter`, `EventLog`, the persisted request bands the data batch engine and the runtime ai embed lane mint, the serializable `Machine` actor the ui atom bridge binds); the record of truth never depends on it.
- `@effect/opentelemetry` — the OTLP bridge; the runtime otel sub-domain owns the wire, every folder emits through the core `Convention` vocabulary.
- `ssh2` — the in-process SSHv2 root behind the remote-transfer rail; the external `rsync`/`scp` binaries ride `@effect/platform` `Command`.
- `@effect/vitest` — the dev-plane spec runner binding the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) to every folder's colocated specs.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapses to a dash). The substrate tier lives here at `libs/typescript/.api/`; folder-local tiers live at `<folder>/.api/`; the dev-tool tier lives at `tests/typescript/.api/`; a package is catalogued at exactly one tier. A design fence transcribes an external member only at a spelling its owning catalogue verifies; an unverifiable member is a RESEARCH item, never settled code.
