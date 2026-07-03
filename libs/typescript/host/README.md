# [HOST]

`host` is the W1 process-runtime substrate of `libs/typescript` — the one owner for process-runtime selection: the Node/Bun exec rows, the config provider chain, flag verdicts, the branch-wide net client/channel policy rows, and lifecycle choreography. Distributing config/flags/exec/net policy across folders forks under app growth; this folder keeps them one surface. A bun swap is a Layer selection at the app root, never a fork. `flag/verdict` is runtime-neutral so browser apps evaluate the same verdicts node services do, re-evaluating over a live SSE stream row with verdict cache/stickiness rows on `flag/rollout`; entitlement claims stay in `security/authz`, which consumes verdicts. The folder imports `kernel` only and publishes per-runtime subpath exports (`./server`, `./browser`) so node code is physically unreachable from browser resolution. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[RUNTIME](.planning/exec/runtime.md): the Node | Bun runtime rows — a bun swap is a Layer selection in the app root, never a fork.
- [02]-[PROCESS](.planning/exec/process.md): subprocess execution, signals, and `WorkerRunner` pools.
- [03]-[CLIENT](.planning/net/client.md): the branch-wide `HttpClient` default-policy rows (timeout/retry/proxy) — `ai` providers, `work` runner discovery, and OTLP export compose these.
- [04]-[CHANNEL](.planning/net/channel.md): the Socket/Ndjson channel rows — framed stream transport with backpressure, selected beside client policy by the same consumers.
- [05]-[PROVIDER](.planning/config/provider.md): the `ConfigProvider` chain — env, `doppler run` injection, file, remote.
- [06]-[SCHEMA](.planning/config/schema.md): the typed config schema with validation-at-boot.
- [07]-[VERDICT](.planning/flag/verdict.md): runtime-neutral `FlagVerdict` evaluation over the shared OpenFeature contract, with the live verdict/config SSE stream row (remote provider re-evaluation); decode transits `wire` `codec/flag`.
- [08]-[ROLLOUT](.planning/flag/rollout.md): rollout/targeting policy rows plus verdict cache/stickiness rows.
- [09]-[CYCLE](.planning/life/cycle.md): startup/shutdown/drain choreography.
- [10]-[HEALTH](.planning/life/health.md): the readiness/liveness probe vocabulary.

## [2]-[DOMAIN_PACKAGES]

None. `host` admits no folder-local packages — the folder rides the substrate platform bindings; every exec, net, config, flag, and life row composes `effect` and the `@effect/platform` family. A folder-local admission lands here from the folder's ideas and tasks with its catalogue at `.api/`.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry lives in `libs/typescript/.planning/README.md` with catalogues at `libs/typescript/.api/`.

- `effect` — rails, `Schema`, `Layer`, `Match`, `Stream`, and the `Config`/`ConfigProvider` vocabulary the config chain owns.
- `@effect/platform` — the platform service contracts: the `HttpClient`, Socket, worker, and command-execution surfaces the exec and net rows type against.
- `@effect/platform-node` — the Node binding behind the `exec/runtime` Node row.
- `@effect/platform-bun` — the Bun binding behind the `exec/runtime` Bun row.
- `@effect/platform-browser` — the browser binding the runtime-neutral `flag/verdict` evaluation and the `./browser` subpath ride.
