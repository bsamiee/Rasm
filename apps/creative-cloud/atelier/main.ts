import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Automation } from '@rasm/automation/node';
import { serve } from '@rasm/mcp/node';
import { Config, Duration, Effect, Layer } from 'effect';
import manifest from '#/package.json' with { type: 'json' };
import { registerAdobeTools } from '#/register-adobe-tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

serve(
    manifest,
    { retention: Duration.hours(1), instructions: ['Specifiers come from results', 'State-changing calls return early when a dialog requires an answer', 'No tool changes frontmost application'] },
    Layer.effectDiscard(registerAdobeTools).pipe(Layer.provide(Automation.layer(Config.String('ATELIER_PATH'))), Layer.provide(layerDigest)),
).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
