# [RUNTIME_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress dials through one entry applying its lane's status admission, transient retry, redirect ceiling, total budget, circuit admission, machine-credential presentation, and W3C trace propagation as composed transformers over the runtime row's shared `HttpClient`. Per-folder clients, bare `fetch`, call-site retry loops, a hand breaker beside the ledger, a hand-carried static token at the call site, and a second timeout convention are the named defects. Module `runtime/src/net/client.ts`.

Each lane is a policy row whose durations are the core budget ledger's: a row names its `Fault.Budget` kind and its total budget is that row's `total`, so no per-lane duration literal exists. This circuit ledger is the branch's one breaker owner — a keyed closed→open→half-open cell folded purely, applied as a guard transformer, and exported so fanout publish and delivery transmit inherit one admission law. Residency pins at the root as two pools — undici dispatcher and Connect `node:http2` session — reading one ceiling, so policy stays composed transformers.

## [01]-[INDEX]

- [02]-[LANE_ROWS]: closes the egress policy table — ledger binding, pulse compile, hops, circuit row, credential posture; `Client`.
- [03]-[BREAK_STATE]: folds the keyed circuit ledger — pure admission and settle folds, the guard transformer; `Breaker`, `Lapse`.
- [04]-[DIAL_SEAM]: seats the one entry — budget geometry, the credential projection, and the consumer law; `Client`, `Machine`.
- [05]-[DISPATCH_ROWS]: pins undici residency beneath the node row's client; root data.
- [06]-[CONNECT_ROW]: rosters the Connect wire dialects — session residency, codec coordinates, lane inheritance; `Rpc`.

## [02]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the interior `_lanes` anchor — `live` (interactive calls), `batch` (bulk and export egress), `feed` (long-lived streaming responses) — each row carrying `kind` (the `core/value/fault#RETRY_BUDGET` ledger row the lane's durations read), `budget` (`Option<Duration>` — the ledger row's `total` on the settled lanes, stated absence on `feed` because the connection outlives any deadline), `hops` (the redirect ceiling, zero on `feed`), and `break` (`Option<Breaker.Policy>` — the circuit row the guard reads; stated absence on `feed` because the reconnect pulse already paces re-dials).
- Law: the row guard closes the member set and the table grows by evidence — `_Rows` proves every lane carries the full policy complement, the anchor itself is the lane set, and a genuinely new egress contract (a webhook lane, a hedged lane) is one row and zero new surface.
- Law: every lane states `fits`, `admit`, `present`, `lifetime`, and `degrade` as cells a root reads before it selects — the `feed` row's stated absences are its forfeit, not an omission, so a caller choosing it accepts no total budget and no circuit knowingly.
- Law: `present` names WHEN the lane reads the credential source, because that is the axis the lanes genuinely differ on — a settled lane re-reads on every dial, so a rotation lands on the next call for free, while `feed` reads once at the dial that opens a response outliving it, so the credential ages inside the stream and only a re-dial re-presents. The projection itself is one stamp at `[04]`; this cell states the consequence a selector accepts, and `feed`'s `degrade` carries the forfeit beside its budget and circuit ones.
- Law: `admit` names the dial modality this lane is entered through, because the two are not interchangeable per lane — the settled lanes admit `dial(lane, request, shape)`, which materializes and decodes inside the one total budget, while `feed` admits `dial(lane, request)` alone, since a shape materializes a body a streaming response never ends.
- Law: this table decides NO tenancy — an egress lane carries policy over a shared client and isolates nothing, so it states no tenancy cell at all; per-tenant isolation is a circuit key suffix at `[03]`, and the closed axis stays `proc/config#ADMISSION_ROWS` `Profile`'s.
- Boundary: proxy is transport residency, not per-call policy — the lane table carries no proxy knob, the browser lane has none by construction, and the dispatcher rows in `[5]` own residency.
- Packages: `effect` (`Duration`, `Function`, `Option`), `@rasm/ts/core` (`Fault.Budget`).

## [03]-[BREAK_STATE]

[BREAK_STATE]:
- Owner: `Breaker` — the one circuit owner of the branch. Each circuit is a keyed cell holding one closed `Data.taggedEnum` state family — `Closed` carrying its generation and fault count, `Open` its generation and reopen instant, `Half` its generation and probe ration, so an invalid field combination is unconstructible — whose transitions are two total `$match` folds: `_admitted` returns the admitted generation and advances `Open→Half` when the cool window lapses, `_settled` accepts only a termination from that generation and applies the `_verdicts` row it names. `Breaker.guard(key, policy)` is the transformer any egress effect composes; the dial keys it by lane and request origin — path and query variants share one circuit because they share the remote's fate — and fanout publish and delivery transmit compose the same guard under their own identities.
- Law: termination is a three-row verdict, never a settled-channel pair — `Effect.onExit` fires once after the outcome settles and `_verdict` folds the `Exit` interrupt-first through `Cause.isInterruptedOnly`, so `passed` advances a fresh closed generation, `faulted` (a typed fault or a defect alike) charges the trip count or reopens off a half-open probe, and `halted` RESTORES the probe `_admitted` consumed. Observing the two settled channels alone strands every interrupted probe: `_gated` composes `Effect.timeoutFail` inside the guard while a caller race, a scope close, an abandoned `ExecutionPlan` step, and an early `Stream` consumer exit all pass through unsettled, so a `probes: 1` lane wedges at `Half { probes: 0 }` — a cell `_admitted` refuses forever, since `Half` carries no `until` escape.
- Law: state rides `Ref.modify` — admission and settlement are atomic pure folds over the cell, so concurrent dials race on the ledger, never on a lock, and the machine is replayable from its fold functions alone; a new circuit state is one case row breaking every `$match` record loudly, and a new termination class is one `_verdicts` row the `Verdict` union derives.
- Law: rejection is `Lapse` evidence — `reason: "break"`, class `unavailable`, the policy's `cool` as the spent span — so an open circuit routes through the same budget gate as every transient and no second shed fault exists.
- Law: the registry is a `Context.Reference` — cells key by guard key in one `MutableHashMap` default bounded by the capacity row: a mint at capacity flushes the ledger to rest, so degradation is a cold circuit, never process-lifetime memory growth; the requirement channel stays clean (`R` never grows), and a root or proof overrides the whole ledger by providing the Reference. Exemption: `_cell` is the one statement kernel — the synchronous mint-or-get, with the capacity flush, against the registry map.
- Growth: hedging and load-shed stay owned elsewhere (`Effect.raceAll` at the caller, `Gate.shed` at the serving edge); a per-tenant circuit is a key suffix, zero new surface.
- Packages: `effect` (`Cause`, `Clock`, `Context`, `Data`, `Duration`, `Exit`, `MutableHashMap`, `Option`, `Ref`, `Schema`), `@rasm/ts/core` (`Fault.Class`).

