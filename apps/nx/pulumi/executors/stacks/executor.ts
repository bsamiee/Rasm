import { createRequire } from 'node:module';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import type { InlineProgramArgs, LocalWorkspace, Stack, StackNotFoundError } from '@pulumi/pulumi/automation/index.js';
import { Array, Config, Console, Data, Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type StackRequest = Data.TaggedEnum<{
    Install: { readonly context: ExecutorContext };
    Deploy: { readonly context: ExecutorContext; readonly stack: Option.Option<string> };
    Release: { readonly context: ExecutorContext; readonly version: string };
}>;
type Stacks = (request: StackRequest) => readonly InlineProgramArgs[] | PromiseLike<readonly InlineProgramArgs[]>;
interface Automation {
    readonly LocalWorkspace: typeof LocalWorkspace;
    readonly StackNotFoundError: typeof StackNotFoundError;
}
type Run = (stack: Stack, handlers: { readonly signal: AbortSignal; readonly onOutput: (out: string) => void }) => Promise<Pick<typeof Line.Type, 'changes' | 'outputs'>>;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    programs: Schema.NonEmptyArray(Schema.String),
    stack: Schema.optionalKey(Schema.String),
    destroy: Schema.optionalKey(Schema.Boolean),
    dryRun: Schema.optionalKey(Schema.Boolean),
    nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))),
});
const Line = Schema.Struct({
    project: Schema.String,
    stack: Schema.String,
    changes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Finite)),
    outputs: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)),
    missing: Schema.optionalKey(Schema.Boolean),
    error: Schema.optionalKey(Schema.String),
});

// --- [ERRORS] --------------------------------------------------------------------------

class StackError extends Schema.TaggedError<StackError, { readonly brand: unique symbol }>()('StackError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause: unknown) => StackError.make({ subject, detail, cause });

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(Predicate.isError(this.cause) ? [this.cause.message] : [])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const OPERATIONS = {
    up: {
        create: true,
        run: ((stack, handlers) =>
            stack.up({ ...handlers, refresh: true, runProgram: true }).then(({ summary, outputs }) => ({
                changes: summary.resourceChanges ?? {},
                outputs: Record.map(
                    Record.filter(outputs, ({ secret }) => !secret),
                    ({ value }): unknown => value,
                ),
            }))) satisfies Run,
    },
    preview: {
        create: false,
        run: ((stack, handlers) => stack.preview({ ...handlers, refresh: true, runProgram: true }).then(({ changeSummary }) => ({ changes: changeSummary }))) satisfies Run,
    },
    destroy: {
        create: false,
        run: ((stack, handlers) => stack.destroy({ ...handlers, refresh: true, runProgram: true, remove: true }).then(({ summary }) => ({ changes: summary.resourceChanges ?? {} }))) satisfies Run,
    },
} as const;

const declared = (value: unknown): value is { readonly stacks: Stacks } => Predicate.hasProperty(value, 'stacks') && Predicate.isFunction(value.stacks);

const applied = Effect.fnUntraced(function* (automation: Automation, args: InlineProgramArgs, operation: (typeof OPERATIONS)[keyof typeof OPERATIONS]) {
    const fs = yield* FileSystem.FileSystem;
    const subject = { project: args.projectName, stack: args.stackName };
    const workDir = yield* fs.makeTempDirectoryScoped();
    const selected = Effect.tryPromise({ try: () => (operation.create ? automation.LocalWorkspace.createOrSelectStack(args, { workDir }) : automation.LocalWorkspace.selectStack(args, { workDir })), catch: StackError.of(args.stackName, 'cannot select the stack') });
    return yield* Effect.flatMap(selected, (stack) => Effect.tryPromise({ try: (signal) => operation.run(stack, { signal, onOutput: (out) => process.stderr.write(out) }), catch: StackError.of(args.stackName, 'failed') })).pipe(
        Effect.match({
            onSuccess: (result): typeof Line.Type => ({ ...subject, ...result }),
            onFailure: (error): typeof Line.Type => (!operation.create && error.cause instanceof automation.StackNotFoundError ? { ...subject, missing: true } : { ...subject, error: error.message }),
        }),
    );
}, Effect.scoped);

const programmed = Effect.fnUntraced(function* (context: ExecutorContext, program: string, request: StackRequest, operation: keyof typeof OPERATIONS) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const { root } = yield* Effect.fromNullishOr(context.projectsConfigurations.projects[project]);
    const entry = yield* Effect.try({ try: () => createRequire(path.join(context.root, root, 'package.json')).resolve(program), catch: StackError.of(program, `unresolvable from ${root}`) });
    const module: unknown = yield* Effect.tryPromise({ try: () => import(pathToFileURL(entry).href), catch: StackError.of(program, `failed to load ${entry}`) });
    const automation: Automation = yield* Effect.try({ try: () => createRequire(entry)('@pulumi/pulumi/automation/index.js'), catch: StackError.of(program, 'declares no @pulumi/pulumi') });
    const { stacks } = declared(module) ? module : yield* StackError.make({ subject: program, detail: `${entry} exports no stacks function` });
    const held = yield* Effect.tryPromise({ try: () => Promise.resolve(stacks(request)), catch: StackError.of(program, 'stacks failed') });
    return yield* Effect.forEach(held, (args) => applied(automation, args, OPERATIONS[operation]), { concurrency: 'unbounded' });
});

const run = (request: (options: typeof Options.Type, context: ExecutorContext) => Effect.Effect<Option.Option<{ readonly request: StackRequest; readonly operation: keyof typeof OPERATIONS }>, StackError, never>) => (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const decoded = yield* Schema.decodeUnknownEffect(Options)(options);
            const selected = yield* request(decoded, context);
            const lines = yield* Option.match(selected, {
                onNone: () => Effect.succeed(Array.empty<typeof Line.Type>()),
                onSome: ({ request: chosen, operation }) =>
                    Effect.map(
                        Effect.forEach(decoded.programs, (program) => programmed(context, program, chosen, operation), { concurrency: 'unbounded' }),
                        Array.flatten,
                    ),
            });
            yield* Effect.forEach(lines, (line) => Effect.flatMap(Schema.encodeEffect(Schema.fromJsonString(Line))(line), Console.log));
            return { success: lines.every(({ error }) => error === undefined) };
        }).pipe(Effect.provide(layer)),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const install = run((_, context) => Effect.succeedSome({ request: Data.taggedEnum<StackRequest>().Install({ context }), operation: 'up' }));

const deploy = run(({ stack, destroy }, context) => Effect.succeedSome({ request: Data.taggedEnum<StackRequest>().Deploy({ context, stack: Option.fromUndefinedOr(stack) }), operation: destroy === true ? 'destroy' : 'up' }));

const publish = run(({ dryRun, nxReleaseVersionData }, context) =>
    Effect.gen(function* () {
        const project = yield* Effect.fromNullishOr(context.projectName).pipe(Effect.mapError(StackError.of('nx-release-publish', 'runs without a project')));
        const version = yield* Effect.fromNullishOr(nxReleaseVersionData?.[project]).pipe(Effect.mapError(StackError.of(project, 'has no version from nx release; run it through nx release publish')));
        const dry = yield* Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false), Effect.orDie);
        return Option.map(Option.fromNullOr(version.newVersion), (released) => ({ request: Data.taggedEnum<StackRequest>().Release({ context, version: released }), operation: dryRun === true || dry ? 'preview' : 'up' }) as const);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { deploy, install, publish, type StackRequest, type Stacks };
