import { globSync, readFileSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { playwright } from '@vitest/browser-playwright';
import { Array, Cause, Effect, Option, Record, Schema } from 'effect';
import { configDefaults, defineConfig, type Plugin, type TestProjectInlineConfiguration, type ViteUserConfig } from 'vitest/config';
import type { BrowserConfigOptions } from 'vitest/node';
import nx from './nx.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

interface Hosted {
    readonly suffix: Option.Option<string>;
    readonly plugins: NonNullable<TestProjectInlineConfiguration['plugins']>;
    readonly test: NonNullable<TestProjectInlineConfiguration['test']>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITTEN = join(nx.pluginsConfig['@rasm/nx-distribution'].outputPath, 'vitest');

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const browser = (): BrowserConfigOptions => ({ enabled: true, provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure' } });

const captured = (): Plugin => ({
    name: 'captured',
    configureVitest({ project, vitest }) {
        Object.assign(project.config.browser, { screenshotDirectory: resolve(vitest.config.attachmentsDir, '..', 'screenshots') });
    },
});

const named = (root: string) =>
    Effect.map(
        Effect.flatMap(
            Effect.try({ try: () => readFileSync(join(import.meta.dirname, root, 'package.json'), 'utf8'), catch: (cause) => new Cause.UnknownError(cause, `${root} holds no readable package.json`) }),
            Schema.decodeEffect(Manifest),
        ),
        ({ name }) => name,
    );

const split = (include: readonly string[], suffix: string, plugins: Hosted['plugins'] = []): Hosted => ({ suffix: Option.some(suffix), plugins, test: { include: [...include] } });

// --- [TABLES] --------------------------------------------------------------------------

const HOSTED: Record.ReadonlyRecord<string, readonly Hosted[]> = {
    'tests/typescript/nx': [{ suffix: Option.none(), plugins: [], test: { globalSetup: ['setup.ts'], fileParallelism: false, testTimeout: 120_000 } }],
    'libs/typescript/data/tests/database': [split(['node.test.ts'], 'node'), split(['d1.test.ts'], 'workers', [cloudflareTest({ config: true })])],
    'libs/typescript/data/tests/object-storage': [{ suffix: Option.none(), plugins: [cloudflareTest({ config: true })], test: {} }],
    'libs/typescript/messaging/tests/webhooks': [split(['node.test.ts'], 'node'), split(['workers.test.ts'], 'workers', [cloudflareTest({ config: true })])],
    'libs/typescript/runtime/tests/workers': [split(['deploy.test.ts'], 'node'), split(['unit.test.ts', 'integration.test.ts'], 'workers', [cloudflareTest({ config: true })])],
    'libs/typescript/web/tests/ui': [{ suffix: Option.none(), plugins: [react({ compiler: { panicThreshold: 'all_errors' } }), captured()], test: { browser: browser() } }],
};
const STORIES = ['apps/web/portfolio', 'libs/typescript/web/ui'] as const;

const projects = Effect.gen(function* () {
    const roots = Array.dedupe(
        globSync(['{apps,libs}/**/tests/**/*.test.{ts,tsx}', 'tests/**/*.test.{ts,tsx}'], { cwd: import.meta.dirname, exclude: ['**/node_modules/**', '**/.*/**'] }).flatMap((file) =>
            Array.fromNullishOr(findPackageJSON(pathToFileURL(join(import.meta.dirname, file)))).map((manifest) => relative(import.meta.dirname, dirname(manifest))),
        ),
    );
    const suites = yield* Effect.forEach(roots, (root) =>
        Effect.map(named(root), (name) => (HOSTED[root] ?? [{ suffix: Option.none(), plugins: [], test: {} }]).map(({ suffix, plugins, test }): TestProjectInlineConfiguration => ({ root, plugins, test: { ...test, name: Option.match(suffix, { onNone: () => name, onSome: (part) => `${name} ${part}` }) } }))),
    );
    const stories = yield* Effect.forEach(STORIES, (root) => Effect.map(named(root), (name): TestProjectInlineConfiguration => ({ plugins: [storybookTest({ configDir: join(import.meta.dirname, root, '.storybook') }), captured()], test: { name, api: { allowWrite: true }, browser: browser() } })));
    return [...suites.flat(), ...stories];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(projects, (declared): ViteUserConfig => ({
            test: {
                chaiConfig: { includeStack: true, truncateThreshold: 0 },
                attachmentsDir: join(WRITTEN, 'attachments'),
                coverage: { reporter: ['text', 'html'], reportOnFailure: true, reportsDirectory: join(WRITTEN, 'coverage'), skipFull: true },
                outputFile: { junit: join(WRITTEN, 'junit.xml') },
                reporters: [...configDefaults.reporters, 'junit', ['html', { outputDir: join(WRITTEN, 'html') }]],
                silent: 'passed-only',
                projects: declared,
            },
        })),
    ),
);
