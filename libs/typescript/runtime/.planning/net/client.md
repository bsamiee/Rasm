# [RUNTIME_CLIENT]

Outbound HTTP policy is one lane table, composed once, inherited everywhere: every branch egress — AI provider calls, runner discovery, OTLP export, the config chain's remote stage — dials through one entry that applies its lane's status admission, transient retry, redirect ceiling, total budget, circuit admission, and W3C trace propagation as composed transformers over the shared `HttpClient` the runtime row provided. A lane is a policy row whose durations are the core budget ledger's — each row names its `Budget` kind, its retry pulse compiles from that row's axes, and its total budget is that row's `total` — so retry posture is one cross-language ledger and no per-lane duration literal exists. This circuit ledger is the branch's one breaker owner: a keyed closed→open→half-open cell folded purely and applied as a guard transformer, riding every dial by row and exported so the fanout publish and the delivery transmit inherit the identical admission law. Transport residency tunes beneath the table: the undici dispatcher rows — connection ceilings, pipelining, keep-alive — pin under the node row's client at the root, so pooling is dispatcher configuration and policy is composed transformers, one client, both concerns. A per-folder client, a bare `fetch`, a call-site retry loop, a hand breaker beside the ledger, and a second timeout convention are the named defects; framed and server-sent transport is `channel`'s. This module is `runtime/src/net/client.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                        | [PUBLIC]           |
| :-----: | :-------------- | :---------------------------------------------------------------------------- | :----------------- |
|  [01]   | `LANE_ROWS`     | the closed policy table — ledger binding, pulse compile, hops, circuit row    | `Client`           |
|  [02]   | `BREAK_STATE`   | the keyed circuit ledger — pure admission/settle folds, the guard transformer | `Breaker`, `Lapse` |
|  [03]   | `DIAL_SEAM`     | the one entry, the budget geometry, the consumer law                          | `Client`           |
|  [04]   | `DISPATCH_ROWS` | undici dispatcher tuning beneath the node row's client                        | root data          |
|  [05]   | `CONNECT_ROW`   | the Connect/gRPC dispatch row — transport table and lane inheritance          | `Rpc`              |

## [02]-[LANE_ROWS]

[LANE_ROWS]:
- Owner: the interior `_lanes` anchor — `live` (interactive calls), `batch` (bulk and export egress), `feed` (long-lived streaming responses) — each row carrying `kind` (the `core/value/fault#RETRY_BUDGET` ledger row the lane's durations read), `budget` (`Option<Duration>` — the ledger row's `total` on the settled lanes, stated absence on `feed` because the connection outlives any deadline), `hops` (the redirect ceiling, zero on `feed`), and `break` (`Option<Breaker.Policy>` — the circuit row the guard reads; stated absence on `feed` because the reconnect pulse already paces re-dials).
- Law: the pulse is the ledger owner's own compile — `Budget.schedule(kind, Function.constTrue)` hands the lane the shared compiled base with the class gate stood down, because `HttpClient.retryTransient` already gates transience at the transport altitude; no lane re-spells the compile chain, so tuning a lane is editing the ledger row and every consumer of that lane inherits the edit at once.
- Law: the row guard closes the member set and the table grows by evidence — `_Rows` proves every lane carries the full policy complement, the anchor itself is the lane set, and a genuinely new egress contract (a webhook lane, a hedged lane) is one row and zero new surface.
- Boundary: proxy is transport residency, not per-call policy — the lane table carries no proxy knob, the browser lane has none by construction, and the dispatcher rows in `[5]` own residency.
- Packages: `effect` (`Duration`, `Function`, `Option`), `@rasm/ts/core` (`Budget`).

## [03]-[BREAK_STATE]

