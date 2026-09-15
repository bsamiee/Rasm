// --- [IMPORTS] -------------------------------------------------------------------------

import { Command, Options } from '@effect/cli';
import { Path } from '@effect/platform';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Cause, Console, Data, Effect, flow, Runtime } from 'effect';
import { program } from './program.ts';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{ readonly operation: 'select' | 'up' | 'refresh'; readonly cause: unknown }> {
    override get message(): string {
        return `Stack ${this.operation} failed`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _operation = (operation: Exclude<StackError['operation'], 'select'>, adopt: boolean): Effect.Effect<void, StackError, Path.Path> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const runtime = yield* Effect.runtime<never>();
        const stack = yield* Effect.tryPromise({
            try: () =>
                LocalWorkspace.createOrSelectStack(
                    { stackName: 'rasm', projectName: 'rasm-infra', program: () => Runtime.runPromise(runtime)(program(adopt)) },
                    { pulumiHome: path.join(import.meta.dirname, '..', '.cache', 'pulumi') },
                ),
            catch: (cause) => new StackError({ operation: 'select', cause }),
        });
        yield* Effect.tryPromise({ try: () => stack[operation]({ onOutput: (output) => process.stdout.write(output) }), catch: (cause) => new StackError({ operation, cause }) });
    });

// --- [ENTRY] ---------------------------------------------------------------------------

Command.make('automation')
    .pipe(
        Command.withSubcommands([
            Command.make('up', { adopt: Options.boolean('import').pipe(Options.withDescription('Adopt the Doppler project, environments, branch configs, and repository')) }, ({ adopt }) =>
                _operation('up', adopt),
            ),
            Command.make('refresh', {}, () => _operation('refresh', false)),
        ]),
        Command.run({ name: 'automation', version: '' }),
    )(process.argv)
    .pipe(
        Effect.tapErrorTag('StackError', (error) => Console.error(Cause.pretty(Cause.fail(error), { renderErrorCause: true }))),
        Effect.tapDefect(flow(Cause.pretty, Console.error)),
        Effect.provide(NodeContext.layer),
        NodeRuntime.runMain({ disableErrorReporting: true }),
    );
