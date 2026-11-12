import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-playwright';
import manifest from '@rasm/nx-playwright/package.json' with { type: 'json' };
import { Array, Effect, Order, Predicate, Record, Schema } from 'effect';
import { cases, inferred, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMPOSED = [
    { dependencies: true, target: 'build--compose' },
    { dependencies: true, target: 'serve--compose', waitFor: 'ready' },
] as const;
const SPEC = "import { test } from '@playwright/test';\n\ntest('opens', async ({ page }) => {\n    await page.goto('/');\n});\n";
const config = (servers: string) => `import { defineConfig } from '@playwright/test';\n\nexport default defineConfig({ testDir: '.', reporter: [['html', { open: 'never' }]], ${servers} });\n`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: ['@playwright/test'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["site", "reused", "owned", "split"] }\n',
        'site/package.json': '{ "name": "site", "nx": { "targets": { "serve": { "command": "node server.js", "continuous": true, "readyWhen": { "signal": true } }, "preview": { "command": "node server.js --built", "continuous": true, "readyWhen": { "signal": true } } } } }\n',
        'reused/package.json': '{ "name": "reused", "type": "module" }\n',
        'reused/playwright.config.ts': config("webServer: { command: 'npx nx run site:serve', url: 'http://localhost:4173', reuseExistingServer: true }"),
        'reused/site.spec.ts': SPEC,
        'owned/package.json': '{ "name": "owned", "type": "module" }\n',
        'owned/playwright.config.ts': config("webServer: { command: 'npx nx run site:serve', url: 'http://localhost:4173', reuseExistingServer: false }"),
        'owned/site.spec.ts': SPEC,
        'split/package.json': '{ "name": "split", "type": "module" }\n',
        'split/playwright.config.ts': config("projects: [{ name: 'chromium', webServer: { command: 'npx nx run site:preview', url: 'http://localhost:4174', reuseExistingServer: true } }]"),
        'split/site.spec.ts': SPEC,
    },
    broken: ['owned/playwright.config.ts', "import { defineConfig } from '@playwright/test';\n\nexport default defineConfig({ testDir:\n"],
};

const REUSED = [
    { command: 'bunx nx run site:serve', source: '"should infer the serve dependency from a bun-prefixed webServer command"' },
    { command: 'bunx nx serve site', source: '"should infer the serve dependency from a bun-prefixed webServer command"' },
    { command: 'bun x nx run site:serve', source: '"should infer the serve dependency from a bun-prefixed webServer command"' },
    { command: 'bun x nx serve site', source: '"should infer the serve dependency from a bun-prefixed webServer command"' },
    { command: 'npx nx run site:serve:production', source: '"should keep the dependency but not gate a webServer command with a trailing configuration"' },
] as const;

const PLUGIN = 'nrwl/nx@2a424d3 packages/playwright/src/plugins/plugin.spec.ts';
const SITE = "{ command: 'npx nx run site:serve', port: 4173, reuseExistingServer: true }";
const SERVERS = [
    {
        label: 'two reused servers',
        servers: `${SITE}, { command: 'npx nx run api:serve', port: 4300, reuseExistingServer: true }`,
        served: ['api:serve', 'site:serve'],
        source: '"should not set parallelism to false and should infer dependsOn using the tasks run in the different webServer.command that have reuseExistingServer set to true"',
    },
    {
        label: 'a reused server beside one Playwright owns',
        servers: `${SITE}, { command: 'npx nx run api:serve', port: 4300, reuseExistingServer: false }`,
        served: ['site:serve'],
        source: '"should not set parallelism to false and should infer dependsOn using the tasks run in the different webServer.command that have reuseExistingServer set to true"',
    },
    { label: 'a reused server beside one that waits for command output', servers: `${SITE}, { command: 'npx nx run api:serve', port: 4300, reuseExistingServer: true, wait: { stdout: /ready/ } }`, served: ['site:serve'], source: '"should not gate or depend on a webServer that waits for command output"' },
    {
        label: 'a reused server beside one that sets env',
        servers: `${SITE}, { command: 'npx nx run api:serve', url: 'http://localhost:4300', reuseExistingServer: true, env: { PORT: '4300' } }`,
        served: ['site:serve'],
        source: '"should not gate or depend on a webServer that sets env, leaving its launch to Playwright"',
    },
] as const;
const ATOMS = ['e2e-ci--tests/run-me-2.spec.ts', 'e2e-ci--tests/run-me.spec.ts'];

