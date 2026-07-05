# [RASM_PERSISTENCE_API_PG_NET]

`pg_net` supplies asynchronous, non-blocking HTTP/HTTPS from SQL — `net.http_get`/`http_post`/
`http_delete` enqueue a request and return a `bigint` request-id immediately, a `libcurl` background
worker drives the I/O off the calling backend, and the response lands in `net._http_response` keyed by
that id. It carries no managed assembly: every surface is server-side SQL the
`Store/provisioning#SERVER_EXTENSIONS` `ServerExtension("pg_net")` row installs and a server-local
webhook/HTTP egress consumer drives through raw `Npgsql`/`FromSql`/`SqlQuery`, so an in-DB outbound
call beside the process-side `Version/egress#EGRESS_SINK` sinks fires without blocking the transaction.
The extension IS preload-gated — its worker is registered statically in `_PG_init`, so it REQUIRES
`pg_net` on the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row and hard-errors on
`CREATE EXTENSION` otherwise.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_net`
- package: server-side PostgreSQL extension (C/`libcurl`, not a NuGet package); repo `supabase/pg_net`, version line `0.20.x`
- namespace: SQL `net` schema (the request functions, the `_http_response` table, the response composite types, the worker-control functions)
- license: Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- registration: preload-gated — the worker is `RegisterBackgroundWorker`'d in `_PG_init` at postmaster start, so `pg_net` MUST be on the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row; the `ServerExtension("pg_net", PreloadGated: true)` row emits its `CreateSql` through `Migrate` (preload prereq cannot ride `HasPostgresExtension`)
- consumed by: a server-local webhook/HTTP egress beside `Version/egress#EGRESS_SINK`/`#EGRESS_PUMP`, driven through raw `Npgsql`
- dependency: `libcurl >= 7.83.0`; one worker per cluster, bound to one database via `pg_net.database_name`
- rail: http-provisioning, http-egress

## [02]-[REQUEST_FUNCTIONS]

Each function url-encodes `params`, inserts one row into `net.http_request_queue`, registers a
commit-time wake callback, and returns the queue id as a `bigint` request-id. The request is NOT
started until the enqueuing transaction COMMITs (the wake callback fires on `XACT_EVENT_COMMIT`), and
the timeout default is `5000` ms for all three.

| [INDEX] | [FUNCTION]         | [SIGNATURE]                                                                                                              | [SEMANTICS]                              |
| :-----: | :----------------- | :--------------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `net.http_get`     | `net.http_get(url text, params jsonb => '{}', headers jsonb => '{}', timeout_milliseconds int => 5000)` → `bigint`      | enqueue a GET; returns the request-id    |
|  [02]   | `net.http_post`    | `net.http_post(url text, body jsonb => '{}', params jsonb => '{}', headers jsonb => '{"Content-Type": "application/json"}', timeout_milliseconds int => 5000)` → `bigint` | enqueue a POST; body is JSON             |
|  [03]   | `net.http_delete`  | `net.http_delete(url text, params jsonb => '{}', headers jsonb => '{}', timeout_milliseconds int => 5000, body jsonb => NULL)` → `bigint` | enqueue a DELETE (optional body)         |

`net.http_post` enforces `Content-Type: application/json` — it auto-injects the header when omitted and
raises if it is set to anything else. There is no `net.http_put` or `net.http_head`: the `net.http_method`
domain admits only `get`/`post`/`delete` (case-insensitive), so a PUT/HEAD spelling is the rejected form.

## [03]-[RESPONSE_MODEL]

The response lands in the `net._http_response` UNLOGGED table keyed by the request-id; the body column
is `content` on the table but `body` on the composite type. The collector wraps a single row into the
`net.http_response_result` composite.

| [INDEX] | [OBJECT]                       | [SHAPE]                                                                                          | [SEMANTICS]                              |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `net._http_response`           | `(id bigint, status_code int, content_type text, headers jsonb, content text, timed_out bool, error_msg text, created timestamptz)` | UNLOGGED response store, indexed on `created` |
|  [02]   | `net.http_response`            | composite `(status_code int, headers jsonb, body text)`                                          | the response value the collector returns |
|  [03]   | `net.request_status`           | enum `('PENDING','SUCCESS','ERROR')`                                                             | collector status discriminant            |
|  [04]   | `net.http_response_result`     | composite `(status net.request_status, message text, response net.http_response)`               | the wrapped collect result               |
|  [05]   | `net.http_collect_response`    | `net.http_collect_response(request_id bigint, async boolean => true)` → `net.http_response_result` | DEPRECATED — delegates to `net._http_collect_response` |
|  [06]   | `net._http_collect_response`   | `net._http_collect_response(request_id bigint, async boolean => true)` → `net.http_response_result` | `async => false` blocks (50 ms poll) until the row lands |

The public `net.http_collect_response` raises a deprecation notice and the implementation lives in the
private `net._http_collect_response`; the catalogued consumer reads `net._http_response` directly or
calls `net._http_collect_response`, never the deprecated wrapper.

## [04]-[QUEUE_WORKER]

The request queue and the background-worker controls. The worker is one per cluster, processes up to
`pg_net.batch_size` rows per iteration, deletes successful queue rows, and opportunistically purges
`net._http_response` rows older than `pg_net.ttl` while processing.

