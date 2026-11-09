import process from 'node:process';
import { layerDigest, layerMac } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { serve } from '@rasm/mcp/node';
import { Duration, Effect, Layer, Path, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Adobe } from '#/adobe';
import manifest from '#/package.json' with { type: 'json' };
import { registerAdobeTools } from '#/register-adobe-tools';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.fromJsonString(Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleShortVersionString: Schema.String })).annotate({ description: 'Info.plist of the app bundle holding this server, as plutil prints it in JSON' });

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [path, spawner] = yield* Effect.all([Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const contents = path.join(path.dirname(process.execPath), '..');
    const { CFBundleExecutable, CFBundleShortVersionString } = yield* spawner.string(ChildProcess.make('/usr/bin/plutil', ['-convert', 'json', '-o', '-', path.join(contents, 'Info.plist')], { stdin: 'ignore', stderr: 'inherit' })).pipe(Effect.flatMap(Schema.decodeEffect(Bundle)));
    return yield* serve(
        { name: manifest.name, description: manifest.description, version: CFBundleShortVersionString },
        {
            retention: Duration.hours(1),
            listen: Peer.link.listen,
            instructions: [
                'Specifiers come from results',
                'State-changing calls return early when a dialog requires an answer',
                'screenshot reads every window of a background application with no activation',
                'execute_script runs a body as written, activate in a body takes focus from the user and reports active on the call',
                'A blocking window is a new window on a blocked main thread, perform answers it with a key action, its leaving alone proves the answer, else a person answers it',
            ],
        },
        Layer.effectDiscard(registerAdobeTools).pipe(Layer.provide(Layer.provideMerge(Adobe.layer, Automation.layer(ChildProcess.make(path.join(contents, 'MacOS', CFBundleExecutable), { stderr: 'inherit' })))), Layer.provide([layerDigest, layerMac])),
    );
}).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
