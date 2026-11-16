import type { CreateNodesContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-pulumi';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

const [glob, createNodesFunction] = createNodes;
const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('pulumi-programs');
        const cwd = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('targetsFor', ({ tempFs, context }) => async (files: Readonly<Record<string, string>>, root: string, options = {}) => {
        await tempFs.createFiles(files);
        const matched = await globWithWorkspaceContext(tempFs.tempDir, [glob]);
        const results = await createNodesFunction(matched, options, context);
        return Object.fromEntries(results.flatMap(([, r]) => Object.entries(r.projects ?? {})))[root]?.targets;
    });

describe('@rasm/nx-pulumi', () => {
    it('should only claim packages depending on @pulumi/pulumi', async ({ targetsFor }) => {
        const files = {
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/stacks.ts': 'export {};\n',
            'packages/cli/package.json': JSON.stringify({ name: '@proj/cli', bin: './cli.ts' }),
            'packages/cli/cli.ts': 'export {};\n',
        };

        expect(Object.keys({ ...(await targetsFor(files, 'packages/infra')) })).toStrictEqual(['install', 'install--pulumi', 'deploy', 'deploy--pulumi']);
        await expect(targetsFor(files, 'packages/cli')).resolves.toBeUndefined();
    });

    it('should infer bin targets, leaving cache unset as Nx caches no target without cache: true', async ({ targetsFor }) => {
        const files = {
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: { infra: './bin/stacks.ts' }, devDependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/bin/stacks.ts': 'export {};\n',
            'packages/tool/package.json': JSON.stringify({ name: '@proj/tool', bin: { tool: './bin/a.ts', 'tool-b': './bin/b.ts' }, dependencies: { '@pulumi/pulumi': '*' } }),
        };

        await expect(targetsFor(files, 'packages/tool')).resolves.toBeUndefined();
        await expect(targetsFor(files, 'packages/infra')).resolves.toStrictEqual({
            install: { dependsOn: ['install--*'], inputs: ['...', 'default', '^default', { externalDependencies: [] }] },
            'install--pulumi': {
                command: 'node bin/stacks.ts install',
                options: { cwd: 'packages/infra' },
                dependsOn: ['^build', '^install'],
                parallelism: false,
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { technologies: ['pulumi'], description: 'Runs the Pulumi Automation API program of the project with node bin/stacks.ts install' },
            },
            deploy: { dependsOn: ['deploy--*'], inputs: ['...', 'default', '^default', { externalDependencies: [] }] },
            'deploy--pulumi': {
                command: 'node bin/stacks.ts deploy',
                options: { cwd: 'packages/infra' },
                dependsOn: ['^build', '^deploy'],
                parallelism: false,
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { technologies: ['pulumi'], description: 'Runs the Pulumi Automation API program of the project with node bin/stacks.ts deploy' },
            },
        });
    });

    it('should infer a string bin whose file is build output, waiting for the build of its package', async ({ targetsFor }) => {
        const targets = await targetsFor(
            {
                'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
                'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: 'dist/stacks.js', dependencies: { '@pulumi/pulumi': '*' } }),
            },
            'packages/infra',
        );

        expect(targets?.['deploy--pulumi']?.command).toBe('node dist/stacks.js deploy');
        expect(targets?.['deploy--pulumi']?.dependsOn).toStrictEqual(['build--*', '^build', '^deploy']);
    });

    it('should wait for the build of a depended program package whose bin is build output', async ({ targetsFor }) => {
        const targets = await targetsFor(
            {
                'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
                'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: 'dist/stacks.js', dependencies: { '@pulumi/pulumi': '*' } }),
                'packages/site/package.json': JSON.stringify({ name: '@proj/site', dependencies: { '@proj/infra': 'workspace:*' } }),
            },
            'packages/site',
        );

        expect(targets?.['deploy--pulumi']?.command).toBe('node ../infra/dist/stacks.js deploy');
        expect(targets?.['deploy--pulumi']?.dependsOn).toStrictEqual(['build--*', '^build', '^deploy']);
    });
});
