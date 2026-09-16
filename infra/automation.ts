// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { LocalWorkspace, type Stack } from '@pulumi/pulumi/automation/index.js';
import { Cause, Console, Data, Effect, flow, Path, type PlatformError, Queue, Stdio, Stream } from 'effect';
import { Command, Flag } from 'effect/unstable/cli';
import { program } from './program.ts';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{ readonly operation: 'select' | 'up' | 'refresh'; readonly cause: unknown }> {
    override get message(): string {
        return `Stack ${this.operation} failed`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

type Operation = Exclude<StackError['operation'], 'select'>;

const _output = (stack: Stack, operation: Operation, output: Queue.Queue<string, StackError | Cause.Done>): Effect.Effect<void, StackError> =>
    Effect.andThen(
        Effect.tryPromise({ try: () => stack[operation]({ onOutput: (text) => Queue.offerUnsafe(output, text) }), catch: (cause) => new StackError({ operation, cause }) }),
        Queue.end(output),
    );

const _operation = (operation: Operation, adopt: boolean): Effect.Effect<void, StackError | PlatformError.PlatformError, Path.Path | Stdio.Stdio> =>
    Effect.gen(function* () {
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
            Stream.callback<string, StackError>((output) => _output(stack, operation, output)),
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
).pipe(
    Effect.tapErrorTag(['StackError', 'PlatformError'], (error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
