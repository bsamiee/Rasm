// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Console, Effect, FileSystem, flow, Option, Path, Record, Schema } from 'effect';
import { Command } from 'effect/unstable/cli';
import { HOUSE, write } from './acrobat/actions.ts';
import { resolve } from './hosts.ts';
import { generate } from './illustrator/generate.ts';
import { processId } from './jobs.ts';
import { HOSTS } from './values.ts';

// --- [MODELS] --------------------------------------------------------------------------

const HostRunning: Schema.TaggedStruct<'hostRunning', { readonly pid: Schema.Int }> = Schema.TaggedStruct('hostRunning', { pid: Schema.Int });

// --- [DEPLOY] --------------------------------------------------------------------------

const _deploy = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    yield* Option.match(yield* processId(HOSTS.acrobat), { onNone: () => Effect.void, onSome: (pid) => Effect.fail(HostRunning.make({ pid })) });
    const { acrobat } = yield* resolve;
    yield* fs.remove(path.join(acrobat.supportFolder, 'DC', 'JavaScripts'), { recursive: true, force: true });
    yield* Effect.forEach(Record.values(HOUSE), (action) => Effect.flatMap(write(acrobat, action), Console.log), { discard: true });
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(Command.make('automation').pipe(Command.withSubcommands([Command.make('deploy', {}, () => _deploy), Command.make('generate', {}, () => generate())])), { version: '' }).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
