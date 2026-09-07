// Automation API entry that runs up or refresh on the stack over a file backend

// --- [IMPORTS] -------------------------------------------------------------------------

import { Command, Options, ValidationError } from '@effect/cli';
import { FileSystem, Path } from '@effect/platform';
import type { PlatformError } from '@effect/platform/Error';
import { NodeContext, NodeRuntime } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Cause, Config, type ConfigError, Console, Data, Effect, Inspectable, Match, Predicate, Runtime } from 'effect';
import { program } from './program.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PROJECT = 'rasm-infra';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{ readonly operation: 'select' | 'up' | 'refresh'; readonly cause: unknown }> {
    override get message(): string {
        const detail = Match.value(this.cause).pipe(
            Match.when(Predicate.isError, (error) => error.message),
            Match.orElse(Inspectable.toStringUnknown),
        );
        return `Stack ${this.operation} failed: ${detail}`;
    }
}

// --- [STACK] ---------------------------------------------------------------------------

// Pulumi and its providers read credentials from the environment injected by doppler run
const _operation = (
    operation: 'up' | 'refresh',
    adopt: boolean,
): Effect.Effect<void, StackError | ConfigError.ConfigError | PlatformError, FileSystem.FileSystem | Path.Path> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const runtime = yield* Effect.runtime<never>();
        // An unset or empty XDG_STATE_HOME falls back to the specification default under the home directory
        const stateRoot = yield* Config.nonEmptyString('XDG_STATE_HOME').pipe(
            Config.orElse(() => Config.map(Config.nonEmptyString('HOME'), (home) => path.join(home, '.local', 'state'))),
        );
        const stateDirectory = path.join(stateRoot, _PROJECT);
        const backend = yield* path.toFileUrl(stateDirectory);
        yield* fs.makeDirectory(stateDirectory, { recursive: true, mode: 0o700 });
        const stack = yield* Effect.tryPromise({
            try: () =>
                LocalWorkspace.createOrSelectStack(
                    { stackName: 'rasm', projectName: _PROJECT, program: () => Runtime.runPromise(runtime)(program(adopt)) },
                    {
                        projectSettings: { name: _PROJECT, runtime: 'nodejs', backend: { url: backend.href } },
                        pulumiHome: path.join(import.meta.dirname, '..', '.cache', 'pulumi'),
                        secretsProvider: 'passphrase',
                    },
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
