# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_JINJA2]

`opentelemetry-instrumentation-jinja2` supplies jinja2 template tracing: one `BaseInstrumentor` patching the render, compile, and load paths so every template pass emits an internal span — `jinja2.render`, `jinja2.compile`, `jinja2.load` — each carrying `jinja2.template_name`, with the resolved `jinja2.template_path` riding the load span alone. It is the train row that puts template work on the one distributed trace; jinja2 renders happen at artifacts altitude, spans activate once at runtime altitude.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-jinja2`
- package: `opentelemetry-instrumentation-jinja2`
- module: `opentelemetry.instrumentation.jinja2`
- owner: `runtime`
- rail: observability
- asset: pure-Python runtime library
- namespaces: `opentelemetry.instrumentation.jinja2`
- capability: global `Template.render`/`Template.generate`/`Environment.compile`/`Environment._load_template` patching over the three-span jinja2 layer — `jinja2.render`, `jinja2.compile`, `jinja2.load`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor
- rail: observability

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------- | :------------ | :------------------------------- |
|  [01]   | `Jinja2Instrumentor` | instrumentor  | jinja2 render/compile/load spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle
- rail: observability
- `instrument` kwargs: `tracer_provider`.
- jinja2 exposes no per-object instrument form; activation patches the module classes globally and `uninstrument()` unwraps the same four members.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `Jinja2Instrumentor().instrument(**kwargs)`   | enable         | patch render/generate/compile/load |
|  [02]   | `Jinja2Instrumentor().uninstrument(**kwargs)` | disable        | unwrap the four wrapped members    |

## [04]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- activation law: one `instrument()` at the composition root — the runtime metrics `Instrumentation.install` train row; jinja2 renders execute at artifacts altitude, spans activate once at runtime altitude, and a library package never instruments.
- wrap law: activation patches `Template.render` and `Template.generate` onto the `jinja2.render` span, `Environment.compile` onto `jinja2.compile`, and `Environment._load_template` onto `jinja2.load`; `uninstrument()` unwraps the same four members.
- span law: `jinja2.render`, `jinja2.compile`, and `jinja2.load` each carry `jinja2.template_name`; `jinja2.template_path` rides the `jinja2.load` span alone, set from the resolved template filename.
- provider law: `tracer_provider` defaults to the global provider; SDK install stays the telemetry composition root's.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-jinja2`
- Owns: internal render/compile/load spans on every jinja2 template pass
- Accept: one train-row `instrument()` at the composition root
- Reject: activation inside an artifacts or library module, hand-rolled jinja2 timing spans beside the patched members
