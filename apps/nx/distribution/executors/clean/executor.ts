import { type ExecutorContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Array, Effect, FileSystem, Function, Glob, Path, Record, Result, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { getFilesForOutputsBatch } from 'nx/src/native/index';
import { run } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String).annotate({ description: 'Targets of the project, each declared output of every configuration is removed', default: [] }),
    commands: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own clean commands, each an argument list", default: [] }),
    cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
}).annotate({ title: 'Clean', description: "Runs the tool's own clean commands when given, then removes the outputs the named targets of the project declare, keeping any the workspace tracks as source" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const cleared = Effect.fnUntraced(function* (root: string, output: string, negated: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = Array.takeWhile(output.split('/'), (segment) => Glob.escape(segment) === segment).join('/') || '.';
    const [matched = [], held = []] = getFilesForOutputsBatch(root, [[output, ...negated], [base]]);
    const tracked = new Set(yield* Effect.promise(() => globWithWorkspaceContext(root, [output, path.join(output, '**')])));
    const [removable, kept] = Array.partition(
        matched,
        Result.liftPredicate((at: string) => !tracked.has(at), Function.identity),
    );
    const removed = Array.isReadonlyArrayNonEmpty(held) && removable.length === held.length ? [base] : removable;
    yield* Effect.forEach(kept, (at) => Effect.logInfo(`Kept ${at}, a declared output the workspace tracks as source`), { discard: true });
    yield* Effect.forEach(removed, (at) => Effect.andThen(fs.remove(path.join(root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ targets, commands, cwd }: typeof Clean.Type, context: ExecutorContext) {
    const [path, project] = yield* Effect.all([Path.Path, Effect.fromNullishOr(context.projectName)]);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [outputs, negated] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Result.liftPredicate((output: string) => !output.startsWith('!'), Function.identity),
    );
    yield* Effect.forEach(commands, ([command, ...args]) => Completed.output(ChildProcess.make(command, args, { cwd: path.join(context.root, cwd), detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })), { discard: true });
    yield* Effect.forEach(outputs, (output) => cleared(context.root, output, negated), { discard: true });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Clean, execute };
