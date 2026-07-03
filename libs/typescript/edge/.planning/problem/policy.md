# [EDGE_POLICY]

`problem/policy.ts` is the outbound fault-policy floor: one governed record maps every kernel `FaultClass` to its HTTP projection — status, title, problem-type slug, retry grace — so the class vocabulary decides transport semantics in exactly one place and a new kernel class breaks this record loudly at compile time. Exposure is never a column: it derives from the kernel `blame` axis, so caller-blamed detail crosses outward and system-blamed detail redacts to its title with zero drift risk. The extension allowlist fixes which fault fields survive into a problem body, and the upstream rows project the structurally-probed wire fault — `retryable` re-drives under 503 grace, `terminal` refuses as 502 — without the `wire` import the edge ledger forbids. `problem/detail.ts` folds these rows; nothing else in the branch maps a class to a status.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                       | [PUBLIC]        |
| :-----: | :--------------- | :---------------------------------------------------------------------------- | :-------------- |
|  [01]   | [STATUS_RECORD]  | the class-to-status governed record, type slugs, grace rows                   | `ProblemPolicy` |
|  [02]   | [REDACTION_ROWS] | blame-derived exposure, the extension allowlist, the redact fold              | `ProblemPolicy` |
|  [03]   | [UPSTREAM_ROWS]  | the wire-fault projection rows over structural `retryable`/`terminal` facts   | `ProblemPolicy` |

## [2]-[STATUS_RECORD]

[STATUS_RECORD]:
- Owner: `_rows` — the governed record under a stated `Record<FaultClass.Kind, ProblemPolicy.Grade>` annotation: the mapped domain demands one row per kernel class and rejects any excess row, so kernel growth lands here as one compile-forced edit and the record is the only class-to-status site in the branch (`kernel/fault/classify` keeps its table transport-free by this boundary).
- Law: rows carry three edge-owned axes — `status` (the response code), `title` (the RFC 9457 type summary, human-stable and free of occurrence data), `grace` (the default `Retry-After` window as an `Option<Duration>`, inhabited only on the re-drivable classes) — and nothing the kernel row already states: rank, retryability, and blame stay kernel columns read through `FaultClass[kind]`.
- Law: the problem `type` member derives — `ProblemPolicy.type(kind)` is `_TYPE_BASE` plus the class literal, so the type-URI vocabulary is the kernel key space and a hand-authored slug registry cannot exist; `about:blank` never appears because every fold lands on a class.
- Law: `grace` resolution is a two-rung ladder — a runtime hint carried by the fault value (a quota verdict's own window, a limiter's `retryAfter` evidence) wins, the row's `grace` default fills, and absence stays absent — `ProblemPolicy.retryAfter(grace, hint)` folds the ladder to whole seconds once over any row's grace, class or upstream alike, so no consumer re-derives header arithmetic.
- Growth: a new kernel class is one row here (compile-forced); a new response axis is one `Grade` field plus its column on ten rows.
- Packages: `effect` (`Duration`, `Option`, `Record`, `Array`); `kernel/fault/classify` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/kernel"
import { Array, Duration, Option, Record } from "effect"

const _TYPE_BASE = "/problems/"

const _rows: Record<FaultClass.Kind, ProblemPolicy.Grade> = {
  absent: { status: 404, title: "resource absent", grace: Option.none() },
  conflicted: { status: 409, title: "state conflict", grace: Option.none() },
  invalid: { status: 422, title: "unprocessable input", grace: Option.none() },
  malformed: { status: 400, title: "malformed request", grace: Option.none() },
  denied: { status: 403, title: "access denied", grace: Option.none() },
  expired: { status: 401, title: "credential expired", grace: Option.none() },
  exhausted: { status: 429, title: "quota exhausted", grace: Option.some(Duration.seconds(30)) },
  unavailable: { status: 503, title: "temporarily unavailable", grace: Option.some(Duration.seconds(10)) },
  breached: { status: 500, title: "internal fault", grace: Option.none() },
  defect: { status: 500, title: "internal fault", grace: Option.none() },
}
```

## [3]-[REDACTION_ROWS]

[REDACTION_ROWS]:
- Law: exposure derives from blame — `ProblemPolicy.expose(kind)` is `FaultClass[kind].blame === "caller"`, never a column: a caller-blamed class carries its fault detail outward as actionable repair material, a system-blamed class redacts to the row title so no internal evidence, path, or dependency name leaks through a 5xx body.
- Law: `_EXPOSED` is the extension allowlist — `tag`, `reason`, and `requestId` are the only fault-derived members a problem body may carry — and `ProblemPolicy.redact(kind, extensions)` filters against it unconditionally, then empties further on a non-exposing class; a new public member is one tuple entry, and an extension written past the fold is the leak defect the architecture suite audits.
- Law: `requestId` survives redaction on every class — correlation is the one occurrence datum a system-blamed problem must keep, because the operator resolves the redacted body against telemetry through it — so the redact fold re-admits it after the class gate.
- Boundary: which values populate the extensions is `problem/detail.ts`'s fold; log-side and OTLP-side scrubbing is `telemetry` policy; this owner fixes only what crosses the HTTP body outward.

```typescript
const _EXPOSED = ["tag", "reason", "requestId"] as const

