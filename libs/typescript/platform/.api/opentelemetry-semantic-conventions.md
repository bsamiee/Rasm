# [API_CATALOGUE] @opentelemetry/semantic-conventions

`@opentelemetry/semantic-conventions` supplies stable `ATTR_*` attribute name constants, `METRIC_*` metric name constants, deprecated `SEMATTRS_*` and `SEMRESATTRS_*` legacy constants, and an incubating entry-point (`index-incubating`) that adds experimental attribute and metric constants for use in span and resource annotation across the platform signal pipeline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/semantic-conventions`
- package: `@opentelemetry/semantic-conventions`
- module: `@opentelemetry/semantic-conventions`
- asset: runtime library
- rail: observability

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: stable entry-point exports
- rail: observability

| [INDEX] | [SYMBOL_FAMILY]           | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `ATTR_*` constants        | const string  | stable span and resource attribute name strings |
|  [02]   | `METRIC_*` constants      | const string  | stable metric instrument name strings           |
|  [03]   | `*_VALUE_*` constants     | const string  | enum value strings for bounded `ATTR_*` fields  |
|  [04]   | `SEMATTRS_*` constants    | const string  | deprecated legacy span attribute names          |
|  [05]   | `SEMRESATTRS_*` constants | const string  | deprecated legacy resource attribute names      |

[PUBLIC_TYPE_SCOPE]: stable ATTR_ attribute namespaces
- rail: observability

| [INDEX] | [NAMESPACE_PREFIX]     | [EXAMPLE_CONSTANT]             | [DOMAIN]                    |
| :-----: | :--------------------- | :----------------------------- | :-------------------------- |
|  [01]   | `ATTR_CLIENT_*`        | `ATTR_CLIENT_ADDRESS`          | client identity             |
|  [02]   | `ATTR_DB_*`            | `ATTR_DB_SYSTEM_NAME`          | database operations         |
|  [03]   | `ATTR_ERROR_*`         | `ATTR_ERROR_TYPE`              | error classification        |
|  [04]   | `ATTR_EXCEPTION_*`     | `ATTR_EXCEPTION_MESSAGE`       | exception recording         |
|  [05]   | `ATTR_HTTP_*`          | `ATTR_HTTP_REQUEST_METHOD`     | HTTP spans                  |
|  [06]   | `ATTR_NETWORK_*`       | `ATTR_NETWORK_PROTOCOL_NAME`   | network layer               |
|  [07]   | `ATTR_OTEL_*`          | `ATTR_OTEL_STATUS_DESCRIPTION` | OTel SDK metadata           |
|  [08]   | `ATTR_SERVER_*`        | `ATTR_SERVER_ADDRESS`          | server identity             |
|  [09]   | `ATTR_SERVICE_*`       | `ATTR_SERVICE_NAME`            | service identity            |
|  [10]   | `ATTR_TELEMETRY_SDK_*` | `ATTR_TELEMETRY_SDK_NAME`      | SDK identity resource attrs |
|  [11]   | `ATTR_URL_*`           | `ATTR_URL_FULL`                | URL decomposition           |
|  [12]   | `ATTR_USER_AGENT_*`    | `ATTR_USER_AGENT_ORIGINAL`     | HTTP user agent             |

[PUBLIC_TYPE_SCOPE]: stable METRIC_ metric namespaces
- rail: observability

| [INDEX] | [CONSTANT]                            | [METRIC_NAME]                  | [DOMAIN]        |
| :-----: | :------------------------------------ | :----------------------------- | :-------------- |
|  [01]   | `METRIC_DB_CLIENT_OPERATION_DURATION` | `db.client.operation.duration` | database client |
|  [02]   | `METRIC_HTTP_CLIENT_REQUEST_DURATION` | `http.client.request.duration` | HTTP client     |
|  [03]   | `METRIC_HTTP_SERVER_REQUEST_DURATION` | `http.server.request.duration` | HTTP server     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: entry-point modules
- rail: observability

| [INDEX] | [IMPORT_PATH]                                      | [CONTENT]                                                             |
| :-----: | :------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `@opentelemetry/semantic-conventions`              | stable `ATTR_*`, `METRIC_*`, deprecated `SEMATTRS_*`, `SEMRESATTRS_*` |
|  [02]   | `@opentelemetry/semantic-conventions/experimental` | stable + experimental `ATTR_*` and `METRIC_*`                         |

[ENTRYPOINT_SCOPE]: function-valued attribute constants
- rail: observability

| [INDEX] | [SURFACE]                        | [SIGNATURE]               |
| :-----: | :------------------------------- | :------------------------ |
|  [01]   | `ATTR_HTTP_REQUEST_HEADER(key)`  | `(key: string) => string` |
|  [02]   | `ATTR_HTTP_RESPONSE_HEADER(key)` | `(key: string) => string` |

## [04]-[IMPLEMENTATION_LAW]

[SEMCONV_TOPOLOGY]:
- The stable entry-point (`@opentelemetry/semantic-conventions`) exports only GA-stable constants; the incubating entry-point adds experimental constants subject to breaking change
- `ATTR_*` constants are typed as literal string types (e.g. `"service.name"`); they are the string values themselves, not objects — use them directly as attribute keys
- `*_VALUE_*` constants are the allowed enum values for bounded `ATTR_*` fields; use them instead of raw strings to remain compatible with semconv version upgrades
- `SEMATTRS_*` and `SEMRESATTRS_*` are deprecated; migrate to the corresponding `ATTR_*` constant

[LOCAL_ADMISSION]:
- Import from the stable entry-point for production span and resource attribute keys
- Import function-valued constants (`ATTR_HTTP_REQUEST_HEADER`, `ATTR_HTTP_RESPONSE_HEADER`) to generate per-header attribute names at runtime
- Service identity attributes (`ATTR_SERVICE_NAME`, `ATTR_SERVICE_VERSION`, `ATTR_SERVICE_NAMESPACE`, `ATTR_SERVICE_INSTANCE_ID`) are the resource keys consumed by `@opentelemetry/resources` detectors and `@effect/opentelemetry`'s `Resource.layer`

[RAIL_LAW]:
- Package: `@opentelemetry/semantic-conventions`
- Owns: canonical OTel attribute and metric name string constants
- Accept: `ATTR_*` and `METRIC_*` from the stable entry-point for all attribute and metric naming
- Reject: hardcoded attribute name strings; incubating constants in production signal paths without explicit experimental acceptance policy
