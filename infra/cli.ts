import process from 'node:process';
import type { NodeServices } from '@effect/platform-node/NodeServices';
import { type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { Config, Effect, FileSystem, Path, Record } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hostingStack, hostingTargets } from '#/infra/hosting-stack';
import { repositoryStack } from '#/infra/repository-stack';

// --- [COMPOSITION] ---------------------------------------------------------------------

const stackOperation = { operation: Argument.Literals('operation', ['preview', 'up', 'refresh', 'destroy']) };
const operateStack = Effect.fn('operateStack')(function* <E, R extends NodeServices>(
    { select, program, ...args }: Readonly<Omit<InlineProgramArgs, 'program'> & { select: 'selectStack' | 'createOrSelectStack'; program: Effect.Effect<Record<string, unknown>, E, R> }>,
    operation: Command.Command.Config.Infer<typeof stackOperation>['operation'],
) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME'), Effect.context<R>()]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const stack = yield* Effect.tryPromise(() => LocalWorkspace[select]({ ...args, program: () => Effect.runPromiseWith(context)(program) }, { workDir }));
    const display = { diff: true, onOutput: process.stdout.write.bind(process.stdout), onError: process.stderr.write.bind(process.stderr) };
    yield* operation === 'destroy' ? Effect.tryPromise<unknown>(() => stack.previewDestroy(display)) : Effect.void;
    yield* Effect.uninterruptible(Effect.tryPromise<unknown>(() => stack[operation](display)));
});
const cli = Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make('repository', stackOperation, ({ operation }) => operateStack(repositoryStack, operation)),
        Command.make(
            'hosting',
            { directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }), target: Argument.Literals('target', Record.keys(hostingTargets)), ...stackOperation, disposable: Flag.Boolean('disposable').pipe(Flag.withDefault(false)) },
            ({ directory, target, operation, disposable }) => Effect.flatMap(hostingStack(directory, target, disposable), (stack) => operateStack(stack, operation)),
        ),
    ]),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { cli };
