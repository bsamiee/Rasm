# [RUNTIME_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress dials through one entry applying its lane's status admission, transient retry, redirect ceiling, total budget, circuit admission, machine-credential presentation, and W3C trace propagation as composed transformers over the runtime row's shared `HttpClient`. Per-folder clients, bare `fetch`, call-site retry loops, a hand breaker beside the ledger, a hand-carried static token at the call site, and a second timeout convention are the named defects. Module `runtime/src/net/client.ts`.

Each lane is a policy row whose durations are the core budget ledger's: a row names its `Fault.Budget` kind and its total budget is that row's `total`, so no per-lane duration literal exists. This circuit ledger is the branch's one breaker owner — a keyed closed→open→half-open cell folded purely, applied as a guard transformer, and exported so fanout publish and delivery transmit inherit one admission law. Residency pins at the root as two pools — undici dispatcher and Connect `node:http2` session — reading one ceiling, so policy stays composed transformers; the scoped Node adapter supplies `core:interchange/invoke#DIAL_AXIS` with the three public connect-node transports and owns no selection or retry policy.

## [01]-[INDEX]

- [02]-[LANE_ROWS]: closes the egress policy table — ledger binding, pulse compile, hops, circuit row, credential posture; `Client`.
- [03]-[BREAK_STATE]: folds the keyed circuit ledger — pure admission and settle folds, the guard transformer; `Breaker`, `Lapse`.
- [04]-[DIAL_ENTRY]: seats the one entry — budget geometry, optional and required credential projections, and the consumer law; `Client`, `Machine`, `WebhookOrigin`.
- [05]-[DISPATCH_ROWS]: pins undici residency beneath the node row's client; root data.
- [06]-[NODE_ADAPTER]: seats connect-node as the core dial's scoped `node` adapter — public transport factories, session residency, and the credential interceptor; `Rpc`.

## [02]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the interior `_lanes` anchor — `live` (interactive calls), `batch` (bulk, export, and webhook egress), `feed` (long-lived streaming responses) — each row carrying `kind` (the `core/value/fault#RETRY_BUDGET` ledger row the lane's durations read), `budget` (`Option<Duration>` — the ledger row's `total` on the settled lanes, stated absence on `feed` because the connection outlives any deadline), `body` (`Option<number>` — the materialized-body ceiling the platform member takes verbatim, stated absence on `feed` and the cell `Client.Settled` derives from), `hops` (the redirect ceiling: interactive navigation alone follows, while batch/webhook and feed stay pinned to the addressed origin), and `break` (`Option<Breaker.Policy>` — the circuit row the guard reads; stated absence on `feed` because the reconnect pulse already paces re-dials).
- Law: the row guard closes the member set and the table grows by evidence — `_Rows` proves every lane carries the full policy complement, the anchor itself is the lane set, and a genuinely new egress contract (a webhook lane, a hedged lane) is one row and zero new surface.
- Law: every lane states `fits`, `admit`, `present`, `lifetime`, and `degrade` as cells a root reads before it selects — the `feed` row's stated absences are its forfeit, not an omission, so a caller choosing it accepts no total budget and no circuit knowingly.
- Law: `present` names WHEN the lane reads the credential source, because that is the axis the lanes genuinely differ on — a settled lane re-reads on every dial, so a rotation lands on the next call for free, while `feed` reads once at the dial that opens a response outliving it, so the credential ages inside the stream and only a re-dial re-presents. The projection itself is one stamp at `[04]`; this cell states the consequence a selector accepts, and `feed`'s `degrade` carries the forfeit beside its budget and circuit ones.
- Law: `admit` names the dial modality this lane is entered through and the TYPE enforces it — the settled lanes admit `dial(lane, request, shape)`, which materializes and decodes inside the one total budget, while `feed` admits `dial(lane, request)` alone, since a shape materializes a body a streaming response never ends. `Client.Settled` derives that roster off the `body` cell, because a lane declares a body ceiling exactly where it materializes a body; Naming it in prose alone left the overload accepting `feed` and that row's ceiling a number nothing ever read.
- Law: this table decides NO tenancy — an egress lane carries policy over a shared client and isolates nothing, so it states no tenancy cell at all; per-tenant isolation is a circuit key suffix at `[03]`, and the closed axis stays `proc/config#ADMISSION_ROWS` `Profile`'s.
- Boundary: proxy is transport residency, not per-call policy — the lane table carries no proxy knob, the browser lane has none by construction, and the dispatcher rows in `[5]` own residency.
- Packages: `effect` (`Duration`, `Option`), `@rasm/core` (`Fault.Budget`).

