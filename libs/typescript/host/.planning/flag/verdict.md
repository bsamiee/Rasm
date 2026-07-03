# [HOST_VERDICT]

Flag evaluation is total, runtime-neutral, and one vocabulary wide. `Verdict` is the branch projection of the shared OpenFeature evaluation contract — flag, typed value, variant, reason, error code, instant — the single shape a C#-evaluated `FlagVerdictWire` decodes into (through `wire/codec/flag`, admitted as `CACHED` evidence) and the single shape local evaluation mints, so browser apps and node services answer flag questions identically. `Flags` is the evaluation service: one ruleset cell patched live by the SSE feed with a poll backfill demoted beneath it, one reason-expiring memo collapsing concurrent evaluations, and one `evaluate` entry that never fails — a missing flag, a malformed rule, and a cold provider are verdict evidence (`reason: "ERROR"`, a code row), never channel faults. Entitlement claims stay in `security/authz`, which consumes these verdicts over its legal edge.

## [1]-[INDEX]

- [01]-[VERDICT_CONTRACT]: the OpenFeature projection — `Verdict`, the `Ruleset` document, the live `Shift` delta family.
- [02]-[FLAGS_SERVICE]: the evaluation owner — cell, feed, backfill, memo, and the total `evaluate`.

## [2]-[VERDICT_CONTRACT]

- Owner: `Verdict` — one `Schema.Class` carrying `flag`, `kind` (`boolean | string | number`), `value` (the kind-typed union), `variant: Option`, `reason` (the tuple spread of `Rollout.reasons` — one anchor, zero parallel spellings), `code: Option` (the OpenFeature error-code rows), and `at`. The wire twins ride the owner: `Verdict.Ruleset` is the provider document (an epoch plus a `Schema.HashMap` of flag rules), `Verdict.Shift` the live delta family (`Set | Clear | Reset`), and `Verdict.codes` the error-code anchor — one import carries the whole contract.
- Law: the contract is shared, not owned twice — `Rasm.AppHost` mints `FlagVerdictWire` over the same OpenFeature evaluation semantics; `wire/codec/flag` decodes it into this class, and `host` owns evaluation; a second verdict shape anywhere in the branch is the named defect (`ONE_FEATURE_FLAG_PROJECTION`).
- Law: the OpenFeature object-valued kind is deferred — its JSON-value arm is a branch catalogue RESEARCH row (no verified JSON-object schema member exists), and the kind row lands here and in `wire/codec/flag` together when the spelling settles; until then the value union stays exactly as wide as its kinds.
- Law: rules travel inside the document — `Ruleset.flags` values are `Rollout.Rule` trees, so targeting semantics arrive as decoded data and the provider never ships executable configuration.
- Law: deltas are epoch-guarded — `Reset` replaces the document only at an equal-or-newer epoch, `Set`/`Clear` patch single flags in place; a stale delta is a no-op by fold, never a race.
- Packages: `effect` (`Schema`, `Option`, `DateTime`, `HashMap`), `./rollout.ts` (`Rollout`).

