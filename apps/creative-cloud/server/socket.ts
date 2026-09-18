// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeSocketServer } from '@effect/platform-node';
import {
    Array,
    type Config,
    Context,
    type Crypto,
    Deferred,
    Duration,
    Effect,
    Exit,
    type FileSystem,
    identity,
    Layer,
    Match,
    Option,
    type Path,
    Predicate,
    Queue,
    Record,
    Result,
    Schedule,
    Schema,
    Stream,
    Struct,
    SubscriptionRef,
    Tuple,
} from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { RpcSerialization, RpcServer } from 'effect/unstable/rpc';
import { SocketServer } from 'effect/unstable/socket';
import { BridgeError, notDecodable } from './errors.ts';
import { AlreadyAttached, Execute, Frames, type Identity, type Job, Link as LinkFrame, type Settle } from './frames.ts';
import { type Hosts, layer as resolved } from './hosts.ts';
import { type Host, Jobs, probe, layer as queues, run } from './jobs.ts';
import { type JobId, LOOPBACK, PROBE_MS, SOCKETS, type SocketHost } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Done = Pick<Settle, 'autocorrections'> & { readonly value: Schema.Json };

interface Pending {
    readonly jobId: JobId;
    readonly settled: Deferred.Deferred<Done, BridgeError>;
}

type Link = Exclude<LinkFrame, { readonly _tag: 'attached' }> | (Extract<LinkFrame, { readonly _tag: 'attached' }> & { readonly jobs: Queue.Queue<Job>; readonly pending: Option.Option<Pending> });

interface Endpoint {
    readonly host: SocketHost;
    readonly link: SubscriptionRef.SubscriptionRef<Link>;
}

interface Session {
    readonly link: Endpoint;
    readonly host: Host;
}

type Links = Readonly<Record<SocketHost, Endpoint>>;

type Scope = Pick<Job, 'suspendHistory' | 'commandName'>;