## [03]-[BREAK_STATE]

[BREAK_STATE]:
- Owner: `Breaker` — the one circuit owner of the branch. Each circuit is a keyed cell holding one closed `Data.taggedEnum` state family — `Closed` carrying its generation and fault count, `Open` its generation and reopen instant, `Half` its generation and probe ration, so an invalid field combination is unconstructible — whose transitions are two total `$match` folds: `_admitted` returns the admitted generation and advances `Open→Half` when the cool window lapses, `_settled` accepts only a termination from that generation and applies the `_verdicts` row it names. `Breaker.guard(key, policy)` is the transformer any egress effect composes; the dial keys it by lane and request origin — path and query variants share one circuit because they share the remote's fate — and fanout publish and delivery transmit compose the same guard under their own identities.
- Law: termination is a three-row verdict, never a settled-channel pair — `Effect.onExit` fires once after the outcome settles and `_verdict` folds the `Exit` interrupt-first through `Cause.isInterruptedOnly`, so `passed` advances a fresh closed generation, `faulted` (a typed fault or a defect alike) charges the trip count or reopens off a half-open probe, and `halted` RESTORES the probe `_admitted` consumed. Observing the two settled channels alone strands every interrupted probe: `_gated` composes `Effect.timeoutFail` inside the guard while a caller race, a scope close, an abandoned `ExecutionPlan` step, and an early `Stream` consumer exit all pass through unsettled, so a `probes: 1` lane wedges at `Half { probes: 0 }` — a cell `_admitted` refuses forever, since `Half` carries no `until` escape.
- Law: state rides `Ref.modify` — admission and settlement are atomic pure folds over the cell, so concurrent dials race on the ledger, never on a lock, and the machine is replayable from its fold functions alone; cool windows read `Clock.currentTimeNanos` and project monotonic milliseconds into the pure fold, so a wall-clock correction cannot strand or prematurely reopen a circuit; a new circuit state is one case row breaking every `$match` record loudly, and a new termination class is one `_verdicts` row the `Verdict` union derives.
- Law: rejection is `Lapse` evidence — `reason: "break"`, class `unavailable`, the policy's `cool` as the spent span — so an open circuit routes through the same budget gate as every transient and no second shed fault exists.
- Law: the registry is a `Context.Reference` — cells key by guard key in one `MutableHashMap` default bounded by the capacity row: a mint at capacity flushes the ledger to rest, so degradation is a cold circuit, never process-lifetime memory growth; the requirement channel stays clean (`R` never grows), and a root or proof overrides the whole ledger by providing the Reference. Exemption: `_cell` is the one statement kernel — the synchronous mint-or-get, with the capacity flush, against the registry map.
- Growth: hedging and load-shed stay owned elsewhere (`Effect.raceAll` at the caller, `Gate.shed` at the serving edge); a per-tenant circuit is a key suffix, zero new surface.
- Packages: `effect` (`Cause`, `Clock`, `Context`, `Data`, `Duration`, `Exit`, `MutableHashMap`, `Option`, `Ref`, `Schema`), `@rasm/core` (`Fault.Class`).

