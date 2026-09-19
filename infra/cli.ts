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

const _run = Effect.fnUntraced(function* (operation: Exclude<StackError['operation'], 'select'>, imports: boolean) {
    const path = yield* Path.Path;
    const stdio = yield* Stdio.Stdio;
    const stack = yield* Effect.tryPromise({
        try: () =>
            LocalWorkspace.createOrSelectStack(
                { stackName: 'rasm', projectName: 'rasm-infra', program: async () => program(imports) },
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
    Command.make('infra').pipe(
        Command.withSubcommands([
            Command.make(
                'up',
                {
                    imports: Flag.Boolean('import').pipe(
                        Flag.withDescription('Import the existing Doppler project, environments, branch configs, and repository into the stack'),
                        Flag.withDefault(false),
                    ),
                },
                ({ imports }) => _run('up', imports),
            ),
            Command.make('refresh', {}, () => _run('refresh', false)),
        ]),
    ),
    { version: '' },
).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
