# [RUNTIME_PROBLEM]

The outbound-fault law of the front door: every fault leaving the branch over HTTP renders ITSELF through the `HttpServerRespondable` symbol protocol, and `Problem` — one RFC 9457 owner carrying the body schema, the governed class-to-status record, the total fold from any refused value, and the `Cause` fold — is the value that implements it, so the central error-mapper middleware has no existence: the route seam's net is one `toResponseOrElse` fold where a respondable failure short-circuits into its own response and everything else lifts through the total probe ladder first. The ladder is ordered by evidence specificity and every rung is structural: an existing `Problem` passes untouched, the adopted-verbatim `FaultDetail` tag projects through the upstream rows with its `retryable`/`terminal` facts read off the value, a `ParseError` lands on `malformed` and a `RouteNotFound` on `absent`, and the residue classifies through `FaultClass.of` into the governed record — total, closed, and free of any cross-branch import. Exposure derives from the core `blame` axis so caller-blamed detail crosses outward and system-blamed detail redacts to its title; inbound never touches this module — a problem body is never decoded, reconstructed, or matched on. The module ships on the `./server` exports subpath as `runtime/src/serve/problem.ts`; a new core fault class breaks the record loudly at compile time.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                        | [PUBLIC]  |
| :-----: | :------------------ | :----------------------------------------------------------------------------- | :-------- |
|  [01]   | `STATUS_RECORD`     | the class-to-status governed record, type-slug derivation, grace resolution     | interior  |
|  [02]   | `REDACTION_ROWS`    | blame-derived exposure, the extension allowlist, the redact fold                | interior  |
|  [03]   | `UPSTREAM_ROWS`     | the wire-fault projection over structural `retryable`/`terminal` facts          | interior  |
|  [04]   | `RESPONDABLE_OWNER` | the RFC 9457 owner, the symbol implementation, the total fold, the seam net     | `Problem` |

## [2]-[STATUS_RECORD]

