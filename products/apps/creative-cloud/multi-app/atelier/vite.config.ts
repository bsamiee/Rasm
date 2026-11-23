import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, relative } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { helper } from '@rasm/applications/vite';
import { script } from '@rasm/extendscript/vite';
import { run } from '@rasm/process';
import { checks } from '@rasm/vite';
import { Array, Cause, Effect, FileSystem, Schema, Struct } from 'effect';
import type { Plugin, Rolldown, UserConfigFnPromise } from 'vite';

// --- [MODELS] --------------------------------------------------------------------------

const Node = Schema.Struct({
    data: Schema.Struct({ metadata: Schema.Struct({ buildSettings: Schema.Struct({ Atelier: Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.String, DEVELOPMENT_TEAM: Schema.String, CODE_SIGN_ENTITLEMENTS: Schema.String }) }) }) }),
}).annotate({ description: 'Atelier project node of the Nx graph, with the build settings @rasm/nx-apple resolves for its app target' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const listed = (fs: FileSystem.FileSystem, root: string, kept: RegExp) =>
    Effect.map(fs.readDirectory(root, { recursive: true }), (files) =>
        Array.map(
            files.filter((file) => kept.test(file)),
            (file) => join(root, file),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(
    function* () {
        const directory = import.meta.dirname;
        const resolved = createRequire(join(directory, 'package.json')).resolve;
        const [context, fs] = yield* Effect.all([Effect.context<Effect.Services<ReturnType<typeof run>>>(), FileSystem.FileSystem]);
        const compiler = Effect.map(Effect.fromNullishOr(findPackageJSON(`@typescript/typescript-${process.platform}-${process.arch}`, import.meta.resolve('@typescript/native/package.json'))), (found) => join(dirname(found), 'lib'));
        const assets = Effect.gen(function* () {
            const [vendors, root, pages] = yield* Effect.all([
                Effect.forEach([findPackageJSON('@types/photoshop', import.meta.url), findPackageJSON('@adobe/premierepro', import.meta.url), findPackageJSON('@adobe/cc-ext-uxp-types', import.meta.url), findPackageJSON('indesign-uxp-types', import.meta.url)], (found) => Effect.fromNullishOr(found)),
                compiler,
                Effect.map(Effect.fromNullishOr(findPackageJSON('uxp-after-effects', import.meta.url)), (found) => join(dirname(found), 'src', 'pages', 'after-effects-api')),
            ]);
            const [declared, libraries, documented] = yield* Effect.all([Effect.forEach(vendors, (manifest) => listed(fs, dirname(manifest), /(?:\.d\.ts|package\.json)$/u)), listed(fs, root, /\.d\.ts$/u), listed(fs, pages, /\.md$/u)]);
            const [read, { stdout }] = yield* Effect.all([
                Effect.forEach([...Array.map(Array.flatten(declared), (file) => [relative(directory, file), file] as const), ...Array.map(libraries, (file) => [join('typescript', relative(root, file)), file] as const)], ([fileName, file]) =>
                    fs.readFile(file).pipe(Effect.bindTo('source'), Effect.map(Struct.assign({ fileName }))),
                ),
                run(['mq', '--aggregate', '--from-file', join(directory, 'after-effects.mq'), '--output-format', 'jsonl', ...documented]),
            ]);
            return [...read, { fileName: 'after-effects.jsonl', source: stdout }];
        });
        const app = Effect.try({ try: () => readCachedProjectGraph().nodes['Atelier'], catch: (cause) => new Cause.UnknownError(cause, 'No cached Nx project graph, which an nx run writes') }).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Node)),
            Effect.map(({ data }) => data.metadata.buildSettings.Atelier),
            Effect.map((settings) => ({ identifier: settings.PRODUCT_BUNDLE_IDENTIFIER, team: settings.DEVELOPMENT_TEAM, entitlements: join(workspaceRoot, settings.CODE_SIGN_ENTITLEMENTS) })),
        );
        const declarations = {
            name: 'declarations',
            apply: 'build',
            buildStart(this: Rolldown.PluginContext): Promise<void> {
                return Effect.flatMap(
                    assets,
                    Effect.forEach((asset) => Effect.sync(() => this.emitFile({ type: 'asset', ...asset })), { discard: true }),
                ).pipe(Effect.runPromiseWith(context));
            },
            writeBundle: ({ dir = directory }) =>
                Effect.runPromiseWith(context)(
                    Effect.gen(function* () {
                        const root = yield* compiler;
                        yield* Effect.all([fs.copyFile(join(root, 'tsc.original'), join(dir, 'tsc')), fs.makeDirectory(join(dir, 'Helpers'))], { discard: true });
                        yield* fs.symlink(join('..', 'Resources', 'lib.d.ts'), join(dir, 'Helpers', 'lib.d.ts'));
                    }),
                ),
        } satisfies Plugin;
        return {
            build: { rolldownOptions: { experimental: { lazyBarrel: true }, output: { codeSplitting: false } } },
            plugins: [checks(), declarations, script(resolved('@rasm/extendscript/host'), 'runtime'), script(resolved('es5-shim'), 'shim'), script(resolved('es5-shim/es5-sham.js'), 'sham'), yield* helper(join(directory, 'main.ts'), 'server', { KeepAlive: true }, app)],
        };
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
