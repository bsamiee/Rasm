# [RUNTIME_INSTRUMENT]

`Instrument` under the `browser` condition owns document auto-instrumentation: one `_rows` roster covering document load, both request surfaces, and admitted user interaction, one `ZoneContextManager` install published as `Instrument.Ambient`, and exactly one `registerInstrumentations` bound to the lane's own providers. Every admitted interaction event is a span, so the interaction row is the cardinality gate this condition holds and a high-frequency event enters only beside a refusing predicate.

Composed owners: `otel/emit#POLICY` supplies the `browser` policy group every row reads and the `egress` roster `_self` anchors, `otel/emit#HOOKS` supplies the contributed instrumentation rows and the raw meter provider this node drains into its one activation, and `otel/emit#LANES` exposes the tracer and logger providers the requirement channel names. `vital#CONTEXT` reads the same resource-timing buffer the fetch row must leave intact. Its module is `runtime/src/otel/instrument.ts`, resolved by the `browser` exports condition alone.

## [01]-[INDEX]

- [02]-[REGISTRATION] — the zone manager install, the document row roster, pattern self-exclusion, and the one activation; `Instrument`.

## [02]-[REGISTRATION]

- Owner: `Instrument` carries two names because the halves carry different requirements — `Instrument.ambient` publishes the installed `ContextManager` and requires nothing, so a native-lane root composes it alone, while `Instrument.live(policy)` names `OtelTracerProvider` in its requirement channel and only an SDK lane satisfies it. Reading `_Ambient` from `R` orders the pair at the type level, so registration provably follows the manager install rather than trusting root composition order.
- Cases: `_rows` seats four document instruments — the document-load row, the `fetch` row, the interaction row, and the `XMLHttpRequest` row — each keyed to its own `policy.browser.rows` cell, with `_request` handing the two request surfaces one identical exclusion and propagation pair.
- Law: the condition split is structural rather than disciplinary — `@opentelemetry/context-zone` patches the global `Zone`, so a condition-bound side effect physically cannot load on the server plane and the exports map is what keeps it out.
- Law: `enabled` is the zeroth column on every row, so a kiosk build drops interaction spans through that column alone and keeps document load and both request surfaces armed.
- Law: exclusion is a pattern roster rather than a base URL — `ignoreUrls` and `propagateTraceHeaderCorsUrls` both run the SDK's `urlMatches`, which compares a STRING entry to the whole request URL by equality, so a bare origin never equals the `/v1/<signal>` URL an exporter posts to and never equals an API path. `_self` anchors one `RegExp` per self-egress origin on its escaped form, folding `policy.collector.baseUrl` with every `policy.egress` row, and `policy.browser.propagate` carries patterns for the same reason.
- Law: interaction admission is the cardinality gate — `eventNames` and `shouldPreventSpanCreation` read the policy's interaction rows because every admitted event is a span; click-only with an admit-all predicate is the stated default, and a high-frequency row (scroll, pointermove) enters only beside a refusing predicate.
- Law: `XMLHttpRequest` traffic and `fetch` traffic split one surface under one policy spelling, so `_request` hands both rows the same exclusion and propagation pair and a CORS origin is granted once.
- Exemption: the acquire body is the platform-forced registration seam — the global manager install and the composed unload closure are the SDK's own imperative contract.
- Entry: `Instrument.ambient` merges at every root, native and SDK alike, because `emit#LANES`'s `_tracerContext` and every foreign `context.active()` read depend on the install; `Instrument.live(policy)` merges beside it under an SDK lane's `Export.live(policy)`, where a native-lane root dies at the requirement channel.
- Packages: `@opentelemetry/instrumentation` (`registerInstrumentations`, `Instrumentation`, `InstrumentationConfig.enabled`), `@opentelemetry/context-zone` (`ZoneContextManager`), `@opentelemetry/instrumentation-fetch`, `-document-load`, `-user-interaction`, `-xml-http-request`, `@opentelemetry/api` (`context`, `ContextManager`), `@effect/opentelemetry` (`Tracer.OtelTracerProvider`, `Logger.OtelLoggerProvider`), `effect` (`Array`, `Context`).
- Growth: a new document instrumentation is one `_rows` entry with its `policy.browser.rows` cell and its policy fields, or one `Hooks.contribute` row from the feature plane that needs it; a new self-egress backend is one `policy.egress` origin.
- Boundary: registration composes only at a composition root beside `Export.live` — a library composing either node double-instruments its host; the fetch row leaves `clearTimingResources` off because `vital#CONTEXT` reads the same resource-timing buffer, and the server condition's own roster and manager live at `server#REGISTRATION`.

