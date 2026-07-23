# [RASM_PERSISTENCE_API_PG_NET]

`pg_net` owns in-database asynchronous HTTP/HTTPS: a request function enqueues and hands back its `bigint` id inside the calling transaction, a `libcurl` background worker drives the transfer off that backend, and the response lands in `net._http_response` keyed by the same id. Every surface is SQL, so the outbound call fires from the database tier and no backend holds the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_net`
- package: `pg_net` (Apache-2.0)
- namespace: SQL `net` schema; `pg_net.*` GUC namespace
- rail: http-provisioning, http-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: request and response vocabulary of the `net` schema.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `net.http_method`          | domain         | `get` `post` `delete`, case-insensitive  |
|  [02]   | `net.request_status`       | enum           | `PENDING` `SUCCESS` `ERROR` discriminant |
|  [03]   | `net.http_response`        | composite      | one response value                       |
|  [04]   | `net.http_response_result` | composite      | status-wrapped collect result            |
|  [05]   | `net.http_request_queue`   | unlogged table | pending requests the worker drains       |
|  [06]   | `net._http_response`       | unlogged table | response store keyed by request id       |

- `net.http_response` `(status_code int, headers jsonb, body text)` rides `net.http_response_result` `(status net.request_status, message text, response net.http_response)`; `body` carries the table's `content` column.
- `net.http_request_queue` `(id bigserial, method net.http_method, url text, headers jsonb, body bytea, timeout_milliseconds int)` drains into `net._http_response` `(id bigint, status_code int, content_type text, headers jsonb, content text, timed_out bool, error_msg text, created timestamptz)`, indexed on `created`; a failed transfer nulls every response column and states its cause in `error_msg`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: enqueue, collect, and worker control; every request function url-encodes `params` onto the url and defaults `timeout_milliseconds` to `5000`.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `net.http_get(text, jsonb, jsonb, int) -> bigint`                      | function | enqueue a GET                           |
|  [02]   | `net.http_post(text, jsonb, jsonb, jsonb, int) -> bigint`              | function | enqueue a POST carrying a `jsonb` body  |
|  [03]   | `net.http_delete(text, jsonb, jsonb, int, jsonb) -> bigint`            | function | enqueue a DELETE, body optional         |
|  [04]   | `net._http_collect_response(bigint, bool) -> net.http_response_result` | function | read one response wrapped in its status |
|  [05]   | `net.wake() -> void`                                                   | function | arm the commit-time worker wake         |
|  [06]   | `net.worker_restart() -> bool`                                         | function | reload config and restart the worker    |
|  [07]   | `net.wait_until_running() -> void`                                     | function | block until the worker reaches running  |
|  [08]   | `net.check_worker_is_up() -> void`                                     | function | raise when no `pg_net` backend is up    |
|  [09]   | `pg_net.ttl`                                                           | guc      | max lifetime of a response row          |
|  [10]   | `pg_net.batch_size`                                                    | guc      | queue rows per worker iteration         |
|  [11]   | `pg_net.database_name`                                                 | guc      | the one database the worker connects to |
|  [12]   | `pg_net.username`                                                      | guc      | the worker connection role              |

- `net.http_post` raises unless `Content-Type` is `application/json`, and auto-injects that header when the caller omits it.
- `net._http_collect_response` blocks on a 50 ms poll at `async => false` and never emits `PENDING` — an absent row and a failed transfer both return `ERROR`, so an in-flight request reads as an unknown id.
- `net.wake` registers one commit callback per transaction, so a rollback never wakes the worker and a prepared transaction needs a manual call.
- GUC binding: `pg_net.ttl` (`'6 hours'`) and `pg_net.batch_size` (`200`) reload on `SIGHUP`; `pg_net.database_name` (`'postgres'`) and `pg_net.username` (unset selects the bootstrap superuser) fix at backend start.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `_PG_init` hard-errors unless `pg_net` sits on `shared_preload_libraries`, so its worker registers at postmaster start and install rides the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension("pg_net", PreloadGated: true)` row whose `CreateSql` the `Migrate` fold emits.
- A request function returns its id inside the transaction and the transfer starts at COMMIT, so a rolled-back enqueue never sends and the response is read later by id, never awaited on the calling backend.
- One worker per cluster binds to `pg_net.database_name`, drains `net.http_request_queue` by `DELETE ... RETURNING` bounded by `pg_net.batch_size`, and purges `net._http_response` past `pg_net.ttl` on the same iteration.
- Both `net` tables are UNLOGGED, so a crash truncates queued requests and landed responses alike.

[STACKING]:
- `Npgsql`(`.api/api-npgsql.md`), `Npgsql.EntityFrameworkCore.PostgreSQL`(`.api/api-npgsql-ef.md`): url, `params`, `headers`, `body`, and `timeout_milliseconds` bind as `NpgsqlParameter` values through `FromSql`/`RelationalDatabaseFacadeExtensions.SqlQuery<T>`; no EF translator covers the `net` schema.
- `Version/egress#EGRESS_SINK`: `EgressSink.Webhook` enqueues `net.http_post` under the content key as idempotency header and folds `net.http_response_result` on the NEXT drain — `SUCCESS` advances the cursor, `ERROR` and timeout refuse, an unresolved row holds it as `EgressFault.DeliveryUnconfirmed`.
- `Version/egress#EGRESS_PUMP`: retriability reads three `net._http_response` columns directly — a `5xx` `status_code`, a NULL `status_code` with its transport cause in `error_msg`, and `timed_out = true` re-enqueue through the request function, while a `4xx` dead-letters.
- `Store/provisioning#SERVER_EXTENSIONS`: `ClusterConfig` verifies `pg_net.ttl` and `pg_net.batch_size` read-only against `pg_settings` after boot, and its verification fold calls `net.check_worker_is_up`/`net.wait_until_running` for liveness.

[LOCAL_ADMISSION]:
- Admission binds on a deploy image carrying `pg_net` on `shared_preload_libraries`; `FailureRank.Observational` degrades the webhook egress lane rather than failing provisioning.

[RAIL_LAW]:
- Package: `pg_net`
- Owns: in-database asynchronous HTTP — the enqueue functions, the `libcurl` worker, and the `net._http_response` store
- Accept: commit-scoped enqueue returning a `bigint` id with every value bound through `Npgsql`, the response read by id off `net._http_response` or `net._http_collect_response`, the `pg_net.*` GUCs verified read-only, and the worker-control probes
- Reject: an inline-awaited HTTP call holding the calling backend, and a direct `net.http_request_queue` INSERT that never calls `net.wake`
