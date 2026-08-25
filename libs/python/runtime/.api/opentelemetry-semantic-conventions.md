# [PY_RUNTIME_API_OPENTELEMETRY_SEMANTIC_CONVENTIONS]

`opentelemetry-semantic-conventions` ships constants alone: the released schema-url roster beside the generated attribute-key and metric-name spellings. Carrying no provider, exporter, or instrumentation, it composes below every observability owner. Stability decides admission — `attributes`/`metrics` freeze, `_incubating.*` renames within a minor, and the two aggregate enums are deprecated whole.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schema roster

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------ | :------------ | :------------------------------------------- |
|  [01]   | `Schemas`           | enum          | one member per released specification schema |
|  [02]   | `Schemas.V1_21_0`   | member        | oldest schema the release carries            |
|  [03]   | `Schemas.V1_43_0`   | member        | newest schema the release carries            |
|  [04]   | `Schemas.<V>.value` | str           | the schema url a scope and a `Resource` pin  |

[PUBLIC_TYPE_SCOPE]: frozen attribute modules

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `attributes.service_attributes`      | constants     | `SERVICE_NAME`, `SERVICE_NAMESPACE`, `SERVICE_VERSION`, `SERVICE_INSTANCE_ID` |
|  [02]   | `attributes.deployment_attributes`   | constants     | `DEPLOYMENT_ENVIRONMENT_NAME`                                                 |
|  [03]   | `attributes.error_attributes`        | constants     | `ERROR_TYPE`                                                                  |
|  [04]   | `attributes.exception_attributes`    | constants     | `EXCEPTION_TYPE`, `EXCEPTION_MESSAGE`, `EXCEPTION_STACKTRACE`                 |
|  [05]   | `attributes.otel_attributes`         | constants     | `OTEL_SCOPE_NAME`, `OTEL_SCOPE_VERSION`, `OTEL_STATUS_CODE`                   |
|  [06]   | `attributes.http_attributes`         | constants     | `HTTP_REQUEST_METHOD`, `HTTP_RESPONSE_STATUS_CODE`, `HTTP_ROUTE`              |
|  [07]   | `attributes.db_attributes`           | constants     | frozen database keys                                                          |
|  [08]   | `attributes.k8s_attributes`          | constants     | frozen kubernetes keys                                                        |
|  [09]   | `attributes.container_attributes`    | constants     | frozen container keys                                                         |
|  [10]   | `attributes.telemetry_attributes`    | constants     | frozen SDK-identity keys                                                      |
|  [11]   | `attributes.{client,server,network}` | constants     | frozen peer and network keys                                                  |
|  [12]   | `attributes.{url,code,user_agent}`   | constants     | frozen url, callsite, and agent keys                                          |

[PUBLIC_TYPE_SCOPE]: frozen metric modules

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `metrics.http_metrics` | constants     | `HTTP_SERVER_REQUEST_DURATION`, `HTTP_CLIENT_REQUEST_DURATION` |
|  [02]   | `metrics.db_metrics`   | constants     | frozen database metric names                                   |

[PUBLIC_TYPE_SCOPE]: incubating and deprecated surfaces

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `_incubating.attributes.<domain>` | constants     | unfrozen attribute domains, one module per group    |
|  [02]   | `_incubating.metrics.<domain>`    | constants     | unfrozen metric-name domains                        |
|  [03]   | `resource.ResourceAttributes`     | deprecated    | aggregate resource enum, `@deprecated` at the class |
|  [04]   | `trace.SpanAttributes`            | deprecated    | aggregate span enum, `@deprecated` at the class     |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema pin

| [INDEX] | [ENTRYPOINT]            | [KIND] | [CAPABILITY]                            |
| :-----: | :---------------------- | :----- | :-------------------------------------- |
|  [01]   | `Schemas.V1_43_0.value` | member | the one schema coordinate a branch pins |

[ENTRYPOINT_SCOPE]: attribute keys

| [INDEX] | [ENTRYPOINT]                                                | [KIND]   | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `from ...attributes.<domain> import <CONSTANT>`             | constant | a frozen specification key      |
|  [02]   | `from ..._incubating.attributes.<domain> import <CONSTANT>` | constant | an unfrozen key a minor renames |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Constants alone ship here, so importing the distribution reifies no SDK tier and costs no composition custody
- `opentelemetry-api` pins EQUAL, so this minor moves in lockstep with the api line
- Distribution version `0.<minor>b<patch>` tracks packaging, never schema; `Schemas` names the schema
- `ResourceAttributes` and `SpanAttributes` carry `@deprecated` at the class, so every member read warns
- `Schemas` is a plain `Enum`; a member reads `.value` for the url and is no string itself

[STACKING]:
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `get_tracer`/`get_meter`/`get_logger` take `schema_url` fourth positionally
- `opentelemetry-api`: one `Schemas` member stamps an identical scope triple across all three signals of one scope
- `opentelemetry-sdk`(`.api/opentelemetry-sdk.md`): `Resource.create(attributes, schema_url)` takes the same member
- `opentelemetry-sdk`: imports `exception_attributes` directly, so the distribution resolves transitively
- `structlog`(`.api/structlog.md`): exception-attribute constants name the keys a fault processor binds

[LOCAL_ADMISSION]:
- Branches pin ONE schema member read from the roster; a hand-spelled url desyncs silently on a bump
- frozen per-domain modules are the default import
- Incubating keys enter only with a live consumer, rename risk stated on the consuming row
- Fences citing `ResourceAttributes` or `SpanAttributes` re-author against the owning per-domain module
