# [RUNTIME_FLAG]

Feature evaluation is one owner over the real OpenFeature server SDK: targeting is data — a closed recursive rule family decoded from the provider document and folded by one total `decide` whose percentage bucket derives from the kernel content-key mint, so the same subject lands in the same bucket in every language — and evaluation is the SDK's own lifecycle: this page's provider implements the SDK `Provider` contract over the live ruleset cell, registers through `OpenFeature.setProviderAndWait`, emits `ConfigurationChanged` on every accepted patch, and answers through the SDK client so hooks and evaluation context ride the standard seam, with `track` seating business outcomes on that same plane under the one targeting-identity join. `Verdict` is the branch projection of the shared evaluation contract — flag, kind-typed value (the object kind is a real arm over one recursive JSON schema), variant, reason, error code, error message, metadata, instant — the one shape local evaluation mints and the one a peer verdict decodes into at its admission boundary. Stickiness and memoization are policy rows: a held variant is a ledger fact with an epoch and a lease, a memoized verdict expires by its own reason, and `evaluate` never fails — a missing flag, a malformed rule, and a cold provider are verdict evidence, never channel faults. The `security` `FlagGate` port is satisfied here. The module is `runtime/src/proc/flag.ts`.

## [01]-[INDEX]

- [02]-[TARGETING_RULES]: the recursive rule family, the deterministic bucket, the total `decide` fold; `Rollout`.
- [03]-[STICKY_ROWS]: stickiness mode rows, the held-variant ledger, the reason-keyed expiry fold; `Sticky`.
- [04]-[VERDICT_CONTRACT]: the OpenFeature projection, the document and delta families, the JSON value arm; `Verdict`.
- [05]-[PROVIDER_OWNER]: the SDK `Provider` implementation, events, hooks, the tracking seat, the promise boundary; `Flags`.
- [06]-[GATE_SERVICE]: the evaluation service, the reason-expiring memo, the `FlagGate` satisfaction; `Flags`.

## [02]-[TARGETING_RULES]

[TARGETING_RULES]:
- Law: `decide` is total — every rule folds to an `Outcome` (`on`, `variant: Option`, `reason`), and the reason rows are the OpenFeature `StandardResolutionReasons` spellings anchored once as the `_REASONS` tuple; `Verdict` and the provider project these and never re-declare them.
- Law: determinism is parameterization — `decide(rule, probe)` reads the wall clock and the bucket from the `probe` value (`at: DateTime.Utc`, `bucket: (salt) => number`), so evaluation is a pure fold provable by replay; an ambient clock or hash read inside the fold is the named defect.
- Law: bucket parity is a delegation fact — the bucket function is the low 32 bits of the kernel `XxHash128` seed-zero mint over `salt:subjectKey`, modulo 100; `core/value/contentKey` owns the mint, this page owns only the projection, and the C# evaluator lands identical buckets because the mint already holds cross-language parity.
- Boundary: rules arrive decoded (the provider document transits the interchange codec into this family); the fold neither fetches nor caches — sourcing is `[4]`'s, holding is `[3]`'s.
- Entry: `Rollout.decide(rule, probe)`; `Rollout.Rule` as the decode target.
- Packages: `effect` (`Schema`, `Match`, `Array`, `Option`, `DateTime`).

