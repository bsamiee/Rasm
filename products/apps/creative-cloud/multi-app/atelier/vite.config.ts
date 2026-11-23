import { createRequire, findPackageJSON } from 'node:module';
import { dirname, join, relative } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { helper } from '@rasm/applications/vite';
import { script } from '@rasm/extendscript/vite';
import { run } from '@rasm/process';
import { checks } from '@rasm/vite';
import { Array, Effect, FileSystem, Struct } from 'effect';
import type { Plugin, Rolldown, UserConfigFnPromise } from 'vite';

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
        const declarations = {
            name: 'declarations',
            apply: 'build',
            buildStart(this: Rolldown.PluginContext): Promise<void> {
                return Effect.flatMap(
                    assets,
                    Effect.forEach((asset) => Effect.sync(() => this.emitFile({ type: 'asset', ...asset })), { discard: true }),
                ).pipe(Effect.runPromiseWith(context));
            },
            writeBundle: ({ dir = directory }) => Effect.runPromiseWith(context)(Effect.flatMap(compiler, (root) => fs.copyFile(join(root, 'tsc.original'), join(dir, 'tsc')))),
        } satisfies Plugin;
        return {
            build: { rolldownOptions: { output: { codeSplitting: false } } },
            plugins: [checks(), declarations, script(resolved('@rasm/extendscript/host'), 'runtime'), script(resolved('es5-shim'), 'shim'), script(resolved('es5-shim/es5-sham.js'), 'sham'), yield* helper(join(directory, 'main.ts'), 'server')],
        };
    },
    Effect.provide(layer),
    Effect.runPromise,
) satisfies UserConfigFnPromise;
