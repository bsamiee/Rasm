import { posix } from 'node:path';
import { getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, Filter, Glob, Order, Path, Record, Schema } from 'effect';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { above, below, executor, inside, spawned, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String).annotate({ description: 'Targets of the project, each declared output of every configuration is removed', default: [] }),
    commands: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own clean commands, each an argument list", default: [] }),
    cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    folders: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: "Folders whose entries are removed, keeping the entry holding Nx's workspace data, each an environment variable naming a folder followed by path segments under it", default: [] }),
    patterns: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Glob patterns relative to the workspace root, each match is removed whole unless the workspace tracks it or a file under it', default: [] }),
    __unparsed__: Words,
}).annotate({
    title: 'Clean',
    description: "Runs the tool's own clean commands when given, then removes the outputs the named targets of the project declare and the matches of the named patterns, keeping any the workspace tracks as source, and empties the named folders",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Clean, function* ({ targets, commands, cwd, folders, patterns = [], __unparsed__: words = [] }, context) {
    const [fs, path, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(context.projectName)]);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [negated, outputs] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Filter.fromPredicate((output: string) => output.startsWith('!')),
    );
    yield* Effect.forEach(commands, (command) => spawned(Array.appendAll(command, words), { cwd: path.join(context.root, cwd) }), { discard: true });
    const globs = [...outputs, ...patterns].flatMap((glob) => [glob, posix.join(glob, '**')]);
    const tracked = new Set(Array.isReadonlyArrayNonEmpty(globs) ? (yield* Effect.promise(() => globWithWorkspaceContext(context.root, globs))).flatMap(above) : []);
    const removed = yield* Effect.forEach(
        outputs,
        Effect.fnUntraced(function* (output) {
            const base = Array.takeWhile(output.split('/'), (segment) => Glob.escape(segment) === segment).join('/') || '.';
            const [matched = [], held = []] = getFilesForOutputsBatch(context.root, [[output, ...negated], [base]]);
            const [kept, removable] = Array.partition(
                matched,
                Filter.fromPredicate((at: string) => tracked.has(at)),
            );
            const swept = removable.length === held.length && (Array.isReadonlyArrayNonEmpty(held) || (base === output && (yield* fs.exists(path.join(context.root, base))))) ? [base] : removable;
            yield* Effect.forEach(kept, (at) => Effect.logInfo(`Kept ${at}, a declared output the workspace tracks as source`), { discard: true });
            yield* Effect.forEach(swept, (at) => Effect.andThen(fs.remove(path.join(context.root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)), { discard: true });
            return [base, ...swept];
        }),
    );
    const parents = Array.dedupe(removed.flat().flatMap((at) => above(posix.dirname(at)))).filter(below(node.data.root));
    yield* Effect.forEach(
        Array.sort(parents, Order.flip(Order.String)),
        Effect.fnUntraced(function* (parent) {
            const empty = (yield* fs.exists(path.join(context.root, parent))) && (yield* fs.readDirectory(path.join(context.root, parent))).length === 0;
            yield* empty ? Effect.andThen(fs.remove(path.join(context.root, parent), { recursive: true }), Effect.logInfo(`Removed ${parent}, left empty`)) : Effect.void;
        }),
        { discard: true },
    );
    yield* Effect.forEach(
        folders,
        Effect.fnUntraced(function* ([variable, ...segments]) {
            const folder = path.join(yield* Config.String(variable), ...segments);
            const entries = (yield* fs.glob('*', { root: folder, dot: true })).map((entry) => path.join(folder, entry)).filter((entry) => !inside(entry)(workspaceDataDirectory));
            yield* Effect.forEach(entries, (entry) => Effect.andThen(fs.remove(entry, { recursive: true, force: true }), Effect.logInfo(`Removed ${entry}`)), { discard: true });
        }),
        { discard: true },
    );
    const matched = yield* Effect.forEach(patterns, (pattern) => fs.glob(pattern, { root: context.root }));
    yield* Effect.forEach(
        matched.flat().filter((at) => !tracked.has(at)),
        (at) => Effect.andThen(fs.remove(path.join(context.root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)),
        { discard: true },
    );
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Clean, execute };
