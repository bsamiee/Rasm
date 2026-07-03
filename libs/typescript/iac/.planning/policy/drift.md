# [IAC_DRIFT]

The drift fold: `Stack.previewRefresh` is the engine's read-only reconciliation leg — it re-reads live provider state against the desired graph and streams the divergence as `resourcePreEvent` steps — and `Drift` wraps it once with the same signal-bound `Effect.async` discipline as the ledger, folds the events through `Automation.receipt`, and buckets the outcome over the `OpType` vocabulary `program/automation` anchors. The product is a `DriftReport`: the non-`same` step rows (a hand-edited Grafana board, an out-of-band role grant, a mutated ingress all surface as `update` steps), the rotation watch (certificate resources inside their renewal window, matched on the `tls:` type-token prefix the `readyForRenewal` law feeds), and the skew record when the event-folded buckets disagree with the engine's own `changeSummary` — the fold auditing its source. Cadence is a caller-composed `Schedule` value, calendar recurrence included, so sweeping a fleet is one repeat over one check. The module is `iac/src/policy/drift.ts`; a new drift dimension is one report field folded from the rows already carried.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                            | [PUBLIC] |
| :-----: | :-------------- | :------------------------------------------------------------------- | :------- |
|  [01]   | `DRIFT_REPORT`  | the report owner: drifted rows, rotation watch, skew evidence       | `Drift`  |
|  [02]   | `DRIFT_FOLD`    | the previewRefresh wrap, the reconcile, and the fleet sweep         | `Drift`  |

## [2]-[DRIFT_REPORT]

[DRIFT_REPORT]:
- Owner: `DriftReport`, one `Schema.Class` reusing the automation owner's field schemas — `summary` and `drifted` are `RunReceipt.fields.summary` and `RunReceipt.fields.steps` composed directly, so the drift vocabulary cannot fork from the receipt vocabulary — plus `rotations` (the urns of certificate resources whose reissue window is open) and the `Option`-carried `skew` pair.
- Law: `clean` is a projection — no drifted row and no open rotation; a report is evidence, and acting on it (re-running `up`, bumping an epoch) is the caller's decision over data.
- Law: rotation is type-token matched — a `tls:`-prefixed step whose op is not `same` is a certificate moving through its renewal window, the deploy-plane read of `kube/traffic`'s `earlyRenewalHours`/`readyForRenewal` law; the match is a prefix on the step's own `type` field, no second watch channel exists.
- Law: skew is fold-audit evidence — the engine's `changeSummary` and the event-folded buckets must agree; a disagreement ships as the `skew` pair rather than a silent preference, because a fold that quietly trusts one source cannot detect its own decode drift.
- Growth: a new watch family is one prefix row in the rotation filter or one projection field.
- Boundary: step and summary shapes are `program/automation.md`'s; what a drifted board means is `observe/apply.md`'s content-hash law; remediation is never automatic here.
- Packages: `effect` (`Schema`, `Option`); `../program/automation.ts` (`RunReceipt`).

```typescript
import type { EngineEvent, Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, pipe, Record, Schema, type Schedule } from "effect"
import { Automation, DeployFault, RunReceipt } from "../program/automation.ts"
import type { StackSpec } from "../program/spec.ts"

class DriftReport extends Schema.Class<DriftReport>("DriftReport")({
  stack: Schema.NonEmptyString,
  summary: RunReceipt.fields.summary,
  drifted: RunReceipt.fields.steps,
  rotations: Schema.Array(Schema.String),
  skew: Schema.optionalWith(
    Schema.Struct({
      expected: RunReceipt.fields.summary,
      observed: RunReceipt.fields.summary,
    }),
    { as: "Option" },
  ),
}) {
  get clean(): boolean {
    return this.drifted.length === 0 && this.rotations.length === 0
  }
}
```

## [3]-[DRIFT_FOLD]

