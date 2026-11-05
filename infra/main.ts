import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { version } from '@pulumi/pulumi/version.js';
import { Config, Effect, FileSystem, Path, Record } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hostingStack, hostingTargets } from '#/infra/hosting-stack';
import { repositoryStack } from '#/infra/repository-stack';

// --- [COMPOSITION] ---------------------------------------------------------------------

const stackOperation = { operation: Argument.Literals('operation', ['preview', 'up', 'refresh', 'destroy']) };
const operateStack = Effect.fn('operateStack')(function* <E, R extends NodeServices>({ program, ...args }: Readonly<Omit<InlineProgramArgs, 'program'> & { program: Effect.Effect<Record<string, unknown>, E, R> }>, operation: Command.Command.Config.Infer<typeof stackOperation>['operation']) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME'), Effect.context<R>()]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const stack = yield* Effect.tryPromise(() => LocalWorkspace.createOrSelectStack({ ...args, program: () => Effect.runPromiseWith(context)(program) }, { workDir }));
    yield* Effect.tryPromise<unknown>((signal) => stack[operation]({ diff: true, signal, onOutput: process.stdout.write.bind(process.stdout), onError: process.stderr.write.bind(process.stderr) }));
});

Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make('repository', stackOperation, ({ operation }) => operateStack(repositoryStack, operation)),
        Command.make(
            'hosting',
            {
                directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }),
                target: Argument.Literals('target', Record.keys(hostingTargets)),
                ...stackOperation,
                disposable: Flag.Boolean('disposable').pipe(Flag.withDefault(false)),
                tag: Flag.String('tag').pipe(Flag.optional),
            },
            ({ directory, target, operation, disposable, tag }) => Effect.flatMap(hostingStack(directory, target, disposable, tag), (stack) => operateStack(stack, operation)),
        ),
    ]),
    Command.run({ version }),
    Effect.provide(layer),
    runMain,
);
