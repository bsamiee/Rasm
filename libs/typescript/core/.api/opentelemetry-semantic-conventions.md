# [@opentelemetry/semantic-conventions] — the attribute/metric/event name registry the convention vocabulary sources; the sole `[R3]` survivor

`@opentelemetry/semantic-conventions` is a CODE-GENERATED vocabulary of literal-typed string constants — one flat `const` per OpenTelemetry attribute key, metric name, event name, and bounded attribute value — with ZERO runtime dependency and ZERO peer (not even `@opentelemetry/api`). It is the ONE `@opentelemetry/*` package that survives the `[R3]` collapse: when native `Otlp` retires the SDK/exporter block, this stays as the standing name source, because it is pure data, not SDK machinery. The mechanism is not a roster — it is a two-tier × four-family generated pattern: a STABLE entrypoint (`.`) of API-frozen names and an INCUBATING entrypoint (`./incubating`) of experimental names that can churn between minor releases, each carrying the `ATTR_*` (attribute key), `*_VALUE_*` (bounded value), `METRIC_*` (instrument name), and `EVENT_*` (event name) families. Inside Rasm `observe/convention` imports these as data rows so every span/metric/log names its fields through a typed constant, never a string literal (root policy bans stringy attribute keys); `otlp/export` stamps them on egress, and the `Resource` identity spine, `signal/vital` RUM, and `signal/crash` fault capture each fold the relevant namespace.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/semantic-conventions`
- package: `@opentelemetry/semantic-conventions` · version `1.41.1` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM (`build/esm`) + `esnext` (`build/esnext`); `sideEffects: false`, so unused constants tree-shake to zero bytes.
- exports: TWO subpaths — `.` (STABLE, `build/src/index.d.ts`) and `./incubating` (STABLE + EXPERIMENTAL, `build/src/index-incubating.d.ts`). The stable entry is `trace` + `resource` + `stable_attributes` + `stable_metrics` + `stable_events`; incubating adds `experimental_attributes` + `experimental_metrics` + `experimental_events`.
- asset: TSDECL `build/src/index.d.ts` (`assay api resolve @opentelemetry/semantic-conventions` → `1.41.1`, restored).
- peer: NONE. deps: NONE. Fully self-contained pure-data package — the property that makes it the `[R3]` survivor.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:telemetry` — but see [STACK]: cross-language parity with C# `Rasm.AppHost` telemetry rides the same OTel names, not this package.
- rail: observability/convention; the `[CONVENTIONS]` pin block (a block of one) — NOT an `[R3]`-collapse member; it is the standing vocabulary source the collapse leaves behind.
- role: the name source for `observe/convention`; every attribute key, metric name, and event name in the four-signal plane resolves to a constant here.

## [02]-[VOCABULARY_PATTERN]

The surface is ONE generated pattern, not a member list: four literal-typed `const` families, each a `NAME → "dotted.string"` binding whose TYPE is the string literal (so a mistyped key is a compile error and the value narrows for discriminated dispatch). A new convention is a generated row in the owning family, never a new export shape. `observe/convention` re-exports the Rasm-relevant subset as typed rows; the design NEVER writes the string literal — it references the constant, and the literal type flows to the OTLP attribute record.