```typescript signature
// Every row names the lane, and each names the ONE span or number that decided it — the deadline that lapsed, the
// cool window still open, the peer's own refusal status. The sender-binding row names none, because a proof that
// cannot be minted spends no span: the `Duration.zero` it used to carry was a slot the raise filled with a lie.
const _family = Fault.Class.family(['budget', 'break', 'binding', 'throttled'] as const, {
    budget: Fault.Class.row({
        class: 'expired',
        leg: 'dial',
        detail: Schema.Struct({ lane: Schema.String, budget: Schema.DurationFromSelf }),
        render: ({ lane, budget }) => `${lane} outlived its ${Duration.toMillis(budget)}ms budget`,
    }),
    break: Fault.Class.row({
        class: 'unavailable',
        leg: 'break',
        detail: Schema.Struct({ lane: Schema.String, cool: Schema.DurationFromSelf }),
        render: ({ lane, cool }) => `${lane} circuit is open for another ${Duration.toMillis(cool)}ms`,
    }),
    binding: Fault.Class.row({
        class: 'denied',
        leg: 'dial',
        detail: Schema.Struct({ lane: Schema.String }),
        render: ({ lane }) => `${lane} cannot present a sender-constrained credential over a bare authorization header`,
    }),
    // The one row whose PRODUCER states the window: a peer answering `Retry-After` measured the wait itself, so this
    // reason classes `exhausted` — the single `throttled`-capable kind — and its raise fills `after`.
    throttled: Fault.Class.row({
        class: 'exhausted',
        leg: 'dial',
        detail: Schema.Struct({ lane: Schema.String, status: Schema.Int }),
        render: ({ lane, status }) => `${lane} peer answered ${status} and stated its own re-offer window`,
    }),
});

class Lapse extends Schema.TaggedError<Lapse>()('Lapse', {
    case: _family.payload,
    // The stated window rides the VALUE under the one word `core/value/fault#CLASS_VOCABULARY` fixes, so
    // `Fault.Class.statedOf` reads exactly this field and `Fault.Budget.schedule` re-seats its base from it; every
    // row but `throttled` states absence rather than a zero a gate would re-offer against immediately.
    after: Fault.Class.After,
}) {
    get class(): Fault.Class.Kind {
        return _family.classOf(this.case.reason);
    }
    override get message(): string {
        return _family.render(this.case);
    }
}

declare namespace Breaker {
    type Cell = Data.TaggedEnum<{
        Closed: { readonly generation: number; readonly faults: number };
        Open: { readonly generation: number; readonly until: number };
        Half: { readonly generation: number; readonly probes: number };
    }>;
    type Policy = { readonly trip: number; readonly cool: Duration.Duration; readonly probes: number };
    type Verdict = keyof typeof _verdicts;
    type Settle = (held: Cell, generation: number, now: number, policy: Policy) => Cell;
}

const _Cell = Data.taggedEnum<Breaker.Cell>();
const _REST: Breaker.Cell = _Cell.Closed({ generation: 0, faults: 0 });

class Breakers extends Context.Reference<Breakers>()('runtime/Breakers', {
    defaultValue: () => MutableHashMap.empty<string, Ref.Ref<Breaker.Cell>>(),
}) {}

const _admitted = (held: Breaker.Cell, now: number, policy: Breaker.Policy): readonly [Option.Option<number>, Breaker.Cell] =>
    _Cell.$match(held, {
        Closed: (closed): readonly [Option.Option<number>, Breaker.Cell] => [Option.some(closed.generation), closed],
        Open: (open): readonly [Option.Option<number>, Breaker.Cell] =>
            now >= open.until
                ? [Option.some(open.generation), _Cell.Half({ generation: open.generation, probes: policy.probes - 1 })]
                : [Option.none(), open],
        Half: (half): readonly [Option.Option<number>, Breaker.Cell] =>
            half.probes > 0
                ? [Option.some(half.generation), _Cell.Half({ generation: half.generation, probes: half.probes - 1 })]
                : [Option.none(), half],
    });

const _reopened = (generation: number, now: number, policy: Breaker.Policy): Breaker.Cell =>
    _Cell.Open({ generation: generation + 1, until: now + Duration.toMillis(policy.cool) });

const _verdicts = {
    // one row per termination class: a fourth class is a row, and the Verdict union derives off this anchor
    passed: (_held, generation) => _Cell.Closed({ generation: generation + 1, faults: 0 }),
    faulted: (held, generation, now, policy) =>
        _Cell.$match(held, {
            Closed: ({ faults }) =>
                faults + 1 >= policy.trip ? _reopened(generation, now, policy) : _Cell.Closed({ generation, faults: faults + 1 }),
            Open: (open) => open, // a late loser settles against an already-open cell: the cell holds
            Half: () => _reopened(generation, now, policy),
        }),
    halted: (held) =>
        _Cell.$match(held, {
            Closed: (closed) => closed, // a closed admission consumed no ration, so an abandoned call charges nothing
            Open: (open) => open,
            Half: (half) => _Cell.Half({ generation: half.generation, probes: half.probes + 1 }), // the abandoned probe returns to the ration: an interrupt is not evidence about the remote
        }),
} as const satisfies Record<string, Breaker.Settle>;

