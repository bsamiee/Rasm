# [RUNTIME_PROBLEM]

`Problem` owns the outbound-fault law of the front door: every fault leaving the branch over HTTP renders ITSELF through the `HttpServerRespondable` symbol protocol, and this one RFC 9457 owner — body schema, governed class-to-status record, total fold from any refused value, `Cause` fold — is the value implementing it, so the central error-mapper middleware has no existence. Module `runtime/src/serve/problem.ts` ships on the `./server` exports subpath, and a new core fault class breaks the record loudly at compile time.

Ladder order is evidence specificity and every rung structural: an existing `Problem` passes untouched, the `Remote` tag the wire landing mints projects through the upstream rows on its `retryable`/`terminal` facts, a `ParseError` lands `malformed` and a `RouteNotFound` `absent`, and the residue classifies through `Fault.Class.of` into the governed record, free of any cross-branch import. A stated re-drive window survives every rung under its own altitude's word. Exposure derives from the core `blame` axis; the `extensions` band is CLOSED at the schema, so a key outside the vocabulary is unrepresentable in any `Problem` value. Inbound never touches this module.

## [01]-[INDEX]

- [02]-[STATUS_RECORD]: the class-to-status governed record, type-slug derivation, grace resolution; interior.
- [03]-[REDACTION_ROWS]: blame-derived exposure, the structural extension band, the redact fold; interior.
- [04]-[UPSTREAM_ROWS]: the wire-fault projection over structural `retryable`/`terminal` facts and the peer's stated window; interior.
- [05]-[RESPONDABLE_OWNER]: the RFC 9457 owner, the symbol implementation, the total fold, the edge net; `Problem`.

## [02]-[STATUS_RECORD]

[STATUS_RECORD]:
- Law: the problem `type` member derives — `_type(kind)` is `_TYPE_BASE` plus the class literal, so the type-URI vocabulary is the core key space and a hand-authored slug registry cannot exist; `about:blank` never appears because every fold lands on a class.
- Law: grace resolution is a two-rung ladder — a runtime hint carried by the fault value wins, the row's `grace` default fills, absence stays absent — `_retryAfter(grace, hint)` folds the ladder to whole seconds once, so no consumer re-derives header arithmetic.
- Law: one stated window, three altitudes, three words, and this door reads each under its own — the wire's `retry_after` decodes into the upstream fault's `retryAfter` recovery arm, a domain refusal carries `after` on the VALUE and `Fault.Class.statedOf` reads it back, and the response header is `retry-after`; a probe spelling one altitude's word at another altitude sees nothing and silently spends the row's default.
- Growth: a new core class is one row (compile-forced); a new response axis is one `_Grade` field plus its column on ten rows.
- Packages: `effect` (`Duration`, `Option`, `Record`, `Array`); `@rasm/core` (`Fault.Class`).

```typescript
import { HttpServerRespondable, HttpServerResponse } from "@effect/platform"
import { Array, Cause, Duration, Effect, Option, Predicate, Record, Schema } from "effect"
import { Fault } from "@rasm/core"
import { Current } from "./api.ts"

const _TYPE_BASE = "/problems/"

type _Grade = { readonly status: number; readonly title: string; readonly grace: Option.Option<Duration.Duration> }

const _Status = Schema.Int.pipe(Schema.between(100, 599))

const _rows: { readonly [K in Fault.Class.Kind]: _Grade } = {
  absent: { status: 404, title: "resource absent", grace: Option.none() },
  conflicted: { status: 409, title: "state conflict", grace: Option.none() },
  invalid: { status: 422, title: "unprocessable input", grace: Option.none() },
  malformed: { status: 400, title: "malformed request", grace: Option.none() },
  denied: { status: 403, title: "access denied", grace: Option.none() },
  expired: { status: 401, title: "credential not accepted", grace: Option.none() },
  exhausted: { status: 429, title: "quota exhausted", grace: Option.some(Duration.seconds(30)) },
  unavailable: { status: 503, title: "temporarily unavailable", grace: Option.some(Duration.seconds(10)) },
  breached: { status: 500, title: "internal fault", grace: Option.none() },
  defect: { status: 500, title: "internal fault", grace: Option.none() },
}

const _type = (kind: Fault.Class.Kind): string => `${_TYPE_BASE}${kind}`

const _retryAfter = (
  grace: Option.Option<Duration.Duration>,
  hint: Option.Option<Duration.Duration>,
): Option.Option<number> =>
  Option.map(Option.orElse(hint, () => grace), (held) => Math.ceil(Duration.toMillis(held) / 1000))
```

