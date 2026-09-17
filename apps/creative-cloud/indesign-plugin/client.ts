// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Event, MeasurementUnits, UserInteractionLevels } from 'adobe:indesign';
import { host, versions } from 'adobe:uxp';
import type { HostRejection } from '@rasm/creative-cloud-server/errors';
import { Frames, type Identity, type Job, type State } from '@rasm/creative-cloud-server/frames';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import { Array, Cause, Effect, Layer, Option, Predicate, Queue, type Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { RpcClient, type RpcClientError, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { execute } from './jobs/execute.ts';
import { manifest } from './uxp.config.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _JOBS: { readonly [Kind in Job['kind']]: (body: Schema.Json) => Effect.Effect<Schema.Json, HostRejection> } = { execute };

const _EVENTS = ['afterContextChanged', 'afterSelectionChanged', 'afterOpen', 'afterClose', 'afterNew', 'afterSave'];

const _CLOSE_NORMAL = 1000;

// --- [HOST] ----------------------------------------------------------------------------

const _identity = (): Identity => ({
    plugin: manifest.id,
    version: versions.plugin,
    host: { name: host.name, version: host.version },
    uxp: versions.uxp,
    app: Option.some(app.version),
    dom: Option.some(app.scriptPreferences.version),
});

const _state = (): State => {
    const active = app.documents.length === 0 ? Option.none() : Option.some(app.activeDocument);
    return { modalState: app.modalState, activeDocumentId: Option.map(active, Struct.get('id')), modified: Option.exists(active, Struct.get('modified')) };
};

const _states: Stream.Stream<State> = Stream.map(
    Stream.mergeAll(
        Array.map(_EVENTS, (type) => Stream.fromEventListener<Event>(app, type)),
        { concurrency: 'unbounded' },
    ),
    _state,
);

const _executor = Effect.acquireRelease(
    Effect.sync(() => {
        const preferences = app.scriptPreferences;
        const saved = { userInteractionLevel: preferences.userInteractionLevel, measurementUnit: preferences.measurementUnit };
        preferences.userInteractionLevel = UserInteractionLevels.NEVER_INTERACT;
        preferences.measurementUnit = MeasurementUnits.POINTS;
        return saved;
    }),
    (saved) =>
        Effect.sync(() => {
            app.scriptPreferences.userInteractionLevel = saved.userInteractionLevel;
            app.scriptPreferences.measurementUnit = saved.measurementUnit;
        }),
);

// --- [LINK] ----------------------------------------------------------------------------

const _mark = (status: SubscriptionRef.SubscriptionRef<string>, text: string): Effect.Effect<void> => SubscriptionRef.set(status, `${manifest.version} › ${text}`);

const _traced =
    (status: SubscriptionRef.SubscriptionRef<string>, label: string) =>
    <A, E, R>(effect: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
        Effect.tapCauseIf(effect, Predicate.not(Cause.hasInterruptsOnly), (cause) => _mark(status, `${label}: ${Cause.pretty(cause)}`));

const _attached = (client: RpcClient.FromGroup<typeof Frames, RpcClientError.RpcClientError>, status: SubscriptionRef.SubscriptionRef<string>): Effect.Effect<void> =>
    _mark(status, 'Attached').pipe(
        Effect.andThen(
            Stream.runDrain(
                Stream.merge(
                    Stream.mapEffect(client.attach(_identity()), (job) =>
                        Effect.scoped(
                            Effect.flatMap(Effect.andThen(_executor, Effect.result(_JOBS[job.kind](job.body))), (result) =>
                                client.settle({ jobId: job.jobId, autocorrections: Option.none(), result }),
                            ),
                        ),
                    ),
                    Stream.mapEffect(_states, (state) => client.state(state)),
                ),
            ),
        ),
        Effect.andThen(_mark(status, 'Detached')),
        Effect.catchCause((cause) => _mark(status, Cause.hasInterruptsOnly(cause) ? 'Detached' : `Detached: ${Cause.pretty(cause)}`)),
    );

const run = (status: SubscriptionRef.SubscriptionRef<string>): Effect.Effect<void> =>
    Effect.gen(function* () {
        const connections = yield* Queue.unbounded<'connected' | 'disconnected'>();
        const hooks = Layer.succeed(RpcClient.ConnectionHooks, {
            onConnect: Effect.andThen(_mark(status, 'Connected'), Effect.asVoid(Queue.offer(connections, 'connected'))),
            onDisconnect: Effect.andThen(_mark(status, 'Disconnected'), Effect.asVoid(Queue.offer(connections, 'disconnected'))),
        });
        const socket = Socket.fromWebSocket(
            Effect.acquireRelease(
                Effect.try({
                    try: () => new WebSocket(`ws://localhost:${HOSTS.indesign.port}`),
                    catch: (cause) => new Socket.SocketError({ reason: new Socket.SocketOpenError({ kind: 'Unknown', cause }) }),
                }),
                (ws) =>
                    Effect.sync(() => {
                        ws.close(_CLOSE_NORMAL);
                    }),
            ),
        );
        const protocol = RpcClient.layerProtocolSocket({ retryTransientErrors: true, onTransientError: (error) => _mark(status, `Retrying: ${error.message}`) }).pipe(
            Layer.provide(hooks),
            Layer.provide(Layer.effect(Socket.Socket, socket)),
            Layer.provide(RpcSerialization.layerJson),
        );
        yield* Effect.flatMap(RpcClient.make(Frames), (client) =>
            Stream.runDrain(Stream.switchMap(Stream.fromQueue(connections), (connection) => (connection === 'connected' ? Stream.fromEffect(_attached(client, status)) : Stream.empty))),
        ).pipe(Effect.scoped, Effect.provide(protocol));
    }).pipe(_traced(status, 'Stopped'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
