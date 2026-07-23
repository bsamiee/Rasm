# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_JINJA2]

`opentelemetry-instrumentation-jinja2` traces jinja2 template work: one `BaseInstrumentor` patches the render, compile, and load paths so each pass opens an internal span — `jinja2.render`, `jinja2.compile`, `jinja2.load` — carrying `jinja2.template_name`, with the resolved `jinja2.template_path` on the load span alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-jinja2`
- package: `opentelemetry-instrumentation-jinja2`
- module: `opentelemetry.instrumentation.jinja2`
- namespaces: `opentelemetry.instrumentation.jinja2`
- rail: observability
- abi: pure-Python runtime library

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------- | :------------ | :------------------------------- |
|  [01]   | `Jinja2Instrumentor` | instrumentor  | jinja2 render/compile/load spans |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instrumentor lifecycle

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Jinja2Instrumentor().instrument(**kwargs)`   | instance | patch render/generate/compile/load |
|  [02]   | `Jinja2Instrumentor().uninstrument(**kwargs)` | instance | unwrap the four wrapped members    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `instrument()` patches the module classes globally; `uninstrument()` reverts the same four members, activated once and never per template pass.
- `Template.render`/`Template.generate` open `jinja2.render`, `Environment.compile` opens `jinja2.compile`, `Environment._load_template` opens `jinja2.load`; each span carries `jinja2.template_name`, and `jinja2.template_path` rides `jinja2.load` alone from the resolved template filename.

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): each patched member opens its span through `Tracer.start_as_current_span` off the `tracer_provider`, defaulting to the global provider; `jinja2.template_name`/`jinja2.template_path` land as `Span.set_attribute` values. SDK provider install is the composition-root concern, never this row.
- within-lib: the runtime metrics `Instrumentation.install` train row activates this once at runtime altitude, while jinja2 renders execute at artifacts altitude (`libs/python/artifacts/.api/jinja2.md`) — a cross-tier consumer whose spans light up only after the runtime train runs.

[LOCAL_ADMISSION]:
- one train-row `instrument()` at the composition root; an artifacts or library module never activates it.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-jinja2`
- Owns: internal render/compile/load spans on every jinja2 template pass
- Accept: one train-row `instrument()` at the composition root
- Reject: activation inside an artifacts or library module, hand-rolled jinja2 timing spans beside the patched members
