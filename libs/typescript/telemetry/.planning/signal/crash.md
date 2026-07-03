# [TELEMETRY_CRASH]

Crash capture is one total fold from any settled `Cause` to a structured fatal emission: the exception triple (`exception.type`/`message`/`stacktrace`) plus `error.type` stamp through `Convention` rows, forensic enrichment rides the kernel-declared fault-enricher contract — the folded `Cause` seeds a `FaultCapture`, `wire`'s Layer round-trips it with wire-grade forensics in the attribute band, `FaultEnricher.identity` is the shipped no-`wire` pass-through, and `telemetry` never imports `wire` — and the recent-history replay ring attaches as breadcrumb evidence redacted at the moment of capture, so raw PII never sits in memory waiting for a crash. Capture itself is infallible by law: a pass-through enrichment and a full ring degrade, never fail, because the crash path is the last place a fault channel may open. The fiber-external net — `window.onerror`, `unhandledrejection`, process-level hooks — registers as a `Layer` whose hook arrives as a runtime-supplied value, keeping this module runtime-neutral while the `./browser`/`./server` subpaths each contribute a one-line hook row.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                                       |
| :-----: | :-------- | :---------------------------------------------------------------------------- |
|  [01]   | [REPLAY]  | the breadcrumb ring: bounded window, redaction-at-capture, evidence projection |
|  [02]   | [CAPTURE] | the `Cause` fold, enricher consumption, convention-named fatal emission, net   |
|  [03]   | [NET]     | fiber-external hook rows as runtime-supplied values                            |

## [2]-[REPLAY]