```typescript
const _family = Fault.Class.family(['budget', 'break', 'binding', 'credential', 'throttled'] as const, {
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
    credential: Fault.Class.row({
        class: 'denied',
        leg: 'dial',
        detail: Schema.Struct({ lane: Schema.String }),
        render: ({ lane }) => `${lane} requires a machine credential`,
    }),
    throttled: Fault.Class.row({
        class: 'exhausted',
        leg: 'dial',
        detail: Schema.Struct({ lane: Schema.String, status: Schema.Int }),
        render: ({ lane, status }) => `${lane} peer answered ${status} and stated its own re-offer window`,
    }),
});

class Lapse extends Schema.TaggedError<Lapse>()('Lapse', {
    case: _family.payload,
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
    passed: (_held, generation) => _Cell.Closed({ generation: generation + 1, faults: 0 }),
    faulted: (held, generation, now, policy) =>
        _Cell.$match(held, {
            Closed: ({ faults }) =>
                faults + 1 >= policy.trip ? _reopened(generation, now, policy) : _Cell.Closed({ generation, faults: faults + 1 }),
            Open: (open) => open,
            Half: () => _reopened(generation, now, policy),
        }),
    halted: (held) =>
        _Cell.$match(held, {
            Closed: (closed) => closed,
            Open: (open) => open,
            Half: (half) => _Cell.Half({ generation: half.generation, probes: half.probes + 1 }),
        }),
} as const satisfies Record<string, Breaker.Settle>;

const _verdict: (exit: Exit.Exit<unknown, unknown>) => Breaker.Verdict = Exit.match({
    onFailure: (cause) => (Cause.isInterruptedOnly(cause) ? 'halted' : 'faulted'),
    onSuccess: () => 'passed',
});

const _settled = (held: Breaker.Cell, generation: number, now: number, policy: Breaker.Policy, verdict: Breaker.Verdict): Breaker.Cell =>
    held.generation === generation ? _verdicts[verdict](held, generation, now, policy) : held;

const _LEDGER = { capacity: 512 } as const;

const _cell = (cells: MutableHashMap.MutableHashMap<string, Ref.Ref<Breaker.Cell>>, key: string): Ref.Ref<Breaker.Cell> =>
    Option.getOrElse(MutableHashMap.get(cells, key), () => {
        if (MutableHashMap.size(cells) >= _LEDGER.capacity) MutableHashMap.clear(cells);
        const minted = Ref.unsafeMake(_REST);
        MutableHashMap.set(cells, key, minted);
        return minted;
    });

const _guard =
    (key: string, policy: Breaker.Policy) =>
    <A, E, R>(self: Effect.Effect<A, E, R>): Effect.Effect<A, E | Lapse, R> =>
        Effect.gen(function* () {
            const cell = _cell(yield* Breakers, key);
            const now = globalThis.Number((yield* Clock.currentTimeNanos) / 1_000_000n);
            const admitted = yield* Ref.modify(cell, (held) => _admitted(held, now, policy));
            return Option.isSome(admitted)
                ? yield* Effect.onExit(self, (exit) =>
                      Effect.flatMap(Clock.currentTimeNanos, (at) =>
                          Ref.update(cell, (held) =>
                              _settled(held, admitted.value, globalThis.Number(at / 1_000_000n), policy, _verdict(exit)),
                          ),
                      ),
                  )
                : yield* new Lapse({ case: { reason: 'break', lane: key, cool: policy.cool }, after: Option.none() });
        });

const Breaker = { guard: _guard } as const;
```

## [04]-[DIAL_ENTRY]

