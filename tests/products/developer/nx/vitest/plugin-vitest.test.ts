import { globSync, symlinkSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { basename, dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';
import { type CreateNodesContext, type ExecutorContext, runExecutor } from '@nx/devkit';
import { getOutputs, resolveCommandSyntacticSugar } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-vitest';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';
import manifest from 'vitest/package.json' with { type: 'json' };

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('temp', async ({}, { onCleanup }) => {
        const temp = new TempFs('vitest-plugin');
        const previous = process.cwd();
        process.chdir(temp.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            temp.cleanup();
        });
        temp.createDirSync('node_modules');
        ['vitest', 'vite'].forEach((name) => symlinkSync(dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`))), join(temp.tempDir, 'node_modules', name), 'dir'));
        await temp.createFiles({ 'nx.json': '{}', 'package.json': '{"name":"repo","nx":{},"workspaces":["libs/*","packages/*"]}', 'package-lock.json': '{}' });
        return temp;
    })
    .extend('context', ({ temp }): CreateNodesContext => ({
        nxJsonConfiguration: { targetDefaults: {}, namedInputs: { default: ['{projectRoot}/**/*'], production: ['!{projectRoot}/**/*.spec.ts'] } },
        workspaceRoot: temp.tempDir,
    }))
    .extend(
        'listed',
        ({ temp }) =>
            () =>
                globSync('**', { cwd: temp.tempDir, withFileTypes: true, exclude: ['node_modules/{vite,vitest,@vitest}/**'] })
                    .filter((entry) => entry.isFile())
                    .map((entry) => relative(temp.tempDir, join(entry.parentPath, entry.name))),
    );

describe('@rasm/nx-vitest', () => {
    const [, createNodesFunction] = createNodes;

    describe('root project', () => {
        it('should create nodes, running vitest run since watch mode never exits under an Nx TTY, and declaring the lcov.info the coverage configuration writes', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);

            expect(nodes).toMatchObject([
                [
                    'vitest.config.ts',
                    {
                        projects: {
                            '.': {
                                metadata: {},
                                root: '.',
                                targets: {
                                    test: { dependsOn: ['test--*'] },
                                    'test--vitest': {
                                        cache: true,
                                        command: 'vitest run',
                                        inputs: ['default', '^production', { externalDependencies: ['vitest'] }, { env: 'CI' }, { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true }],
                                        metadata: {
                                            description: 'Runs every Vitest project of the suite with vitest run',
                                            help: { command: 'npx vitest run --help', example: { args: ['--project', '<name>'] } },
                                            technologies: ['vitest'],
                                        },
                                        options: { cwd: '.' },
                                        outputs: ['{projectRoot}/.vitest', '{projectRoot}/coverage/lcov.info'],
                                        configurations: { coverage: { 'coverage.enabled': true, 'coverage.reporter': 'lcovonly' } },
                                    },
                                },
                            },
                        },
                    },
                ],
            ]);
        });

        it('should not set projectType so it does not override the type from other plugins', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['.']?.projectType).toBeUndefined();
        });

        it('should create nodes with outputPath and coveragePath', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['vitest.config.ts'], { outputPath: '.artifacts/vitest/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' }, context);

            expect(nodes[0]?.[1].projects?.['.']?.targets?.['test--vitest']).toMatchObject({
                command: "vitest run {args} --attachmentsDir '.artifacts/vitest/{projectName}/attachments'",
                outputs: ['{projectRoot}/.artifacts/vitest/{projectName}', '{projectRoot}/.vitest', '{projectRoot}/.artifacts/coverage/{projectName}/lcov.info'],
                configurations: {
                    coverage: {
                        'coverage.enabled': true,
                        'coverage.reporter': 'lcovonly',
                        'coverage.reportsDirectory': '.artifacts/coverage/{projectName}',
                    },
                },
            });
        });
    });

    describe('typecheck enabled', () => {
        it('should include d.ts in dependentTasksOutputFiles when typecheck is enabled, through the ts extension of every runtime module glob', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default { test: { typecheck: { enabled: true } } };\n' });

            const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['.']?.targets?.['test']?.inputs).toContainEqual({ dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true });
        });

        it('should include every runtime module extension in dependentTasksOutputFiles when typecheck is not enabled, since tests import ts source packages export and codegen writes', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['.']?.targets?.['test']?.inputs).toContainEqual({ dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true });
        });
    });

    describe('workspace config with projects', () => {
        it('should NOT create a project for root config with projects array, naming the config Vitest 5.1 refuses when no project matches', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n" });

            await expect(createNodesFunction(['vitest.config.ts'], {}, context)).rejects.toHaveProperty(['errors', 0, 0], 'vitest.config.ts');
        });

        it('should NOT create a project for root config with empty projects array, naming the config Vitest 5.1 refuses', async ({ temp, context }) => {
            await temp.createFiles({ 'vitest.config.ts': 'export default { test: { projects: [] } };\n' });

            await expect(createNodesFunction(['vitest.config.ts'], {}, context)).rejects.toHaveProperty(['errors', 0, 0], 'vitest.config.ts');
        });

        it('should give each project a root config lists a test target at the package holding it', async ({ temp, context }) => {
            await temp.createFiles({
                'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n",
                'packages/a/package.json': '{"name":"a"}',
                'packages/b/package.json': '{"name":"b"}',
            });

            const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects).not.toHaveProperty(['.']);
            expect(nodes[0]?.[1].projects?.['packages/a']?.targets?.['test--vitest']?.command).toBe('vitest run --project a');
            expect(nodes[0]?.[1].projects?.['packages/a']?.targets?.['test--vitest']?.inputs).toContainEqual('{workspaceRoot}/vitest.config.ts');
            expect(nodes[0]?.[1].projects?.['packages/a']?.targets?.['test--vitest']?.outputs).toContain('{workspaceRoot}/coverage/lcov.info');
        });
    });

    describe('project roots', () => {
        it('should create a project for a config beside a project.json', async ({ temp, context }) => {
            await temp.createFiles({ 'libs/lib1/project.json': '{}', 'libs/lib1/vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets).toHaveProperty(['test--vitest']);
        });

        it('should create no project for a config beside a package.json the workspaces do not include', async ({ temp, context }) => {
            await temp.createFiles({ 'outside/lib1/package.json': '{"name":"lib1"}', 'outside/lib1/vitest.config.ts': 'export default {};\n' });

            const nodes = await createNodesFunction(['outside/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects).toStrictEqual({});
        });
    });

    describe('vite config', () => {
        it('should create a test target for a vite config that defines test, and none for one that does not', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/viteful/package.json': '{"name":"viteful"}',
                'libs/viteful/vite.config.ts': 'export default { test: {} };\n',
                'libs/plain/package.json': '{"name":"plain"}',
                'libs/plain/vite.config.ts': 'export default {};\n',
            });

            const nodes = await createNodesFunction(['libs/viteful/vite.config.ts', 'libs/plain/vite.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/viteful']?.targets).toHaveProperty(['test--vitest']);
            expect(nodes[1]).toStrictEqual(['libs/plain/vite.config.ts', { projects: {} }]);
        });
    });

    describe('browser mode', () => {
        it('should leave parallelism unset for a browser suite, since Nx starts a non-parallel task only when no task runs and reschedules nothing when a continuous task exits', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/ui/package.json': '{"name":"ui"}',
                'libs/ui/vitest.config.ts': "import { playwright } from '@vitest/browser-playwright';\n\nexport default { test: { browser: { enabled: true, provider: playwright(), instances: [{ browser: 'chromium' }], screenshotDirectory: 'shots' } } };\n",
            });
            temp.createDirSync('node_modules/@vitest');
            symlinkSync(dirname(`${findPackageJSON(import.meta.resolve('@vitest/browser-playwright'))}`), join(temp.tempDir, 'node_modules/@vitest/browser-playwright'), 'dir');

            const nodes = await createNodesFunction(['libs/ui/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/ui']?.targets?.['test--vitest']).not.toHaveProperty('parallelism');
            expect(nodes[0]?.[1].projects?.['libs/ui']?.targets?.['test--vitest']?.outputs).toContain('{projectRoot}/shots');
        });
    });

    describe('config imports', () => {
        it('should name each installed package a config imports in externalDependencies', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/vitest.config.ts': "import { mergeConfig } from 'vite';\n\nexport default mergeConfig({ test: {} }, {});\n",
            });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test--vitest']?.inputs).toContainEqual({ externalDependencies: ['vitest', 'vite'] });
        });

        it('should give a workspace package a config imports as a project input, not an external dependency', async ({ temp, context }) => {
            await temp.createFiles({
                'libs/helper/package.json': '{"name":"helper","type":"module","exports":"./index.js"}',
                'libs/helper/index.js': "export const include = ['*.test.ts'];\n",
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/vitest.config.ts': "import { include } from 'helper';\n\nexport default { test: { include } };\n",
            });
            temp.createSymlinkSync('libs/helper', 'node_modules/helper', 'dir');

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test--vitest']?.inputs).toContainEqual({ input: 'default', projects: ['helper'] });
            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test--vitest']?.inputs).toContainEqual({ externalDependencies: ['vitest'] });
        });

        it('should declare a file a config names in forceRerunTriggers as an input of its test target alone', async ({ temp, context }) => {
            await temp.createFiles({
                'shared/schema.json': '{}',
                'libs/lib1/package.json': '{"name":"lib1"}',
                'libs/lib1/vitest.config.ts': "export default { test: { forceRerunTriggers: ['**/shared/schema.json'] } };\n",
                'libs/lib2/package.json': '{"name":"lib2"}',
                'libs/lib2/vitest.config.ts': 'export default {};\n',
            });

            const nodes = await createNodesFunction(['libs/lib1/vitest.config.ts', 'libs/lib2/vitest.config.ts'], {}, context);

            expect(nodes[0]?.[1].projects?.['libs/lib1']?.targets?.['test--vitest']?.inputs).toContainEqual('{workspaceRoot}/**/shared/schema.json');
            expect(nodes[1]?.[1].projects?.['libs/lib2']?.targets?.['test--vitest']?.inputs).not.toContainEqual('{workspaceRoot}/**/shared/schema.json');
        });
    });
});

describe('@rasm/nx-vitest real run', () => {
    const [, createNodesFunction] = createNodes;

    it('should write every file a run writes under the declared outputs, and the lcov.info the coverage configuration declares', async ({ temp, context, listed }) => {
        await temp.createFiles({
            'package.json': '{"name":"repo","type":"module","nx":{}}',
            'vitest.config.ts': 'export default {};\n',
            'sum.test.ts': "import { expect, test } from 'vitest';\n\ntest('adds', () => expect(1 + 1).toBe(2));\n",
        });
        temp.createDirSync('node_modules/@vitest');
        temp.createDirSync('node_modules/.bin');
        symlinkSync(dirname(`${findPackageJSON(import.meta.resolve('@vitest/coverage-v8'))}`), join(temp.tempDir, 'node_modules/@vitest/coverage-v8'), 'dir');
        symlinkSync(join(temp.tempDir, 'node_modules/vitest', manifest.bin.vitest), join(temp.tempDir, 'node_modules/.bin/vitest'));
        symlinkSync(dirname(fileURLToPath(import.meta.resolve('nx/package.json'))), join(temp.tempDir, 'node_modules/nx'), 'dir');

        const nodes = await createNodesFunction(['vitest.config.ts'], {}, context);
        const before = new Set(listed());
        const node = { name: 'repo', type: 'lib' as const, data: { root: '.', targets: Record.map({ ...nodes[0]?.[1].projects?.['.']?.targets }, resolveCommandSyntacticSugar) } };
        const executor: ExecutorContext = {
            root: temp.tempDir,
            cwd: temp.tempDir,
            isVerbose: false,
            projectName: 'repo',
            targetName: 'test--vitest',
            projectsConfigurations: { version: 2, projects: { repo: node.data } },
            projectGraph: { nodes: { repo: node }, dependencies: {} },
            nxJsonConfiguration: {},
        };
        const plain = await Array.fromAsync(await runExecutor({ project: 'repo', target: 'test--vitest' }, {}, executor));
        const ran = listed().filter((file) => !before.has(file));
        const seen = new Set([...before, ...ran]);
        const covered = await Array.fromAsync(await runExecutor({ project: 'repo', target: 'test--vitest', configuration: 'coverage' }, {}, { ...executor, configurationName: 'coverage' }));
        const written = listed().filter((file) => !seen.has(file));
        const caches = [...ran, ...written].filter((file) => basename(file) === 'CACHEDIR.TAG').map(dirname);
        const [declared, reported] = [getOutputs({ repo: node }, { project: 'repo', target: 'test--vitest' }, {}), getOutputs({ repo: node }, { project: 'repo', target: 'test--vitest', configuration: 'coverage' }, {})];
        const [plainUnder, coveredUnder] = [declared, reported].map((outputs) => new RegExp(`^(?:${[...outputs, ...caches].map(RegExp.escape).join('|')})(?:/|$)`, 'u'));

        expect([...plain, ...covered]).toMatchObject([{ success: true }, { success: true }]);
        expect(ran.filter((file) => plainUnder?.test(file) !== true)).toStrictEqual([]);
        expect(written.filter((file) => coveredUnder?.test(file) !== true)).toStrictEqual([]);
        expect(reported).toContain('coverage/lcov.info');
        expect(written).toContain('coverage/lcov.info');
    });
});
