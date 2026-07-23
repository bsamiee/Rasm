# [PY_RUNTIME_API_PYROSCOPE_OTEL]

`pyroscope-otel` links continuous profiles to traces: its `PyroscopeSpanProcessor` stamps each root span with `pyroscope.profile.id` and tags the profiler's thread samples with `span_id`/`span_name`/`trace_id`, so a trace click-through lands on its flame graph and a profile slices back to its trace. `pyroscope-io` (`.api/pyroscope-io.md`) owns the push agent it tags; this catalog owns only the span-side correlation processor and its SDK attach.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyroscope-otel`
- package: `pyroscope-otel`
- module: `pyroscope.otel`
- namespaces: `pyroscope.otel`
- rail: observability
- abi: pure-Python span processor tagging the pyroscope push agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: span processor and correlation link keys

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------------- |
|  [01]   | `PyroscopeSpanProcessor`        | class         | `on_start` stamps + tags root spans, `on_end` untags them       |
|  [02]   | `PROFILE_ID_SPAN_ATTRIBUTE_KEY` | constant      | `pyroscope.profile.id` — the span-side link attribute           |
|  [03]   | `PROFILE_ID_PYROSCOPE_TAG_KEY`  | constant      | `span_id` — the sample-side link tag keying a profile to a span |
|  [04]   | `SPAN_NAME_PYROSCOPE_TAG_KEY`   | constant      | `span_name` — the sample-side span-name tag                     |
|  [05]   | `TRACE_ID_PYROSCOPE_TAG_KEY`    | constant      | `trace_id` — the sample-side trace-id tag                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: composition-root attach

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `TracerProvider.add_span_processor(PyroscopeSpanProcessor())` | instance | the one processor attach on the registered provider |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- link law: only a root span — no parent, or a remote parent — stamps and tags, so one profile id spans one local trace leg and nested spans inherit the correlation through their root.
- attach law: the processor attaches after the telemetry install registers the SDK provider, so the profiles owner's install is the one composition site.

[STACKING]:
- `opentelemetry-sdk`(`libs/python/.api/opentelemetry-sdk.md`): `PyroscopeSpanProcessor` implements `sdk.trace.SpanProcessor.on_start`/`on_end` (root-span gated) and attaches via `TracerProvider.add_span_processor` at the registered provider, fanning through the provider's multi-processor.
- `pyroscope-io`(`.api/pyroscope-io.md`): `on_start` calls the agent's `add_thread_tag` with `span_id`/`span_name`/`trace_id` and stamps `pyroscope.profile.id` on the root span; `on_end` calls `remove_thread_tag` to unwind the leg's tags.

[RAIL_LAW]:
- Package: `pyroscope-otel`
- Owns: the span-profile correlation link — root-span profile-id stamp and profiler thread-tag correlation
- Accept: one `PyroscopeSpanProcessor` attach on the registered SDK provider at the composition root
- Reject: a library-altitude import of the SDK-coupled processor, per-span hand tagging the processor already owns
