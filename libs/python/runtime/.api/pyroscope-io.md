# [PY_RUNTIME_API_PYROSCOPE_IO]

`pyroscope-io` owns the native continuous-profiling push agent: one process-wide background sampler pushes CPU or wall pprof frames to a Pyroscope store, scoped by the block, thread, or process sample tags a store slices on. `pyroscope-otel` wraps the same agent to bind profile samples onto trace spans, and its `Profiles` owner drives the push directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyroscope-io`
- package: `pyroscope-io`
- module: `pyroscope`
- namespaces: `pyroscope`
- rail: observability
- abi: native CFFI push agent behind a pure-Python `pyroscope` facade

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sampling posture
- `LineNo` fixes which instruction a sampled frame attributes to, resolving a hot line against the profiler's frame-address convention.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `LineNo.LastInstruction` | enum          | attribute a sample to the line's last instruction — the default |
|  [02]   | `LineNo.First`           | enum          | attribute a sample to the line's first instruction              |
|  [03]   | `LineNo.NoLine`          | enum          | drop line attribution, folding to function granularity          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: agent lifecycle — every surface is a module-level `pyroscope` function
- `configure` carry: `application_name`, `server_address`, `sample_rate`, `oncpu`, `gil_only`, `tags`, `tenant_id`, `basic_auth_username`/`basic_auth_password`, `http_headers`, `report_pid`/`report_thread_id`/`report_thread_name`, `line_no`, `enable_logging`.
- `oncpu` toggles CPU-time versus wall-time sampling; `gil_only=False` retains samples from Python threads while native kernels release the GIL.

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `configure(...)`                                               | static  | start the push agent toward the profile store       |
|  [02]   | `shutdown()` / `stop()`                                        | static  | flush and stop the push agent                       |
|  [03]   | `tag_wrapper(tags)`                                            | static  | context-managed sample tags around a block          |
|  [04]   | `add_thread_tag(key, value)` / `remove_thread_tag(key, value)` | static  | per-thread sample tag mutation the wrapper composes |
|  [05]   | `tag(tags)` / `remove_tags(*keys)`                             | static  | process-wide sample tag set and clear               |
|  [06]   | `change_name(name)`                                            | static  | retarget the application-name dimension live        |
|  [07]   | `build_summary()` / `runtime_name()` / `runtime_version()`     | static  | agent build and interpreter identity                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- lifecycle: `configure` starts one process-wide background sampler; `shutdown`/`stop` flush and halt it.
- posture: `oncpu=True` with `gil_only=False` keeps CPU-active native-kernel work visible while idle wait falls out.
- tenancy: `tenant_id` carries the store's org routing behind a multi-tenant profile store, and `application_name` keys the profile stream a store slices.
- tagging: `tag_wrapper` brackets a block through the `add_thread_tag`/`remove_thread_tag` pair, scoping sample tags to the running thread and unwinding on exit.

[STACKING]:
- `pyroscope-otel`(`.api/pyroscope-otel.md`): `PyroscopeSpanProcessor.on_start` stamps the running thread's samples with `span_id`/`span_name`/`trace_id` through this agent's `add_thread_tag`, and `on_end` clears them via `remove_thread_tag` — closing the profile-to-trace link the `configure` push opens.
- runtime profiles owner: one `configure` at the composition root, each kernel entry seam bracketed in `tag_wrapper`, `shutdown` on drain.

[LOCAL_ADMISSION]:
- Only the profiles owner calls `configure`; worker-floor code composes `tag_wrapper` and the `add_thread_tag`/`remove_thread_tag` pair, reaching process-global `tag`/`remove_tags` only from the composition root.
- `build_summary`/`runtime_name`/`runtime_version` feed the offline-job envelope's agent-identity fields.

[RAIL_LAW]:
- Package: `pyroscope-io`
- Owns: the native continuous-profiling push agent — configure/shutdown lifecycle, block/thread/process sample tagging, sampling posture, and agent introspection
- Accept: one profiles-owner `configure` at the composition root, `tag_wrapper` bracketing a kernel entry seam, `shutdown` on drain
- Reject: a second profiling agent beside the push, a per-worker reconfigure racing the process-global sampler, hand-rolled pprof push the agent already owns
