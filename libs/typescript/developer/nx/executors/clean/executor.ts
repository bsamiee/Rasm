import { posix } from 'node:path';
import { getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { createTaskId, globWithWorkspaceContext, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, Filter, Glob, Order, Path, Record, Schema } from 'effect';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { above, below, executor, inside, NxError, spawned, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String).annotate({ description: 'Targets of the project, each declared output of every configuration is removed', default: [] }),
    commands: Schema.String.pipe(Schema.NonEmptyArray, Schema.Array).annotate({ description: "The tool's own clean commands, each an argument list", default: [] }),
    cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    folders: Schema.String.pipe(Schema.NonEmptyArray, Schema.Array).annotate({ description: "Folders whose entries are removed, keeping the entry holding Nx's workspace data, each an environment variable naming a folder followed by path segments under it", default: [] }),
    patterns: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({ description: 'Glob patterns relative to the workspace root, each match is removed whole unless the workspace tracks it or a file under it', default: [] }),
    __unparsed__: Words,
}).annotate({
    title: 'Clean',
    description: "Runs the tool's own clean commands when given, stopping at the first that fails, then removes the outputs the named targets of the project declare and the matches of the named patterns, keeping any the workspace tracks as source, and empties the named folders, attempting every removal and failing with each one that failed",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Clean, function* ({ targets, commands, cwd, folders, patterns = [], __unparsed__: words = [] }, context) {
    const [fs, path, project, targetName] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(context.projectName), Effect.fromNullishOr(context.targetName)]);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [negated, outputs] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Filter.fromPredicate((output: string) => output.startsWith('!')),
    );
    yield* Effect.forEach(commands, (command) => spawned(Array.appendAll(command, words), { cwd: path.join(context.root, cwd) }), { discard: true });
    const globs = [...outputs, ...patterns].flatMap((glob) => [glob, posix.join(glob, '**')]);
    const tracked = new Set(Array.isReadonlyArrayNonEmpty(globs) ? (yield* Effect.promise(() => globWithWorkspaceContext(context.root, globs))).flatMap(above) : []);
    const [declared, undeclared] = yield* Effect.partition(
        outputs,
        Effect.fnUntraced(function* (output) {
            const base = Array.takeWhile(output.split('/'), (segment) => Glob.escape(segment) === segment).join('/') || '.';
            const [matched = [], held = []] = getFilesForOutputsBatch(context.root, [[output, ...negated], [base]]);
            const [kept, removable] = Array.partition(
                matched,
                Filter.fromPredicate((at: string) => tracked.has(at)),
            );
            yield* Effect.forEach(kept, (at) => Effect.annotateLogs(Effect.logInfo('kept'), { path: at, tracked: true }), { discard: true });
            return { base, swept: removable.length === held.length && (Array.isReadonlyArrayNonEmpty(held) || (base === output && (yield* fs.exists(path.join(context.root, base))))) ? [base] : removable };
        }),
    );
    const [emptied, unemptied] = yield* Effect.partition(
        folders,
        Effect.fnUntraced(function* ([variable, ...segments]) {
            const folder = path.join(yield* Config.String(variable), ...segments);
            return (yield* fs.glob('*', { root: folder, dot: true })).map((entry) => path.join(folder, entry)).filter((entry) => !inside(entry)(workspaceDataDirectory));
        }),
    );
    const [matched, unmatched] = yield* Effect.partition(patterns, (pattern) => fs.glob(pattern, { root: context.root }));
    const [, unremoved] = yield* Effect.partition([...declared.flatMap(({ swept }) => swept), ...emptied.flat(), ...matched.flat().filter((at) => !tracked.has(at))], (at) =>
        Effect.andThen(fs.remove(path.resolve(context.root, at), { recursive: true, force: true }), Effect.annotateLogs(Effect.logInfo('removed'), { path: at })),
    );
    const parents = Array.dedupe(declared.flatMap(({ base, swept }) => [base, ...swept]).flatMap((at) => above(posix.dirname(at)))).filter(below(node.data.root));
    const [, unpruned] = yield* Effect.partition(
        Array.sort(parents, Order.flip(Order.String)),
        Effect.fnUntraced(function* (parent) {
            const empty = (yield* fs.exists(path.join(context.root, parent))) && (yield* fs.readDirectory(path.join(context.root, parent))).length === 0;
            yield* empty ? Effect.andThen(fs.remove(path.join(context.root, parent), { recursive: true }), Effect.annotateLogs(Effect.logInfo('removed'), { path: parent, empty: true })) : Effect.void;
        }),
    );
    const failed = [...undeclared, ...unemptied, ...unmatched, ...unremoved, ...unpruned];
    return Array.isReadonlyArrayNonEmpty(failed) ? yield* NxError.of({ _tag: 'Failed', step: 'remove' }, createTaskId(project, targetName, context.configurationName))(failed) : { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Clean, execute };
