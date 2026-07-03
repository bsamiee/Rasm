# [API_CATALOGUE] @effect/vitest — services test-harness overlay

`@effect/vitest` is a branch-tier test substrate: the generic `it.effect`/`scoped`/`live`/`layer`/`prop`/`flakyTest` binding, the `Vitest` runner type tree, and the `./utils` Effect-data assertions are catalogued once at the canonical owner `../../.api/effect-vitest.md` (the services README routes it under `[SUBSTRATE_PACKAGES]`, not `[DOMAIN_PACKAGES]`). This page is the services-scoped overlay: it documents how a `services` `.spec.ts` stacks that binding onto the folder's durable/persistence/cluster/entity rails — sharing a `WorkflowEngine.layerMemory` engine, a testcontainers Postgres/Redis `Layer`, and the one `PgClient`/`Migrator` across a block via `it.layer`; driving durable clocks with `TestClock`; and asserting `Data`/`Schema` domain values structurally. The `Vitest` runner contract and the `./utils` assertion signatures are the canonical owner's (full `ts contract` blocks at `../../.api/effect-vitest.md`); this overlay does not restate them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/vitest`
- package: `@effect/vitest` (0.29.0, MIT, © Effectful Technologies) — full generic surface at `../../.api/effect-vitest.md`
- entry: `@effect/vitest` (`vitest` re-export + Effect runner/method tree), `@effect/vitest/utils` (Effect-aware assertions)
- rail: proof (dev-plane; services specs co-located with the folder they prove)
- tier: `neutral` (dev/test-only `devDependency`; peers `vitest@^3.2.0` + `effect@^3.21.0`, no `node:*` import — but a services integration spec pulls `node`-tier layers, so it runs under the node Vitest environment)
- overlay scope: this page owns only the services domain test stacking; every generic collector/modifier/harness/assertion is the canonical owner's

## [02]-[SERVICES_HARNESS_STACKING]

[HARNESS_SCOPE]: the services test layers `it.layer` shares across a spec block
- rail: proof

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY]   | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------------------ | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `it.layer(WorkflowEngine.layerMemory)(…)`                                 | durable engine   | `execution/engine`+`saga` — a durable-execution spec shares the in-memory engine; `it.effect` bodies `Workflow.execute`/`poll`/`interrupt`/`resume`, the test double for production `ClusterWorkflowEngine.layer` |
|  [02]   | `it.layer(pgContainerLayer, { timeout })(…)` then nested `it.layer(SqlBoundary.layer)` | real Postgres | `persistence/store`+`work`+`tenancy`+`idempotency`+`outbox` — a `GenericContainer("postgres:…")` wrapped as a scoped `Layer`, `Migrator.run` applied once per block, every spec on the one `PgClient` |
|  [03]   | `it.layer(redisContainerLayer)(…)`                                        | real Redis       | `messaging/quota`+`persistence/reactive` — a `GenericContainer("redis:…")` `Layer` backing the `@effect/experimental` `RateLimiter`/`Reactivity`/`BackingPersistence` rails over `ioredis` |
|  [04]   | `it.scoped(name, (ctx) => Effect<A, E, TestServices \| Scope>)`           | scoped resource  | any spec acquiring a `Redis`/`S3`/workflow-scope resource — one `Scope` opened and finalized per test |
|  [05]   | `it.effect.prop(name, arbitraries, ({ … }, ctx) => Effect)`              | domain law       | `Schema`-derived arbitraries (`Arbitrary.make`) over the branded domain — `ObjectKey`/`SecretRef`/`Objective`/`SagaStep`/`PresignGrant` — one generator per brand, every law consumes it |
|  [06]   | `it.flakyTest(effect, timeout)`                                           | timing relay     | `execution/outbox` `LISTEN`/`NOTIFY` wake, the `DurableClock` durable wake, the `Reactivity` invalidation fan — retry an inherently timing-dependent assertion to a `Duration` deadline, never a bare `setTimeout` |
|  [07]   | `addEqualityTesters()`                                                    | equality setup   | one call so `expect(a).toEqual(b)` compares `Data.TaggedEnum`/`Schema` domain values (`SagaTerminal`/`StepOutcome`/`DeliverySink`/`Result`) by structural `Equal`, not reference |

The composition pattern is nested `it.layer`: the outer block shares the container/`PgClient` once (`memoMap` reuses it across sibling blocks), an inner `it.layer` extends context with the folder service under test (`SqlBoundary`, `ObjectStore`, `Authn`, `RunnerBackplane`), and each `it.effect`/`it.scoped` runs against the real, shared substrate. A services spec never constructs a container or a client in `beforeAll` and threads it by hand — that is the wrapper the package exists to delete.

## [03]-[GENERIC_SURFACE_REFERENCE]

The generic runner surface is the canonical owner's, not restated here: the collector families (`it.effect`/`scoped`/`live`/`scopedLive`), the `.skip`/`.skipIf`/`.runIf`/`.only`/`.each`/`.fails`/`.prop` modifiers, the `it.layer` harness, `it.prop`/`it.flakyTest`, `addEqualityTesters`, the `vitest` re-export, the full `Vitest` runner type tree (`Methods`/`MethodsNonLive`/`Tester`/`Arbitraries` + the root `API`/`TestCollectorCallable`), and the `@effect/vitest/utils` assertion signatures are documented as `ts contract` blocks at `../../.api/effect-vitest.md`. A services harness composes against that runner contract and calls those `./utils` assertions directly; this page adds only the services-domain layer stacking below it.

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `../../.api/effect-vitest.md` (canonical owner): the generic collector family, the `it.layer`/`it.prop`/`it.flakyTest` semantics, `TestClock`/`TestRandom` `TestServices`, and the `vitest` re-export are the branch tier's; this overlay adds only the services layer composition. `expect`/`describe` and the Effect runner import from `@effect/vitest` so `addEqualityTesters` and the runner share one module instance.
- `@effect/workflow` `WorkflowEngine.layerMemory` (`effect-workflow.md`): the deterministic durable-execution test engine — `it.layer(WorkflowEngine.layerMemory)` shares it; `TestClock.adjust` drives `DurableClock.sleep` so a month-long durable wait resolves instantly. Production wires `ClusterWorkflowEngine.layer`; the spec swaps only the engine `Layer`, never the workflow body.
- `@effect/sql` `Migrator` + `SqlClient` (`effect-sql.md`, `persistence/store`): the persistence harness is `it.layer(pgContainerLayer)` → `it.layer(SqlBoundary.layer)` with `Migrator.run` once per block; `it.effect` specs exercise `Model.Class` repositories, RLS tenancy, the outbox relay, and `SqlClient.reactive` against a real Postgres, not a mock.
- `@effect/experimental` `RateLimiter`/`Reactivity`/`BackingPersistence` (`effect-experimental.md`) over `ioredis` (`ioredis.md`): a `GenericContainer("redis:…")` `Layer` backs the distributed-rate-limit and cross-node-invalidation specs; `it.scoped` owns the connection lifecycle so each test gets a clean keyspace.
- `testcontainers` `GenericContainer` (`testcontainers.md`): each container is `Effect.acquireRelease(Effect.promise(() => new GenericContainer(image).start()), (c) => Effect.promise(() => c.stop()))` exposed as a `Layer`, shared through `it.layer(containerLayer, { timeout })` — the `AsyncDisposable` `StartedTestContainer` stops on scope exit.
- `effect` `Schema`/`Arbitrary`/`Equal` (`../../.api/effect.md`): `it.effect.prop` derives generators from `Schema` via `Arbitrary.make` (one per domain brand); `addEqualityTesters` uses `Equal.equals`; the `./utils` assertions fold `Exit`/`Option`/`Either` results from the domain rails (`DurableFault`, `SaveResult`, `StepOutcome`).

[LOCAL_ADMISSION]:
- Share every container/`PgClient`/engine with `it.layer` (nested, `memoMap`-reused); never build one in `beforeAll` and thread it by hand.
- Default to `it.effect` under `TestServices`; use `it.live`/`it.scopedLive` only where an assertion genuinely depends on wall-clock timing (a real container health probe), and reach for `it.flakyTest` for the outbox/reactive/durable-clock relays.
- Use `it.scoped` for any acquired resource (a `Redis`/`S3`/workflow scope); never leak a `Scope` under a plain `it.effect`.
- Use `it.effect.prop` with `Schema`-derived arbitraries for domain law tests; never a hand-written example array where a brand's `Schema` already generates the domain.
- Prefer the `./utils` `assertSuccess`/`assertFailure`/`assertSome`/`assertLeft` over `._tag` unwrapping; call `addEqualityTesters()` once in the services proof setup.

[RAIL_LAW]:
- Package: `@effect/vitest` (services overlay; generic owner = `../../.api/effect-vitest.md`)
- Owns (here): the services test-harness stacking — `it.layer` over the durable engine / testcontainers Postgres+Redis / the one `PgClient`+`Migrator`, `it.effect.prop` over `Schema` domain arbitraries, `it.flakyTest` for the timing relays, and the `Data`/`Schema` structural-equality setup
- Accept: `it.layer(WorkflowEngine.layerMemory)` for durable specs, `it.layer(containerLayer)` for real-substrate integration, `it.scoped` for resources, `it.effect.prop` over branded-domain generators, the `./utils` Effect-data assertions
- Reject: a duplicate re-documentation of the generic surface owned by the branch tier, `Effect.runPromise` inside a plain `it`, a hand-threaded container/client in `beforeAll` where `it.layer` fits, a `layerMemory` engine passed off as the production durable backend, and `._tag` unwrapping in place of the `./utils` assertions
