# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION]

`opentelemetry-instrumentation` owns the contracts every `opentelemetry-instrumentation-*` sibling implements: the `BaseInstrumentor` lifecycle, the dependency gate deciding whether a patch may install, the monkeypatch-reversal helper, the ambient suppression scopes, the response-header propagator, and the semconv stability opt-in. It patches nothing itself — the train rides it, and the composition root reaches it through the sibling instrumentors rather than directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation`
- package: `opentelemetry-instrumentation` (Apache-2.0)
- module: `opentelemetry.instrumentation`
- namespaces: `.instrumentor`, `.distro`, `.dependencies`, `.propagators`, `.utils`, `.log_utils`, `.environment_variables`, `._semconv`, `._labeler`, `.cidict`, `.sqlcommenter_utils`, `.auto_instrumentation`, `.bootstrap`
- requires: `opentelemetry-api~=1.4`, `opentelemetry-semantic-conventions` pinned equal, `packaging`, `wrapt<3`
- commands: `opentelemetry-instrument`, `opentelemetry-bootstrap`
- abi: pure-Python runtime library
- rail: observability

`opentelemetry.instrumentation` is a namespace package the whole train shares, so a sibling distribution's sub-package sits beside these owned modules under one import root and `pip` ownership, never import path, decides which distribution ships a name.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentor lifecycle and its admission gate

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `BaseInstrumentor`        | abstract      | per-subclass singleton carrying the instrument/uninstrument fold |
|  [02]   | `BaseDistro`              | abstract      | configures the SDK before any instrumentor loads                 |
|  [03]   | `DefaultDistro`           | concrete      | the shipped distro, wiring the OTLP defaults                     |
|  [04]   | `DependencyConflict`      | value         | `required`/`found` beside `required_any`/`found_any`             |
|  [05]   | `DependencyConflictError` | exception     | the raising arm of the same verdict                              |

[PUBLIC_TYPE_SCOPE]: response propagation and carrier setters

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------ | :------------ | :------------------------------------------------- |
|  [01]   | `ResponsePropagator`      | abstract      | inject contract for a server RESPONSE carrier      |
|  [02]   | `TraceResponsePropagator` | concrete      | stamps `traceresponse` and exposes it through CORS |
|  [03]   | `Setter`                  | abstract      | response-carrier write contract                    |
|  [04]   | `DictHeaderSetter`        | concrete      | mapping-carrier setter, the shipped default        |
|  [05]   | `FuncSetter`              | concrete      | setter over a carrier's own header-write callable  |

[PUBLIC_TYPE_SCOPE]: metric enrichment and shared carriers

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `Labeler`                                   | class         | per-context custom metric attributes under caps              |
|  [02]   | `CIDict`                                    | mapping       | case-insensitive `MutableMapping` retaining the original key |
|  [03]   | `_StabilityMode`                            | enum          | `default`, `http`, `http/dup`, `database`, `database/dup`    |
|  [04]   | `_OpenTelemetrySemanticConventionStability` | class         | reads the stability env var once per process                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the lifecycle a sibling instrumentor subclasses and a composition root calls

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `instrument(**kwargs)`                                      | instance | gate, opt-in init, then `_instrument`      |
|  [02]   | `uninstrument(**kwargs)`                                    | instance | `_uninstrument` behind the installed latch |
|  [03]   | `is_instrumented_by_opentelemetry`                          | property | the latch itself                           |
|  [04]   | `instrumentation_dependencies() -> Collection[str]`         | abstract | the requirement rows the gate resolves     |
|  [05]   | `_instrument(**kwargs)`                                     | override | the patch; DEFAULTS to a no-op             |
|  [06]   | `_uninstrument(**kwargs)`                                   | abstract | the reversal; a subclass must supply it    |
|  [07]   | `instrument(skip_dep_check=, raise_exception_on_conflict=)` | kwargs   | bypass the gate, or make it raise          |

[ENTRYPOINT_SCOPE]: dependency verdicts, patch reversal, and ambient suppression

| [INDEX] | [SURFACE]                                                           | [SHAPE]     | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------ | :---------- | :---------------------------------------------- |
|  [01]   | `get_dependency_conflicts(deps, deps_any=None)`                     | verdict     | conflict or `None` over requirement rows        |
|  [02]   | `get_dist_dependency_conflicts(dist)`                               | verdict     | the same verdict from a distribution's metadata |
|  [03]   | `unwrap(obj, attr)`                                                 | reversal    | restores one `wrapt`-patched attribute          |
|  [04]   | `is_instrumentation_enabled()`                                      | gate        | reads the ambient suppression key               |
|  [05]   | `is_http_instrumentation_enabled()`                                 | gate        | the HTTP-scoped suppression key                 |
|  [06]   | `suppress_instrumentation()`                                        | context mgr | suppresses every instrumentor in scope          |
|  [07]   | `suppress_http_instrumentation()`                                   | context mgr | suppresses the HTTP leg alone                   |
|  [08]   | `http_status_to_status_code(status, allow_redirect=, server_span=)` | projection  | HTTP status onto `StatusCode`                   |
|  [09]   | `extract_attributes_from_object(obj, attributes, existing=None)`    | projection  | named object fields onto a `str` mapping        |
|  [10]   | `std_to_otel(levelno) -> SeverityNumber`                            | projection  | stdlib log level onto the OTLP severity band    |

[ENTRYPOINT_SCOPE]: response propagation, metric enrichment, and the process-launch commands

| [INDEX] | [SURFACE]                                                        | [SHAPE]      | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `set_global_response_propagator(propagator)`                     | install      | seats the process response propagator              |
|  [02]   | `get_global_response_propagator()`                               | resolve      | reads it back                                      |
|  [03]   | `TraceResponsePropagator.inject(carrier, context=, setter=)`     | inject       | writes `traceresponse` onto a response carrier     |
|  [04]   | `Labeler(max_custom_attrs=20, max_attr_value_length=100)`        | ctor         | the caps a labeler enforces                        |
|  [05]   | `get_labeler()` / `set_labeler(labeler)` / `clear_labeler()`     | context      | the labeler bound to the active OTel context       |
|  [06]   | `Labeler.add(key, value)` / `.add_attributes(mapping)`           | instance     | admits primitive attribute values                  |
|  [07]   | `get_labeler_attributes()`                                       | read         | read-only mapping, empty where none is bound       |
|  [08]   | `enrich_metric_attributes(base_attributes, enrich_enabled=True)` | fold         | base attributes plus admitted labeler attributes   |
|  [09]   | `_add_sql_comment(sql, **meta)`                                  | sqlcommenter | appends the trace-context SQL comment              |
|  [10]   | `initialize(swallow_exceptions=True)`                            | launch       | loads distro, configurator, and every instrumentor |
|  [11]   | `bootstrap.run(default_instrumentations=None, libraries=None)`   | launch       | installs the instrumentors an env's libraries earn |

[ENTRYPOINT_SCOPE]: environment coordinates the launch path reads

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `OTEL_PYTHON_DISTRO`                                         | env     | selects the `BaseDistro` entry point         |
|  [02]   | `OTEL_PYTHON_CONFIGURATOR`                                   | env     | selects the configurator entry point         |
|  [03]   | `OTEL_PYTHON_DISABLED_INSTRUMENTATIONS`                      | env     | names train rows the auto path skips         |
|  [04]   | `OTEL_SEMCONV_STABILITY_OPT_IN`                              | env     | picks the `_StabilityMode` per signal family |
|  [05]   | `OTEL_PYTHON_AUTO_INSTRUMENTATION_EXPERIMENTAL_GEVENT_PATCH` | env     | patches gevent ahead of the train            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BaseInstrumentor.__new__` returns `cls._instance`, so every subclass is a PROCESS singleton and its installed latch is process state: two call sites constructing the same instrumentor hold one object, and a second `instrument()` logs a warning and returns `None` rather than re-patching or raising.
- `uninstrument()` ahead of any `instrument()` warns and returns `None` on the same latch, so a teardown running twice is safe and a teardown running against a never-installed instrumentor is indistinguishable from success.
- Dependency verdicts decide admission and DEFAULT to a silent failure: an unmet or conflicting requirement row logs an error and returns `None`, so a manual `instrument()` reports nothing and installs nothing; `raise_exception_on_conflict=True` converts that to `DependencyConflictError`, which is the arm the auto-instrumentation path takes, and `skip_dep_check=True` removes the gate whole.
- `_instrument` carries a NO-OP default while `_uninstrument` is abstract, so a subclass that forgets its patch still constructs, still passes the gate, and still sets the installed latch — the class shape catches a missing teardown and never a missing patch.
- `instrument()` initializes the semconv stability opt-in immediately before `_instrument`, so `OTEL_SEMCONV_STABILITY_OPT_IN` is read once per process at the FIRST instrumentor to install; mutating that variable afterwards changes nothing, and a process wanting the dual mode sets it before any train row runs.
- Suppression rides UUID-suffixed context keys, so `suppress_instrumentation()` scopes to the active context rather than a process flag and an exporter's own outbound HTTP call inside that scope emits no nested span.
- `Labeler` drops silently at its caps: a NEW key past 20 attributes vanishes while an existing key still updates, a string past 100 characters truncates, and a non-primitive value logs a warning and never lands — so a producer treating it as an attribute sink loses the tail with no fault.
- `enrich_metric_attributes` never overwrites a base attribute, which makes the labeler additive by construction and makes a collision resolve to the instrument's own dimension rather than the caller's.

