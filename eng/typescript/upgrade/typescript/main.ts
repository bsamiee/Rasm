import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect, Layer } from 'effect';
import { upgrade } from '#/eng/typescript/upgrade/typescript/upgrade';

// --- [COMPOSITION] ---------------------------------------------------------------------

upgrade.pipe(Effect.provide(Layer.merge(layer, layerUndici)), runMain);