```typescript signature
import { context as ambient, type ContextManager } from "@opentelemetry/api"
import { ZoneContextManager } from "@opentelemetry/context-zone"
import { registerInstrumentations, type Instrumentation } from "@opentelemetry/instrumentation"
import { DocumentLoadInstrumentation } from "@opentelemetry/instrumentation-document-load"
import { FetchInstrumentation } from "@opentelemetry/instrumentation-fetch"
import { UserInteractionInstrumentation } from "@opentelemetry/instrumentation-user-interaction"
import { XMLHttpRequestInstrumentation } from "@opentelemetry/instrumentation-xml-http-request"
import { Logger as OtelLogger, Tracer as OtelBridge } from "@effect/opentelemetry"
import { Array, Context, Effect, Layer } from "effect"
import { type Export, Hooks } from "./emit.ts"

// Same published-capability split as the server node under this condition's own manager: the zone manager install is
// process-global, so its teardown belongs to a Layer rather than to a bracket the registration shares.
class _Ambient extends Context.Tag("runtime/Instrument/Ambient")<_Ambient, ContextManager>() {}

const _ambient: Layer.Layer<_Ambient> = Layer.scoped(
  _Ambient,
  Effect.acquireRelease(
    Effect.sync(() => {
      const manager = new ZoneContextManager().enable()
      ambient.setGlobalContextManager(manager)
      return manager
    }),
    () => Effect.sync(() => ambient.disable()),
  ),
)

// urlMatches compares a STRING entry to the whole request URL by equality, so a base never matches the /v1/<signal>
// URL the exporter posts to — every self-egress origin anchors as its own pattern, the collector beside each
// `policy.egress` row, so a second telemetry backend joins the exclusion without a second parse
const _self = (policy: Export.Policy): ReadonlyArray<RegExp> =>
  Array.map(
    [policy.collector.baseUrl, ...policy.egress],
    (raw) => new RegExp(`^${new URL(raw).origin.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}/`),
  )

const _request = (policy: Export.Policy) => ({
  ignoreUrls: [..._self(policy)],
  propagateTraceHeaderCorsUrls: [...policy.browser.propagate],
})

const _rows = (policy: Export.Policy): ReadonlyArray<Instrumentation> => [
  new DocumentLoadInstrumentation({ enabled: policy.browser.rows.document }),
  new FetchInstrumentation({ ..._request(policy), enabled: policy.browser.rows.fetch }),
  new UserInteractionInstrumentation({
    // a kiosk build drops interaction spans through this column alone, where dropping the whole node would take
    // document-load and both request surfaces with it
    enabled: policy.browser.rows.interaction,
    eventNames: [...policy.browser.interaction.events],
    shouldPreventSpanCreation: policy.browser.interaction.prevent,
  }),
  // This legacy request surface under the identical self-exclusion and propagate rows as the fetch row
  new XMLHttpRequestInstrumentation({ ..._request(policy), enabled: policy.browser.rows.xhr }),
]

const Instrument: {
  readonly Ambient: typeof _Ambient
  readonly ambient: Layer.Layer<_Ambient>
  readonly live: (
    policy: Export.Policy,
  ) => Layer.Layer<never, never, _Ambient | Hooks | Hooks.Meter | OtelBridge.OtelTracerProvider | OtelLogger.OtelLoggerProvider>
} = {
  Ambient: _Ambient,
  ambient: _ambient,
  live: (policy) =>
    Layer.scopedDiscard(
      Effect.flatMap(
        Effect.all([
          Effect.flatMap(Hooks, (hooks) => hooks.drained),
          Hooks.Meter,
          OtelBridge.OtelTracerProvider,
          OtelLogger.OtelLoggerProvider,
          _Ambient,
        ]),
        ([adds, raw, tracerProvider, loggerProvider]) =>
          Effect.acquireRelease(
            Effect.sync(() =>
              registerInstrumentations({
                instrumentations: [..._rows(policy), ...adds.instruments],
                loggerProvider,
                meterProvider: raw.provider,
                tracerProvider,
              })),
            (unload) => Effect.sync(unload),
          ),
      ),
    ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Instrument }
```

## [03]-[RESEARCH]

(none)
