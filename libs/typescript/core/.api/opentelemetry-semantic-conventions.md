# [TS_CORE_API_OPENTELEMETRY_SEMANTIC_CONVENTIONS]

`@opentelemetry/semantic-conventions` is a CODE-GENERATED vocabulary of literal-typed string constants — one flat `const` per OpenTelemetry attribute key, metric name, event name, and bounded attribute value — with ZERO runtime dependency and ZERO peer (not even `@opentelemetry/api`). It is the ONE `@opentelemetry/*` package that survives the `[OTEL_PIN_BLOCK]` collapse: when native `Otlp` retires the SDK/exporter block, this stays as the standing name source, because it is pure data, not SDK machinery.

Mechanism is a two-tier × four-family generated pattern: a STABLE entrypoint (`.`) of API-frozen names and an INCUBATING entrypoint (`./incubating`) of overlay names that churn between minor releases, each carrying the `ATTR_*` (attribute key), `*_VALUE_*` (bounded value), `METRIC_*` (instrument name), and `EVENT_*` (event name) families.

Inside Rasm exactly two owners import it — `observe/convention`, the re-export hub whose typed rows every telemetry node names fields through, and `value/fault`, whose `FaultCapture.Forensic` anchors the crash vocabulary directly; runtime's `otel/emit`, `otel/vital`, and `otel/crash` consume the `Convention` rows, never a second semconv import, and a raw string literal at any signal site is the stringy-key defect root policy bans.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/semantic-conventions`
- package: `@opentelemetry/semantic-conventions` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM (`build/esm`) + `esnext` (`build/esnext`); `sideEffects: false`, so unused constants tree-shake to zero bytes.
- exports: TWO subpaths — `.` (STABLE, `build/src/index.d.ts`) and `./incubating` (STABLE + overlay, `build/src/index-incubating.d.ts`). Stable entry re-exports `trace` + `resource` + `stable_attributes` + `stable_metrics` + `stable_events`; incubating adds `experimental_attributes` + `experimental_metrics` + `experimental_events`.
- asset: `build/src/index.d.ts`.
- peer: NONE. deps: NONE. Fully self-contained pure-data package — the property that makes it the `[OTEL_PIN_BLOCK]` survivor.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:telemetry` — but see [STACK]: cross-language parity with C# `Rasm.AppHost` telemetry rides the same OTel names, not this package.
- rail: observability/convention; the `[CONVENTIONS]` pin block (a block of one) — NOT an `[OTEL_PIN_BLOCK]`-collapse member; it is the standing vocabulary source the collapse leaves behind.
- role: the name source for `observe/convention`; every attribute key, metric name, and event name in the four-signal plane resolves to a constant here.

## [02]-[VOCABULARY_PATTERN]

Surface is ONE generated pattern, not a member list: four literal-typed `const` families, each a `NAME → "dotted.string"` binding whose TYPE is the string literal (so a mistyped key is a compile error and the value narrows for discriminated dispatch). A new convention is a generated row in the owning family, never a new export shape. `observe/convention` re-exports the Rasm-relevant subset as typed rows; the design NEVER writes the string literal — it references the constant, and the literal type flows to the OTLP attribute record.

| [INDEX] | [FAMILY]               | [SHAPE]                | [CONSUMER_BOUNDARY]                                                       |
| :-----: | :--------------------- | :--------------------- | :------------------------------------------------------------------------ |
|  [01]   | `ATTR_<NAME>`          | `const: "dotted.key"`  | span/metric/log attribute keys — the field-name vocabulary                |
|  [02]   | `<GROUP>_VALUE_<ENUM>` | `const: "value"`       | an enum attribute's closed value set — a discriminated-union fold value   |
|  [03]   | `METRIC_<NAME>`        | `const: "metric.name"` | `Metric` names on `data` fact-journal meter rows + native OTLP metrics    |
|  [04]   | `EVENT_<NAME>`         | `const: "event.name"`  | span/log event names (`EVENT_EXCEPTION` = `"exception"`, the crash event) |

```ts signature
// Every member is a literal-typed constant — the TYPE is the string, so keys are compile-checked and values narrow:
export declare const ATTR_HTTP_REQUEST_METHOD: "http.request.method"
export declare const ATTR_URL_FULL: "url.full"
export declare const ATTR_SERVICE_NAME: "service.name"
export declare const METRIC_HTTP_SERVER_REQUEST_DURATION: "http.server.request.duration"
export declare const EVENT_EXCEPTION: "exception"
// Crash forensic vocabulary (stable) — value/fault FaultCapture.Forensic anchors the exception/error set and the code.* frame quartet:
export declare const ATTR_ERROR_TYPE: "error.type"
export declare const ATTR_EXCEPTION_MESSAGE: "exception.message"
export declare const ATTR_EXCEPTION_STACKTRACE: "exception.stacktrace"
export declare const ATTR_EXCEPTION_TYPE: "exception.type"
export declare const ATTR_CODE_FUNCTION_NAME: "code.function.name"
export declare const ATTR_CODE_FILE_PATH: "code.file.path"
export declare const ATTR_CODE_LINE_NUMBER: "code.line.number"
export declare const ATTR_CODE_COLUMN_NUMBER: "code.column.number"
// A bounded-value attribute exposes its closed set as *_VALUE_* constants — the smart-enum values a Match fold discriminates over:
export declare const HTTP_REQUEST_METHOD_VALUE_GET: "GET"          // + _POST/_PUT/_DELETE/_HEAD/_OPTIONS/_PATCH/_CONNECT/_TRACE/__OTHER
export declare const DB_SYSTEM_NAME_VALUE_POSTGRESQL: "postgresql" // + _MYSQL/_MARIADB/_MICROSOFT_SQL_SERVER/…
// observe/convention composes the constant into the record — never the literal:
const span = { [ATTR_HTTP_REQUEST_METHOD]: HTTP_REQUEST_METHOD_VALUE_GET, [ATTR_URL_FULL]: url } // typed keys + values
```

## [03]-[TIER_SPLIT]

Load-bearing decision is which entrypoint a namespace comes from. STABLE (`.`) names are API-frozen — safe to embed in durable dashboards, SLO policy rows, and cross-language wire parity. INCUBATING (`./incubating`) names are overlay — a minor release can rename or remove them, so `observe/convention` imports them behind a Rasm-owned alias row that absorbs the churn at one seam. Stable entry is the default import; `/incubating` covers only namespaces stable has not yet promoted.

STABLE (`.`) namespaces — API-frozen, safe for durable dashboards, SLO rows, and cross-language parity:

| [INDEX] | [RASM_NAMESPACES]                                                             | [CONSUMER_BOUNDARY]                                      |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `service.*` (name/version/instance.id/namespace)                              | the `Resource` identity spine — `AppIdentity → resource` |
|  [02]   | `telemetry.sdk.*`/`telemetry.distro.*`, `otel.*`, `network.*`                 | unrowed — consumer-earned admission law                  |
|  [03]   | `http.request.*`/`http.response.*`/`http.route`, `user_agent.original`        | edge ingress http span attrs                             |
|  [04]   | `url.*`, `server.*`/`client.*`                                                | `host/net` client span attrs                             |
|  [05]   | `error.type`, `exception.*`, `EVENT_EXCEPTION`, `code.*`                      | `value/fault` `FaultCapture.Forensic` crash anchors      |
|  [06]   | `deployment.environment.name`; `db.*` (`ATTR_DB_*`, `DB_SYSTEM_NAME_VALUE_*`) | env tag on resource; `db` rows feed `board#QUERY` only   |