const _redact = (kind: FaultClass.Kind, extensions: Readonly<Record<string, string>>): Readonly<Record<string, string>> =>
  Record.filter(extensions, (_, key) =>
    key === "requestId" || (FaultClass[kind].blame === "caller" && Array.contains(_EXPOSED, key)))
```

## [4]-[UPSTREAM_ROWS]

[UPSTREAM_ROWS]:
- Owner: `_upstream` — the two-row projection for the wire-reconstructed fault: a `retryable` non-`terminal` upstream hop projects as 503 under the unavailable grace window, and everything else refuses as 502 with no grace — the `terminal` fact vetoes re-drive even where the hop claims retryability, so a wrong-program upstream reads distinctly from a saturated one and never invites a retry.
- Law: the probe is structural, never an import — `problem/detail.ts` recognizes the adopted-verbatim `FaultDetail` tag and reads its `retryable`/`terminal` projections as facts on the value; this page owns only the row those facts select, so the `edge -> wire` edge stays ledger-impossible while the wire altitude still exits through the one problem door (invariant 6).
- Law: upstream rows are never class rows — an upstream refusal is not the caller's fault and not this process's invariant breach, so each row derives its own `type` over the one `_TYPE_BASE` anchor (`upstream-unavailable`, `upstream-refused`) and always redacts detail: hop chains, sites, and elapsed spans are telemetry material, never response bodies.
- Growth: a new upstream disposition is one row keyed by a new structural fact; the probe extends in `problem/detail.ts`, the row lands here.

```typescript
const _upstream = {
  retryable: { type: `${_TYPE_BASE}upstream-unavailable`, status: 503, title: "upstream temporarily unavailable", grace: _rows.unavailable.grace },
  refused: { type: `${_TYPE_BASE}upstream-refused`, status: 502, title: "upstream refused", grace: Option.none<Duration.Duration>() },
} as const

declare namespace ProblemPolicy {
  type Grade = { readonly status: number; readonly title: string; readonly grace: Option.Option<Duration.Duration> }
  type Upstream = { readonly retryable: boolean; readonly terminal: boolean }
}

const ProblemPolicy: {
  readonly expose: (kind: FaultClass.Kind) => boolean
  readonly grade: (kind: FaultClass.Kind) => ProblemPolicy.Grade
  readonly redact: (kind: FaultClass.Kind, extensions: Readonly<Record<string, string>>) => Readonly<Record<string, string>>
  readonly retryAfter: (grace: Option.Option<Duration.Duration>, hint: Option.Option<Duration.Duration>) => Option.Option<number>
  readonly type: (kind: FaultClass.Kind) => string
  readonly upstream: (facts: ProblemPolicy.Upstream) => { readonly type: string } & ProblemPolicy.Grade
} = {
  expose: (kind) => FaultClass[kind].blame === "caller",
  grade: (kind) => _rows[kind],
  redact: _redact,
  retryAfter: (grace, hint) =>
    Option.map(Option.orElse(hint, () => grace), (held) => Math.ceil(Duration.toMillis(held) / 1000)),
  type: (kind) => `${_TYPE_BASE}${kind}`,
  upstream: (facts) => (facts.retryable && !facts.terminal ? _upstream.retryable : _upstream.refused),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ProblemPolicy }
```
