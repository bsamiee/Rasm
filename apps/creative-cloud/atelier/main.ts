import process from 'node:process';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { serve } from '@rasm/mcp/node';
import { Duration, Effect, Equal, FileSystem, Layer, Option, Path, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { parse } from 'plist';
import { Adobe } from '#/adobe';
import manifest from '#/package.json' with { type: 'json' };
import { registerAdobeTools } from '#/register-adobe-tools';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this server' });

// --- [COMPOSITION] ---------------------------------------------------------------------

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
            Layer.effectDiscard(registerAdobeTools).pipe(Layer.provide(Layer.provideMerge(Adobe.layer, Automation.layer(ChildProcess.make(path.join(contents, 'MacOS', CFBundleExecutable), { stderr: 'inherit' })))), Layer.provide(layerDigest)),
        ),
        replaced,
    );
}).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
