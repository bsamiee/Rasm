// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeSocketServer } from '@effect/platform-node';
import {
    Array,
    Cache,
    Cause,
    Clock,
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
    Schema,
    Stream,
    Struct,
    SubscriptionRef,
    Tuple,
} from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import { RpcClient, type RpcClientError, type RpcGroup, RpcSerialization, RpcServer } from 'effect/unstable/rpc';
import { Socket, SocketServer } from 'effect/unstable/socket';
import { BridgeError, notDecodable } from './errors.ts';
import { type Activity, AlreadyAttached, Broker, type Completion, Execute, Frames, type Identity, type Job, Link as LinkFrame, Outcome, type Request, type Settle } from './frames.ts';
import { type Hosts, installed, layer as resolved } from './hosts.ts';
import { type Host, Jobs, probe, layer as queues, request, submit } from './jobs.ts';
import { read } from './osascript.ts';
import { type JobId, LOOPBACK, QUEUE_DEPTH, SOCKETS, type SocketHost } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Pending {
    readonly jobId: JobId;
    readonly settled: Deferred.Deferred<(typeof Completion)['Type'], BridgeError>;
}

type Link =
    | { readonly _tag: 'opening' }
    | Exclude<LinkFrame, { readonly _tag: 'attached' }>
    | (Extract<LinkFrame, { readonly _tag: 'attached' }> & { readonly clientId: number; readonly jobs: Queue.Queue<Job>; readonly pending: Option.Option<Pending> })
    | {
          readonly _tag: 'shared';
          readonly client: RpcClient.RpcClient<RpcGroup.Rpcs<typeof Broker>, RpcClientError.RpcClientError>;
          readonly state: LinkFrame;
          readonly activity: (typeof Activity)['Type'];
      };

