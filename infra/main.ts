import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { version } from '@pulumi/pulumi/version.js';
import { Effect } from 'effect';
import { Command } from 'effect/cli';
import { cli } from '#/infra/cli';

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.run(cli, { version }).pipe(Effect.provide(layer), runMain);
