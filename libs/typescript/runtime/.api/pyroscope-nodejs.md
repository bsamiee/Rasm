# [TS_RUNTIME_API_PYROSCOPE_NODEJS]

`@pyroscope/nodejs` streams continuous wall and heap profiles to a Pyroscope backend over the native `@datadog/pprof` engine, pushing pprof frames on its own cadence outside the OTLP lane.

`init` seats identity, backend, auth, and sampling once at the node root, `start`/`stop` bracket the profiler lifetime, and `wrapWithLabels` bands ambient labels around a synchronous region so samples group on the `service.name` coordinate traces carry.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `init` config family, the label-map type, and the sourcemap symbolicator

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `PyroscopeConfig`     | interface     | the whole `init` knob surface — identity, backend, auth |
|  [02]   | `PyroscopeWallConfig` | interface     | wall sampling knobs — duration, interval, cpu-time      |
|  [03]   | `PyroscopeHeapConfig` | interface     | heap sampling knobs — interval bytes, stack depth       |
|  [04]   | `StripFilenamesMode`  | union         | `'all' \| 'dependencies'` path-stripping selector       |
|  [05]   | `LabelSet`            | type          | `@datadog/pprof` key/value label map samples carry      |
|  [06]   | `SourceMapper`        | class         | sourcemap symbolicator resolving transpiled frames      |
|  [07]   | `Logger`              | interface     | six-level log sink `setLogger` injects                  |

- `PyroscopeConfig`: `appName?` `serverAddress?` `authToken?` `basicAuthUser?` `basicAuthPassword?` `tenantID?` `flushIntervalMs?` `tags?: LabelSet` `wall?` `heap?` `sourceMapper?` `stripFilenames?: StripFilenamesMode` `shortenPaths?`
- `PyroscopeWallConfig`: `samplingDurationMs?` `samplingIntervalMicros?` `collectCpuTime?`; `PyroscopeHeapConfig`: `samplingIntervalBytes?` `stackDepth?`
- Rows [01]-[03] re-export from the root ALONE. `StripFilenamesMode`, `LabelSet`, `SourceMapper`, and `Logger` declare in their own modules and reach no root specifier, so an import of the bare name resolves nowhere — each reads off the surface carrying it: `NonNullable<PyroscopeConfig["stripFilenames"]>`, `NonNullable<PyroscopeConfig["tags"]>`, `InstanceType<typeof Pyroscope.SourceMapper>`, `Parameters<typeof Pyroscope.setLogger>[0]`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap, the profiler lifecycle, label scoping, and the pull-mode middleware

`init` seats `appName`/`serverAddress`/`tags`; auth rides `authToken` bearer or `basicAuthUser`/`basicAuthPassword` basic with `tenantID`; `wall`/`heap` toggle the two profilers, and `flushIntervalMs` paces the push. Named VALUE exports are `init`/`start`/`stop`/`wrapWithLabels`; every other surface — the per-profiler pairs, the label ops, `SourceMapper`, `setLogger`, and the middleware factories — reaches through the default-export object alone.

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------ | :------ | :--------------------------------------- |
|  [01]   | `init(PyroscopeConfig?) -> void`                                                | static  | seat identity, backend, auth, sampling   |
|  [02]   | `start() -> void`                                                               | static  | arm wall + heap profilers, begin push    |
|  [03]   | `stop() -> Promise<void>`                                                       | static  | drain both profilers, flush last profile |
|  [04]   | `wrapWithLabels(Record<string,string\|number>, ()=>void, ...unknown[]) -> void` | static  | band ambient labels on a sync region     |
|  [05]   | `startWallProfiling() -> void` / `stopWallProfiling() -> Promise<void>`         | static  | per-profiler wall arm/drain              |
|  [06]   | `startHeapProfiling() -> void` / `stopHeapProfiling() -> Promise<void>`         | static  | per-profiler heap arm/drain              |
|  [07]   | `startCpuProfiling()` / `stopCpuProfiling()`                                    | static  | DEPRECATED ALIAS — the wall pair renamed |
|  [08]   | `getWallLabels()` / `setWallLabels()` / `getLabels()` / `setLabels(LabelSet)`   | static  | read/set the wall ambient labels         |
|  [09]   | `expressMiddleware()` / `fastifyMiddleware() -> FastifyPluginCallback`          | factory | mount a pull-mode profile endpoint       |
|  [10]   | `SourceMapper.create(string[], boolean?) -> Promise<SourceMapper>`              | factory | build the symbolicator over roots        |
|  [11]   | `setLogger(Logger) -> void`                                                     | static  | inject the profiler + pprof log sink     |