const _verdict: (exit: Exit.Exit<unknown, unknown>) => Breaker.Verdict = Exit.match({
    onFailure: (cause) => (Cause.isInterruptedOnly(cause) ? 'halted' : 'faulted'), // interrupt-first: a defect is evidence about the remote, an abandoned wait is not
    onSuccess: () => 'passed',
});

const _settled = (held: Breaker.Cell, generation: number, now: number, policy: Breaker.Policy, verdict: Breaker.Verdict): Breaker.Cell =>
    held.generation === generation ? _verdicts[verdict](held, generation, now, policy) : held;

const _LEDGER = { capacity: 512 } as const;

const _cell = (cells: MutableHashMap.MutableHashMap<string, Ref.Ref<Breaker.Cell>>, key: string): Ref.Ref<Breaker.Cell> =>
    Option.getOrElse(MutableHashMap.get(cells, key), () => {
        if (MutableHashMap.size(cells) >= _LEDGER.capacity) MutableHashMap.clear(cells); // a mint at capacity flushes the ledger to rest: cold circuits, never unbounded growth
        const minted = Ref.unsafeMake(_REST);
        MutableHashMap.set(cells, key, minted);
        return minted;
    });

const _guard =
    (key: string, policy: Breaker.Policy) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | Lapse, R> =>
        Effect.gen(function* () {
            const cell = _cell(yield* Breakers, key);
            const now = yield* Clock.currentTimeMillis;
            const admitted = yield* Ref.modify(cell, (held) => _admitted(held, now, policy));
            return Option.isSome(admitted)
                ? yield* Effect.onExit(self, (exit) =>
                      Effect.flatMap(Clock.currentTimeMillis, (at) =>
                          Ref.update(cell, (held) => _settled(held, admitted.value, at, policy, _verdict(exit))),
                      ),
                  ) // one emission point, fired uninterruptibly after the outcome settles: an interrupted probe never escapes unaccounted
                : yield* new Lapse({ case: { reason: 'break', lane: key, cool: policy.cool }, after: Option.none() });
        });

const Breaker = { guard: _guard } as const;
```

## [04]-[DIAL_SEAM]

[DIAL_SEAM]:
- Owner: `Client.dial` — the one entry. Modality follows the call shape: `dial(lane, request)` yields the scoped `HttpClientResponse` (the caller owns the body's lifetime — the `feed` posture); `dial(lane, request, shape)` fuses execution, status admission, bounded body materialization through `HttpIncomingMessage.withMaxBodySize`, JSON-body decode through `HttpClientResponse.schemaBodyJson`, and scope closure into one self-contained step; both apply the lane's transformers — `HttpClient.filterStatusOk`, `HttpClient.followRedirects`, `HttpClient.retryTransient({ schedule })`, `HttpClient.withTracerPropagation(true)` — over the client yielded from the requirement channel.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above transient retry and, on the settled modality, above body drain and Schema decode, so retries and a slow body spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one composes the ledger row's `attempt` duration as its own `Effect.fn` pipeline step under the rails layering law.
- Law: expiry, shed, throttling, and credential refusal are one typed family — each `Lapse` row names the lane beside the one span or number that decided it, splitting `budget` (class `expired`, the deadline that lapsed) from `break` (class `unavailable`, the cool window still open) from `binding` (class `denied`, naming no span at all) from `throttled` (class `exhausted`, the peer's own refusal status), so the core budget gate re-drives the transient rows while a sender-binding refusal routes as the terminal evidence it is; transport and status faults ride the platform's own `HttpClientError` family untouched, and decode skew rides `ParseError` — three families, each already routable, none re-wrapped.
- Law: a peer that STATES its re-offer window is honored under that window and never under a curve this lane invented — `Retry-After` reads off the refusal as delay-seconds or an HTTP-date, rides the raised value as `Fault.Class.After`, and spends `Fault.Budget.schedule`'s `stated` slot; the blind transient policy gates such a refusal OUT, so one refusal is re-offered once under one wait rather than twice under two policies. A header stating nothing parsable leaves the platform's own refusal untouched.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `setHeader`, `setUrlParams` compose at the consumer's seam; the dial owns policy, never request vocabulary. `bearerToken` is the one member this seam takes BACK, because a workload credential is lane policy and a call-site token is the hand-carried secret the projection deletes.
- Law: the credential is a projection off one port, never a call-site header — `Machine` is a `Context.Reference` holding a source the app root fills from `security:authn/workload`'s `Workload`, so this plane mounts a resolved `MachinePrincipal` and holds no grant grammar, no client secret, and no rotation timer; the requirement channel stays clean exactly as the breaker ledger's does, and an estate binding nothing keeps the default source, which answers no principal and dials anonymous — today's posture, unchanged, rather than a lane that refuses until someone remembers a Layer.
- Law: the audience is the request ORIGIN and the source decides per audience — a fleet token minted for one service is not a bearer instrument for whatever host a caller names, so `Machine.at` asks for the origin the request already addresses and a source holding nothing for it presents nothing; scoping the read by lane instead would hand service A's credential to service B on the same lane, which is credential exfiltration wearing a policy table.
- Law: a sender-constrained principal REFUSES here rather than downgrading — RFC 9449 binds the token to a proof over this call's own method and URL, the certified client's `DPoPHandle` publishes `calculateThumbprint()` and nothing that mints a proof, so no header this plane could write carries the binding; presenting a `dpop`-scheme token under a bare `authorization` strips exactly what the grant paid for, and the `binding` refusal routes the caller to `Workload.call`, which owns the proved read. A bare `Bearer ` prefix is the same defect spelled shorter, which is why `HttpClientRequest.bearerToken` — hardcoding that prefix and spelling no other scheme — is foreclosed and `MachinePrincipal.credential`, carrying the scheme its ISSUER chose, is what lands on the header.
- Law: refresh rides the grant lifecycle, never a lane timer — the source is read per dial, so whatever principal `Workload.rotate` last landed is what the next call stamps and no lane holds a cached credential with an expiry of its own; a lane-side cache re-derives `MachinePrincipal.lapsed` and then drifts from it, and a stale token's `401` is the authority's own answer rather than this plane's guess.
- Law: the stamped header never leaks and this plane adds no scrub — `authorization` is already in the platform's default `Headers.currentRedactedNames`, so every `Headers` inspection, log line, and span attribute renders it `Redacted`; the value stays `Redacted` from the security plane to the one `Redacted.value` at the stamp, which is the exact point the wire demands a string.
- Boundary: the client binding is the runtime row's (`proc/exec#RUNTIME_ROWS`); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork. Principal mint, rotation, introspection, and revocation are `security:authn/workload`'s; this owner mounts the projection and decides nothing about the grant.
- Entry: `Client.dial(lane, request[, shape])`; `R` carries `HttpClient` (and `Scope` on the response modality) to the root, and the app root overrides `Machine` where a workload identity exists.
- Receipt: the overload annotations are the whole seam contract — fault union and requirement set readable without opening the body.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientError`, `HttpClientRequest`, `HttpClientResponse`, `Headers.get`), `effect` (`Data`, `DateTime`, `Duration`, `Effect`, `Either`, `Function`, `Number`, `Option`, `Redacted`, `Schema`), `@rasm/ts/core` (`Fault.Budget`, `Fault.Class`), `@rasm/ts/security` (`MachinePrincipal`).

```typescript signature
import { Headers, HttpClient, HttpClientError, HttpClientRequest, HttpClientResponse, HttpIncomingMessage } from '@effect/platform';
import {
    Cause,
    Clock,
    Context,
    Data,
    DateTime,
    Duration,
    Effect,
    Either,
    Exit,
    Function,
    MutableHashMap,
    Number,
    Option,
    type ParseResult,
    Redacted,
    Ref,
    Schema,
    type Scope,
    pipe,
} from 'effect';
import { Fault } from '@rasm/ts/core';
import type { MachinePrincipal } from '@rasm/ts/security';

