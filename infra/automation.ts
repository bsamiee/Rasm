// --- [IMPORTS] -------------------------------------------------------------------------

import { Command, Options, ValidationError } from '@effect/cli';
import { Path } from '@effect/platform';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Cause, Console, Data, Effect, Inspectable, Match, Predicate, Runtime } from 'effect';
import { program } from './program.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROJECT = 'rasm-infra';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{
    readonly operation: 'select' | 'up' | 'refresh';
    readonly cause: unknown;
}> {
    override get message(): string {
        const detail = Match.value(this.cause).pipe(
            Match.when(Predicate.isError, (error) => error.message),
            Match.orElse(Inspectable.toStringUnknown),
        );
        return `Stack ${this.operation} failed: ${detail}`;
    }
}

// --- [STACK] ---------------------------------------------------------------------------

const _operation = (operation: Exclude<StackError['operation'], 'select'>, adopt: boolean): Effect.Effect<void, StackError, Path.Path> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const runtime = yield* Effect.runtime<never>();
        const stack = yield* Effect.tryPromise({
            try: () =>
                LocalWorkspace.createOrSelectStack(
                    { stackName: 'rasm', projectName: _PROJECT, program: () => Runtime.runPromise(runtime)(program(adopt)) },
                    { projectSettings: { name: _PROJECT, runtime: 'nodejs' }, pulumiHome: path.join(import.meta.dirname, '..', '.cache', 'pulumi') },
                ),
            catch: (cause) => new StackError({ operation: 'select', cause }),
        });
        const result = yield* Effect.tryPromise({
            try: () => stack[operation]({ onOutput: (output) => process.stdout.write(output) }),
            catch: (cause) => new StackError({ operation, cause }),
        });
        yield* Console.log(JSON.stringify(result.summary.resourceChanges ?? {}));
    });

// --- [COMMANDS] ------------------------------------------------------------------------

const _import = Options.boolean('import').pipe(
    Options.withDescription('Adopt the live Doppler project, environments, branch configs, and repository'),
);

const _automation = Command.make('automation').pipe(
    Command.withSubcommands([
        Command.make('up', { import: _import }, ({ import: adopt }) => _operation('up', adopt)),
        Command.make('refresh', {}, () => _operation('refresh', false)),
    ]),
);

// --- [ENTRY] ---------------------------------------------------------------------------

NodeRuntime.runMain(
    Command.run(_automation, { name: 'automation', version: '' })(process.argv).pipe(
        Effect.tapError((error) =>
            Match.value(error).pipe(
                Match.when(ValidationError.isValidationError, () => Effect.void),
                Match.orElse((failure) => Console.error(failure.message)),
            ),
        ),
        Effect.tapDefect((cause) => Console.error(Cause.pretty(cause))),
        Effect.provide(NodeContext.layer),
    ),
    { disableErrorReporting: true },
);
