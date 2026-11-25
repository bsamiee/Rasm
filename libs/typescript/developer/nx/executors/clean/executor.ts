import { glob } from 'node:fs/promises';
import { posix } from 'node:path';
import { getOutputsForTargetAndConfiguration, normalizePath } from '@nx/devkit';
import { globWithWorkspaceContext, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, Function, Glob, Order, Path, Record, Result, Schema } from 'effect';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { executor, spawned, Words } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String).annotate({ description: 'Targets of the project, each declared output of every configuration is removed', default: [] }),
    commands: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own clean commands, each an argument list", default: [] }),
    cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    folders: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: "Folders whose entries are removed, keeping the entry holding Nx's workspace data, each an environment variable naming a folder followed by path segments under it", default: [] }),
    patterns: Schema.Array(Schema.String)
        .annotate({ description: 'Glob patterns relative to the workspace root, each match is removed whole unless the workspace tracks it or a file under it', default: [] })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    __unparsed__: Words,
}).annotate({
    title: 'Clean',
    description: "Runs the tool's own clean commands when given, then removes the outputs the named targets of the project declare and the matches of the named patterns, keeping any the workspace tracks as source, and empties the named folders",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const cleared = Effect.fnUntraced(function* (root: string, project: string, output: string, negated: readonly string[], tracked: ReadonlySet<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = Array.takeWhile(output.split('/'), (segment) => Glob.escape(segment) === segment).join('/') || '.';
    const [matched = [], held = []] = getFilesForOutputsBatch(root, [[output, ...negated], [base]]);
    const [removable, kept] = Array.partition(
        matched,
        Result.liftPredicate((at: string) => !tracked.has(at), Function.identity),
    );
    const whole = base === output && kept.length === 0 && (yield* fs.exists(path.join(root, base)));
    const removed = whole || (Array.isReadonlyArrayNonEmpty(held) && removable.length === held.length) ? [base] : removable;
    yield* Effect.forEach(kept, (at) => Effect.logInfo(`Kept ${at}, a declared output the workspace tracks as source`), { discard: true });
    yield* Effect.forEach(removed, (at) => Effect.andThen(fs.remove(path.join(root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)), { discard: true });
    const parents = Array.dedupe(
        [base, ...removed].flatMap((at) =>
            posix
                .relative(project, at)
                .split('/')
                .slice(0, -1)
                .map((_, depth, segments) => posix.join(project, ...segments.slice(0, depth + 1))),
        ),
    ).filter((parent) => !posix.relative(project, parent).startsWith('..'));
    yield* Effect.forEach(
        Array.sort(parents, Order.flip(Order.String)),
        Effect.fnUntraced(function* (parent) {
            const empty = (yield* fs.exists(path.join(root, parent))) && (yield* fs.readDirectory(path.join(root, parent))).length === 0;
            yield* empty ? Effect.andThen(fs.remove(path.join(root, parent), { recursive: true }), Effect.logInfo(`Removed ${parent}, left empty`)) : Effect.void;
        }),
        { discard: true },
    );
});

const emptied = Effect.fnUntraced(function* ([variable, ...segments]: Array.NonEmptyReadonlyArray<string>) {
    const [fs, path, named] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String(variable)]);
    const folder = path.join(named, ...segments);
    const entries = (yield* fs.exists(folder)) ? yield* fs.readDirectory(folder) : [];
    const removable = entries.filter((entry) => path.relative(path.join(folder, entry), workspaceDataDirectory).startsWith('..'));
    yield* Effect.forEach(removable, (entry) => Effect.andThen(fs.remove(path.join(folder, entry), { recursive: true, force: true }), Effect.logInfo(`Removed ${path.join(folder, entry)}`)), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Clean, function* ({ targets, commands, cwd, folders, patterns, __unparsed__: words = [] }, context) {
    const [fs, path, project] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.fromNullishOr(context.projectName)]);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const selections = targets.flatMap((target) => [{ target }, ...Record.keys(node.data.targets?.[target]?.configurations ?? {}).map((configuration) => ({ target, configuration }))]);
    const [outputs, negated] = Array.partition(
        Array.dedupe(selections.flatMap((selected) => getOutputsForTargetAndConfiguration({ project, ...selected }, {}, node))),
        Result.liftPredicate((output: string) => !output.startsWith('!'), Function.identity),
    );
    yield* Effect.forEach(commands, (command) => spawned(Array.appendAll(command, words), { cwd: path.join(context.root, cwd) }), { discard: true });
    const tracked = new Set(
        yield* Effect.promise(() =>
            globWithWorkspaceContext(
                context.root,
                outputs.flatMap((output) => [output, posix.join(output, '**')]),
            ),
        ),
    );
    yield* Effect.forEach(outputs, (output) => cleared(context.root, node.data.root, output, negated, tracked), { discard: true });
    yield* Effect.forEach(folders, emptied, { discard: true });
    const [matched, sourced] = yield* Array.match(patterns, {
        onEmpty: () => Effect.succeed([[], []]),
        onNonEmpty: (globs) =>
            Effect.all([
                Effect.promise(() => globalThis.Array.fromAsync(glob([...globs], { cwd: context.root }))),
                Effect.promise(() =>
                    globWithWorkspaceContext(
                        context.root,
                        globs.flatMap((pattern) => [pattern, posix.join(pattern, '**')]),
                    ),
                ),
            ]),
    });
    const scattered = matched.map(normalizePath).filter((at) => !sourced.some((file) => file === at || file.startsWith(`${at}/`)));
    yield* Effect.forEach(scattered, (at) => Effect.andThen(fs.remove(path.join(context.root, at), { recursive: true, force: true }), Effect.logInfo(`Removed ${at}`)), { discard: true });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Clean, execute };
