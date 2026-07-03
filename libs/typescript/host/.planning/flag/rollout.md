# [HOST_ROLLOUT]

Targeting is data: a rollout is one closed, recursive rule family — fraction, segment, window, split, and the boolean composites — decoded from the provider as wire values and folded by one total `decide` into an outcome carrying its variant and its reason. The fold is pure and deterministic: the clock arrives as a value, the subject arrives as a value, and the percentage bucket derives from the kernel content-key mint so the same subject lands in the same bucket in every language and every runtime. Stickiness and verdict caching are policy rows beside the rules, never re-evaluation forks: a held variant is a ledger fact with an epoch and a lease, and a memoized verdict expires by its own reason. `verdict.md` composes everything here; entitlement claims stay in `security/authz`, which consumes verdicts and never targets.

## [1]-[INDEX]

- [01]-[TARGETING_RULES]: the recursive `Rule` family, the `Subject`/`Outcome` vocabulary, the deterministic bucket, the total `decide` fold.
- [02]-[STICKY_ROWS]: stickiness mode rows, the held-variant ledger shape, the reason-keyed expiry fold.

## [2]-[TARGETING_RULES]

- Owner: `Rollout` — the targeting owner. `Rollout.Rule` is one `Schema.Union` of tagged cases: `On`/`Off` (static arms), `Fraction` (salted percentage gate), `Segment` (axis membership over the subject's dimensions — the axes are the kernel `AppIdentity` span: app, tenant, build, host-fingerprint, plus free attributes), `Window` (a UTC validity interval), `Split` (weighted variant arms over the same salted bucket), and the composites `AllOf`/`AnyOf`/`Not` closing self-reference through `Schema.suspend`. Rules are wire values — the remote provider ships them, the family decodes them — so a new targeting dimension is one case row plus one fold arm, breaking every dispatch loudly.
- Law: `decide` is total — every rule folds to an `Outcome` (`on`, `variant: Option`, `reason`), and the reason rows are the shared OpenFeature evaluation vocabulary (`STATIC`, `DEFAULT`, `TARGETING_MATCH`, `SPLIT`, `CACHED`, `DISABLED`, `STALE`, `ERROR`, `UNKNOWN`) anchored here as one tuple; `verdict.md` projects these onto the wire contract and never re-declares them.
- Law: determinism is parameterization — `decide(rule, probe)` reads the wall clock and the bucket from the `probe` value (`at: DateTime.Utc`, `bucket: (salt) => number`), so evaluation is a pure fold provable by replay; an ambient clock or hash read inside the fold is the named defect.
- Law: bucket parity is a delegation fact — the bucket function is the low 32 bits of the kernel `XxHash128` seed-zero mint over `salt:subjectKey`, modulo 100; the kernel owns the mint, this page owns only the projection, and the C# evaluator lands identical buckets because the mint already holds cross-language parity.
- Boundary: rules arrive decoded (the provider document transits `wire/codec/flag` into this family); the fold neither fetches nor caches — sourcing is `verdict.md`'s, holding is `[3]`'s.
- Entry: `Rollout.decide(rule, probe)`; `Rollout.Rule` as the decode target.
- Packages: `effect` (`Schema`, `Match`, `Array`, `Option`, `DateTime`).

```typescript
import { Array, DateTime, Duration, Match, Option, Schema, pipe } from "effect"

const _REASONS = ["STATIC", "DEFAULT", "TARGETING_MATCH", "SPLIT", "CACHED", "DISABLED", "STALE", "ERROR", "UNKNOWN"] as const

const _On = Schema.TaggedStruct("On", {})
const _Off = Schema.TaggedStruct("Off", {})
const _Fraction = Schema.TaggedStruct("Fraction", {
  gate: Schema.Int.pipe(Schema.between(0, 100)),
  salt: Schema.NonEmptyString,
})
const _Segment = Schema.TaggedStruct("Segment", {
  axis: Schema.NonEmptyString,
  values: Schema.Array(Schema.NonEmptyString),
})
const _Window = Schema.TaggedStruct("Window", {
  from: Schema.DateTimeUtc,
  until: Schema.DateTimeUtc,
})
const _Split = Schema.TaggedStruct("Split", {
  arms: Schema.NonEmptyArray(Schema.Struct({ variant: Schema.NonEmptyString, weight: Schema.Number.pipe(Schema.positive()) })),
  salt: Schema.NonEmptyString,
})

interface _AllOfEncoded {
  readonly _tag: "AllOf"
  readonly rules: ReadonlyArray<_RuleEncoded>
}
interface _AnyOfEncoded {
  readonly _tag: "AnyOf"
  readonly rules: ReadonlyArray<_RuleEncoded>
}
interface _NotEncoded {
  readonly _tag: "Not"
  readonly rule: _RuleEncoded
}
type _RuleEncoded =
  | typeof _On.Encoded
  | typeof _Off.Encoded
  | typeof _Fraction.Encoded
  | typeof _Segment.Encoded
  | typeof _Window.Encoded
  | typeof _Split.Encoded
  | _AllOfEncoded
  | _AnyOfEncoded
  | _NotEncoded

const _Rule: Schema.Schema<Rollout.Rule, _RuleEncoded> = Schema.Union(
  _On,
  _Off,
  _Fraction,
  _Segment,
  _Window,
  _Split,
  Schema.TaggedStruct("AllOf", { rules: Schema.Array(Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule)) }),
  Schema.TaggedStruct("AnyOf", { rules: Schema.Array(Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule)) }),
  Schema.TaggedStruct("Not", { rule: Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule) }),
)

declare namespace Rollout {
  type Reason = (typeof _REASONS)[number]
  type Rule = typeof _On.Type | typeof _Off.Type | typeof _Fraction.Type | typeof _Segment.Type | typeof _Window.Type | typeof _Split.Type
    | { readonly _tag: "AllOf"; readonly rules: ReadonlyArray<Rule> }
    | { readonly _tag: "AnyOf"; readonly rules: ReadonlyArray<Rule> }
    | { readonly _tag: "Not"; readonly rule: Rule }
  type Subject = { readonly key: string; readonly axes: Readonly<Record<string, string>> }
  type Probe = { readonly subject: Subject; readonly at: DateTime.Utc; readonly bucket: (salt: string) => number }
  type Outcome = { readonly on: boolean; readonly variant: Option.Option<string>; readonly reason: Reason }
  type Shape = {
    readonly Rule: typeof _Rule
    readonly reasons: typeof _REASONS
    readonly decide: (rule: Rule, probe: Probe) => Outcome
  }
}

const _OUTCOME = (on: boolean, reason: Rollout.Reason, variant: Option.Option<string> = Option.none()): Rollout.Outcome =>
  ({ on, variant, reason })

const _picked = (arms: Array.NonEmptyReadonlyArray<{ readonly variant: string; readonly weight: number }>, point: number): Option.Option<string> =>
  pipe(
    Array.mapAccum(arms, 0, (spent, arm) => [spent + arm.weight, { until: spent + arm.weight, variant: arm.variant }] as const),
    ([total, spans]) => Array.findFirst(spans, (span) => (point * total) / 100 < span.until),
    Option.map((span) => span.variant),
  )

const _decide = (rule: Rollout.Rule, probe: Rollout.Probe): Rollout.Outcome =>
  Match.valueTags(rule, {
    On: () => _OUTCOME(true, "STATIC"),
    Off: () => _OUTCOME(false, "DISABLED"),
    Fraction: ({ gate, salt }) => (probe.bucket(salt) < gate ? _OUTCOME(true, "TARGETING_MATCH") : _OUTCOME(false, "DEFAULT")),
    Segment: ({ axis, values }) =>
      Option.match(Option.fromNullable(probe.subject.axes[axis]), {
        onNone: () => _OUTCOME(false, "DEFAULT"),
        onSome: (held) => (Array.contains(values, held) ? _OUTCOME(true, "TARGETING_MATCH") : _OUTCOME(false, "DEFAULT")),
      }),
    Window: ({ from, until }) =>
      DateTime.between(probe.at, { minimum: from, maximum: until })
        ? _OUTCOME(true, "TARGETING_MATCH")
        : _OUTCOME(false, "DEFAULT"),
    Split: ({ arms, salt }) =>
      Option.match(_picked(arms, probe.bucket(salt)), {
        onNone: () => _OUTCOME(false, "DEFAULT"),
        onSome: (variant) => _OUTCOME(true, "SPLIT", Option.some(variant)),
      }),
    AllOf: ({ rules }) =>
      pipe(
        Array.map(rules, (child) => _decide(child, probe)),
        (folds) =>
          Array.every(folds, (fold) => fold.on)
            ? _OUTCOME(true, "TARGETING_MATCH", Array.head(Array.getSomes(Array.map(folds, (fold) => fold.variant))))
            : _OUTCOME(false, "DEFAULT"),
      ),
    AnyOf: ({ rules }) =>
      Option.match(Array.findFirst(Array.map(rules, (child) => _decide(child, probe)), (fold) => fold.on), {
        onNone: () => _OUTCOME(false, "DEFAULT"),
        onSome: (fold) => fold,
      }),
    Not: ({ rule: inner }) =>
      pipe(_decide(inner, probe), (fold) => (fold.on ? _OUTCOME(false, "DEFAULT") : _OUTCOME(true, "TARGETING_MATCH"))),
  })

const Rollout: Rollout.Shape = { Rule: _Rule, reasons: _REASONS, decide: _decide }
```

## [3]-[STICKY_ROWS]

- Owner: `Sticky` — the holding policy beside the rules. Mode rows close the vocabulary: `none` (evaluate every read), `session` (memoize in-process — concurrent reads of one flag/subject pair collapse into one evaluation), `durable` (memoize plus a held-variant ledger surviving restarts — the browser's localStorage row and the node filesystem row satisfy the same `KeyValueStore` Tag). A held variant is `Sticky.Held`: flag, variant, epoch, and mint instant — one Schema class, one ledger shape on every runtime.
- Law: the epoch is the invalidation edge — a ruleset version bump retires every held variant at recall time (`recalled` folds an epoch mismatch to `None`), so stickiness never outlives the rules that granted it, and no ledger sweep exists.
- Law: expiry is reason-keyed — `Sticky.expiry(reason, lease)` folds `ERROR`/`STALE`/`UNKNOWN` outcomes to a short quarantine window and every settled outcome to the configured lease (`Setting.flag.sticky`), so a degraded evaluation never lingers as long as a targeted one; `verdict.md` hands this fold to its cache as the `timeToLive` policy.
- Boundary: the ledger is a `SchemaStore` over the abstract `KeyValueStore` Tag — binding is a root row; the memo tier is `verdict.md`'s `Cache`, this page owns only the policy values and folds it consumes.
- Packages: `effect` (`Schema`, `Duration`, `DateTime`, `Option`).

```typescript
const _modes = {
  none: { memo: false, ledger: false },
  session: { memo: true, ledger: false },
  durable: { memo: true, ledger: true },
} as const

class Held extends Schema.Class<Held>("Held")({
  flag: Schema.NonEmptyString,
  variant: Schema.NonEmptyString,
  epoch: Schema.Int,
  at: Schema.DateTimeUtc,
}) {}

declare namespace Sticky {
  type Mode = keyof typeof _modes
  type Row = { readonly memo: boolean; readonly ledger: boolean }
  type Shape = {
    readonly Held: typeof Held
    readonly modes: typeof _modes
    readonly expiry: (reason: Rollout.Reason, lease: Duration.Duration) => Duration.Duration
    readonly recalled: (held: Held, epoch: number, at: DateTime.Utc, lease: Duration.Duration) => Option.Option<string>
  }
  type _Rows<T extends Record<Mode, Row> = typeof _modes> = T
}

const _QUARANTINE = Duration.seconds(20)

const Sticky: Sticky.Shape = {
  Held,
  modes: _modes,
  expiry: (reason, lease) =>
    reason === "ERROR" || reason === "STALE" || reason === "UNKNOWN" ? _QUARANTINE : lease,
  recalled: (held, epoch, at, lease) =>
    held.epoch === epoch && Duration.lessThan(DateTime.distanceDuration(held.at, at), lease)
      ? Option.some(held.variant)
      : Option.none(),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Rollout, Sticky }
```
