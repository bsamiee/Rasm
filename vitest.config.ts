import { globSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { reporting } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { playwright } from '@vitest/browser-playwright';
import { Array, Cause, Effect, Option, Predicate, type Record, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { defineConfig, type Plugin, type TestProjectInlineConfiguration, type ViteUserConfig } from 'vitest/config';
import type { BrowserConfigOptions } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Hosted {
    readonly suffix: Option.Option<string>;
    readonly plugins: NonNullable<TestProjectInlineConfiguration['plugins']>;
    readonly test: NonNullable<TestProjectInlineConfiguration['test']>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.Struct({ default: Schema.Struct({ name: Schema.String }) });
const Placed = Schema.Struct({ plugins: Schema.Array(Schema.Union([Schema.String, Schema.Struct({ plugin: Schema.String, options: Schema.optionalKey(Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) })) })])) });

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
        Effect.flatMap(Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(join(import.meta.dirname, root, 'package.json')).href, { with: { type: 'json' } }), catch: (cause) => new Cause.UnknownError(cause, `${root} holds no readable package.json`) }), Schema.decodeUnknownEffect(Manifest)),
        ({ default: { name } }) => name,
    );

const reports = (): Plugin => ({
    name: 'reports',
    config: ({ test }): ViteUserConfig => (test?.attachmentsDir === undefined ? {} : { test: { reporters: [['html', { outputDir: resolve(import.meta.dirname, test.attachmentsDir, '..', 'html') }]] } }),
});

const split = (include: readonly string[], suffix: string, plugins: Hosted['plugins'] = []): Hosted => ({ suffix: Option.some(suffix), plugins, test: { include: [...include] } });

// --- [TABLES] --------------------------------------------------------------------------

const HOSTED: Record.ReadonlyRecord<string, readonly Hosted[]> = {
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
    ).filter((root) => Array.isReadonlyArrayEmpty(globSync('vitest.config.*', { cwd: join(import.meta.dirname, root) })));
    const [names, titles, { plugins: rows }] = yield* Effect.all([Effect.forEach(roots, named), Effect.forEach(STORIES, named), Schema.decodeUnknownEffect(Placed)(readNxJson(import.meta.dirname))]);
    const template = yield* Effect.fromOption(Array.findFirst(rows, (row) => (Predicate.isString(row) || row.plugin !== '@rasm/nx-vitest' ? Option.none() : Option.fromUndefinedOr(row.options?.outputPath))));
    const suites = Array.zipWith(roots, names, (root, name) =>
        (HOSTED[root] ?? [{ suffix: Option.none(), plugins: [], test: {} }]).map(({ suffix, plugins, test }): TestProjectInlineConfiguration => ({ root, plugins, test: { ...test, name: Option.match(suffix, { onNone: () => name, onSome: (part) => `${name} ${part}` }) } })),
    );
    const stories = Array.zipWith(STORIES, titles, (root, name): TestProjectInlineConfiguration => ({ plugins: [storybookTest({ configDir: join(import.meta.dirname, root, '.storybook') }), captured()], test: { name, api: { allowWrite: true }, browser: browser() } }));
    return { declared: [...suites.flat(), ...stories], written: dirname(template) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(
            projects,
            ({ declared, written }): ViteUserConfig => ({
                plugins: [reports()],
                test: {
                    ...reporting,
                    attachmentsDir: join(written, 'attachments'),
                    coverage: { reporter: ['text', 'html'], reportOnFailure: true, reportsDirectory: join(written, 'coverage'), skipFull: true },
                    outputFile: { junit: join(written, 'junit.xml') },
                    silent: 'passed-only',
                    projects: declared,
                },
            }),
        ),
    ),
);
