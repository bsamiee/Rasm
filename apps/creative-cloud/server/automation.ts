// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Console, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { Argument, Command } from 'effect/unstable/cli';
import { HOUSE, write } from './acrobat/actions.ts';
import { bundle, info, resolve } from './hosts.ts';
import { generate } from './illustrator/generate.ts';
import { processId } from './jobs.ts';
import manifest from './package.json' with { type: 'json' };
import { HOSTS, HostId } from './values.ts';

// --- [MODELS] --------------------------------------------------------------------------

const HostRunning = Schema.TaggedStruct('hostRunning', { pid: Schema.Int });

// --- [COMMANDS] ------------------------------------------------------------------------

const _deploy = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const { acrobat } = yield* resolve;
    yield* Effect.transposeOption(Option.map(yield* processId(acrobat), (pid) => Effect.fail(HostRunning.make({ pid }))));
    yield* fs.remove(path.join(acrobat.supportFolder, 'DC', 'JavaScripts'), { recursive: true, force: true });
    yield* Effect.forEach(Record.values(HOUSE), (action) => Effect.flatMap(write(acrobat, action), Console.log), { discard: true });
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(
    Command.make('automation').pipe(
        Command.withSubcommands([
            Command.make('deploy', {}, () => _deploy),
            Command.make('generate', {}, generate),
            Command.make('version', { host: Argument.Literals('host', HostId.literals) }, ({ host }) =>
                bundle(HOSTS[host]).pipe(
                    Effect.flatMap(info),
                    Effect.flatMap((details) => Console.log(details.version)),
                ),
            ),
        ]),
    ),
    { version: manifest.version },
).pipe(
    Effect.tapCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.void : Console.error(Cause.pretty(cause)))),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
