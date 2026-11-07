import { join, relative } from 'node:path';
import process from 'node:process';
import { Config, Effect, flow, Option } from 'effect';
import { type ConfigEnv, type Plugin, searchForWorkspaceRoot } from 'vite';
import { HostingError } from '#/error.ts';
import type { Head } from '#/head.ts';
import { nodeBundle } from '#/node-bundle.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosting = flow(
    Effect.fnUntraced(
        function* ({ head, nodeEntry, mode }: { readonly head: Head; readonly nodeEntry: string; readonly mode: ConfigEnv['mode'] }) {
            const [{ siteHead }, { cloudflare }, analyze, context] = yield* Effect.all([Effect.promise(() => import('#/head.ts')), Effect.promise(() => import('@cloudflare/vite-plugin')), Config.withDefault(Config.Boolean('ANALYZE'), false), Effect.context<never>()], { concurrency: 'unbounded' });
            const workspace = searchForWorkspaceRoot(process.cwd());
            const project = relative(workspace, process.cwd());
            const bundle: Plugin = {
                name: '@rasm/hosting',
                config: (_, { command }) => ({
                    environments: {
                        [nodeBundle.layout.environment]: {
                            consumer: 'server',
                            resolve: { noExternal: true },
                            build: { outDir: join(workspace, '.artifacts', project), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [nodeBundle.layout.entry]: nodeEntry }, platform: 'node' } },
                        },
                    },
                    ...(analyze && command === 'build' ? { build: { rolldownOptions: { devtools: { mode: 'report' } } } } : {}),
                }),
                buildApp: {
                    order: 'post',
                    handler: (builder) =>
                        Effect.fromNullishOr(builder.environments[nodeBundle.layout.environment]).pipe(
                            Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.build(nodeBundle.layout.environment, nodeEntry) })),
                            Effect.asVoid,
                            Effect.runPromiseWith(context),
                        ),
                },
            };
            return [bundle, siteHead(head), cloudflare({ types: { generate: false }, ...(mode === 'e2e' ? { persistState: false, inspectorPort: false } : { persistState: { path: join(workspace, '.cache', 'cloudflare', project) } }) })];
        },
        Effect.when(Effect.map(Effect.all([Config.withDefault(Config.Boolean('VITEST'), false), Config.withDefault(Config.Boolean('STORYBOOK'), false)]), ([vitest, storybook]) => !(vitest || storybook))),
        Effect.map(Option.getOrElse(() => [])),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hosting };