```typescript
import { Cache, Data, DateTime, Effect, Exit, HashMap, Match, Option, Schedule, Schema, Stream, SubscriptionRef } from "effect"
import { Setting } from "../config/schema.ts"
import { Feed } from "../net/channel.ts"
import { Rollout, Sticky } from "./rollout.ts"

const _CODES = ["PROVIDER_NOT_READY", "FLAG_NOT_FOUND", "PARSE_ERROR", "TYPE_MISMATCH", "TARGETING_KEY_MISSING", "INVALID_CONTEXT", "GENERAL"] as const

class Ruleset extends Schema.Class<Ruleset>("Ruleset")({
  epoch: Schema.Int,
  flags: Schema.HashMap({ key: Schema.NonEmptyString, value: Rollout.Rule }),
}) {}

const _Shift = Schema.Union(
  Schema.TaggedStruct("Set", { flag: Schema.NonEmptyString, rule: Rollout.Rule }),
  Schema.TaggedStruct("Clear", { flag: Schema.NonEmptyString }),
  Schema.TaggedStruct("Reset", { ruleset: Ruleset }),
)

class Verdict extends Schema.Class<Verdict>("Verdict")({
  flag: Schema.NonEmptyString,
  kind: Schema.Literal("boolean", "string", "number"),
  value: Schema.Union(Schema.Boolean, Schema.String, Schema.Number),
  variant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  reason: Schema.Literal(...Rollout.reasons),
  code: Schema.optionalWith(Schema.Literal(..._CODES), { as: "Option" }),
  at: Schema.DateTimeUtc,
}) {
  static readonly Ruleset = Ruleset
  static readonly Shift = _Shift
  static readonly codes = _CODES
}

declare namespace Verdict {
  type Code = (typeof _CODES)[number]
  type Shift = typeof _Shift.Type
  type Document = Ruleset
  type Wire = typeof Verdict.Encoded
}
```

## [3]-[FLAGS_SERVICE]

- Owner: `Flags` — one `Effect.Service` whose `Default` is a Layer factory taking the bucket digest: the kernel `XxHash128` mint's low-32 projection, passed at the root so evaluation stays pure and cross-language bucket parity rides the existing content-key parity contract. The scoped build holds one `SubscriptionRef<Ruleset>` cell fed by one source: the live SSE feed (`Feed.open` on `Setting.flag.origin`, each `event.data` decoded through `Schema.parseJson(Verdict.Shift)`, every patch epoch-guarded). The provider's feed contract closes the backfill inside the same fold — a fresh connect and a cursor-bearing reattach both open with a `Reset` frame — and a dead feed, faulted or cleanly closed alike, re-registers on the `Setting.flag.cadence` pacing (`Effect.retry` plus `Effect.repeat` over one pace value — a succeeded drain is not an ended obligation), so a flip propagates in seconds, an outage backfills on reconnect, and no second flag source exists.
- Law: `evaluate` is total — the memo's lookup recalls the cell, folds `Rollout.decide` under the subject's salted bucket, and mints the verdict; a miss mints `reason: "ERROR"` carrying the stated fallback value with its code read from the cell's own evidence — `PROVIDER_NOT_READY` while the cell has never seen a `Reset` (epoch 0), `FLAG_NOT_FOUND` on a populated ruleset — so the error channel is `never` and every degradation is verdict evidence policy can read.
- Law: the memo is the stickiness tier — `Cache.makeWith` keyed by the `Data`-constructed (flag, subject, fallback) probe collapses concurrent evaluations of one key into one fold, its capacity the `_MEMO` policy row, and its `timeToLive` folds each verdict's reason through `Sticky.expiry` — the failure arm folds through the same quarantine, so no second duration literal exists — degraded verdicts expire in seconds while targeted verdicts hold the configured lease; the durable held-variant ledger (`Sticky.modes.durable`) composes the `Sticky.Held` store over the `KeyValueStore` Tag at the root.
- Law: a decode failure retains the last-good ruleset — a malformed delta folds to a skipped patch, never a cleared cell; `changes` exposes the cell's stream so consumers re-evaluate on every accepted patch.
- Boundary: runtime-neutral by construction — the build touches only `Setting`, `Feed`, and the digest parameter; the `./browser` subpath resolves this module unchanged, and the browser root satisfies `Feed` with its own bindings.
- Entry: `Flags.Default(digest)` at the root; `flags.evaluate(flag, subject, fallback)` everywhere.
- Receipt: every read is a `Verdict` — reason, code, variant, and instant travel with the value, so audit and telemetry consume evaluation evidence with no second surface.
- Packages: `effect` (`Cache`, `Data`, `DateTime`, `Effect`, `Exit`, `HashMap`, `Match`, `Option`, `Schedule`, `Schema`, `Stream`, `SubscriptionRef`), `../config/schema.ts` (`Setting`), `../net/channel.ts` (`Feed`), `./rollout.ts` (`Rollout`, `Sticky`).

