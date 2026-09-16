// --- [IMPORTS] -------------------------------------------------------------------------

import { Context, Deferred, Effect, Exit, Layer, Option, Queue, Ref, Result, Struct } from 'effect';
import { RpcSerialization, RpcServer } from 'effect/unstable/rpc';
import type { SocketServer } from 'effect/unstable/socket';
import { BridgeError } from './errors.ts';
import { AlreadyAttached, type Done, Frames, type Identity, type Job, type Link as LinkState, type Settle } from './frames.ts';
import type { Resolved } from './hosts.ts';
import type { JobId } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type SocketHost = Extract<Resolved, { readonly channel: 'socket' }>['id'];

type Link =
    | Extract<LinkState, { readonly _tag: 'listening' }>
    | (Extract<LinkState, { readonly _tag: 'attached' }> & {
          readonly jobs: Queue.Queue<Job>;
          readonly pending: Option.Option<{ readonly jobId: JobId; readonly settled: Deferred.Deferred<Done, BridgeError> }>;
      });

interface Endpoint {
    readonly host: SocketHost;
    readonly link: Ref.Ref<Link>;
}

type Links = Readonly<Record<SocketHost, Endpoint>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CLOSE_NORMAL = 1000;

// --- [SERVICES] ------------------------------------------------------------------------

const Links: Context.Service<Links, Links> = Context.Service<Links>('Links');

// --- [HANDLERS] ------------------------------------------------------------------------

const _attach = Effect.fnUntraced(function* (endpoint: Endpoint, identity: Identity) {
    const jobs = yield* Queue.make<Job>();
    const attached: Link = { _tag: 'attached', identity, state: Option.none(), jobs, pending: Option.none() };
    yield* Effect.acquireRelease(
        Effect.flatMap(
            Ref.modify(endpoint.link, (link): readonly [Result.Result<void, AlreadyAttached>, Link] =>
                link._tag === 'listening' ? [Result.void, attached] : [Result.fail(AlreadyAttached.make({})), link],
            ),
            Effect.fromResult,
        ),
        () =>
            Effect.flatMap(Ref.getAndSet(endpoint.link, { _tag: 'listening' }), (link) =>
                link._tag === 'listening'
                    ? Effect.void
                    : Option.match(link.pending, {
                          onNone: () => Effect.void,
                          onSome: ({ settled }) =>
                              Deferred.fail(settled, BridgeError.cases.transportClosed.make({ host: endpoint.host, code: _CLOSE_NORMAL, reason: 'plugin detached while a job was in flight' })),
                      }),
            ),
    );
    return jobs;
});

const _settle = Effect.fnUntraced(function* (endpoint: Endpoint, { jobId, autocorrections, result }: Settle) {
    const link = yield* Ref.get(endpoint.link);
    return yield* Option.match(link._tag === 'listening' ? Option.none() : Option.filter(link.pending, (pending) => pending.jobId === jobId), {
        onNone: () => Effect.void,
        onSome: ({ settled }) =>
            Deferred.done(
                settled,
                Result.match(result, {
                    onSuccess: (value) => Exit.succeed({ value, autocorrections }),
                    onFailure: (rejection) => Exit.fail(BridgeError.cases.hostThrew.make({ host: endpoint.host, rejection, autocorrections })),
                }),
            ),
    });
}, Effect.asVoid);

// --- [BOUNDARY] ------------------------------------------------------------------------

const serve = (host: SocketHost): Layer.Layer<never, never, Links | SocketServer.SocketServer> =>
    RpcServer.layer(Frames).pipe(
        Layer.provide(
            Frames.toLayer(
                Links.useSync((links) =>
                    Frames.of({
                        attach: (identity) => _attach(links[host], identity),
                        settle: (payload) => _settle(links[host], payload),
                        state: (state) => Ref.update(links[host].link, (link) => (link._tag === 'listening' ? link : { ...link, state: Option.some(state) })),
                    }),
                ),
            ),
        ),
        Layer.provide(Layer.fresh(RpcServer.layerProtocolSocketServer)),
        Layer.provide(RpcSerialization.layerJson),
    );

const linkState = (endpoint: Endpoint): Effect.Effect<LinkState> => Effect.map(Ref.get(endpoint.link), (link) => (link._tag === 'listening' ? link : Struct.omit(link, ['jobs', 'pending'])));

const dispatch: (endpoint: Endpoint, job: Job) => Effect.Effect<Done, BridgeError> = Effect.fnUntraced(function* (endpoint: Endpoint, job: Job) {
    const settled = yield* Deferred.make<Done, BridgeError>();
    const jobs = yield* Effect.flatMap(
        Ref.modify(endpoint.link, (link): readonly [Result.Result<Queue.Queue<Job>, BridgeError>, Link] =>
            link._tag === 'listening'
                ? [Result.fail(BridgeError.cases.hostNotAttached.make({ host: endpoint.host })), link]
                : [Result.succeed(link.jobs), { ...link, pending: Option.some({ jobId: job.jobId, settled }) }],
        ),
        Effect.fromResult,
    );
    yield* Queue.offer(jobs, job);
    return yield* Deferred.await(settled);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Endpoint, Link, SocketHost };
export { dispatch, Links, linkState, serve };