[BREAK_STATE]:
- Owner: `Breaker` — the one circuit owner of the branch. A circuit is a keyed cell holding one closed `Data.taggedEnum` state family — `Closed` carrying its generation and fault count, `Open` its generation and reopen instant, `Half` its generation and probe ration, so an invalid field combination is unconstructible — whose transitions are two total `$match` folds: `_admitted` returns the admitted generation and advances `Open→Half` when the cool window lapses, `_settled` accepts only an outcome from that generation, success advances to a fresh closed generation, and failure opens a fresh generation after a half-open fault or trip-count breach. `Breaker.guard(key, policy)` is the transformer any egress effect composes; the dial keys it by lane and request origin — path and query variants share one circuit because they share the remote's fate — and fanout publish and delivery transmit compose the same guard under their own identities.
- Law: state rides `Ref.modify` — admission and settlement are atomic pure folds over the cell, so concurrent dials race on the ledger, never on a lock, and the machine is replayable from its fold functions alone; a new circuit state is one case row breaking both folds loudly at their `$match` records, never a widened field bag.
- Law: rejection is `Lapse` evidence — `reason: "break"`, class `unavailable`, the policy's `cool` as the spent span — so an open circuit routes through the same budget gate as every transient and no second shed fault exists.
- Law: the registry is a `Context.Reference` — cells key by guard key in one `MutableHashMap` default bounded by the capacity row: a mint at capacity flushes the ledger to rest, so degradation is a cold circuit, never process-lifetime memory growth; the requirement channel stays clean (`R` never grows), and a root or proof overrides the whole ledger by providing the Reference. Exemption: `_cell` is the one statement kernel — the synchronous mint-or-get, with the capacity flush, against the registry map.
- Growth: hedging and load-shed stay owned elsewhere (`Effect.raceAll` at the caller, `Gate.shed` at the serving edge); a per-tenant circuit is a key suffix, zero new surface.
- Packages: `effect` (`Clock`, `Context`, `Data`, `Duration`, `MutableHashMap`, `Option`, `Ref`), `@rasm/ts/core` (`FaultClass`).

```typescript signature
class Lapse extends Data.TaggedError('Lapse')<{
    readonly lane: string;
    readonly budget: Duration.Duration;
    readonly reason: 'budget' | 'break';
}> {
    get class(): FaultClass.Kind {
        return this.reason === 'budget' ? 'expired' : 'unavailable';
    }
}

declare namespace Breaker {
    type Cell = Data.TaggedEnum<{
        Closed: { readonly generation: number; readonly faults: number };
        Open: { readonly generation: number; readonly until: number };
        Half: { readonly generation: number; readonly probes: number };
    }>;
    type Policy = { readonly trip: number; readonly cool: Duration.Duration; readonly probes: number };
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

const _settled = (held: Breaker.Cell, generation: number, now: number, policy: Breaker.Policy, passed: boolean): Breaker.Cell =>
    held.generation !== generation
        ? held
        : passed
          ? _Cell.Closed({ generation: generation + 1, faults: 0 })
          : _Cell.$match(held, {
                Closed: ({ faults }) =>
                    faults + 1 >= policy.trip
                        ? _Cell.Open({ generation: generation + 1, until: now + Duration.toMillis(policy.cool) })
                        : _Cell.Closed({ generation, faults: faults + 1 }),
                Open: (open) => open, // a late loser settles against an already-open cell: the cell holds
                Half: () => _Cell.Open({ generation: generation + 1, until: now + Duration.toMillis(policy.cool) }),
            });

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
                ? yield* Effect.tapBoth(self, {
                      onFailure: () =>
                          Effect.flatMap(Clock.currentTimeMillis, (at) =>
                              Ref.update(cell, (held) => _settled(held, admitted.value, at, policy, false)),
                          ),
                      onSuccess: () =>
                          Effect.flatMap(Clock.currentTimeMillis, (at) =>
                              Ref.update(cell, (held) => _settled(held, admitted.value, at, policy, true)),
                          ),
                  })
                : yield* new Lapse({ lane: key, budget: policy.cool, reason: 'break' });
        });

const Breaker = { guard: _guard } as const;
```

## [04]-[DIAL_SEAM]

