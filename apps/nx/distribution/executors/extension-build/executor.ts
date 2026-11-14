import { hash } from 'node:crypto';
import { patch } from '@decimalturn/toml-patch';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Array, Effect, Equivalence, FileSystem, Option, Path, Record, Schema, Struct } from 'effect';
import { Toml } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { refused, run } from '#/executor';
import { BUILDER, EXTENSION } from '#/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TARGETS: Record.ReadonlyRecord<string, string> = {
    'linux-x64': 'x86_64-manylinux_2_28',
    'linux-arm64': 'aarch64-manylinux_2_28',
    'macos-arm64': 'aarch64-apple-darwin',
    'macos-x64': 'x86_64-apple-darwin',
    'windows-x64': 'x86_64-pc-windows-msvc',
    'windows-arm64': 'aarch64-pc-windows-msvc',
};

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({ outputPath: Schema.String.annotate({ description: 'Workspace-relative folder the archives are written to' }) }).annotate({
    title: 'Extension build',
    description:
        "Builds the Blender extension archives the project's blender_manifest.toml declares through Blender's blender_ext.py, with no Blender installed, one archive per platform the manifest lists and one for every platform where it lists none; a project holding pyproject.toml builds from a staged copy that bundles its local dependencies as subpackages and its locked registry dependencies as the wheels uv selects per platform at the one Python minor its requires-python pins as ==<major>.<minor>.*, leaving out each package its blender dependency group pins as one Blender bundles",
});
const Manifest = Schema.Struct({ id: Schema.String, platforms: Schema.optionalKey(Schema.Array(Schema.String)) });
const Minor = Schema.TemplateLiteralParser(['==', Schema.Natural, '.', Schema.Natural, '.*']);
const Locked = Schema.Struct({
    'requires-python': Schema.String,
    packages: Schema.Array(Schema.Struct({ name: Schema.String, directory: Schema.optionalKey(Schema.Struct({ path: Schema.String })), wheels: Schema.optionalKey(Schema.Array(Schema.Struct({ url: Schema.String, hashes: Schema.Struct({ sha256: Schema.String }) }))) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = (text: string, subject: string) => Effect.mapError(Effect.fromResult(Toml.parse(text)), refused(subject, 'does not parse as TOML'));

const uv = (args: readonly string[], cwd: string) => Completed.output(ChildProcess.make('uv', ['--color', 'never', ...args], { cwd, stdin: 'ignore' }));

const locked = (args: readonly string[], cwd: string) =>
    uv(args, cwd).pipe(
        Effect.flatMap((pylock) => toml(pylock, `uv ${args.join(' ')}`)),
        Effect.flatMap(Schema.decodeUnknownEffect(Locked)),
    );

const fetched = Effect.fnUntraced(function* (url: string, sha256: string, folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const response = yield* Effect.flatMap(HttpClient.get(url), HttpClientResponse.filterStatusOk);
    const bytes = new Uint8Array(yield* response.arrayBuffer);
    yield* hash('sha256', bytes, 'hex') === sha256 ? fs.writeFile(path.join(folder, decodeURIComponent(path.basename(new URL(url).pathname))), bytes) : Effect.fail(refused(url, `does not hash to ${sha256}, the sha256 its lock states`)());
}, Effect.provide(FetchHttpClient.layer));

const staged = Effect.fnUntraced(function* (context: ExecutorContext, root: string, text: string, document: Record.ReadonlyRecord<string, unknown>, declared: typeof Manifest.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const stage = yield* fs.makeTempDirectoryScoped();
    const wheels = path.join(stage, 'wheels');
    const member = path.join(context.root, root);
    const exported = ['export', '--locked', '--no-default-groups', '--no-emit-project'];
    const [bundled, whole] = yield* Effect.all([locked(['export', '--locked', '--only-group', 'blender', '--format', 'pylock.toml'], member), locked([...exported, '--format', 'pylock.toml'], member)], { concurrency: 'unbounded' });
    const [, major, , minor] = yield* Effect.mapError(Schema.decodeUnknownEffect(Minor)(whole['requires-python']), refused(`requires-python ${whole['requires-python']}`, 'pins no single Python minor as ==<major>.<minor>.*, the one interpreter the bundled wheels load in'));
    const files = yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**', '*')]));
    yield* Effect.forEach(
        files.filter((file) => path.basename(file) !== EXTENSION),
        (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(stage, path.relative(root, file))), { recursive: true }), fs.copyFile(path.join(context.root, file), path.join(stage, path.relative(root, file)))),
        { concurrency: 'unbounded' },
    );
    yield* fs.makeDirectory(wheels, { recursive: true });
    const requirements = path.join(stage, 'requirements.txt');
    yield* uv([...exported, '--format', 'requirements.txt', '--no-emit-local', '--no-header', '--output-file', requirements, ...bundled.packages.flatMap(({ name }) => ['--no-emit-package', name])], member);
    const selected = yield* Effect.forEach(
        declared.platforms ?? Record.keys(TARGETS),
        Effect.fnUntraced(function* (platform) {
            const target = yield* Effect.mapError(Effect.fromNullishOr(TARGETS[platform]), refused(EXTENSION, `lists platform ${platform}, where uv selects wheels for ${Record.keys(TARGETS).join(', ')}`));
            const resolved = yield* locked(['pip', 'compile', requirements, '--no-deps', '--python-platform', target, '--python-version', `${major}.${minor}`, '--format', 'pylock.toml'], member);
            return yield* Effect.forEach(resolved.packages, ({ name, wheels: built = [] }) => Effect.mapError(Effect.fromOption(Array.head(built)), refused(name, `locks no wheel for ${platform}`)));
        }),
        { concurrency: 'unbounded' },
    );
    yield* Effect.forEach(Array.dedupeWith(selected.flat(), Equivalence.mapInput(Equivalence.String, Struct.get('url'))), ({ url, hashes }) => fetched(url, hashes.sha256, wheels), { concurrency: 'unbounded', discard: true });
    yield* fs.remove(requirements);
    const vendor = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(Array.getSomes(whole.packages.map(({ directory }) => Option.fromUndefinedOr(directory?.path))), (local) => uv(['build', local, '--wheel', '--out-dir', vendor], member), { concurrency: 'unbounded' });
    const vendored = (yield* fs.readDirectory(vendor)).filter((file) => path.extname(file) === '.whl').map((wheel) => path.join(vendor, wheel));
    yield* uv(['run', '--script', yield* path.fromFileUrl(new URL(import.meta.resolve('#vendored'))), stage, ...vendored], context.root);
    const listed = (yield* fs.readDirectory(wheels)).toSorted();
    yield* fs.writeFileString(path.join(stage, EXTENSION), patch(text, { ...document, wheels: listed.map((wheel) => `./wheels/${wheel}`) }));
    return stage;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ outputPath: folder }: typeof Build.Type, context: ExecutorContext) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const { root } = (yield* Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => Effect.fromNullishOr(context.projectGraph.nodes[project]))).data;
    const text = yield* fs.readFileString(path.join(context.root, root, EXTENSION));
    const document = yield* toml(text, EXTENSION);
    const declared = yield* Schema.decodeUnknownEffect(Manifest)(document);
    const bundle = yield* fs.exists(path.join(context.root, root, 'pyproject.toml'));
    yield* fs.remove(path.join(context.root, folder), { recursive: true, force: true });
    yield* fs.makeDirectory(path.join(context.root, folder), { recursive: true });
    const source = bundle ? yield* staged(context, root, text, document, declared) : root;
    yield* uv([...BUILDER, 'build', ...(declared.platforms === undefined ? [] : ['--split-platforms']), '--source-dir', source, '--output-dir', folder], context.root);
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, execute };
