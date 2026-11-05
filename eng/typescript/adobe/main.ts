import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect } from 'effect';
import { Command } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { install } from '#/eng/typescript/adobe/install';

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.run(install, { version: nx.version }).pipe(Effect.provide(layer), runMain);
