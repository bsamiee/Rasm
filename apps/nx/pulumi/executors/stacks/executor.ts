import { createRequire } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext } from '@nx/devkit';
import type { InlineProgramArgs, LocalWorkspaceOptions, Stack, UpOptions } from '@pulumi/pulumi/automation/index.js';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Console, Data, Effect, FileSystem, Function, Match, Option, Path, Record, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type StackRequest = Data.TaggedEnum<{
    Install: { readonly context: ExecutorContext };
    Deploy: { readonly context: ExecutorContext; readonly stack: Option.Option<string> };
    Release: { readonly context: ExecutorContext; readonly version: string };
}>;
type Stacks = (request: StackRequest) => Promise<readonly (InlineProgramArgs & LocalWorkspaceOptions)[]>;

// --- [MODELS] --------------------------------------------------------------------------

const StackRequest = Data.taggedEnum<StackRequest>();
const Options = Released.mapFields(
    Struct.assign({
        programs: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Program packages, each resolved from the project and exporting stacks(request)' }),
        request: Schema.Literals(['Install', 'Deploy', 'Release']).annotate({ description: "What the run asks each program's stacks for: a local install, a deploy, or a released version", default: 'Deploy' }),
        stack: Schema.optionalKey(Schema.String).annotate({ description: 'Name of the one stack a deploy applies, absent applies every stack' }),
        destroy: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Destroys the stacks of a deploy and removes them' }),
    }),
).annotate({ title: 'Stacks', description: 'Applies the stacks a program package states for a project through the Pulumi Automation API, refreshing live state first, previewing a release under a dry run' });
const Line = Schema.fromJsonString(
    Schema.Struct({
        project: Schema.String,
        stack: Schema.String,
        changes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Finite)),
        outputs: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)),
        missing: Schema.optionalKey(Schema.Boolean),
        error: Schema.optionalKey(Schema.String),
    }),
);
const Program = Schema.Struct({ stacks: Schema.declare((value: unknown): value is Stacks => typeof value === 'function') });

// --- [OPERATIONS] ----------------------------------------------------------------------

const OPERATIONS = {
    up: {
        create: true,
        run: (stack, handlers) =>
            stack.up({ ...handlers, refresh: true, runProgram: true }).then(({ summary, outputs }) => ({
                changes: summary.resourceChanges ?? {},
                outputs: Record.map(
                    Record.filter(outputs, ({ secret }) => !secret),
                    ({ value }): unknown => value,
                ),
            })),
    },
    preview: {
        create: false,
        run: (stack, handlers) => stack.preview({ ...handlers, refresh: true, runProgram: true }).then(({ changeSummary }) => ({ changes: changeSummary })),
    },
    destroy: {
        create: false,
        run: (stack, handlers) => stack.destroy({ ...handlers, refresh: true, runProgram: true, remove: true }).then(({ summary }) => ({ changes: summary.resourceChanges ?? {} })),
    },
} as const satisfies Record<string, { readonly create: boolean; readonly run: (stack: Stack, handlers: Required<Pick<UpOptions, 'signal' | 'onOutput'>>) => Promise<Pick<typeof Line.Type, 'changes' | 'outputs'>> }>;

