import { globSync, readFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join } from 'node:path';
import process from 'node:process';
import { bundle } from '@rasm/uxp/vite';
import { Array, Config, Effect, Record } from 'effect';
import { NetAddress } from 'effect/net';
import { build } from 'plist';
import type { Plugin, Rolldown, UserConfigFnPromise } from 'vite';
import { atelierLink } from '#/atelier-link';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default Effect.fnUntraced(function* () {
    const directory = import.meta.dirname;
    const name = basename(directory);
    const [identifier, program] = yield* Effect.all([Config.String('PRODUCT_BUNDLE_IDENTIFIER'), Config.String('ATELIER_PATH')]);
    const addon = `@swc/core-${process.platform}-${process.arch}`;
    const binding = createRequire(createRequire(join(directory, 'package.json')).resolve('@swc/core')).resolve(addon);
    const label = `${identifier}.server`;
    const agent = build({
        Label: label,
        BundleProgram: `Contents/Helpers/${name}`,
        AssociatedBundleIdentifiers: [identifier],
        KeepAlive: true,
        EnvironmentVariables: { MCP_LISTEN: NetAddress.formatInet(atelierLink.listen), ATELIER_PATH: program, SWC_BINARY_PATH: join(dirname(dirname(program)), 'Frameworks', basename(binding)) },
    });
    return bundle(directory, {
        build: { ssr: true, rolldownOptions: { input: { [name]: join(directory, 'main.ts') }, external: (id) => id.startsWith('@swc/core-') && id !== addon, output: { entryFileNames: '[name].mjs' } } },
        ssr: { noExternal: true },
        plugins: [
            {
                name: 'sea',
                enforce: 'pre',
                resolveId: (id) => (id === addon ? `\0${addon}` : undefined),
                load: (id) => (id === `\0${addon}` ? 'module.exports = require(process.env.SWC_BINARY_PATH);' : undefined),
                generateBundle: {
                    order: 'post',
                    handler(this: Rolldown.PluginContext, _options, output): void {
                        const { outDir } = this.environment.config.build;
                        const folders = Array.dedupe(Object.values(output).flatMap((file) => (file.type === 'chunk' ? file.moduleIds.filter(isAbsolute).map(dirname) : [])));
                        const assets = Record.fromIterableWith(
                            Array.flatMap(folders, (folder) => globSync('*.wasm', { cwd: folder, withFileTypes: true })),
                            ({ name: file, parentPath }) => [file, join(parentPath, file)],
                        );
                        this.emitFile({ type: 'asset', fileName: 'sea.json', source: `${JSON.stringify({ main: join(outDir, `${name}.mjs`), mainFormat: 'module', output: join(outDir, name), useVfs: true, disableExperimentalSEAWarning: true, assets }, null, 4)}\n` });
                        this.emitFile({ type: 'asset', fileName: join('LaunchAgents', `${label}.plist`), source: agent });
                        this.emitFile({ type: 'asset', fileName: basename(binding), source: readFileSync(binding) });
                    },
                },
            } satisfies Plugin,
        ],
    });
}, Effect.runPromise) satisfies UserConfigFnPromise;
