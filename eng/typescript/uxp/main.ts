import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect } from 'effect';
import { Command } from 'effect/cli';
import { agentBundle, install } from '#/eng/typescript/uxp/install';

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.flatMap(agentBundle, ({ version, agent }) => Command.run(install(agent), { version })).pipe(Effect.provide(layer), runMain);
