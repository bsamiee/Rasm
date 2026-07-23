# [PY_RUNTIME_API_HISHEL]

`hishel` owns the RFC-9111 HTTP cache above the runtime's `httpx` transport: a transport-agnostic `AsyncCacheProxy` wraps as an `AsyncCacheTransport` mounted at the `httpx` transport seam, or as an `AsyncCacheClient` subclass. `SpecificationPolicy` drives freshness, revalidation, and every stored/served decision through an `AnyState`-tagged state machine over a persistent `AsyncSqliteStorage` keyed store. `httpx` owns the client beneath — its `Timeout`/`Limits`/`Auth` and OTel spans — while this catalog owns only the cache layer above the transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hishel`
- package: `hishel[httpx]` (BSD-3-Clause)
- module: `hishel`, `hishel.httpx`
- rail: transport
- namespaces: `hishel`, `hishel.httpx`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: httpx integration family (`hishel.httpx`)
- Async members are canonical; each sync twin mirrors it over `httpx.Client`/`httpx.BaseTransport`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `AsyncCacheClient`    | client        | `httpx.AsyncClient` subclass popping `storage`/`policy`, else identical |
|  [02]   | `AsyncCacheTransport` | transport     | wraps a `next_transport` with the cache proxy (the injection seam)      |
|  [03]   | `SyncCacheClient`     | client        | sync `httpx.Client` twin (boundary scripts only)                        |
|  [04]   | `SyncCacheTransport`  | transport     | sync transport twin over `httpx.BaseTransport`                          |

[PUBLIC_TYPE_SCOPE]: cache-proxy core (`hishel`)
- transport-agnostic: one core drives any `request_sender` callable, so it backs every integration behind the httpx surface.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `AsyncCacheProxy`  | proxy         | `(request_sender, storage, policy)` -> `handle_request` core   |
|  [02]   | `SyncCacheProxy`   | proxy         | sync twin over a synchronous `request_sender`                  |
|  [03]   | `Request`          | message       | hishel-internal request the proxy transports (httpx-decoupled) |
|  [04]   | `Response`         | message       | hishel-internal response with cache decoration                 |
|  [05]   | `Headers`          | value         | case-insensitive header multimap on the internal messages      |
|  [06]   | `RequestMetadata`  | value         | request-side metadata mapping on an `Entry`                    |
|  [07]   | `ResponseMetadata` | value         | response-side metadata mapping on an `Entry`                   |

[PUBLIC_TYPE_SCOPE]: storage family
- `AsyncBaseStorage` is the async storage protocol every backend satisfies; the sqlite arm is the persistent local store, redis the server-backed arm.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `AsyncSqliteStorage` | storage       | `anysqlite`-backed keyed store; `database_path`, `default_ttl`, refresh  |
|  [02]   | `SyncSqliteStorage`  | storage       | stdlib `sqlite3`-backed sync twin                                        |
|  [03]   | `AsyncRedisStorage`  | storage       | `redis` client store; `ttl`, `key_prefix`, `soft_delete_ttl`, stream cap |
|  [04]   | `RedisStorage`       | storage       | sync redis twin                                                          |
|  [05]   | `AsyncBaseStorage`   | protocol      | async storage ABC every backend satisfies                                |
|  [06]   | `SyncBaseStorage`    | protocol      | sync storage ABC                                                         |

[PUBLIC_TYPE_SCOPE]: policy family
- `CachePolicy` is the ABC the proxy consults per request; `SpecificationPolicy` is the default, `FilterPolicy` the allow/deny gate.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `CachePolicy`         | policy base   | ABC carrying `use_body_key`; drives the state transitions per request      |
|  [02]   | `SpecificationPolicy` | policy        | RFC-9111 freshness/revalidation over a `CacheOptions` (`use_body_key` opt) |
|  [03]   | `FilterPolicy`        | policy        | `request_filters`/`response_filters` allow-deny over `BaseFilter` lists    |
|  [04]   | `BaseFilter`          | filter ABC    | generic `needs_body()`/`apply(item, body)` predicate                       |
|  [05]   | `CacheOptions`        | config        | `shared`, `supported_methods`, `allow_stale` — the spec-policy tuning      |

[PUBLIC_TYPE_SCOPE]: cache-state union (`AnyState`)
- `State.next` advances the machine; the union tags every stored/served decision the proxy reaches.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `State`             | state base    | base carrying the `next` transition             |
|  [02]   | `IdleClient`        | state         | pre-lookup entry state                          |
|  [03]   | `CacheMiss`         | state         | no usable entry; forward to origin              |
|  [04]   | `FromCache`         | state         | fresh entry served without origin contact       |
|  [05]   | `NeedRevalidation`  | state         | conditional revalidation against stored entries |
|  [06]   | `NeedToBeUpdated`   | state         | `304` refresh of stored entry metadata          |
|  [07]   | `StoreAndUse`       | state         | store the origin response, then serve it        |
|  [08]   | `CouldNotBeStored`  | state         | response is non-cacheable; served uncached      |
|  [09]   | `InvalidateEntries` | state         | unsafe-method invalidation of stored entries    |

[PUBLIC_TYPE_SCOPE]: entry model

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `Entry`     | model         | `id`, `request`, `meta`, `response`, `cache_key`, `extra` stored row |
|  [02]   | `EntryMeta` | model         | `created_at`/`deleted_at` soft-delete metadata                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cache-transport injection (canonical runtime seam)
- `AsyncCacheTransport` wraps the runtime's `AsyncHTTPTransport`; the long-lived `AsyncClient` keeps its `Timeout`/`Limits`/`Auth` and mounts it.

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `AsyncCacheTransport(next_transport, storage, policy)` | build   | wrap a transport with the cache proxy (injection seam) |
|  [02]   | `AsyncCacheTransport.handle_async_request(request)`    | send    | proxy a request through lookup/store/revalidate        |
|  [03]   | `AsyncCacheTransport.aclose()`                         | drain   | close the wrapped transport and storage                |
|  [04]   | `AsyncCacheClient(*args, storage=, policy=, **kwargs)` | build   | drop-in `AsyncClient` subclass carrying the cache      |