[REPLAY]:
- Owner: the breadcrumb ring inside the `Crash` service — a `Ref<Chunk<Breadcrumb>>` sliding window whose width is the interior `_RING` policy row; a breadcrumb is `{ at, label, detail }` with `detail` a flat attribute record.
- Law: redaction-at-capture — the capture-time scrub (deny keys plus value patterns, the same rule shape `otlp/export`'s `Redaction` owns, applied here with capture defaults) runs when the breadcrumb is RECORDED, so the ring never holds an unscrubbed value and a memory dump is as clean as the export; this is deliberately distinct from the export-boundary scrub, which governs what leaves the process.
- Law: the ring is a `Chunk` fold — append then `takeRight(width)` — never a mutable array; a full ring drops the oldest silently because breadcrumbs are lossy context by definition.
- Receipt: on capture the ring projects as ordered breadcrumb records under the `Convention.event.breadcrumb` name, an annotation row on the fatal emission — evidence, never a second signal.
- Growth: a new breadcrumb source is one `note` call site; a richer scrub is a rule row.

```typescript
import { Chunk, DateTime, Effect, Ref } from "effect"
import { Convention } from "./convention.ts"
import { Redaction } from "../otlp/export.ts"

declare namespace Crash {
  type Breadcrumb = {
    readonly at: DateTime.Utc
    readonly detail: Convention.Attributes
    readonly label: string
  }
  type Hook = {
    readonly install: (emit: (defect: unknown) => void) => () => void
  }
}

const _RING = { width: 64 } as const

const _crumb = (
  ring: Ref.Ref<Chunk.Chunk<Crash.Breadcrumb>>,
  rules: Redaction.Rules,
  label: string,
  detail: Convention.Attributes,
): Effect.Effect<void> =>
  Effect.flatMap(DateTime.now, (at) =>
    Ref.update(ring, (held) =>
      Chunk.takeRight(Chunk.append(held, { at, detail: Redaction.scrub(rules, detail), label }), _RING.width)))
```

## [3]-[CAPTURE]

[CAPTURE]:
- Owner: the `Crash` service — a Layer factory over the app's `AppIdentity` (`Crash.Default(identity)`), `scoped` construction holding the ring, the capture-defaults scrub rules, and the kernel enrichment port; `capture` and `note` are the accessors, and `Crash.net` is the class-carried registration static so the whole crash surface travels one import.
- Law: the enricher is a requirement, never a branch — `FaultEnricher` rides `R` because the kernel ships `FaultEnricher.identity` as the no-`wire` pass-through, so an unwired root is a compile error and forensic absence is a root selection; the port is total by signature (`enrich: (capture) => Effect<FaultCapture>`), so enrichment can never make a crash worse.
- Law: the capture value is the kernel `FaultCapture` — `FaultClass.of` over the squashed `Cause` seeds the bounded class, the shaped exception seeds `tag`/`detail`, `identity.label` is the owning `surface`, and the enricher round-trips the value with wire-grade forensics merged into the string attribute band; the emission spreads that band verbatim, so a new forensic axis is an enricher band row (`Convention.rasm.crashHop` names the hop key), never an edit here.
- Law: the emission is one declaration — `Effect.logFatal` annotated with the exception triple, the enriched band, `error.type` carrying the bounded `FaultClass` kind, and the replay projection, plus `Effect.annotateCurrentSpan` carrying `error.type` so the active span records the failure — the fatal log becomes an OTLP log record on the shared `Resource` through the replaced process logger, and the `Convention.metric.crashCaptured` counter tags by the same bounded class.
- Law: the `Cause` folds through the forensic projections — `Cause.squash` yields the dominant value the `Match.instanceOf` triage shapes, and `Cause.isInterruptedOnly` short-circuits because an interrupted fiber is not a crash; flattening to a message string upstream of the projections is the rejected fold.
- Law: capture is total — its type is `Effect<void>` with no error channel; the enricher is total by contract and the ring read is infallible, so no interior step can open a fault channel on the crash path.
- Law: `Crash.net(hook)` brackets `install`/unregister on the graph `Scope` and forks each emitted defect through a `FiberSet.makeRuntime` fork function — the sanctioned callback-seam spelling — so net-captured fibers are owned members that die with the runtime and a leaked global handler is unspellable.
- Boundary: the enricher contract and the `FaultCapture`/`FaultClass` shapes are `kernel/fault/classify`'s declarations; the `wire/gateway` support-capture verb is this owner's standing consumer, reaching `Crash.capture` through app-root composition because the ledger admits no `wire -> telemetry` edge.
- Entry: `Crash.capture(cause)`; `Crash.note(label, detail)`; `Crash.net(hook)` merged at the composition root; wiring is `Crash.Default(identity)` under the enricher Layer — `wire`'s, or `FaultEnricher.identity`.
- Growth: a new evidence axis on the emission is one enricher band row; a new classification is a kernel row this page inherits.

```typescript
import { Cause, Chunk, DateTime, Effect, FiberSet, Layer, Match, Metric, Option, Ref, pipe } from "effect"
import { type AppIdentity, FaultCapture, FaultClass, FaultEnricher } from "@rasm/ts/kernel"

const _captured = Metric.counter(Convention.metric.crashCaptured, { incremental: true })

const _shaped: (squashed: unknown) => { readonly name: string; readonly note: string; readonly stack: string } = pipe(
  Match.type<unknown>(),
  Match.when(Match.instanceOf(Error), (fault) => ({ name: fault.name, note: fault.message, stack: fault.stack ?? "" })),
  Match.orElse((residue) => ({ name: "defect", note: String(residue), stack: "" })),
)

const _exception = (shaped: { readonly name: string; readonly note: string; readonly stack: string }): Convention.Attributes => ({
  [Convention.attr.exceptionEscaped]: true,
  [Convention.attr.exceptionMessage]: shaped.note,
  [Convention.attr.exceptionStacktrace]: shaped.stack,
  [Convention.attr.exceptionType]: shaped.name,
})

class Crash extends Effect.Service<Crash>()("telemetry/Crash", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const enricher = yield* FaultEnricher
      const ring = yield* Ref.make(Chunk.empty<Crash.Breadcrumb>())
      const rules = Redaction.defaults
      return {
        capture: (cause: Cause.Cause<unknown>): Effect.Effect<void> =>
          Cause.isInterruptedOnly(cause)
            ? Effect.void
            : Effect.gen(function* () {
                const squashed = Cause.squash(cause)
                const shaped = _shaped(squashed)
                const at = yield* DateTime.now
                const capture = yield* enricher.enrich(
                  new FaultCapture({
                    at,
                    attributes: {},
                    class: FaultClass.of(squashed),
                    correlation: Option.none(),
                    detail: shaped.note,
                    surface: identity.label,
                    tag: shaped.name,
                  }),
                )
                const crumbs = yield* Ref.get(ring)
                yield* Effect.annotateCurrentSpan(Convention.attr.errorType, capture.class)
                yield* Effect.logFatal("crash").pipe(
                  Effect.annotateLogs({
                    ...capture.attributes,
                    ..._exception(shaped),
                    [Convention.attr.errorType]: capture.class,
                    [Convention.event.breadcrumb]: JSON.stringify(Chunk.toReadonlyArray(crumbs)),
                  }),
                )
                yield* Metric.increment(Metric.tagged(_captured, Convention.attr.errorType, capture.class))
              }),
        note: (label: string, detail: Convention.Attributes): Effect.Effect<void> =>
          _crumb(ring, rules, label, detail),
      }
    }),
  accessors: true,
}) {
  static readonly net = (hook: Crash.Hook): Layer.Layer<never, never, Crash> =>
    Layer.scopedDiscard(
      Effect.gen(function* () {
        const crash = yield* Crash
        const fork = yield* FiberSet.makeRuntime<never>()
        yield* Effect.acquireRelease(
          Effect.sync(() => hook.install((defect) => void fork(crash.capture(Cause.die(defect))))),
          (unregister) => Effect.sync(unregister),
        )
      }),
    )
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Crash }
```

## [4]-[NET]

[NET]:
- Owner: the per-runtime `Hook` rows — plain values the `./browser` and `./server` subpath modules contribute, each wiring its platform's fiber-external failure sources and returning the composed unregister; `crash.ts` names no runtime, so the net law is closed while the hook roster grows at the edges. The fence below is the `./browser` subpath module in full.
- Exemption: the hook `install` body is the platform-forced statement seam — listener registration and removal are the platform's own callback contract.
- Law: a hook emits the raw thrown value; the net folds it to `Cause.die` at the seam, so foreign junk enters the capture fold typed as a defect and the triage lives in one place.
- Growth: a new runtime is one `Hook` literal at its boot edge — never a change here.

```typescript
import type { Crash } from "./signal/crash.ts"

const browserHook: Crash.Hook = {
  install: (emit) => {
    const onError = (event: ErrorEvent) => emit(event.error ?? event.message)
    const onRejection = (event: PromiseRejectionEvent) => emit(event.reason)
    globalThis.addEventListener("error", onError)
    globalThis.addEventListener("unhandledrejection", onRejection)
    return () => {
      globalThis.removeEventListener("error", onError)
      globalThis.removeEventListener("unhandledrejection", onRejection)
    }
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { browserHook }
```
