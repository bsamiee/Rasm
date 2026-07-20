# [RUNTIME_CRASH]

Crash capture is one total fold from any settled `Cause` to a structured fatal emission: the exception triple and `error.type` land in the capture's attribute band through `FaultCapture.forensic`, and forensic enrichment rides the core-declared fault-enricher contract — the `Cause` seeds a `FaultCapture` already carrying its exception evidence, `FaultEnricher.identity` is the shipped no-interchange pass-through. Capture is infallible by law: a pass-through enrichment and a full ring degrade, never fail, because the crash path is the last place a fault channel may open.

Breadcrumb history attaches as replay evidence redacted at the moment of capture, so raw PII never sits in memory waiting for a crash; the scrub authority is one ambient rule set — `Redaction.Current` — applied at BOTH crash seams: breadcrumbs scrub when recorded, the enriched forensic band scrubs at the fatal emission. Fiber-external failure sources register as a `Layer` whose hook arrives as a runtime-supplied value, keeping this module runtime-neutral while the `./browser`/`./server` subpaths each contribute a one-line hook row. Its module is `runtime/src/otel/crash.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER] | [OWNS]                                                                         | [PUBLIC] |
| :-----: | :-------- | :------------------------------------------------------------------------------ | :------- |
|  [01]   | `REPLAY`  | the breadcrumb ring: bounded window, redaction-at-capture, evidence projection | `Crash`  |
|  [02]   | `CAPTURE` | the `Cause` fold, enricher consumption, convention-named fatal emission        | `Crash`  |
|  [03]   | `NET`     | fiber-external hook rows as runtime-supplied values                            | hooks    |

## [02]-[REPLAY]

- Owner: the breadcrumb ring inside the `Crash` service — a `Ref<Chunk<Breadcrumb>>` sliding window whose width is the interior `_RING` policy row; a breadcrumb is `{ at, label, detail }` with `detail` an open attribute bag (`Convention.Bag` — crash context lawfully carries keys the vocabulary never minted).
- Law: redaction-at-capture — the ambient rule set (`Redaction.Current`, the one `Rules` shape `emit#REDACTION` owns) runs when the breadcrumb is RECORDED, so the ring never holds an unscrubbed value and a memory dump is as clean as the export; this is deliberately distinct from the export-boundary scrub, which governs what leaves the process — sibling consumption sites in the one signal-safety ledger, one rule shape.
- Law: the ring is a `Chunk` fold — append then `takeRight(width)` — never a mutable array; a full ring drops the oldest silently because breadcrumbs are lossy context by definition.
- Receipt: on capture the ring projects as ordered breadcrumb records under the `Convention.event.breadcrumb` name, an annotation row on the fatal emission — evidence, never a second signal.
- Growth: a new breadcrumb source is one `note` call site; a richer scrub is a rule row.
- Packages: `effect` (`Chunk`, `DateTime`, `Ref`), `@rasm/ts/core` (`Convention`), `./emit.ts` (`Redaction`).

```typescript signature
import { Cause, Chunk, DateTime, Effect, FiberSet, Layer, Match, Metric, Option, Ref, pipe } from "effect"
import { type AppIdentity, Convention, FaultCapture, FaultClass, FaultEnricher } from "@rasm/ts/core"
import { Redaction } from "./emit.ts"

declare namespace Crash {
  type Breadcrumb = {
    readonly at: DateTime.Utc
    readonly detail: Convention.Bag
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
  detail: Convention.Bag,
): Effect.Effect<void> =>
  Effect.flatMap(DateTime.now, (at) =>
    Ref.update(ring, (held) =>
      Chunk.takeRight(Chunk.append(held, { at, detail: Redaction.scrub(rules, detail), label }), _RING.width)))
```

## [03]-[CAPTURE]

- Owner: the `Crash` service — a Layer factory over the app's `AppIdentity` (`Crash.Default(identity)`), `scoped` construction holding the ring, the ambient scrub rules read once from `Redaction.Current`, and the core enrichment port; `capture` and `note` are the accessors, and `Crash.net` is the class-carried registration static so the whole crash surface travels one import.
- Law: the enricher is a requirement, never a branch — `FaultEnricher` rides `R` because the core floor ships `FaultEnricher.identity` as the no-interchange pass-through, so an unwired root is a compile error and forensic absence is a root selection; the port is total by signature, so enrichment can never make a crash worse.
- Law: the capture value is the core `FaultCapture` — `FaultClass.of` over the WHOLE `Cause` tree folds every failure and defect node through the severity lattice to the bounded class, the shaped exception seeds `tag`/`detail`, `identity.label` is the owning `surface`, and `FaultCapture.forensic` writes the exception evidence into the attribute band BEFORE the enricher round-trips the value — so the capture the enricher observes already carries the forensic triple and `error.type`, wire-grade forensics merge into the same band, and the emission spreads that band ONLY through the scrub; a new forensic axis is an `Evidence` field or an enricher band row (`Convention.rasm.crashHop` names the hop key), never an edit here.
- Law: the fatal forensic band is scrubbed material — exception messages, stack text, and enricher-added rows are the highest-PII-risk attributes the process emits, so the emission passes `capture.attributes` through `Redaction.scrub` with the same ambient rules the breadcrumbs consumed; no crash attribute reaches a log annotation outside the shared fold, and the signal-safety ledger (`emit#REDACTION`) names this seam explicitly.
- Law: the emission is one declaration — `Effect.logFatal` annotated with the scrubbed enriched band and the replay projection, and `Effect.annotateCurrentSpan` carrying `error.type` so the active span records the failure — the fatal log becomes an OTLP log record on the shared `Resource` through the replaced process logger, and the `Convention.metric.crashCaptured` counter tags by the same bounded class.
- Law: classification and exception shaping are two reads of one `Cause` — `FaultClass.of(cause)` folds the tree's failure and defect nodes through the severity lattice so a parallel cause classifies by dominance, never by squash ordering; `Cause.squash` serves ONLY the free-form exception shaping (`Match.instanceOf` triage into name/message/stack), and `Cause.isInterruptedOnly` short-circuits because an interrupted fiber is not a crash; flattening to a message string upstream of the projections is the rejected fold.
- Law: capture is total — its type is `Effect<void>` with no error channel; the enricher is total by contract and the ring read is infallible, so no interior step can open a fault channel on the crash path.
- Boundary: the core `FaultEnricher`, `FaultCapture`, and `FaultClass` owners carry the enrichment contract; the serving edge's support-capture verb is this owner's standing consumer, reaching `Crash.capture` through app-root composition.
- Entry: `Crash.capture(cause)`; `Crash.note(label, detail)`; `Crash.net(hook)` merged at the composition root; wiring is `Crash.Default(identity)` under the enricher Layer — the interchange codec's, or `FaultEnricher.identity` — with the root's `Redaction.Current` override governing both scrub seams.
- Growth: a new evidence axis on the emission is one enricher band row; a new classification is a core row this page inherits.
- Packages: `effect` (`Cause`, `Match`, `FiberSet`, `Metric`), `@rasm/ts/core` (`AppIdentity`, `FaultCapture`, `FaultClass`, `FaultEnricher`).