## [03]-[REDACTION_ROWS]

[REDACTION_ROWS]:
- Law: the extension band is structural — `_Extensions` is the exact-optional record over `tag`, `reason`, and `requestId`, the `_EXPOSED` tuple anchors the key census, and the guard pair closes tuple and schema against each other in both directions; because the `Problem` class carries this schema as its `extensions` field, a key outside the vocabulary is unrepresentable in any `Problem` value — the allowlist cannot be bypassed at a construction site, and a new public member is one field row plus its tuple entry.
- Law: the reason probe reads the fault's own subject seat — `case` on a single-issue raise, `dominant` on a census — so a family that closed its reason axis behind a declared subject still exposes the word a caller acts on; the free top-level field it replaced is the retired shape and probing it would blank the extension on every conformed fault.
- Law: `_redact(kind, extensions)` owns the blame gate over the structural band — a non-exposing class empties `tag` and `reason` while `requestId` survives on every class, because correlation is the one occurrence datum a system-blamed problem must keep: the operator resolves the redacted body against telemetry through it.
- Boundary: which values populate the extensions is `[05]`'s fold; log-side and OTLP-side scrubbing is `otel/emit#REDACTION`'s policy; this cluster fixes only what crosses the HTTP body outward.

```typescript
const _EXPOSED = ["tag", "reason", "requestId"] as const

const _Extensions = Schema.Struct({
  tag: Schema.String,
  reason: Schema.String,
  requestId: Schema.String,
}).pipe(Schema.partialWith({ exact: true }))

const _expose = (kind: Fault.Class.Kind): boolean => Fault.Class.blameOf(kind) === "caller"

const _redact = (kind: Fault.Class.Kind, extensions: Problem.Extensions): Problem.Extensions =>
  _expose(kind)
    ? extensions
    : Option.match(Option.fromNullable(extensions.requestId), {
        onNone: () => ({}),
        onSome: (requestId) => ({ requestId }),
      })
```

## [04]-[UPSTREAM_ROWS]

[UPSTREAM_ROWS]:
- Owner: `_upstream` — the two-row projection for the wire-reconstructed fault: a `retryable` non-`terminal` upstream hop projects as 503 under the peer's own stated window where its recovery arm carried one and the unavailable grace window otherwise, everything else refuses as 502 with no grace — the `terminal` fact vetoes re-drive even where the hop claims retryability, so a wrong-program upstream reads distinctly from a saturated one and never invites a retry.
- Law: the probe is structural, never an import — `[05]`'s ladder recognizes the `Remote` tag `core/interchange/codec#LANDING_WIRE` mints and reads the `retryable` and `terminal` members that owner publishes as facts beside its decoded `recovery` arm, so the serve-to-interchange edge stays import-free while the wire altitude still exits through the one problem door.
- Law: the arm is CONSUMED, not merely recognized — the peer's `retryAfter` delay projects into the response's own `retry-after` header, so a window an upstream measured survives the hop instead of being replaced by this row's default.
- Law: the probed tag and the probed members are that owner's LANDED spelling, so a rung naming a shape no producer mints admits nothing and hands every upstream refusal to the residue arm, which grades this process's own defect over a peer's refusal and renders 500 where the pair below render 502 and 503.
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

## [05]-[RESPONDABLE_OWNER]

