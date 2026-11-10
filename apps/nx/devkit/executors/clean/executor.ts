import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, Function, flow, Glob, Path, Record, Result, Schema } from 'effect';
import { line } from '#/inference';
import { ran, runtime, ToolError } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String),
    commands: Schema.Array(Schema.NonEmptyArray(Schema.String)),
    previews: Schema.optionalKey(Schema.Array(Schema.NonEmptyArray(Schema.String))),
    cwd: Schema.String,
    dryRun: Schema.Boolean,
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const declared = Effect.fnUntraced(function* (context: ExecutorContext, targets: readonly string[]) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [outputs, negated] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Result.liftPredicate(
            (output: string) => !output.startsWith('!'),
            (output) => output.slice(1),
        ),
    );
    return { project, negated, outputs: outputs.filter((output) => !(output.startsWith('..') || path.isAbsolute(output))) };
});

const cleared = Effect.fnUntraced(function* (root: string, project: string, output: string, negated: readonly string[], dry: boolean) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const within = (at: string) => [at, path.join(at, '**')];
    const listed = (pattern: string, exclude: readonly string[]) =>
        Effect.mapBoth(
            Effect.forEach(within(pattern), (at) => fs.glob(at, { root, exclude: exclude.flatMap(within), dot: true }), { concurrency: 'unbounded' }),
            { onSuccess: (found) => Array.dedupe(found.flat()), onFailure: ToolError.of(project, `cannot list its declared output ${output}`) },
        );
    const base = Array.takeWhile(output.split('/'), (segment) => Glob.escape(segment) === segment).join('/') || '.';
    const [matched, under, tracked] = yield* Effect.all([listed(output, negated), listed(base, []), Effect.promise(() => globWithWorkspaceContext(root, within(output)))], { concurrency: 'unbounded' });
    const [folders, sources] = [new Set(under.map((at) => path.dirname(at))), new Set(tracked)];
    const held = under.filter((at) => !folders.has(at));
    const [removable, kept] = Array.partition(
        matched.filter((at) => !folders.has(at)),
        Result.liftPredicate((at: string) => !sources.has(at), Function.identity),
    );
    const removed = Array.isReadonlyArrayNonEmpty(held) && removable.length === held.length ? [base] : removable;
    yield* Effect.forEach(kept, (at) => Effect.logInfo(`Kept ${at}, a declared output the workspace tracks as source`), { discard: true });
    yield* Effect.forEach(removed, (at) => Effect.andThen(dry ? Effect.void : fs.remove(path.join(root, at), { recursive: true, force: true }), Effect.logInfo(`${dry ? 'Would remove' : 'Removed'} ${at}`)), { discard: true });
});

const cleaned = Effect.fnUntraced(function* (options: unknown, context: ExecutorContext) {
    const [path, { targets, commands, previews, cwd, dryRun }, environment] = yield* Effect.all([Path.Path, Schema.decodeUnknownEffect(Clean)(options), Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false))]);
    const dry = dryRun || environment;
    const { project, outputs, negated } = yield* declared(context, targets);
    yield* Effect.forEach(outputs, (output) => cleared(context.root, project, output, negated, dry), { discard: true });
    yield* Effect.forEach(dry && previews === undefined ? commands : [], (command) => Effect.logInfo(`Would run ${line(command)}`), { discard: true });
    const codes = yield* Effect.forEach(dry ? (previews ?? []) : commands, (command) => Effect.map(ran(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(context.root, cwd), stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code));
    return { success: codes.every((code) => code === 0) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = flow(cleaned, runtime.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
