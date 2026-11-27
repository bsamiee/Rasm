import { dirname, join, posix, sep } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Effect, FileSystem, Option, Record, Schema } from 'effect';
import type { UserConfig, UserConfigFnPromise } from 'vite';
import { checks } from '#/checks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOOKS = 'hooks';
const MANIFESTS = { hooks: `${HOOKS}/hooks.json`, plugin: '.claude-plugin/plugin.json' } as const;
const CHUNK = 900_000;

// --- [MODELS] --------------------------------------------------------------------------

const Hooks = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)])).annotate({
    description: 'The hooks/hooks.json of a Claude Code plugin, each module a hooks module source file relative to its folder',
});
const Plugin = Schema.fromJsonString(Schema.Struct({ types: Schema.optionalKey(Schema.String) })).annotate({
    description: 'The .claude-plugin/plugin.json of a Claude Code plugin, its types contract a declaration file relative to the plugin folder',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const settle = Effect.fnUntraced(function* (out: string, bundle: Readonly<Record<string, unknown>>, manifests: Readonly<Record<string, string>>) {
    const fs = yield* FileSystem.FileSystem;
    const changed = yield* Effect.filter(Record.toEntries(manifests), ([file, source]) => Effect.map(Effect.option(fs.readFileString(join(out, file))), (held) => !Option.contains(held, source)));
    yield* Effect.forEach(changed, ([file, source]) => Effect.andThen(fs.makeDirectory(dirname(join(out, file)), { recursive: true }), fs.writeFileString(join(out, file), source)), { discard: true });
    const stale = (yield* fs.readDirectory(join(out, HOOKS))).map((name) => posix.join(HOOKS, name)).filter((file) => !Record.has(bundle, file) && !Record.has(manifests, file));
    yield* Effect.forEach(stale, (file) => fs.remove(join(out, file)), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const claude = (directory: string): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const [fs, services] = yield* Effect.all([FileSystem.FileSystem, Effect.context<FileSystem.FileSystem>()]);
            const [hooks, plugin] = yield* Effect.all([Effect.flatMap(fs.readFileString(join(directory, MANIFESTS.hooks)), Schema.decodeEffect(Hooks)), fs.readFileString(join(directory, MANIFESTS.plugin))]);
            const { types } = yield* Schema.decodeEffect(Plugin)(plugin);
            const contract = yield* Effect.forEach(Option.toArray(Option.fromUndefinedOr(types)), (path) => Effect.map(fs.readFileString(join(directory, path)), (source) => [posix.normalize(path), source] as const));
            const names = Array.map(hooks.modules, (module) => posix.join(posix.dirname(module), posix.parse(module).name));
            const manifests = { [MANIFESTS.hooks]: yield* Schema.encodeEffect(Hooks)({ ...hooks, modules: Array.map(names, (name) => `./${name}.js`) }), [MANIFESTS.plugin]: plugin, ...Record.fromEntries(contract) };
            return {
                build: {
                    lib: { entry: Record.fromIterableWith(Array.zip(names, hooks.modules), ([name, module]) => [name, join(directory, HOOKS, module)]), formats: ['es'] },
                    target: 'esnext',
                    rolldownOptions: {
                        external: ['claude-code'],
                        output: {
                            entryFileNames: `${HOOKS}/[name].js`,
                            chunkFileNames: `${HOOKS}/[name]-[hash].js`,
                            minify: { compress: true, mangle: false, codegen: { removeWhitespace: true } },
                            codeSplitting: { maxSize: CHUNK, groups: [{ name: 'modules', test: new RegExp(`^(?!${RegExp.escape(join(directory, sep))})`, 'u') }] },
                            strictExecutionOrder: true,
                        },
                    },
                },
                plugins: [
                    checks(),
                    {
                        name: '@rasm/vite/claude',
                        config: () => ({ build: { emptyOutDir: false } }),
                        writeBundle({ dir }, bundle) {
                            return Effect.runPromiseWith(services)(Effect.flatMap(Effect.fromNullishOr(dir), (out) => settle(out, bundle, manifests)));
                        },
                    },
                ],
            } satisfies UserConfig;
        },
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { claude };