| [INDEX] | [FAMILY]                     | [SHAPE]                                   | [CONSUMER / BOUNDARY]                                                         |
| :-----: | :--------------------------- | :---------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ATTR_<NAME>`                | `const: "dotted.key"` (attribute key)     | span/metric/log attribute keys — the field-name vocabulary                   |
|  [02]   | `<GROUP>_VALUE_<ENUM>`       | `const: "value"` (bounded attribute value)| the closed value set for an enum attribute — a discriminated-union fold value |
|  [03]   | `METRIC_<NAME>`              | `const: "metric.name"` (instrument name)  | `Metric` instrument names on `signal/meter` + native OTLP metrics            |
|  [04]   | `EVENT_<NAME>`               | `const: "event.name"` (event name)        | span/log event names (`EVENT_EXCEPTION` = `"exception"`, the crash event)    |

```ts contract
// Every member is a literal-typed constant — the TYPE is the string, so keys are compile-checked and values narrow:
export declare const ATTR_HTTP_REQUEST_METHOD: "http.request.method"
export declare const ATTR_URL_FULL: "url.full"
export declare const ATTR_SERVICE_NAME: "service.name"
export declare const METRIC_HTTP_SERVER_REQUEST_DURATION: "http.server.request.duration"
export declare const EVENT_EXCEPTION: "exception"
// A bounded-value attribute exposes its closed set as *_VALUE_* constants — the smart-enum values a Match fold discriminates over:
export declare const HTTP_REQUEST_METHOD_VALUE_GET: "GET"          // + _POST/_PUT/_DELETE/_HEAD/_OPTIONS/_PATCH/_CONNECT/_TRACE/__OTHER
export declare const DB_SYSTEM_NAME_VALUE_POSTGRESQL: "postgresql" // + _MYSQL/_MARIADB/_MICROSOFT_SQL_SERVER/…
// observe/convention composes the constant into the record — never the literal:
const span = { [ATTR_HTTP_REQUEST_METHOD]: HTTP_REQUEST_METHOD_VALUE_GET, [ATTR_URL_FULL]: url } // typed keys + values
```

## [03]-[TIER_SPLIT]

The load-bearing decision is which entrypoint a namespace comes from. STABLE (`.`) names are API-frozen — safe to embed in durable dashboards, SLO policy rows, and cross-language wire parity. INCUBATING (`./incubating`) names are experimental — a minor release can rename or remove them, so `observe/convention` imports them behind a Rasm-owned alias row that absorbs the churn at one seam. The stable entry is the default import; reach for `/incubating` only for a namespace stable has not yet promoted.

| [INDEX] | [TIER]                          | [RASM NAMESPACES]                                                                                      | [CONSUMER / BOUNDARY]                                          |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | STABLE `.`                      | `service.*` (name/version/instance.id/namespace), `telemetry.sdk.*`/`telemetry.distro.*`               | the `Resource` identity spine — `AppIdentity → resource`      |
|  [02]   | STABLE `.`                      | `http.request.*`/`http.response.*`/`http.route`, `url.*`, `network.*`, `server.*`/`client.*`, `user_agent.original` | edge ingress + `host/net` client span attrs        |
|  [03]   | STABLE `.`                      | `error.type`, `exception.*` (type/message/stacktrace/escaped), `EVENT_EXCEPTION`, `code.*`, `otel.*`   | `signal/crash` fault capture → `FaultDetail`                  |
|  [04]   | STABLE `.`                      | `deployment.environment.name`; `db.*` (`ATTR_DB_*`, `DB_SYSTEM_NAME_VALUE_*`)                          | env tag on resource; `store` DB-client spans                 |
|  [05]   | INCUBATING `./incubating`       | `browser.*` (brands/language/mobile/platform), `device.*`                                              | `signal/vital` browser RUM enrichment                        |
|  [06]   | INCUBATING `./incubating`       | `host.*`, `process.*`, `container.*`, `k8s.*`, `cloud.*`                                               | resource infra enrichment; `iac/observe` correlation         |

## [04]-[DEPRECATED_LEGACY]

The `trace/SemanticAttributes` (`SEMATTRS_*`) and `resource/SemanticResourceAttributes` (`SEMRESATTRS_*`) modules are the PRE-1.0 enum-object API — a parallel spelling of the same vocabulary, re-exported from the stable entry for back-compat and marked `@deprecated` on every member (each JSDoc redirects to the flat `ATTR_*` in the incubating entry). This is a two-spellings-for-one-concept collapse point: the design uses exactly ONE form — the flat `ATTR_*`/`METRIC_*` constants — and never the `SEMATTRS_*`/`SEMRESATTRS_*` objects. A `observe/convention` row referencing a `SEMATTRS_*` constant is the drift defect.

- `SEMATTRS_*` (e.g. `SEMATTRS_DB_SYSTEM = "db.system"`) — deprecated; superseded by `ATTR_DB_SYSTEM` (incubating).
- `SEMRESATTRS_*` (e.g. `SEMRESATTRS_CLOUD_PROVIDER = "cloud.provider"`) — deprecated; superseded by `ATTR_CLOUD_PROVIDER` (incubating).
- Older constant-object aggregates (`SemanticAttributes`/`SemanticResourceAttributes` namespaces) are the same legacy shape — never composed.

## [05]-[STACKING]

- [STACK: `observe/convention` (the primary consumer)] — the vocabulary spine of the whole plane. `convention.ts` imports the Rasm-relevant `ATTR_*`/`METRIC_*`/`EVENT_*` + `*_VALUE_*` constants (stable by default, incubating behind a churn-absorbing alias row) and re-exports them as typed convention rows; every other `telemetry` node names its fields through these rows, never a string literal. The `*_VALUE_*` families become `Match`-discriminated union values, not free strings.
- [STACK: `.api/effect-opentelemetry.md` `Otlp` export + `Resource`] — `otlp/export` stamps convention keys on span/metric/log attributes at egress; `Resource.layer({ serviceName, serviceVersion, attributes })` keys the identity resource with `ATTR_SERVICE_NAME`/`ATTR_SERVICE_VERSION`/`ATTR_SERVICE_INSTANCE_ID` (stable) + `telemetry.sdk.*`, derived from the one `AppIdentity` value. The egress-redaction policy rows scrub PII by attribute key using these constants as the allow/deny vocabulary.
- [STACK: `signal/vital` + `.api/opentelemetry-sdk-trace-web.md` RUM] — browser RUM spans name their fields with `browser.*` (incubating) + `http.*`/`url.*` (stable); the `sdk-trace-web` resource-timing folds attach `*_body_size`/network attrs by these keys. `signal/vital` reads native `PerformanceObserver` (zero web-vitals) and stamps convention keys on the vital facts.
- [STACK: `signal/crash` fault capture] — crash capture reconstructs `FaultDetail` and names it with `exception.type`/`exception.message`/`exception.stacktrace`/`exception.escaped` + `error.type` (stable) and the `EVENT_EXCEPTION` event name — through the kernel fault-enricher contract, no `wire` import.
- [STACK: Rasm-owned vocab that is NOT semconv] — the `signal/audit` actor/action/target vocabulary and the `signal/meter` (app, tenant)-keyed request/compute/storage/token counters are DOMAIN rows Rasm owns, not OTel conventions; they live as `observe/convention` Rasm rows beside the semconv imports. semconv supplies the standard namespaces; the audit/meter fact families are the project's own name space.
- [STACK: cross-language parity — C# `Rasm.AppHost/Observability/Telemetry`] — the C# OTLP egress emits the SAME OTel attribute names (the wire is OTel, not this package); parity is name-level against the OpenTelemetry spec, so a Rasm span from either language carries `service.name`/`http.route`/`exception.type` identically. This TS package is the JS-side name source, not a shared artifact.

## [06]-[RAIL_LAW]

- Owns: the OpenTelemetry attribute-key / metric-name / event-name / bounded-value vocabulary as code-generated literal-typed constants, split into the stable (`.`) and incubating (`./incubating`) tiers across the `ATTR_*`/`*_VALUE_*`/`METRIC_*`/`EVENT_*` families.
- Accept: the flat `ATTR_*`/`METRIC_*`/`EVENT_*` constants imported into `observe/convention` as typed rows; the `*_VALUE_*` bounded sets as `Match`-discriminated values; stable-tier names by default, incubating names behind a Rasm churn-absorbing alias row; the constant referenced everywhere a field is named, never the string literal.
- Reject: the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` enum-object spelling (superseded by flat `ATTR_*`); a raw string literal where a constant exists (the stringy-key defect root policy bans); embedding an incubating name directly in a durable dashboard/SLO row without the churn-absorbing alias; treating this package as an `[R3]`-collapse member (it is the survivor); re-declaring OTel standard names as Rasm-owned rows (only genuinely-Rasm vocab — audit/meter — is a project row).
- Boundary: pure data, zero deps — the standing name source the `[R3]` SDK-block retirement leaves behind. Rasm-owned fact vocabularies (audit actor/action/target, meter counters) are NOT semconv and live as project convention rows beside these imports. Cross-language parity is name-level against the OTel spec, not a shared package.
