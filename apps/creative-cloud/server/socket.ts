// --- [IMPORTS] -------------------------------------------------------------------------

import { Context, type Crypto, Deferred, Duration, Effect, Exit, identity, Layer, Match, Option, Predicate, Queue, Result, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { RpcSerialization, RpcServer } from 'effect/unstable/rpc';
import type { SocketServer } from 'effect/unstable/socket';
import { BridgeError, notDecodable } from './errors.ts';
import { AlreadyAttached, type Done, Frames, type Identity, type Job, type Link as LinkState, type Settle } from './frames.ts';
import type { Resolved } from './hosts.ts';
import { type Jobs, run } from './jobs.ts';
import type { JobId } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type SocketHost = Extract<Resolved, { readonly channel: 'socket' }>['id'];

interface Pending {
    readonly jobId: JobId;
    readonly settled: Deferred.Deferred<Done, BridgeError>;
}

type Link = Extract<LinkState, { readonly _tag: 'listening' }> | (Extract<LinkState, { readonly _tag: 'attached' }> & { readonly jobs: Queue.Queue<Job>; readonly pending: Option.Option<Pending> });

interface Endpoint {
    readonly host: SocketHost;
    readonly link: SubscriptionRef.SubscriptionRef<Link>;
}

type Links = Readonly<Record<SocketHost, Endpoint>>;

interface Answer<Value> {
    readonly jobId: JobId;
    readonly value: Value;
    readonly autocorrections: readonly string[];
    readonly tookMs: number;
}

// --- [SERVICES] ------------------------------------------------------------------------

const Links: Context.Service<Links, Links> = Context.Service<Links>('Links');

// --- [STATE] ---------------------------------------------------------------------------

const _pending: (link: Link) => Option.Option<Pending> = Match.valueTags({ listening: Option.none, attached: Struct.get('pending') });

const _public: (link: Link) => LinkState = Match.valueTags({ listening: identity, attached: Struct.omit(['jobs', 'pending']) });

const _detach = (endpoint: Endpoint): Effect.Effect<void> =>
    Effect.flatMap(SubscriptionRef.getAndSet(endpoint.link, { _tag: 'listening' }), (link) =>
        Effect.asVoid(Effect.transposeOption(Option.map(_pending(link), ({ jobId, settled }) => Deferred.fail(settled, BridgeError.cases.pluginDetached.make({ host: endpoint.host, jobId }))))),
    );

const _attach = Effect.fnUntraced(function* (endpoint: Endpoint, plugin: Identity) {
    const jobs = yield* Queue.make<Job>();
    const attached: Link = { _tag: 'attached', identity: plugin, state: Option.none(), jobs, pending: Option.none() };
    yield* Effect.acquireRelease(
        Effect.flatMap(
            SubscriptionRef.modify(
                endpoint.link,
                Match.valueTags({
                    listening: (): readonly [Result.Result<void, AlreadyAttached>, Link] => [Result.void, attached],
                    attached: (link): readonly [Result.Result<void, AlreadyAttached>, Link] => [Result.fail(AlreadyAttached.make({})), link],
                }),
            ),
            Effect.fromResult,
        ),
        () => _detach(endpoint),
    );
    return jobs;
});

const _settle = (endpoint: Endpoint, { jobId, autocorrections, result }: Settle): Effect.Effect<void> =>
    Effect.flatMap(SubscriptionRef.get(endpoint.link), (link) =>
        Effect.asVoid(
            Effect.transposeOption(
                Option.map(
                    Option.filter(_pending(link), (pending) => pending.jobId === jobId),
                    ({ settled }) =>
                        Deferred.done(
                            settled,
                            Result.match(result, {
                                onSuccess: (value) => Exit.succeed({ value, autocorrections }),
                                onFailure: (rejection) => Exit.fail(BridgeError.cases.hostThrew.make({ host: endpoint.host, rejection, autocorrections })),
                            }),
                        ),
                ),
            ),
        ),
    );

// --- [BOUNDARY] ------------------------------------------------------------------------

const serve = (host: SocketHost): Layer.Layer<never, never, Links | SocketServer.SocketServer> =>
    RpcServer.layer(Frames).pipe(
        Layer.provide(
            Frames.toLayer(
                Links.useSync((links) =>
                    Frames.of({
                        attach: (plugin) => _attach(links[host], plugin),
                        settle: (payload) => _settle(links[host], payload),
                        state: (state) => SubscriptionRef.update(links[host].link, Match.valueTags({ listening: identity, attached: (link): Link => ({ ...link, state: Option.some(state) }) })),
                    }),
                ),
            ),
        ),
        Layer.provide(Layer.fresh(RpcServer.layerProtocolSocketServer)),
        Layer.provide(RpcSerialization.layerJson),
    );

const linkState = (endpoint: Endpoint): Effect.Effect<LinkState> => Effect.map(SubscriptionRef.get(endpoint.link), _public);

const attached = (endpoint: Endpoint): Effect.Effect<void> => Effect.asVoid(Stream.runHead(Stream.filter(SubscriptionRef.changes(endpoint.link), Predicate.isTagged('attached'))));

const dispatch: (endpoint: Endpoint, job: Job) => Effect.Effect<Done, BridgeError> = Effect.fnUntraced(function* (endpoint: Endpoint, job: Job) {
    const settled = yield* Deferred.make<Done, BridgeError>();
    const jobs = yield* Effect.flatMap(
        SubscriptionRef.modify(
            endpoint.link,
            Match.valueTags({
                listening: (link): readonly [Result.Result<Queue.Queue<Job>, BridgeError>, Link] => [Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link],
                attached: (link): readonly [Result.Result<Queue.Queue<Job>, BridgeError>, Link] => [Result.succeed(link.jobs), { ...link, pending: Option.some({ jobId: job.jobId, settled }) }],
            }),
        ),
        Effect.fromResult,
    );
    yield* Queue.offer(jobs, job);
    return yield* Deferred.await(settled);
});

const answered = <S extends Schema.ConstraintCodec<unknown, unknown, never, never>, R>(
    endpoint: Endpoint,
    host: Jobs[SocketHost],
    timeoutMs: number,
    result: S,
    job: (jobId: JobId) => Effect.Effect<Job, BridgeError, R>,
): Effect.Effect<Answer<S['Type']>, BridgeError, R | Crypto.Crypto> =>
    Effect.flatMap(
        Effect.timed(
            run(host, timeoutMs, (jobId) =>
                Effect.andThen(
                    attached(endpoint),
                    Effect.flatMap(job(jobId), (built) => Effect.map(dispatch(endpoint, built), (done) => ({ jobId, done }))),
                ),
            ),
        ),
        ([took, { jobId, done }]) =>
            Effect.map(Effect.mapError(Schema.decodeUnknownEffect(result)(done.value), notDecodable(endpoint.host, done.value)), (value) => ({
                jobId,
                value,
                autocorrections: Option.getOrElse(done.autocorrections, () => []),
                tookMs: Duration.toMillis(took),
            })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Answer, Endpoint, Link, SocketHost };
export { answered, attached, dispatch, Links, linkState, serve };