[RESPONDABLE_OWNER]:
- Owner: `Problem` — a `Schema.Class` carrying exactly the RFC members (`type`, `title`, `status`, `detail`, `instance` as `Option`) plus the CLOSED `extensions` band and the `retry` seconds the grace ladder resolved; the class is the value, the encode anchor, the fold entry, and the self-rendering respondable under one import, and its encoded twin is the wire body verbatim.
- Law: the symbol implementation IS the egress projection — `Problem` implements `[HttpServerRespondable.symbol]()` as its own `respond`: `Schema`-encoded body under `application/problem+json` at the problem's own `status`, the `retry-after` header stamped exactly when `retry` is inhabited, `instance` and the `requestId` extension stamped from the ambient `Current.Stamp` inside the render so every egress path carries correlation; encoding the branch's own `Problem` failing is a defect (`Effect.orDie`), never a channel member — the fault altitude cannot itself fault.
- Law: the class ALONE answers the status — a folder fault reaching this ladder carries `class` and nothing else of the taxonomy, so no per-fault status override exists to read and `[02]`'s record is the branch's one class-to-status site in fact, not only in claim; a refusal needing a code the record does not spell is a missing core class, never a serve-local column.
- Law: `Problem.fromCause` discriminates in interrupt-first order — `Cause.isInterruptedOnly` folds to the `unavailable` row (the edge only observes an interrupt under shed or shutdown), a typed failure re-enters the ladder, a defect lands on the `defect` row — the same order every telemetry outcome fold uses.
- Law: the net is self-rendering-first, never a mapper — `Problem.net(cause)` folds the cause once, then renders: a failure implementing the symbol runs its OWN projection, and everything else rides the total ladder into `Problem.respond`. Both arms split on `HttpServerRespondable.isRespondable` because the ladder's render is effectful (it reads the ambient stamp) where the platform's `toResponseOrElse` demands a settled response value.
- Law: the opt-in arm invokes the symbol DIRECTLY and catches its whole cause — `HttpServerRespondable.toResponse` is `orDie` over a `respond` the platform types `unknown`, so it converts a refusing projection into a defect this edge's cause fold never observes and the platform answers a bare 500 carrying no body, no correlation, and no derived shield. Catching that cause routes the refusal back onto the ladder, the render the fault takes when it never opts in, so the served app's error channel is `never` in fact.
- Law: declared endpoint faults keep their `HttpApiEndpoint.addError` status at the spec altitude; this net is the floor under everything undeclared, and the class family DECIDES no tenancy and no lifetime — a status grades a refusal, and the request that carried it ended at the edge that rendered this body.
- Law: unsupported request media is the protocol's exact 415 row, minted only through `Problem.media`; it does not masquerade as malformed syntax and no route authors a status literal.
- Boundary: attachment is `route#EDGE_ROWS`'s one composition; log/OTLP emission of the folded cause is `otel/crash#CAPTURE`'s, fed from the same edge; the class table and blame axis are `core/value/fault#CLASS_VOCABULARY`'s.
- Growth: a new probe rung is one arm in `_of` plus its row in `[02]`; a new extension is one field row under `[03]`'s band plus its populate line.
- Packages: `effect` (`Schema`, `Option`, `Effect`, `Cause`, `Predicate`); `@effect/platform` (`HttpServerRespondable`, `HttpServerResponse`); `./api.ts` (`Current`).

