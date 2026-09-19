// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Data, Effect, Path, Queue, Stdio, Stream } from 'effect';
import { Command, Flag } from 'effect/unstable/cli';
import { program } from './program.ts';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{ readonly operation: 'select' | 'up' | 'refresh'; readonly cause: unknown }> {
    override get message(): string {
        return `Stack ${this.operation} failed`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _operation = Effect.fnUntraced(function* (operation: Exclude<StackError['operation'], 'select'>, adopt: boolean) {
    const path = yield* Path.Path;
    const stdio = yield* Stdio.Stdio;
    const stack = yield* Effect.tryPromise({
        try: () =>
            LocalWorkspace.createOrSelectStack(
                { stackName: 'rasm', projectName: 'rasm-infra', program: () => Effect.runPromise(program(adopt)) },
                { pulumiHome: path.join(import.meta.dirname, '..', '.cache', 'pulumi') },
            ),
        catch: (cause) => new StackError({ operation: 'select', cause }),
    });
    yield* Stream.run(
        Stream.callback<string, StackError>((output) =>
            Effect.andThen(
                Effect.tryPromise({ try: () => stack[operation]({ onOutput: (text) => Queue.offerUnsafe(output, text) }), catch: (cause) => new StackError({ operation, cause }) }),
                Queue.end(output),
            ),
        ),
        stdio.stdout(),
    );
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(
    Command.make('automation').pipe(
        Command.withSubcommands([
            Command.make(
                'up',
                { adopt: Flag.Boolean('import').pipe(Flag.withDescription('Adopt the Doppler project, environments, branch configs, and repository'), Flag.withDefault(false)) },
                ({ adopt }) => _operation('up', adopt),
            ),
            Command.make('refresh', {}, () => _operation('refresh', false)),
        ]),
    ),
    { version: '' },
).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