interface Endpoint {
    readonly host: SocketHost;
    readonly link: SubscriptionRef.SubscriptionRef<Link>;
    readonly outcomes: Cache.Cache<JobId, Deferred.Deferred<(typeof Completion)['Type'], BridgeError>>;
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

const _pending: (link: Link) => Option.Option<Pending> = Match.valueTags({ opening: Option.none, unbound: Option.none, listening: Option.none, shared: Option.none, attached: Struct.get('pending') });

const _state: (link: Link) => LinkFrame = Match.valueTags({
    opening: () => LinkFrame.cases.listening.make({}),
    unbound: identity,
    listening: identity,
    shared: Struct.get('state'),
    attached: Struct.omit(['clientId', 'jobs', 'pending']),
});

const _detach = (endpoint: Endpoint, clientId: number): Effect.Effect<void> =>
    Effect.flatMap(
        SubscriptionRef.modify(endpoint.link, (link) =>
            link._tag === 'attached' && link.clientId === clientId ? Tuple.make(link.pending, LinkFrame.cases.listening.make({})) : Tuple.make(Option.none<Pending>(), link),
        ),
        Option.match({
            onNone: () => Effect.void,
            onSome: ({ jobId, settled }: Pending) => Effect.asVoid(Deferred.fail(settled, BridgeError.cases.pluginDetached.make({ host: endpoint.host, jobId }))),
        }),
    );

const _attach = Effect.fnUntraced(function* (endpoint: Endpoint, plugin: Identity, clientId: number) {
    const jobs = yield* Queue.make<Job>();
    const attached: Link = { _tag: 'attached', identity: plugin, state: Option.none(), clientId, jobs, pending: Option.none() };
    yield* Effect.acquireRelease(
        Effect.flatMap(
            SubscriptionRef.modify<Link, Result.Result<void, AlreadyAttached>>(
                endpoint.link,
                Match.valueTags({
                    opening: () => Tuple.make(Result.void, attached),
                    unbound: () => Tuple.make(Result.void, attached),
                    listening: () => Tuple.make(Result.void, attached),
                    shared: (link) => Tuple.make(Result.fail(AlreadyAttached.make({})), link),
                    attached: (link) => Tuple.make(Result.fail(AlreadyAttached.make({})), link),
                }),
            ),
            Effect.fromResult,
        ),
        () => _detach(endpoint, clientId),
    );
    return jobs;
});

const _settle = Effect.fnUntraced(function* (endpoint: Endpoint, { jobId, autocorrections, result }: Settle, clientId: number) {
    const link = yield* SubscriptionRef.get(endpoint.link);
    if (link._tag !== 'attached' || link.clientId !== clientId) {
        return;
    }
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

const serve = (host: SocketHost): Layer.Layer<never, never, Links | Jobs | Crypto.Crypto | SocketServer.SocketServer | ChildProcessSpawner.ChildProcessSpawner> =>
    RpcServer.layer(Frames.merge(Broker)).pipe(
        Layer.provide(
            Frames.toLayer(
                Links.useSync((links) =>
                    Frames.of({
                        attach: (plugin, { client }) => _attach(links[host], plugin, client.id),
                        settle: (payload, { client }) => _settle(links[host], payload, client.id),
                        state: (state, { client }) =>
                            Effect.andThen(
                                Stream.runHead(Stream.filter(SubscriptionRef.changes(links[host].link), (link) => link._tag === 'attached' && link.clientId === client.id)),
                                SubscriptionRef.update(links[host].link, (link) => (link._tag === 'attached' && link.clientId === client.id ? { ...link, state: Option.some(state) } : link)),
                            ),
                    }),
                ),
            ),
        ),
        Layer.provide(
            Broker.toLayer(
                Effect.map(Effect.all([Links, Jobs]), ([links, jobs]) => ({
                    submit: ({ job, deadlineAt }) => _submitted({ link: links[host], host: jobs[host] }, job, deadlineAt),
                    probe: ({ statement, ...entry }) => probing({ link: links[host], host: jobs[host] }, entry, statement),
                    outcome: (jobId) => outcome(links[host], jobId),
                    observe: () =>
                        Stream.zipLatestWith(SubscriptionRef.changes(links[host].link), SubscriptionRef.changes(jobs[host].activity), (link, current) => ({ link: _state(link), activity: current })),
                })),
            ),
        ),
        Layer.provide(Layer.fresh(RpcServer.layerProtocolSocketServer)),
        Layer.provide(RpcSerialization.layerJson),
    );

const open = (host: SocketHost): Effect.Effect<Endpoint> =>
    Effect.map(
        Effect.all([
            SubscriptionRef.make<Link>({ _tag: 'opening' }),
            Cache.make<JobId, Deferred.Deferred<(typeof Completion)['Type'], BridgeError>>({ capacity: QUEUE_DEPTH, lookup: () => Deferred.make<(typeof Completion)['Type'], BridgeError>() }),
        ]),
        ([link, outcomes]) => ({ host, link, outcomes }),
    );

const _shared = (endpoint: Endpoint): Effect.Effect<void> =>
    Effect.gen(function* () {
        const client = yield* RpcClient.make(Broker);
        yield* Stream.runForEach(client.observe(), ({ link: state, activity: current }) => SubscriptionRef.set(endpoint.link, { _tag: 'shared', client, state, activity: current })).pipe(
            Effect.catchCauseIf(Cause.hasInterruptsOnly, () => Effect.void),
        );
    }).pipe(
        Effect.scoped,
        Effect.provide(
            RpcClient.layerProtocolSocket().pipe(
                Layer.provide(Socket.layerWebSocket(`ws://${LOOPBACK.bound}:${SOCKETS[endpoint.host].port}`)),
                Layer.provide(Socket.layerWebSocketConstructorGlobal),
                Layer.provide(RpcSerialization.layerJson),
            ),
        ),
        Effect.catchTag('RpcClientError', () => Effect.void),
        Effect.ensuring(SubscriptionRef.set(endpoint.link, { _tag: 'opening' })),
    );

const _occupied = Schema.is(Schema.Struct({ code: Schema.Literal('EADDRINUSE') }));

const listen = (host: SocketHost): Effect.Effect<never, SocketServer.SocketServerError, Links | Jobs | Crypto.Crypto | ChildProcessSpawner.ChildProcessSpawner> =>
    Effect.flatMap(Links, (links) =>
        Layer.launch(
            Layer.provide(
                Layer.provide(serve(host), Layer.effectDiscard(Effect.andThen(SocketServer.SocketServer, SubscriptionRef.set(links[host].link, LinkFrame.cases.listening.make({}))))),
                NodeSocketServer.layerWebSocket({ host: LOOPBACK.bound, port: SOCKETS[host].port }),
            ),
        ).pipe(
            Effect.catchIf(
                (error) => error.reason._tag === 'SocketServerOpenError' && _occupied(error.reason.cause),
                () => _shared(links[host]),
            ),
            Effect.tapError((error) => SubscriptionRef.set(links[host].link, LinkFrame.cases.unbound.make({ port: SOCKETS[host].port, reason: error.reason._tag, cause: error.reason.cause }))),
            Effect.forever,
        ),
    );

const layer = (hosts: readonly SocketHost[]): Layer.Layer<Links | Jobs | Hosts, Config.ConfigError, ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto | FileSystem.FileSystem | Path.Path> =>
    Layer.provideMerge(
        Layer.provideMerge(Layer.mergeAll(Layer.empty, ...Array.map(hosts, (host) => Layer.effectDiscard(Effect.forkScoped(listen(host))))), queues),
        Layer.mergeAll(Layer.effect(Links, Effect.all(Record.map(SOCKETS, (row) => open(row.id)))), resolved),
    );

const linkState = (endpoint: Endpoint): Effect.Effect<LinkFrame> => Effect.map(SubscriptionRef.get(endpoint.link), _state);

const activity = (channel: Session): Effect.Effect<(typeof Activity)['Type']> =>
    Effect.flatMap(SubscriptionRef.get(channel.link.link), (link) => (link._tag === 'shared' ? Effect.succeed(link.activity) : SubscriptionRef.get(channel.host.activity)));

const attached = (endpoint: Endpoint): Effect.Effect<void> => Effect.asVoid(Stream.runHead(Stream.filter(Stream.map(SubscriptionRef.changes(endpoint.link), _state), Predicate.isTagged('attached'))));

const dispatch: (endpoint: Endpoint, job: Job) => Effect.Effect<(typeof Completion)['Type'], BridgeError> = Effect.fnUntraced(function* (endpoint: Endpoint, job: Job) {
    const existing = yield* Cache.getOption(endpoint.outcomes, job.jobId);
    if (Option.isSome(existing)) {
        return yield* Deferred.await(existing.value);
    }
    const previous = yield* Effect.map(SubscriptionRef.get(endpoint.link), _pending);
    yield* Option.match(previous, { onNone: () => Effect.void, onSome: (pending) => Effect.asVoid(Effect.exit(Deferred.await(pending.settled))) });
    const settled = yield* Cache.get(endpoint.outcomes, job.jobId);
    const jobs = yield* Effect.flatMap(
        SubscriptionRef.modify<Link, Result.Result<Queue.Queue<Job>, BridgeError>>(
            endpoint.link,
            Match.valueTags({
                opening: (link) => Tuple.make(Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link),
                unbound: (link) => Tuple.make(Result.fail(BridgeError.cases.portNotBound.make({ host: endpoint.host, port: link.port, cause: link.cause })), link),
                listening: (link) => Tuple.make(Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link),
                shared: (link) => Tuple.make(Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link),
                attached: (link) => Tuple.make(Result.succeed(link.jobs), { ...link, pending: Option.some({ jobId: job.jobId, settled }) }),
            }),
        ),
        Effect.fromResult,
    ).pipe(Effect.tapError((error) => Deferred.fail(settled, error)));
    yield* Queue.offer(jobs, job);
    return yield* Deferred.await(settled);
});

const outcome: (endpoint: Endpoint, jobId: JobId) => Effect.Effect<(typeof Outcome)['Type'], BridgeError> = Effect.fnUntraced(function* (endpoint: Endpoint, jobId: JobId) {
    const link = yield* Effect.flatMap(Stream.runHead(Stream.filter(SubscriptionRef.changes(endpoint.link), (state) => state._tag !== 'opening')), Effect.fromOption).pipe(Effect.orDie);
    if (link._tag === 'shared') {
        return yield* link.client.outcome(jobId).pipe(Effect.catchTag('RpcClientError', () => Effect.fail(BridgeError.cases.pluginDetached.make({ host: endpoint.host, jobId }))));
    }
    const retained = yield* Cache.getOption(endpoint.outcomes, jobId);
    if (Option.isNone(retained)) {
        return Outcome.cases.unknown.make({});
    }
    const completed = yield* Deferred.poll(retained.value);
    return yield* Option.match(completed, {
        onNone: () => Effect.succeed(Outcome.cases.pending.make({})),
        onSome: (settled) => Effect.map(Effect.result(settled), (result) => Outcome.cases.settled.make({ result })),
    });
});

const _submitted = Effect.fnUntraced(
    function* (channel: Session, job: Job, deadlineAt: number) {
        const retained = yield* Cache.getOption(channel.link.outcomes, job.jobId);
        if (Option.isSome(retained)) {
            return yield* Deferred.await(retained.value);
        }
        const link = yield* Effect.flatMap(
            Stream.runHead(Stream.filter(SubscriptionRef.changes(channel.link.link), (state) => state._tag !== 'opening' && state._tag !== 'listening')),
            Effect.fromOption,
        ).pipe(Effect.orDie);
        return yield* Match.value(link).pipe(
            Match.tagsExhaustive({
                unbound: (failed) => Effect.fail(BridgeError.cases.portNotBound.make({ host: channel.link.host, port: failed.port, cause: failed.cause })),
                shared: (shared) =>
                    shared.client
                        .submit({ job, deadlineAt })
                        .pipe(Effect.catchTag('RpcClientError', () => Effect.fail(BridgeError.cases.pluginDetached.make({ host: channel.link.host, jobId: job.jobId })))),
                attached: () => submit(channel.host, { jobId: job.jobId, deadlineAt }, dispatch(channel.link, job)),
            }),
        );
    },
    (work, channel, job, deadlineAt) =>
        Effect.flatMap(Clock.currentTimeMillis, (at) =>
            work.pipe(
                Effect.timeoutOrElse({
                    duration: Duration.millis(deadlineAt - at),
                    orElse: () => Effect.fail(BridgeError.cases.deadlineExceeded.make({ host: channel.link.host, jobId: job.jobId })),
                }),
            ),
        ),
);

const execution =
    (code: string) =>
    (jobId: JobId): Job => ({ jobId, kind: 'execute', body: Schema.encodeSync(Schema.toCodecJson(Execute))({ code, undoName: Option.none() }), ...READ });

const probing: (
    channel: Session,
    entry: (typeof Request)['Type'],
    statement: Option.Option<string>,
) => Effect.Effect<Result.Result<Schema.Json, BridgeError>, never, ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(function* (
    channel: Session,
    entry: (typeof Request)['Type'],
    statement: Option.Option<string>,
) {
    const native = Effect.gen(function* () {
        if (Option.isNone(statement)) {
            return (yield* dispatch(channel.link, execution('1')(entry.jobId))).value;
        }
        const { bundlePath } = yield* installed(channel.host.id, channel.host.resolved);
        return yield* read(channel.link.host, bundlePath, entry.deadlineAt - (yield* Clock.currentTimeMillis), statement.value, Option.none());
    });
    return yield* Effect.flatMap(
        Effect.flatMap(Stream.runHead(Stream.filter(SubscriptionRef.changes(channel.link.link), (state) => state._tag !== 'opening')), Effect.fromOption).pipe(Effect.orDie),
        (link) =>
            link._tag === 'shared'
                ? link.client
                      .probe({ ...entry, statement })
                      .pipe(Effect.catchTag('RpcClientError', () => Effect.succeed(Result.fail(BridgeError.cases.hostNotAttached.make({ host: channel.link.host })))))
                : probe(channel.host, entry, native),
    ).pipe(
        Effect.timeoutOrElse({
            duration: Duration.millis(entry.deadlineAt - (yield* Clock.currentTimeMillis)),
            orElse: () => Effect.succeed(Result.fail(BridgeError.cases.deadlineExceeded.make({ host: channel.link.host, jobId: entry.jobId }))),
        }),
    );
});

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
    const { jobId, deadlineAt } = yield* request(timeoutMs);
    return yield* Effect.gen(function* () {
        const built = yield* job(jobId);
        const done = yield* _submitted(channel, built, deadlineAt);
        const value = yield* Effect.mapError(Schema.decodeUnknownEffect(Schema.toCodecJson(result))(done.value), notDecodable(channel.link.host, done.value));
        return { jobId, value, autocorrections: Option.getOrElse(done.autocorrections, () => []), tookMs: (yield* Clock.currentTimeMillis) - deadlineAt + timeoutMs };
    }).pipe(
        Effect.timeoutOrElse({
            duration: Duration.millis(deadlineAt - (yield* Clock.currentTimeMillis)),
            orElse: () => Effect.fail(BridgeError.cases.deadlineExceeded.make({ host: channel.link.host, jobId })),
        }),
    );
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

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Answer, Endpoint, Link, Scope, Session };
export { activity, answered, attached, dispatch, execution, Links, layer, linkState, listen, open, outcome, prepared, probing, READ, session };
