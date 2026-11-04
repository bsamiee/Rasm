import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect } from 'effect';
import { cleanup } from '#/eng/typescript/cleanup/cleanup';

// --- [COMPOSITION] ---------------------------------------------------------------------

cleanup.pipe(Effect.provide(layer), runMain);