INCUBATING (`./incubating`) namespaces — overlay, imported behind the churn-absorbing alias row:

| [INDEX] | [RASM_NAMESPACES]                                         | [CONSUMER_BOUNDARY]                                   |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `browser.*` (brands/language/mobile/platform), `device.*` | vital RUM enrichment through the `Convention` aliases |
|  [02]   | `host.*`, `process.*`, `container.*`, `k8s.*`, `cloud.*`  | resource infra enrichment; iac correlation queries    |

## [04]-[DEPRECATED_LEGACY]

`trace/SemanticAttributes` (`SEMATTRS_*`) and `resource/SemanticResourceAttributes` (`SEMRESATTRS_*`) are the PRE-1.0 enum-object API — a parallel spelling of the same vocabulary, `@deprecated` on every member (each JSDoc redirects to the flat `ATTR_*`). `observe/convention` uses exactly ONE form — the flat `ATTR_*`/`METRIC_*` constants — never the `SEMATTRS_*`/`SEMRESATTRS_*` objects; a row referencing a `SEMATTRS_*` constant is the drift defect.

- `SEMATTRS_*` (e.g. `SEMATTRS_DB_SYSTEM = "db.system"`) — deprecated; superseded by `ATTR_DB_SYSTEM` (incubating).
- `SEMRESATTRS_*` (e.g. `SEMRESATTRS_CLOUD_PROVIDER = "cloud.provider"`) — deprecated; superseded by `ATTR_CLOUD_PROVIDER` (incubating).
- `ATTR_EXCEPTION_ESCAPED` (`"exception.escaped"`) — member-level `@deprecated` in the stable entry: the spec records only escaping exceptions, so the flag carries zero information; no Rasm row references it.
- Older constant-object aggregates (`SemanticAttributes`/`SemanticResourceAttributes` namespaces) are the same retired shape — never composed.

