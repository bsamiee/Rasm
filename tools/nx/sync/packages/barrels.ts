import { globAsync, type Tree } from '@nx/devkit';
import { Array, Effect, Equivalence, Filter, Function, Option, Path, Record, String, Struct } from 'effect';
import { parseSync } from 'rolldown/utils';
import { accumulate } from '#/eng/typescript/accumulate';
import { ClaudeHooks, libraryRoot, Package } from '#/eng/typescript/manifest';
import { decodeTree, formatted, syncGenerator, treeText } from '#/tools/nx/sync/tree';
import { WorkspaceError } from '#/tools/nx/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BARREL = 'index.ts';
const DIVIDER_COLUMN = 90;
const DIVIDER = '// --- [EXPORTS] '.padEnd(DIVIDER_COLUMN, '-');

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]
const exportedNames = (file: string, text: string) => {
    const { module, errors } = parseSync(file, text);
    return Array.match(
        errors.filter(({ severity }) => severity === 'Error'),
        {
            onEmpty: () =>
                Effect.succeed(
                    Array.dedupeWith(
                        Array.getSomes(
                            module.staticExports.flatMap(Struct.get('entries')).map(({ moduleRequest, exportName: { kind, name }, isType }) =>
                                Option.map(
                                    Option.filter(Option.fromNullOr(name), (held) => moduleRequest === null && kind === 'Name' && held !== 'default'),
                                    (held) => ({ name: held, type: isType }),
                                ),
                            ),
                        ),
                        Equivalence.mapInput(Equivalence.String, Struct.get('name')),
                    ),
                ),
            onNonEmpty: ([{ message }]) => Effect.fail(WorkspaceError.unparsed(file, message)),
        },
    );
};

// --- [LAYOUT]
const selfImport = (imports: Readonly<Record<string, string>>, module: string) => Array.findFirst(Record.toEntries(imports), ([key, target]) => Option.map(Option.fromUndefinedOr(new RegExp(`^${target.split('*').map(RegExp.escape).join('(.+)')}$`, 'u').exec(`./${module}`)?.[1]), (stem) => key.replace('*', stem)));
const exportLine = (imports: Readonly<Record<string, string>>, relative: string, file: string, exported: Effect.Success<ReturnType<typeof exportedNames>>) =>
    Array.match(exported, {
        onEmpty: Array.empty,
        onNonEmpty: (names) => [`export { ${names.map(({ name, type }) => (type ? `type ${name}` : name)).join(', ')} } from '${Option.getOrElse(selfImport(imports, file), Function.constant(relative))}';`],
    });
const ownerFolder = (folders: readonly string[], separator: string) => (file: string) => Option.getOrUndefined(Array.findFirst(folders, (folder) => folder === '.' || file.startsWith(`${folder}${separator}`)));
const barrel = Effect.fnUntraced(function* (tree: Tree, dir: string, imports: Readonly<Record<string, string>>, folder: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const file = path.join(dir, folder, BARREL);
    const lines = yield* Effect.forEach(
        files,
        Effect.fnUntraced(function* (module: string) {
            const source = path.join(dir, module);
            return exportLine(imports, `./${path.relative(folder, module)}`, module, yield* exportedNames(source, yield* treeText(tree, source)));
        }),
    );
    return { file, next: yield* formatted(tree, file, [DIVIDER, '', ...lines.flat(), ''].join('\n')) };
});
const libraryBarrels = Effect.fnUntraced(function* (tree: Tree, dir: string, manifest: typeof Package.schema.Type, files: readonly string[]) {
    const path = yield* Path.Path;
    const [hooks, sources] = Array.partition(files, Filter.fromPredicate(String.endsWith(ClaudeHooks.file)));
    const modules = sources.map((file) => path.relative(dir, file)).filter((file) => !(file.endsWith('.d.ts') || file.endsWith('.config.ts') || file.split(path.sep).some(String.startsWith('.'))));
    const stem = (file: string) => path.join(path.dirname(file), path.parse(file).name);
    yield* accumulate(
        modules.filter((file) => modules.some(String.startsWith(`${stem(file)}${path.sep}`))),
        (file) => WorkspaceError.shadowed(path.join(dir, file), stem(file)),
    );
    const declared = yield* Effect.forEach(hooks, (file) => Effect.map(decodeTree(tree, ClaudeHooks.schema, file), Struct.assign({ file })));
    const programs = Array.bind(declared, 'module', Struct.get('modules')).map(({ file, module }) => path.relative(dir, path.join(path.dirname(file), module)));
    const entries = Object.values(manifest.exports).map(path.normalize);
    const folders = entries
        .filter((entry) => path.basename(entry) === BARREL)
        .map(path.dirname)
        .toSorted((left, right) => right.length - left.length);
    const exported = modules.filter((file) => !(entries.includes(file) || programs.includes(file) || file.split(path.sep).includes('internal')));
    const byFolder = Map.groupBy(exported, ownerFolder(folders, path.sep));
    const grouped = folders.map((folder) => ({ folder, owned: byFolder.get(folder) ?? [] }));
    yield* accumulate(
        grouped.filter(({ owned }) => owned.length < 2),
        ({ folder, owned }) => Array.match(owned, { onEmpty: () => WorkspaceError.missing(path.join(dir, folder, BARREL), 'module to re-export'), onNonEmpty: ([module]) => WorkspaceError.single(path.join(dir, folder, BARREL), module) }),
    );
    return {
        written: yield* Effect.forEach(grouped, ({ folder, owned }) => barrel(tree, dir, manifest.imports, folder, owned), { concurrency: 'unbounded' }),
        stale: modules.filter((file) => path.basename(file) === BARREL && !folders.includes(path.dirname(file)) && !file.split(path.sep).includes('internal')).map((file) => path.join(dir, file)),
    };
});

const rows = Effect.fnUntraced(function* (tree: Tree) {
    const [path, candidates] = yield* Effect.all([Path.Path, Effect.tryPromise(() => globAsync(tree, [`**/${Package.file}`, '**/*.{ts,tsx}', `**/${ClaudeHooks.file}`]))]);
    const [packageFiles, files] = Array.partition(candidates, Filter.fromPredicate(String.endsWith(Package.file)));
    const manifests = yield* Effect.forEach(packageFiles, (file) => Effect.map(decodeTree(tree, Package.schema, file), (manifest) => ({ dir: path.dirname(file), manifest })), { concurrency: 'unbounded' });
    const directories = manifests.map(Struct.get('dir')).toSorted((left, right) => right.length - left.length);
    const byPackage = Map.groupBy(files, ownerFolder(directories, path.sep));
    const libraries = yield* Effect.forEach(
        manifests.filter(({ manifest }) => Option.isSome(libraryRoot(manifest))),
        ({ dir, manifest }) => libraryBarrels(tree, dir, manifest, byPackage.get(dir) ?? []),
        { concurrency: 'unbounded' },
    );
    return [...libraries.flatMap(Struct.get('written')), ...libraries.flatMap(Struct.get('stale')).map((file) => ({ file }))];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const barrels = syncGenerator("Library barrels differ from each library's exports and modules", rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { barrels };
