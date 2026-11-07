#!/usr/bin/env node
import process from 'node:process';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { CommandError, type EngineEvent, type InlineProgramArgs, LocalWorkspace, type OutputMap, type PreviewResult, StackNotFoundError, type UpdateSummary } from '@pulumi/pulumi/automation/index.js';
import { version } from '@pulumi/pulumi/version.js';
import { runCommand } from '@rasm/eng';
import { Array, Cause, Config, Console, Effect, FileSystem, Match, Option, Path, Record, Schema, Struct } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { hostingStack, hostingTargets } from '#/hosting-stack.ts';
import { repositoryStack } from '#/repository-stack.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.fromJsonString(Schema.Struct({ stack: Schema.String, changes: Schema.Record(Schema.String, Schema.Finite), outputs: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)) }));
const Hosted = Schema.Struct({ tags: Schema.Array(Schema.String), targets: Schema.Struct({ infra: Schema.Struct({ options: Schema.Struct({ env: Schema.Struct({ MinVerTagPrefix: Schema.OptionFromOptionalKey(Schema.String) }) }) }) }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const shown = (outputs: OutputMap) =>
    Record.map(
        Record.filter(outputs, ({ secret }) => !secret),
        Struct.get('value'),
    );
const reconcile = Effect.fn('reconcile')(function* <E, R extends NodeServices>({ program, ...args }: Readonly<Omit<InlineProgramArgs, 'program'> & { program: Effect.Effect<Record<string, unknown>, E, R> }>, { apply, destroy }: { readonly apply: boolean; readonly destroy: boolean }) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PULUMI_HOME'), Effect.context<R>()]);
    const workDir = path.join(home, args.projectName);
    yield* fs.makeDirectory(workDir, { recursive: true });
    const inline = { ...args, program: () => Effect.runPromiseWith(context)(program) };
    const stage = Effect.fnUntraced(function* () {
        const empty = yield* fs.makeTempDirectoryScoped();
        const backend = yield* path.toFileUrl(empty);
        return yield* Effect.tryPromise({ try: () => LocalWorkspace.createStack(inline, { workDir: empty, envVars: { PULUMI_BACKEND_URL: backend.href, PULUMI_CONFIG_PASSPHRASE: '' } }), catch: (cause) => new Cause.UnknownError(cause, `Cannot stage an empty ${args.stackName}`) });
    });
    const stack = yield* Effect.tryPromise({
        try: () => (apply && !destroy ? LocalWorkspace.createOrSelectStack(inline, { workDir }) : LocalWorkspace.selectStack(inline, { workDir })),
        catch: (cause) => (cause instanceof StackNotFoundError ? cause : new Cause.UnknownError(cause, `Cannot select ${args.stackName}`)),
    }).pipe(Effect.catchIf((error): error is StackNotFoundError => error instanceof StackNotFoundError, stage));
    const diagnostics: string[] = [];
    const options = {
        diff: true,
        refresh: true,
        onOutput: process.stderr.write.bind(process.stderr),
        onEvent: ({ diagnosticEvent }: EngineEvent) => {
            if (diagnosticEvent?.severity === 'error') {
                diagnostics.push(diagnosticEvent.message.trim());
            }
        },
    };
    const run = <A>(call: (signal: AbortSignal) => Promise<A>) => Effect.tryPromise<A, Cause.UnknownError>({ try: call, catch: (cause) => new Cause.UnknownError(cause instanceof CommandError ? diagnostics.join('\n') : cause, `Pulumi ${apply ? 'apply' : 'preview'} failed for ${args.stackName}`) });
    const applied = ({ summary }: { readonly summary: UpdateSummary }) => ({ changes: summary.resourceChanges ?? {} });
    const previewed = ({ changeSummary }: PreviewResult) => ({ changes: changeSummary });
    const result = yield* Match.value({ apply, destroy }).pipe(
        Match.when({ apply: true, destroy: true }, () =>
            Effect.map(
                run((signal) => stack.destroy({ ...options, signal, remove: true })),
                applied,
            ),
        ),
        Match.when({ apply: false, destroy: true }, () =>
            Effect.map(
                run((signal) => stack.previewDestroy({ ...options, signal })),
                previewed,
            ),
        ),
        Match.when({ apply: true, destroy: false }, () =>
            Effect.map(
                run((signal) => stack.up({ ...options, signal })),
                (up) => ({ ...applied(up), outputs: shown(up.outputs) }),
            ),
        ),
        Match.orElse(() =>
            Effect.map(
                run((signal) => stack.preview({ ...options, signal })),
                previewed,
            ),
        ),
    );
    yield* Effect.flatMap(Schema.encodeEffect(Report)({ stack: args.stackName, ...result }), Console.log);
}, Effect.scoped);
const hosted = Effect.fnUntraced(function* (directory: string, target: Option.Option<keyof typeof hostingTargets>) {
    const { nodes } = yield* Effect.try(readCachedProjectGraph);
    const { data } = yield* Effect.fromOption(
        Array.findFirst(Object.values(nodes), ({ data: { root } }) => root === directory),
        () => new Cause.NoSuchElementError(`${directory} is no project`),
    );
    const { tags, targets } = yield* Schema.decodeUnknownEffect(Hosted)(data);
    const chosen = yield* Effect.fromOption(
        Option.orElse(target, () => Array.findFirst(Record.keys(hostingTargets), (key) => tags.includes(`output:${key}`))),
        () => new Cause.NoSuchElementError(`${directory} declares no hosting target`),
    );
    const tag = yield* Option.match(targets.infra.options.env.MinVerTagPrefix, {
        onNone: () => Effect.succeedNone,
        onSome: (prefix) => Effect.map(runCommand.records('\n', 'git', ['tag', '--points-at', 'HEAD', '--list', `${prefix}*`]), Array.head),
    });
    return yield* hostingStack(directory, chosen, tag);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make(
    'rasm-infra',
    {
        directory: Argument.Path('directory', { pathType: 'directory', mustExist: true }).pipe(Argument.withDescription('Project folder, the workspace root for repository resources')),
        target: Argument.Literals('target', Record.keys(hostingTargets)).pipe(Argument.optional, Argument.withDescription('Hosting target, the output tag of the project when absent')),
        apply: Flag.Boolean('apply').pipe(Flag.withDefault(false), Flag.withDescription('Applies the printed difference')),
        destroy: Flag.Boolean('destroy').pipe(Flag.withDefault(false), Flag.withDescription('Removes every resource and the stack in place of reconciling them')),
    },
    Effect.fn('infra')(function* ({ directory, target, apply, destroy }) {
        const path = yield* Path.Path;
        const relative = path.relative(workspaceRoot, path.resolve(directory));
        const mode = { apply, destroy };
        if (relative === '') {
            return yield* reconcile(repositoryStack, mode);
        }
        const stack = yield* hosted(relative, target);
        yield* reconcile(stack, mode);
    }),
).pipe(Command.run({ version }), Effect.provide(layer), runMain);