- TWO native samplers exist, wall and heap. `start` arms both and `stop` awaits both drains in parallel, so the per-profiler roster buys SELECTION — arming one engine alone — and never a third engine. The `cpu` pair is a backwards-compatibility alias whose start and stop resolve the wall profiler itself, so seating it beside the wall row arms one engine twice: the second arm is a logged no-op against the already-running sampler and the second drain finds nothing to drain, with which row gets the real drain decided by release order alone.
- Every label op, start, and stop resolves the singleton profiler, which `init` seats — so ANY of them reached before a successful `init` THROWS `Pyroscope is not configured` rather than no-opping. That reaches `wrapWithLabels` too, which is what makes a label band on an unarmed process a defect rather than a silent pass-through; a composing fence gates the band on its own record of the seat. A stop on a started-then-idle profiler returns immediately, and registering a drain ahead of the arm turns a construction failure into a shutdown fault, so an arm and its stop belong to one bracket. Stopping past a live arm still rejects on a failed final push, since the last export awaits before the profiler's own capture guard.
- Both arms are IDEMPOTENT — the continuous profiler guards on its own flush timer and the wall profiler additionally guards on the native engine's started flag, so a second start logs and returns rather than double-sampling.
- `Logger` declares six members — `error`/`warn`/`info`/`debug`/`trace`/`fatal` — each `(...args: Array<{}>) => void`, so a bridge spells a mutable rest array; `setLogger` installs the sink on both this package and `@datadog/pprof`.
- `getLabels`/`setLabels` bind the wall profiler's ambient thread labels; the heap profiler's label ops are no-ops.
- `wrapWithLabels` bands the callback synchronously — samples taken during `fn` carry the labels and `...args` forward to `fn`; an async region escapes the band. The band MERGES over the ambient set and restores that prior set after `fn` returns, so nesting composes — but the restore sits outside any `finally`, so a callback throwing through the band leaks its labels onto every later sample on that thread and nothing else rewrites them. A composing fence catches inside the callback and re-raises outside it.
- `SourceMapper.create` returns before `init` seats the mapper; `SourceMapper.hasMappingInfo(string)` and `mappingInfo(GeneratedLocation) -> SourceLocation` examine a built map.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — `init`/`start` seat once at the node root; a library arming the profiler double-samples the native engine.
- push cadence is self-owned — `@datadog/pprof` samples wall and heap through the native pprof engine and pushes pprof frames on `flushIntervalMs`, bypassing the OTLP export lane.
- label identity is one projection — `appName` and `tags` mirror the `AppIdentity` `service.name` spelling, so a profile joins its traces and metrics on one identity coordinate; a free-string label beside the projection splits identity.
- `wrapWithLabels` bands synchronously — the wall profiler tags every sample taken during the callback, so an async region leaves the band unlabeled.
- `stop()` joins the process drain — its `Promise` settles before exit so the final profile flushes to the backend.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `PyroscopeConfig.appName`/`tags` fold from the same `AppIdentity` that `Resource.layer({ serviceName, attributes })` stamps on the shared `Resource.Resource` Tag, so a profile carries the `service.name` traces and metrics already carry — one identity, two transports.
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `PyroscopeConfig.tags` and the `resourceFromAttributes` `service.name` attribute derive from one `AppIdentity`, so a backend correlates a pprof profile with its OTLP signal on the shared coordinate.
- `otel/profile` (within-lib): `Profile.live(policy)` folds the policy into one `PyroscopeConfig` — the tagged credential arm's own fields from a `Redacted` unwrapped once, `tags` from `Convention.profiled`, `sourceMapper` from `SourceMapper.create(roots)` — arms each rostered sampler as its own `acquireRelease` extended into the child scope a ranked `Life` row closes, bridges `setLogger` onto the Effect logger, and `Profile.banded` scopes `wrapWithLabels` around a schema-decoded band whose parser is held per vocabulary.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane only — `init`/`start`/`stop` live in the node boot and drain graph; the browser and worker lanes carry no profiler.
- push is the default mode — `expressMiddleware`/`fastifyMiddleware` mount a pull endpoint only where a scrape topology owns collection.
- `Setting.otel.profile` carries the backend origin and the sealed credential — bearer or basic pair; an absent origin leaves the lane unarmed and composes zero profiler code.
