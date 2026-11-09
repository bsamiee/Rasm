import { createRequire } from 'node:module';
import { join } from 'node:path';
import { cloudflare } from '@cloudflare/vite-plugin';
import { sites } from '@openai/sites-vite-plugin';
import { siteHead } from '@rasm/hosting/head';
import { nodeBundle } from '@rasm/hosting/node-bundle';
import { Sites } from '@rasm/hosting/sites';
import { nodeServer } from '@rasm/hosting/vite';
import { pdfjs } from '@rasm/media/vite';
import { react } from '@rasm/ui/vite';
import tailwindcss from '@tailwindcss/vite';
import { bindings } from 'cf/config';
import { Config, Effect } from 'effect';
import { defineConfig, searchForWorkspaceRoot } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const workspace = searchForWorkspaceRoot(import.meta.dirname);
const { name }: { readonly name: string } = createRequire(import.meta.url)('./package.json');
const tooling = Effect.runSync(Effect.map(Effect.all([Config.withDefault(Config.Boolean('VITEST'), false), Config.withDefault(Config.Boolean('STORYBOOK'), false)]), ([vitest, storybook]) => vitest || storybook));

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(({ command }) => ({
    build: { target: 'esnext', rolldownOptions: { experimental: { lazyBarrel: true }, output: { strictExecutionOrder: true } } },
    environments: { [nodeBundle.environment]: { build: { outDir: join(workspace, '.artifacts', 'vite', name) } } },
    plugins: [
        react(),
        tailwindcss(),
        pdfjs(),
        sites(),
        ...(tooling
            ? []
            : [
                  siteHead({ master: 'favicon.svg', stylesheet: 'site/theme.css', fill: '--color-accent', background: '--color-background' }),
                  nodeServer({ entry: './server/main.ts' }),
                  cloudflare({
                      types: { generate: false },
                      persistState: { path: join(workspace, '.cache', 'cloudflare', name) },
                      ...(command === 'serve' && { config: (loaded) => ({ ...loaded, worker: { ...loaded.worker, env: { ...loaded.worker.env, OWNER_EMAIL: bindings.text(Sites.local.email) } } }) }),
                  }),
              ]),
    ],
}));
