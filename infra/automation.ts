// Automation API entry that runs up or refresh on the stack under Pulumi Cloud

// --- [IMPORTS] -------------------------------------------------------------------------

import { Command, Options, ValidationError } from '@effect/cli';
import { Path } from '@effect/platform';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Cause, Config, type ConfigError, Console, Data, Effect, Inspectable, Match, Predicate, Record, Runtime } from 'effect';
import { ACTIONS_VARIABLES, program } from './program.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROJECT = 'rasm-infra';

// Each Actions variable value comes from the environment under its own name, and an unset name fails the run naming it
const _variables = Config.all(Record.fromIterableWith(ACTIONS_VARIABLES, (name) => [name, Config.nonEmptyString(name)]));

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

// Pulumi reads PULUMI_ACCESS_TOKEN and its providers read their tokens from the environment injected by doppler run
const _operation = (
    operation: Exclude<StackError['operation'], 'select'>,
    adopt: boolean,
): Effect.Effect<void, StackError | ConfigError.ConfigError, Path.Path> =>
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const runtime = yield* Effect.runtime<never>();
        const variables = yield* _variables;
        const stack = yield* Effect.tryPromise({
            try: () =>
                LocalWorkspace.createOrSelectStack(
                    { stackName: 'rasm', projectName: _PROJECT, program: () => Runtime.runPromise(runtime)(program(adopt, variables)) },
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
    Options.withDescription('Adopt the live Doppler project, environments, branch configs, and repository into the state'),
);

const _automation = Command.make('automation').pipe(
    Command.withSubcommands([
        Command.make('up', { import: _import }, ({ import: adopt }) => _operation('up', adopt)),
        Command.make('refresh', {}, () => _operation('refresh', false)),
    ]),
);

// --- [ENTRY] ---------------------------------------------------------------------------

// The cli prints its own help on a validation error, every other failure prints its message and a defect prints its cause
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
