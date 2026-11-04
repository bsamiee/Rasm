import { Lang, parse, type SgNode } from '@ast-grep/napi';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Function, Option, Path, Record, Schema, Stream, String, Struct } from 'effect';
import { accumulate } from '#/eng/typescript/accumulate';
import { ClaudeHooks, libraryRoot, Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { glob } from '#/tools/nx/internal/open-workspace';
import { runPlugin } from '#/tools/nx/internal/plugin-runtime';
import { WorkspaceError } from '#/tools/nx/internal/workspace-error';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BARREL = 'index.ts';
const TYPE_DECLARATIONS = ['interface_declaration', 'type_alias_declaration'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]
const typeOnly = (node: SgNode) => node.children().some((child) => child.kind() === 'type');
const namedExport = (type: boolean) => (name: SgNode) => ({ name: name.text(), type });
const specifiedNames = (statement: SgNode) => Array.getSomes(statement.findAll({ rule: { kind: 'export_specifier' } }).map((specifier) => Option.map(Option.fromNullOr(specifier.field('alias') ?? specifier.field('name')), namedExport(typeOnly(statement) || typeOnly(specifier)))));
const declaredNames = (declaration: SgNode) =>
    Array.getSomes((['lexical_declaration', 'variable_declaration'].some((kind) => declaration.is(kind)) ? declaration.children().filter((child) => child.kind() === 'variable_declarator') : [declaration]).map((node) => Option.fromNullOr(node.field('name')))).map(
        namedExport(TYPE_DECLARATIONS.some((kind) => declaration.is(kind))),
    );
const exportedNames = (file: string, text: string) =>
    parse(file.endsWith('.tsx') ? Lang.Tsx : Lang.TypeScript, text)
        .root()
        .children()
        .filter((node) => node.kind() === 'export_statement' && node.field('source') === null && !node.children().some((child) => child.kind() === 'default'))
        .flatMap((statement) => [...specifiedNames(statement), ...Option.match(Option.fromNullOr(statement.field('declaration')), { onNone: Array.empty, onSome: declaredNames })])
        .filter(({ name }) => name !== 'default');

// --- [LAYOUT]
const selfImport = (imports: Readonly<Record<string, string>>, module: string) => Array.findFirst(Record.toEntries(imports), ([key, target]) => Option.map(Option.fromUndefinedOr(new RegExp(`^${target.split('*').map(RegExp.escape).join('(.+)')}$`, 'u').exec(`./${module}`)?.[1]), (stem) => key.replace('*', stem)));
const exportLine = (imports: Readonly<Record<string, string>>, relative: string, file: string, text: string) =>
    Array.match(exportedNames(file, text), {
        onEmpty: Array.empty,
        onNonEmpty: (names) => [`export { ${names.map(({ name, type }) => (type ? `type ${name}` : name)).join(', ')} } from '${Option.getOrElse(selfImport(imports, file), Function.constant(relative))}';`],
    });
const ownerFolder = (folders: readonly string[], separator: string) => (file: string) => Array.findFirst(folders, (folder) => folder === '.' || file.startsWith(`${folder}${separator}`));
const readText = (tree: Tree, file: string) => Effect.fromNullishOr(tree.read(file, 'utf-8'));
const readManifest = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const path = yield* Path.Path;
    return { dir: path.dirname(file), manifest: yield* Effect.flatMap(readText(tree, file), Schema.decodeEffect(Package.schema)) };
});
const barrel = Effect.fnUntraced(function* (tree: Tree, dir: string, imports: Readonly<Record<string, string>>, folder: string, files: readonly string[]) {
    const path = yield* Path.Path;
    const file = path.join(dir, folder, BARREL);
    const sources = yield* Effect.forEach(files, (module) => readText(tree, path.join(dir, module)));
    const lines = Array.zipWith(files, sources, (module, source) => exportLine(imports, `./${path.relative(folder, module)}`, module, source));
    const text = yield* runCommand('biome', ['check', '--write', `--stdin-file-path=${file}`], { cwd: tree.root, stdin: Stream.encodeText(Stream.make(['// --- [EXPORTS] -------------------------------------------------------------------------', '', ...lines.flat(), ''].join('\n'))) });
    return { file, text };
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
    const declared = yield* Effect.forEach(hooks, (file) => Effect.map(Effect.flatMap(readText(tree, file), Schema.decodeEffect(ClaudeHooks.schema)), Struct.assign({ file })));
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const barrels = Effect.fnUntraced(function* (tree: Tree) {
    const path = yield* Path.Path;
    const manifests = yield* Effect.forEach(yield* glob(tree.root, [`**/${Package.file}`]), (file) => readManifest(tree, file));
    const dirs = manifests.map(Struct.get('dir'));
    const libraries = yield* Effect.forEach(
        manifests.filter(({ manifest }) => Option.isSome(libraryRoot(manifest))),
        ({ dir, manifest }) => libraryBarrels(tree, dir, manifest, dirs.filter(String.startsWith(`${dir}${path.sep}`))),
        { concurrency: 'unbounded' },
    );
    const drifted = libraries.flatMap(Struct.get('written')).filter(({ file, text }) => tree.read(file, 'utf-8') !== text);
    const stale = libraries.flatMap(Struct.get('stale'));
    yield* Effect.forEach(drifted, ({ file, text }) => Effect.sync(tree.write.bind(tree, file, text)), { discard: true });
    yield* Effect.forEach(stale, (file) => Effect.sync(tree.delete.bind(tree, file)), { discard: true });
    return Array.match([...drifted.map(Struct.get('file')), ...stale], { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: "Library barrels differ from each library's exports and modules", outOfSyncDetails: changed }) });
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { barrels };