interface Answer<Value> {
    readonly jobId: JobId;
    readonly value: Value;
    readonly autocorrections: readonly string[];
    readonly tookMs: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ: Scope = { suspendHistory: Option.none(), commandName: Option.none() };

// --- [SERVICES] ------------------------------------------------------------------------

const Links: Context.Service<Links, Links> = Context.Service<Links>('Links');

const session = (host: SocketHost): { readonly tag: Context.Service<Session, Session>; readonly layer: Layer.Layer<Session, never, Links | Jobs> } => {
    const tag = Context.Service<Session>(`Session/${host}`);
    return {
        tag,
        layer: Layer.effect(
            tag,
            Effect.map(Effect.all([Links, Jobs]), ([links, jobs]) => ({ link: links[host], host: jobs[host] })),
        ),
    };
};

// --- [STATE] ---------------------------------------------------------------------------

const _pending: (link: Link) => Option.Option<Pending> = Match.valueTags({ unbound: Option.none, listening: Option.none, attached: Struct.get('pending') });

const _detach = (endpoint: Endpoint): Effect.Effect<void> =>
    Effect.flatMap(SubscriptionRef.getAndSet(endpoint.link, LinkFrame.cases.listening.make({})), (link) =>
        Option.match(_pending(link), {
            onNone: () => Effect.void,
            onSome: ({ jobId, settled }) => Effect.asVoid(Deferred.fail(settled, BridgeError.cases.pluginDetached.make({ host: endpoint.host, jobId }))),
        }),
    );

const _attach = Effect.fnUntraced(function* (endpoint: Endpoint, plugin: Identity) {
    const jobs = yield* Queue.make<Job>();
    const attached: Link = { _tag: 'attached', identity: plugin, state: Option.none(), jobs, pending: Option.none() };
    const claimed = yield* SubscriptionRef.modify<Link, Result.Result<void, AlreadyAttached>>(
        endpoint.link,
        Match.valueTags({
            unbound: () => Tuple.make(Result.void, attached),
            listening: () => Tuple.make(Result.void, attached),
            attached: (link) => Tuple.make(Result.fail(AlreadyAttached.make({})), link),
        }),
    );
    yield* Effect.acquireRelease(Effect.fromResult(claimed), () => _detach(endpoint));
    return jobs;
});

const _settle = Effect.fnUntraced(function* (endpoint: Endpoint, { jobId, autocorrections, result }: Settle) {
    const link = yield* SubscriptionRef.get(endpoint.link);
    const exit = Result.match(result, {
        onSuccess: (value) => Exit.succeed({ value, autocorrections }),
        onFailure: (rejection) => Exit.fail(BridgeError.cases.hostThrew.make({ host: endpoint.host, rejection, autocorrections })),
    });
    yield* Option.match(
        Option.filter(_pending(link), (pending) => pending.jobId === jobId),
        { onNone: () => Effect.void, onSome: ({ settled }) => Effect.asVoid(Deferred.done(settled, exit)) },
    );
});

// --- [BOUNDARY] ------------------------------------------------------------------------

const serve = (host: SocketHost): Layer.Layer<never, never, Links | SocketServer.SocketServer> =>
    RpcServer.layer(Frames).pipe(
        Layer.provide(
            Frames.toLayer(
                Links.useSync((links) =>
                    Frames.of({
                        attach: (plugin) => _attach(links[host], plugin),
                        settle: (payload) => _settle(links[host], payload),
                        state: (state) =>
                            SubscriptionRef.update(links[host].link, Match.valueTags({ unbound: identity, listening: identity, attached: (link): Link => ({ ...link, state: Option.some(state) }) })),
                    }),
                ),
            ),
        ),
        Layer.provide(Layer.fresh(RpcServer.layerProtocolSocketServer)),
        Layer.provide(RpcSerialization.layerJson),
    );

const open = (host: SocketHost): Effect.Effect<Endpoint> => Effect.map(SubscriptionRef.make<Link>(LinkFrame.cases.listening.make({})), (link) => ({ host, link }));

const listen = (host: SocketHost): Effect.Effect<never, SocketServer.SocketServerError, Links> =>
    Effect.flatMap(Links, (links) =>
        Layer.launch(
            Layer.provide(
                Layer.merge(serve(host), Layer.effectDiscard(Effect.andThen(SocketServer.SocketServer, SubscriptionRef.set(links[host].link, LinkFrame.cases.listening.make({}))))),
                NodeSocketServer.layerWebSocket({ host: LOOPBACK.bound, port: SOCKETS[host].port }),
            ),
        ).pipe(
            Effect.tapError((error) => SubscriptionRef.set(links[host].link, LinkFrame.cases.unbound.make({ port: SOCKETS[host].port, reason: error.reason._tag, cause: error.reason.cause }))),
            Effect.retry(Schedule.spaced(Duration.millis(PROBE_MS))),
        ),
    );

const layer = (hosts: readonly SocketHost[]): Layer.Layer<Links | Jobs | Hosts, Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path> =>
    Layer.provideMerge(
        Layer.mergeAll(queues, ...Array.map(hosts, (host) => Layer.effectDiscard(Effect.forkScoped(listen(host))))),
        Layer.mergeAll(Layer.effect(Links, Effect.all(Record.map(SOCKETS, (row) => open(row.id)))), resolved),
    );

const linkState = (endpoint: Endpoint): Effect.Effect<LinkFrame> =>
    Effect.map(SubscriptionRef.get(endpoint.link), (link): LinkFrame => Match.valueTags(link, { unbound: identity, listening: identity, attached: Struct.omit(['jobs', 'pending']) }));

const attached = (endpoint: Endpoint): Effect.Effect<void> => Effect.asVoid(Stream.runHead(Stream.filter(SubscriptionRef.changes(endpoint.link), Predicate.isTagged('attached'))));

const dispatch: (endpoint: Endpoint, job: Job) => Effect.Effect<Done, BridgeError> = Effect.fnUntraced(function* (endpoint: Endpoint, job: Job) {
    const settled = yield* Deferred.make<Done, BridgeError>();
    const jobs = yield* Effect.flatMap(
        SubscriptionRef.modify<Link, Result.Result<Queue.Queue<Job>, BridgeError>>(
            endpoint.link,
            Match.valueTags({
                unbound: (link) => Tuple.make(Result.fail(BridgeError.cases.portNotBound.make({ host: endpoint.host, port: link.port, cause: link.cause })), link),
                listening: (link) => Tuple.make(Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link),
                attached: (link) => Tuple.make(Result.succeed(link.jobs), { ...link, pending: Option.some({ jobId: job.jobId, settled }) }),
            }),
        ),
        Effect.fromResult,
    );
    yield* Queue.offer(jobs, job);
    return yield* Deferred.await(settled);
});

const execution =
    (code: string) =>
    (jobId: JobId): Job => ({ jobId, kind: 'execute', body: Schema.encodeSync(Schema.toCodecJson(Execute))({ code, undoName: Option.none() }), ...READ });

const probing = (channel: Session): Effect.Effect<Result.Result<Schema.Json, BridgeError>, never, Crypto.Crypto> =>
    probe(channel.host, (jobId) => Effect.map(dispatch(channel.link, execution('1')(jobId)), Struct.get('value')));

const answered: <S extends Schema.ConstraintCodec<unknown, unknown, never, never>, R>(
    channel: Session,
    timeoutMs: number,
    result: S,
    job: (jobId: JobId) => Effect.Effect<Job, BridgeError, R>,
) => Effect.Effect<Answer<S['Type']>, BridgeError, R | Crypto.Crypto> = Effect.fnUntraced(function* <S extends Schema.ConstraintCodec<unknown, unknown, never, never>, R>(
    channel: Session,
    timeoutMs: number,
    result: S,
    job: (jobId: JobId) => Effect.Effect<Job, BridgeError, R>,
) {
    const [took, { jobId, done }] = yield* Effect.timed(
        run(
            channel.host,
            timeoutMs,
            Effect.fnUntraced(function* (id: JobId) {
                yield* attached(channel.link);
                const built = yield* job(id);
                const settled = yield* dispatch(channel.link, built);
                return { jobId: id, done: settled };
            }),
        ),
    );
    const value = yield* Effect.mapError(Schema.decodeUnknownEffect(result)(done.value), notDecodable(channel.link.host, done.value));
    return { jobId, value, autocorrections: Option.getOrElse(done.autocorrections, () => []), tookMs: Duration.toMillis(took) };
});

const prepared =
    <Fields extends Record<string, Schema.ConstraintCodec<unknown, unknown, never, never>>>(bodies: Schema.Struct<Fields>) =>
    <K extends keyof Fields & string, S extends Schema.ConstraintCodec<unknown, unknown, never, never>, R>(
        channel: Session,
        timeoutMs: number,
        kind: K,
        prepare: (jobId: JobId) => Effect.Effect<Fields[K]['Type'], BridgeError, R>,
        result: S,
        scope: Scope,
    ): Effect.Effect<Answer<S['Type']>, BridgeError, R | Crypto.Crypto> =>
        answered(
            channel,
            timeoutMs,
            result,
            Effect.fnUntraced(function* (jobId: JobId) {
                const value = yield* prepare(jobId);
                return { jobId, kind, body: Schema.encodeSync(Schema.toCodecJson(Struct.get(bodies.fields, kind)))(value), ...scope };
            }),
        );

const answer =
    <Fields extends Record<string, Schema.ConstraintCodec<unknown, unknown, never, never>>>(bodies: Schema.Struct<Fields>) =>
    <K extends keyof Fields & string, S extends Schema.ConstraintCodec<unknown, unknown, never, never>>(
        channel: Session,
        timeoutMs: number,
        kind: K,
        value: Fields[K]['Type'],
        result: S,
        scope: Scope,
    ): Effect.Effect<Answer<S['Type']>, BridgeError, Crypto.Crypto> =>
        prepared(bodies)(channel, timeoutMs, kind, () => Effect.succeed(value), result, scope);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Answer, Endpoint, Link, Scope, Session };
export { answer, answered, attached, dispatch, execution, Links, layer, linkState, listen, open, prepared, probing, READ, session };
