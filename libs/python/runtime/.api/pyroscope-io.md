# [PY_RUNTIME_API_PYROSCOPE_IO]

`pyroscope-io` is the native continuous-profiling push agent — the substrate `pyroscope-otel` wraps and the profiles owner drives directly. Its `pyroscope` module owns the agent lifecycle (`configure`/`shutdown`/`stop`), the sample-tag surface (block-scoped `tag_wrapper`, per-thread and process tag mutation), and the introspection verbs (`build_summary`, `runtime_name`/`runtime_version`) the offline-job envelope reports.

A background sampler pushes CPU or wall pprof frames to the store, and `pyroscope-otel`'s `PyroscopeSpanProcessor` tags them with the active `span_id`/`trace_id` — a geometry worker-floor flame graph correlates back to its evidence span.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyroscope-io`
- package: `pyroscope-io`
- module: `pyroscope`
- owner: `runtime`
- rail: observability
- asset: native CFFI push agent behind a pure-Python `pyroscope` facade
- namespaces: `pyroscope`
- capability: agent configure/shutdown lifecycle, block/thread/process sample tagging, CPU-vs-wall sampling posture, multi-tenant push routing, and agent introspection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sampling posture
- rail: observability
- `LineNo` selects which instruction a sampled frame attributes to, so a hot line resolves against the profiler's frame-address convention rather than a caller default.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                               |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `LineNo.LastInstruction` | enum          | attribute a sample to the last instruction of the line — the default |
|  [02]   | `LineNo.First`           | enum          | attribute a sample to the first instruction of the line              |
|  [03]   | `LineNo.NoLine`          | enum          | drop line attribution, folding to function granularity               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: agent lifecycle
- rail: observability
- `configure` keywords the profiles owner drives: `application_name`, `server_address`, `sample_rate`, `oncpu`, `gil_only`, `tags`, `tenant_id`, `basic_auth_username`/`basic_auth_password`, `http_headers`, `report_pid`/`report_thread_id`/`report_thread_name`, `line_no`, `enable_logging`.
- `oncpu=True` samples interpreter CPU time; `oncpu=False` samples wall time. `gil_only=False` retains samples from Python threads while native kernels release the GIL.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `configure(...)`                                               | enable         | start the push agent toward the profile store       |
|  [02]   | `shutdown()` / `stop()`                                        | disable        | flush and stop the push agent                       |
|  [03]   | `tag_wrapper(tags)`                                            | scope          | context-managed sample tags around a block          |
|  [04]   | `add_thread_tag(key, value)` / `remove_thread_tag(key, value)` | thread-tag     | per-thread sample tag mutation the wrapper composes |
|  [05]   | `tag(tags)` / `remove_tags(*keys)`                             | process-tag    | process-wide sample tag set and clear               |
|  [06]   | `change_name(name)`                                            | identity       | retarget the application-name dimension live        |
|  [07]   | `build_summary()` / `runtime_name()` / `runtime_version()`     | introspect     | agent build and interpreter identity                |

## [04]-[IMPLEMENTATION_LAW]

[AGENT_TOPOLOGY]:
- lifecycle law: `configure` starts one process-wide background sampler; `shutdown`/`stop` flush and halt it — the profiles owner's install is the single call site, never a per-worker reconfigure.
- posture law: `oncpu=True` + `gil_only=False` keeps CPU-active native-kernel work visible while idle wait falls out.
- tenancy law: `tenant_id` carries the store's org routing when a multi-tenant profile store fronts the push, and `application_name` keys the profile stream a store slices by.
- tag law: `tag_wrapper` brackets a block through the `add_thread_tag`/`remove_thread_tag` pair, so sample tags scope to the running thread and unwind on exit — the shape the span processor tags a trace leg with, never process-global `tag` in worker-floor code.

[RAIL_LAW]:
- Package: `pyroscope-io`
- Owns: the native continuous-profiling push agent — configure/shutdown lifecycle, block/thread/process sample tagging, sampling posture, and agent introspection
- Accept: one profiles-owner `configure` at the composition root, `tag_wrapper` bracketing a kernel entry seam, `shutdown` on drain
- Reject: a second profiling agent beside the push, a per-worker reconfigure racing the process-global sampler, hand-rolled pprof push the agent already owns