// `fits` is the sentence a caller selects a lane on, `admit` the dial modality that lane is entered through,
// `present` when the credential source is read, and `degrade` what the selection costs, so all four leave prose and
// become cells a root reads. `feed` is the row that forfeits most and says so: no total budget, no circuit, and one
// credential read for a response that outlives it, because a long-lived response outlives any deadline and its
// reconnect pulse already paces re-dials, and its modality is the scoped response for the same reason.
const _lanes = {
    live: {
        kind: 'pulse',
        budget: Option.some(Fault.Budget.at("pulse").total),
        body: 8_388_608,
        hops: 2,
        break: Option.some({ trip: 8, cool: Duration.seconds(30), probes: 1 }),
        fits: '<interactive:a caller waits on this answer>',
        admit: '<settled-modality:dial(lane,request,shape) materializes and decodes inside the one total budget>',
        present: '<per-call:each dial re-reads the source for this origin,so a rotation lands on the next call>',
        lifetime: '<total-budget>',
        degrade: '<none>',
    },
    batch: {
        kind: 'bulk',
        budget: Option.some(Fault.Budget.at("bulk").total),
        body: 33_554_432,
        hops: 2,
        break: Option.some({ trip: 16, cool: Duration.seconds(45), probes: 2 }),
        fits: '<bulk-and-export:no interactive waiter,wider body and trip ceilings>',
        admit: '<settled-modality:dial(lane,request,shape) under the wider body ceiling this row carries>',
        present: '<per-call:each dial re-reads the source for this origin,so a rotation lands on the next call>',
        lifetime: '<total-budget>',
        degrade: '<slower-trip:sixteen faults before shed, so a sick remote is charged longer>',
    },
    feed: {
        kind: 'feed',
        budget: Option.none<Duration.Duration>(),
        body: 1_048_576,
        hops: 0,
        break: Option.none<Breaker.Policy>(),
        fits: '<long-lived-streaming-response:the connection is the unit of work>',
        admit: '<response-modality:dial(lane,request) alone,a shape materializes a body this lane never ends>',
        present: '<at-dial:one read opens a response that outlives it,so only a re-dial re-presents>',
        lifetime: '<consumer-scope:the caller ends it, never a deadline>',
        degrade: '<no-total-budget,no-circuit,no-redirect:a wedged remote sheds only when the consumer closes its scope;the presented credential ages inside the open response and the remote decides when it stops honouring it>',
    },
} as const;

declare namespace Client {
    type Lane = keyof typeof _lanes;
    type Row = {
        readonly kind: Fault.Budget.Kind;
        readonly budget: Option.Option<Duration.Duration>;
        readonly body: number;
        readonly hops: number;
        readonly break: Option.Option<Breaker.Policy>;
        readonly fits: string;
        readonly admit: string;
        readonly present: string;
        readonly lifetime: string;
        readonly degrade: string;
    };
    type _Rows<T extends Record<Lane, Row> = typeof _lanes> = T;
}

declare namespace Machine {
    type Source = { readonly held: (audience: string) => Effect.Effect<Option.Option<MachinePrincipal>> };
}

const _origin = (request: HttpClientRequest.HttpClientRequest): string => URL.parse(request.url)?.origin ?? request.url;

const _route = (lane: Client.Lane, request: HttpClientRequest.HttpClientRequest): string =>
    `${lane}:${_origin(request)}`; // one circuit per lane and origin: path and query variants share the cell they share fate with

