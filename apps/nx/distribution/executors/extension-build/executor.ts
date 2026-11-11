import { hash } from 'node:crypto';
import { patch } from '@decimalturn/toml-patch';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { executor } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Equivalence, FileSystem, Option, Path, Record, Schema, Stream, String, Struct } from 'effect';
import { Toml, Zip } from 'effect/encoding';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { EXTENSION } from '#/host-packages';

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
        "Builds the Blender extension archives the project's blender_manifest.toml declares; a project holding pyproject.toml builds from a staged copy that bundles its local dependencies as subpackages and its locked registry dependencies as the wheels uv selects for each platform the manifest lists, one archive per platform",
});
const Manifest = Schema.Struct({ id: Schema.String, platforms: Schema.optionalKey(Schema.Array(Schema.String)) });
const Interpreter = Schema.fromJsonString(Schema.Struct({ python: Schema.String, platform: Schema.String, distributions: Schema.Record(Schema.String, Schema.String), undeclared: Schema.Record(Schema.String, Schema.String), modules: Schema.Array(Schema.String) }));
const Locked = Schema.Struct({
    packages: Schema.Array(Schema.Struct({ name: Schema.String, directory: Schema.optionalKey(Schema.Struct({ path: Schema.String })), wheels: Schema.optionalKey(Schema.Array(Schema.Struct({ url: Schema.String, hashes: Schema.Struct({ sha256: Schema.String }) }))) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = (text: string, subject: string) => Effect.mapError(Effect.fromResult(Toml.parse(text)), ToolError.of(subject, 'does not parse as TOML'));

const locked = (args: readonly string[], cwd: string) =>
    printed('uv', args, { cwd }).pipe(
        Effect.flatMap((pylock) => toml(pylock, `uv ${args.join(' ')}`)),
        Effect.flatMap(Schema.decodeUnknownEffect(Locked)),
    );

const fetched = Effect.fnUntraced(function* (url: string, sha256: string, folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const response = yield* Effect.flatMap(HttpClient.get(url), HttpClientResponse.filterStatusOk);
    const bytes = new Uint8Array(yield* response.arrayBuffer);
    yield* hash('sha256', bytes, 'hex') === sha256 ? fs.writeFile(path.join(folder, decodeURIComponent(path.basename(new URL(url).pathname))), bytes) : ToolError.make({ subject: url, detail: `does not hash to ${sha256}, the sha256 its lock states` });
}, Effect.provide(FetchHttpClient.layer));

const probed = Effect.fnUntraced(function* (cwd: string) {
    const path = yield* Path.Path;
    const report = yield* printed('blender', ['--python-use-system-env', '--background', '--factory-startup', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(new URL(import.meta.resolve('#blender')))], { cwd });
    return yield* Effect.fromOption(
        Array.findFirst(report.split('\n'), (line) => Schema.decodeOption(Interpreter)(line)),
        () => ToolError.make({ subject: 'blender', detail: 'printed no interpreter report' }),
    );
});

const shipped = Effect.fnUntraced(function* (wheel: string, bundled: ReadonlySet<string>) {
    const fs = yield* FileSystem.FileSystem;
    const paths = yield* fs.stream(wheel).pipe(
        Stream.pipeThroughChannel(Zip.decode()),
        Stream.map(({ header }) => Array.headNonEmpty(String.split(header.path, '/'))),
        Stream.runCollect,
    );
    return !paths.filter((top) => !(top.endsWith('.dist-info') || top.endsWith('.data'))).every((top) => bundled.has(Array.headNonEmpty(String.split(top, '.'))));
});

const staged = Effect.fnUntraced(function* (context: ExecutorContext, root: string, text: string, document: Record.ReadonlyRecord<string, unknown>, declared: typeof Manifest.Type) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const stage = yield* fs.makeTempDirectoryScoped();
    const wheels = path.join(stage, 'wheels');
    const member = path.join(context.root, root);
    const interpreter = yield* probed(context.root);
    const platforms = declared.platforms ?? [interpreter.platform];
    const files = yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**', '*')]));
    yield* Effect.forEach(
        files.filter((file) => path.basename(file) !== EXTENSION),
        (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(stage, path.relative(root, file))), { recursive: true }), fs.copyFile(path.join(context.root, file), path.join(stage, path.relative(root, file)))),
        { concurrency: 'unbounded' },
    );
    yield* fs.makeDirectory(wheels, { recursive: true });
    const requirements = path.join(stage, 'requirements.txt');
    const exported = ['export', '--locked', '--no-default-groups', '--no-emit-project'];
    yield* printed('uv', [...exported, '--format', 'requirements.txt', '--no-emit-local', '--no-header', '--output-file', requirements, ...Record.keys(interpreter.distributions).flatMap((name) => ['--no-emit-package', name])], { cwd: member });
    const selected = yield* Effect.forEach(
        platforms,
        Effect.fnUntraced(function* (platform) {
            const target = yield* Effect.mapError(Effect.fromNullishOr(TARGETS[platform]), ToolError.of(EXTENSION, `lists platform ${platform}, where uv selects wheels for ${Record.keys(TARGETS).join(', ')}`));
            const resolved = yield* locked(['pip', 'compile', requirements, '--no-deps', '--python-platform', target, '--python-version', interpreter.python, '--format', 'pylock.toml'], member);
            return yield* Effect.forEach(resolved.packages, ({ name, wheels: built = [] }) => Effect.mapError(Effect.fromOption(Array.head(built)), ToolError.of(name, `locks no wheel for ${platform}`)));
        }),
        { concurrency: 'unbounded' },
    );
    yield* Effect.forEach(Array.dedupeWith(selected.flat(), Equivalence.mapInput(Equivalence.String, Struct.get('url'))), ({ url, hashes }) => fetched(url, hashes.sha256, wheels), { concurrency: 'unbounded', discard: true });
    yield* fs.remove(requirements);
    const { packages } = yield* locked([...exported, '--format', 'pylock.toml'], member);
    const vendor = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(Array.getSomes(packages.map(({ directory }) => Option.fromUndefinedOr(directory?.path))), (local) => printed('uv', ['build', local, '--wheel', '--out-dir', vendor], { cwd: member }), { concurrency: 'unbounded' });
    const vendored = (yield* fs.readDirectory(vendor)).filter((file) => path.extname(file) === '.whl').map((wheel) => path.join(vendor, wheel));
    yield* printed('uv', ['run', '--script', yield* path.fromFileUrl(new URL(import.meta.resolve('#vendored'))), stage, ...vendored], { cwd: context.root });
    const bundled = new Set(interpreter.modules);
    const listed = (yield* fs.readDirectory(wheels)).toSorted();
    const kept = yield* Effect.filter(
        listed.filter((file) => path.extname(file) === '.whl'),
        (wheel) => shipped(path.join(wheels, wheel), bundled),
    );
    yield* Effect.forEach(Array.difference(listed, kept), (file) => fs.remove(path.join(wheels, file)));
    yield* fs.writeFileString(path.join(stage, EXTENSION), patch(text, { ...document, platforms, wheels: kept.map((wheel) => `./wheels/${wheel}`) }));
    return stage;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Build,
    Effect.fnUntraced(function* ({ outputPath: folder }, context, project) {
        const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
        const { root } = (yield* Effect.fromNullishOr(context.projectGraph.nodes[project])).data;
        const text = yield* fs.readFileString(path.join(context.root, root, EXTENSION));
        const document = yield* toml(text, EXTENSION);
        const declared = yield* Schema.decodeUnknownEffect(Manifest)(document);
        const bundle = yield* fs.exists(path.join(context.root, root, 'pyproject.toml'));
        yield* fs.makeDirectory(path.join(context.root, folder), { recursive: true });
        yield* Effect.forEach(
            (yield* fs.readDirectory(path.join(context.root, folder))).filter((file) => file.startsWith(`${declared.id}-`) && path.extname(file) === '.zip'),
            (file) => fs.remove(path.join(context.root, folder, file)),
        );
        const source = bundle ? yield* staged(context, root, text, document, declared) : root;
        yield* printed('blender', ['--command', 'extension', 'build', ...(bundle || declared.platforms !== undefined ? ['--split-platforms'] : []), '--source-dir', source, '--output-dir', folder], { cwd: context.root });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
