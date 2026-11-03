import process from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { type EngineEvent, type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { version } from '@pulumi/pulumi/version.js';
import { Config, Effect, FileSystem, flow, Match, Option, Path, Record } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hosting, targets } from '#/infra/hosting';
import { stack } from '#/infra/repository';

// --- [COMPOSITION] ---------------------------------------------------------------------

const options = { operation: Argument.Literals('operation', ['preview', 'up', 'refresh', 'destroy']) };
const run = Effect.fn('run')(function* <E, R extends NodeServices.NodeServices>(select: 'selectStack' | 'createOrSelectStack', args: Readonly<Omit<InlineProgramArgs, 'program'> & { program: Effect.Effect<Record<string, unknown>, E, R> }>, operation: Command.Command.Config.Infer<typeof options>['operation']) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME'), Effect.context<R>()]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const selected = yield* Effect.tryPromise(() => LocalWorkspace[select]({ ...args, program: () => Effect.runPromiseWith(context)(args.program) }, { workDir }));
    const display = {
        onEvent: flow(
            Match.type<EngineEvent>().pipe(
                Match.when({ resOutputsEvent: { metadata: { op: (op) => op !== 'same' } } }, ({ resOutputsEvent: { metadata } }) =>
                    [`${metadata.op} ${metadata.urn}`, ...(metadata.diffs ?? []).map((key) => `    ${key}: ${JSON.stringify(metadata.old?.inputs[key])} -> ${JSON.stringify(metadata.new?.inputs[key])}`)].join('\n'),
                ),
                Match.when({ diagnosticEvent: { severity: Match.is('warning', 'error') } }, ({ diagnosticEvent: { severity, urn, message } }) => [severity, urn, message.trim()].join(' ')),
                Match.when({ summaryEvent: Match.defined }, ({ summaryEvent: { resourceChanges } }) => JSON.stringify(resourceChanges)),
                Match.option,
            ),
            Option.map((line) => process.stdout.write(`${line}\n`)),
        ),
    };
    yield* operation === 'destroy' ? Effect.tryPromise<unknown>(() => selected.previewDestroy(display)) : Effect.void;
    yield* Effect.uninterruptible(Effect.tryPromise<unknown>(() => selected[operation](display)));
});

Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make('repository', options, ({ operation }) => run('selectStack', stack, operation)),
        Command.make('hosting', { directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }), target: Argument.Literals('target', Record.keys(targets)), ...options, disposable: Flag.Boolean('disposable').pipe(Flag.withDefault(false)) }, ({ directory, target, operation, disposable }) =>
            Effect.flatMap(hosting(directory, target, disposable), (args) => run('createOrSelectStack', args, operation)),
        ),
    ]),
    Command.run({ version }),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain,
);
