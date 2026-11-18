import type { CreateNodesContext } from '@nx/devkit';
import { globWithWorkspaceContext, resetWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-pulumi';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

const [glob, createNodesFunction] = createNodes;
const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('program-nodes');
        const cwd = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }))
    .extend('run', ({ tempFs, context }) => async (files: Readonly<Record<string, string>>, options = {}) => {
        await tempFs.createFiles(files);
        const matched = await globWithWorkspaceContext(tempFs.tempDir, [glob]);
        return createNodesFunction(matched, options, context);
    });

describe('@rasm/nx-pulumi createNodes', () => {
    it('should match package manifests and project markers', () => {
        expect(glob).toBe('{**/package.json,**/project.json}');
    });

    it('should infer the targets of a program in each package depending on it, leaving cache unset as Nx caches no target without cache: true', async ({ run }) => {
        const results = await run({
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/stacks.ts': 'export {};\n',
            'packages/site/package.json': JSON.stringify({ name: '@proj/site', dependencies: { '@proj/infra': 'workspace:*' } }),
            'packages/plain/package.json': JSON.stringify({ name: '@proj/plain' }),
        });

        expect(results.map(([file, result]) => [file, Object.keys({ ...result.projects })])).toStrictEqual([
            ['packages/infra/package.json', ['packages/infra']],
            ['packages/site/package.json', ['packages/site']],
        ]);
        expect(results[1]?.[1].projects?.['packages/site']?.targets?.['deploy--pulumi']).toStrictEqual({
            command: 'node ../infra/stacks.ts deploy',
            options: { cwd: 'packages/site' },
            dependsOn: ['build--*', '^deploy--*'],
            parallelism: false,
            inputs: ['default', '^default', { externalDependencies: [] }],
            metadata: { technologies: ['pulumi'], description: 'Runs the Pulumi Automation API program the project depends on with node ../infra/stacks.ts deploy' },
        });
    });

    it('should not reuse targets when the matched manifests change', async ({ tempFs, context }) => {
        await tempFs.createFiles({
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/stacks.ts': 'export {};\n',
            'packages/site/package.json': JSON.stringify({ name: '@proj/site', dependencies: { '@proj/infra': 'workspace:*' } }),
        });

        const all = await createNodesFunction(['package.json', 'packages/infra/package.json', 'packages/site/package.json'], {}, context);
        const narrowed = await createNodesFunction(['package.json', 'packages/site/package.json'], {}, context);

        expect(all.map(([file]) => file)).toStrictEqual(['packages/infra/package.json', 'packages/site/package.json']);
        expect(narrowed).toStrictEqual([]);
    });

    it('should not reuse targets when a manifest changes', async ({ tempFs, context, run }) => {
        const before = await run({
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/stacks.ts': 'export {};\n',
        });

        tempFs.createFilesSync({
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './apply.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/apply.ts': 'export {};\n',
        });
        resetWorkspaceContext();
        const after = await createNodesFunction(['package.json', 'packages/infra/package.json'], {}, context);

        expect(before[0]?.[1].projects?.['packages/infra']?.targets?.['deploy--pulumi']?.command).toBe('node stacks.ts deploy');
        expect(after[0]?.[1].projects?.['packages/infra']?.targets?.['deploy--pulumi']).toMatchObject({ command: 'node apply.ts deploy', dependsOn: ['^build--*', '^deploy--*'] });
    });

    it("should treat a package.json as a project root only where core's default package-json plugin does", async ({ run }) => {
        const results = await run({
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["tools/esm"] }',
            'tools/scripts/package.json': JSON.stringify({ name: 'scripts', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'tools/scripts/stacks.ts': 'export {};\n',
            'tools/named/package.json': JSON.stringify({ name: 'named', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'tools/named/project.json': '{}',
            'tools/named/stacks.ts': 'export {};\n',
            'tools/esm/package.json': JSON.stringify({ name: 'esm', bin: './stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'tools/esm/stacks.ts': 'export {};\n',
        });

        expect(results.map(([, r]) => Object.keys({ ...r.projects }))).toStrictEqual([['tools/esm'], ['tools/named']]);
    });

    it('should quote program paths the shell would split', async ({ run }) => {
        const results = await run({
            'package.json': '{ "name": "root", "nx": {}, "workspaces": ["packages/*"] }',
            'packages/infra/package.json': JSON.stringify({ name: '@proj/infra', bin: './my stacks.ts', dependencies: { '@pulumi/pulumi': '*' } }),
            'packages/infra/my stacks.ts': 'export {};\n',
        });

        expect(results[0]?.[1].projects?.['packages/infra']?.targets?.['deploy--pulumi']?.command).toBe("node 'my stacks.ts' deploy");
    });
});
