import type { Tree } from '@nx/devkit';
import { Array, Effect, Equivalence, Function, Option, Path, Record, String, Struct } from 'effect';
import { parseSync } from 'vite';
import { accumulate } from '#/eng/typescript/accumulate';
import { ClaudeHooks, libraryRoot, Package } from '#/eng/typescript/manifest';
import { decodeTree, formatted, syncGenerator, treeText } from '#/tools/nx/sync/tree';
import { glob, WorkspaceError } from '#/tools/nx/workspace';

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
const ownerFolder = (folders: readonly string[], separator: string) => (file: string) => Array.findFirst(folders, (folder) => folder === '.' || file.startsWith(`${folder}${separator}`));
const readManifest = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const path = yield* Path.Path;
    return { dir: path.dirname(file), manifest: yield* decodeTree(tree, Package.schema, file) };
});
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
const libraryBarrels = Effect.fnUntraced(function* (tree: Tree, dir: string, manifest: typeof Package.schema.Type, nested: readonly string[]) {
    const path = yield* Path.Path;
    const excluded = nested.map((inner) => `${inner}/**`);
    const [files, hooks] = yield* Effect.all([glob(tree.root, [`${dir}/**/*.{ts,tsx}`], excluded), glob(tree.root, [`${dir}/**/${ClaudeHooks.file}`], excluded)]);
    const modules = files.map((file) => path.relative(dir, file)).filter((file) => !(file.endsWith('.d.ts') || file.endsWith('.config.ts') || file.split(path.sep).some(String.startsWith('.'))));
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
    const owner = ownerFolder(folders, path.sep);
    const exported = modules.filter((file) => !(entries.includes(file) || programs.includes(file) || file.split(path.sep).includes('internal')));
    const grouped = folders.map((folder) => ({ folder, owned: exported.filter(Function.flow(owner, Option.contains(folder))) }));
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
    const path = yield* Path.Path;
    const manifests = yield* Effect.forEach(yield* glob(tree.root, [`**/${Package.file}`]), (file) => readManifest(tree, file));
    const dirs = manifests.map(Struct.get('dir'));
    const libraries = yield* Effect.forEach(
        manifests.filter(({ manifest }) => Option.isSome(libraryRoot(manifest))),
        ({ dir, manifest }) => libraryBarrels(tree, dir, manifest, dirs.filter(String.startsWith(`${dir}${path.sep}`))),
        { concurrency: 'unbounded' },
    );
    return [...libraries.flatMap(Struct.get('written')), ...libraries.flatMap(Struct.get('stale')).map((file) => ({ file }))];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const barrels = syncGenerator("Library barrels differ from each library's exports and modules", rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { barrels };
