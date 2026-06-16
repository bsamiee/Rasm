# [PY_RUNTIME_API_STRUCTLOG]

`structlog` supplies structured contextual logging: bound loggers with immutable context, a configurable processor chain (timestamps, level, callsite, exceptions, JSON/logfmt rendering), contextvar-bound merged context, a stdlib-logging bridge, and test-capture loggers. It is the runtime owner for local structured logging that feeds the receipt and OTel surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `structlog`
- package: `structlog`
- import: `structlog`
- version: `26.1.0`
- owner: `runtime`
- rail: observability
- namespaces: `structlog`, `structlog.processors`, `structlog.contextvars`, `structlog.stdlib`, `structlog.dev`, `structlog.testing`, `structlog.tracebacks`, `structlog.typing`
- capability: bound contextual loggers, processor chains, contextvar context merging, stdlib bridge, JSON/logfmt rendering, test capture

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: logger family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `BoundLogger` | logger | context-bound structured logger |
| [2] | `BoundLoggerBase` | logger base | custom bound-logger base |
| [3] | `stdlib.BoundLogger` | logger | stdlib-shaped bound logger |
| [4] | `PrintLogger` | logger | stdout writer logger |
| [5] | `BytesLogger` | logger | bytes-emitting logger |
| [6] | `PrintLoggerFactory` | factory | print-logger factory |
| [7] | `stdlib.LoggerFactory` | factory | stdlib logger factory |

[PUBLIC_TYPE_SCOPE]: processor family
- rail: observability

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `processors.JSONRenderer` | renderer | JSON event renderer |
| [2] | `processors.LogfmtRenderer` | renderer | logfmt event renderer |
| [3] | `processors.KeyValueRenderer` | renderer | key=value renderer |
| [4] | `processors.TimeStamper` | processor | timestamp injector |
| [5] | `processors.CallsiteParameterAdder` | processor | callsite metadata injector |
| [6] | `processors.StackInfoRenderer` | processor | stack-info attacher |
| [7] | `processors.UnicodeDecoder` | processor | bytes-to-text normaliser |
| [8] | `processors.EventRenamer` | processor | event-key rename |
| [9] | `stdlib.ProcessorFormatter` | formatter | stdlib-handler bridge formatter |
| [10] | `stdlib.add_log_level` | processor | level-name injector |
| [11] | `dev.ConsoleRenderer` | renderer | colorised dev renderer |
| [12] | `testing.LogCapture` | capture | test event recorder |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration and logging operations
- rail: observability

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `configure` | config | install processor chain and factory |
| [2] | `configure_once` | config | idempotent configuration |
| [3] | `get_logger` | logger | obtain a bound logger |
| [4] | `make_filtering_bound_logger` | logger | level-filtered bound-logger class |
| [5] | `BoundLogger.bind` | context | add bound key/value context |
| [6] | `BoundLogger.unbind` | context | remove bound context keys |
| [7] | `BoundLogger.new` | context | replace bound context |
| [8] | `contextvars.bind_contextvars` | context | bind ambient contextvar context |
| [9] | `contextvars.merge_contextvars` | processor | merge contextvar context into event |
| [10] | `contextvars.clear_contextvars` | context | reset ambient context |
| [11] | `contextvars.bound_contextvars` | context | scoped contextvar binding |

## [4]-[IMPLEMENTATION_LAW]

[OBSERVABILITY_TOPOLOGY]:
- configuration law: the processor chain is installed once via `configure`/`configure_once` — `merge_contextvars`, `add_log_level`, `TimeStamper(fmt="iso")`, `StackInfoRenderer`, then a `JSONRenderer` terminal; the chain is one declaration, never reconfigured per module.
- logger law: modules call `get_logger(__name__)` and `bind` correlation/deadline context from the runtime context owner; ambient cross-call context flows through `contextvars`, never thread-locals or globals.
- rendering law: machine output is `JSONRenderer`; the OTLP path attaches via `ProcessorFormatter` so one event stream feeds both stdout and the OTel log signal — no parallel logging system.
- correlation law: the bound `correlation_id` is the same identity the receipt owner carries; structlog renders it, the receipt owner records it.
- test law: specs capture with `testing.LogCapture`; production code never branches on capture mode.

[LOCAL_ADMISSION]:
- structlog is the local structured-logging owner; the OTel log signal is fed from the same processor chain via the SDK bridge, never a second logging configuration.
- The receipt surface consumes bound context as receipt fields; structlog renders, the receipt owner persists.

[RAIL_LAW]:
- Package: `structlog`
- Owns: bound contextual logging, the processor chain, contextvar context merging, and the stdlib/OTel bridge formatter
- Accept: single `configure` chain, `get_logger` + `bind`, contextvar context, JSON rendering, `ProcessorFormatter` bridge
- Reject: per-module reconfiguration, thread-local context, a parallel logging stack, duplicated correlation identity