[DRIFT_FOLD]:
- Owner: `Drift` — `check(stack, name)` runs the read-only leg: one `Effect.async` binds the interrupt signal and accumulates the event stream while `previewRefresh` settles with its `PreviewResult`, `Automation.receipt("preview", ...)` decodes the events, and `_report` projects drifted rows, rotations, observed buckets, and the skew comparison against `changeSummary`; `sweep(stacks, cadence, sink)` repeats the fleet check under the caller's `Schedule` at the fiber's inherited concurrency budget — the boundary that launches the sweep owns the degree through `Effect.withConcurrency`, never a literal here — delivering each cycle's reports to the sink.
- Law: the leg never mutates — `previewRefresh` is the engine's non-mutating reconcile; the mutating `refresh` stays a ledger op a human or workflow chooses after reading a report.
- Law: observed buckets fold from steps — group by op, count, compare per `OpType` against the engine summary with absent buckets read as zero; the comparison is total over the anchored vocabulary, so a new engine op is a compile-time event here, never a silent bucket.
- Law: the callback seam mirrors the ledger wrap — statements live only inside the `Effect.async` registration; the fold, the projections, and the sweep stay expression-shaped on the rail.
- Entry: `Drift.check(stack, spec.name)` ad hoc; `Drift.sweep(fleet, Schedule.cron("0 4 * * *"), sink)` as the standing watch.
- Growth: a per-arm drift posture (ignore rows an operator owns) is one filter parameter over the drifted rows, defaulted permissive.
- Boundary: the cadence value and its composition are the rails law consumed as a parameter; receipts persist wherever the caller's sink writes them.
- Packages: `effect` (`Effect`, `Array`, `Option`, `pipe`, `Schedule`); `@pulumi/pulumi/automation` (`Stack`, `EngineEvent`); `../program/automation.ts` (`Automation`, `DeployFault`, `RunReceipt`).

```typescript
const _observed = (steps: RunReceipt["steps"]): Record.ReadonlyRecord<string, number> =>
  pipe(
    Array.groupBy(steps, (step) => step.op),
    Record.map((rows) => rows.length),
  )

const _skewed = (
  expected: RunReceipt["summary"],
  observed: Record.ReadonlyRecord<string, number>,
): Option.Option<{ readonly expected: RunReceipt["summary"]; readonly observed: Record.ReadonlyRecord<string, number> }> =>
  Array.every(RunReceipt.opTypes, (op) => (expected[op] ?? 0) === (observed[op] ?? 0))
    ? Option.none()
    : Option.some({ expected, observed })

const _report = (receipt: RunReceipt): unknown => {
  const drifted = Array.filter(receipt.steps, (step) => step.op !== "same")
  const observed = _observed(receipt.steps)
  return {
    stack: receipt.stack,
    summary: receipt.summary,
    drifted,
    rotations: Array.map(
      Array.filter(drifted, (step) => step.type.startsWith("tls:")),
      (step) => step.urn,
    ),
    ...Option.match(_skewed(receipt.summary, observed), {
      onNone: () => ({}),
      onSome: (skew) => ({ skew }),
    }),
  }
}

const Drift = {
  check: (stack: Stack, name: string): Effect.Effect<DriftReport, DeployFault> =>
    Effect.async<ReadonlyArray<EngineEvent>, DeployFault>((resume, signal) => {
      const events: Array<EngineEvent> = []
      const onEvent = (event: EngineEvent): void => void events.push(event)
      stack.previewRefresh({ signal, onEvent }).then(
        () => resume(Effect.succeed(events)),
        (caught) => resume(Effect.fail(DeployFault.triaged(name)(caught))),
      )
    }).pipe(
      Effect.flatMap((events) => Automation.receipt("preview", name, events)),
      Effect.flatMap((receipt) =>
        Effect.mapError(
          Schema.decodeUnknown(DriftReport)(_report(receipt)),
          (parse) => new DeployFault({ reason: "alien", stack: name, detail: parse.message }),
        )),
    ),
  sweep: <R>(
    fleet: ReadonlyArray<readonly [StackSpec, Stack]>,
    cadence: Schedule.Schedule<unknown>,
    sink: (reports: ReadonlyArray<DriftReport>) => Effect.Effect<void, never, R>,
  ): Effect.Effect<void, DeployFault, R> =>
    Effect.repeat(
      Effect.flatMap(
        Effect.forEach(fleet, ([spec, stack]) => Drift.check(stack, spec.name), { concurrency: "inherit" }),
        sink,
      ),
      { schedule: cadence },
    ).pipe(Effect.asVoid),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Drift, DriftReport }
```
