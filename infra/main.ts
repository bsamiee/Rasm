import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { type EngineEvent, type InlineProgramArgs, LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { version } from '@pulumi/pulumi/version.js';
import { type Cause, Config, Console, Effect, FileSystem, Match, Path, Queue, Record, Stream } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hosting, targets } from './hosting.ts';
import { program, stack } from './repository.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Operation = typeof options.operation extends Argument.Argument<infer A> ? A : never;

// --- [COMPOSITION] ---------------------------------------------------------------------

const options = { operation: Argument.Literals('operation', ['preview', 'up', 'refresh', 'destroy']) };
const run = Effect.fn('run')(function* (select: 'selectStack' | 'createOrSelectStack', args: InlineProgramArgs, operation: Operation) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME')]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const selected = yield* Effect.tryPromise(() => LocalWorkspace[select](args, { workDir }));
    yield* operation === 'destroy' ? Effect.tryPromise(() => selected.previewDestroy()) : Effect.void;
    yield* Stream.callback<EngineEvent, Cause.UnknownError>((queue) => Effect.tryPromise<unknown>(() => selected[operation]({ onEvent: (event) => Queue.offerUnsafe(queue, event) })).pipe(Effect.uninterruptible, Queue.into(queue))).pipe(
        Stream.filterMap(
            Match.type<EngineEvent>().pipe(
                Match.when({ resOutputsEvent: { metadata: { op: (op) => op !== 'same' } } }, ({ resOutputsEvent: { metadata } }) =>
                    [`${metadata.op} ${metadata.urn}`, ...(metadata.diffs ?? []).map((key) => `    ${key}: ${JSON.stringify(metadata.old?.inputs[key])} -> ${JSON.stringify(metadata.new?.inputs[key])}`)].join('\n'),
                ),
                Match.when({ diagnosticEvent: { severity: Match.is('warning', 'error') } }, ({ diagnosticEvent: { severity, urn, message } }) => [severity, urn, message.trim()].join(' ')),
                Match.when({ summaryEvent: Match.defined }, ({ summaryEvent: { resourceChanges } }) => JSON.stringify(resourceChanges)),
                Match.result,
            ),
        ),
        Stream.runForEach(Console.log),
    );
});

Command.make('infra').pipe(
    Command.withSubcommands([
        Command.make('repository', options, ({ operation }) => run('selectStack', { ...stack, program }, operation)),
        Command.make('hosting', { directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }), target: Argument.Literals('target', Record.keys(targets)), ...options, disposable: Flag.Boolean('disposable').pipe(Flag.withDefault(false)) }, ({ directory, target, operation, disposable }) =>
            Effect.flatMap(hosting(directory, target, disposable), (args) => run('createOrSelectStack', args, operation)),
        ),
    ]),
    Command.run({ version }),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain,
);
