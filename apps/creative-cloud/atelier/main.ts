import process from 'node:process';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { HostError } from '@rasm/automation';
import { serve } from '@rasm/mcp/node';
import { teardown } from '@rasm/process';
import { Array, Deferred, Duration, Effect, FileSystem, identity, Layer, Path, Result, Schema, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
import { ChildProcess } from 'effect/process';
import { parse } from 'plist';
import { Adobe } from '#/adobe';
import { appleEvents } from '#/features/apple-events';
import { applications, control } from '#/features/applications';
import { faces, font } from '#/features/fonts';
import { plugins } from '#/features/plugins';
import { typesetting } from '#/features/typesetting';
import { listening } from '#/libraries';
import manifest from '#/package.json' with { type: 'json' };
import { routes } from '#/routes';
import { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this server' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.fnUntraced(function* (ended: Deferred.Deferred<never, HostError>) {
    const [adobe, links, dependencies] = yield* Effect.all([Adobe, listening, status]);
    yield* Effect.match(adobe.exited, { onSuccess: (code) => `exit code ${code}`, onFailure: identity }).pipe(
        Effect.flatMap((cause) => HostError.make({ reason: { _tag: 'Unavailable' }, subject: 'Atelier host', cause })),
        Deferred.into(ended),
        Effect.forkScoped,
    );
    yield* Effect.all([applications, faces], { discard: true });
    const round = Effect.flatMap(routes(dependencies, links), (routed) => Effect.all([control(routed, dependencies), appleEvents(routed), font(routed), typesetting(routed), plugins(routed, links)], { discard: true }));
    const discovered = SubscriptionRef.changes(adobe.installations).pipe(
        Stream.tap(Result.match({ onSuccess: () => Effect.void, onFailure: (error) => Effect.logError('Adobe application discovery failed, tools keep the last installations', error) })),
        Stream.filterMap(identity),
        Stream.map(Array.map((row) => [row.name, row.url.href] as const)),
        Stream.changes,
    );
    const [first, rest] = yield* Stream.peel(discovered, Sink.head());
    const generation = yield* ScopedRef.fromAcquire(Effect.forEach(first, () => round, { discard: true }));
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(links.changes, 1)), () => ScopedRef.set(generation, round)));
});
Effect.gen(function* () {
    const [path, files, ended] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Deferred.make<never, HostError>()]);
    const contents = path.join(path.dirname(process.execPath), '..');
    const { CFBundleExecutable, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
        Effect.flatMap((text) => Effect.try(() => parse(text))),
        Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
    );
    return yield* serve(
        { name: manifest.name, description: manifest.description, version: CFBundleShortVersionString },
        {
            retention: Duration.hours(1),
            listen: Peer.link.endpoint,
            instructions: [
                'Specifiers come from results',
                'State-changing calls return early when a dialog requires an answer',
                'screenshot reads every window of a background application with no activation',
                'execute_script runs a body as written, activate in a body takes focus from the user and reports active on the call',
                'A blocking window is a new window on a blocked main thread, perform answers it with a key action, its leaving alone proves the answer, else a person answers it',
            ],
        },
        Layer.effectDiscard(tools(ended)).pipe(Layer.provide(Layer.provideMerge(Adobe.layer, Automation.layer(ChildProcess.make(path.join(contents, 'MacOS', CFBundleExecutable), { stderr: 'inherit' })))), Layer.provide(layerDigest)),
        Deferred.await(ended),
    );
}).pipe(Effect.provide(layer), runMain({ teardown }));
