import { execFileSync } from 'node:child_process';
import { globSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { pathToFileURL } from 'node:url';
import { AggregateCreateNodesError, type CreateNodesContext } from '@nx/devkit';
import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';
import { createNodes } from '@rasm/nx-pulumi';
import { environment } from '@rasm/testing/nx';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

const [, createNodesFunction] = createNodes;
const program = JSON.stringify({ name: '@proj/infra', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } });
const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('test');
        const cwd = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
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
    .extend('backend', ({}, { onCleanup }) => {
        const backend = new TempFs('pulumi-backend', false);
        onCleanup(() => backend.cleanup());
        return backend;
    });

describe('@rasm/nx-pulumi', () => {
    it('should create nodes based on package.json, leaving cache unset as Nx caches no target without cache: true', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "workspaces": ["proj"] }',
            'proj/package.json': program,
            'proj/stacks.ts': 'export {};\n',
        });

        const results = await createNodesFunction(['proj/package.json'], {}, context);

        const inputs = ['production', '^production', { externalDependencies: [] }];
        const technologies = ['pulumi'];
        expect(results).toStrictEqual([
            [
                'proj/package.json',
                {
                    projects: {
                        proj: {
                            root: 'proj',
                            metadata: { technologies },
                            targets: {
                                install: { dependsOn: ['install--*'], inputs: ['...', ...inputs] },
                                'install--pulumi': {
                                    command: 'node stacks.ts install',
                                    options: { cwd: 'proj' },
                                    dependsOn: ['^build', '^install'],
                                    parallelism: false,
                                    inputs,
                                    metadata: { technologies, description: 'Runs the Pulumi Automation API program of the project with node stacks.ts install' },
                                },
                                deploy: { dependsOn: ['deploy--*'], inputs: ['...', ...inputs] },
                                'deploy--pulumi': {
                                    command: 'node stacks.ts deploy',
                                    options: { cwd: 'proj' },
                                    dependsOn: ['^build', '^deploy'],
                                    parallelism: false,
                                    inputs,
                                    metadata: { technologies, description: 'Runs the Pulumi Automation API program of the project with node stacks.ts deploy' },
                                },
                                'dry-run': { dependsOn: ['dry-run--*'], inputs: ['...', ...inputs] },
                                'dry-run--pulumi': {
                                    command: 'node stacks.ts deploy --operation=preview',
                                    options: { cwd: 'proj' },
                                    dependsOn: [],
                                    parallelism: false,
                                    inputs,
                                    metadata: { technologies, description: 'Runs the Pulumi Automation API program of the project with node stacks.ts deploy --operation=preview' },
                                },
                            },
                        },
                    },
                },
            ],
        ]);
    });

    it('should use default namedInputs when production is not available', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "workspaces": ["proj"] }',
            'proj/package.json': program,
            'proj/stacks.ts': 'export {};\n',
        });

        const results = await createNodesFunction(['proj/package.json'], {}, { ...context, nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } } });

        expect(results[0]?.[1].projects?.['proj']?.targets?.['deploy--pulumi']?.inputs).toStrictEqual(['default', '^default', { externalDependencies: [] }]);
        expect(results[0]?.[1].projects?.['proj']?.targets?.['install--pulumi']?.inputs).toStrictEqual(['default', '^default', { externalDependencies: [] }]);
    });

    it('should handle multiple programs', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "workspaces": ["app1", "app2"] }',
            'app1/package.json': JSON.stringify({ name: 'app1', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'app1/stacks.ts': 'export {};\n',
            'app2/package.json': JSON.stringify({ name: 'app2', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'app2/stacks.ts': 'export {};\n',
        });

        const results = await createNodesFunction(['app1/package.json', 'app2/package.json'], {}, context);

        expect(results).toHaveLength(2);
        expect(results[0]?.[0]).toBe('app1/package.json');
        expect(results[1]?.[0]).toBe('app2/package.json');
        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['app1']);
        expect(Object.keys({ ...results[1]?.[1].projects })).toStrictEqual(['app2']);
    });

    it('should give equal results for equal reads', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "workspaces": ["proj"] }',
            'proj/package.json': program,
            'proj/stacks.ts': 'export {};\n',
        });

        const results1 = await createNodesFunction(['proj/package.json'], {}, context);
        const results2 = await createNodesFunction(['proj/package.json'], {}, context);

        expect(results1).toStrictEqual(results2);
    });

    it('should handle a root level program', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: '@proj/infra', nx: {}, bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'stacks.ts': 'export {};\n',
        });

        const results = await createNodesFunction(['package.json'], {}, context);

        expect(Object.keys({ ...results[0]?.[1].projects })).toStrictEqual(['.']);
        expect(results[0]?.[1].projects?.['.']?.targets?.['deploy--pulumi']).toMatchObject({ command: 'node stacks.ts deploy', options: { cwd: '.' } });
    });

    it('should name the manifest that fails to parse, and keep the targets of every other program', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "workspaces": ["proj", "torn"] }',
            'proj/package.json': program,
            'proj/stacks.ts': 'export {};\n',
            'torn/package.json': '{ "name": ',
        });

        const read = createNodesFunction(['proj/package.json', 'torn/package.json'], {}, context);

        await expect(read).rejects.toBeInstanceOf(AggregateCreateNodesError);
        await expect(read).rejects.toHaveProperty(['errors', 0, 0], 'torn/package.json');
        await expect(read).rejects.toHaveProperty(['errors', 0, 1, 'message'], expect.stringMatching(/^Expected a valid JSON string/u));
        await expect(read).rejects.toHaveProperty(['partialResults', 0, 1, 'projects', 'proj', 'targets', 'deploy--pulumi', 'command'], 'node stacks.ts deploy');
    });

    describe('dependency handling', () => {
        it('should set each verb target to depend on its tool targets', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'package.json': '{ "name": "root", "workspaces": ["proj"] }',
                'proj/package.json': program,
                'proj/stacks.ts': 'export {};\n',
            });

            const results = await createNodesFunction(['proj/package.json'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['deploy']?.dependsOn).toStrictEqual(['deploy--*']);
            expect(results[0]?.[1].projects?.['proj']?.targets?.['install']?.dependsOn).toStrictEqual(['install--*']);
        });

        it('should set each tool target to wait for the same verb of the projects it depends on', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'package.json': '{ "name": "root", "workspaces": ["proj"] }',
                'proj/package.json': program,
                'proj/stacks.ts': 'export {};\n',
            });

            const results = await createNodesFunction(['proj/package.json'], {}, context);

            expect(results[0]?.[1].projects?.['proj']?.targets?.['deploy--pulumi']?.dependsOn).toContain('^deploy');
            expect(results[0]?.[1].projects?.['proj']?.targets?.['install--pulumi']?.dependsOn).toContain('^install');
        });

        it('should run the dry-run target with no task before it, so a preview applies no dependency stack', async ({ tempFs, context }) => {
            await tempFs.createFiles({
                'package.json': '{ "name": "root", "workspaces": ["proj", "site"] }',
                'proj/package.json': program,
                'proj/stacks.ts': 'export {};\n',
                'site/package.json': JSON.stringify({ name: '@proj/site', dependencies: { '@proj/infra': 'workspace:*' } }),
            });

            const results = await createNodesFunction(['proj/package.json', 'site/package.json'], {}, context);

            expect(results[1]?.[1].projects?.['site']?.targets?.['dry-run--pulumi']).toMatchObject({ command: 'node ../proj/stacks.ts deploy --operation=preview', dependsOn: [] });
            expect(results[1]?.[1].projects?.['site']?.targets?.['deploy--pulumi']?.dependsOn).toStrictEqual(['build--*', '^deploy']);
        });
    });

    it('should preview a program for real through nx run dry-run--pulumi, writing no file and applying no stack', { timeout: 120_000 }, async ({ tempFs, backend }) => {
        symlinkSync(join(import.meta.dirname, 'node_modules'), join(tempFs.tempDir, 'node_modules'), 'dir');
        await tempFs.createFiles({
            'nx.json': JSON.stringify({ plugins: ['@rasm/nx-pulumi'] }),
            'package.json': '{ "name": "root", "workspaces": ["proj"] }',
            'proj/package.json': JSON.stringify({ name: '@proj/infra', type: 'module', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'proj/stacks.ts': [
                "import { parseArgs } from 'node:util';",
                "import { LocalWorkspace } from '@pulumi/pulumi/automation/index.js';",
                "const { values } = parseArgs({ args: process.argv.slice(3), options: { operation: { type: 'string', default: 'up' } } });",
                "const stack = await LocalWorkspace.createOrSelectStack({ stackName: 'dev', projectName: 'fixture', program: async () => ({ greeting: 'hello' }) });",
                'await stack[values.operation]();',
                '',
            ].join('\n'),
        });
        const envVars = { PULUMI_BACKEND_URL: pathToFileURL(backend.tempDir).href, PULUMI_CONFIG_PASSPHRASE: 'suite' };
        const listed = () => globSync('**', { cwd: tempFs.tempDir, exclude: ['node_modules', '.nx'] }).toSorted();
        const before = listed();

        execFileSync('nx', ['run', '@proj/infra:dry-run--pulumi', '--outputStyle=static'], {
            cwd: tempFs.tempDir,
            env: environment(tempFs.tempDir, { NX_WORKSPACE_DATA_DIRECTORY: '.nx/workspace-data', ...envVars }),
            stdio: 'pipe',
        });
        const workspace = await LocalWorkspace.create({ projectSettings: { name: 'fixture', runtime: 'nodejs' }, envVars });

        expect(listed()).toStrictEqual(before);
        await expect(workspace.stackOutputs('dev')).resolves.toStrictEqual({});
    });
});
