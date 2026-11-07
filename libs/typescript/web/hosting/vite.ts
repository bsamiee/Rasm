import { join, relative } from 'node:path';
import { Config, Effect, flow } from 'effect';
import { type ConfigEnv, type PluginOption, searchForWorkspaceRoot, type UserConfig, type UserConfigFnPromise } from 'vite';
import type { Head } from '#/head.ts';
import { nodeBundle } from '#/node-bundle.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const cloudflareWorker = ({ directory, plugins, hosts, head, nodeEntry }: { readonly directory: string; readonly plugins: () => PluginOption; readonly hosts?: () => PluginOption; readonly head: Head; readonly nodeEntry?: string }): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ command, mode }: ConfigEnv) {
            const [storybook, analyze] = yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('ANALYZE'), false)]);
            const root = searchForWorkspaceRoot(directory);
            const hosted =
                mode === 'test' || storybook
                    ? []
                    : yield* Effect.map(Effect.all([Effect.promise(() => import('#/head.ts')), Effect.promise(() => import('@cloudflare/vite-plugin'))], { concurrency: 'unbounded' }), ([{ siteHead }, { cloudflare }]) => [
                          hosts?.(),
                          siteHead(head),
                          nodeEntry === undefined ? [] : nodeBundle(nodeEntry, join(root, '.artifacts', relative(root, directory))),
                          cloudflare({ types: { generate: false }, ...(mode === 'e2e' ? { persistState: false, inspectorPort: false } : { persistState: { path: join(root, '.cache', 'cloudflare', relative(root, directory)) } }) }),
                      ]);
            return {
                build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true }, ...(analyze && command === 'build' && mode !== 'test' ? { devtools: { mode: 'report' } } : {}) } },
                plugins: [plugins(), hosted],
            } satisfies UserConfig;
        }),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cloudflareWorker };
