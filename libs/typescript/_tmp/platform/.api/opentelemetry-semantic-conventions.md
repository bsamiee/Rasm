# [API_CATALOGUE] @opentelemetry/semantic-conventions

`@opentelemetry/semantic-conventions` is the code-generated OTel attribute/metric/event NAME vocabulary: a flat table of `const` string literals (`ATTR_*` attribute keys, `METRIC_*` instrument names, `EVENT_*` event names, `<FIELD>_VALUE_<VARIANT>` bounded-field value constants) emitted from the upstream semantic-conventions YAML spec, split into a GA-stable barrel (`.`) and an incubating superset (`./incubating`). It carries no runtime logic — the constants ARE the strings, typed as their own literal. In the `platform` telemetry stack it is the primary `[R3]` convention-vocabulary survivor: when the `@effect/opentelemetry` native `Otlp` lane retires the `@opentelemetry/sdk-*` + `exporter-*` machinery (`libs/typescript/.api/effect-opentelemetry.md` `[R3]`), this vocabulary source stays alongside the surviving `@opentelemetry/core` W3C propagation family and the `@opentelemetry/resources` `Resource`-identity substrate — `Observability/telemetry.md` re-exports the stable keys as the span/metric annotation vocabulary every `Effect.withSpan`/`Metric` call reads, and `@opentelemetry/resources` + `@effect/opentelemetry` `Resource` stamp the `ATTR_SERVICE_*` identity keys from here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/semantic-conventions`
- package: `@opentelemetry/semantic-conventions`
- version: `1.41.1` (central pin `pnpm-workspace.yaml`; a transitive `1.28.0` also resides in the store — pin 1.41.1 is the consumer-bound surface, verify against it, never the fallback)
- license: `Apache-2.0`
- api-peer: none — `peerDependencies` is `null`; pure string constants carry zero `@opentelemetry/api` dependency (the base signal packages own the api peer)
- module: `@opentelemetry/semantic-conventions` (stable barrel `build/src/index.d.ts`) + `@opentelemetry/semantic-conventions/incubating` (`build/src/index-incubating.d.ts`); exactly TWO entry-points, no other subpaths
- runtime: universal — pure string constants, zero platform branch, browser + node + edge identical
- asset: runtime constant library (build-time-resolvable literals) — side-effects-free (`sideEffects: false`), fully tree-shakeable, unused constants drop at bundle time; import only the keys a signal path stamps
- rail: observability — the signal NAME vocabulary
- collapse-fence: `[R3]`-SURVIVOR — the convention-vocabulary survivor `Observability/telemetry.md` re-exports (the primary DIRECT-import survivor; the `@opentelemetry/core` W3C propagation family and the `@opentelemetry/resources` `Resource`-identity substrate also persist, while the sdk-*/exporter machinery collapses); NOT fenced to `scope:telemetry` (pure vocabulary, usable at any annotation site), admitted wherever a span/metric/resource is annotated

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: constant families — one spec-generated table, five naming axes
- rail: observability
- These are NOT hand-authored rosters; every symbol is generated from the OTel semconv YAML registry. The catalogue documents the naming AXES + the families `platform` stamps, never the 76 stable `ATTR_*` / 51 stable `METRIC_*` / N incubating members verbatim. A new key is a spec version bump (`resolve` re-verifies), never a locally-added constant.

| [INDEX] | [SYMBOL_FAMILY]                     | [TYPE_FAMILY]      | [ENTRY]            | [CONSUMER / BOUNDARY]                                            |
| :-----: | :---------------------------------- | :----------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `ATTR_*`                            | `const` string lit | stable `.`         | span + resource attribute KEYS (76 stable; e.g. `ATTR_HTTP_REQUEST_METHOD` = `"http.request.method"`) |
|  [02]   | `METRIC_*`                          | `const` string lit | stable `.`         | instrument NAMES (51 stable) for `Metric.*`/`meter.create*`     |
|  [03]   | `EVENT_*`                           | `const` string lit | stable `.`         | event NAMES — stable barrel carries exactly one, `EVENT_EXCEPTION` = `"exception"` |
|  [04]   | `<FIELD>_VALUE_<VARIANT>`           | `const` string lit | stable `.`         | bounded-field enum VALUES (69 stable; e.g. `HTTP_REQUEST_METHOD_VALUE_GET` = `"GET"`) — no `ATTR_` prefix; replaces the removed `*Values` enum objects |
|  [05]   | `SEMATTRS_*` / `SEMRESATTRS_*`      | `const` string lit | stable `.` (legacy)| `@deprecated` — still re-exported via `./trace`+`./resource`, each pointing at an incubating `ATTR_*`; NEVER cite in new code |
|  [06]   | `ATTR_*` / `METRIC_*` / `EVENT_*` (experimental) | `const` string lit | `./incubating` | unstable keys subject to breaking rename (`ATTR_DB_SYSTEM_NAME`, `EVENT_GEN_AI_*`); gated behind an explicit experimental-acceptance policy |

[PUBLIC_TYPE_SCOPE]: stable `ATTR_*` namespaces the browser SPA stamps
- rail: observability
- The namespace prefix IS the axis; a `platform` span/resource annotation composes keys from these families, never a raw `"service.name"` string.

| [INDEX] | [NAMESPACE_PREFIX]     | [EXAMPLE_CONSTANT]             | [DOMAIN]                                          |
| :-----: | :--------------------- | :----------------------------- | :------------------------------------------------ |
|  [01]   | `ATTR_SERVICE_*`       | `ATTR_SERVICE_NAME`            | resource identity — all four stable (see `[03]`)  |
|  [02]   | `ATTR_TELEMETRY_SDK_*` | `ATTR_TELEMETRY_SDK_NAME`      | SDK identity resource attrs                       |
|  [03]   | `ATTR_HTTP_*`          | `ATTR_HTTP_REQUEST_METHOD`     | HTTP client/server spans (fetch instrumentation)  |
|  [04]   | `ATTR_URL_*`           | `ATTR_URL_FULL`                | URL decomposition on outbound spans               |
|  [05]   | `ATTR_ERROR_*`         | `ATTR_ERROR_TYPE`              | error classification on a failed span             |
|  [06]   | `ATTR_EXCEPTION_*`     | `ATTR_EXCEPTION_MESSAGE`       | exception recording (`EVENT_EXCEPTION` payload)   |
|  [07]   | `ATTR_OTEL_*`          | `ATTR_OTEL_STATUS_DESCRIPTION` | OTel span-status metadata                         |
|  [08]   | `ATTR_USER_AGENT_*`    | `ATTR_USER_AGENT_ORIGINAL`     | browser user-agent on RUM spans                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the two entry-points
- rail: observability
- The stable barrel re-exports `./stable_attributes` + `./stable_metrics` + `./stable_events` + the legacy `./trace` (`SEMATTRS_*`) + `./resource` (`SEMRESATTRS_*`); `./incubating` re-exports the stable set PLUS `./experimental_{attributes,metrics,events}`. There is no `/experimental` subpath.

| [INDEX] | [IMPORT_PATH]                                    | [CONTENT]                                                                        |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `@opentelemetry/semantic-conventions`            | stable `ATTR_*`/`METRIC_*`/`EVENT_EXCEPTION`/`*_VALUE_*` + deprecated `SEMATTRS_*`/`SEMRESATTRS_*` |
|  [02]   | `@opentelemetry/semantic-conventions/incubating` | stable set + experimental `ATTR_*`/`METRIC_*`/`EVENT_*` (breaking-change lane)   |

[ENTRYPOINT_SCOPE]: function-valued header constants
- rail: observability
- Two stable `ATTR_*` are FACTORIES, not literals — they mint a per-header attribute key at runtime.

| [INDEX] | [SURFACE]                        | [SIGNATURE]               |
| :-----: | :------------------------------- | :------------------------ |
|  [01]   | `ATTR_HTTP_REQUEST_HEADER(key)`  | `(key: string) => string` |
|  [02]   | `ATTR_HTTP_RESPONSE_HEADER(key)` | `(key: string) => string` |

[ENTRYPOINT_SCOPE]: the resource-identity key set (all stable)
- rail: observability
- The exact keys `@opentelemetry/resources` + `@effect/opentelemetry` `Resource` stamp; all four resolve from the STABLE barrel, so the identity lane needs no incubating import.

| [INDEX] | [CONSTANT]                 | [KEY]                   |
| :-----: | :------------------------- | :---------------------- |
|  [01]   | `ATTR_SERVICE_NAME`        | `service.name`          |
|  [02]   | `ATTR_SERVICE_VERSION`     | `service.version`       |
|  [03]   | `ATTR_SERVICE_NAMESPACE`   | `service.namespace`     |
|  [04]   | `ATTR_SERVICE_INSTANCE_ID` | `service.instance.id`   |

## [04]-[IMPLEMENTATION_LAW]

[SEMCONV_TOPOLOGY]:
- every `ATTR_*` is a literal STRING typed as its own value (`ATTR_SERVICE_NAME: "service.name"`), not an object — use it directly as the key; there is no wrapper to unwrap.
- `<FIELD>_VALUE_<VARIANT>` constants are the allowed values for a bounded `ATTR_*` field (`HTTP_REQUEST_METHOD_VALUE_GET`); stamp the VALUE constant, never a raw `"GET"`, so a semconv version upgrade that renames a variant is a type break, not a silent drift.
- stable vs incubating is the stability contract: `.` keys are frozen, `./incubating` keys can rename between minors — an incubating key in a production signal path is admitted only under an explicit experimental-acceptance policy row.
- `SEMATTRS_*`/`SEMRESATTRS_*` survive in 1.41.1 (re-exported from the STABLE barrel via `./trace`+`./resource`) but are `@deprecated`; most map to an INCUBATING `ATTR_*` (they never stabilized), so a blind `SEMATTRS_*` → `ATTR_*` swap can silently move a key from stable to incubating — migrate deliberately, checking each replacement's entry-point.

[INTEGRATION_LAW]:
- Stack with `@opentelemetry/resources` + `@effect/opentelemetry` `Resource`: the identity resource is built from `{ [ATTR_SERVICE_NAME]: id.name, [ATTR_SERVICE_VERSION]: id.version, [ATTR_SERVICE_NAMESPACE]: id.namespace, [ATTR_SERVICE_INSTANCE_ID]: id.instance }` lowered through `Resource.layer({ serviceName, ... })` (`libs/typescript/_tmp/platform/.api/opentelemetry-resources.md` `[INTEGRATION_LAW]` names this exact reciprocal); the `AppIdentity`-derived keys are semconv constants, never hardcoded strings.
- Stack with the OTLP exporters + `sdk-trace-web`: outbound RUM spans stamp `ATTR_HTTP_*`/`ATTR_URL_*`/`ATTR_USER_AGENT_*` keys and failed spans stamp `ATTR_ERROR_TYPE`/`ATTR_EXCEPTION_*` + `EVENT_EXCEPTION`; the exporter (`libs/typescript/_tmp/platform/.api/exporter-trace-otlp-http.md`) serializes those annotated spans verbatim — the vocabulary is fixed here, the transport there.
- Stack with `effect` `Effect.withSpan`/`Metric` (`libs/typescript/.api/effect.md`): `Observability/telemetry.md` re-exports the consumed subset as ITS span/metric key vocabulary; instrumentation code annotates via `Effect.withSpan(name, { attributes: { [ATTR_HTTP_REQUEST_METHOD]: method } })` and names instruments with `METRIC_*` — the design page owns the re-export so a semconv version bump changes one import site, not every call.
- Stack with `Observability/vitals.md` + `crash.md`: web-vitals breach spans and the crash `EVENT_EXCEPTION` payload draw their keys (`ATTR_EXCEPTION_MESSAGE`, `ATTR_EXCEPTION_STACKTRACE`) from the same table, so a browser-emitted exception frame is wire-identical to a server-emitted one on the collector.

[LOCAL_ADMISSION]:
- import the stable barrel for every production span/resource/metric name; reach `./incubating` only under a named experimental-acceptance policy and record the incubating key as breaking-change-exposed.
- unlike the `@opentelemetry/sdk-*`/`exporter-*` block, semconv is NOT fenced to `scope:telemetry` — it is a pure-string vocabulary usable at any annotation site; centralize the CONSUMED keys behind the `Observability/telemetry.md` re-export so folders read one vocabulary surface, not scattered semconv imports.
- prefer `<FIELD>_VALUE_*` constants over raw enum strings on bounded fields; use the `ATTR_HTTP_{REQUEST,RESPONSE}_HEADER(key)` factories for per-header keys, never string concatenation.

[RAIL_LAW]:
- Package: `@opentelemetry/semantic-conventions`
- Owns: the canonical OTel span/resource attribute-key, metric-name, event-name, and bounded-field value vocabulary; the stable/incubating stability split
- Accept: stable-barrel `ATTR_*`/`METRIC_*`/`EVENT_EXCEPTION`/`*_VALUE_*` for all naming; the `ATTR_SERVICE_*` set for the shared resource identity; the header-key factories; `./incubating` only under an explicit experimental policy
- Reject: hardcoded attribute/metric name strings; raw enum values where a `*_VALUE_*` constant exists; the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` surface; the phantom `/experimental` subpath (the real incubating path is `/incubating`); locally-added constants (a missing key is a spec bump, not a hand-written literal); treating this as a collapsing SDK dependency (it survives `[R3]`)
