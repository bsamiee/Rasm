import { execSync } from 'node:child_process';
import { globSync, mkdirSync, symlinkSync } from 'node:fs';
import { delimiter, dirname, join } from 'node:path';
import { chdir, cwd, env } from 'node:process';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-playwright';
import { TempFs } from '@rasm/testing/temp-fs';
import { updateFilesInContext } from 'nx/src/utils/workspace-context';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const [, createNodesFunction] = createNodes;
const playwright = dirname(fileURLToPath(import.meta.resolve('@playwright/test/package.json')));
const internal = JSON.stringify(fileURLToPath(import.meta.resolve('@nx/devkit/internal')));
const written = ['NO_PROXY', 'HTTP_PROXY', 'PROXY_HOST', 'SERVE_TARGET', 'BOOT_FLAG', 'TOOL_HOME', 'ITERM_PROFILE'];
const codegen = ['codegen--*', '^codegen'];
const served = { projects: ['app1'], target: 'serve', waitFor: 'ready' };
const inputs = ['default', '^production', '^{projectRoot}/tsconfig*.json', { dependentTasksOutputFiles: '**/*', transitive: true }, { env: 'CI' }, { externalDependencies: ['@playwright/test'] }];
const metadata = {
    technologies: ['playwright'],
    description: 'Runs every Playwright project of the config with playwright test after the Nx task each reused webServer row runs is ready, beside other tasks only where an Nx task starts every reused web server',
    help: { command: 'npx playwright test --help', example: { args: ['--project', '<name>'] } },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluations = (): number => Number(Reflect.get(globalThis, 'evaluations') ?? 0);

const mockPlaywrightConfig = (tempFs: TempFs, config: string) => tempFs.writeFile('playwright.config.js', config);

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('ambient', { auto: true }, ({}, { onCleanup }) => {
        const ambient = { ...env };
        written.forEach((name) => {
            Reflect.deleteProperty(env, name);
        });
        onCleanup(() => {
            Object.keys(env)
                .filter((name) => !(name in ambient))
                .forEach((name) => {
                    Reflect.deleteProperty(env, name);
                });
            Object.assign(env, ambient);
        });
        return ambient;
    })
    .extend('tempFs', async ({}, { onCleanup }) => {
        const tempFs = new TempFs('playwright-plugin');
        const previous = cwd();
        chdir(tempFs.tempDir);
        onCleanup(() => {
            chdir(previous);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'proj', workspaces: ['apps/*'], nx: {} }),
            'package-lock.json': '{}',
            'playwright.config.js': 'module.exports = {}',
        });
        mkdirSync(join(tempFs.tempDir, 'node_modules/@playwright'), { recursive: true });
        symlinkSync(playwright, join(tempFs.tempDir, 'node_modules/@playwright/test'), 'dir');
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
                production: ['!{projectRoot}/**/*.spec.ts'],
            },
        },
        workspaceRoot: tempFs.tempDir,
    }))
    .extend('warn', ({}, { onCleanup }) => {
        const warn = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        onCleanup(() => warn.mockRestore());
        return warn;
    });

