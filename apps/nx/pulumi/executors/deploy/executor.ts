import { createRequire } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import type { InlineProgramArgs, LocalWorkspaceOptions, Stack, UpOptions } from '@pulumi/pulumi/automation/index.js';
import { Console, Data, Effect, FileSystem, Formatter, Function, Option, Path, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type StackRequest = Data.TaggedEnum<{
    Install: { readonly context: ExecutorContext };
    Deploy: { readonly context: ExecutorContext; readonly stack: Option.Option<string> };
    Release: { readonly context: ExecutorContext; readonly version: string };
}>;
type Stacks = (request: StackRequest) => Promise<readonly (InlineProgramArgs & LocalWorkspaceOptions)[]>;

// --- [MODELS] --------------------------------------------------------------------------

const StackRequest = Data.taggedEnum<StackRequest>();
const Options = Schema.Struct({
    programs: Schema.NonEmptyArray(Schema.String),
    stack: Schema.optionalKey(Schema.String),
    destroy: Schema.optionalKey(Schema.Boolean),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
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

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Schema.TaggedError<StackError, { readonly brand: unique symbol }>()('StackError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause: unknown) => StackError.make({ subject, detail, cause });

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

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
    return yield* Effect.tryPromise({ try: () => (operation.create ? automation.LocalWorkspace.createOrSelectStack(args, workspace) : automation.LocalWorkspace.selectStack(args, workspace)), catch: StackError.of(args.stackName, 'cannot select the stack') }).pipe(
        Effect.flatMap((stack) => Effect.tryPromise({ try: (signal) => operation.run(stack, { signal, onOutput: (out) => process.stderr.write(out) }), catch: StackError.of(args.stackName, 'failed') })),
        Effect.map((result): typeof Line.Type => ({ ...subject, ...result })),
        Effect.catchIf(
            (error) => !operation.create && error.cause instanceof automation.StackNotFoundError,
            (): Effect.Effect<typeof Line.Type> => Effect.succeed({ ...subject, missing: true }),
        ),
        Effect.match({ onSuccess: Function.identity, onFailure: (error): typeof Line.Type => ({ ...subject, error: error.message }) }),
    );
}, Effect.scoped);

const programmed = Effect.fnUntraced(function* (entry: string, { program, request, operation }: { readonly program: string; readonly request: StackRequest; readonly operation: keyof typeof OPERATIONS }) {
    const resolved = yield* Effect.try({ try: () => createRequire(entry).resolve(program), catch: StackError.of(program, `unresolvable from ${entry}`) });
    const module: unknown = yield* Effect.tryPromise({ try: () => import(pathToFileURL(resolved).href), catch: StackError.of(program, `failed to load ${resolved}`) });
    const automation: typeof import('@pulumi/pulumi/automation/index.js') = yield* Effect.try({ try: () => createRequire(resolved)('@pulumi/pulumi/automation/index.js'), catch: StackError.of(program, 'declares no @pulumi/pulumi') });
    const { stacks } = yield* Effect.mapError(Schema.decodeUnknownEffect(Program)(module), StackError.of(program, `${resolved} exports no stacks function`));
    const selected = yield* Effect.tryPromise({ try: () => stacks(request), catch: StackError.of(program, 'stacks failed') });
    return yield* Effect.forEach(selected, (args) => applied(automation, args, OPERATIONS[operation]), { concurrency: 'unbounded' });
});

const run = (select: (options: typeof Options.Type, context: ExecutorContext, project: string) => Effect.Effect<Option.Option<{ readonly request: StackRequest; readonly operation: keyof typeof OPERATIONS }>, StackError>) => {
    const executed = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
        const [path, decoded, project] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Options)(options), Effect.fromNullishOr(context.projectName)]);
        const { root } = yield* Effect.fromNullishOr(context.projectsConfigurations.projects[project]);
        const chosen = Option.toArray(yield* select(decoded, context, project));
        const [reports, failures] = yield* Effect.partition(
            chosen.flatMap((selected) => decoded.programs.map((program) => ({ ...selected, program }))),
            (selected) => programmed(path.join(context.root, root, 'package.json'), selected),
            { concurrency: 'unbounded' },
        );
        const lines = reports.flat();
        yield* Effect.forEach(lines, (line) => Effect.flatMap(Schema.encodeEffect(Line)(line), Console.log));
        yield* Effect.forEach(failures, (failure) => Console.error(failure.message));
        return { success: failures.length === 0 && lines.every(({ error }) => error === undefined) };
    }, Effect.provide(layer));
    return (options: unknown, context: ExecutorContext) => Effect.runPromise(executed(options, context));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = run(({ stack, destroy }, context) => Effect.succeedSome({ request: StackRequest.Deploy({ context, stack: Option.fromUndefinedOr(stack) }), operation: destroy === true ? 'destroy' : 'up' }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, run, StackError, StackRequest, type Stacks };