```typescript signature
import type { UnknownEnum } from '@bufbuild/protobuf';
import { FlagReason } from '@rasm\/contracts/rasm/contracts/feature/v1/verdict_pb';
import { Wire } from '@rasm/core';
import { Array, DateTime, Duration, Effect, Match, Option, Record, Schema, String, pipe } from 'effect';

const _REASONS = ['STATIC', 'DEFAULT', 'TARGETING_MATCH', 'SPLIT', 'CACHED', 'DISABLED', 'STALE', 'ERROR', 'UNKNOWN'] as const;

const _On = Schema.TaggedStruct('On', {});
const _Off = Schema.TaggedStruct('Off', {});
const _Fraction = Schema.TaggedStruct('Fraction', {
    gate: Schema.Int.pipe(Schema.between(0, 100)),
    salt: Schema.NonEmptyString,
});
const _Segment = Schema.TaggedStruct('Segment', {
    axis: Schema.NonEmptyString,
    values: Schema.Array(Schema.NonEmptyString),
});
const _Window = Schema.TaggedStruct('Window', {
    from: Schema.DateTimeUtc,
    until: Schema.DateTimeUtc,
});
const _Split = Schema.TaggedStruct('Split', {
    arms: Schema.NonEmptyArray(Schema.Struct({ variant: Schema.NonEmptyString, weight: Schema.Number.pipe(Schema.positive()) })),
    salt: Schema.NonEmptyString,
});

interface _AllOfEncoded {
    readonly _tag: 'AllOf';
    readonly rules: readonly [_RuleEncoded, ...ReadonlyArray<_RuleEncoded>];
}
interface _AnyOfEncoded {
    readonly _tag: 'AnyOf';
    readonly rules: readonly [_RuleEncoded, ...ReadonlyArray<_RuleEncoded>];
}
interface _NotEncoded {
    readonly _tag: 'Not';
    readonly rule: _RuleEncoded;
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
    | _NotEncoded;

const _Rule: Schema.Schema<Rollout.Rule, _RuleEncoded> = Schema.Union(
    _On,
    _Off,
    _Fraction,
    _Segment,
    _Window,
    _Split,
    Schema.TaggedStruct('AllOf', { rules: Schema.NonEmptyArray(Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule)) }),
    Schema.TaggedStruct('AnyOf', { rules: Schema.NonEmptyArray(Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule)) }),
    Schema.TaggedStruct('Not', { rule: Schema.suspend((): Schema.Schema<Rollout.Rule, _RuleEncoded> => _Rule) }),
);

declare namespace Rollout {
    type Reason = (typeof _REASONS)[number];
    type Rule =
        | typeof _On.Type
        | typeof _Off.Type
        | typeof _Fraction.Type
        | typeof _Segment.Type
        | typeof _Window.Type
        | typeof _Split.Type
        | { readonly _tag: 'AllOf'; readonly rules: Array.NonEmptyReadonlyArray<Rule> }
        | { readonly _tag: 'AnyOf'; readonly rules: Array.NonEmptyReadonlyArray<Rule> }
        | { readonly _tag: 'Not'; readonly rule: Rule };
    type Subject = { readonly key: string; readonly axes: Readonly<Record<string, string>> };
    type Probe = { readonly subject: Subject; readonly at: DateTime.Utc; readonly bucket: (salt: string) => number };
    type Outcome = { readonly on: boolean; readonly variant: Option.Option<string>; readonly reason: Reason };
    type Shape = {
        readonly Rule: typeof _Rule;
        readonly reasons: typeof _REASONS;
        readonly decide: (rule: Rule, probe: Probe) => Outcome;
        readonly targeted: (rule: Rule) => boolean;
    };
}

const _OUTCOME = (on: boolean, reason: Rollout.Reason, variant: Option.Option<string> = Option.none()): Rollout.Outcome => ({ on, variant, reason });

const _picked = (arms: Array.NonEmptyReadonlyArray<{ readonly variant: string; readonly weight: number }>, point: number): Option.Option<string> =>
    pipe(
        Array.mapAccum(arms, 0, (spent, arm) => [spent + arm.weight, { until: spent + arm.weight, variant: arm.variant }] as const),
        ([total, spans]) => Array.findFirst(spans, (span) => (point * total) / 100 < span.until),
        Option.map((span) => span.variant),
    );

const _decide = (rule: Rollout.Rule, probe: Rollout.Probe): Rollout.Outcome =>
    Match.valueTags(rule, {
        On: () => _OUTCOME(true, 'STATIC'),
        Off: () => _OUTCOME(false, 'DISABLED'),
        Fraction: ({ gate, salt }) => (probe.bucket(salt) < gate ? _OUTCOME(true, 'TARGETING_MATCH') : _OUTCOME(false, 'DEFAULT')),
        Segment: ({ axis, values }) =>
            Option.match(Option.fromNullable(probe.subject.axes[axis]), {
                onNone: () => _OUTCOME(false, 'DEFAULT'),
                onSome: (held) => (Array.contains(values, held) ? _OUTCOME(true, 'TARGETING_MATCH') : _OUTCOME(false, 'DEFAULT')),
            }),
        Window: ({ from, until }) =>
            DateTime.between(probe.at, { minimum: from, maximum: until }) ? _OUTCOME(true, 'TARGETING_MATCH') : _OUTCOME(false, 'DEFAULT'),
        Split: ({ arms, salt }) =>
            Option.match(_picked(arms, probe.bucket(salt)), {
                onNone: () => _OUTCOME(false, 'DEFAULT'),
                onSome: (variant) => _OUTCOME(true, 'SPLIT', Option.some(variant)),
            }),
        AllOf: ({ rules }) =>
            pipe(
                Array.map(rules, (child) => _decide(child, probe)),
                (folds) =>
                    Array.every(folds, (fold) => fold.on)
                        ? _OUTCOME(true, 'TARGETING_MATCH', Array.head(Array.getSomes(Array.map(folds, (fold) => fold.variant))))
                        : _OUTCOME(false, 'DEFAULT'),
            ),
        AnyOf: ({ rules }) =>
            Option.match(
                Array.findFirst(
                    Array.map(rules, (child) => _decide(child, probe)),
                    (fold) => fold.on,
                ),
                {
                    onNone: () => _OUTCOME(false, 'DEFAULT'),
                    onSome: (fold) => fold,
                },
            ),
        Not: ({ rule: inner }) => pipe(_decide(inner, probe), (fold) => (fold.on ? _OUTCOME(false, 'DEFAULT') : _OUTCOME(true, 'TARGETING_MATCH'))),
    });

const _targeted = (rule: Rollout.Rule): boolean =>
    Match.valueTags(rule, {
        On: () => false,
        Off: () => false,
        Fraction: () => true,
        Segment: () => true,
        Window: () => false,
        Split: () => true,
        AllOf: ({ rules }) => Array.some(rules, _targeted),
        AnyOf: ({ rules }) => Array.some(rules, _targeted),
        Not: ({ rule: inner }) => _targeted(inner),
    });

const Rollout: Rollout.Shape = { Rule: _Rule, reasons: _REASONS, decide: _decide, targeted: _targeted };
```

## [03]-[STICKY_ROWS]

[STICKY_ROWS]:
- Owner: `Sticky` — the holding policy beside the rules. Mode rows close the vocabulary: `none` (evaluate every read), `session` (memoize in-process), `durable` (memoize plus a held-variant ledger surviving restarts — the browser localStorage row and the node filesystem row satisfy the same `KeyValueStore` Tag); a held variant is `Sticky.Held`: flag, variant, epoch, mint instant — one Schema class, one ledger shape on every runtime.
- Law: the epoch is the invalidation edge — a ruleset version bump retires every held variant at recall time (`recalled` folds an epoch mismatch to `None`), so stickiness never outlives the rules that granted it and no ledger sweep exists.
- Law: expiry is reason-keyed — `Sticky.expiry(reason, policy)` folds `ERROR`/`STALE`/`UNKNOWN` outcomes to `Setting.flag.quarantine` and every settled outcome to `Setting.flag.sticky`, so a degraded evaluation never lingers as long as a targeted one; `[5]`'s memo consumes this fold as its `timeToLive` policy and `Setting.flag.memo` as its admitted capacity.
- Boundary: the ledger is a `SchemaStore` over the abstract `KeyValueStore` Tag — binding is a root row; the memo tier is `[5]`'s `Cache`, this cluster owns only the policy values and folds it consumes.
- Packages: `effect` (`Schema`, `Duration`, `DateTime`, `Option`).

