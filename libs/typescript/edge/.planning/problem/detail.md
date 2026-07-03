# [EDGE_DETAIL]

`problem/detail.ts` is the outbound-only fault altitude: `Problem` is one RFC 9457 owner — the body schema, the total fold from any refused value, the response projection, and the serve-level guard under a single name — and every fault leaving the branch over HTTP leaves through it. The fold is total over `unknown` by a structural probe ladder: a fault carrying a `policy.status` row keeps its own status, the adopted-verbatim `FaultDetail` tag projects through the upstream rows, a `ParseError` and a `RouteNotFound` land on their fixed classes, and everything else classifies through `FaultClass.of` into the governed class record — so the third fault altitude (invariant 6) is one file, one fold, and zero `wire` imports. Inbound never touches this module: a problem body is never decoded, reconstructed, or matched on — `Problem` has no ingress arm by construction.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                 | [PUBLIC]  |
| :-----: | :-------------- | :----------------------------------------------------------------------- | :-------- |
|  [01]   | [TOTAL_FOLD]    | the interior probe ladder over `unknown` and its policy projections       | `Problem` |
|  [02]   | [PROBLEM_OWNER] | the RFC 9457 owner: body schema, response projection, `Cause` fold, guard | `Problem` |

## [2]-[TOTAL_FOLD]

[TOTAL_FOLD]:
- Law: the probe ladder is ordered by evidence specificity and every rung is structural — (1) an existing `Problem` passes through untouched; (2) `FaultDetail` by tag probe projects through `ProblemPolicy.upstream` with its `retryable`/`terminal` facts read off the value, detail always redacted; (3) `ParseError` lands on `malformed` and `RouteNotFound` on `absent`; (4) the residue classifies through `FaultClass.of` — class-carrying faults land on their row, a fault also carrying a `policy.status` row keeps that status over the class default, and everything else is `defect` — so the fold is total, closed, and free of any sibling-folder import.
- Law: detail text obeys exposure — an exposing class carries the fault's derived `message` (or `String(fault)` residue), a redacting class carries the policy title — and the grace hint probes the fault's own `retryAfter` field before the class default, so a quota refusal's measured window survives to the header.
- Law: extensions populate then pass the policy fold in one construction — `tag` and `reason` lift from the fault's own fields, `ProblemPolicy.redact` filters them against the allowlist and the exposure gate — so an over-shared member is structurally impossible downstream of this ladder.
- Growth: a new probe rung is one arm in `_of` plus its policy row in `problem/policy.ts`; a new extension is one populate line under the redact fold.
- Packages: `effect` (`Predicate`, `Option`, `Cause`, `Duration`); `kernel/fault/classify` (`FaultClass`); `problem/policy` (`ProblemPolicy`).

