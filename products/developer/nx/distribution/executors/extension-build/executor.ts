import { hash } from 'node:crypto';
import { patch } from '@decimalturn/toml-patch';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Cause, Effect, Equivalence, FileSystem, Function, Option, Path, Record, Result, Schema, Struct } from 'effect';
import { Toml } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { run, Words } from '#/executor';
import { BUILDER, EXTENSION } from '#/host-packages';
import { spawned } from '#/inference';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TARGETS: Record.ReadonlyRecord<string, string> = {
    'linux-x64': 'x86_64-manylinux_2_28',
    'macos-arm64': 'aarch64-apple-darwin',
    'macos-x64': 'x86_64-apple-darwin',
    'windows-x64': 'x86_64-pc-windows-msvc',
    'windows-arm64': 'aarch64-pc-windows-msvc',
};

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({
    outputPath: Schema.String.annotate({ description: 'Workspace-relative folder the archives are written to, each earlier archive of the extension in it removed first' }),
    operation: Schema.Literals(['build', 'check']).annotate({ description: "What runs: the archive build, or blender_ext.py validate over the project's source folder", default: 'build' }),
    __unparsed__: Words,
}).annotate({
    title: 'Extension build',
    description:
        "Builds the Blender extension archives the project's blender_manifest.toml declares through Blender's blender_ext.py, with no Blender installed, one archive per platform the manifest lists and one for every platform where it lists none, or for a check validates the manifest and files of the source folder with blender_ext.py validate; a project holding pyproject.toml builds one staged copy per platform the manifest lists, or one for every platform where it lists none, that bundles its local dependencies as subpackages and as wheels the registry dependencies uv installs from the exported lock for its platforms at the one Python minor its requires-python pins as ==<major>.<minor>.*, leaving out each package its blender dependency group pins as one Blender bundles",
});
const Manifest = Schema.Struct({ id: Schema.String, platforms: Schema.optionalKey(Schema.Array(Schema.String)) });
const Minor = Schema.TemplateLiteralParser(['==', Schema.Natural, '.', Schema.Natural, '.*']);
const Locked = Schema.Struct({
    'requires-python': Schema.String,
    packages: Schema.Array(Schema.Struct({ name: Schema.String, directory: Schema.optionalKey(Schema.Struct({ path: Schema.String })), wheels: Schema.optionalKey(Schema.Array(Schema.Struct({ url: Schema.String, hashes: Schema.Struct({ sha256: Schema.String }) }))) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = (text: string, subject: string) => Effect.mapError(Effect.fromResult(Toml.parse(text)), (cause) => new Cause.UnknownError(cause, `${subject} does not parse as TOML`));

const uv = (args: readonly string[], cwd: string) => spawned(['uv', '--color', 'never', ...args], { cwd, stdout: 'pipe' });

const locked = (args: readonly string[], cwd: string) =>
    uv(args, cwd).pipe(
        Effect.flatMap((pylock) => toml(pylock, `uv ${args.join(' ')}`)),
        Effect.flatMap(Schema.decodeUnknownEffect(Locked)),
    );

const keyed = (stem: string, tags: readonly string[]) =>
    [
        stem.split('-', 2).join(' ').toLowerCase().replaceAll(/[_.]+/gu, '-'),
        ...Array.dedupe(
            tags.flatMap((tag) => {
                const [platform, abi, python] = tag.split('-').toReversed();
                return Array.cartesianWith(
                    Array.cartesianWith(python?.split('.') ?? [], abi?.split('.') ?? [], (interpreter, binary) => `${interpreter}-${binary}`),
                    platform?.split('.') ?? [],
                    (prefix, system) => `${prefix}-${system}`,
                );
            }),
        ).toSorted(),
    ].join(' ');

const fetched = Effect.fnUntraced(function* (url: string, sha256: string, file: string) {
    const fs = yield* FileSystem.FileSystem;
    const response = yield* Effect.flatMap(HttpClient.get(url), HttpClientResponse.filterStatusOk);
    const bytes = new Uint8Array(yield* response.arrayBuffer);
    yield* hash('sha256', bytes, 'hex') === sha256 ? fs.writeFile(file, bytes) : Effect.fail(new Cause.UnknownError(sha256, `${url} does not hash to ${sha256}, the sha256 its lock states`));
}, Effect.provide(FetchHttpClient.layer));

const staged = Effect.fnUntraced(function* (context: ExecutorContext, root: string, text: string, document: Record.ReadonlyRecord<string, unknown>, declared: typeof Manifest.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const [source, work, vendor] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped()]);
    const member = path.join(context.root, root);
    const registry = path.join(work, 'pylock.toml');
    const exported = ['export', '--locked', '--no-default-groups', '--no-emit-project', '--format', 'pylock.toml'];
    const [bundled, whole] = yield* Effect.all([locked(['export', '--locked', '--only-group', 'blender', '--format', 'pylock.toml'], member), locked(exported, member)], { concurrency: 'unbounded' });
    const [, major, , minor] = yield* Effect.mapError(Schema.decodeUnknownEffect(Minor)(whole['requires-python']), (cause) => new Cause.UnknownError(cause, `requires-python ${whole['requires-python']} pins no single Python minor as ==<major>.<minor>.*, the one interpreter the bundled wheels load in`));
    const registered = yield* locked([...exported, '--no-emit-local', '--output-file', registry, ...bundled.packages.flatMap(({ name }) => ['--no-emit-package', name])], member);
    const wheels = new Map(registered.packages.flatMap(({ wheels: built = [] }) => built).map(({ url, hashes }) => Function.pipe(decodeURIComponent(path.basename(new URL(url).pathname, '.whl')), (stem) => [keyed(stem, [stem]), { url, sha256: hashes.sha256, file: `${stem}.whl` }] as const)));
    const files = yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**', '*')]));
    yield* Effect.forEach(
        files.filter((file) => path.basename(file) !== EXTENSION),
        (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(source, path.relative(root, file))), { recursive: true }), fs.copyFile(path.join(context.root, file), path.join(source, path.relative(root, file)))),
        { concurrency: 'unbounded' },
    );
    yield* Effect.forEach(Array.getSomes(whole.packages.map(({ directory }) => Option.fromUndefinedOr(directory?.path))), (local) => uv(['build', local, '--wheel', '--out-dir', vendor], member), { concurrency: 'unbounded' });
    const vendored = (yield* fs.readDirectory(vendor)).filter((file) => path.extname(file) === '.whl').map((wheel) => path.join(vendor, wheel));
    yield* uv(['run', '--script', yield* path.fromFileUrl(new URL(import.meta.resolve('#vendored'))), source, ...vendored], context.root);
    const selected = yield* Effect.forEach(
        declared.platforms ?? Record.keys(TARGETS),
        Effect.fnUntraced(function* (platform) {
            const target = yield* Effect.mapError(Effect.fromNullishOr(TARGETS[platform]), (cause) => new Cause.UnknownError(cause, `${EXTENSION} lists platform ${platform}, where uv selects wheels for ${Record.keys(TARGETS).join(', ')}`));
            const folder = path.join(work, platform);
            yield* uv(['pip', 'install', '--requirements', registry, '--only-binary', ':all:', '--python', `${major}.${minor}`, '--python-platform', target, '--target', folder], member);
            const installed = (yield* fs.readDirectory(folder)).filter((name) => path.extname(name) === '.dist-info');
            const records = yield* Effect.forEach(installed, (info) => fs.readFileString(path.join(folder, info, 'WHEEL')), { concurrency: 'unbounded' });
            const [present, missing] = Array.separate(Array.zipWith(installed, records, (info, record) => Result.fromNullishOr(wheels.get(keyed(path.basename(info, '.dist-info'), record.match(/(?<=^Tag:\s*)\S+$/gmu) ?? [])), Function.constant(info))));
            return yield* Array.match(missing, { onEmpty: () => Effect.succeed(present), onNonEmpty: (names) => Effect.fail(new Cause.UnknownError(names, `${names.join(', ')} uv installs for ${platform} carry the tags of no wheel the lock names`)) });
        }),
        { concurrency: 'unbounded' },
    );
    const pool = path.join(work, 'wheels');
    yield* fs.makeDirectory(pool);
    yield* Effect.forEach(Array.dedupeWith(selected.flat(), Equivalence.mapInput(Equivalence.String, Struct.get('file'))), ({ url, sha256, file }) => fetched(url, sha256, path.join(pool, file)), { concurrency: 'unbounded', discard: true });
    const builds = declared.platforms === undefined ? [{ stated: document, chosen: selected.flat() }] : Array.zipWith(declared.platforms, selected, (platform, chosen) => ({ stated: { ...document, platforms: [platform] }, chosen }));
    return yield* Effect.forEach(
        builds,
        Effect.fnUntraced(function* ({ stated, chosen }) {
            const stage = yield* fs.makeTempDirectoryScoped();
            const listed = Array.dedupe(chosen.map(Struct.get('file'))).toSorted();
            yield* fs.copy(source, stage);
            yield* fs.makeDirectory(path.join(stage, 'wheels'));
            yield* Effect.forEach(listed, (file) => fs.copyFile(path.join(pool, file), path.join(stage, 'wheels', file)), { concurrency: 'unbounded', discard: true });
            yield* fs.writeFileString(path.join(stage, EXTENSION), patch(text, { ...stated, wheels: listed.map((file) => `./wheels/${file}`) }));
            return stage;
        }),
        { concurrency: 'unbounded' },
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ outputPath: folder, operation, __unparsed__: words = [] }: typeof Build.Type, context: ExecutorContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const { root } = (yield* Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => Effect.fromNullishOr(context.projectGraph.nodes[project]))).data;
    yield* {
        check: spawned(['uv', '--color', 'never', ...BUILDER, 'validate', root, ...words], { cwd: context.root, stdout: 'inherit' }),
        build: Effect.gen(function* () {
            const text = yield* fs.readFileString(path.join(context.root, root, EXTENSION));
            const document = yield* toml(text, EXTENSION);
            const declared = yield* Schema.decodeUnknownEffect(Manifest)(document);
            const bundle = yield* fs.exists(path.join(context.root, root, 'pyproject.toml'));
            yield* fs.makeDirectory(path.join(context.root, folder), { recursive: true });
            const stale = (yield* fs.readDirectory(path.join(context.root, folder))).filter((name) => name.startsWith(`${declared.id}-`) && path.extname(name) === '.zip');
            yield* Effect.forEach(stale, (name) => fs.remove(path.join(context.root, folder, name)), { discard: true });
            const sources = bundle ? yield* staged(context, root, text, document, declared) : [root];
            yield* Effect.forEach(sources, (source) => uv([...BUILDER, 'build', ...(declared.platforms === undefined ? [] : ['--split-platforms']), '--source-dir', source, '--output-dir', folder, ...words], context.root), { concurrency: 'unbounded', discard: true });
        }),
    }[operation];
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, execute };
