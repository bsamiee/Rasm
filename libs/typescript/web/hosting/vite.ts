import { Config, Effect, flow } from 'effect';
import type { ConfigEnv, PluginOption, UserConfigFnPromise } from 'vite';
import type { Head } from '#/head';
import { nodeBundle } from '#/node-bundle';

// --- [COMPOSITION] ---------------------------------------------------------------------

const cloudflareWorker = ({
    plugins,
    hosts = [],
    head,
    nodeEntry,
    nodeOutDir,
    state,
}: {
    readonly plugins: PluginOption;
    readonly hosts?: PluginOption;
    readonly head: Head;
    readonly nodeEntry?: string;
    readonly nodeOutDir: string;
    readonly state: { readonly path: string; readonly ephemeralMode: string };
}): UserConfigFnPromise =>
    flow(
        Effect.fnUntraced(function* ({ mode }: ConfigEnv) {
            const [storybook, analyze] = yield* Effect.all([Config.withDefault(Config.Boolean('STORYBOOK'), false), Config.withDefault(Config.Boolean('ANALYZE'), false)]);
            const hosted =
                mode === 'test' || storybook
                    ? []
                    : yield* Effect.map(Effect.all([Effect.promise(() => import('#/head')), Effect.promise(() => import('@cloudflare/vite-plugin'))], { concurrency: 'unbounded' }), ([{ siteHead }, { cloudflare }]) => [
                          hosts,
                          siteHead(head),
                          nodeEntry === undefined ? [] : nodeBundle(nodeEntry, nodeOutDir),
                          cloudflare({ types: { generate: false }, ...(mode === state.ephemeralMode ? { persistState: false, inspectorPort: false } : { persistState: { path: state.path } }) }),
                      ]);
            const diagnosed = analyze ? [(yield* Effect.promise(() => import('rolldown/experimental'))).bundleAnalyzerPlugin({ format: 'md' })] : [];
            return { build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true } } }, plugins: [plugins, hosted, diagnosed] };
        }),
        Effect.runPromise,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { cloudflareWorker };
