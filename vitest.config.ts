import { existsSync, globSync, readFileSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, join, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { cloudflareTest } from '@cloudflare/vitest-plugin';
import { readJsonFile } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, type PackageJson } from '@nx/devkit/internal';
import { reporting } from '@rasm/testing/vitest';
import { storybookTest } from '@storybook/addon-vitest/vitest-plugin';
import react from '@vitejs/plugin-react';
import { type PlaywrightProviderOptions, playwright } from '@vitest/browser-playwright';
import { Array, Cause, Config, Effect, Filter, Match, Option, Schema } from 'effect';
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

const browser = (connect: Option.Option<NonNullable<PlaywrightProviderOptions['connectOptions']>>): BrowserConfigOptions => ({
    enabled: true,
    provider: playwright({ ...Option.getOrUndefined(Option.map(connect, (connectOptions) => ({ connectOptions }))) }),
    headless: true,
    instances: [{ browser: 'chromium' }],
    locators: { errorFormat: 'aria' },
    trace: { mode: 'retain-on-failure' },
});

const placed = (): Plugin => ({
    name: 'placed',
    configureVitest({ project, vitest: { config } }) {
        Object.assign(project.config.browser, { screenshotDirectory: resolve(config.attachmentsDir, '..', 'screenshots') });
        Object.assign(config, { reporters: [...config.reporters.filter((reporter) => !(Array.isArray(reporter) && reporter[0] === 'html')), ['html', { outputDir: resolve(config.attachmentsDir, '..', 'html') }]] });
    },
});

const manifested = (root: string) =>
    Effect.map(
        Effect.flatMap(Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(join(import.meta.dirname, root, 'package.json')).href, { with: { type: 'json' } }), catch: (cause) => new Cause.UnknownError(cause, `${root} holds no readable package.json`) }), Schema.decodeUnknownEffect(Manifest)),
        ({ default: manifest }) => manifest,
    );

const hosted = (root: string, { devDependencies = {} }: (typeof Manifest.Type)['default'], connect: Parameters<typeof browser>[0]): readonly Hosted[] => {
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
            (): readonly Hosted[] => [{ suffix: Option.none(), plugins: [react({ compiler: { panicThreshold: 'all_errors' } })], test: { browser: browser(connect) } }],
        ),
        Match.when(
            (all) => all.length > 1,
            (all) => all.map(({ files, suffix, plugins }) => ({ suffix: Option.some(suffix), plugins, test: { include: files } })),
        ),
        Match.orElse((all) => [{ suffix: Option.none(), plugins: all.flatMap(({ plugins }) => plugins), test: {} }]),
    );
};

const projects = Effect.gen(function* () {
    const patterns = buildPackageJsonPatterns(import.meta.dirname, (file: string) => readJsonFile<PackageJson>(join(import.meta.dirname, file)));
    const members = new Set(
        globSync(patterns.positive, { cwd: import.meta.dirname, exclude: ['**/node_modules/**', '**/.*/**'] })
            .filter(buildPackageJsonWorkspacesMatcher(patterns))
            .map(dirname),
    );
    const found = Array.dedupe(
        globSync(
            [...members].map((root) => join(root, '**/*.test.{ts,tsx}')),
            { cwd: import.meta.dirname, exclude: ['**/node_modules/**', '**/.*/**'] },
        ).flatMap((file) => Array.fromNullishOr(findPackageJSON(pathToFileURL(join(import.meta.dirname, file)))).map((manifest) => relative(import.meta.dirname, dirname(manifest)))),
    ).filter((root) => members.has(root));
    const storied = globSync(
        [...members].map((root) => join(root, '.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}')),
        { cwd: import.meta.dirname },
    ).map((file) => dirname(dirname(file)));
    const [declared, titles] = yield* Effect.all([Effect.forEach(found, manifested), Effect.forEach(storied, manifested)]);
    const [roots, manifests] = Array.unzip(Array.zip(found, declared).filter(([, { devDependencies = {} }]) => 'vitest' in devDependencies));
    const [endpoint, exposed] = yield* Config.all([Config.option(Config.String('PW_TEST_CONNECT_WS_ENDPOINT')), Config.option(Config.String('PW_TEST_CONNECT_EXPOSE_NETWORK'))]);
    const connect = Option.map(endpoint, (wsEndpoint) => ({ wsEndpoint, ...Option.getOrUndefined(Option.map(exposed, (exposeNetwork) => ({ exposeNetwork }))) }));
    const suites = Array.zipWith(roots, manifests, (root, manifest) =>
        hosted(root, manifest, connect).map(({ suffix, plugins, test }): TestProjectInlineConfiguration => ({
            root,
            plugins: [...plugins, placed()],
            test: {
                ...test,
                name: Option.match(suffix, { onNone: () => manifest.name, onSome: (part) => `${manifest.name} ${part}` }),
                ...(Array.isReadonlyArrayNonEmpty(Array.intersection(Object.keys(manifest.devDependencies ?? {}), ['nx', '@nx/devkit'])) && { env: { NX_DAEMON: 'false', NX_WORKSPACE_DATA_DIRECTORY: '.nx/workspace-data', NO_COLOR: '1', FORCE_COLOR: '0' } }),
            },
        })),
    );
    const stories = Array.zipWith(storied, titles, (root, { name }): TestProjectInlineConfiguration => ({ plugins: [storybookTest({ configDir: join(import.meta.dirname, root, '.storybook') }), placed()], test: { name, api: { allowWrite: true }, browser: browser(connect) } }));
    return [...suites.flat(), ...stories];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

export default defineConfig(() =>
    Effect.runPromise(
        Effect.map(projects, (declared): ViteUserConfig => ({
            test: {
                ...reporting,
                attachmentsDir: '.artifacts/vitest/attachments',
                coverage: { allowExternal: true, reportOnFailure: true },
                silent: 'passed-only',
                projects: declared,
            },
        })),
    ),
);