```typescript signature
const _captured = Metric.counter(Convention.metric.crashCaptured, { incremental: true })

const _shaped: (squashed: unknown) => { readonly name: string; readonly note: string; readonly stack: string } = pipe(
  Match.type<unknown>(),
  Match.when(Match.instanceOf(Error), (fault) => ({ name: fault.name, note: fault.message, stack: fault.stack ?? "" })),
  Match.orElse((residue) => ({ name: "defect", note: String(residue), stack: "" })),
)

class Crash extends Effect.Service<Crash>()("runtime/Crash", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const enricher = yield* FaultEnricher
      const ring = yield* Ref.make(Chunk.empty<Crash.Breadcrumb>())
      const rules = yield* Redaction.Current
      return {
        capture: (cause: Cause.Cause<unknown>): Effect.Effect<void> =>
          Cause.isInterruptedOnly(cause)
            ? Effect.void
            : Effect.gen(function* () {
                const shaped = _shaped(Cause.squash(cause))
                const at = yield* DateTime.now
                const capture = yield* enricher.enrich(
                  new FaultCapture({
                    at,
                    attributes: {},
                    class: FaultClass.of(cause),
                    correlation: Option.none(),
                    detail: shaped.note,
                    surface: identity.label,
                    tag: shaped.name,
                  }).forensic({
                    message: shaped.note,
                    stacktrace: shaped.stack === "" ? Option.none() : Option.some(shaped.stack),
                    type: shaped.name,
                  }),
                )
                const crumbs = yield* Ref.get(ring)
                yield* Effect.annotateCurrentSpan(Convention.attr.errorType, capture.class)
                yield* Effect.logFatal("crash").pipe(
                  Effect.annotateLogs({
                    // forensic band rides the same fold the export boundary runs: message and stack text never land raw
                    ...Redaction.scrub(rules, capture.attributes),
                    [Convention.event.breadcrumb]: JSON.stringify(Chunk.toReadonlyArray(crumbs)),
                  }),
                )
                yield* Metric.increment(Metric.tagged(_captured, Convention.attr.errorType, capture.class))
              }),
        note: (label: string, detail: Convention.Bag): Effect.Effect<void> =>
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

## [04]-[NET]

- Owner: the per-runtime `Hook` rows — plain values the `./browser` and `./server` subpath modules contribute, each wiring its platform's fiber-external failure sources and returning the composed unregister; `crash.ts` names no runtime, so the net law is closed while the hook roster grows at the edges; the fence below carries both one-row subpath modules.
- Law: `Crash.net(hook)` brackets `install`/unregister on the graph `Scope` and forks each emitted defect through a `FiberSet.makeRuntime` fork function — the sanctioned callback-seam spelling — so net-captured fibers are owned members that die with the runtime and a leaked global handler is unspellable.
- Law: a hook emits the raw thrown value; the net folds it to `Cause.die` at the seam, so foreign junk enters the capture fold typed as a defect and the triage lives in one place; process teardown after a fatal capture stays `runMain`'s — a hook never calls `process.exit`.
- Exemption: the hook `install` body is the platform-forced statement seam — listener registration and removal are the platform's own callback contract.
- Growth: a new runtime is one `Hook` literal at its boot edge — never a change here.

```typescript signature
import type { Crash } from "./crash.ts"

const browserHook: Crash.Hook = {
  // one-row ./browser subpath module in full
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

const serverHook: Crash.Hook = {
  // one-row ./server subpath module in full: capture-and-continue — exit disposition stays runMain's
  install: (emit) => {
    const onException = (fault: Error) => emit(fault)
    const onRejection = (reason: unknown) => emit(reason)
    process.on("uncaughtException", onException)
    process.on("unhandledRejection", onRejection)
    return () => {
      process.off("uncaughtException", onException)
      process.off("unhandledRejection", onRejection)
    }
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { browserHook, serverHook }
```

## [05]-[RESEARCH]

(none)
