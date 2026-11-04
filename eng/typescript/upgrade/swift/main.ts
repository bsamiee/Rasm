import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect } from 'effect';
import { upgrade } from '#/eng/typescript/upgrade/swift/upgrade';

// --- [COMPOSITION] ---------------------------------------------------------------------

upgrade.pipe(Effect.provide(layer), runMain);
