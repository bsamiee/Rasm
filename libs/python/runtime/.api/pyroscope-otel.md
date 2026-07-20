# [PY_RUNTIME_API_PYROSCOPE_OTEL]

`pyroscope-otel` links continuous profiles to traces: `PyroscopeSpanProcessor` stamps every ROOT span with the `pyroscope.profile.id` attribute and tags the profiler's thread samples with `span_id`/`span_name`/`trace_id`, so a trace click-through lands on its flame graph and a profile slices back to its trace. It carries the pyroscope push agent as its substrate, so one admission supplies both the processor and the `pyroscope.configure` push surface the profiles owner drives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyroscope-otel`
- package: `pyroscope-otel`
- module: `pyroscope.otel`
- owner: `runtime`
- rail: observability
- asset: pure-Python span processor over the native pyroscope push agent
- namespaces: `pyroscope.otel`, `pyroscope`
- installed: `1.1.0`
- capability: root-span profile-id stamping, profiler thread-tag correlation, and the push agent's configure/shutdown/tag lifecycle

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: span processor
- rail: observability
- SDK-coupled by construction — the processor subclasses `opentelemetry.sdk.trace.SpanProcessor` — so it composes only at the composition root, attached to the registered SDK `TracerProvider`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `PyroscopeSpanProcessor`         | span processor | `on_start` stamps + tags root spans, `on_end` untags them      |
|  [02]   | `PROFILE_ID_SPAN_ATTRIBUTE_KEY`  | constant       | `pyroscope.profile.id` — the span-side link key                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: push-agent lifecycle (`pyroscope`)
- rail: observability
- `configure` keywords the runtime drives: `application_name`, `server_address`, `sample_rate`, `oncpu`, `gil_only`, `tags`, `tenant_id`, `basic_auth_username`/`basic_auth_password`, `http_headers`, `report_pid`/`report_thread_id`/`report_thread_name`, `line_no`.

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `pyroscope.configure(...)`                     | enable         | start the push agent toward the profile store    |
|  [02]   | `pyroscope.shutdown()` / `pyroscope.stop()`    | disable        | stop the push agent                              |
|  [03]   | `pyroscope.tag_wrapper(tags)`                  | scope          | context-managed sample tags around a block       |
|  [04]   | `add_thread_tag(key, value)` / `remove_thread_tag(key, value)` | tag | per-thread sample tagging |
|  [05]   | `TracerProvider.add_span_processor(PyroscopeSpanProcessor())` | attach | the one processor attach at the composition root |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- link law: only a root span — no parent, or a remote parent — stamps and tags, so one profile id spans one local trace leg and nested spans inherit the correlation through their root.
- sequencing law: the processor attaches after the telemetry install registers the SDK provider, and the push agent runs under the same `emit_otel` profile gate — the profiles owner's install is the one composition site.
- posture law: `oncpu=True` + `gil_only=True` keep samples on interpreter work; `tenant_id` carries the store's org routing when a multi-tenant store fronts the push.

[RAIL_LAW]:
- Package: `pyroscope-otel`
- Owns: the span-profile correlation link and the continuous-profiling push lifecycle
- Accept: one `Profiles.install` composition — `pyroscope.configure` plus one `PyroscopeSpanProcessor` attach on the registered provider
- Reject: a second profiling agent beside the push, a library-altitude import of the SDK-coupled processor, per-span hand tagging the processor already owns