| [INDEX] | [SURFACE]                  | [SHAPE / SIGNATURE]                                                              | [SEMANTICS]                              |
| :-----: | :------------------------- | :------------------------------------------------------------------------------ | :--------------------------------------- |
|  [01]   | `net.http_request_queue`   | UNLOGGED `(id bigserial, method net.http_method, url text, headers jsonb, body bytea, timeout_milliseconds int)` | the request queue; a direct INSERT does NOT wake the worker |
|  [02]   | `pg_net.ttl`               | GUC `string` (interval), default `'6 hours'` (`SIGHUP`)                          | max lifetime of `net._http_response` rows |
|  [03]   | `pg_net.batch_size`        | GUC `int`, default `200` (`SIGHUP`)                                              | max queue rows processed per worker iteration |
|  [04]   | `pg_net.database_name`     | GUC `string`, default `'postgres'` (restart)                                     | the one database the worker connects to  |
|  [05]   | `pg_net.username`          | GUC `string`, default bootstrap user (restart)                                   | the worker connection role               |
|  [06]   | `net.worker_restart`       | `net.worker_restart()` → `bool`                                                  | reload config (`pg_reload_conf`) and restart the worker |
|  [07]   | `net.wait_until_running`   | `net.wait_until_running()` → `void`                                              | block until the worker reaches running   |
|  [08]   | `net.check_worker_is_up`   | `net.check_worker_is_up()` → `void`                                              | raise if no `pg_net` worker backend is present |

## [05]-[USAGE_PATTERN]

The canonical idiom is fire-and-collect: enqueue (the request fires on COMMIT), then read the response
by request-id once it lands. The id is the only join key between the call and its response.

| [INDEX] | [STEP]                                                                                   | [SEMANTICS]                              |
| :-----: | :--------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `SELECT net.http_post('https://host/hook', body => '{"k":"v"}'::jsonb) AS request_id`    | enqueue; returns the `bigint` id (fires on COMMIT) |
|  [02]   | `SELECT * FROM net._http_response WHERE id = <request_id>`                                | poll the response table by id            |
|  [03]   | `SELECT (response).status_code, (response).body FROM net._http_collect_response(<request_id>, async => false)` | block until complete and read the wrapped result |
|  [04]   | `SELECT * FROM net._http_response WHERE status_code >= 500`                               | scan retriable server (`5xx`) responses  |
|  [05]   | `SELECT id, error_msg FROM net._http_response WHERE status_code IS NULL`                  | scan transport failures (DNS/connect) — `status_code` is NULL, the cause lands in `error_msg`, distinct from `timed_out` |

Failure discriminant: the response failure axis is the `Version/egress#EGRESS_PUMP` `EgressDeadLetter`
`Retriable`/`Advances` split read directly off three columns — a `5xx` `status_code`, a
`status_code IS NULL` transport failure (DNS/connection, cause in `error_msg`), and `timed_out = true`
(the `timeout_milliseconds` cancellation) are the retriable rows the pump re-enqueues through the
request function; a `4xx` `status_code` is the permanent client-error row it dead-letters. The pump
discriminates retriability from `net._http_response` directly, never the deprecated collect wrapper.

## [06]-[IMPLEMENTATION_LAW]

[PG_NET_TOPOLOGY]:
- Preload-gated, statically-registered worker: `pg_net`'s background worker is `RegisterBackgroundWorker`'d from `_PG_init` at postmaster start, so it MUST appear on the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row — `_PG_init` hard-errors (`pg_net is not in shared_preload_libraries`) otherwise. It is therefore NOT dynamically registered on `CREATE EXTENSION`; install is `ServerExtension("pg_net", PreloadGated: true)` whose `CreateSql` rides `Store/provisioning#SERVER_EXTENSIONS` `Migrate` (the preload prerequisite cannot ride `HasPostgresExtension`), exactly like the `pg_search` preload-gated row, and the `ClusterConfig` probe verifies `pg_net` read-only against `pg_settings` after boot.
- Commit-scoped, fire-and-collect: a request function returns a `bigint` request-id synchronously but the I/O does not start until the transaction COMMITs (the commit-time wake callback), so a rolled-back transaction never sends; the response is read later from `net._http_response` by id, never awaited inline on the calling backend. The deprecated `net.http_collect_response` wrapper is the rejected spelling — read `net._http_response` directly or call `net._http_collect_response`.
- No managed assembly, no EF translator: the request enqueue and the response read both ride raw `Npgsql`/`FromSql`/`SqlQuery`; the URL, `params`, `headers`, `body`, and `timeout_milliseconds` arrive as `Npgsql` parameters from the egress consumer, never a runtime-concatenated SQL string. `net.http_post` always carries `Content-Type: application/json` (auto-injected or rejected-if-mismatched), and a PUT/HEAD method is unrepresentable in the `net.http_method` domain. The worker is one-per-cluster bound to `pg_net.database_name`, so a second per-database HTTP pump is the rejected form.

[RAIL_LAW]:
- Package: `pg_net` (server-side, in the deploy-image PG18, on `shared_preload_libraries`)
- Owns: in-DB asynchronous non-blocking HTTP/HTTPS — the `net.http_get`/`http_post`/`http_delete` enqueue functions, the `libcurl` background worker, and the `net._http_response` response store
- Accept: `CREATE EXTENSION pg_net` via the preload-gated `ServerExtension("pg_net", PreloadGated: true)` with `pg_net` on the `ClusterConfig` `shared_preload_libraries` row, the request functions returning a `bigint` id with parameters bound through `Npgsql`, the commit-scoped fire-and-collect pattern reading `net._http_response` by id, the `pg_net.ttl`/`batch_size` GUCs verified read-only, `net.worker_restart`/`wait_until_running` worker control
- Reject: linking the extension into managed code, installing without `pg_net` on `shared_preload_libraries` (hard-errors), an inline-awaited synchronous HTTP call on the calling backend, the deprecated `net.http_collect_response` wrapper, a `net.http_put`/`net.http_head` spelling (unrepresentable), a runtime-concatenated request string, a direct `net.http_request_queue` INSERT expecting the worker to wake (only the request functions wake it)