[DIAL_ENTRY]:
- Owner: `Client.dial` and `Client.authorized` are the one execution entry with optional and required machine postures. Both apply the same lane transformers; `authorized` refuses before I/O when the audience has no machine principal.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above transient retry and, on the settled modality, above body drain and Schema decode, so retries and a slow body spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one composes the ledger row's `attempt` duration as its own `Effect.fn` pipeline step under the effect layering law.
- Law: expiry, shed, throttling, and credential refusal are one typed family: `budget`, `break`, `binding`, required `credential`, and `throttled` each carry only the evidence that decided them.
- Law: a peer that STATES its re-offer window is honored under that window and never under a curve this lane invented — `_retryAfter` admits only the protocol's non-negative integer delay-seconds or an HTTP-date once, its result both excluding the blind curve and raising `Fault.Class.After`; a past date states an immediate zero-duration re-offer, while malformed or fractional delay-seconds state nothing and therefore remain eligible for the ordinary transient policy. The stated schedule re-drives only faults that themselves still carry a stated window, so a later unrelated refusal cannot inherit the first response's consent to retry.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `setHeader`, `setUrlParams` compose at the consumer's boundary; the dial owns policy, never request vocabulary. `bearerToken` is the one member this entry takes BACK, because a workload credential is lane policy and a call-site token is the hand-carried secret the projection deletes.
- Law: the credential is a projection off one port, never a call-site header. `Client.dial` accepts the default absent principal for genuinely public egress; `Client.authorized` reads the same `Machine` source once and makes absence a typed terminal refusal, so protocols requiring bearer authorization cannot silently downgrade to anonymous.
- Law: the audience is the request ORIGIN and the source decides per audience — a fleet token minted for one service is not a bearer instrument for whatever host a caller names, so `Machine.at` asks for the origin the request already addresses and a source holding nothing for it presents nothing; scoping the read by lane instead would hand service A's credential to service B on the same lane, which is credential exfiltration wearing a policy table.
- Law: a sender-constrained principal REFUSES here rather than downgrading — RFC 9449 binds the token to a proof over this call's own method and URL, the certified client's `DPoPHandle` publishes `calculateThumbprint()` and nothing that mints a proof, so no header this plane could write carries the binding; presenting a `dpop`-scheme token under a bare `authorization` strips exactly what the grant paid for, and the `binding` refusal routes the caller to `Workload.call`, which owns the proved read. A bare `Bearer ` prefix is the same defect spelled shorter, which is why `HttpClientRequest.bearerToken` — hardcoding that prefix and spelling no other scheme — is foreclosed and `MachinePrincipal.credential`, carrying the scheme its ISSUER chose, is what lands on the header.
- Law: refresh rides the grant lifecycle, never a lane timer — the source is read per dial, so whatever principal `Workload.rotate` last landed is what the next call stamps and no lane holds a cached credential with an expiry of its own; a lane-side cache re-derives `MachinePrincipal.lapsed` and then drifts from it, and a stale token's `401` is the authority's own answer rather than this plane's guess.
- Law: the stamped header never leaks and this plane adds no scrub — `authorization` is already in the platform's default `Headers.currentRedactedNames`, so every `Headers` inspection, log line, and span attribute renders it `Redacted`; the value stays `Redacted` from the security plane to the one `Redacted.value` at the stamp, which is the exact point the wire demands a string.
- Boundary: the client binding is the runtime row's (`proc/exec#RUNTIME_ROWS`); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork. Principal mint, rotation, introspection, and revocation are `security:authn/workload`'s; this owner mounts the projection and decides nothing about the grant.
- Entry: `Client.dial(lane, request[, shape])` and `Client.authorized(lane, request)`; `R` carries `HttpClient` and `Scope`, and the app root overrides `Machine` where workload identity exists.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientError`, `HttpClientRequest`, `HttpClientResponse`, `Headers.get`), `effect` (`Data`, `DateTime`, `Duration`, `Effect`, `Number`, `Option`, `Redacted`, `Schema`), `@rasm/core` (`Fault.Budget`, `Fault.Class`), `@rasm/security` (`MachinePrincipal`).

```typescript
import { Headers, HttpClient, HttpClientError, HttpClientRequest, HttpClientResponse, HttpIncomingMessage } from '@effect/platform';
import {
    Cause,
    Clock,
    Context,
    Data,
    DateTime,
    Duration,
    Effect,
    Exit,
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
import { Fault, Invoke } from '@rasm/core';
import type { MachinePrincipal } from '@rasm/security';

const _lanes = {
    live: {
        kind: 'pulse',
        budget: Option.some(Fault.Budget.at("pulse").total),
        body: Option.some(8_388_608),
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
        body: Option.some(33_554_432),
        hops: 0,
        break: Option.some({ trip: 16, cool: Duration.seconds(45), probes: 2 }),
        fits: '<bulk-export-and-webhook:no interactive waiter,wider body and trip ceilings,no redirect away from the addressed origin>',
        admit: '<settled-modality:dial(lane,request,shape) under the wider body ceiling this row carries>',
        present: '<per-call:each dial re-reads the source for this origin,so a rotation lands on the next call>',
        lifetime: '<total-budget>',
        degrade: '<slower-trip:sixteen faults before shed;redirect responses refuse instead of moving a signed or machine-credentialed request>',
    },
    feed: {
        kind: 'feed',
        budget: Option.none<Duration.Duration>(),
        body: Option.none<number>(),
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
    type Settled = { [L in Lane]: (typeof _lanes)[L]['body'] extends Option.None<number> ? never : L }[Lane];
    type Row = {
        readonly kind: Fault.Budget.Kind;
        readonly budget: Option.Option<Duration.Duration>;
        readonly body: Option.Option<number>;
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

const WebhookOrigin = Schema.String.pipe(
    Schema.pattern(/^(?=.{1,253}$)(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)*[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$/i),
    Schema.brand('WebhookOrigin'),
);

const _origin = (request: HttpClientRequest.HttpClientRequest): string => URL.parse(request.url)?.origin ?? request.url;

const _route = (lane: Client.Lane, request: HttpClientRequest.HttpClientRequest): string =>
    `${lane}:${_origin(request)}`;

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

const _band = (lane: string, audience: string): Effect.Effect<Readonly<Record<string, string>>, Lapse> =>
    Effect.map(
        _at(lane, audience),
        Option.match({
            onNone: () => ({}),
            onSome: (principal: MachinePrincipal) => ({ authorization: Redacted.value(principal.credential) }),
        }),
    );

const _requiredBand = (lane: string, audience: string): Effect.Effect<Readonly<Record<string, string>>, Lapse> =>
    Effect.flatMap(
        _at(lane, audience),
        Option.match({
            onNone: () => Effect.fail(new Lapse({ case: { reason: 'credential', lane }, after: Option.none() })),
            onSome: (principal: MachinePrincipal) =>
                Effect.succeed({ authorization: Redacted.value(principal.credential) }),
        }),
    );

class Machine extends Context.Reference<Machine>()('runtime/Machine', {
    defaultValue: (): Machine.Source => ({ held: () => Effect.succeedNone }),
}) {
    static readonly at = _at;
    static readonly band = _band;
    static readonly requiredBand = _requiredBand;
}

type _RetryAfter =
    | { readonly kind: 'delay'; readonly seconds: number }
    | { readonly kind: 'date'; readonly deadline: DateTime.Utc };

const _retryAfter = (named: string): Option.Option<_RetryAfter> =>
    Option.match(Option.filter(Number.parse(named), (seconds) => globalThis.Number.isInteger(seconds) && seconds >= 0), {
        onNone: () => Option.map(DateTime.make(named), (deadline) => ({ kind: 'date', deadline } as const)),
        onSome: (seconds) => Option.some({ kind: 'delay', seconds }),
    });

const _blind = (fault: unknown): boolean =>
    !(
        fault instanceof HttpClientError.ResponseError &&
        Option.isSome(Option.flatMap(Headers.get(fault.response.headers, 'retry-after'), _retryAfter))
    );

const _tempered =
    (lane: Client.Lane) =>
    (client: HttpClient.HttpClient): HttpClient.HttpClient =>
        client.pipe(
            HttpClient.followRedirects(_lanes[lane].hops),
            HttpClient.filterStatusOk,
            HttpClient.retryTransient({ schedule: Fault.Budget.schedule(_lanes[lane].kind, _blind) }),
            HttpClient.withTracerPropagation(true),
        );

const _stated = (response: HttpClientResponse.HttpClientResponse): Effect.Effect<Fault.Class.Stated> =>
    Option.match(Option.flatMap(Headers.get(response.headers, 'retry-after'), _retryAfter), {
        onNone: () => Effect.succeedNone,
        onSome: (stated) =>
            stated.kind === 'delay'
                ? Effect.succeedSome(Duration.seconds(stated.seconds))
                : Effect.map(DateTime.now, (now) =>
                      Option.some(Duration.millis(globalThis.Math.max(0, DateTime.distance(now, stated.deadline)))),
                  ),
    });

const _throttled =
    (lane: Client.Lane) =>
    (refusal: HttpClientError.ResponseError): Effect.Effect<never, Lapse | HttpClientError.ResponseError> =>
        Effect.flatMap(_stated(refusal.response), (after) =>
            Option.isSome(after)
                ? Effect.fail(new Lapse({ case: { reason: 'throttled', lane, status: refusal.response.status }, after }))
                : Effect.fail(refusal));

const _paced =
    (lane: Client.Lane) =>
    <A, E, R>(dial: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
        Effect.catchIf(
            dial,
            (fault: E) => Option.isSome(Fault.Class.statedOf(fault)),
            (fault) =>
                Effect.retry(
                    dial,
                    Fault.Budget.schedule(
                        _lanes[lane].kind,
                        (raised) => Option.isSome(Fault.Class.statedOf(raised)),
                        Fault.Class.statedOf(fault),
                    ),
                ),
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

const _sentWith = (
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
    band: Effect.Effect<Readonly<Record<string, string>>, Lapse>,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
    Effect.flatMap(band, (headers) =>
        Effect.flatMap(HttpClient.HttpClient, (client) =>
            Effect.catchTag(
                _tempered(lane)(client).execute(HttpClientRequest.setHeaders(request, headers)),
                'ResponseError',
                _throttled(lane),
            )));

const _sent = (
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
    _sentWith(lane, request, _band(lane, _origin(request)));

const _authorized = (
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope> =>
    _gated(lane, request, _sentWith(lane, request, _requiredBand(lane, _origin(request))));

function dial(
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError | Lapse, HttpClient.HttpClient | Scope.Scope>;
function dial<A, I, R>(
    lane: Client.Settled,
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
                  HttpIncomingMessage.withMaxBodySize(_lanes[lane].body),
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
- Law: the raw `undici` surface is reached only through the binding's `Undici` re-export at this one boundary — a direct `undici` import anywhere else bypasses tracing, the typed error channel, and pooling policy in one stroke.
- Growth: a new residency fact (an egress proxy, a TLS pin, a flow-control window) is one field on the root policy passed to the same generator.
- Packages: `@effect/platform-node` (`NodeHttpClient`, `Undici`), `effect` (`Effect`, `Layer`).

```typescript
import { NodeHttpClient, Undici } from '@effect/platform-node';
import { Effect, Layer } from 'effect';

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
                    Effect.orDie(Effect.try(() => new Undici.Agent(policy))),
                    (agent) => Effect.orDie(Effect.tryPromise(() => agent.close())),
                ),
            ),
        ),
    );

const Client = { authorized: _authorized, dial, resident: _resident, residency: _dispatch } as const;
```

## [06]-[NODE_ADAPTER]

[NODE_ADAPTER]:
- Owner: `Rpc` — the runtime's client-side seat of `@connectrpc/connect-node`: `Rpc.adapter(config)` acquires one `Http2SessionManager` for the decoded peer and returns `Invoke.Dial.Adapter<"node">`, whose total factory record contains the package's public Connect, gRPC-Web, and gRPC constructors; `Rpc.credential` stamps the machine band onto every call from `Machine.band`.
- Law: core owns selection and this owner realizes capability — the adapter accepts `Invoke.Dial.Policy`, maps it once into the three public option records, and returns a closed record; it grades no code, owns no retry ladder, and exposes no protocol knob.
- Law: all three Node transports ride HTTP/2 and the same scoped manager; Connect and gRPC-Web state `httpVersion: "2"`, while native gRPC's public factory fixes HTTP/2 itself. A root needing browser or Bun supplies `Invoke.Dial.web(fetch)`, never a downgraded Node lane.
- Law: only the package's public transport factories inhabit the adapter — no private universal HTTP interface or internal protocol subpath enters branch architecture.
- Law: the credential band is the same projection the HTTP arm stamps, read once per call ahead of the dial's retry ladder — gRPC call metadata IS the request header bag, so the interceptor asks `Machine.band` for the origin the call addresses and writes `authorization` alone, naming this BOUNDARY rather than a lane because no lane transformer brackets a Connect call and a lane-named refusal cites a row that decided nothing; the W3C pair is `core:interchange/invoke#DIAL_AXIS`'s per-call lift on the same bag, so the two owners merge without either spelling the other's names.
- Law: the interceptor re-enters Effect through the captured runtime at the package's promise-shaped interceptor boundary and nowhere else, so a sender-binding refusal (`Lapse` `binding`) rejects before a request dials and surfaces as the dial's `Transport` fault.
- Law: PING liveness reads the `[05]` residency row at the one arm undici never dispatches; `pingTimeoutMs` takes the header-stall bound because both measure the same failure — the remote left a frame unanswered.
- Boundary: session residency is this row's, not `[05]`'s — connect-node opens `node:http2` sessions through its own module and no undici agent dispatches them, so `_KEEPALIVE` and the manager's session settings READ the `[05]` residency row and the two pools answer to one declared ceiling.
- Boundary: the server half — the Connect router mounted through the foreign-protocol port — is `serve/live#MOUNT_PORT`'s row and the `@effect/rpc` transports are `serve/api`'s; this owner is the client adapter alone.
- Boundary: circuit admission is the HTTP lanes' — `Breaker.guard` brackets `Client.dial` and nothing promise-shaped — so Connect egress sheds through the dial's execution-plan failover and the remote's own `Unavailable` class, never through this ledger.
- Entry: `Rpc.adapter(config)` and `Rpc.credential(runtime)` at the root, handed into `Invoke.Dial`'s adapters as `node` and `interceptors`.
- Growth: a new residency posture is one `Http2SessionOptions` value; a new per-call header is one write in the interceptor over the same bag.
- Packages: `@connectrpc/connect` (`Interceptor`), `@connectrpc/connect-node` (three public transport factories, compression rows, `Http2SessionManager`, `Http2SessionOptions`), `effect` (`Effect`, `Runtime`, `Scope`), `@rasm/core` (`Fault.Class`, `Invoke.Dial`).

```typescript
import type { Interceptor } from '@connectrpc/connect';
import {
    compressionBrotli,
    compressionGzip,
    createConnectTransport,
    createGrpcTransport,
    createGrpcWebTransport,
    Http2SessionManager,
    type Http2SessionOptions,
} from '@connectrpc/connect-node';

const _KEEPALIVE = {
    pingIntervalMs: _dispatch.keepAliveTimeout,
    pingTimeoutMs: _dispatch.headersTimeout,
    pingIdleConnection: true,
    idleConnectionTimeoutMs: _dispatch.keepAliveMaxTimeout,
} as const satisfies Http2SessionOptions;

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
        (session) => Effect.orDie(Effect.try(() => session.abort())),
    );

const _rpcOptions = (policy: Invoke.Dial.Policy, session: Http2SessionManager) => ({
    baseUrl: policy.baseUrl,
    useBinaryFormat: policy.useBinaryFormat,
    interceptors: [...policy.interceptors],
    defaultTimeoutMs: policy.defaultTimeoutMs,
    acceptCompression: [compressionBrotli, compressionGzip],
    sendCompression: compressionGzip,
    compressMinBytes: 1_024,
    readMaxBytes: policy.readMaxBytes,
    writeMaxBytes: policy.writeMaxBytes,
    sessionManager: session,
});

const _adapter = (config: Invoke.Dial.Config): Effect.Effect<Invoke.Dial.Adapter<'node'>, never, Scope.Scope> =>
    Effect.map(_session(config.baseUrl), (session) => ({
        kind: 'node',
        factories: {
            connect: (policy) => createConnectTransport({ ..._rpcOptions(policy, session), httpVersion: '2' }),
            'grpc-web': (policy) => createGrpcWebTransport({ ..._rpcOptions(policy, session), httpVersion: '2' }),
            grpc: (policy) => createGrpcTransport(_rpcOptions(policy, session)),
        },
    }));

const _audience = (url: string): string => URL.parse(url)?.origin ?? url;

const _credential = (runtime: Runtime.Runtime<never>): Interceptor => (next) => (request) =>
    Runtime.runPromise(runtime)(
        Effect.map(_band('rpc', _audience(request.url)), (band) => {
            Object.entries(band).forEach(([name, value]) => request.header.set(name, value));
            return request;
        }),
        { signal: request.signal },
    ).then(next);

const Rpc = {
    adapter: _adapter,
    credential: _credential,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Breaker, Client, Lapse, Machine, Rpc, WebhookOrigin };
```

## [07]-[RESEARCH]

(none)
