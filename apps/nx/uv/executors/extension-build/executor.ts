import { parse, patch } from '@decimalturn/toml-patch';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Schema, Stream } from 'effect';
import { Zip } from 'effect/encoding';
import { EXTENSION, spawned, UvError } from '#/plugins/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({ outputPath: Schema.String, bundle: Schema.optionalKey(Schema.Boolean) });
const Manifest = Schema.Struct({ platforms: Schema.optionalKey(Schema.Array(Schema.String)) });
const Host = Schema.fromJsonString(Schema.Struct({ python: Schema.String, platform: Schema.String, tags: Schema.Array(Schema.String), distributions: Schema.Array(Schema.String), modules: Schema.Array(Schema.String) }));
const Locked = Schema.Struct({ packages: Schema.Array(Schema.Struct({ directory: Schema.optionalKey(Schema.Struct({ path: Schema.String })) })) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = (text: string, subject: string) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => UvError.make({ subject, detail: String(cause) }) });

const shipped = Effect.fnUntraced(function* (wheel: string, bundled: ReadonlySet<string>) {
    const fs = yield* FileSystem.FileSystem;
    const paths = yield* fs.stream(wheel).pipe(
        Stream.pipeThroughChannel(Zip.decode()),
        Stream.map(({ header }) => header.path.split('/')[0] ?? ''),
        Stream.runCollect,
    );
    return !paths.filter((top) => !(top.endsWith('.dist-info') || top.endsWith('.data'))).every((top) => bundled.has(top.split('.')[0] ?? top));
});

const staged = Effect.fnUntraced(function* (context: ExecutorContext, root: string, folder: string, text: string) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const stage = path.join(context.root, folder, 'source');
    const wheels = path.join(stage, 'wheels');
    const project = yield* Effect.fromNullishOr(context.projectName);
    const [host] = Array.getSomes((yield* spawned('blender', ['--background', '--factory-startup', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(new URL(import.meta.resolve('#blender')))], context.root)).split('\n').map((line) => Schema.decodeOption(Host)(line)));
    const held = yield* Effect.fromNullishOr(host).pipe(Effect.mapError(() => UvError.make({ subject: 'blender', detail: 'printed no interpreter report' })));
    const declared = yield* Effect.flatMap(toml(text, EXTENSION), Schema.decodeUnknownEffect(Manifest));
    yield* (declared.platforms ?? [held.platform]).includes(held.platform) ? Effect.void : UvError.make({ subject: EXTENSION, detail: `declares no ${held.platform} platform, the one this Blender builds for` });
    yield* fs.remove(stage, { recursive: true, force: true });
    const files = yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**', '*')]));
    yield* Effect.forEach(
        files.filter((file) => path.basename(file) !== EXTENSION),
        (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(stage, path.relative(root, file))), { recursive: true }), fs.copyFile(path.join(context.root, file), path.join(stage, path.relative(root, file)))),
        { concurrency: 'unbounded' },
    );
    yield* fs.makeDirectory(wheels, { recursive: true });
    const requirements = path.join(stage, 'requirements.txt');
    const exported = ['export', '--locked', '--package', project, '--no-default-groups', '--no-emit-project'];
    yield* spawned('uv', [...exported, '--format', 'requirements.txt', '--no-emit-local', '--no-header', '--output-file', requirements, ...held.distributions.flatMap((name) => ['--no-emit-package', name])], path.join(context.root, root));
    yield* spawned('uv', ['tool', 'run', 'pip', 'download', '--requirement', requirements, '--dest', wheels, '--only-binary=:all:', '--no-deps', '--python-version', held.python, '--implementation', 'cp', ...held.tags.flatMap((tag) => ['--platform', tag])], context.root);
    yield* fs.remove(requirements);
    const locked = yield* spawned('uv', [...exported, '--format', 'pylock.toml'], path.join(context.root, root)).pipe(
        Effect.flatMap((pylock) => toml(pylock, 'uv export --format pylock.toml')),
        Effect.flatMap(Schema.decodeUnknownEffect(Locked)),
    );
    yield* Effect.forEach(Array.getSomes(locked.packages.map(({ directory }) => Option.fromUndefinedOr(directory?.path))), (local) => spawned('uv', ['build', local, '--wheel', '--out-dir', wheels], path.join(context.root, root)), { concurrency: 'unbounded' });
    const bundled = new Set(held.modules);
    const listed = (yield* fs.readDirectory(wheels)).toSorted();
    const kept = yield* Effect.filter(
        listed.filter((file) => path.extname(file) === '.whl'),
        (wheel) => shipped(path.join(wheels, wheel), bundled),
    );
    yield* Effect.forEach(
        listed.filter((file) => !kept.includes(file)),
        (file) => fs.remove(path.join(wheels, file)),
    );
    const document = yield* Effect.flatMap(toml(text, EXTENSION), Schema.decodeUnknownEffect(Schema.Record(Schema.String, Schema.Unknown)));
    yield* fs.writeFileString(path.join(stage, EXTENSION), patch(text, { ...document, platforms: [held.platform], wheels: kept.map((wheel) => `./wheels/${wheel}`) }));
    return path.relative(context.root, stage);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [path, fs, { outputPath: folder, bundle = false }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeUnknownEffect(Build)(options)]);
            const project = yield* Effect.fromNullishOr(context.projectName);
            const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
            const text = yield* fs.readFileString(path.join(context.root, node.data.root, EXTENSION));
            const declared = yield* Effect.flatMap(toml(text, EXTENSION), Schema.decodeUnknownEffect(Manifest));
            yield* fs.makeDirectory(path.join(context.root, folder), { recursive: true });
            const source = bundle ? yield* staged(context, node.data.root, folder, text) : node.data.root;
            yield* spawned('blender', ['--command', 'extension', 'build', ...(declared.platforms === undefined ? [] : ['--split-platforms']), '--source-dir', source, '--output-dir', folder], context.root);
            return { success: true };
        }).pipe(
            Effect.catch((error) => Effect.as(Effect.logError(error.message), { success: false })),
            Effect.provide(layer),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