// The gated read every wire projects off — the whole principal, because each wire needs a different member of it:
// the HTTP band takes `credential`, a SASL frame takes the bare `token` beside `clientId` and `expiresAt`. The ONE
// decision made here is the sender-binding refusal, since `DPoPHandle` publishes `calculateThumbprint()` alone and
// no proof exists for any of these wires to carry.
const _at = (lane: string, audience: string): Effect.Effect<Option.Option<MachinePrincipal>, Lapse> =>
    Effect.flatMap(Machine, (source) =>
        Effect.flatMap(
            source.held(audience),
            Option.match({
                onNone: () => Effect.succeedNone,
                onSome: (principal: MachinePrincipal) =>
                    principal.scheme === 'dpop'
                        ? Effect.fail(new Lapse({ case: { reason: 'binding', lane }, after: Option.none() }))
                        : Effect.succeed(Option.some(principal)),
            }),
        ));

// `credential` already carries the scheme its issuer chose, which is why `bearerToken` — hardcoding `Bearer ` — is
// the foreclosed member; `authorization` sits in the platform's default redacted-name set, so the one unwrap here is
// the last place the value is a bare string.
const _band = (lane: string, audience: string): Effect.Effect<Readonly<Record<string, string>>, Lapse> =>
    Effect.map(
        _at(lane, audience),
        Option.match({
            onNone: () => ({}),
            onSome: (principal: MachinePrincipal) => ({ authorization: Redacted.value(principal.credential) }),
        }),
    );

// The credential SOURCE, never a credential: an app root fills this with `security:authn/workload`'s `Workload`, so
// grant grammar, rotation, and client secrets all stay at their owner and this plane holds a read keyed by audience.
// A Reference rather than a Tag because the default has to be a real posture: an estate with no workload identity
// dials anonymous exactly as it does today, and `R` never grows on a lane that presents nothing. The two statics
// seat AFTER their functions because a class body evaluates its statics eagerly.
class Machine extends Context.Reference<Machine>()('runtime/Machine', {
    defaultValue: (): Machine.Source => ({ held: () => Effect.succeedNone }),
}) {
    static readonly at = _at;
    static readonly band = _band;
}

// A peer that STATED its window is not spent on the blind curve: the gate reads the refusal's own `Retry-After`
// presence off the response the platform's refusal already carries, so a throttling answer leaves the transient
// policy untouched and re-offers at the wait its producer measured. Everything else — a torn transport, an ungraded
// 5xx — is exactly what the blind curve exists for.
const _blind = (fault: unknown): boolean =>
    !(fault instanceof HttpClientError.ResponseError && Option.isSome(Headers.get(fault.response.headers, 'retry-after')));

const _tempered =
    (lane: Client.Lane) =>
    (client: HttpClient.HttpClient): HttpClient.HttpClient =>
        client.pipe(
            HttpClient.filterStatusOk,
            HttpClient.followRedirects(_lanes[lane].hops),
            HttpClient.retryTransient({ schedule: Fault.Budget.schedule(_lanes[lane].kind, _blind) }),
            HttpClient.withTracerPropagation(true),
        );

// `Retry-After` arrives as delay-seconds or as an HTTP-date and both spell ONE `Duration`. The date form measures
// against the reading clock through the SIGNED distance, so a deadline already elapsed answers absence rather than
// the absolute magnitude a bare distance returns; an unparsable header states nothing rather than collapsing to a
// zero the gate would re-offer against immediately.
const _stated = (response: HttpClientResponse.HttpClientResponse): Effect.Effect<Fault.Class.Stated> =>
    Option.match(Headers.get(response.headers, 'retry-after'), {
        onNone: () => Effect.succeedNone,
        onSome: (named) =>
            Option.match(Number.parse(named), {
                onNone: () =>
                    Option.match(DateTime.make(named), {
                        onNone: () => Effect.succeedNone,
                        onSome: (deadline) =>
                            Effect.map(DateTime.now, (now) => Either.getRight(DateTime.distanceDurationEither(now, deadline))),
                    }),
                onSome: (seconds) => Effect.succeedSome(Duration.seconds(seconds)),
            }),
    });

// The refusal becomes THIS page's fault only where a window was actually stated; otherwise the platform's own
// `ResponseError` passes through untouched, so no re-wrap invents evidence the peer never sent.
const _throttled =
    (lane: Client.Lane) =>
    (refusal: HttpClientError.ResponseError): Effect.Effect<never, Lapse | HttpClientError.ResponseError> =>
        Effect.flatMap(_stated(refusal.response), (after) =>
            Option.isSome(after)
                ? Effect.fail(new Lapse({ case: { reason: 'throttled', lane, status: refusal.response.status }, after }))
                : Effect.fail(refusal));