// --- [MODELS] --------------------------------------------------------------------------

const Waited = Schema.Struct({ projects: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])), target: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const only = (folder: string, files: Record.ReadonlyRecord<string, string>, shared: Record.ReadonlyRecord<string, string> = {}): Subject => {
    const served = '{ "targets": { "serve": { "command": "node server.js", "continuous": true, "readyWhen": { "signal": true } } } }';
    return {
        ...subject,
        files: {
            'package.json': `{ "name": "root", "workspaces": ["site", "api", "${folder}"] }\n`,
            'site/package.json': `{ "name": "site", "nx": ${served} }\n`,
            'api/package.json': `{ "name": "api", "nx": ${served} }\n`,
            [`${folder}/package.json`]: `{ "name": "${folder}", "type": "module" }\n`,
            ...Record.mapKeys(files, (file) => `${folder}/${file}`),
            ...shared,
        },
    };
};

const reported = (settings: string) => `import { defineConfig } from '@playwright/test';\n\nexport default defineConfig({ ${settings} });\n`;

const reused = ({ projects = [], target }: typeof Waited.Type) => Array.ensure(projects).map((project) => `${project}:${target}`);

const atomized = (blob = '') =>
    inferred({
        ...only('atoms', {
            'playwright.config.ts': reported(`testDir: 'tests', testIgnore: [/.*skip.*/, '**/ignored/**'], reporter: [['html', { outputFolder: 'test-results/html' }], ['junit', { outputFile: 'test-results/report.xml' }]${blob}]`),
            'tests/run-me.spec.ts': SPEC,
            'tests/run-me-2.spec.ts': SPEC,
            'tests/skip-me.spec.ts': SPEC,
            'tests/ignored/run-me.spec.ts': SPEC,
            'not-tests/run-me.spec.ts': SPEC,
        }),
        options: { ciTargetName: 'e2e-ci' },
    });