```typescript
const _shifted = Schema.decodeUnknown(Schema.parseJson(_Shift))

const _MEMO = { capacity: 4096 } as const

const _patched = (held: Ruleset, shift: Verdict.Shift): Ruleset =>
  Match.valueTags(shift, {
    Set: ({ flag, rule }) => new Ruleset({ epoch: held.epoch, flags: HashMap.set(held.flags, flag, rule) }),
    Clear: ({ flag }) => new Ruleset({ epoch: held.epoch, flags: HashMap.remove(held.flags, flag) }),
    Reset: ({ ruleset }) => (ruleset.epoch >= held.epoch ? ruleset : held),
  })

const _minted = (flag: string, outcome: Rollout.Outcome, at: DateTime.Utc, fallback: boolean, code: Option.Option<Verdict.Code>): Verdict =>
  new Verdict({
    flag,
    kind: Option.isSome(outcome.variant) ? "string" : "boolean",
    value: Option.getOrElse(outcome.variant, (): string | boolean => (outcome.reason === "ERROR" ? fallback : outcome.on)),
    variant: outcome.variant,
    reason: outcome.reason,
    code,
    at,
  })

const _keyed = (flag: string, subject: Rollout.Subject, fallback: boolean) =>
  Data.struct({ flag, key: subject.key, axes: Data.struct(subject.axes), fallback })

class Flags extends Effect.Service<Flags>()("host/Flags", {
  scoped: (digest: (text: string) => number) =>
    Effect.gen(function* () {
      const setting = yield* Setting
      const feed = yield* Feed
      const cell = yield* SubscriptionRef.make(new Ruleset({ epoch: 0, flags: HashMap.empty() }))
      const pace = Schedule.spaced(setting.flag.cadence)

      yield* feed.open(setting.flag.origin).pipe(
        Stream.runForEach((event) =>
          _shifted(event.data).pipe(
            Effect.flatMap((shift) => SubscriptionRef.update(cell, (held) => _patched(held, shift))),
            Effect.catchAll(() => Effect.void),
          )),
        Effect.retry(pace),
        Effect.repeat(pace),
        Effect.forkScoped,
      )

      const memo = yield* Cache.makeWith({
        capacity: _MEMO.capacity,
        lookup: (probe: ReturnType<typeof _keyed>) =>
          Effect.gen(function* () {
            const held = yield* SubscriptionRef.get(cell)
            const at = yield* DateTime.now
            const bucket = (salt: string): number => digest(`${salt}:${probe.key}`) % 100
            const subject: Rollout.Subject = { key: probe.key, axes: probe.axes }
            return Option.match(HashMap.get(held.flags, probe.flag), {
              onNone: () =>
                _minted(probe.flag, { on: probe.fallback, variant: Option.none(), reason: "ERROR" }, at, probe.fallback,
                  Option.some(held.epoch === 0 ? "PROVIDER_NOT_READY" : "FLAG_NOT_FOUND")),
              onSome: (rule) => _minted(probe.flag, Rollout.decide(rule, { subject, at, bucket }), at, probe.fallback, Option.none()),
            })
          }),
        timeToLive: (exit) =>
          Exit.match(exit, {
            onFailure: () => Sticky.expiry("ERROR", setting.flag.sticky),
            onSuccess: (verdict) => Sticky.expiry(verdict.reason, setting.flag.sticky),
          }),
      })

      return {
        evaluate: (flag: string, subject: Rollout.Subject, fallback: boolean = false): Effect.Effect<Verdict> =>
          memo.get(_keyed(flag, subject, fallback)),
        changes: cell.changes,
      }
    }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Flags, Verdict }
```
