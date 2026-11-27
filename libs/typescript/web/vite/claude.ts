import { join, posix, sep } from 'node:path';
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const claude = (directory: string): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const fs = yield* FileSystem.FileSystem;
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
                        generateBundle() {
                            Record.toEntries(manifests).forEach(([fileName, source]) => {
                                this.emitFile({ type: 'asset', fileName, source });
                            });
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
