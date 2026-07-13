# [TYPESCRIPT_BRANCH]

`libs/typescript` routes the branch folders in build-wave order and owns the substrate registry every folder composes. Capability domains ship as composable `Layer`/`Service` families under one npm package `@rasm/ts` with per-domain subpath exports and `server`/`browser`/`wasm` conditions — the lib-grade substrate for hundreds of complex apps, complete in isolation and aligned by contract with the C# and Python branches, never coupled to them. Topology, wave order, and the permitted-edge table are `ARCHITECTURE.md`; the data spine is `dataflow-system.md`; the `pnpm-workspace.yaml` catalog pins versions. Test infrastructure lives under `tests/`, never the branch: the frozen corpus is `tests/contracts/`, the `@rasm/ts-testkit` kit is `tests/typescript/_testkit`, and the gauge audits are the `tests/typescript/_architecture` suite.

## [01]-[ROUTER]

Folder roots in build-wave order; each root `README.md` carries the folder page router and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map and seams.

- [01]-[CORE](../core/README.md) — W0 branch law: value floor, state algebra, the one keyed-decode interchange plane, observability vocabulary.
- [02]-[SECURITY](../security/README.md) — W1 identity and custody, stateless behind ports the data wave satisfies.
- [03]-[DATA](../data/README.md) — W2 durable persistence: journal spine, guarantee lanes, object plane, typed read side.
- [04]-[RUNTIME](../runtime/README.md) — W3 execution across both process planes: proc, transport, OTLP wire, front door, durable work, ai, browser.
- [05]-[UI](../ui/README.md) — W4 interface: component system and view plane, `viewer` the spatial second Nx project.
- [06]-[IAC](../iac/README.md) — W4 deploy plane: Pulumi typed programs over one `StackSpec`, arm dispatch, self-hosted Kubernetes tiers.

Branch-level page beside this router: [dataflow-system.md](dataflow-system.md) — the data spine binding content identity, the interchange plane, the journal, tenancy, and the cross-language invariants.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate every folder composes, catalogued once at `libs/typescript/.api/` and never duplicated into a folder `.api/`. A consuming folder names its substrate set in its README `[03]-[SUBSTRATE_PACKAGES]` section; every other package is folder-local, registered in the owning folder README and catalogued at that folder's `.api/`.

- `effect` — the rail, `Schema`, `Layer`, `Match`, `Stream`, and `STM` substrate.
- `@effect/platform` — platform service contracts; `-node`/`-bun` bindings back runtime exec and serve, `-browser` backs the browser condition.
- `@effect/experimental` — durable overlays: `DurableQueue`, `PersistedCache`, `EventLog`, serializable `Machine`; never the record of truth.
- `@effect/opentelemetry` — the OTLP bridge; the runtime otel sub-domain owns the wire, every folder emits through the core `Convention` vocabulary.
- `@effect/vitest` — the spec runner binding `@rasm/ts-testkit` combinators to colocated specs; catalogued at `tests/typescript/.api/`.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapsing to a dash). Substrate-tier catalogues live at `libs/typescript/.api/`, folder-local tiers at `<folder>/.api/`, the dev-tool tier at `tests/typescript/.api/`; a package is catalogued at exactly one tier. A design fence transcribes an external member only at a spelling its owning catalogue verifies; an unverifiable member is a RESEARCH item, never settled code.