[DIAL_SEAM]:
- Owner: `Client.dial` — the one entry. Modality follows the call shape: `dial(lane, request)` yields the scoped `HttpClientResponse` (the caller owns the body's lifetime — the `feed` posture); `dial(lane, request, shape)` fuses execution, status admission, bounded body materialization through `HttpIncomingMessage.withMaxBodySize`, JSON-body decode through `HttpClientResponse.schemaBodyJson`, and scope closure into one self-contained step; both apply the lane's transformers — `HttpClient.filterStatusOk`, `HttpClient.followRedirects`, `HttpClient.retryTransient({ schedule })`, `HttpClient.withTracerPropagation(true)` — over the client yielded from the requirement channel.
- Law: budget geometry is stated, not accidental — the lane budget is the TOTAL budget, applied above transient retry and, on the settled modality, above body drain and Schema decode, so retries and a slow body spend the same allowance; a per-attempt sub-budget is deliberately not a knob, and a surface needing one composes the ledger row's `attempt` duration as its own `Effect.fn` pipeline step under the rails layering law.
- Law: expiry and shed are one typed family — `Lapse` carries the lane and the spent span as evidence, its `reason` splitting `budget` (class `expired`) from `break` (class `unavailable`), so the core budget gate re-drives both where a consumer composes a ledger schedule; transport and status faults ride the platform's own `HttpClientError` family untouched, and decode skew rides `ParseError` — three families, each already routable, none re-wrapped.
- Law: request construction is the platform surface at full depth — `HttpClientRequest.get`/`post`, `bodyJson`, `bearerToken`, `setHeader`, `setUrlParams` compose at the consumer's seam; the dial owns policy, never request vocabulary.
- Boundary: the client binding is the runtime row's (`exec#RUNTIME_ROWS`); OTLP export composes the `batch` lane so telemetry egress inherits the same posture as every other call — an exporter with a private client is the named fork.
- Entry: `Client.dial(lane, request[, shape])`; `R` carries `HttpClient` (and `Scope` on the response modality) to the root.
- Receipt: the overload annotations are the whole seam contract — fault union and requirement set readable without opening the body.
- Packages: `@effect/platform` (`HttpClient`, `HttpClientRequest`, `HttpClientResponse`), `effect` (`Data`, `Effect`, `Function`, `Option`).

```typescript signature
import { HttpClient, type HttpClientError, type HttpClientRequest, HttpClientResponse, HttpIncomingMessage } from '@effect/platform';
import {
    Clock,
    Context,
    Data,
    Duration,
    Effect,
    Function,
    MutableHashMap,
    Option,
    type ParseResult,
    Ref,
    type Schema,
    type Scope,
    pipe,
} from 'effect';
import { Budget, type FaultClass } from '@rasm/ts/core';

const _lanes = {
    live: {
        kind: 'pulse',
        budget: Option.some(Budget.pulse.total),
        body: 8_388_608,
        hops: 2,
        break: Option.some({ trip: 8, cool: Duration.seconds(30), probes: 1 }),
    },
    batch: {
        kind: 'bulk',
        budget: Option.some(Budget.bulk.total),
        body: 33_554_432,
        hops: 2,
        break: Option.some({ trip: 16, cool: Duration.seconds(45), probes: 2 }),
    },
    feed: { kind: 'feed', budget: Option.none<Duration.Duration>(), body: 1_048_576, hops: 0, break: Option.none<Breaker.Policy>() },
} as const;

declare namespace Client {
    type Lane = keyof typeof _lanes;
    type Row = {
        readonly kind: Budget.Kind;
        readonly budget: Option.Option<Duration.Duration>;
        readonly body: number;
        readonly hops: number;
        readonly break: Option.Option<Breaker.Policy>;
    };
    type _Rows<T extends Record<Lane, Row> = typeof _lanes> = T;
}

const _route = (lane: Client.Lane, request: HttpClientRequest.HttpClientRequest): string =>
    `${lane}:${URL.parse(request.url)?.origin ?? request.url}`; // one circuit per lane and origin: path and query variants share the cell they share fate with

const _tempered =
    (lane: Client.Lane) =>
    (client: HttpClient.HttpClient): HttpClient.HttpClient =>
        client.pipe(
            HttpClient.filterStatusOk,
            HttpClient.followRedirects(_lanes[lane].hops),
            HttpClient.retryTransient({ schedule: Budget.schedule(_lanes[lane].kind, Function.constTrue) }),
            HttpClient.withTracerPropagation(true),
        );

const _gated = <A, E, R>(
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
    self: Effect.Effect<A, E, R>,
): Effect.Effect<A, E | Lapse, R> =>
    pipe(
        self,
        (sent) =>
            Option.match(_lanes[lane].budget, {
                onNone: () => sent,
                onSome: (budget) => Effect.timeoutFail(sent, { duration: budget, onTimeout: () => new Lapse({ lane, budget, reason: 'budget' }) }),
            }),
        (bounded) =>
            Option.match(_lanes[lane].break, {
                onNone: () => bounded,
                onSome: (policy) => Breaker.guard(_route(lane, request), policy)(bounded),
            }),
    );

const _sent = (
    lane: Client.Lane,
    request: HttpClientRequest.HttpClientRequest,
): Effect.Effect<HttpClientResponse.HttpClientResponse, HttpClientError.HttpClientError, HttpClient.HttpClient | Scope.Scope> =>
    Effect.flatMap(HttpClient.HttpClient, (client) => _tempered(lane)(client).execute(request));

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
- Owner: `Client.resident(policy)` — the parameterized node-residency Layer generator the root composes beneath the lane algebra: `NodeHttpClient.layerUndici` remains the default binding, while a supplied `Undici.Agent.Options` value provides the `NodeHttpClient.Dispatcher` Tag with a scoped `new Undici.Agent(policy)` beneath `NodeHttpClient.layerUndiciWithoutDispatcher`; proxy residency selects an `Undici.ProxyAgent` or `Undici.EnvHttpProxyAgent` dispatcher through the same scoped Tag, never a parallel client surface.
- Law: the option vocabulary is the agent's own — `connections` (per-origin pool ceiling), `pipelining` (HTTP/1.1 pipeline depth), `keepAliveTimeout`/`keepAliveMaxTimeout` (idle-socket posture), `maxHeaderSize`, `headersTimeout`/`bodyTimeout` (transport-level stall bounds beneath the lane budgets), `allowH2` (HTTP/2 session admission), `maxOrigins`, and `connect` (the TLS pin and CA material) — every row a declared `Agent.Options` member; the proxy residency swaps the constructor, never the shape: `Undici.ProxyAgent` for a pinned egress proxy, `Undici.EnvHttpProxyAgent` where the environment declares one.
- Law: residency is root data — the dispatcher row composes once at the boot edge under `exec#RUNTIME_ROWS`'s node row; a lane never names a dispatcher fact, the bun row has no dispatcher by construction (native fetch), and the browser lane's transport is `browser/fetch#BINDING_ROWS`'s XHR client row.
- Law: the raw `undici` surface is reached only through the binding's `Undici` re-export at this one seam — a direct `undici` import anywhere else bypasses tracing, the typed error rail, and pooling policy in one stroke.
- Growth: a new residency fact (an egress proxy, a TLS pin, HTTP/2 session tuning) is one field on the root policy passed to the same generator.
- Packages: `@effect/platform-node` (`NodeHttpClient`, `Undici`), `effect` (`Effect`, `Layer`).

```typescript signature
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
    maxOrigins: 512,
} as const satisfies Undici.Agent.Options;

const _resident = (policy: Undici.Agent.Options): Layer.Layer<HttpClient.HttpClient> =>
    NodeHttpClient.layerUndiciWithoutDispatcher.pipe(
        Layer.provide(
            Layer.scoped(
                NodeHttpClient.Dispatcher,
                Effect.acquireRelease(
                    Effect.sync(() => new Undici.Agent(policy)),
                    (agent) => Effect.orDie(Effect.tryPromise(() => agent.close())),
                ),
            ),
        ),
    );

const Client = { dial, resident: _resident, residency: _dispatch } as const;
```

## [06]-[CONNECT_ROW]

[CONNECT_ROW]:
- Owner: `Rpc` — the outbound Connect/gRPC dispatch row conversing with the C# gRPC host under the branch egress law. `Rpc.transport(spec)` takes one discriminated transport request and dispatches `connect` to `createConnectTransport` or `grpc` to `createGrpcTransport`; both return the same transport shape, so the caller's generated client remains protocol-blind. `Rpc.call(lane, origin, thunk)` lifts the promise-world call and applies the lane's total budget and circuit admission through the same `_gated` fold every HTTP dial rides, keyed by lane and origin.
- Law: rpc traffic inherits the lane table — budget geometry, circuit trip and cool, and the `Lapse` evidence family are `[02]`/`[03]`'s rows unchanged, so a Connect call and an HTTP call to one origin share the circuit they share fate with, and no bespoke rpc timeout convention exists.
- Law: generated service-client construction stays at the consumer seam over the core-admitted contract surface; interceptor-based W3C print and continuation remain blocked in `[07]`, so this row makes no propagation claim.
- Law: the promise seam converts at this owner — the thunk's rejection minted through `Effect.tryPromise` rides `Cause.UnknownException` for the caller's triage into its own fault family; Connect's own `ConnectError` code discrimination is the consumer's dispatch material, never re-wrapped here.
- Boundary: the server half — the Connect router mounted through the foreign-protocol port — is `live#MOUNT_PORT`'s row; this owner is egress only.
- Entry: `Rpc.transport(spec)` at the root; `Rpc.call(lane, origin, () => client.<member>(payload))` at the consumer seam.
- Growth: a new dialect (gRPC-Web egress) is one `TransportSpec` case and one total dispatch arm; compression and deadline posture remain factory-option or call-option values.
- Packages: `@connectrpc/connect-node` (`createConnectTransport`, `createGrpcTransport`), `effect` (`Cause`, `Effect`).

```typescript signature
import { type ConnectTransportOptions, createConnectTransport, createGrpcTransport, type GrpcTransportOptions } from '@connectrpc/connect-node';
import type { Cause } from 'effect';

declare namespace Rpc {
    type Transport = ReturnType<typeof createConnectTransport>;
    type TransportSpec =
        | { readonly kind: 'connect'; readonly options: ConnectTransportOptions }
        | { readonly kind: 'grpc'; readonly options: GrpcTransportOptions };
}

const _transport = (spec: Rpc.TransportSpec): Rpc.Transport => {
    switch (spec.kind) {
        case 'connect':
            return createConnectTransport(spec.options);
        case 'grpc':
            return createGrpcTransport(spec.options);
    }
};

const _rpc = <A>(
    lane: Client.Lane,
    origin: string,
    thunk: () => Promise<A>,
): Effect.Effect<A, Cause.UnknownException | Lapse> =>
    _gated(lane, HttpClientRequest.get(origin), Effect.tryPromise(thunk));

const Rpc = {
    call: _rpc,
    transport: _transport,
} as const;

// --- [EXPORTS] --------------------------------------------------------------------------

export { Breaker, Client, Lapse, Rpc };
```

## [07]-[RESEARCH]

- [CONNECT_INTERCEPTORS]-[BLOCKED]: which exact Connect `Interceptor` call shape and server-router interceptor option compose immutable W3C injection and extraction through the runtime `Propagation` owner without a call-site header thunk; route first through `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/runtime/.api/connectrpc-connect-node.md`, then `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/.api/connectrpc-connect-node.md`, with the peer contract at `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/typescript/core/.api/connectrpc-connect.md`; arm when the client and server interceptor members and the composite carrier setter carry exact catalog rows.
