import { createRequire } from 'node:module';
import { join, relative } from 'node:path';
import { cloudflare } from '@cloudflare/vite-plugin';
import compiler from '@vitejs/plugin-react';
import { Array, Config, Effect, Record, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, type PluginOption, type Rollup, type UserConfig, type UserConfigFnObject } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _HOSTED = /^adobe:/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const react = (): PluginOption => compiler({ compiler: { panicThreshold: 'all_errors' } });
const worker =
    (directory: string, plugins: PluginOption, hosting: PluginOption): UserConfigFnObject =>
    ({ mode }) => ({
        build: { target: 'esnext' },
        plugins: [
            plugins,
            Effect.runSync(Effect.map(Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]), Array.some(Boolean)))
                ? []
                : [hosting, cloudflare({ types: { generate: false }, ...(mode === 'test' ? { persistState: false, inspectorPort: false } : { persistState: { path: join(_ROOT, '.cache', 'cloudflare', relative(_ROOT, directory)) } }) })],
        ],
    });

const uxp = (directory: string, { main, manifest }: { readonly main: string; readonly manifest: object }): UserConfig => {
    const load = createRequire(import.meta.url);
    const encoding = load.resolve('@exodus/bytes/encoding-lite.js');
    return {
        build: {
            ssr: true,
            outDir: join(_ROOT, '.artifacts', relative(_ROOT, directory)),
            emptyOutDir: true,
            rolldownOptions: {
                input: { main },
                platform: 'neutral',
                transform: {
                    inject: Record.fromIterableWith(
                        Array.filter(Object.keys(load(encoding)), (name) => name in globalThis),
                        (name) => [name, [encoding, name] satisfies [string, string]],
                    ),
                },
                output: { format: 'cjs', entryFileNames: '[name].js', codeSplitting: false, paths: String.replace(_HOSTED, '') },
            },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [_HOSTED], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
        plugins: [
            {
                name: 'manifest',
                generateBundle(this: Rollup.PluginContext): void {
                    this.emitFile({ type: 'asset', fileName: 'manifest.json', source: JSON.stringify(manifest) });
                },
            },
        ],
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { react, uxp, worker };