const distributed = (graph: ProjectGraph) => {
    const targets = Object.values(graph.nodes)
        .flatMap((node) => (node.name === 'atoms' ? Record.toEntries(node.data.targets ?? {}) : []))
        .filter(([name]) => name === 'e2e-ci' || name.startsWith('e2e-ci--'));
    return {
        names: targets.map(([name]) => name).toSorted(Order.String),
        parents: targets.filter(([name]) => name === 'e2e-ci').map(([, { cache, executor, dependsOn = [] }]) => ({ cache, executor, after: dependsOn.map((entry) => (Predicate.isString(entry) ? entry : entry.target)).toSorted(Order.String) })),
        atoms: running('playwright test')(graph)
            .filter(({ target, command, cache }) => ATOMS.includes(target) && cache && command.includes(` ${target.replace('e2e-ci--', '')}`))
            .map(({ target }) => target),
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('runs playwright test for a config whose webServer states no command', () =>
        inferred({ ...subject, files: { 'package.json': '{ "name": "root", "workspaces": ["bare"] }\n', 'bare/package.json': '{ "name": "bare", "type": "module" }\n', 'bare/playwright.config.ts': config("webServer: { url: 'http://localhost:4175', reuseExistingServer: true }"), 'bare/site.spec.ts': SPEC } }).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.map(({ project }) => project)),
            Effect.map((tested) => assert.deepStrictEqual(tested, ['bare'], 'nrwl/nx@2a424d3 packages/playwright/src/plugins/plugin.spec.ts "should not crash on a webServer without a command"')),
        ),
    );

    it.effect.each(REUSED)('waits for the serve target of site for the reused webServer command $command', ({ command, source }) =>
        inferred({ ...subject, files: { ...subject.files, 'reused/playwright.config.ts': config(`webServer: { command: '${command}', url: 'http://localhost:4173', reuseExistingServer: true }`) } }).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.filter(({ project }) => project === 'reused')),
            Effect.map(Array.flatMap(({ waits }) => waits)),
            Effect.map((waits) => assert.deepInclude(waits, { projects: ['site'], target: 'serve', waitFor: 'ready' }, `nrwl/nx@2a424d3 packages/playwright/src/plugins/plugin.spec.ts ${source}`)),
        ),
    );

    it.effect('runs playwright test cached, writing its results and HTML report folders, and waits for the Nx task a reused webServer command runs, at the top level and per Playwright project, and for no server Playwright owns', () =>
        inferred(subject).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.map(({ project, cache, outputs, waits }) => ({ project, cache, outputs: outputs.toSorted(), waits }))),
            Effect.map((tested) =>
                assert.deepStrictEqual(tested, [
                    { project: 'owned', cache: true, outputs: ['owned/playwright-report', 'owned/test-results'], waits: [...COMPOSED] },
                    { project: 'reused', cache: true, outputs: ['reused/playwright-report', 'reused/test-results'], waits: [{ projects: ['site'], target: 'serve', waitFor: 'ready' }, ...COMPOSED] },
                    { project: 'split', cache: true, outputs: ['split/playwright-report', 'split/test-results'], waits: [{ projects: ['site'], target: 'preview', waitFor: 'ready' }, ...COMPOSED] },
                ]),
            ),
        ),
    );

    it.effect('writes the output file or folder of each reporter the config lists, beside its results folder', () =>
        inferred(only('listed', { 'playwright.config.ts': reported("testDir: '.', reporter: [['list'], ['json', { outputFile: 'artifacts/report.json' }], ['html', { outputFolder: 'artifacts/html', open: 'never' }]]"), 'site.spec.ts': SPEC })).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.flatMap(({ outputs }) => outputs.toSorted(Order.String))),
            Effect.map((outputs) => assert.deepStrictEqual(outputs, ['listed/artifacts/html', 'listed/artifacts/report.json', 'listed/test-results'], `${PLUGIN} "should create nodes with reporters configured"`)),
        ),
    );

    it.effect.each(SERVERS)('waits for $served alone for $label', ({ servers, served, source }) =>
        inferred(only('many', { 'playwright.config.ts': config(`webServer: [${servers}]`), 'site.spec.ts': SPEC })).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.flatMap(({ waits }) => waits.filter(Schema.is(Waited)).flatMap(reused))),
            Effect.map((waits) => assert.deepStrictEqual(waits.toSorted(Order.String), [...served], `${PLUGIN} ${source}`)),
        ),
    );

    it.effect('reads each tsconfig outside its folder that the project tsconfig extends', () =>
        inferred(only('typed', { 'playwright.config.ts': config(''), 'site.spec.ts': SPEC, 'tsconfig.json': '{ "extends": "../tsconfig.shared.json" }\n' }, { 'tsconfig.shared.json': '{ "compilerOptions": { "strict": true } }\n' })).pipe(
            Effect.map(running('playwright test')),
            Effect.map(Array.flatMap(({ read }) => read)),
            Effect.map((read) => assert.include(read, 'tsconfig.shared.json', `${PLUGIN} "should add tsconfig files from the project tsconfig extends chain that live outside the project root"`)),
        ),
    );

    it.effect('gives each test file the config matches one cached target under ciTargetName that runs that file alone, and a cached nx:noop parent after every one', () =>
        Effect.map(atomized(), (graph) => assert.deepStrictEqual(distributed(graph), { names: ['e2e-ci', ...ATOMS], parents: [{ cache: true, executor: 'nx:noop', after: ATOMS }], atoms: ATOMS }, `${PLUGIN} "should create nodes for distributed CI"`)),
    );

    it.effect('adds a merge-reports target after the per-file targets where the config lists a blob reporter', () =>
        Effect.map(atomized(", ['blob', { outputFile: 'blob-report/blob.zip' }]"), (graph) => assert.deepStrictEqual(distributed(graph).names, ['e2e-ci', 'e2e-ci--merge-reports', ...ATOMS], `${PLUGIN} "should create nodes for distributed CI and merge reports"`)),
    );
});