// One re-offer at the wait the producer named: `Fault.Budget.schedule` re-seats the lane row's `base` from the
// stated window and the curve grows from there under that row's own attempts, reset, and elapsed ceiling — so the
// blind curves stay compiled at module evaluation and only a stated re-offer pays a compile, once per refusal.
const _paced =
    (lane: Client.Lane) =>
    <A, E, R>(dial: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
        Effect.catchIf(
            dial,
            (fault: E) => Option.isSome(Fault.Class.statedOf(fault)),
            (fault) =>
                Effect.retry(dial, Fault.Budget.schedule(_lanes[lane].kind, Function.constTrue, Fault.Class.statedOf(fault))),
        );

const _gated = <A, E, R>(
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
    self: Effect.Effect<A, E, R>,
): Effect.Effect<A, E | Lapse, R> =>
    pipe(
        self,
        _paced(lane),
        (sent) =>
            Option.match(_lanes[lane].budget, {
                onNone: () => sent,
                onSome: (budget) =>
                    Effect.timeoutFail(sent, {
                        duration: budget,
                        onTimeout: () => new Lapse({ case: { reason: 'budget', lane, budget }, after: Option.none() }),
                    }),
            }),
        (bounded) =>
            Option.match(_lanes[lane].break, {
                onNone: () => bounded,
                onSome: (policy) => Breaker.guard(_route(lane, request), policy)(bounded),
            }),
    );

// The band reads ONCE per dial and outside the client, so a transient retry re-presents the same credential — one
// call is one attempt at one identity — and the `feed` row's `present` cell is that single read stated as a forfeit.
const _sent = (
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
    Effect.flatMap(_band(lane, _origin(request)), (band) =>
        Effect.flatMap(HttpClient.HttpClient, (client) =>
            Effect.catchTag(
                _tempered(lane)(client).execute(HttpClientRequest.setHeaders(request, band)),
                'ResponseError',
                _throttled(lane),
            )));

function dial(
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope>;
function dial<A, I, R>(
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
    shape: Schema.Schema<A, I, R>,
): Effect.Effect<A, HttpClientError.HttpClientError | Lapse | ParseResult.ParseError, HttpClient.HttpClient | R>;
function dial<A, I, R>(lane: Client.Lane, request: HttpClientRequest.HttpClientRequest, shape?: Schema.Schema<A, I, R>) {
    const sent = _sent(lane, request);
    return shape === undefined
        ? _gated(lane, request, sent)
        : _gated(
              lane,
              request,
              Effect.scoped(Effect.flatMap(sent, HttpClientResponse.schemaBodyJson(shape))).pipe(
                  HttpIncomingMessage.withMaxBodySize(Option.some(_lanes[lane].body)),
              ),
          );
}
```

## [05]-[DISPATCH_ROWS]

[DISPATCH_ROWS]:
- Owner: `Client.resident(policy)` — the parameterized node-residency Layer generator the root composes beneath the lane algebra: `NodeHttpClient.layerUndici` remains the default binding, while a supplied `Undici.Agent.Options` value fills the `NodeHttpClient.Dispatcher` Tag with a scoped `new Undici.Agent(policy)` beneath `NodeHttpClient.layerUndiciWithoutDispatcher`; proxy residency selects an `Undici.ProxyAgent` or `Undici.EnvHttpProxyAgent` dispatcher through the same scoped Tag, never a parallel client surface.
- Law: the option vocabulary is the agent's own — `connections` (per-origin client ceiling), `pipelining` (HTTP/1.1 pipeline depth), `keepAliveTimeout`/`keepAliveMaxTimeout` (idle-socket posture), `maxHeaderSize`, `headersTimeout`/`bodyTimeout` (transport-level stall bounds beneath the lane budgets), `maxOrigins`, and `connect` (the TLS pin and CA material) — every row a declared `Agent.Options` member; the proxy residency swaps the constructor, never the shape: `Undici.ProxyAgent` for a pinned egress proxy, `Undici.EnvHttpProxyAgent` where the environment declares one.
- Law: admitting HTTP/2 states its whole residency, never `allowH2` alone — `maxConcurrentStreams` is the h2 analogue of `connections`, `initialWindowSize`/`connectionWindowSize` are the stream and session flow-control windows a large body stalls against, and `pingInterval` is the h2 liveness probe; arming the protocol and leaving its coordinates at their defaults sizes nothing.
- Law: this dispatcher governs the platform `HttpClient` alone — `[06]`'s Connect arm opens `node:http2` sessions through its own module, which no undici agent ever sees, so that arm READS this row for its session ceiling, window sizes, and ping posture instead of inheriting them; one declared residency, two pools, no second set of numbers.
- Law: residency is root data — the dispatcher row composes once at the boot edge under `proc/exec#RUNTIME_ROWS`'s node row; a lane never names a dispatcher fact, the bun row has no dispatcher by construction (native fetch), and the browser lane's transport is `browser/fetch#BINDING_ROWS`'s XHR client row.
- Law: the raw `undici` surface is reached only through the binding's `Undici` re-export at this one seam — a direct `undici` import anywhere else bypasses tracing, the typed error rail, and pooling policy in one stroke.
- Growth: a new residency fact (an egress proxy, a TLS pin, a flow-control window) is one field on the root policy passed to the same generator.
- Packages: `@effect/platform-node` (`NodeHttpClient`, `Undici`), `effect` (`Effect`, `Layer`).

```typescript signature
import { NodeHttpClient, Undici } from '@effect/platform-node';
import { Effect, Layer } from 'effect';

// `[06]`'s Connect arm reads the h2 rows beneath `allowH2`: undici never dispatches that arm's own `node:http2`
// sessions, so this row writes down the estate's one h2 residency ceiling.
const _dispatch = {
    connections: 128,
    pipelining: 1,
    keepAliveTimeout: 30_000,
    keepAliveMaxTimeout: 600_000,
    maxHeaderSize: 32_768,
    headersTimeout: 30_000,
    bodyTimeout: 300_000,
    allowH2: true,
    maxConcurrentStreams: 128,
    initialWindowSize: 4_194_304,
    connectionWindowSize: 16_777_216,
    pingInterval: 30_000,
    maxOrigins: 512,
} as const satisfies Undici.Agent.Options;

const _resident = (policy: Undici.Agent.Options): Layer.Layer<HttpClient.HttpClient> =>
    NodeHttpClient.layerUndiciWithoutDispatcher.pipe(
        Layer.provide(
            Layer.scoped(
                NodeHttpClient.Dispatcher,
                Effect.acquireRelease(
                    Effect.orDie(Effect.try(() => new Undici.Agent(policy))), // the constructor refuses an out-of-range option by throwing: a boot-edge misconfiguration dies as a defect, never as a silent agent
                    (agent) => Effect.orDie(Effect.tryPromise(() => agent.close())),
                ),
            ),
        ),
    );

const Client = { dial, resident: _resident, residency: _dispatch } as const;
```

## [06]-[CONNECT_ROW]

[CONNECT_ROW]:
- Owner: `Rpc` — the outbound Connect dispatch row conversing with the C# gRPC host under the branch egress law. `_dialects` is the roster: each row pairs one published factory with the policy it dials under, so `Rpc.transport(dialect, baseUrl)` is a keyed table read yielding a scoped `Transport`, and every arm returns that one shape so a generated client stays protocol-blind. `Rpc.call(lane, origin, thunk)` lifts the promise-world call and spends the lane's retry curve, total budget, circuit admission, and credential band through the same `_gated` fold every HTTP dial rides, keyed by lane and origin.
- Cases: `connect` (the Connect protocol's own wire, GET reachable on side-effect-free unary methods), `grpc` (the native wire the C# host serves), `grpc-web` (the wire an intermediary terminates ahead of that host) — rows over one uniform `make`, so a further wire is one row and zero dispatch arms.
- Law: every row dials HTTP/2 — `Http2SessionManager` is the residency owner and a session binding means nothing on HTTP/1.1, so the two records declaring `httpVersion` pin it here while the `grpc` record declares no such member at all and its factory injects `"2"` itself. Proxies refusing h2 earn their own Connect row, never a knob the third arm cannot answer.
- Law: codec coordinates are declared, never inherited — `_CODEC` pins `sendCompression` to `compressionGzip` under the estate conformance row binding gzip as a composition VALUE on every sender exposing an encoding knob, accepts gzip ahead of `compressionBrotli`, states `compressMinBytes` at the package's own kibibyte floor, and reads its frame caps off the widest lane body, so this row exposes an encoding knob and answers it rather than declaring a parity column.
- Law: the transport declares no `defaultTimeoutMs` and the call sets no `CallOptions.timeoutMs` — a transport-wide deadline cannot know which lane a call selects, and a per-call one answers `Code.DeadlineExceeded` beside the lane's own `Lapse`, minting the second timeout convention this page's lead names as a defect.
- Law: the thunk takes ONE `Rpc.Call` fragment and spreads it whole into `CallOptions`, because the two coordinates this seam owns — the fiber's `AbortSignal` and the lane's credential band — are both per-call `CallOptions` members and handing them over separately invites a consumer to forward one and drop the other. Threading `signal` is what makes a `Lapse` on the lane budget cancel the HTTP/2 stream as `Code.Canceled` instead of abandoning it — a give-up leaving the stream in flight holds a session slot for the full server-side duration, against the `maxConcurrentStreams` ceiling `[05]` declares. `Effect.tryPromise` hands its evaluator that signal directly, so no `AbortController` threads through the caller's own flow, and the interrupt this cancellation raises is the one `[03]`'s `halted` verdict refuses to charge.
- Law: the credential band is the same projection the HTTP arm stamps, read once ahead of the retry ladder — gRPC call metadata IS `CallOptions.headers`, so `Machine.band` answers the origin this transport addresses and the band rides into the call rather than onto a transport-wide record, which could not know which audience a generated client reached. Reading once is correct rather than thrifty: `Code.Unauthenticated` grades `denied` in `_grades` and spends no attempt, so a stale credential ends the call and the caller's NEXT call re-reads the source, which is exactly where a rotation lands.
- Law: the band and the trace print stay disjoint by key — this seam writes `authorization` alone while `core:interchange/invoke#DIAL_AXIS`'s per-call lift writes the bare W3C pair onto the same `headers`, so the two owners merge without either spelling the other's names, and neither needs an interceptor onion to stay out of the other's way.
- Law: connect-node publishes no retry knob on any transport record, so the one-attempt provider pin binds nothing and `core/value/fault#RETRY_BUDGET` composes the whole curve at this seam — `_graded` folds the protocol's closed `Code` enum onto `Fault.Class` kinds and the lane schedule gates on the derived `Fault.Class.retryable` projection, so a caller-blamed code spends no attempt. `Http2SessionManager` reopening past a `GOAWAY` replaces the connection inside one attempt and is not a re-attempt: it reads no schedule and spends no budget.
- Law: the promise seam folds at this owner — a rejection converts through the package's own `ConnectError.from`, so the fault channel carries the typed `ConnectError` family a consumer's `Code` dispatch already routes and no `unknown` channel escapes the seam that widened it.
- Law: W3C print crosses at the Connect client owner, never this row — `core:interchange/invoke#DIAL_AXIS`'s per-call lift folds `Carrier.current` through `Carrier.inject("connect", ...)` onto the call's own `headers`, so the trace continues with no interceptor onion and no ambient runtime handle; `Rpc.call` stays the lane-budget gate over an opaque promise thunk, and generated service-client construction stays at the consumer seam over the core-admitted contract surface.
- Boundary: session residency is this row's, not `[05]`'s — connect-node opens `node:http2` sessions through its own module and no undici agent dispatches them, so `_KEEPALIVE` and the manager's session settings READ the `[05]` residency row and the two pools answer to one declared ceiling.
- Boundary: the server half — the Connect router mounted through the foreign-protocol port — is `serve/live#MOUNT_PORT`'s row and the `@effect/rpc` transports are `serve/api`'s; this owner is egress only.
- Entry: `Rpc.transport(dialect, baseUrl)` at the root; `Rpc.call(lane, origin, (call) => client.<member>(payload, call))` at the consumer seam.
- Growth: a new wire dialect or a new residency posture is one `_dialects` row; a new per-call coordinate is one `Rpc.Call` member every consumer already spreads; codec and keepalive coordinates widen in place.
- Packages: `@connectrpc/connect` (`CallOptions`, `Code`, `ConnectError`, `Transport`), `@connectrpc/connect-node` (`compressionBrotli`, `compressionGzip`, `createConnectTransport`, `createGrpcTransport`, `createGrpcWebTransport`, `Http2SessionManager`, `Http2SessionOptions`), `effect` (`Effect`, `Scope`), `@rasm/ts/core` (`Fault.Budget`, `Fault.Class`).

```typescript signature
import { type CallOptions, Code, ConnectError, type Transport } from '@connectrpc/connect';
import {
    compressionBrotli,
    compressionGzip,
    createConnectTransport,
    createGrpcTransport,
    createGrpcWebTransport,
    Http2SessionManager,
    type Http2SessionOptions,
} from '@connectrpc/connect-node';

// Frame caps read the widest lane body so no rpc-local size literal exists, and the compression floor restates the
// package default in the open: a caller reads the threshold on the row instead of inheriting it from a changelog.
const _CODEC = {
    useBinaryFormat: true,
    sendCompression: compressionGzip,
    acceptCompression: [compressionGzip, compressionBrotli],
    compressMinBytes: 1_024,
    readMaxBytes: _lanes.batch.body,
    writeMaxBytes: _lanes.batch.body,
};

// PING liveness reads the [05] residency row at the one arm undici never dispatches; `pingTimeoutMs` takes the
// header-stall bound because both measure the same failure — the remote left a frame unanswered.
const _KEEPALIVE = {
    pingIntervalMs: _dispatch.keepAliveTimeout,
    pingTimeoutMs: _dispatch.headersTimeout,
    pingIdleConnection: true,
    idleConnectionTimeoutMs: _dispatch.keepAliveMaxTimeout,
} as const satisfies Http2SessionOptions;

declare namespace Rpc {
    type Dial = { readonly baseUrl: string; readonly session: Http2SessionManager };
    type Dialect = keyof typeof _dialects;
    // The per-call fragment a consumer spreads WHOLE into `CallOptions`: both members are this seam's, and handing
    // them over one at a time is how a call ends up cancellable but unauthenticated, or the reverse.
    type Call = Pick<Required<CallOptions>, 'headers' | 'signal'>;
}

// One row per wire dialect over one uniform `make`: the `grpc` record carries no `httpVersion` member and its
// factory injects `"2"`, so only the arms declaring the member spell it and dispatch stays a table read.
const _dialects = {
    connect: {
        make: ({ baseUrl, session }: Rpc.Dial): Transport =>
            createConnectTransport({ baseUrl, httpVersion: '2', sessionManager: session, ..._CODEC }),
        fits: '<connect-wire:the Connect protocol own wire,GET reachable on side-effect-free unary methods>',
    },
    grpc: {
        make: ({ baseUrl, session }: Rpc.Dial): Transport => createGrpcTransport({ baseUrl, sessionManager: session, ..._CODEC }),
        fits: '<grpc-wire:the native wire the C# host serves>',
    },
    'grpc-web': {
        make: ({ baseUrl, session }: Rpc.Dial): Transport =>
            createGrpcWebTransport({ baseUrl, httpVersion: '2', sessionManager: session, ..._CODEC }),
        fits: '<grpc-web-wire:an intermediary terminating gRPC-Web ahead of the host>',
    },
} as const;

// One row per Connect code: the protocol closes the enum, so `satisfies` proves the grading total and no code
// reaches the retry gate ungraded; caller-blamed rows grade non-retryable, so a bad argument spends no attempt.
const _grades = {
    [Code.Canceled]: 'expired',
    [Code.Unknown]: 'defect',
    [Code.InvalidArgument]: 'invalid',
    [Code.DeadlineExceeded]: 'expired',
    [Code.NotFound]: 'absent',
    [Code.AlreadyExists]: 'conflicted',
    [Code.PermissionDenied]: 'denied',
    [Code.ResourceExhausted]: 'exhausted',
    [Code.FailedPrecondition]: 'conflicted',
    [Code.Aborted]: 'conflicted',
    [Code.OutOfRange]: 'invalid',
    [Code.Unimplemented]: 'breached',
    [Code.Internal]: 'defect',
    [Code.Unavailable]: 'unavailable',
    [Code.DataLoss]: 'breached',
    [Code.Unauthenticated]: 'denied',
} as const satisfies Record<Code, Fault.Class.Kind>;

const _graded = (fault: unknown): Fault.Class.Kind => (fault instanceof ConnectError ? _grades[fault.code] : Fault.Class.of(fault));

const _session = (baseUrl: string): Effect.Effect<Http2SessionManager, never, Scope.Scope> =>
    Effect.acquireRelease(
        Effect.orDie(
            Effect.try(
                () =>
                    new Http2SessionManager(baseUrl, _KEEPALIVE, {
                        peerMaxConcurrentStreams: _dispatch.maxConcurrentStreams,
                        settings: { initialWindowSize: _dispatch.initialWindowSize },
                    }),
            ),
        ),
        (session) => Effect.orDie(Effect.try(() => session.abort())), // scope closure ends every open stream: an orphaned session outlives the root that opened it
    );

const _transport = (dialect: Rpc.Dialect, baseUrl: string): Effect.Effect<Transport, never, Scope.Scope> =>
    Effect.map(_session(baseUrl), (session) => _dialects[dialect].make({ baseUrl, session }));

const _rpc = <A>(lane: Client.Lane, origin: string, thunk: (call: Rpc.Call) => Promise<A>): Effect.Effect<A, ConnectError | Lapse> =>
    // The band reads ahead of the ladder because `Code.Unauthenticated` grades `denied` and spends no attempt: a
    // stale credential ends this call, and the caller's next one reads the source that rotation already refreshed.
    Effect.flatMap(_band(lane, origin), (headers) =>
        _gated(
            lane,
            HttpClientRequest.get(origin),
            Effect.retry(
                Effect.tryPromise({
                    try: (signal) => thunk({ headers, signal }), // the evaluator receives the fiber's interrupt-wired signal: the lane budget becomes real cancellation, not a local give-up
                    catch: (reason) => ConnectError.from(reason),
                }),
                Fault.Budget.schedule(_lanes[lane].kind, (fault) => Fault.Class.retryable(_graded(fault))),
            ),
        ));

const Rpc = {
    call: _rpc,
    transport: _transport,
} as const;

// --- [EXPORTS] --------------------------------------------------------------------------

export { Breaker, Client, Lapse, Machine, Rpc };
```

## [07]-[RESEARCH]

(none)