[STACKING]:
- `opentelemetry-api`(`libs/python/.api/opentelemetry-api.md`): supplies the `Context` that carries suppression keys and the labeler, the `SeverityNumber` band `std_to_otel` projects onto, and the `StatusCode` `http_status_to_status_code` returns.
- `opentelemetry-semantic-conventions`(`.api/opentelemetry-semantic-conventions.md`): pinned EQUAL by this distribution's own requirement, so the two bump as one and a stability mode selected here resolves against that release's constant spellings.
- `opentelemetry-instrumentation-{asyncio,dbapi,grpc,httpx,jinja2,psycopg,sqlite3,system-metrics,threading}`: each ships its `BaseInstrumentor` subclass against this lifecycle and reverses its own patch through `unwrap`; `dbapi` alone ships no instrumentor and is reached directly.
- `opentelemetry-processor-baggage`(`.api/opentelemetry-processor-baggage.md`): rides the same composition root and carries no instrumentor, so baggage promotion is unaffected by a suppression scope.

[LOCAL_ADMISSION]:
- Composition roots call each train row's `instrument()` once and hold the singleton; a library module activating an instrumentor imposes a process-wide patch its own caller never asked for.
- Instrumentors install with the gate ARMED and the raising arm selected, so a version drift refuses with typed evidence instead of returning a silently unpatched process.
- `opentelemetry-instrument` and `bootstrap` stay out of the estate: the branch composes its providers explicitly, and an auto-loader picking rows off installed distributions re-mints the roster the composition root declares.
- Response propagation stays off unless a server surface exposes the header deliberately, since `TraceResponsePropagator` publishes trace identity to the client.

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation`
- Owns: the `BaseInstrumentor` lifecycle, the dependency admission gate, `wrapt` patch reversal, ambient suppression scopes, response-header propagation, the semconv stability opt-in, and the shared HTTP and DB attribute projections
- Accept: one `instrument()` per train row at the composition root with the gate armed, `unwrap` for reversal, `suppress_instrumentation` around an exporter's own egress, `std_to_otel` and `http_status_to_status_code` as the projections
- Reject: instrumentor activation inside a library module, `skip_dep_check` standing in for a version pin, the `opentelemetry-instrument` auto-loader, a hand-rolled monkeypatch reversal beside `unwrap`
