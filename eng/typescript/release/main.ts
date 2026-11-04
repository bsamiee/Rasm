import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect } from 'effect';
import { Command } from 'effect/cli';
import nx from 'nx/package.json' with { type: 'json' };
import { release } from '#/eng/typescript/release/release';

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.run(release, { version: nx.version }).pipe(Effect.provide(layer), runMain);
