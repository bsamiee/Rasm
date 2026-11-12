import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Function, Glob, Path, Record, Result, Schema } from 'effect';
import { executor } from '#/executor';
import { ran, ToolError } from '#/process';

// --- [MODELS] --------------------------------------------------------------------------

const Commands = Schema.Array(Schema.NonEmptyArray(Schema.String));
const Targets = Schema.Array(Schema.String);
const Clean = Schema.Struct({
    targets: Targets.annotate({ description: 'Targets of the project, each declared output of every configuration is removed', default: [] }),
    commands: Commands.annotate({ description: "The tool's own clean commands, each an argument list", default: [] }),
    cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
}).annotate({ title: 'Clean', description: "Removes the outputs the named targets of the project declare, keeping any the workspace tracks as source, and runs the tool's own clean commands when given" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const declared = Effect.fnUntraced(function* (context: ExecutorContext, project: string, targets: readonly string[]) {
    const path = yield* Path.Path;
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [outputs, negated] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Result.liftPredicate(
            (output: string) => !output.startsWith('!'),
            (output) => output.slice(1),
        ),
    );
    return { negated, outputs: outputs.filter((output) => !(output.startsWith('..') || path.isAbsolute(output))) };
});

const cleared = Effect.fnUntraced(function* (root: string, project: string, output: string, negated: readonly string[]) {
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
    yield* Effect.forEach(removed, (at) => Effect.andThen(fs.remove(path.join(root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Clean,
    Effect.fnUntraced(function* ({ targets, commands, cwd }, context, project) {
        const path = yield* Path.Path;
        const { outputs, negated } = yield* declared(context, project, targets);
        yield* Effect.forEach(outputs, (output) => cleared(context.root, project, output, negated), { discard: true });
        const codes = yield* Effect.forEach(commands, (command) => Effect.map(ran(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(context.root, cwd), stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }), ({ code }) => code));
        return codes.every((code) => code === 0);
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