## [05]-[STACKING]

- Stack with `observe/convention` (the primary consumer): the vocabulary spine of the whole plane. `convention.ts` imports the Rasm-relevant `ATTR_*`/`METRIC_*`/`EVENT_*` + `*_VALUE_*` constants (stable by default, incubating behind a churn-absorbing alias row) and re-exports them as typed convention rows; every other telemetry node names its fields through these rows, never a string literal. `*_VALUE_*` families become `Match`-discriminated union values, not free strings.
- Stack with runtime `otel/emit`: the OTLP export lane stamps `Convention` rows on span/metric/log attributes at egress and keys the identity `Resource` from the one `Convention.identity` projection; egress-redaction policy rows scrub PII by attribute key using the same vocabulary as the allow/deny roster.
- Stack with runtime `otel/vital`: browser RUM spans name their fields through the `Convention` incubating alias rows (`browser.*`, `device.*`, `session.*`) beside stable `http.*`/`url.*`; the vital owner reads native `PerformanceObserver` and stamps `Convention` keys on the vital facts.
- Stack with `value/fault` + runtime `otel/crash`: `FaultCapture.Forensic` anchors `exception.*`/`error.type` and the `code.*` frame quartet, `FaultCapture.event` anchors `EVENT_EXCEPTION` — a shared-import boundary beside `observe/convention`, two owners over one spec vocabulary, never a re-export hop; the runtime crash owner constructs captures and parses frames.
- Stack with Rasm-owned vocab that is NOT semconv: the fact-journal audit actor/action/target vocabulary and the meter (app, tenant)-keyed request/compute/storage/token counters (`data` `journal/fact`) are DOMAIN rows Rasm owns, not OTel conventions; they live as `observe/convention` Rasm rows beside the semconv imports. semconv supplies the standard namespaces; the `rasm.*` families are the project's own name space.
- Stack with cross-language parity — C# `Rasm.AppHost/Observability/Telemetry`: the C# OTLP egress emits the SAME OTel attribute names (the wire is OTel, not this package); parity is name-level against the OpenTelemetry spec, so a Rasm span from either language carries `service.name`/`http.route`/`exception.type` identically. This TS package is the JS-side name source, not a shared artifact.

## [06]-[RAIL_LAW]

- Owns: the OpenTelemetry attribute-key / metric-name / event-name / bounded-value vocabulary as code-generated literal-typed constants, split into the stable (`.`) and incubating (`./incubating`) tiers across the `ATTR_*`/`*_VALUE_*`/`METRIC_*`/`EVENT_*` families.
- Accept: the flat `ATTR_*`/`METRIC_*`/`EVENT_*` constants imported into `observe/convention` as typed rows; the `*_VALUE_*` bounded sets as `Match`-discriminated values; stable-tier names by default, incubating names behind a Rasm churn-absorbing alias row; the constant referenced everywhere a field is named, never the string literal.
- Reject: the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` enum-object spelling (superseded by flat `ATTR_*`); a raw string literal where a constant exists (the stringy-key defect root policy bans); embedding an incubating name directly in a durable dashboard/SLO row without the churn-absorbing alias; treating this package as an `[OTEL_PIN_BLOCK]`-collapse member (it is the survivor); re-declaring OTel standard names as Rasm-owned rows (only genuinely-Rasm vocabulary is a project row).
- Boundary: pure data, zero deps — the standing name source the `[OTEL_PIN_BLOCK]` SDK-block retirement leaves behind. Rasm-owned fact vocabularies (audit actor/action/target, meter counters) are NOT semconv and live as project convention rows beside these imports. Cross-language parity is name-level against the OTel spec, not a shared package.
