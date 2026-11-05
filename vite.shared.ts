import { isAbsolute, join, relative } from 'node:path';
import viteReact from '@vitejs/plugin-react';
import { Config, Effect, flow } from 'effect';
import type { ConfigEnv, Plugin, PluginOption, UserConfigFnPromise } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const testMode = 'test';
const ROOT = import.meta.dirname;
const EXPERIMENTAL = { lazyBarrel: true };

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = (folder: string, directory: string) => join(ROOT, folder, relative(ROOT, directory));

// --- [COMPOSITION] ---------------------------------------------------------------------

const react = () => viteReact({ compiler: { panicThreshold: 'all_errors' } });
const servedConfigSources: Plugin = {
    name: 'served-config-sources',
    enforce: 'post',
    config: ({ server }) => {
        const fs = server?.fs;
        if (fs?.deny !== undefined) {
            fs.deny = fs.deny.filter((pattern) => !isAbsolute(pattern));
        }
    },
};
const cloudflareWorker = (directory: string, { plugins, hosting }: { readonly plugins: PluginOption; readonly hosting: PluginOption }): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const [storybook, vitest] = yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('VITEST'), false)]);
            const hosted =
                storybook || vitest
                    ? []
                    : [hosting, servedConfigSources, (yield* Effect.promise(() => import('@cloudflare/vite-plugin'))).cloudflare({ types: { generate: false }, ...(mode === testMode ? { persistState: false, inspectorPort: false } : { persistState: { path: located(join('.cache', 'cloudflare'), directory) } }) })];
            return { build: { target: 'esnext', rolldownOptions: { experimental: EXPERIMENTAL } }, plugins: [plugins, hosted] };
        }),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cloudflareWorker, located, react, testMode };
