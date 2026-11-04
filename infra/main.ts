import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { version } from '@pulumi/pulumi/version.js';
import { Config, Effect, FileSystem, Path, Record } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hosting, hostingTargets } from '#/infra/hosting';
import { repositoryStack } from '#/infra/repository';

// --- [COMPOSITION] ---------------------------------------------------------------------

const options = { operation: Argument.Literals('operation', ['preview', 'up', 'refresh', 'destroy']) };
const run = Effect.fn('run')(function* <E, R extends NodeServices>(select: 'selectStack' | 'createOrSelectStack', args: Readonly<Omit<InlineProgramArgs, 'program'> & { program: Effect.Effect<Record<string, unknown>, E, R> }>, operation: Command.Command.Config.Infer<typeof options>['operation']) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME'), Effect.context<R>()]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const selected = yield* Effect.tryPromise(() => LocalWorkspace[select]({ ...args, program: () => Effect.runPromiseWith(context)(args.program) }, { workDir }));
    const display = { diff: true, onOutput: process.stdout.write.bind(process.stdout), onError: process.stderr.write.bind(process.stderr) };
    yield* operation === 'destroy' ? Effect.tryPromise<unknown>(() => selected.previewDestroy(display)) : Effect.void;
    yield* Effect.uninterruptible(Effect.tryPromise<unknown>(() => selected[operation](display)));
});

Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make('repository', options, ({ operation }) => run('selectStack', repositoryStack, operation)),
        Command.make('hosting', { directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }), target: Argument.Literals('target', Record.keys(hostingTargets)), ...options, disposable: Flag.Boolean('disposable').pipe(Flag.withDefault(false)) }, ({ directory, target, operation, disposable }) =>
            Effect.flatMap(hosting(directory, target, disposable), (args) => run('createOrSelectStack', args, operation)),
        ),
    ]),
    Command.run({ version }),
    Effect.provide(layer),
    runMain,
);
