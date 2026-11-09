import { isSea } from 'node:sea';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/applications/node';
import { serve } from '@rasm/mcp/node';
import { Config, Duration, Effect, Layer } from 'effect';
import { ChildProcess } from 'effect/process';
import { Adobe } from '#/adobe';
import { executable } from '#/features/delivery';
import manifest from '#/package.json' with { type: 'json' };
import { registerAdobeTools } from '#/register-adobe-tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

const automation = Layer.unwrap(
    Effect.gen(function* () {
        return Automation.layer(ChildProcess.make(yield* isSea() ? executable : Config.String('ATELIER_PATH'), { stderr: 'inherit' }));
    }),
);

serve(
    manifest,
    {
        retention: Duration.hours(1),
        instructions: [
            'Specifiers come from results',
            'State-changing calls return early when a dialog requires an answer',
            'screenshot reads every window of a background application with no activation',
            'execute_script runs a body as written, activate in a body takes focus from the user and reports active on the call',
            'A blocking window is a new window on a blocked main thread, perform answers it with a key action, its leaving alone proves the answer, else a person answers it',
        ],
    },
    Layer.effectDiscard(registerAdobeTools).pipe(Layer.provide(Layer.provideMerge(Adobe.layer, automation)), Layer.provide(layerDigest)),
).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
