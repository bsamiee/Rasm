import { patch } from '@decimalturn/toml-patch';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Schema, Stream, String } from 'effect';
import { Toml, Zip } from 'effect/encoding';
import { DistributionError, EXTENSION, spawned } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Build = Schema.Struct({ outputPath: Schema.String });
const Manifest = Schema.Struct({ id: Schema.String, platforms: Schema.optionalKey(Schema.Array(Schema.String)) });
const Interpreter = Schema.fromJsonString(Schema.Struct({ python: Schema.String, platform: Schema.String, tags: Schema.Array(Schema.String), distributions: Schema.Array(Schema.String), modules: Schema.Array(Schema.String) }));
const Locked = Schema.Struct({ packages: Schema.Array(Schema.Struct({ directory: Schema.optionalKey(Schema.Struct({ path: Schema.String })) })) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const toml = (text: string, subject: string) => Effect.mapError(Effect.fromResult(Toml.parse(text)), DistributionError.of(subject, 'does not parse as TOML'));

const shipped = Effect.fnUntraced(function* (wheel: string, bundled: ReadonlySet<string>) {
    const fs = yield* FileSystem.FileSystem;
    const paths = yield* fs.stream(wheel).pipe(
        Stream.pipeThroughChannel(Zip.decode()),
        Stream.map(({ header }) => Array.headNonEmpty(String.split(header.path, '/'))),
        Stream.runCollect,
    );
    return !paths.filter((top) => !(top.endsWith('.dist-info') || top.endsWith('.data'))).every((top) => bundled.has(Array.headNonEmpty(String.split(top, '.'))));
});

const staged = Effect.fnUntraced(function* (context: ExecutorContext, root: string, text: string) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const stage = yield* fs.makeTempDirectoryScoped();
    const wheels = path.join(stage, 'wheels');
    const member = path.join(context.root, root);
    const printed = yield* spawned('blender', ['--background', '--factory-startup', '--python-exit-code', '1', '--python', yield* path.fromFileUrl(new URL(import.meta.resolve('#blender')))], context.root);
    const interpreter = yield* Effect.fromOption(
        Array.findFirst(printed.split('\n'), (line) => Schema.decodeOption(Interpreter)(line)),
        () => DistributionError.make({ subject: 'blender', detail: 'printed no interpreter report' }),
    );
    const declared = yield* Effect.flatMap(toml(text, EXTENSION), Schema.decodeUnknownEffect(Manifest));
    yield* (declared.platforms ?? [interpreter.platform]).includes(interpreter.platform) ? Effect.void : DistributionError.make({ subject: EXTENSION, detail: `declares no ${interpreter.platform} platform, the one this Blender builds for` });
    const files = yield* Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**', '*')]));
    yield* Effect.forEach(
        files.filter((file) => path.basename(file) !== EXTENSION),
        (file) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(stage, path.relative(root, file))), { recursive: true }), fs.copyFile(path.join(context.root, file), path.join(stage, path.relative(root, file)))),
        { concurrency: 'unbounded' },
    );
    yield* fs.makeDirectory(wheels, { recursive: true });
    const requirements = path.join(stage, 'requirements.txt');
    const exported = ['export', '--locked', '--no-default-groups', '--no-emit-project'];
    yield* spawned('uv', [...exported, '--format', 'requirements.txt', '--no-emit-local', '--no-header', '--output-file', requirements, ...interpreter.distributions.flatMap((name) => ['--no-emit-package', name])], member);
    yield* spawned('uv', ['tool', 'run', 'pip', 'download', '--requirement', requirements, '--dest', wheels, '--only-binary=:all:', '--no-deps', '--python-version', interpreter.python, '--implementation', 'cp', ...interpreter.tags.flatMap((tag) => ['--platform', tag])], context.root);
    yield* fs.remove(requirements);
    const locked = yield* spawned('uv', [...exported, '--format', 'pylock.toml'], member).pipe(
        Effect.flatMap((pylock) => toml(pylock, 'uv export --format pylock.toml')),
        Effect.flatMap(Schema.decodeUnknownEffect(Locked)),
    );
    const vendor = yield* fs.makeTempDirectoryScoped();
    yield* Effect.forEach(Array.getSomes(locked.packages.map(({ directory }) => Option.fromUndefinedOr(directory?.path))), (local) => spawned('uv', ['build', local, '--wheel', '--out-dir', vendor], member), { concurrency: 'unbounded' });
    const vendored = (yield* fs.readDirectory(vendor)).filter((file) => path.extname(file) === '.whl').map((wheel) => path.join(vendor, wheel));
    yield* spawned('uv', ['run', '--locked', '--script', yield* path.fromFileUrl(new URL(import.meta.resolve('#vendored'))), stage, ...vendored], context.root);
    const bundled = new Set(interpreter.modules);
    const listed = (yield* fs.readDirectory(wheels)).toSorted();
    const kept = yield* Effect.filter(
        listed.filter((file) => path.extname(file) === '.whl'),
        (wheel) => shipped(path.join(wheels, wheel), bundled),
    );
    yield* Effect.forEach(
        listed.filter((file) => !kept.includes(file)),
        (file) => fs.remove(path.join(wheels, file)),
    );
    yield* fs.writeFileString(path.join(stage, EXTENSION), patch(text, { ...(yield* toml(text, EXTENSION)), platforms: [interpreter.platform], wheels: kept.map((wheel) => `./wheels/${wheel}`) }));
    return stage;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [path, fs, { outputPath: folder }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeUnknownEffect(Build)(options)]);
            const project = yield* Effect.fromNullishOr(context.projectName);
            const { root } = (yield* Effect.fromNullishOr(context.projectGraph.nodes[project])).data;
            const text = yield* fs.readFileString(path.join(context.root, root, EXTENSION));
            const declared = yield* Effect.flatMap(toml(text, EXTENSION), Schema.decodeUnknownEffect(Manifest));
            const bundle = yield* fs.exists(path.join(context.root, root, 'pyproject.toml'));
            yield* fs.makeDirectory(path.join(context.root, folder), { recursive: true });
            yield* Effect.forEach(
                (yield* fs.readDirectory(path.join(context.root, folder))).filter((file) => file.startsWith(`${declared.id}-`) && path.extname(file) === '.zip'),
                (file) => fs.remove(path.join(context.root, folder, file)),
            );
            const source = bundle ? yield* staged(context, root, text) : root;
            yield* spawned('blender', ['--command', 'extension', 'build', ...(bundle || declared.platforms !== undefined ? ['--split-platforms'] : []), '--source-dir', source, '--output-dir', folder], context.root);
            return { success: true };
        }).pipe(Effect.scoped, Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