```typescript signature
const _modes = {
    none: { memo: false, ledger: false },
    session: { memo: true, ledger: false },
    durable: { memo: true, ledger: true },
} as const;

class Held extends Schema.Class<Held>('Held')({
    flag: Schema.NonEmptyString,
    variant: Schema.NonEmptyString,
    epoch: Schema.Int,
    at: Schema.DateTimeUtc,
}) {}

declare namespace Sticky {
    type Mode = keyof typeof _modes;
    type Row = { readonly memo: boolean; readonly ledger: boolean };
    type Shape = {
        readonly Held: typeof Held;
        readonly modes: typeof _modes;
        readonly expiry: (
            reason: Rollout.Reason,
            policy: { readonly lease: Duration.Duration; readonly quarantine: Duration.Duration },
        ) => Duration.Duration;
        readonly recalled: (held: Held, epoch: number, at: DateTime.Utc, lease: Duration.Duration) => Option.Option<string>;
    };
    type _Rows<T extends Record<Mode, Row> = typeof _modes> = T;
}

const Sticky: Sticky.Shape = {
    Held,
    modes: _modes,
    expiry: (reason, policy) => (reason === 'ERROR' || reason === 'STALE' || reason === 'UNKNOWN' ? policy.quarantine : policy.lease),
    recalled: (held, epoch, at, lease) =>
        held.epoch === epoch && Duration.lessThan(DateTime.distanceDuration(held.at, at), lease) ? Option.some(held.variant) : Option.none(),
};
```

## [04]-[VERDICT_CONTRACT]

