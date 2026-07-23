# [TS_RUNTIME_API_PYROSCOPE_NODEJS]

`@pyroscope/nodejs` streams continuous CPU-wall and heap profiles to a Pyroscope backend: `init(config)` seats the app identity and server address, `start()` arms the wall and heap profilers, and `stop()` drains them on shutdown. It rides the `@datadog/pprof` native profiler — a prebuilt binary loads first, a source build is the fallback — so pprof-format samples carry the process labels the backend groups on. Node composition root is its only seat; `wrapWithLabels` scopes label bands around a workload, and a library never calls `init`/`start`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pyroscope/nodejs`
- package: `@pyroscope/nodejs` (Apache-2.0)
- backing: `@datadog/pprof` native wall + heap sampler (prebuilt-first), `pprof-format` sample encoding, `source-map` symbolication
- consumed-by: the node composition root's profiling seat; the deploy plane provisions the target backend
- runtime: node only — native profiler bindings, no browser row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: init config + profiler sub-configs + label shapes
- rail: observability/profiling
- `PyroscopeConfig` is the whole `init` knob surface; `wall` and `heap` are its per-profiler sub-configs, and `tags` carries the `@datadog/pprof` `LabelSet`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                   |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `PyroscopeConfig`      | config        | the `init(config)` surface — identity, backend, auth  |
|  [02]   | `PyroscopeWallConfig`  | config        | wall sampling — duration, interval, `collectCpuTime`  |
|  [03]   | `PyroscopeHeapConfig`  | config        | heap sampling — `samplingIntervalBytes`, `stackDepth` |
|  [04]   | `LabelSet` / `TagList` | label map     | `@datadog/pprof` key/value labels the samples carry   |
|  [05]   | `StripFilenamesMode`   | policy value  | `'all'` \| `'dependencies'` path-stripping selector   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap, lifecycle, label scoping, pull middleware
- rail: observability/profiling
- `init` seats `appName`/`serverAddress`/`tags`; auth rides `authToken` (bearer) or `basicAuthUser`/`basicAuthPassword` (basic) with `tenantID`; `wall`/`heap` toggle the two profilers; `flushIntervalMs` paces the push.
- Named exports are `init`/`start`/`stop`/`wrapWithLabels`; rows [05]-[08] are members of the default export object.

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]     | [CONSUMER_BOUNDARY]                         |
| :-----: | :---------------------------------------------------- | :----------------- | :------------------------------------------ |
|  [01]   | `init(config?)`                                       | bootstrap          | seats identity, backend address, tags, auth |
|  [02]   | `start()`                                             | lifecycle          | arms wall + heap profilers, begins the push |
|  [03]   | `stop()` → `Promise<void>`                            | lifecycle          | drains profilers, flushes the last profile  |
|  [04]   | `wrapWithLabels(lbls, fn, ...args)`                   | label scope        | bands labels around a workload region       |
|  [05]   | `expressMiddleware()` / `fastifyMiddleware()`         | pull middleware    | mounts a pull-mode profile endpoint         |
|  [06]   | `startWallProfiling` / `startHeapProfiling` + `stop*` | granular lifecycle | per-profiler arm/drain                      |
|  [07]   | `getLabels` / `setLabels` (+ `Wall` variants)         | ambient labels     | thread-scoped label get/set                 |
|  [08]   | `SourceMapper` / `setLogger`                          | support            | sourcemap symbolication, logger injection   |

Exact shipped declarations — `SourceMapper` reaches consumers through the default export object, and `SourceMapper.create` mints the instance `PyroscopeConfig.sourceMapper` seats:

[SOURCE_MAPPER]: `SourceMapper.create(string[],boolean?) -> Promise<SourceMapper>` `SourceMapper.hasMappingInfo(string) -> boolean` `SourceMapper.mappingInfo(GeneratedLocation) -> SourceLocation`
[PYROSCOPE_CONFIG]: `PyroscopeConfig.appName: string|undefined` `PyroscopeConfig.authToken: string|undefined` `PyroscopeConfig.basicAuthUser: string|undefined` `PyroscopeConfig.basicAuthPassword: string|undefined` `PyroscopeConfig.tenantID: string|undefined` `PyroscopeConfig.serverAddress: string|undefined` `PyroscopeConfig.flushIntervalMs: number|undefined` `PyroscopeConfig.tags: LabelSet|undefined` `PyroscopeConfig.wall: PyroscopeWallConfig|undefined` `PyroscopeConfig.heap: PyroscopeHeapConfig|undefined` `PyroscopeConfig.sourceMapper: SourceMapper|undefined` `PyroscopeConfig.stripFilenames: StripFilenamesMode|undefined` `PyroscopeConfig.shortenPaths: boolean|undefined`
[STRIP_FILENAMES_MODE]: `StripFilenamesMode = "all"|"dependencies"`
[PYROSCOPE_WALL_CONFIG]: `PyroscopeWallConfig.samplingDurationMs: number|undefined` `PyroscopeWallConfig.samplingIntervalMicros: number|undefined` `PyroscopeWallConfig.collectCpuTime: boolean|undefined`
[PYROSCOPE_HEAP_CONFIG]: `PyroscopeHeapConfig.samplingIntervalBytes: number|undefined` `PyroscopeHeapConfig.stackDepth: number|undefined`
[SURFACES]: `init(PyroscopeConfig?) -> void` `start() -> void` `stop() -> Promise<void>` `wrapWithLabels(Record<string,string|number>,()=>void,...unknown[]) -> void`

## [04]-[IMPLEMENTATION_LAW]

[PROFILE_TOPOLOGY]:
- composition-root only — `init`/`start` seat once at the node root; a library arming the profiler double-samples the process.
- native profiler — `@datadog/pprof` supplies the wall and heap samplers; a prebuilt platform binary loads first, a source build is the fallback.
- label identity — profile labels align with the estate resource identity; `appName` and `PyroscopeConfig.tags` mirror the `service.name` spelling so profiles join traces on one identity.
- `stop()` joins the process drain — its `Promise` settles before exit so the final profile flushes to the backend.

[INTEGRATION_LAW]:
- Stack with the deploy plane: `serverAddress` targets the Pyroscope backend the deploy plane provisions; `authToken`/`basicAuthUser`/`basicAuthPassword`/`tenantID` carry the backend credentials.
- Stack with `otel/emit` resource identity: `appName` and `tags` mirror the `AppIdentity`-derived `service.name`/resource attributes so profiles, traces, and metrics share one identity coordinate.
- Stack with `wrapWithLabels`: label bands scope a workload region, and the ambient `setWallLabels`/`setLabels` thread labels across the wall profiler window.

[LOCAL_ADMISSION]:
- `scope:runtime`, node lane; `init`/`start`/`stop` live only in the node boot and drain graph.
- Push mode is the default rail — `expressMiddleware`/`fastifyMiddleware` mount a pull endpoint only where a scrape topology owns collection.

[RAIL_LAW]:
- Package: `@pyroscope/nodejs`
- Owns: continuous CPU-wall + heap profiling push — `init`/`start`/`stop` lifecycle, `wrapWithLabels` label scoping, express/fastify pull middleware
- Accept: one seat at the node root pointed at the deploy-provisioned backend with resource-aligned tags, `stop()` on the process drain
- Reject: library-altitude `init`, profile labels divergent from the estate resource identity, a second profiler stack beside `@datadog/pprof`