[ENTRYPOINT_SCOPE]: storage construction and lifecycle
- Methods are defined on `AsyncSqliteStorage`; the base protocol methods are the drain/inspection surface.
- `AsyncSqliteStorage` ctor carry: `connection`, `database_path`, `default_ttl`, `refresh_ttl_on_access`.

| [INDEX] | [SURFACE]                                           | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `AsyncSqliteStorage(*, database_path, default_ttl)` | build   | open the store at its admitted cache path        |
|  [02]   | `.create_entry(...)` / `.get_entries(...)`          | store   | write and read stored entries by cache key       |
|  [03]   | `.update_entry(...)` / `.remove_entry(...)`         | store   | refresh metadata and evict                       |
|  [04]   | `.refresh_entry_ttl(...)`                           | store   | extend a served entry's TTL on access            |
|  [05]   | `.mark_pair_as_deleted(...)` / `.is_soft_deleted`   | store   | soft-delete lifecycle and its predicate          |
|  [06]   | `.is_safe_to_hard_delete(...)`                      | store   | hard-eviction guard after the soft-delete window |
|  [07]   | `.close()`                                          | drain   | close the store on host drain                    |

[ENTRYPOINT_SCOPE]: policy construction

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------ | :------ | :----------------------------------------------- |
|  [01]   | `SpecificationPolicy(cache_options=CacheOptions(...))`        | build   | RFC-9111 freshness/revalidation policy           |
|  [02]   | `CacheOptions(shared, supported_methods, allow_stale)`        | build   | shared/private mode, cached methods, stale serve |
|  [03]   | `FilterPolicy(request_filters=[...], response_filters=[...])` | build   | allow-deny gate over `BaseFilter` predicates     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- injection law: the cache rides the `httpx` transport seam, never a swapped client class — the long-lived `AsyncClient` keeps its `Timeout`/`Limits`/`Auth`/`base_url` and mounts `transport=AsyncCacheTransport(next_transport=AsyncHTTPTransport(...))`. `AsyncCacheClient` serves only boundary scripts owning no pre-built transport.
- store law: `AsyncSqliteStorage` is the persistent local store; each app passes an owner-derived `database_path` from its cache root, and distinct instances isolate app cache rows. A multi-tenant app scopes the store per tenant through distinct `database_path` or `key_prefix`, and redis is the server-backed arm composed only where a redis host exists.
- policy law: `SpecificationPolicy` (RFC-9111) is the default; freshness, conditional revalidation, and `304` refresh are owned by the state machine, never a hand-rolled `Cache-Control`/`ETag` parse. `CacheOptions(shared=)` selects shared vs private semantics and `allow_stale=` gates stale-on-error serving.
- state law: cache decisions surface as the `AnyState` union; the runtime reads the reached state for telemetry rather than inferring hit/miss from headers.
- key law: request identity is the default cache key; `CachePolicy.use_body_key` opts the request body into the key for body-varying endpoints only.
- drain law: storage `.close()` and transport `aclose()` run in the host drain under the anyio lane; the sqlite connection is never left to GC.

[STACKING]:
- `AsyncCacheTransport(next_transport=AsyncHTTPTransport(...), storage=AsyncSqliteStorage(database_path=...), policy=SpecificationPolicy(CacheOptions(shared=...)))` mounts on the runtime `AsyncClient`: one transport stack, cache above transport above pool.
- `httpx`(`.api/httpx.md`): the served `httpx.Response` is byte-identical to the origin's, so its `Response.json()` feeds the same `msgspec.convert`/pydantic wire-model decoder the `httpx` rail already owns — the cache adds no decode seam.
- `opentelemetry-instrumentation-httpx`(`.api/opentelemetry-instrumentation-httpx.md`): the httpx client span wraps the transport beneath the cache, so a `FromCache` served response carries no origin span, and the runtime reads its `AnyState` tag onto the transport receipt rather than a separate hit/miss counter.

[LOCAL_ADMISSION]:
- One `AsyncCacheTransport` wraps the one `AsyncHTTPTransport`; the runtime owns no second cache and no parallel per-mode caching client.
- Revalidation is owned by `SpecificationPolicy` beside the content-keyed recipe lanes; the two owners never overlap — hishel caches HTTP responses, the recipe owner caches computed artifacts.
- OTel `opentelemetry-instrumentation-httpx` spans the underlying transport beneath the cache; a `FromCache` state short-circuits the origin span while the cache decision rides the receipt.

[RAIL_LAW]:
- Package: `hishel[httpx]`
- Owns: RFC-9111 HTTP caching over `httpx`, the transport-agnostic cache proxy, spec and filter policies, the `AnyState` stored/served union, and persistent sqlite / redis keyed storages behind one async storage protocol
- Accept: `AsyncCacheTransport` injected at the transport seam, `AsyncSqliteStorage` with a required owner-derived `database_path`, `SpecificationPolicy`/`CacheOptions` for freshness and stale-serve, per-app store isolation, `AnyState`-tagged decisions on the receipt, `.close()`/`aclose()` on drain
- Reject: a swapped `AsyncCacheClient` where a pre-built transport exists, a filesystem store, redis composed without a host, hand-rolled `Cache-Control`/revalidation parsing, a process-global or shared-mutable cache store, overlap with the content-keyed recipe cache