[VERDICT_CONTRACT]:
- Owner: `Verdict` — one `Schema.Class` carrying `flag`, `kind` (`boolean | string | number | object`), `value` (the kind-typed union whose object arm is the page's one recursive `_Json` schema), `variant: Option`, `reason`, the complete OpenFeature error-code row including `PROVIDER_FATAL`, optional `errorMessage`, `flagMetadata`, and `at`; the wire twins ride the owner — `Verdict.Ruleset` is the provider document, `Verdict.Shift` the live delta family (`Set | Clear | Reset`), `Verdict.codes` the error-code anchor — one import carries the whole contract.
- Law: the contract is shared, not owned twice, and the wire DECODES ONCE — `csharp:Rasm.AppHost/Runtime/features#TS_PROJECTION` mints `FlagVerdictWire` over the same OpenFeature evaluation semantics, and `Verdict.admitted` is its first typed consumer: it accepts the opaque frame, calls `Wire.decode` for that direct family, and projects the decoded value without exporting a parallel wire shape or proof-only facade.
- Law: the producer's reason roster is its own and crosses through one total map — it spells `targeting` where the SDK spells `TARGETING_MATCH` and rows no staleness word at all, so `_WIRE_REASON` is the whole crossing and an arrival keeps the reason its own evaluator settled on rather than being restamped as this process's cache read.
- Law: the document row is a flag definition — `FlagDef` carries `kind`, the targeting `rule`, and the per-variant value map whose values ride `_Json`, so an object-valued flag is one definition row and value resolution is a variant lookup; the definition's `kind` gates type agreement at resolution, a mismatch minting `TYPE_MISMATCH` evidence.
- Law: every delta carries its source epoch — `Reset` replaces the document and `Set`/`Clear` patch one flag only when their epoch is equal to or newer than the held document; accepted rows advance `Ruleset.epoch`, stale rows are no-ops, and reset invalidation is the symmetric key difference so removed flags invalidate beside added and changed flags.
- Packages: `effect` (`Schema`, `Option`, `DateTime`, `HashMap`); `@rasm/core` (`Wire.decode` — direct-family first-sight admission); `@openfeature/server-sdk` (`JsonValue` — `_Json` types itself against the SDK's own JSON union, so the provider seam and the `get*Details` calls carry no cast).

```typescript signature
const _CODES = [
    'PROVIDER_NOT_READY',
    'PROVIDER_FATAL',
    'FLAG_NOT_FOUND',
    'PARSE_ERROR',
    'TYPE_MISMATCH',
    'TARGETING_KEY_MISSING',
    'INVALID_CONTEXT',
    'GENERAL',
] as const;

const _Json: Schema.Schema<JsonValue> = Schema.Union(
    Schema.Boolean,
    Schema.Number,
    Schema.String,
    Schema.Null,
    Schema.mutable(Schema.Array(Schema.suspend((): Schema.Schema<JsonValue> => _Json))),
    Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.suspend((): Schema.Schema<JsonValue> => _Json) })),
);

const _KINDS = ['boolean', 'string', 'number', 'object'] as const;

class FlagDef extends Schema.Class<FlagDef>('FlagDef')({
    kind: Schema.Literal(..._KINDS),
    rule: _Rule,
    variants: Schema.Record({ key: Schema.String, value: _Json }),
    fallback: Schema.NonEmptyString,
}) {}

class Ruleset extends Schema.Class<Ruleset>('Ruleset')({
    epoch: Schema.Int,
    flags: Schema.HashMap({ key: Schema.NonEmptyString, value: FlagDef }),
}) {}

const _Shift = Schema.Union(
    Schema.TaggedStruct('Set', { epoch: Schema.Int, flag: Schema.NonEmptyString, def: FlagDef }),
    Schema.TaggedStruct('Clear', { epoch: Schema.Int, flag: Schema.NonEmptyString }),
    Schema.TaggedStruct('Reset', { ruleset: Ruleset }),
);

// The peer verdict's reason roster is the corpus's `FlagReason` enum, and it is neither of the two this page already
// holds: it spells `TARGETING` where the SDK spells `TARGETING_MATCH` and rows no staleness word at all. Deriving it
// from either neighbour would refuse every real targeting document, so the crossing is one total map keyed by the
// generated members — a member added at the corpus breaks this table instead of arriving as an unroutable number —
// and the producer's own `UNKNOWN` arm is the typed fall-through for the open-enum value the generated type admits.
type _WireReason = Exclude<FlagReason, UnknownEnum | typeof FlagReason.UNSPECIFIED>;
const _WIRE_REASON = {
    [FlagReason.CACHED]: 'CACHED',
    [FlagReason.DEFAULT]: 'DEFAULT',
    [FlagReason.DISABLED]: 'DISABLED',
    [FlagReason.ERROR]: 'ERROR',
    [FlagReason.SPLIT]: 'SPLIT',
    [FlagReason.STATIC]: 'STATIC',
    [FlagReason.TARGETING]: 'TARGETING_MATCH',
    [FlagReason.UNKNOWN]: 'UNKNOWN',
} as const satisfies { readonly [R in _WireReason]: Rollout.Reason };
const _wireReason = Schema.is(Schema.Literal(...Record.values(FlagReason).filter((member): member is _WireReason => member !== FlagReason.UNSPECIFIED)));

class Verdict extends Schema.Class<Verdict>('Verdict')({
    flag: Schema.NonEmptyString,
    kind: Schema.Literal(..._KINDS),
    value: _Json,
    variant: Schema.optionalWith(Schema.NonEmptyString, { as: 'Option' }),
    reason: Schema.Literal(...Rollout.reasons),
    code: Schema.optionalWith(Schema.Literal(..._CODES), { as: 'Option' }),
    errorMessage: Schema.optionalWith(Schema.String, { as: 'Option' }),
    flagMetadata: Schema.Record({ key: Schema.String, value: _Json }),
    at: Schema.DateTimeUtc,
}) {
    static readonly Def = FlagDef;
    static readonly Ruleset = Ruleset;
    static readonly Shift = _Shift;
    static readonly codes = _CODES;
    // The one admission decodes the direct family at first sight and projects its result without another wire shape.
    static readonly admitted = (bytes: Uint8Array, at: DateTime.Utc) =>
        Effect.map(Wire.decode('FlagVerdictWire', bytes), (wire) =>
            new Verdict({
                flag: wire.flag,
                kind: 'boolean',
                value: wire.value,
                // proto3 spells an unset singular string as `""`, so absence lifts to the branch carrier at this one seat
                variant: Option.liftPredicate(wire.variant, String.isNonEmpty),
                reason: _wireReason(wire.reason) ? _WIRE_REASON[wire.reason] : 'UNKNOWN',
                code: Option.none(),
                errorMessage: Option.none(),
                flagMetadata: {},
                at,
            }),
        );
}

declare namespace Verdict {
    type Code = (typeof _CODES)[number];
    type Kind = (typeof _KINDS)[number];
    type Json = JsonValue;
    type Shift = typeof _Shift.Type;
    type Document = Ruleset;
    type Wire = typeof Verdict.Encoded;
}
```

## [05]-[PROVIDER_OWNER]

[PROVIDER_OWNER]:
- Owner: the SDK `Provider` implementation the Layer constructs over the live ruleset cell — `runsOn: "server"`, `metadata`, one `OpenFeatureEventEmitter`, the four `resolve*Evaluation` members delegating to one interior `_resolved(kind, flag, fallback, context)` that recalls the cell, folds `Rollout.decide` under the subject's salted bucket, resolves the variant's value from the definition's map, and answers `ResolutionDetails` — value, variant, reason, `errorCode` on degradation; the object kind rides `resolveObjectEvaluation` over the `_Json` arm, so the SDK's whole kind surface is real.
- Law: the kind/value correlation is proven, never asserted — the `_Value` table maps each kind to its value type, `_guards` is the mapped guard record `(value: Verdict.Json) => value is _Value[K]`, and `_resolved` is generic over `K extends Verdict.Kind` returning `ResolutionDetails<_Value[K]>`, so the guard's narrowing IS the evidence and the three primitive members are cast-free; the SDK's `resolveObjectEvaluation<T extends JsonValue>` generic promises the caller's `T` with no runtime witness — a foreign contract unsound by its own declaration — so that one member is the marked boundary adapter: the guard proves the JSON-object shape and the pin crosses the SDK's own seam on one marked line, nowhere else.
- Law: the promise members are the platform-forced boundary — each `resolve*` bridges through the runtime captured at Layer build (`Effect.runtime` then `Runtime.runPromise`), the sanctioned callback-seam spelling, and the bridged effect is total, so a provider promise never rejects on an evaluation condition.
- Law: the optional lifecycle pair stays unimplemented by design — readiness is the cell's own epoch, which `PROVIDER_NOT_READY` already reports and the feed alone advances, so an `initialize` arm restates a readiness `setProviderAndWait` already awaits, and the Layer scope owns the feed fiber, so an `onClose` arm releases what the scope releases. `initialize`'s domain argument is the seam a multi-domain deployment specializes on; this owner registers one provider on the default domain, so the argument decides nothing here and a second domain earns a specialization arm rather than a second provider.
- Law: events are the invalidation edge and the HEALTH edge, and the server event map carries exactly four rows the feed fold drives from its own seams — every accepted patch emits `ConfigurationChanged` with the changed flag keys, a malformed patch emits `Error`, feed death emits `Stale` before the reconnect budget runs, and an accepted `Reset` emits `Ready` because a whole document is the plane's own recovery edge. Without them the cell freezes at its last epoch and `evaluate` keeps answering `CACHED`/`STATIC` off a document nobody refreshes, with no reason column separating that from health and nothing for `client.addHandler` consumers or the `Setting.flag.quarantine` posture to observe; `Rollout.Reason` already carries `STALE`, so the reason vocabulary was waiting on the producer. `OpenFeatureEventEmitter` is the one health producer, so a poll or side-channel epoch probe beside it has no spelling.
- Law: outcome association is a provider member, not a service entry — the client NO-OPS `track` unless the registered `Provider` implements the optional member, so the seat is the provider literal and any caller above it only forwards. The member projects the call onto convention rows: the event name is `rasm.flag.event`, the optional numeric `value` is `rasm.flag.value`, the remaining `TrackingEventDetails` members render into the one `rasm.flag.detail` payload, and `feature_flag.context.id` carries the targeting identity. That identity is the WHOLE join — a tracking call names a business outcome and carries no flag key, so the coordinate the telemetry hook already stamps on every evaluation is the only thing both planes share, and an outcome spelled under a flag-key row asserts a flag the call never named.
- Law: the tracking member is the SDK's one synchronous seat — it answers `void`, never a promise, so the bridge is `Runtime.runSync` over a total effect rather than the promise bridge the `resolve*` members take; a promise here strands its work off the caller's stack, and the client wraps the call in a `try` that swallows a throw into a debug log, so any failure the seat could raise would vanish as silent evidence loss. The client freezes the MERGED context before the call, so global, client, and transaction altitudes all reach the member with no second context shape, and a track before readiness short-circuits inside the client and never arrives.
- Law: the occurrence rides the metric plane beside the span rows, because a trace plane is sampled and an experiment reads its outcomes as a rate — `rasm.flag.tracked` is a frequency row whose word axis IS the event name, so the root hands its outcome CENSUS — the published vocabulary its own owner minted, never a tuple assembled at the call — once at the Layer factory, every declared word reports zero before its first occurrence, and an undeclared one still counts rather than being dropped. The magnitude never joins it: a caller-defined `value` carries no dimension, so one summed series would fold currency, duration, and arity into a code no UCUM row spells, and the wide event is where that evidence is read.
- Law: the subject projects from the SDK `EvaluationContext` — `targetingKey` is the subject key (absent folds to `TARGETING_KEY_MISSING` evidence on targeted rules), string-valued attributes are the axes — so context construction is the SDK's standard seam and transaction-context propagation composes at the app edge, never a parallel context shape; that composition is an EXPLICIT install, because the SDK's default transaction propagator is the no-op — a root wanting request-scoped context sets the async-local-storage propagator through `OpenFeature.setTransactionContextPropagator` once at boot, and a deployment that never installs one reads only the global and per-call altitudes.
- Law: kind agreement is evidence — a resolved value that fails the requested kind's guard answers the fallback with `TYPE_MISMATCH`; a cold cell (epoch 0) answers `PROVIDER_NOT_READY`; a populated cell missing the flag answers `FLAG_NOT_FOUND` — the error channel stays empty and every degradation is data.
- Packages: `@openfeature/server-sdk` (`Provider`, `ResolutionDetails`, `EvaluationContext`, `OpenFeatureEventEmitter`, `ProviderEvents`, `JsonValue`, `TrackingEventDetails`, `TrackingEventValue`), `effect` (`Effect`, `Runtime`, `Metric`, `Option`, `HashMap`, `DateTime`).

## [06]-[GATE_SERVICE]

[GATE_SERVICE]:
- Owner: `Flags` — one `Effect.Service` whose `Default` is a Layer factory taking the bucket digest, a `Sticky.Mode` policy value (the root selects `none | session | durable`, never a call-site knob), and the business-outcome roster the tracking counter preregisters. Its scoped build holds one `SubscriptionRef<Ruleset>` cell fed by one source — the live SSE feed (`net/channel#FEED_SEAM` on `Setting.flag.origin`, each `event.data` decoded through `Schema.parseJson(Verdict.Shift)`, every patch epoch-guarded, a decode failure folding to a skipped patch, never a cleared cell); the session reconnects internally under the feed budget, and only its exhaustion re-opens the feed on the `Setting.flag.cadence` pacing — constructs the provider over the cell, registers it through `OpenFeature.setProviderAndWait`, closes the SDK on scope release, and answers every read through the SDK client so registered hooks observe every evaluation.
- Law: `evaluate` is total — the memo's lookup calls the SDK client's `get*Details` member for the probe's kind, projects the `EvaluationDetails` into a `Verdict`, and folds any rejection to `reason: "ERROR"` with `GENERAL` code and the stated fallback, so the error channel is `never` and every degradation is verdict evidence policy reads.
- Law: the mode row is executable — `none` calls the lookup directly, `session` and `durable` route through `Cache.makeWith`, and `durable` recalls and records `Sticky.Held` through `KeyValueStore.forSchema(Sticky.Held)` on top of that route. Recall validates both epoch and lease before projecting the held variant through the current definition; storage failure degrades to live evaluation, never fails `evaluate`, while an accepted live variant is persisted through an explicitly ruled best-effort tap. `ConfigurationChanged` invalidates the process memo wholesale; durable invalidation remains epoch-based, so no ledger sweep exists.
- Law: one telemetry hook rides registration — an SDK `Hook` whose `after` stamps the active span with the flag key, provider name, resolved reason, resolved variant, and targeting identity as `core/observe/convention` rows under one `Convention.Attributes`-checked record — so evaluation evidence reaches the trace plane through the SDK's own lifecycle, never a hand tap inside `evaluate` and never a free-string attribute key. The last two rows are what a tracked outcome lands against: without the identity an outcome joins nothing, and without the variant it counts but attributes to no arm.
- Law: this owner is the one dialect crossing between the SDK's uppercase `StandardResolutionReasons` and the spec's lowercase result-reason values — `_SPEC` is the total keyed map, the convention owner holds the spec vocabulary alone, and an unrecognized SDK reason folds to the spec's own unknown row rather than reaching the wire raw.
- Law: `Flags.gate` satisfies the `security` port — a Layer requiring the already-built `Flags` service and projecting the claim set to subject axes — so the access fold and direct consumers share one provider cell, memo, feed, and SDK lifecycle; the gate never mounts a second `Flags.Default` beneath itself.
- Entry: `Flags.Default(digest, mode, outcomes)` at the root; `flags.evaluate(flag, subject, fallback)` everywhere; `Flags.gate` beside it for the access graph; `client.track(event, context, details)` at the business-outcome site the roster names.
- Receipt: every read is a `Verdict` — reason, code, variant, and instant travel with the value, so audit and telemetry consume evaluation evidence with no second surface.

```typescript signature
import {
    type Client,
    type EvaluationContext,
    type EvaluationDetails,
    type Hook,
    type JsonValue,
    OpenFeature,
    OpenFeatureEventEmitter,
    type Provider,
    ProviderEvents,
    type ResolutionDetails,
    type TrackingEventDetails,
    type TrackingEventValue,
} from '@openfeature/server-sdk';
import { KeyValueStore } from '@effect/platform';
import {
    Cache, Cause, Data, Exit, Function, HashMap, Layer, Match, Metric, Option, Predicate, Record, Runtime, Schedule, Schema, Stream,
    SubscriptionRef,
} from 'effect';
import { Convention, Fault } from '@rasm/core';
import { FlagGate } from '@rasm/security';
import { Setting } from './config.ts';
import { Feed } from '../net/channel.ts';

const _shifted = Schema.decodeUnknown(Schema.parseJson(_Shift));
const _heldKey = Schema.encodeSync(Schema.parseJson(Schema.Tuple(Schema.String, Schema.String)));

// The tracking remainder is caller-keyed and admits instants, nested records, and arrays no attribute value accepts, so
// the fused codec renders it whole and `Schema.Date` is where an instant becomes its ISO spelling. Encode-only by
// construction: the string arm would swallow a rendered instant on the way back, and nothing here decodes.
interface _DetailEncoded {
    readonly [key: string]: boolean | null | number | string | ReadonlyArray<_DetailEncoded[string]> | _DetailEncoded;
}
const _Detail: Schema.Schema<TrackingEventValue, _DetailEncoded[string]> = Schema.Union(
    Schema.Boolean,
    Schema.Number,
    Schema.String,
    Schema.Null,
    Schema.Date,
    Schema.mutable(Schema.Array(Schema.suspend((): Schema.Schema<TrackingEventValue, _DetailEncoded[string]> => _Detail))),
    Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.suspend((): Schema.Schema<TrackingEventValue, _DetailEncoded[string]> => _Detail) })),
);
const _rendered = Schema.encodeSync(Schema.parseJson(Schema.Record({ key: Schema.String, value: _Detail })));

type _Probe = {
    readonly flag: string;
    readonly key: string;
    readonly axes: Readonly<Record<string, string>>;
    readonly kind: Verdict.Kind;
    readonly fallback: Verdict.Json;
};

const _isReason = Schema.is(Schema.Literal(...Rollout.reasons));
const _isCode = Schema.is(Schema.Literal(...Verdict.codes));

// This owner crosses the two reason dialects: OpenFeature spells resolution reasons uppercase, the semconv value
// family spells them lowercase, and the convention owner holds the spec vocabulary without either translation.
const _SPEC: { readonly [R in Rollout.Reason]: Convention.FlagReason } = {
    CACHED: Convention.value.flagCached,
    DEFAULT: Convention.value.flagDefault,
    DISABLED: Convention.value.flagDisabled,
    ERROR: Convention.value.flagError,
    SPLIT: Convention.value.flagSplit,
    STALE: Convention.value.flagStale,
    STATIC: Convention.value.flagStatic,
    TARGETING_MATCH: Convention.value.flagTargeting,
    UNKNOWN: Convention.value.flagUnknown,
};

// The return annotation closes the record: a key outside the convention vocabulary cannot ride the span. Every optional
// coordinate folds to an OMITTED key rather than an empty one, so a query never reads absence as a value.
const _attributed = (provider: string, event: string, context: EvaluationContext, details: TrackingEventDetails): Convention.Attributes => ({
    [Convention.incubating.flagProvider]: provider,
    [Convention.rasm.flagEvent]: event,
    ...Option.match(Option.fromNullable(context.targetingKey), {
        onNone: () => ({}),
        onSome: (subject) => ({ [Convention.incubating.flagContext]: subject }),
    }),
    ...Option.match(Option.liftPredicate(details.value, Predicate.isNumber), {
        onNone: () => ({}),
        onSome: (magnitude) => ({ [Convention.rasm.flagValue]: magnitude }),
    }),
    ...Option.match(Option.liftPredicate(Record.remove(details, 'value'), Predicate.not(Record.isEmptyRecord)), {
        onNone: () => ({}),
        onSome: (rest) => ({ [Convention.rasm.flagDetail]: _rendered(rest) }),
    }),
});

const _kindOf = (value: Verdict.Json): Verdict.Kind =>
    Predicate.isBoolean(value) ? 'boolean' : Predicate.isString(value) ? 'string' : Predicate.isNumber(value) ? 'number' : 'object';

const _getters = {
    boolean: (client: Client, probe: _Probe, context: EvaluationContext) =>
        client.getBooleanDetails(probe.flag, _guards.boolean(probe.fallback) ? probe.fallback : false, context),
    string: (client: Client, probe: _Probe, context: EvaluationContext) =>
        client.getStringDetails(probe.flag, _guards.string(probe.fallback) ? probe.fallback : '', context),
    number: (client: Client, probe: _Probe, context: EvaluationContext) =>
        client.getNumberDetails(probe.flag, _guards.number(probe.fallback) ? probe.fallback : 0, context),
    object: (client: Client, probe: _Probe, context: EvaluationContext) => client.getObjectDetails(probe.flag, probe.fallback, context),
} as const satisfies Record<Verdict.Kind, (client: Client, probe: _Probe, context: EvaluationContext) => Promise<EvaluationDetails<Verdict.Json>>>;

const _patched = (held: Ruleset, shift: Verdict.Shift): readonly [Ruleset, ReadonlyArray<string>] =>
    Match.valueTags(shift, {
        Set: ({ epoch, flag, def }) =>
            epoch >= held.epoch ? ([new Ruleset({ epoch, flags: HashMap.set(held.flags, flag, def) }), [flag]] as const) : ([held, []] as const),
        Clear: ({ epoch, flag }) =>
            epoch >= held.epoch ? ([new Ruleset({ epoch, flags: HashMap.remove(held.flags, flag) }), [flag]] as const) : ([held, []] as const),
        Reset: ({ ruleset }) =>
            ruleset.epoch >= held.epoch
                ? ([ruleset, Array.dedupe([...HashMap.keys(held.flags), ...HashMap.keys(ruleset.flags)])] as const)
                : ([held, []] as const),
    });

type _Value = {
    readonly boolean: boolean;
    readonly string: string;
    readonly number: number;
    readonly object: Exclude<Verdict.Json, boolean | number | string>;
};

const _guards: { readonly [K in Verdict.Kind]: (value: Verdict.Json) => value is _Value[K] } = {
    boolean: Predicate.isBoolean,
    string: Predicate.isString,
    number: Predicate.isNumber,
    object: (value): value is _Value['object'] => !(Predicate.isBoolean(value) || Predicate.isNumber(value) || Predicate.isString(value)),
};

const _resolved =
    (cell: SubscriptionRef.SubscriptionRef<Ruleset>, digest: (text: string) => number) =>
    <K extends Verdict.Kind>(kind: K, flag: string, fallback: _Value[K], context: EvaluationContext): Effect.Effect<ResolutionDetails<_Value[K]>> =>
        Effect.gen(function* () {
            const held = yield* SubscriptionRef.get(cell);
            const at = yield* DateTime.now;
            const key = context.targetingKey ?? '';
            const axes = Record.filterMap(context, (value) => (Predicate.isString(value) ? Option.some(value) : Option.none()));
            const bucket = (salt: string): number => digest(`${salt}:${key}`) % 100;
            return Option.match(HashMap.get(held.flags, flag), {
                onNone: (): ResolutionDetails<_Value[K]> => ({
                    value: fallback,
                    reason: 'ERROR',
                    errorCode: held.epoch === 0 ? 'PROVIDER_NOT_READY' : 'FLAG_NOT_FOUND',
                }),
                onSome: (def): ResolutionDetails<_Value[K]> => {
                    if (context.targetingKey === undefined && Rollout.targeted(def.rule)) {
                        return { value: fallback, reason: 'ERROR', errorCode: 'TARGETING_KEY_MISSING' };
                    }
                    const outcome = Rollout.decide(def.rule, { subject: { key, axes }, at, bucket });
                    const variant = Option.getOrElse(outcome.variant, () => (outcome.on ? def.fallback : ''));
                    const value = Option.fromNullable(def.variants[variant]).pipe(
                        Option.orElse(() => (def.kind === 'boolean' ? Option.some<Verdict.Json>(outcome.on) : Option.none())),
                    );
                    return Option.match(value, {
                        onNone: () => ({ value: fallback, reason: 'ERROR', errorCode: 'PARSE_ERROR' }),
                        onSome: (resolved) =>
                            def.kind === kind && _guards[kind](resolved)
                                ? { value: resolved, reason: outcome.reason, variant } // the mapped guard narrowed resolved to _Value[K]: the correlation is checker-proven
                                : { value: fallback, reason: 'ERROR', errorCode: 'TYPE_MISMATCH' },
                    });
                },
            });
        });

class Flags extends Effect.Service<Flags>()('runtime/Flags', {
    // The tracking counter is a WORD-counting frequency row, so its mount takes the roster's own published census —
    // the ordered vocabulary its owner minted, where a duplicate word already refused at the mint — never a bare
    // tuple a caller assembled and no owner keeps aligned.
    scoped: <const Outcomes extends Convention.Roster>(
        digest: (text: string) => number,
        mode: Sticky.Mode,
        outcomes: Convention.Census<Outcomes>,
    ) =>
        Effect.gen(function* () {
            const setting = yield* Setting;
            const feed = yield* Feed;
            const ledger = (yield* KeyValueStore).forSchema(Sticky.Held);
            const cell = yield* SubscriptionRef.make(new Ruleset({ epoch: 0, flags: HashMap.empty() }));
            const events = new OpenFeatureEventEmitter();
            const metadata = { name: 'rasm' } as const;
            const tally = Convention.mount(Convention.metric.flagTracked, outcomes);
            const runtime = yield* Effect.runtime<never>();
            const resolve = _resolved(cell, digest);
            const pace = Schedule.spaced(setting.flag.cadence);
            const sticky = { lease: setting.flag.sticky, quarantine: setting.flag.quarantine } as const;
            const posture = Sticky.modes[mode];

            const announce = (event: ProviderEvents, detail: Record<string, unknown>): Effect.Effect<void> =>
                Effect.sync(() => events.emit(event, detail));

            yield* feed
                .open(setting.flag.origin)
                .pipe(
                    Stream.runForEach((event) =>
                        _shifted(event.data).pipe(
                            Effect.flatMap((shift) =>
                                SubscriptionRef.modify(cell, (held) => {
                                    const [next, changed] = _patched(held, shift);
                                    return [[shift, changed] as const, next] as const;
                                }),
                            ),
                            Effect.tap(([shift, changed]) =>
                                Effect.zipRight(
                                    announce(ProviderEvents.ConfigurationChanged, { flagsChanged: [...changed] }),
                                    // an accepted Reset is the plane's own recovery edge: the document is whole again,
                                    // so the SDK's readiness signal closes the Stale it opened
                                    shift._tag === 'Reset' ? announce(ProviderEvents.Ready, {}) : Effect.void,
                                ),
                            ),
                            // a malformed patch is one skipped row, and the SDK hears it: consumers see a provider in
                            // error rather than a cell that silently stops advancing
                            Effect.tapErrorCause((cause) => announce(ProviderEvents.Error, { message: Cause.pretty(cause) })),
                            Effect.ignoreLogged,
                        ),
                    ),
                    // the feed DIED: the cell freezes at its last epoch and every later read is CACHED/STATIC off a
                    // document nobody refreshes, so the SDK's own staleness signal fires before the reconnect budget
                    Effect.tapErrorCause((cause) => announce(ProviderEvents.Stale, { message: Cause.pretty(cause) })),
                    // the reconnect burst rides the core `feed` budget — jittered exponential, attempt-bounded,
                    // elapsed-ceilinged, reset after quiet — then phases into the steady cadence, so a transient blip
                    // backs off against a fleet-decorrelated curve while a long outage keeps re-opening forever;
                    // a bare `spaced` pace re-derives the compile and synchronizes every replica onto one instant
                    Effect.retry(Schedule.andThen(Fault.Budget.schedule('feed', Function.constTrue), pace)),
                    Effect.forkScoped,
                );

            const provider: Provider = {
                runsOn: 'server',
                metadata,
                events,
                resolveBooleanEvaluation: (flag, fallback, context) => Runtime.runPromise(runtime)(resolve('boolean', flag, fallback, context)),
                resolveStringEvaluation: (flag, fallback, context) => Runtime.runPromise(runtime)(resolve('string', flag, fallback, context)),
                resolveNumberEvaluation: (flag, fallback, context) => Runtime.runPromise(runtime)(resolve('number', flag, fallback, context)),
                resolveObjectEvaluation: <T extends JsonValue>(flag: string, fallback: T, context: EvaluationContext) =>
                    // BOUNDARY ADAPTER: the SDK's object generic promises the caller's T with no runtime witness; the guard proved the JSON-object shape, this pin crosses the SDK's own unsound seam
                    Runtime.runPromise(runtime)(resolve('object', flag, fallback as _Value['object'], context)) as Promise<ResolutionDetails<T>>,
                // the SDK's one synchronous member: it answers void, so the bridge runs sync against a total effect
                // rather than stranding a promise off the caller's stack, and the client's own try swallows a throw
                // from here into a debug log — a failure the seat could raise would vanish as silent evidence loss
                track: (event, context, details) =>
                    Runtime.runSync(runtime)(
                        Effect.zipRight(Effect.annotateCurrentSpan(_attributed(metadata.name, event, context, details)), Metric.update(tally, event)),
                    ),
            };

            const traced: Hook = {
                after: (hooked, details) =>
                    Runtime.runPromise(runtime)(
                        Effect.annotateCurrentSpan({
                            // Convention.Attributes closes the record: a key outside the vocabulary cannot ride the span
                            [Convention.incubating.flagKey]: hooked.flagKey,
                            [Convention.incubating.flagProvider]: metadata.name,
                            [Convention.incubating.flagReason]: _SPEC[_isReason(details.reason) ? details.reason : 'UNKNOWN'],
                            // the join a tracked outcome lands against: the identity pairs the two planes and the
                            // variant names the arm the outcome is attributed to
                            ...Option.match(Option.fromNullable(hooked.context.targetingKey), {
                                onNone: () => ({}),
                                onSome: (subject) => ({ [Convention.incubating.flagContext]: subject }),
                            }),
                            ...Option.match(Option.fromNullable(details.variant), {
                                onNone: () => ({}),
                                onSome: (variant) => ({ [Convention.incubating.flagVariant]: variant }),
                            }),
                        } satisfies Convention.Attributes),
                    ),
            };

            yield* Effect.acquireRelease(Effect.orDie(Effect.tryPromise(() => OpenFeature.setProviderAndWait(provider))), () =>
                Effect.orDie(Effect.tryPromise(() => OpenFeature.close())),
            );
            const client = OpenFeature.getClient();
            client.addHooks(traced);

            const fetched = (probe: _Probe): Effect.Effect<EvaluationDetails<Verdict.Json>> =>
                Effect.tryPromise({
                    try: () => _getters[probe.kind](client, probe, { targetingKey: probe.key, ...probe.axes }),
                    catch: () => ({ flagKey: probe.flag, value: probe.fallback, reason: 'ERROR', errorCode: 'GENERAL' }) as const,
                }).pipe(Effect.merge);
            const lookup = (probe: _Probe): Effect.Effect<Verdict> =>
                Effect.gen(function* () {
                    const at = yield* DateTime.now;
                    const key = _heldKey([probe.flag, probe.key]);
                    const held = yield* posture.ledger ? ledger.get(key).pipe(Effect.orElseSucceed(() => Option.none())) : Effect.succeedNone;
                    const document = yield* SubscriptionRef.get(cell);
                    const recalled = Option.flatMap(held, (entry) =>
                        Option.flatMap(Sticky.recalled(entry, document.epoch, at, setting.flag.sticky), (variant) =>
                            Option.flatMap(HashMap.get(document.flags, probe.flag), (def) =>
                                Option.flatMap(Option.fromNullable(def.variants[variant]), (value) =>
                                    def.kind === probe.kind && _guards[probe.kind](value)
                                        ? Option.some(
                                              new Verdict({
                                                  flag: probe.flag,
                                                  kind: probe.kind,
                                                  value,
                                                  variant: Option.some(variant),
                                                  reason: 'CACHED',
                                                  code: Option.none(),
                                                  errorMessage: Option.none(),
                                                  flagMetadata: {},
                                                  at,
                                              }),
                                          )
                                        : Option.none(),
                                ),
                            ),
                        ),
                    );
                    if (Option.isSome(recalled)) return recalled.value;
                    const details = yield* fetched(probe);
                    const verdict = new Verdict({
                        flag: probe.flag,
                        kind: probe.kind,
                        value: details.value,
                        variant: Option.fromNullable(details.variant),
                        reason: _isReason(details.reason) ? details.reason : 'UNKNOWN',
                        code: Option.filter(Option.fromNullable(details.errorCode), _isCode),
                        errorMessage: Option.fromNullable(details.errorMessage),
                        flagMetadata: details.flagMetadata ?? {},
                        at,
                    });
                    return yield* Effect.succeed(verdict).pipe(
                        Effect.tap((settled) =>
                            posture.ledger
                                ? Option.match(settled.variant, {
                                      onNone: () => Effect.void,
                                      onSome: (variant) =>
                                          ledger
                                              .set(
                                                  key,
                                                  new Held({
                                                      flag: probe.flag,
                                                      variant,
                                                      epoch: document.epoch,
                                                      at,
                                                  }),
                                              )
                                              .pipe(Effect.ignore),
                                  })
                                : Effect.void,
                        ),
                    );
                });
            const memo = yield* Cache.makeWith({
                capacity: setting.flag.memo,
                lookup,
                timeToLive: (exit) =>
                    Exit.match(exit, {
                        onFailure: () => Sticky.expiry('ERROR', sticky),
                        onSuccess: (verdict) => Sticky.expiry(verdict.reason, sticky),
                    }),
            });
            client.addHandler(ProviderEvents.ConfigurationChanged, () => void Runtime.runPromise(runtime)(memo.invalidateAll));

            return {
                evaluate: (flag: string, subject: Rollout.Subject, fallback: Verdict.Json = false): Effect.Effect<Verdict> =>
                    pipe(
                        Data.struct({
                            flag,
                            key: subject.key,
                            axes: Data.struct(subject.axes),
                            kind: _kindOf(fallback),
                            fallback,
                        }),
                        (probe) => (posture.memo ? memo.get(probe) : lookup(probe)),
                    ),
                changes: cell.changes,
            };
        }),
}) {
    static readonly gate: Layer.Layer<FlagGate, never, Flags> = Layer.effect(
        FlagGate,
        Effect.map(Flags, (flags) => ({
            enabled: (key, claims) =>
                Effect.map(
                    flags.evaluate(key, {
                        key: claims.subject,
                        axes: Option.match(claims.tenant, {
                            onNone: () => ({}),
                            onSome: (tenant) => ({ tenant }),
                        }),
                    }),
                    (verdict) => verdict.value === true,
                ),
        })),
    );
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Flags, Rollout, Sticky, Verdict };
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