```typescript
import { FaultClass } from "@rasm/ts/kernel"
import { HttpServerResponse } from "@effect/platform"
import { Cause, Duration, Effect, Option, Predicate, Schema } from "effect"
import { Current } from "../api/middleware.ts"
import { ProblemPolicy } from "./policy.ts"

const _text = (fault: unknown): string =>
  Predicate.hasProperty(fault, "message") && Predicate.isString(fault.message) ? fault.message : String(fault)

const _field = (fault: unknown, key: string): Option.Option<string> => {
  const held = Predicate.hasProperty(fault, key) ? (fault as Record<string, unknown>)[key] : undefined
  return Predicate.isString(held) ? Option.some(held) : Option.none()
}

const _statused = (fault: unknown): Option.Option<number> =>
  Predicate.hasProperty(fault, "policy") && Predicate.hasProperty(fault.policy, "status") && Predicate.isNumber(fault.policy.status)
    ? Option.some(fault.policy.status)
    : Option.none()

const _graced = (fault: unknown): Option.Option<Duration.Duration> =>
  Predicate.hasProperty(fault, "retryAfter") && Option.isOption(fault.retryAfter)
    ? (fault.retryAfter as Option.Option<Duration.Duration>)
    : Option.none()

const _extensions = (kind: FaultClass.Kind, fault: unknown): Readonly<Record<string, string>> =>
  ProblemPolicy.redact(kind, {
    ...Option.match(_field(fault, "_tag"), { onNone: () => ({}), onSome: (tag) => ({ tag }) }),
    ...Option.match(_field(fault, "reason"), { onNone: () => ({}), onSome: (reason) => ({ reason }) }),
  })

const _classed = (fault: unknown): Problem => {
  const kind = FaultClass.of(fault)
  const grade = ProblemPolicy.grade(kind)
  return new Problem({
    type: ProblemPolicy.type(kind),
    title: grade.title,
    status: Option.getOrElse(_statused(fault), () => grade.status),
    detail: ProblemPolicy.expose(kind) ? _text(fault) : grade.title,
    instance: Option.none(),
    retry: ProblemPolicy.retryAfter(kind, _graced(fault)),
    extensions: _extensions(kind, fault),
  })
}

const _upstream = (fault: unknown): Problem => {
  const grade = ProblemPolicy.upstream({
    retryable: Predicate.hasProperty(fault, "retryable") && fault.retryable === true,
    terminal: Predicate.hasProperty(fault, "terminal") && fault.terminal === true,
  })
  return new Problem({
    type: `/problems/${grade.slug}`,
    title: grade.title,
    status: grade.status,
    detail: grade.title,
    instance: Option.none(),
    retry: Option.map(grade.grace, (grace) => Math.ceil(Duration.toMillis(grace) / 1000)),
    extensions: {},
  })
}

const _of = (fault: unknown): Problem =>
  fault instanceof Problem
    ? fault
    : Predicate.isTagged(fault, "FaultDetail")
      ? _upstream(fault)
      : Predicate.isTagged(fault, "ParseError")
        ? _classed({ class: "malformed", message: "request body refused" })
        : Predicate.isTagged(fault, "RouteNotFound")
          ? _classed({ class: "absent", message: "route absent" })
          : _classed(fault)

const _stamped = (problem: Problem, mark: Option.Option<Current.Mark>): Problem =>
  Option.match(mark, {
    onNone: () => problem,
    onSome: (held) =>
      new Problem({
        ...problem,
        instance: Option.some(`/requests/${held.id}`),
        extensions: { ...problem.extensions, requestId: held.id },
      }),
  })
```

## [3]-[PROBLEM_OWNER]

[PROBLEM_OWNER]:
- Owner: `Problem` — a `Schema.Class` carrying exactly the RFC members (`type`, `title`, `status`, `detail`, `instance` as `Option`) plus the allowlisted `extensions` band and the `retry` seconds the policy ladder resolved; the class is the value, the encode anchor, the fold entry, and the guard under one import, and its encoded twin is the wire body verbatim.
- Law: `Problem.respond` is the one egress projection — `Schema`-encoded body under `application/problem+json`, the `retry-after` header stamped exactly when `retry` is inhabited — and encoding our own `Problem` failing is a defect (`Effect.orDie`), never a channel member: the fault altitude cannot itself fault.
- Law: `Problem.fromCause` discriminates in interrupt-first order — `Cause.isInterruptedOnly` folds to the `unavailable` row (the guard only observes an interrupt under shed or shutdown), a typed failure re-enters the ladder, a defect lands on the `defect` row — the same order every telemetry outcome fold uses.
- Law: `Problem.guard` is the serve net — `Effect.matchCauseEffect` over the handler app: success passes through, every failure cause folds to a problem, stamps `instance`/`requestId` from the ambient `Current.Stamp`, and responds — so the served app's error channel is `never` by construction and an unmapped fault cannot escape as a naked 500; `instance` and `requestId` are one fact, both projecting the request mark.
- Boundary: the class-to-status record, exposure, grace, and upstream rows are `problem/policy.ts`'s; declared endpoint faults keep their `HttpApiEndpoint.addError` status — this owner is the net under everything undeclared; attachment is `api/serve.ts`'s one composition; log/OTLP emission of the folded cause is `telemetry`'s, fed from the same guard seam.
- Packages: `effect` (`Schema`, `Option`, `Effect`, `Cause`); `@effect/platform` (`HttpServerResponse`); `api/middleware` (`Current`).

```typescript
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
    HttpServerResponse.schemaJson(Problem)(problem).pipe(
      Effect.map(HttpServerResponse.setHeaders({
        "content-type": "application/problem+json",
        ...(Option.isSome(problem.retry) && { "retry-after": String(problem.retry.value) }),
      })),
      Effect.orDie,
    )
  static readonly guard = <E, R>(
    app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>,
  ): Effect.Effect<HttpServerResponse.HttpServerResponse, never, R> =>
    Effect.matchCauseEffect(app, {
      onFailure: (cause) =>
        Effect.flatMap(Current.Stamp, (mark) => Problem.respond(_stamped(Problem.fromCause(cause), mark))),
      onSuccess: Effect.succeed,
    })
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Problem }
```