describe('@rasm/nx-playwright', () => {
    it('should create nodes with default playwright configuration', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, 'module.exports = {}');
        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results).toStrictEqual([
            [
                'playwright.config.js',
                {
                    projects: {
                        '.': {
                            root: '.',
                            metadata: { technologies: ['playwright'] },
                            targets: {
                                test: { dependsOn: ['test--*'], inputs: ['...', ...inputs] },
                                'test--playwright': {
                                    command: 'playwright test',
                                    options: { cwd: '.' },
                                    dependsOn: codegen,
                                    cache: true,
                                    parallelism: false,
                                    inputs,
                                    outputs: ['{projectRoot}/test-results'],
                                    metadata,
                                },
                                clean: { dependsOn: ['clean--*'], inputs: ['...', 'default', '^default', { externalDependencies: ['@playwright/test'] }] },
                                'clean--playwright': {
                                    executor: '@rasm/nx-distribution:clean',
                                    options: { targets: ['test--playwright'], commands: [['playwright', 'clear-cache']], cwd: '.' },
                                    inputs: ['default', '^default', { externalDependencies: ['@playwright/test'] }],
                                    metadata: {
                                        technologies: ['playwright'],
                                        description: 'Removes the outputs test--playwright declare and the Playwright transform cache with playwright clear-cache',
                                        help: { command: 'npx playwright clear-cache --help', example: { args: ['--config', '<file>'] } },
                                    },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should create nodes with reporters configured', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, `module.exports = ${JSON.stringify({ reporter: [['list'], ['json', { outputFile: join(tempFs.tempDir, 'test-results/report.json') }], ['html', { outputFolder: 'test-results/html' }]] })}`);
        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.outputs).toStrictEqual(['{projectRoot}/test-results', '{projectRoot}/test-results/report.json', '{projectRoot}/test-results/html']);
    });

    it('should write test-results and the file of each built-in reporter under outputPath', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { reporter: [['html'], ['junit'], ['list']] }");
        const results = await createNodesFunction(['playwright.config.js'], { outputPath: '.artifacts/playwright/{projectName}' }, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.command).toBe("playwright test {args} '--output=.artifacts/playwright/{projectName}/test-results'");
        expect(target?.options).toStrictEqual({
            cwd: '.',
            env: {
                PLAYWRIGHT_BLOB_OUTPUT_DIR: '.artifacts/playwright/{projectName}/blob-report',
                PLAYWRIGHT_HTML_OUTPUT_DIR: '.artifacts/playwright/{projectName}/playwright-report',
                PLAYWRIGHT_JSON_OUTPUT_FILE: '.artifacts/playwright/{projectName}/report.json',
                PLAYWRIGHT_JUNIT_OUTPUT_FILE: '.artifacts/playwright/{projectName}/junit.xml',
                PLAYWRIGHT_PERFETTO_OUTPUT_FILE: '.artifacts/playwright/{projectName}/perfetto.json',
            },
        });
        expect(target?.outputs).toStrictEqual(['{projectRoot}/.artifacts/playwright/{projectName}']);
    });

    it('should spell an output outside the project root from the workspace root', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'apps/e2e/package.json': '{}', 'apps/e2e/playwright.config.js': "module.exports = { reporter: [['html', { outputFolder: '../../reports/e2e' }]] }" });
        const results = await createNodesFunction(['apps/e2e/playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['apps/e2e']?.targets?.['test--playwright']?.outputs).toStrictEqual(['{projectRoot}/test-results', '{workspaceRoot}/reports/e2e']);
    });

    it('should write every declared output of a playwright test run and no file outside them', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'run-me.spec.ts': "import { test } from '@playwright/test';\n\ntest('passes', () => {});\n" });
        mockPlaywrightConfig(tempFs, "module.exports = { reporter: [['html', { open: 'never' }], ['junit']] }");
        mkdirSync(join(tempFs.tempDir, 'node_modules/.bin'));
        symlinkSync(join(playwright, 'cli.js'), join(tempFs.tempDir, 'node_modules/.bin/playwright'));
        const results = await createNodesFunction(['playwright.config.js'], { outputPath: 'dist/e2e' }, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];
        const routed = {
            PLAYWRIGHT_BLOB_OUTPUT_DIR: 'dist/e2e/blob-report',
            PLAYWRIGHT_HTML_OUTPUT_DIR: 'dist/e2e/playwright-report',
            PLAYWRIGHT_JSON_OUTPUT_FILE: 'dist/e2e/report.json',
            PLAYWRIGHT_JUNIT_OUTPUT_FILE: 'dist/e2e/junit.xml',
            PLAYWRIGHT_PERFETTO_OUTPUT_FILE: 'dist/e2e/perfetto.json',
        };
        const outside = () => globSync('**', { cwd: tempFs.tempDir, exclude: ['node_modules', 'node_modules/**', 'tmp', 'tmp/**', 'dist', 'dist/e2e', 'dist/e2e/**'] });
        const before = outside();

        expect([target?.command, target?.outputs]).toStrictEqual(['playwright test {args} --output=dist/e2e/test-results', ['{projectRoot}/dist/e2e']]);
        expect(target?.options).toStrictEqual({ cwd: '.', env: routed });

        execSync('playwright test --output=dist/e2e/test-results', { cwd: tempFs.tempDir, env: { ...env, ...routed, PATH: [join(tempFs.tempDir, 'node_modules/.bin'), env['PATH']].join(delimiter) }, stdio: 'pipe' });

        expect(outside()).toStrictEqual(before);
        expect(globSync(['dist/e2e/junit.xml', 'dist/e2e/playwright-report/index.html', 'dist/e2e/test-results'], { cwd: tempFs.tempDir })).toHaveLength(3);
    });

    it('should create nodes only where a package.json is in the package manager workspaces or beside a project.json', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/listed/package.json': '{}',
            'apps/listed/playwright.config.js': 'module.exports = {}',
            'tools/marked/package.json': '{}',
            'tools/marked/project.json': '{}',
            'tools/marked/playwright.config.js': 'module.exports = {}',
            'tools/unlisted/package.json': '{}',
            'tools/unlisted/playwright.config.js': 'module.exports = {}',
            'tools/bare/project.json': '{}',
            'tools/bare/playwright.config.js': 'module.exports = {}',
        });

        const results = await createNodesFunction(['apps/listed/playwright.config.js', 'tools/marked/playwright.config.js', 'tools/unlisted/playwright.config.js', 'tools/bare/playwright.config.js'], {}, context);

        expect(results.map(([file]) => file)).toStrictEqual(['apps/listed/playwright.config.js', 'tools/marked/playwright.config.js', 'tools/bare/playwright.config.js']);
    });

    it('should hash each package the config imports and read workspace packages as project inputs', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'node_modules/dep/package.json': JSON.stringify({ name: 'dep', main: 'index.js' }),
            'node_modules/dep/index.js': 'module.exports = {};',
            'libs/shared/package.json': JSON.stringify({ name: 'shared', main: 'index.js' }),
            'libs/shared/index.js': 'module.exports = {};',
        });
        tempFs.createSymlinkSync('libs/shared', 'node_modules/shared', 'dir');
        mockPlaywrightConfig(tempFs, "require('dep'); require('shared'); module.exports = {};");

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const imported = results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.inputs;

        expect(imported).toContainEqual({ externalDependencies: ['@playwright/test', 'dep'] });
        expect(imported).toContainEqual({ input: 'default', projects: ['shared'] });
    });

    it('should name a config that does not load in the failure', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "throw new Error('config boom');");
        const read = createNodesFunction(['playwright.config.js'], {}, context);

        await expect(read).rejects.toHaveProperty(['errors', 0, 0], 'playwright.config.js');
        await expect(read).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringContaining('playwright.config.js does not load as a Playwright config'));
    });

    it('should infer dependsOn using the task run in the webServer.command and not set parallelism to false', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx run app1:serve', reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '', 'tests/run-me-2.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']).toStrictEqual({
            command: 'playwright test',
            options: { cwd: '.' },
            dependsOn: [...codegen, served],
            cache: true,
            inputs,
            outputs: ['{projectRoot}/test-results'],
            metadata,
        });
    });

    it('should keep the dependency of a webServer command with a trailing configuration', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx run app1:serve:production', port: 4200, reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, served]);
    });

    it.for(['bunx nx run app1:serve', 'bunx nx serve app1', 'bun x nx run app1:serve', 'bun x nx serve app1', 'pnpm exec nx run app1:serve'])('should infer the serve dependency from a package-manager-prefixed webServer command (%s)', async (command, { tempFs, context, warn }) => {
        mockPlaywrightConfig(tempFs, `module.exports = { testDir: 'tests', webServer: { command: '${command}', reuseExistingServer: true } }`);
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(warn).not.toHaveBeenCalled();
        expect(target?.dependsOn).toStrictEqual([...codegen, served]);
        expect(target?.parallelism).toBeUndefined();
    });

    it('warns when a reuseExistingServer webServer command cannot be inferred', async ({ tempFs, context, warn }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx run app1:serve --port=4200', port: 4200, reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual(codegen);
        expect(target?.parallelism).toBe(false);
        expect(warn).toHaveBeenCalledExactlyOnceWith(expect.stringContaining('npx nx run app1:serve --port=4200'));
    });

    it('warns for a versioned nx invocation that cannot be inferred', async ({ tempFs, context, warn }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx@latest run app1:serve --port=4200', port: 4200, reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        await createNodesFunction(['playwright.config.js'], {}, context);

        expect(warn).toHaveBeenCalledExactlyOnceWith(expect.stringContaining('npx nx@latest run app1:serve --port=4200'));
    });

    it('does not warn for a webServer command that is not an Nx invocation', async ({ tempFs, context, warn }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npm run start', port: 4200, reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(warn).not.toHaveBeenCalled();
        expect(target?.dependsOn).toStrictEqual(codegen);
        expect(target?.parallelism).toBe(false);
    });

    it('should not crash on a webServer without a command', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { port: 4200, reuseExistingServer: true } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual(codegen);
        expect(target?.parallelism).toBe(false);
    });

    it('should not depend on a webServer that waits for command output', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: [{ command: 'npx nx run app1:serve', port: 4200, reuseExistingServer: true }, { command: 'npx nx run worker1:serve', port: 5000, reuseExistingServer: true, wait: { stdout: /ready/ } }] }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual([...codegen, served]);
        expect(target?.parallelism).toBe(false);
    });

    it('should leave a config whose only webServer waits for command output entirely to Playwright', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx run app1:serve', port: 4200, reuseExistingServer: true, wait: { stdout: /ready/ } } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual(codegen);
        expect(target?.parallelism).toBe(false);
    });

    it('should not depend on a webServer that sets env, leaving its launch to Playwright', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: [{ command: 'npx nx run app1:serve', port: 4200, reuseExistingServer: true }, { command: 'npx nx run api1:serve', url: 'http://localhost:4300', reuseExistingServer: true, env: { PORT: '4300' } }] }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual([...codegen, served]);
        expect(target?.parallelism).toBe(false);
    });

    it('should still depend on a webServer whose env is empty', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { testDir: 'tests', webServer: { command: 'npx nx run app1:serve', port: 4200, reuseExistingServer: true, env: {} } }");
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);
        const target = results[0]?.[1].projects?.['.']?.targets?.['test--playwright'];

        expect(target?.dependsOn).toStrictEqual([...codegen, served]);
        expect(target?.parallelism).toBeUndefined();
    });

    it('should not set parallelism to false and should infer dependsOn using the tasks run in the different webServer.command that have reuseExistingServer set to true', async ({ tempFs, context }) => {
        mockPlaywrightConfig(
            tempFs,
            "module.exports = { testDir: 'tests', webServer: [{ command: 'npx nx run app1:serve', reuseExistingServer: true }, { command: 'npx nx run api1:serve', reuseExistingServer: true }, { command: 'npx nx run api2:dev', reuseExistingServer: true }, { command: 'npx nx run api3:serve', reuseExistingServer: false }] }",
        );
        await tempFs.createFiles({ 'tests/run-me.spec.ts': '', 'tests/run-me-2.spec.ts': '' });

        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']).toStrictEqual({
            command: 'playwright test',
            options: { cwd: '.' },
            dependsOn: [...codegen, { projects: ['app1', 'api1'], target: 'serve', waitFor: 'ready' }, { projects: ['api2'], target: 'dev', waitFor: 'ready' }],
            cache: true,
            inputs,
            outputs: ['{projectRoot}/test-results'],
            metadata,
        });
    });

    it('rebuilds cached targets when an allowed ambient env var changes', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/e2e/package.json': '{}',
            'apps/e2e/playwright.config.js': `// ${tempFs.tempDir}\nglobalThis.evaluations = (globalThis.evaluations ?? 0) + 1;\nmodule.exports = {};`,
        });
        const run = () => createNodesFunction(['apps/e2e/playwright.config.js'], {}, context);

        await run();
        const rebuildsAfterFirst = evaluations();
        await run();

        expect(evaluations()).toBe(rebuildsAfterFirst);

        env['TOOL_HOME'] = '/usr/lib/tool';
        await run();

        expect(evaluations()).toBeGreaterThan(rebuildsAfterFirst);
    });

    it('keeps cached targets when an excluded ambient env var changes', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/e2e/package.json': '{}',
            'apps/e2e/playwright.config.js': `// ${tempFs.tempDir}\nglobalThis.evaluations = (globalThis.evaluations ?? 0) + 1;\nmodule.exports = {};`,
        });
        const run = () => createNodesFunction(['apps/e2e/playwright.config.js'], {}, context);

        await run();
        const rebuildsAfterFirst = evaluations();
        env['ITERM_PROFILE'] = 'Default';
        await run();

        expect(evaluations()).toBe(rebuildsAfterFirst);
    });

    it('persists a pass whose config wrote to the env while loading', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/e2e/package.json': '{}',
            'apps/e2e/playwright.config.js': `// ${tempFs.tempDir}\nglobalThis.evaluations = (globalThis.evaluations ?? 0) + 1;\nprocess.env.BOOT_FLAG = 'set';\nmodule.exports = {};`,
        });
        const run = () => createNodesFunction(['apps/e2e/playwright.config.js'], {}, context);

        await run();

        expect(env['BOOT_FLAG']).toBeUndefined();

        const rebuildsAfterFirst = evaluations();
        await run();

        expect(evaluations()).toBe(rebuildsAfterFirst);
    });

    it('does not persist a pass during which the daemon swapped the env away and back', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'apps/e2e/package.json': '{}',
            'apps/e2e/playwright.config.js': `// ${tempFs.tempDir}
globalThis.evaluations = (globalThis.evaluations ?? 0) + 1;
const { applyDaemonEnvFromClient } = require(${internal});
const ambient = { ...process.env };
applyDaemonEnvFromClient({ ...ambient, PW_INTERIM: 'interim' });
applyDaemonEnvFromClient(ambient);
module.exports = {};`,
        });
        const run = () => createNodesFunction(['apps/e2e/playwright.config.js'], {}, context);

        await run();
        const rebuildsAfterFirst = evaluations();
        await run();

        expect(evaluations()).toBeGreaterThan(rebuildsAfterFirst);
    });

    it('loads an edited config again in the same process', async ({ tempFs, context }) => {
        mockPlaywrightConfig(tempFs, "module.exports = { webServer: { command: 'nx run app1:serve', reuseExistingServer: true } }");
        await createNodesFunction(['playwright.config.js'], {}, context);
        mockPlaywrightConfig(tempFs, "module.exports = { webServer: { command: 'nx run app1:dev', reuseExistingServer: true } }");
        updateFilesInContext(tempFs.tempDir, ['playwright.config.js'], []);

        const results = await createNodesFunction(['playwright.config.js'], {}, context);

        expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, { projects: ['app1'], target: 'dev', waitFor: 'ready' }]);
    });

    describe('normalizeWebServers', () => {
        it('normalizes a single web server and an array the same way', async ({ tempFs, context }) => {
            const server = JSON.stringify({ command: 'nx serve app1', url: 'http://localhost:4300', reuseExistingServer: true, ignoreHTTPSErrors: true, timeout: 1000, cwd: '/tmp/app' });
            await tempFs.createFiles({
                'apps/single/package.json': '{}',
                'apps/single/playwright.config.js': `module.exports = { webServer: ${server} }`,
                'apps/array/package.json': '{}',
                'apps/array/playwright.config.js': `module.exports = { webServer: [${server}] }`,
            });

            const results = await createNodesFunction(['apps/single/playwright.config.js', 'apps/array/playwright.config.js'], {}, context);

            expect(results[0]?.[1].projects?.['apps/single']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, served]);
            expect(results[1]?.[1].projects?.['apps/array']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, served]);
        });

        it.for([
            ['{ stdout: /ready/ }', codegen],
            ['{ stderr: /ready/ }', codegen],
            ['{}', [...codegen, served]],
        ] as const)('sets waitsForOutput for either stdio regex and not for an empty wait (%s)', async ([wait, dependsOn], { tempFs, context }) => {
            mockPlaywrightConfig(tempFs, `module.exports = { webServer: { command: 'nx serve app1', reuseExistingServer: true, wait: ${wait} } };`);

            const results = await createNodesFunction(['playwright.config.js'], {}, context);

            expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.dependsOn).toStrictEqual(dependsOn);
        });
    });

    describe('loadConfigWithProbeEnv', () => {
        it('restores every variable the config wrote, not only the probe ones', async ({ tempFs, context }) => {
            mockPlaywrightConfig(tempFs, "process.env.NO_PROXY = 'localhost';\nprocess.env.PROXY_HOST = 'proxy.example';\nmodule.exports = {};");

            await createNodesFunction(['playwright.config.js'], {}, context);

            expect(env['NO_PROXY']).toBeUndefined();
            expect(env['PROXY_HOST']).toBeUndefined();
        });

        it('consumes the config before restoring, so a getter reading env the load wrote sees it', async ({ tempFs, context }) => {
            mockPlaywrightConfig(tempFs, "process.env.SERVE_TARGET = 'serve-mock';\nmodule.exports = { webServer: { get command() { return 'nx run app1:' + process.env.SERVE_TARGET; }, reuseExistingServer: true } };");

            const results = await createNodesFunction(['playwright.config.js'], {}, context);

            expect(results[0]?.[1].projects?.['.']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, { projects: ['app1'], target: 'serve-mock', waitFor: 'ready' }]);
            expect(env['SERVE_TARGET']).toBeUndefined();
        });

        it("serializes concurrent loads so one config's writes never reach another's", async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'apps/writes/package.json': '{}',
                'apps/writes/playwright.config.js': "process.env.SERVE_TARGET = 'serve-mock';\nmodule.exports = {};",
                'apps/reads/package.json': '{}',
                'apps/reads/playwright.config.js': "module.exports = { webServer: { command: 'nx run app1:' + (process.env.SERVE_TARGET ?? 'serve'), reuseExistingServer: true } };",
            });

            const results = await createNodesFunction(['apps/writes/playwright.config.js', 'apps/reads/playwright.config.js'], {}, context);

            expect(results[1]?.[1].projects?.['apps/reads']?.targets?.['test--playwright']?.dependsOn).toStrictEqual([...codegen, served]);
            expect(env['SERVE_TARGET']).toBeUndefined();
        });

        it('re-applies the client env applied mid-load over the pre-load values', async ({ tempFs, context }) => {
            mockPlaywrightConfig(
                tempFs,
                `require(${internal}).applyDaemonEnvFromClient({ ...process.env, HTTP_PROXY: 'http://client.example:8080' });
process.env.NO_PROXY = 'localhost';
process.env.HTTP_PROXY = 'http://config.example:8080';
module.exports = {};`,
            );

            await createNodesFunction(['playwright.config.js'], {}, context);

            expect(env['NO_PROXY']).toBeUndefined();
            expect(env['HTTP_PROXY']).toBe('http://client.example:8080');
        });

        it('re-applies a mid-load client env whose values the config had already written', async ({ tempFs, context }) => {
            mockPlaywrightConfig(
                tempFs,
                `process.env.NO_PROXY = 'localhost';
require(${internal}).applyDaemonEnvFromClient({ ...process.env });
module.exports = {};`,
            );

            await createNodesFunction(['playwright.config.js'], {}, context);

            expect(env['NO_PROXY']).toBe('localhost');
        });

        it('restores the variables when the load throws', async ({ tempFs, context }) => {
            mockPlaywrightConfig(tempFs, "process.env.NO_PROXY = 'localhost';\nthrow new Error('config boom');");

            const read = createNodesFunction(['playwright.config.js'], {}, context);

            await expect(read).rejects.toHaveProperty(['errors', 0, 1, 'message'], 'playwright.config.js does not load as a Playwright config');
            await expect(read).rejects.toHaveProperty(['errors', 0, 1, 'cause', 'message'], 'config boom');
            expect(env['NO_PROXY']).toBeUndefined();
        });
    });

    describe('tsconfig inputs', () => {
        const fields = ['compilerOptions', 'extends'];

        it('should add tsconfig files from the project tsconfig extends chain that live outside the project root, with the compilerOptions and extends fields the Playwright tsconfig loader reads', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'tsconfig.base.json': JSON.stringify({}),
                'tsconfig.shared.json': JSON.stringify({ extends: './tsconfig.base.json' }),
                'apps/my-app/package.json': '{}',
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.shared.json' }),
                'apps/my-app/playwright.config.js': 'module.exports = {}',
            });

            const results = await createNodesFunction(['apps/my-app/playwright.config.js'], {}, context);
            const read = results[0]?.[1].projects?.['apps/my-app']?.targets?.['test--playwright']?.inputs;

            expect(read).toContainEqual({ json: '{workspaceRoot}/tsconfig.shared.json', fields });
            expect(read).not.toContainEqual({ json: '{workspaceRoot}/tsconfig.base.json', fields });
        });

        it('should not add the workspace root tsconfig.json when tsconfig.base.json exists, since only nxE2EPreset reads it at runtime and this config loads no preset', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'tsconfig.base.json': JSON.stringify({}),
                'tsconfig.json': JSON.stringify({ extends: './tsconfig.base.json', files: [], include: [] }),
                'apps/my-app/package.json': '{}',
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json' }),
                'apps/my-app/playwright.config.js': 'module.exports = {}',
            });

            const results = await createNodesFunction(['apps/my-app/playwright.config.js'], {}, context);
            const read = results[0]?.[1].projects?.['apps/my-app']?.targets?.['test--playwright']?.inputs;

            expect(read).not.toContainEqual({ json: '{workspaceRoot}/tsconfig.json', fields });
            expect(read).not.toContainEqual({ json: '{workspaceRoot}/tsconfig.base.json', fields });
        });

        it('should not add the workspace root tsconfig.json when it is the native hasher file (no tsconfig.base.json)', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'tsconfig.json': JSON.stringify({}),
                'apps/my-app/package.json': '{}',
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.json' }),
                'apps/my-app/playwright.config.js': 'module.exports = {}',
            });

            const results = await createNodesFunction(['apps/my-app/playwright.config.js'], {}, context);

            expect(results[0]?.[1].projects?.['apps/my-app']?.targets?.['test--playwright']?.inputs).not.toContainEqual({ json: '{workspaceRoot}/tsconfig.json', fields });
        });

        it('should not add tsconfig files inside the project root', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'tsconfig.base.json': JSON.stringify({}),
                'apps/my-app/package.json': '{}',
                'apps/my-app/tsconfig.e2e.json': JSON.stringify({ extends: './tsconfig.json' }),
                'apps/my-app/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.base.json' }),
                'apps/my-app/playwright.config.js': 'module.exports = {}',
            });

            const results = await createNodesFunction(['apps/my-app/playwright.config.js'], {}, context);
            const read = results[0]?.[1].projects?.['apps/my-app']?.targets?.['test--playwright']?.inputs;

            expect(read).not.toContainEqual({ json: '{workspaceRoot}/apps/my-app/tsconfig.json', fields });
            expect(read).not.toContainEqual({ json: '{workspaceRoot}/apps/my-app/tsconfig.e2e.json', fields });
        });
    });
});
