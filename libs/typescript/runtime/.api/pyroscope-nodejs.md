# [TS_RUNTIME_API_PYROSCOPE_NODEJS]

`@pyroscope/nodejs` streams continuous wall and heap profiles to a Pyroscope backend over the native `@datadog/pprof` engine, pushing pprof frames on its own cadence outside the OTLP lane.

`init` seats identity, backend, auth, and sampling once at the node root, `start`/`stop` bracket the profiler lifetime, and `wrapWithLabels` bands ambient labels around a synchronous region so samples group on the `service.name` coordinate traces carry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pyroscope/nodejs`
- package: `@pyroscope/nodejs` (Apache-2.0)
- module: dual ESM/CJS, single `.` export; named lifecycle exports beside a default-export object
- runtime: node only — native pprof profiler bindings, no browser lane
- depends: `@datadog/pprof` native wall/heap sampler + `LabelSet`; `source-map` symbolication; optional `express`/`fastify` peers for pull middleware
- rail: observability/profiling — the node continuous-profiling push producer

## [02]-[PUBLIC_TYPES]

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap, the profiler lifecycle, label scoping, and the pull-mode middleware

`init` seats `appName`/`serverAddress`/`tags`; auth rides `authToken` bearer or `basicAuthUser`/`basicAuthPassword` basic with `tenantID`; `wall`/`heap` toggle the two profilers, and `flushIntervalMs` paces the push. Named exports are `init`/`start`/`stop`/`wrapWithLabels`; the remaining surfaces reach through the default-export object.

| [INDEX] | [SURFACE]                                                                       | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------ | :------ | :--------------------------------------- |
|  [01]   | `init(PyroscopeConfig?) -> void`                                                | static  | seat identity, backend, auth, sampling   |
|  [02]   | `start() -> void`                                                               | static  | arm wall + heap profilers, begin push    |
|  [03]   | `stop() -> Promise<void>`                                                       | static  | drain both profilers, flush last profile |
|  [04]   | `wrapWithLabels(Record<string,string\|number>, ()=>void, ...unknown[]) -> void` | static  | band ambient labels on a sync region     |
|  [05]   | `startWallProfiling() -> void` / `stopWallProfiling() -> Promise<void>`         | static  | per-profiler wall arm/drain              |
|  [06]   | `startHeapProfiling() -> void` / `stopHeapProfiling() -> Promise<void>`         | static  | per-profiler heap arm/drain              |
|  [07]   | `getLabels() -> LabelSet` / `setLabels(LabelSet) -> void`                       | static  | read/set the wall ambient labels         |
|  [08]   | `expressMiddleware()` / `fastifyMiddleware() -> FastifyPluginCallback`          | factory | mount a pull-mode profile endpoint       |
|  [09]   | `SourceMapper.create(string[], boolean?) -> Promise<SourceMapper>`              | factory | build the symbolicator over roots        |
|  [10]   | `setLogger(Logger) -> void`                                                     | static  | inject the profiler + pprof log sink     |

- `start`/`stop` fan to both profilers — `start` arms wall then heap, `stop` awaits both drains in parallel; a granular row targets one profiler.
- `getLabels`/`setLabels` bind the wall profiler's ambient thread labels; the heap profiler's label ops are no-ops.
- `wrapWithLabels` bands the callback synchronously — samples taken during `fn` carry the labels and `...args` forward to `fn`; an async region escapes the band.
- `SourceMapper.create` returns before `init` seats the mapper; `SourceMapper.hasMappingInfo(string)` and `mappingInfo(GeneratedLocation) -> SourceLocation` examine a built map.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — `init`/`start` seat once at the node root; a library arming the profiler double-samples the native engine.
- push cadence is self-owned — `@datadog/pprof` samples wall and heap through the native pprof engine and pushes pprof frames on `flushIntervalMs`, bypassing the OTLP export lane.
- label identity is one projection — `appName` and `tags` mirror the `AppIdentity` `service.name` spelling, so a profile joins its traces and metrics on one identity coordinate; a free-string label beside the projection splits identity.
- `wrapWithLabels` bands synchronously — the wall profiler tags every sample taken during the callback, so an async region leaves the band unlabeled.
- `stop()` joins the process drain — its `Promise` settles before exit so the final profile flushes to the backend.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `PyroscopeConfig.appName`/`tags` fold from the same `AppIdentity` that `Resource.layer({ serviceName, attributes })` stamps on the shared `Resource.Resource` Tag, so a profile carries the `service.name` traces and metrics already carry — one identity, two transports.
- `@opentelemetry/resources`(`.api/opentelemetry-resources.md`): `PyroscopeConfig.tags` and the `resourceFromAttributes` `service.name` attribute derive from one `AppIdentity`, so a backend correlates a pprof profile with its OTLP signal on the shared coordinate.
- `otel/profile` (within-lib): `Profile.live(policy)` folds the policy into one `PyroscopeConfig` — `authToken` from a `Redacted` unwrapped once, `tags` from `Convention.identity`, `sourceMapper` from `SourceMapper.create(roots)` — brackets `init`/`start` in a `Layer.scopedDiscard` whose release runs `stop()` as a ranked `Life` drain row, and `Profile.banded` scopes `wrapWithLabels` around a schema-decoded band.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane only — `init`/`start`/`stop` live in the node boot and drain graph; the browser and worker lanes carry no profiler.
- push is the default rail — `expressMiddleware`/`fastifyMiddleware` mount a pull endpoint only where a scrape topology owns collection.
- `Setting.otel.profile` carries the backend origin and sealed token; an absent origin leaves the lane unarmed and composes zero profiler code.

[RAIL_LAW]:
- Package: `@pyroscope/nodejs`
- Owns: continuous wall + heap profiling push over the native pprof engine — the `init`/`start`/`stop` lifecycle, `wrapWithLabels` label banding, `SourceMapper` symbolication, and the express/fastify pull middleware
- Accept: one node-root seat pointed at the `Setting.otel`-provisioned backend with `AppIdentity`-projected `tags`, `stop()` on the ranked process drain, synchronous label bands
- Reject: library-altitude `init`, a profile label divergent from the resource identity, a second sampler beside `@datadog/pprof`, an async region inside a `wrapWithLabels` band
