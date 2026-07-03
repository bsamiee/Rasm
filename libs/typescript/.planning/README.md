# [TYPESCRIPT_BRANCH]

The TypeScript branch is a first-class platform library: thirteen folders shipping composable `Layer`/`Service` families under one npm package `@rasm/ts`, the lib-grade substrate for building hundreds of complex apps — complete in isolation and aligned, never coupled, with the C# and Python branches. This file routes the branch and registers the substrate packages; topology and dependency direction are `ARCHITECTURE.md`; versions live only in the `pnpm-workspace.yaml` catalog. Test infrastructure lives under `tests/`, never in the branch: the frozen corpus is `tests/contracts/`, the TS kit is `@rasm/ts-testkit` at `tests/typescript/_testkit`, and the gauge audits are the `tests/typescript/_architecture` suite.

## [01]-[ROUTER]

Thirteen folder roots in build-wave order; each root `README.md` carries the folder page router and its own package registry, each `ARCHITECTURE.md` the folder sub-domain map.

- [01]-[KERNEL](../kernel/README.md) — cross-language identity, clock, schema-brand, quantity, and fault-classification values; the contract floor.
- [02]-[STATE](../state/README.md) — host-free fold algebra: keyed folds, CRDT merge, causality, evidence, live queries.
- [03]-[HOST](../host/README.md) — process runtime: Node/Bun exec rows, config provider chain, flag verdicts, net policy, lifecycle.
- [04]-[SECURITY](../security/README.md) — authn, authz, sessions, secrets, signing as Effect-owned Layers over stateless primitives.
- [05]-[TELEMETRY](../telemetry/README.md) — four-signal observability plane: OTLP, conventions, RUM, audit/meter fact streams, SLO algebra, dashboards-as-functions.
- [06]-[WIRE](../wire/README.md) — all C#-minted wire decode: codecs, frames, gateway, contract drift, fault reconstruction, capability SDK.
- [07]-[WORK](../work/README.md) — durable execution: cluster entities, workflows, queues, schedules, signed egress.
- [08]-[STORE](../store/README.md) — event-sourced durable persistence: journal, projections, capability rows, tenancy scopes, lanes, retrieval, objects.
- [09]-[AI](../ai/README.md) — intelligence rail: model provider rows, embeddings, tools, durable agents, MCP hosting.
- [10]-[EDGE](../edge/README.md) — the one public front door: HttpApi assembly, serve rows, realtime, webhooks, CLI verbs, problem mapping.
- [11]-[BROWSER](../browser/README.md) — browser runtime: single-boot law, PWA shell, local persistence, decode transport, Navigation-API routing, session ceremonies.
- [12]-[UI](../ui/README.md) — component capability: atoms, tokens, interaction, intl, headless views; `viewer` as a second Nx project for the spatial tier.
- [13]-[IAC](../iac/README.md) — deploy plane: Pulumi typed programs, provider dispatch, K8s tiers, secrets provisioning, observability stacks, policy.

Branch-level pages beside this router: [composition-system.md](composition-system.md) — the composition system: package shape, edge ledger, ports, app composition, extension recipes; [dataflow-system.md](dataflow-system.md) — the data spine: content identity, wire plane, event spine, time/order, tenancy, cross-language invariants.

## [02]-[SUBSTRATE_PACKAGES]

The branch substrate every folder builds on, catalogued at `libs/typescript/.api/` and never duplicated into a folder `.api/`. A consuming folder names its substrate set in its README `[SUBSTRATE_PACKAGES]` section; every other package is folder-local, registered in the owning folder README and catalogued at that folder's `.api/`.

- `effect` — every folder: rails, `Schema`, `Layer`, `Match`, `Stream`, vocabulary substrate.
- `@effect/platform` — the platform service contracts; the `-node`/`-bun` bindings back the `host` exec rows and `edge` serve rows, `-browser` backs the `browser` runtime.
- `@effect/experimental` — overlay lanes only (`EventLog` local-first sync); the record of truth never depends on it.
- `@effect/opentelemetry` — the OTLP export family; `telemetry` owns it, every folder emits through it.
- `@effect/vitest` — the dev-plane spec runner binding the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) to every folder's colocated specs.

## [03]-[API_CATALOGUE_LAW]

One catalogue per published package, named for the package (`effect.md`, `effect-platform.md`, a scoped slash collapses to a dash). The substrate tier lives here at `libs/typescript/.api/`; folder-local tiers live at `<folder>/.api/`; the dev-tool tier lives at `tests/typescript/.api/`; a package is catalogued at exactly one tier. A design fence transcribes an external member only at a spelling its owning catalogue verifies.