const applied = Effect.fnUntraced(function* (automation: typeof import('@pulumi/pulumi/automation/index.js'), args: InlineProgramArgs & LocalWorkspaceOptions, operation: (typeof OPERATIONS)[keyof typeof OPERATIONS]) {
    const fs = yield* FileSystem.FileSystem;
    const subject = { project: args.projectName, stack: args.stackName };
    const workspace = { workDir: yield* fs.makeTempDirectoryScoped(), ...args };
    return yield* Effect.tryPromise({ try: () => (operation.create ? automation.LocalWorkspace.createOrSelectStack(args, workspace) : automation.LocalWorkspace.selectStack(args, workspace)), catch: ToolError.of(args.stackName, 'cannot select the stack') }).pipe(
        Effect.flatMap((stack) => Effect.tryPromise({ try: (signal) => operation.run(stack, { signal, onOutput: (out) => process.stderr.write(out) }), catch: ToolError.of(args.stackName, 'failed') })),
        Effect.map((result): typeof Line.Type => ({ ...subject, ...result })),
        Effect.catchIf(
            (error) => !operation.create && error.cause instanceof automation.StackNotFoundError,
            (): Effect.Effect<typeof Line.Type> => Effect.succeed({ ...subject, missing: true }),
        ),
        Effect.match({ onSuccess: Function.identity, onFailure: (error): typeof Line.Type => ({ ...subject, error: error.message }) }),
    );
}, Effect.scoped);

const programmed = Effect.fnUntraced(function* (entry: string, program: string, request: StackRequest, operation: keyof typeof OPERATIONS) {
    const resolved = yield* Effect.try({ try: () => createRequire(entry).resolve(program), catch: ToolError.of(program, `unresolvable from ${entry}`) });
    const module: unknown = yield* Effect.tryPromise({ try: () => import(pathToFileURL(resolved).href), catch: ToolError.of(program, `failed to load ${resolved}`) });
    const automation: typeof import('@pulumi/pulumi/automation/index.js') = yield* Effect.try({ try: () => createRequire(resolved)('@pulumi/pulumi/automation/index.js'), catch: ToolError.of(program, 'declares no @pulumi/pulumi') });
    const { stacks } = yield* Effect.mapError(Schema.decodeUnknownEffect(Program)(module), ToolError.of(program, `${resolved} exports no stacks function`));
    const selected = yield* Effect.tryPromise({ try: () => stacks(request), catch: ToolError.of(program, 'stacks failed') });
    return yield* Effect.forEach(selected, (args) => applied(automation, args, OPERATIONS[operation]), { concurrency: 'unbounded' });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Options,
    Effect.fnUntraced(function* ({ programs, request, stack, destroy, ...stated }, context, project) {
        const [path, { dry, version, unversioned }] = yield* Effect.all([Path.Path, released(stated, project)]);
        const { root } = yield* Effect.fromNullishOr(context.projectsConfigurations.projects[project]);
        const packaged = Effect.mapError(Effect.fromNullishOr(context.projectGraph.nodes[project]?.data.metadata?.js?.packageVersion), ToolError.of(project, 'states no version in its package.json'));
        const releasing = unversioned ? Effect.succeedNone : Effect.map(Option.match(version, { onSome: Effect.succeed, onNone: () => packaged }), (set) => Option.some<StackRequest>(StackRequest.Release({ context, version: set })));
        const operation = ({ Install: 'up', Deploy: destroy === true ? 'destroy' : 'up', Release: dry ? 'preview' : 'up' } as const)[request];
        const asked = yield* Match.value(request).pipe(
            Match.when('Install', () => Effect.succeedSome<StackRequest>(StackRequest.Install({ context }))),
            Match.when('Deploy', () => Effect.succeedSome<StackRequest>(StackRequest.Deploy({ context, stack: Option.fromUndefinedOr(stack) }))),
            Match.when('Release', () => releasing),
            Match.exhaustive,
        );
        const [reports, failures] = yield* Effect.partition(
            Option.toArray(asked).flatMap((selected) => programs.map((program) => ({ selected, program }))),
            ({ selected, program }) => programmed(path.join(context.root, root, 'package.json'), program, selected, operation),
            { concurrency: 'unbounded' },
        );
        const lines = reports.flat();
        yield* Effect.forEach(lines, (line) => Effect.flatMap(Schema.encodeEffect(Line)(line), Console.log));
        yield* Effect.forEach(failures, (failure) => Console.error(failure.message));
        return failures.length === 0 && lines.every(({ error }) => error === undefined);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, StackRequest, type Stacks };
