# [TS_CORE_API_OPENTELEMETRY_SEMANTIC_CONVENTIONS]

`@opentelemetry/semantic-conventions` owns the OpenTelemetry name vocabulary as code-generated literal-typed string constants — one `const` per attribute key, bounded value, metric name, and event name — with zero runtime dependency and zero peer. `observe/convention` re-exports the Rasm subset as typed rows, and a raw string literal at any signal site is the stringy-key defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/semantic-conventions`
- package: `@opentelemetry/semantic-conventions` (Apache-2.0)
- module: dual CJS (`build/src`) + ESM (`build/esm`) + esnext (`build/esnext`); `sideEffects: false` tree-shakes unused constants to zero bytes.
- exports: `.` (stable tier) and `./incubating` (stable and the overlay names); a not-yet-promoted constant resolves only from `./incubating`.
- runtime: isomorphic — pure data, no addon.
- asset: `build/src/index.d.ts`.
- peer: none. deps: none.
- plane: runtime, `scope:telemetry`.
- rail: observability/convention.

## [02]-[VOCABULARY_PATTERN]

Every name is a literal-typed `const` binding `NAME` to its `"dotted.string"`, TYPE narrowed to the literal, so a mistyped key fails compile and the value discriminates dispatch. A new convention is a generated row in its family; `observe/convention` references the constant, never the literal, and the literal type flows to the OTLP attribute record.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ATTR_<NAME>`          | attribute key | span/metric/log field-name vocabulary; value is the dotted key |
|  [02]   | `<GROUP>_VALUE_<ENUM>` | bounded value | an enum attribute's closed value set — a union fold value      |
|  [03]   | `METRIC_<NAME>`        | metric name   | `Metric` names on `data` meter rows + native OTLP metrics      |
|  [04]   | `EVENT_<NAME>`         | event name    | span/log event names; `EVENT_EXCEPTION` = `"exception"` crash  |

[SURFACES]: `ATTR_HTTP_REQUEST_METHOD` `ATTR_URL_FULL` `ATTR_SERVICE_NAME` `METRIC_HTTP_SERVER_REQUEST_DURATION` `EVENT_EXCEPTION` `ATTR_ERROR_TYPE` `ATTR_EXCEPTION_MESSAGE` `ATTR_EXCEPTION_STACKTRACE` `ATTR_EXCEPTION_TYPE` `ATTR_CODE_FUNCTION_NAME` `ATTR_CODE_FILE_PATH` `ATTR_CODE_LINE_NUMBER` `ATTR_CODE_COLUMN_NUMBER` `HTTP_REQUEST_METHOD_VALUE_GET` `DB_SYSTEM_NAME_VALUE_POSTGRESQL`

## [03]-[TIER_SPLIT]

Which entrypoint a namespace resolves from is the load-bearing decision. Stable (`.`) names are API-frozen — safe in durable dashboards, SLO rows, and cross-language parity. Incubating (`./incubating`) names are overlay, renamed or dropped between minor releases, so `observe/convention` imports them behind a Rasm alias row absorbing the churn at one seam.

Stable (`.`) namespaces, imported by default:

| [INDEX] | [NAMESPACE]                                                                   | [CONSUMER]                                               |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `service.*` (name/version/instance.id/namespace)                              | the `Resource` identity spine — `AppIdentity → resource` |
|  [02]   | `telemetry.sdk.*`/`telemetry.distro.*`, `otel.*`, `network.*`                 | unrowed — consumer-earned admission law                  |
|  [03]   | `http.request.*`/`http.response.*`/`http.route`, `user_agent.original`        | edge ingress http span attrs                             |
|  [04]   | `url.*`, `server.*`/`client.*`                                                | `host/net` client span attrs                             |
|  [05]   | `error.type`, `exception.*`, `EVENT_EXCEPTION`, `code.*`                      | `value/fault` `FaultCapture.Forensic` crash anchors      |
|  [06]   | `deployment.environment.name`; `db.*` (`ATTR_DB_*`, `DB_SYSTEM_NAME_VALUE_*`) | env tag on resource; `db` rows feed `board#QUERY` only   |

Incubating (`./incubating`) namespaces, imported behind the alias row:

| [INDEX] | [NAMESPACE]                                               | [CONSUMER]                                            |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `browser.*` (brands/language/mobile/platform), `device.*` | vital RUM enrichment through the `Convention` aliases |
|  [02]   | `host.*`, `process.*`, `container.*`, `k8s.*`, `cloud.*`  | resource infra enrichment; iac correlation queries    |

## [04]-[STACKING]

- `observe/convention` (primary consumer): the plane's vocabulary spine. It imports the Rasm-relevant `ATTR_*`/`METRIC_*`/`EVENT_*`/`*_VALUE_*` constants — stable by default, incubating behind a churn-absorbing alias row — and re-exports them as typed rows every telemetry node names fields through; `*_VALUE_*` families become `Match`-discriminated union values.
- runtime `otel/emit`: the OTLP export lane stamps `Convention` rows on span/metric/log attributes at egress and keys the identity `Resource` from the one `Convention.identity` projection; egress-redaction rows scrub PII by attribute key against the same vocabulary.
- runtime `otel/vital`: browser RUM spans name fields through the incubating alias rows (`browser.*`, `device.*`, `session.*`) beside stable `http.*`/`url.*`; the owner reads native `PerformanceObserver` and stamps `Convention` keys on the vital facts.
- `value/fault` + runtime `otel/crash`: `FaultCapture.Forensic` anchors `exception.*`/`error.type` and the `code.*` frame quartet, `FaultCapture.event` anchors `EVENT_EXCEPTION` — a shared-import boundary beside `observe/convention`, two owners over one spec vocabulary, never a re-export hop.
- cross-language parity, C# `Rasm.AppHost/Observability/Telemetry`: the wire is OTel, so parity is name-level against the spec — a Rasm span from either language carries `service.name`/`http.route`/`exception.type` identically, and this package is the JS-side name source, not a shared artifact.

## [05]-[RAIL_LAW]

- Package: `@opentelemetry/semantic-conventions`
- Owns: the OpenTelemetry attribute-key / metric-name / event-name / bounded-value vocabulary as literal-typed constants, split into the stable (`.`) and incubating (`./incubating`) tiers across the `ATTR_*`/`*_VALUE_*`/`METRIC_*`/`EVENT_*` families.
- Accept: the flat `ATTR_*`/`METRIC_*`/`EVENT_*` constants imported into `observe/convention` as typed rows; the `*_VALUE_*` bounded sets as `Match`-discriminated values; stable names by default and incubating names behind the Rasm churn-absorbing alias row; the constant referenced wherever a field is named.
- Reject: a raw string literal where a constant exists (the stringy-key defect); an incubating name embedded directly in a durable dashboard or SLO row without the churn-absorbing alias; treating this package as an `[OTEL_PIN_BLOCK]`-collapse member; re-declaring an OTel standard name as a Rasm-owned row.
- Boundary: pure data, zero deps — the standing name source the `[OTEL_PIN_BLOCK]` SDK-block retirement leaves behind. Rasm-owned fact vocabularies (audit actor/action/target, meter counters) are project convention rows beside these imports, and cross-language parity is name-level against the OTel spec, not a shared package.
