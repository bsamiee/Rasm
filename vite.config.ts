import { join, relative } from 'node:path';
import { Array, String } from 'effect';
import { defaultClientConditions, defaultServerConditions, type Rollup, type UserConfig } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = import.meta.dirname;
const _UXP_MODULE = /^adobe:/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const project = (directory: string, config: UserConfig): UserConfig => ({ ...config, cacheDir: join(_ROOT, '.cache', 'vite', relative(_ROOT, directory)) });
const web = (directory: string, config: UserConfig): UserConfig => project(directory, { ...config, build: { ...config.build, target: 'esnext' }, server: { ...config.server, strictPort: true } });

const uxp = (directory: string, { main, manifest }: { readonly main: string; readonly manifest: object }): UserConfig =>
    project(directory, {
        build: {
            ssr: true,
            outDir: join(_ROOT, '.artifacts', relative(_ROOT, directory)),
            emptyOutDir: true,
            rolldownOptions: { input: { main }, platform: 'neutral', output: { format: 'cjs', entryFileNames: '[name].js', codeSplitting: false, paths: String.replace(_UXP_MODULE, '') } },
        },
        environments: { ssr: { resolve: { noExternal: true, builtins: [_UXP_MODULE], conditions: Array.intersection(defaultServerConditions, defaultClientConditions) } } },
        plugins: [
            {
                name: 'manifest',
                generateBundle(this: Rollup.PluginContext): void {
                    this.emitFile({ type: 'asset', fileName: 'manifest.json', source: JSON.stringify(manifest) });
                },
            },
        ],
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { project, uxp, web };
