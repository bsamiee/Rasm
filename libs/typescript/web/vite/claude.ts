import { builtinModules } from 'node:module';
import { join, posix, sep } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Effect, FileSystem, Record, Schema } from 'effect';
import type { UserConfig, UserConfigFnPromise } from 'vite';
import { checks } from '#/checks';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HOOKS = 'hooks';
const MANIFESTS = { hooks: `${HOOKS}/hooks.json`, plugin: '.claude-plugin/plugin.json', package: 'package.json' } as const;
const CHUNK = 900_000;

// --- [MODELS] --------------------------------------------------------------------------

const Hooks = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)])).annotate({
    description: 'The hooks/hooks.json of a Claude Code plugin, each module a hooks module source file relative to its folder',
});
const Package = Schema.fromJsonString(Schema.Struct({ bin: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)), files: Schema.optionalKey(Schema.Array(Schema.String)) })).annotate({
    description: 'The package.json of a plugin: bin names programs a hook command runs, files names the folders and files each harness reads beside the hooks modules',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const claude = (directory: string): UserConfigFnPromise =>
    Effect.fnUntraced(
        function* () {
            const fs = yield* FileSystem.FileSystem;
            const [hooks, plugin, { bin = {}, files = [] }] = yield* Effect.all([
                Effect.flatMap(fs.readFileString(join(directory, MANIFESTS.hooks)), Schema.decodeEffect(Hooks)),
                fs.readFile(join(directory, MANIFESTS.plugin)),
                Effect.flatMap(fs.readFileString(join(directory, MANIFESTS.package)), Schema.decodeEffect(Package)),
            ]);
            const names = Array.map(hooks.modules, (module) => posix.join(posix.dirname(module), posix.parse(module).name));
            const programs = Record.collect(bin, (_, program) => [posix.join(posix.dirname(program), posix.parse(program).name), join(directory, program)] as const);
            const kinds = yield* Effect.forEach(files, (file) => Effect.map(fs.stat(join(directory, file)), ({ type }) => [file, type] as const));
            const nested = yield* Effect.forEach(
                kinds.filter(([, type]) => type === 'Directory'),
                ([folder]) =>
                    Effect.map(
                        fs.readDirectory(join(directory, folder), { recursive: true }),
                        Array.map((inner) => join(folder, inner)),
                    ),
            );
            const assets = [...kinds.filter(([, type]) => type === 'File').map(([file]) => file), ...(yield* Effect.filter(nested.flat(), (file) => Effect.map(fs.stat(join(directory, file)), ({ type }) => type === 'File')))];
            const copied = yield* Effect.forEach(assets, (file) => Effect.map(fs.readFile(join(directory, file)), (source) => [file.split(sep).join('/'), source] as const));
            const manifests = { [MANIFESTS.plugin]: plugin, ...Record.fromEntries(copied), [MANIFESTS.hooks]: yield* Schema.encodeEffect(Hooks)({ ...hooks, modules: Array.map(names, (name) => `./${name}.js`) }) };
            return {
                build: {
                    lib: { entry: Record.fromEntries([...Array.map(Array.zip(names, hooks.modules), ([name, module]) => [posix.join(HOOKS, name), join(directory, HOOKS, module)] as const), ...programs]), formats: ['es'] },
                    target: 'esnext',
                    rolldownOptions: {
                        external: [/^node:/u, ...builtinModules],
                        output: {
                            entryFileNames: '[name].js',
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
