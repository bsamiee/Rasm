import process from 'node:process';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { serve } from '@rasm/mcp/node';
import { Array, Duration, Effect, Equal, FileSystem, identity, Layer, Option, Path, Result, Schema, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
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

const tools = Effect.gen(function* () {
    const [adobe, links, dependencies] = yield* Effect.all([Adobe, listening, status]);
    yield* adobe.exited.pipe(
        Effect.exit,
        Effect.flatMap((exit) => Effect.logError('Atelier host ended', exit)),
        Effect.andThen(Effect.sync(() => process.exit(1))),
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
    const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const contents = path.join(path.dirname(process.execPath), '..');
    const [{ CFBundleExecutable, CFBundleShortVersionString }, own] = yield* Effect.all([
        files.readFileString(path.join(contents, 'Info.plist')).pipe(
            Effect.flatMap((text) => Effect.try(() => parse(text))),
            Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
        ),
        files.stat(process.execPath),
    ]);
    const replaced = files.watch(process.execPath).pipe(
        Stream.mapEffect(() => Effect.option(files.stat(process.execPath))),
        Stream.filter(Option.match({ onNone: () => true, onSome: ({ dev, ino }) => dev !== own.dev || !Equal.equals(ino, own.ino) })),
        Stream.runHead,
        Effect.andThen(Effect.logInfo('Server executable replaced, exiting for launchd to start the installed build')),
    );
    return yield* Effect.raceFirst(
        serve(
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
            Layer.effectDiscard(tools).pipe(Layer.provide(Layer.provideMerge(Adobe.layer, Automation.layer(ChildProcess.make(path.join(contents, 'MacOS', CFBundleExecutable), { stderr: 'inherit' })))), Layer.provide(layerDigest)),
        ),
        replaced,
    );
}).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
