import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Array, Data, Effect, FileSystem, flow, identity, Order, Path, type PlatformError, Queue, Schema, Stdio, Stream } from 'effect';
import { Command, Flag } from 'effect/unstable/cli';
import { parse as yaml } from 'yaml';
import { Actions, program } from './program.ts';

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Data.TaggedError('StackError')<{ readonly operation: 'select' | 'up' | 'refresh'; readonly cause: unknown }> {
    override get message(): string {
        return `Stack ${this.operation} failed`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _run = Effect.fnUntraced(function* (operation: Exclude<StackError['operation'], 'select'>, imports: boolean) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const workDir = path.join(import.meta.dirname, '..', '.cache', 'pulumi', 'work');
    yield* fs.makeDirectory(workDir, { recursive: true });
    const github = path.join(import.meta.dirname, '..', '.github');
    const documents = (directory: string, file: (entry: string) => string): Effect.Effect<unknown[], PlatformError.PlatformError> =>
        fs.readDirectory(directory).pipe(
            Effect.flatMap(
                flow(
                    Array.sort(Order.String),
                    Array.map((entry) => Effect.map(fs.readFileString(path.join(directory, file(entry))), yaml)),
                    Effect.all,
                ),
            ),
        );
    const actions = yield* Effect.all({
        workflows: documents(path.join(github, 'workflows'), identity),
        actions: documents(path.join(github, 'actions'), (entry) => path.join(entry, 'action.yml')),
    }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Actions)));
    const stack = yield* Effect.tryPromise({
        try: () => LocalWorkspace.createOrSelectStack({ stackName: 'rasm', projectName: 'rasm-infra', program: async () => program(imports, actions) }, { workDir }),
        catch: (cause) => new StackError({ operation: 'select', cause }),
    });
    const stdio = yield* Stdio.Stdio;
    yield* Stream.run(
        Stream.callback<string, StackError>((output) =>
            Queue.into(Effect.tryPromise({ try: () => stack[operation]({ onOutput: (text) => Queue.offerUnsafe(output, text) }), catch: (cause) => new StackError({ operation, cause }) }), output),
        ),
        stdio.stdout(),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make(
            'up',
            {
                imports: Flag.Boolean('import').pipe(Flag.withDescription('Import the existing Doppler project, environments, branch configs, and repository into the stack'), Flag.withDefault(false)),
            },
            ({ imports }) => _run('up', imports),
        ),
        Command.make('refresh', {}, () => _run('refresh', false)),
    ]),
    Command.run({ version: '' }),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain,
);
