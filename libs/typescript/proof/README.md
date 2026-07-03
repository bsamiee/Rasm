# [PROOF]

`proof` is the dev-plane infrastructure folder of `libs/typescript` — frozen fixture corpora with typed readers, reusable law combinators, Schema-driven arbitraries, layer/container harnesses, and the branch-wide gauges. Specs live beside their owning folders; `proof` is never the spec home, and no hand-rolled harness wrapper exists because layer-sharing is `@effect/vitest` capability. The `corpus` sub-domain owns the frozen cross-language fixtures and their typed readers — the byte-identity digest, content-key, and HLC two-half parity vectors plus the `MATERIAL_LAYER_GOLDEN`, BimWire golden-byte, and convergence corpora — so every cross-language invariant is asserted as bit-parity against frozen bytes, never re-derived. The `law` sub-domain owns the reusable fast-check law combinators (fold identity, merge commutativity, upcast totality) and the Schema-driven arbitraries for every kernel brand and decoded wire shape, the one arbitrary source every folder's specs import. The `harness` sub-domain owns the two verification lanes: the testcontainers pg-18.4-with-extensions row with an S3-compatible object-store row beside it — the store capability-row and object-presign verification lanes — and the pglite fast unit lane (no server extensions) carrying the layer-sharing patterns. The `gauge` sub-domain owns the checks neither Nx tags nor the exports map can express: Stryker mutation and coverage thresholds as data, the subpath-purity/bundle-law/sub-folder crypto-admission gauge asserting zero `@effect/sql/Migrator`/`@effect/sql-pg/PgMigrator` imports branch-wide, and the playwright + k6 e2e drivers. `proof` sits on the dev plane — it imports anything and is imported by nothing — outside the runtime accounting. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[PARITY](.planning/corpus/parity.md): byte-identity digest + content-key + HLC two-half corpus readers (HLC vectors upstream-gated).
- [02]-[GOLDEN](.planning/corpus/golden.md): `MATERIAL_LAYER_GOLDEN`, BimWire golden bytes, convergence corpus readers.
- [03]-[PROPERTY](.planning/law/property.md): reusable fast-check law combinators — fold identity, merge commutativity, upcast totality.
- [04]-[ARBITRARY](.planning/law/arbitrary.md): Schema-driven arbitraries for every kernel brand and decoded wire shape.
- [05]-[CONTAINER](.planning/harness/container.md): testcontainers pg-18.4-with-extensions + S3-compatible object-store rows — the store capability-row and object-presign verification lanes.
- [06]-[UNIT](.planning/harness/unit.md): pglite fast unit lane (no server extensions) + layer-sharing patterns.
- [07]-[MUTATION](.planning/gauge/mutation.md): Stryker + coverage thresholds as data.
- [08]-[PURITY](.planning/gauge/purity.md): edge-ledger import / subpath-purity / bundle-law / sub-folder crypto-admission gauge — the checks the exports map cannot express; asserts zero `@effect/sql/Migrator` | `@effect/sql-pg/PgMigrator` imports branch-wide.
- [09]-[E2E](.planning/gauge/e2e.md): playwright + k6 drivers.

## [2]-[DOMAIN_PACKAGES]

Every proof-domain package the folder owns — the dev tier of the `pnpm-workspace.yaml` catalog, `plane:dev` fenced so none rides a runtime graph. Versions are centralized in the one catalog and never pinned here; API evidence lives in the adjacent `.api/` folder.

[SPEC_RUNNER]:
- `vitest`
- `@vitest/browser-playwright` — the vitest browser-mode driver.
- `@vitest/coverage-v8`
- `@vitest/ui`

[DOM_ENVIRONMENT]:
- `happy-dom` — the fast DOM environment for the unit lane.
- `jsdom` — the spec-conformant DOM environment where fidelity outranks speed.

[PROPERTY_LAW]:
- `fast-check` — the property engine behind the law combinators and Schema-driven arbitraries.

[CONTAINER_HARNESS]:
- `testcontainers` — pg-18.4-with-extensions + the S3-compatible object-store row.
- `@electric-sql/pglite` — the in-process pg fast unit lane, no server extensions.

[MUTATION_GAUGE]:
- `@stryker-mutator/core`
- `@stryker-mutator/typescript-checker`
- `@stryker-mutator/vitest-runner`

[E2E_GAUGE]:
- `@playwright/test`
- `@types/k6` — typed k6 load-script authoring; the k6 binary is a runner fact, never a JS dependency.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry and charters live in `libs/typescript/.planning/README.md`, with catalogues at `libs/typescript/.api/`.

- `effect` — `Schema`, `Layer`, and the rails the arbitraries, law combinators, and harness Layers derive from.
- `@effect/vitest` — the dev-plane spec runner binding the law combinators and shared Layers to every folder's specs.