```typescript
const _text = (fault: unknown): string =>
  Predicate.hasProperty(fault, "message") && Predicate.isString(fault.message) ? fault.message : String(fault)

const _field = (fault: unknown, key: string): Option.Option<string> => {
  const held = Predicate.hasProperty(fault, key) ? fault[key] : undefined
  return Predicate.isString(held) ? Option.some(held) : Option.none()
}

const _Remote = Schema.Struct({
  _tag: Schema.Literal("Remote"),
  retryable: Schema.Boolean,
  terminal: Schema.Boolean,
  recovery: Schema.Union(
    Schema.Struct({ kind: Schema.Literal("terminal") }),
    Schema.Struct({ kind: Schema.Literal("transient") }),
    Schema.Struct({ kind: Schema.Literal("retryAfter"), delay: Schema.DurationFromSelf }),
  ),
})

const _isRemote: (fault: unknown) => fault is typeof _Remote.Type = Schema.is(_Remote)

const _hopGrace = (fault: typeof _Remote.Type): Option.Option<Duration.Duration> =>
  fault.recovery.kind === "retryAfter" ? Option.some(fault.recovery.delay) : Option.none()

const _Reasoned = Schema.Union(
  Schema.Struct({ case: Schema.Struct({ reason: Schema.String }) }),
  Schema.Struct({ dominant: Schema.Struct({ reason: Schema.String }) }),
)

const _isReasoned: (fault: unknown) => fault is typeof _Reasoned.Type = Schema.is(_Reasoned)

const _reason = (fault: unknown): Option.Option<string> =>
  _isReasoned(fault) ? Option.some("case" in fault ? fault.case.reason : fault.dominant.reason) : Option.none()

const _extensions = (kind: Fault.Class.Kind, fault: unknown): Problem.Extensions =>
  _redact(kind, {
    ...Option.match(_field(fault, "_tag"), { onNone: () => ({}), onSome: (tag) => ({ tag }) }),
    ...Option.match(_reason(fault), { onNone: () => ({}), onSome: (reason) => ({ reason }) }),
  })

const _classed = (fault: unknown): Problem => {
  const kind = Fault.Class.of(fault)
  const grade = _rows[kind]
  return new Problem({
    type: _type(kind),
    title: grade.title,
    status: grade.status,
    detail: _expose(kind) ? _text(fault) : grade.title,
    instance: Option.none(),
    retry: _retryAfter(grade.grace, Fault.Class.statedOf(fault)),
    extensions: _extensions(kind, fault),
  })
}

const _projected = (fault: typeof _Remote.Type): Problem => {
  const grade = _hop(fault)
  return new Problem({
    type: grade.type,
    title: grade.title,
    status: grade.status,
    detail: grade.title,
    instance: Option.none(),
    retry: _retryAfter(grade.grace, _hopGrace(fault)),
    extensions: {},
  })
}

const _of = (fault: unknown): Problem =>
  fault instanceof Problem
    ? fault
    : _isRemote(fault)
      ? _projected(fault)
      : Predicate.isTagged(fault, "ParseError")
        ? _classed({ class: "malformed", message: "request body refused" })
        : Predicate.isTagged(fault, "RouteNotFound")
          ? _classed({ class: "absent", message: "route absent" })
          : _classed(fault)

const _media = (detail: string): Problem => new Problem({
  type: `${_TYPE_BASE}unsupported-media`,
  title: "unsupported media type",
  status: 415,
  detail,
  instance: Option.none(),
  retry: Option.none(),
  extensions: {},
})

class Problem extends Schema.Class<Problem>("Problem")({
  type: Schema.NonEmptyString,
  title: Schema.NonEmptyString,
  status: _Status,
  detail: Schema.String,
  instance: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  retry: Schema.optionalWith(Schema.NonNegativeInt, { as: "Option" }),
  extensions: _Extensions,
}) {
  static readonly of: (fault: unknown) => Problem = _of
  static readonly media = _media
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
          ? Effect.catchAllCause(fault[HttpServerRespondable.symbol](), () => Problem.respond(_of(fault)))
          : Problem.respond(_of(fault)),
    })
  [HttpServerRespondable.symbol](): Effect.Effect<HttpServerResponse.HttpServerResponse> {
    return Problem.respond(this)
  }
}

declare namespace Problem {
  type Extensions = typeof _Extensions.Type
  type _Band<K extends keyof Extensions = (typeof _EXPOSED)[number]> = K
  type _Census<K extends (typeof _EXPOSED)[number] = keyof Extensions> = K
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Problem }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