[STATUS_RECORD]:
- Owner: `_rows` — the governed record under a stated `Record<FaultClass.Kind, _Grade>` annotation: the mapped domain demands one row per core class and rejects any excess row, so core growth lands here as one compile-forced edit and this record is the only class-to-status site in the branch — `fault#CLASS_VOCABULARY` stays transport-free by this boundary.
- Law: rows carry three serve-owned axes — `status` (the response code), `title` (the RFC 9457 type summary, human-stable and free of occurrence data), `grace` (the default `Retry-After` window as `Option<Duration>`, inhabited only on the re-drivable classes) — and nothing the core row already states: rank, retryability, and blame stay core columns read through `FaultClass[kind]`.
- Law: the problem `type` member derives — `_type(kind)` is `_TYPE_BASE` plus the class literal, so the type-URI vocabulary is the core key space and a hand-authored slug registry cannot exist; `about:blank` never appears because every fold lands on a class.
- Law: grace resolution is a two-rung ladder — a runtime hint carried by the fault value (a quota verdict's measured window, a limiter's `retryAfter` evidence) wins, the row's `grace` default fills, absence stays absent — `_retryAfter(grace, hint)` folds the ladder to whole seconds once, so no consumer re-derives header arithmetic.
- Growth: a new core class is one row (compile-forced); a new response axis is one `_Grade` field plus its column on ten rows.
- Packages: `effect` (`Duration`, `Option`, `Record`, `Array`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { HttpServerRespondable, HttpServerResponse } from "@effect/platform"
import { Array, Cause, Duration, Effect, Option, Predicate, Record, Schema } from "effect"
import { FaultClass } from "@rasm/ts/core"
import { Current } from "./api.ts"

const _TYPE_BASE = "/problems/"

type _Grade = { readonly status: number; readonly title: string; readonly grace: Option.Option<Duration.Duration> }

const _rows: { readonly [K in FaultClass.Kind]: _Grade } = {
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

const _type = (kind: FaultClass.Kind): string => `${_TYPE_BASE}${kind}`

const _retryAfter = (
  grace: Option.Option<Duration.Duration>,
  hint: Option.Option<Duration.Duration>,
): Option.Option<number> =>
  Option.map(Option.orElse(hint, () => grace), (held) => Math.ceil(Duration.toMillis(held) / 1000))
```

## [3]-[REDACTION_ROWS]

[REDACTION_ROWS]:
- Law: exposure derives from blame — `_expose(kind)` is `FaultClass[kind].blame === "caller"`, never a column: a caller-blamed class carries its fault detail outward as actionable repair material, a system-blamed class redacts to the row title so no internal evidence, path, or dependency name leaks through a 5xx body.
- Law: `_EXPOSED` is the extension allowlist — `tag`, `reason`, and `requestId` are the only fault-derived members a problem body may carry — and `_redact(kind, extensions)` filters against it unconditionally, then empties further on a non-exposing class; a new public member is one tuple entry, and an extension written past the fold is the leak defect the architecture suite audits.
- Law: `requestId` survives redaction on every class — correlation is the one occurrence datum a system-blamed problem must keep, because the operator resolves the redacted body against telemetry through it — so the redact fold re-admits it after the class gate.
- Boundary: which values populate the extensions is `[05]`'s fold; log-side and OTLP-side scrubbing is `emit#REDACTION`'s policy; this cluster fixes only what crosses the HTTP body outward.

```typescript
const _EXPOSED = ["tag", "reason", "requestId"] as const

const _expose = (kind: FaultClass.Kind): boolean => FaultClass[kind].blame === "caller"

const _redact = (
  kind: FaultClass.Kind,
  extensions: { readonly [key: string]: string },
): { readonly [key: string]: string } =>
  Record.filter(extensions, (_, key) => key === "requestId" || (_expose(kind) && Array.contains(_EXPOSED, key)))
```

## [4]-[UPSTREAM_ROWS]

[UPSTREAM_ROWS]:
- Owner: `_upstream` — the two-row projection for the wire-reconstructed fault: a `retryable` non-`terminal` upstream hop projects as 503 under the unavailable grace window, everything else refuses as 502 with no grace — the `terminal` fact vetoes re-drive even where the hop claims retryability, so a wrong-program upstream reads distinctly from a saturated one and never invites a retry.
- Law: the probe is structural, never an import — `[05]`'s ladder recognizes the adopted-verbatim `FaultDetail` tag and reads its `retryable`/`terminal` projections as facts on the value, so the serve-to-interchange edge stays import-free while the wire altitude still exits through the one problem door.
- Law: upstream rows are never class rows — an upstream refusal is not the caller's fault and not this process's invariant breach, so each row derives its own `type` over the one `_TYPE_BASE` anchor and always redacts detail: hop chains, sites, and elapsed spans are telemetry material, never response bodies.
- Growth: a new upstream disposition is one row keyed by a new structural fact; the probe extends in `[05]`, the row lands here.

```typescript
const _upstream = {
  retryable: {
    type: `${_TYPE_BASE}upstream-unavailable`,
    status: 503,
    title: "upstream temporarily unavailable",
    grace: _rows.unavailable.grace,
  },
  refused: {
    type: `${_TYPE_BASE}upstream-refused`,
    status: 502,
    title: "upstream refused",
    grace: Option.none<Duration.Duration>(),
  },
} as const

const _hop = (facts: { readonly retryable: boolean; readonly terminal: boolean }): { readonly type: string } & _Grade =>
  facts.retryable && !facts.terminal ? _upstream.retryable : _upstream.refused
```

## [5]-[RESPONDABLE_OWNER]

[RESPONDABLE_OWNER]:
- Owner: `Problem` — a `Schema.Class` carrying exactly the RFC members (`type`, `title`, `status`, `detail`, `instance` as `Option`) plus the allowlisted `extensions` band and the `retry` seconds the grace ladder resolved; the class is the value, the encode anchor, the fold entry, and the self-rendering respondable under one import, and its encoded twin is the wire body verbatim.
- Law: the symbol implementation IS the egress projection — `Problem` implements `[HttpServerRespondable.symbol]()` as its own `respond`: `Schema`-encoded body under `application/problem+json` at the problem's own `status`, the `retry-after` header stamped exactly when `retry` is inhabited, `instance` and the `requestId` extension stamped from the ambient `Current.Stamp` inside the render so every egress path carries correlation; encoding the branch's own `Problem` failing is a defect (`Effect.orDie`), never a channel member — the fault altitude cannot itself fault.
- Law: the probe ladder is ordered by evidence specificity — (1) an existing `Problem` passes through; (2) `FaultDetail` by tag probe projects through the upstream rows; (3) `ParseError` lands on `malformed`, `RouteNotFound` on `absent`; (4) the residue classifies through `FaultClass.of`, a fault also carrying a `policy.status` row keeps that status over the class default, detail obeys exposure, the grace hint probes the fault's own `retryAfter` field before the class default, and `tag`/`reason` extensions populate then pass the redact fold in one construction — total over `unknown`, so an over-shared member is structurally impossible downstream.
- Law: `Problem.fromCause` discriminates in interrupt-first order — `Cause.isInterruptedOnly` folds to the `unavailable` row (the seam only observes an interrupt under shed or shutdown), a typed failure re-enters the ladder, a defect lands on the `defect` row — the same order every telemetry outcome fold uses.
- Law: the net is self-rendering-first, never a mapper — `Problem.net(cause)` folds the cause once, then resolves the response through `HttpServerRespondable.toResponseOrElse(fault, orElse)`: a failure that implements the symbol renders itself (a folder fault opting in owns its own projection; `Problem` itself is the standing instance), and the orElse arm is the total ladder — so the route seam carries zero recovery arms, the served app's error channel is `never` by construction, and an unmapped fault cannot escape as a naked 500. Declared endpoint faults keep their `HttpApiEndpoint.addError` status at the spec altitude; this net is the floor under everything undeclared.
- Boundary: attachment is `route#SEAM_ROWS`'s one composition; log/OTLP emission of the folded cause is `crash#CAPTURE`'s, fed from the same seam; the class table and blame axis are `fault#CLASS_VOCABULARY`'s.
- Growth: a new probe rung is one arm in `_of` plus its row in `[02]`; a new extension is one populate line under the redact fold.
- Packages: `effect` (`Schema`, `Option`, `Effect`, `Cause`, `Predicate`); `@effect/platform` (`HttpServerRespondable`, `HttpServerResponse`); `./api.ts` (`Current`).

```typescript
const _text = (fault: unknown): string =>
  Predicate.hasProperty(fault, "message") && Predicate.isString(fault.message) ? fault.message : String(fault)

const _field = (fault: unknown, key: string): Option.Option<string> => {
  const held = Predicate.hasProperty(fault, key) ? (fault as { readonly [k: string]: unknown })[key] : undefined
  return Predicate.isString(held) ? Option.some(held) : Option.none()
}

const _statused = (fault: unknown): Option.Option<number> =>
  Predicate.hasProperty(fault, "policy") && Predicate.hasProperty(fault.policy, "status") && Predicate.isNumber(fault.policy.status)
    ? Option.some(fault.policy.status)
    : Option.none()

const _graced = (fault: unknown): Option.Option<Duration.Duration> =>
  Predicate.hasProperty(fault, "retryAfter") && Option.isOption(fault.retryAfter)
    ? Option.filter(fault.retryAfter, Duration.isDuration)
    : Option.none()

const _extensions = (kind: FaultClass.Kind, fault: unknown): { readonly [key: string]: string } =>
  _redact(kind, {
    ...Option.match(_field(fault, "_tag"), { onNone: () => ({}), onSome: (tag) => ({ tag }) }),
    ...Option.match(_field(fault, "reason"), { onNone: () => ({}), onSome: (reason) => ({ reason }) }),
  })

const _classed = (fault: unknown): Problem => {
  const kind = FaultClass.of(fault)
  const grade = _rows[kind]
  return new Problem({
    type: _type(kind),
    title: grade.title,
    status: Option.getOrElse(_statused(fault), () => grade.status),
    detail: _expose(kind) ? _text(fault) : grade.title,
    instance: Option.none(),
    retry: _retryAfter(grade.grace, _graced(fault)),
    extensions: _extensions(kind, fault),
  })
}

const _projected = (fault: unknown): Problem => {
  const grade = _hop({
    retryable: Predicate.hasProperty(fault, "retryable") && fault.retryable === true,
    terminal: Predicate.hasProperty(fault, "terminal") && fault.terminal === true,
  })
  return new Problem({
    type: grade.type,
    title: grade.title,
    status: grade.status,
    detail: grade.title,
    instance: Option.none(),
    retry: _retryAfter(grade.grace, Option.none()),
    extensions: {},
  })
}

const _of = (fault: unknown): Problem =>
  fault instanceof Problem
    ? fault
    : Predicate.isTagged(fault, "FaultDetail")
      ? _projected(fault)
      : Predicate.isTagged(fault, "ParseError")
        ? _classed({ class: "malformed", message: "request body refused" })
        : Predicate.isTagged(fault, "RouteNotFound")
          ? _classed({ class: "absent", message: "route absent" })
          : _classed(fault)

class Problem extends Schema.Class<Problem>("Problem")({
  type: Schema.NonEmptyString,
  title: Schema.NonEmptyString,
  status: Schema.Int.pipe(Schema.between(100, 599)),
  detail: Schema.String,
  instance: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  retry: Schema.optionalWith(Schema.Int, { as: "Option" }),
  extensions: Schema.Record({ key: Schema.String, value: Schema.String }),
}) {
  static readonly of: (fault: unknown) => Problem = _of
  static readonly fromCause = <E>(cause: Cause.Cause<E>): Problem =>
    Cause.isInterruptedOnly(cause)
      ? _classed({ class: "unavailable", message: "request interrupted" })
      : Option.match(Cause.failureOption(cause), {
          onNone: () => _classed({ class: "defect", message: "internal fault" }),
          onSome: _of,
        })
  static readonly respond = (problem: Problem): Effect.Effect<HttpServerResponse.HttpServerResponse> =>
    Effect.flatMap(Current.Stamp, (mark) => {
      const stamped = Option.match(mark, {
        onNone: () => problem,
        onSome: (held) =>
          new Problem({
            ...problem,
            instance: Option.some(`/requests/${held.id}`),
            extensions: { ...problem.extensions, requestId: held.id },
          }),
      })
      return HttpServerResponse.schemaJson(Problem)(stamped, { status: stamped.status }).pipe(
        Effect.map(HttpServerResponse.setHeaders({
          "content-type": "application/problem+json",
          ...(Option.isSome(stamped.retry) && { "retry-after": String(stamped.retry.value) }),
        })),
        Effect.orDie,
      )
    })
  static readonly net = <E>(cause: Cause.Cause<E>): Effect.Effect<HttpServerResponse.HttpServerResponse> =>
    Option.match(Cause.failureOption(cause), {
      onNone: () => Problem.respond(Problem.fromCause(cause)),
      onSome: (fault) =>
        HttpServerRespondable.isRespondable(fault)
          ? HttpServerRespondable.toResponseOrElse(fault, Problem.respond(_of(fault)))
          : Problem.respond(_of(fault)),
    });
  [HttpServerRespondable.symbol](): Effect.Effect<HttpServerResponse.HttpServerResponse> {
    return Problem.respond(this)
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Problem }
```
