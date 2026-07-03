# [TELEMETRY_ALERT]

Alerting is a derived vocabulary, never authored rules: one `AlertSpec` family derives totally from an `Objective` — one spec per `_BURN` row, keyed by a deterministic slug, carrying the severity row's routing posture, the window pair, the trip factor, and the annotation record under `Convention.rasm` slo keys — and the two consumers compile the same data without re-deciding anything: `board` renders specs as threshold panels and firing annotations, `iac/observe` compiles them into provider alert rules through the grafana apply seam. Delivery is out of scope by law — a spec says WHAT fires and HOW urgent, and the notification transport is the deploy plane's routing concern; a hand-authored alert rule beside this derivation is the drift defect the total function exists to kill.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                 |
| :-----: | :----------- | :----------------------------------------------------------------------- |
|  [01]   | [SEVERITY]   | the severity routing vocabulary: urgency posture per severity row         |
|  [02]   | [DERIVATION] | the `AlertSpec` shape and the `Objective -> specs` total derivation       |

## [2]-[SEVERITY]

[SEVERITY]:
- Owner: the `_severity` table — one row per severity carrying its routing posture: `urgency` (page interrupts a human now, ticket enters the queue), `hold` (how long the condition must hold before the spec is considered firing — page rows fire immediately because the short window already debounces, ticket rows hold to suppress flappy toil), and `tone` (the annotation tone dashboards render).
- Law: the severity axis is exactly `slo/burnrate`'s row projection — the union derives from the burn rows' `severity` column, so a severity this table carries but no burn row produces is dead vocabulary the guard rejects, and the two pages cannot drift.
- Growth: a routing posture axis (a business-hours gate, an escalation tier) is one column every spec inherits.

```typescript
import type { Duration } from "effect"

const _severity = {
  page: { hold: "0 seconds", tone: "critical", urgency: "interrupt" },
  ticket: { hold: "30 minutes", tone: "warning", urgency: "queue" },
} as const

declare namespace Alert {
  type Severity = keyof typeof _severity
  type SeverityRow = { readonly hold: Duration.DurationInput; readonly tone: string; readonly urgency: "interrupt" | "queue" }
  type _Rows<T extends Record<Severity, SeverityRow> = typeof _severity> = T
}
```

## [3]-[DERIVATION]

[DERIVATION]:
- Owner: the assembled `Alert` export — the severity table spread in and `Alert.of` as the one derivation: one spec per burn row, total by construction because the burn table is closed, so every objective yields exactly the four-row discipline.
- Law: the spec is compilation-ready data — `slug` (the deterministic `${objective.name}:${burn}` key both consumers use as the provider-side identity, so a re-apply updates in place), the `sli` carried whole (the consumer compiles it to its own query dialect), `target`, the row's `windows`/`factor`, the severity row inline, and the annotation record under `Convention.rasm.sloObjective`/`sloSeverity`/`sloBurn` keys — everything a rule compiler or a panel builder needs, nothing it must look up elsewhere.
- Law: consumers compile, never re-derive — `board/library`'s slo pack folds specs into threshold panels and firing annotations, `iac/observe` folds the same specs into `@pulumiverse/grafana` rule resources; a consumer computing its own burn thresholds from the objective has forked the discipline and is the named defect.
- Law: delivery routing is not spec data — receivers, schedules, and escalation chains are deploy-plane configuration keyed by the spec's severity row; the spec's `urgency` is the routing INPUT, the route itself lives where the notifier lives.
- Receipt: `AlertSpec` — plain policy data; no effect, no fault channel, no emission.
- Entry: `Alert.of(objective)`; `Alert.severity` for posture lookups.
- Growth: a new spec field is one construction line inherited by both consumers; a new severity is first a burn-row change, then its `_severity` row.

```typescript
import { Array, Struct } from "effect"
import { Convention, Slo, type Sli } from "@rasm/ts/telemetry"

declare namespace Alert {
  type Spec = {
    readonly annotations: Convention.Attributes
    readonly burn: Slo.Burn
    readonly factor: number
    readonly severity: SeverityRow & { readonly kind: Severity }
    readonly sli: Sli
    readonly slug: string
    readonly target: number
    readonly windows: { readonly long: Duration.DurationInput; readonly short: Duration.DurationInput }
  }
}

const _of = (objective: Slo.Objective): ReadonlyArray<Alert.Spec> =>
  Array.map(Struct.keys(Slo.rows), (burn): Alert.Spec => {
    const row = Slo.rows[burn]
    return {
      annotations: {
        [Convention.rasm.sloBurn]: burn,
        [Convention.rasm.sloObjective]: objective.name,
        [Convention.rasm.sloSeverity]: row.severity,
      },
      burn,
      factor: row.factor,
      severity: { ..._severity[row.severity], kind: row.severity },
      sli: objective.sli,
      slug: `${objective.name}:${burn}`,
      target: objective.target,
      windows: { long: row.long, short: row.short },
    }
  })

const Alert: {
  readonly of: (objective: Slo.Objective) => ReadonlyArray<Alert.Spec>
  readonly severity: typeof _severity
} = {
  of: _of,
  severity: _severity,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Alert }
```
