import { existsSync, globSync, readFileSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { reporting } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { playwright } from '@vitest/browser-playwright';
import { Array, Cause, Effect, Filter, Match, Option, Schema } from 'effect';
import { parseSync } from 'vite';
import { defineConfig, type Plugin, type TestProjectInlineConfiguration, type ViteUserConfig } from 'vitest/config';
import type { BrowserConfigOptions } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

interface Hosted {
    readonly suffix: Option.Option<string>;
    readonly plugins: NonNullable<TestProjectInlineConfiguration['plugins']>;
    readonly test: NonNullable<TestProjectInlineConfiguration['test']>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.Struct({ default: Schema.Struct({ name: Schema.String, devDependencies: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const browser = (): BrowserConfigOptions => ({ enabled: true, provider: playwright(), headless: true, instances: [{ browser: 'chromium' }], locators: { errorFormat: 'aria' }, trace: { mode: 'retain-on-failure' } });

const captured = (): Plugin => ({
    name: 'captured',
    configureVitest({ project, vitest }) {
        Object.assign(project.config.browser, { screenshotDirectory: resolve(vitest.config.attachmentsDir, '..', 'screenshots') });
    },
});

const manifested = (root: string) =>
    Effect.map(
        Effect.flatMap(Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(join(import.meta.dirname, root, 'package.json')).href, { with: { type: 'json' } }), catch: (cause) => new Cause.UnknownError(cause, `${root} holds no readable package.json`) }), Schema.decodeUnknownEffect(Manifest)),
        ({ default: manifest }) => manifest,
    );

const hosted = (root: string, { devDependencies = {} }: (typeof Manifest.Type)['default']): readonly Hosted[] => {
    const at = join(import.meta.dirname, root);
    const served = (file: string) => parseSync(file, readFileSync(join(at, file), 'utf-8')).module.staticImports.some(({ moduleRequest }) => moduleRequest.value.startsWith('@effect/platform-node'));
    const [node, workers] = existsSync(join(at, 'cloudflare.config.ts')) ? Array.partition(globSync('**/*.test.ts', { cwd: at, exclude: ['node_modules/**'] }), Filter.fromPredicate(served)) : [[], []];
    const runtimes = [
        { files: node, suffix: 'node', plugins: [] },
        { files: workers, suffix: 'workers', plugins: [cloudflareTest({ config: true })] },
    ].filter(({ files }) => files.length > 0);
    return Match.value(runtimes).pipe(
        Match.when(
            () => 'vitest-browser-react' in devDependencies,
            (): readonly Hosted[] => [{ suffix: Option.none(), plugins: [react({ compiler: { panicThreshold: 'all_errors' } }), captured()], test: { browser: browser() } }],
        ),
        Match.when(
            (all) => all.length > 1,
            (all) => all.map(({ files, suffix, plugins }) => ({ suffix: Option.some(suffix), plugins, test: { include: files } })),
        ),
        Match.orElse((all) => [{ suffix: Option.none(), plugins: all.flatMap(({ plugins }) => plugins), test: {} }]),
    );
};

const reports = (): Plugin => ({
    name: 'reports',
    config: ({ test }): ViteUserConfig => (test?.attachmentsDir === undefined ? {} : { test: { reporters: [['html', { outputDir: resolve(import.meta.dirname, test.attachmentsDir, '..', 'html') }]] } }),
});

const projects = Effect.gen(function* () {
    const roots = Array.dedupe(
        globSync(['{apps,libs}/**/tests/**/*.test.{ts,tsx}', 'tests/**/*.test.{ts,tsx}'], { cwd: import.meta.dirname, exclude: ['**/node_modules/**', '**/.*/**'] }).flatMap((file) =>
            Array.fromNullishOr(findPackageJSON(pathToFileURL(join(import.meta.dirname, file)))).map((manifest) => relative(import.meta.dirname, dirname(manifest))),
        ),
    ).filter((root) => Array.isReadonlyArrayEmpty(globSync('vitest.config.*', { cwd: join(import.meta.dirname, root) })));
    const storied = globSync('{apps,libs}/**/.storybook/main.ts', { cwd: import.meta.dirname, exclude: ['**/node_modules/**'] }).map((file) => dirname(dirname(file)));
    const [manifests, titles] = yield* Effect.all([Effect.forEach(roots, manifested), Effect.forEach(storied, manifested)]);
    const suites = Array.zipWith(roots, manifests, (root, manifest) => hosted(root, manifest).map(({ suffix, plugins, test }): TestProjectInlineConfiguration => ({ root, plugins, test: { ...test, name: Option.match(suffix, { onNone: () => manifest.name, onSome: (part) => `${manifest.name} ${part}` }) } })));
    const stories = Array.zipWith(storied, titles, (root, { name }): TestProjectInlineConfiguration => ({ plugins: [storybookTest({ configDir: join(import.meta.dirname, root, '.storybook') }), captured()], test: { name, api: { allowWrite: true }, browser: browser() } }));
    return [...suites.flat(), ...stories];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(projects, (declared): ViteUserConfig => ({
            plugins: [reports()],
            test: {
                ...reporting,
                coverage: { allowExternal: true, reportOnFailure: true },
                silent: 'passed-only',
                projects: declared,
            },
        })),
    ),
);
