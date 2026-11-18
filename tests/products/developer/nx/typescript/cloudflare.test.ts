import { symlinkSync } from 'node:fs';
import { dirname, join, matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateNodesContext } from '@nx/devkit';
import { setupWorkspaceContext } from '@nx/devkit/internal';
import { createNodes } from '@rasm/nx-typescript';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const worker = (name: string) => `import { defineConfig, defineWorker } from 'cf/config';\nimport { compatibilityDate } from '../../shared/dates.ts';\n\nexport default defineConfig({ worker: defineWorker({ name: '${name}', entrypoint: './worker.ts', compatibilityDate }) });\n`;
const files = {
    'package-lock.json': '',
    'package.json': JSON.stringify({ type: 'module', workspaces: ['**'] }),
    'shared/dates.ts': "export const compatibilityDate = '2026-10-01';\n",
    'apps/edge/package.json': '{ "name": "edge", "type": "module" }',
    'apps/edge/cloudflare.config.ts': worker('edge'),
    'apps/edge/worker.ts': "export default { fetch: () => new Response('ok') };\n",
    'apps/plain/package.json': '{ "name": "plain", "type": "module" }',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const applyFilesToTempFsAndContext = async (tempFs: TempFs, fileSys: Record<string, string>) => {
    await tempFs.createFiles(fileSys);
    setupWorkspaceContext(tempFs.tempDir);
    return Object.keys(fileSys).filter((file) => matchesGlob(file, createNodes[0]));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('cloudflare-plugin');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        tempFs.createDirSync('node_modules');
        symlinkSync(dirname(fileURLToPath(import.meta.resolve('cf/package.json'))), join(tempFs.tempDir, 'node_modules/cf'), 'dir');
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'], production: ['default'] } }, workspaceRoot: tempFs.tempDir }));

describe('cloudflare.config.ts', () => {
    it('should create a codegen target for a cloudflare.config.ts beside a package.json, and none for a package without one', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, files);
        const results = await createNodes[1](configFiles, {}, context);
        const projects = Object.fromEntries(results.flatMap(([, nodes]) => Object.entries({ ...nodes.projects })));

        expect(projects['apps/edge']?.targets).toStrictEqual({
            codegen: { dependsOn: [{ target: 'codegen--*', params: 'forward' }], inputs: ['...', 'default', '^production', '{workspaceRoot}/shared/dates.ts', { externalDependencies: ['cf'] }] },
            'codegen--cf': {
                command: 'cf workers types',
                options: { cwd: 'apps/edge' },
                dependsOn: ['^codegen--*'],
                cache: true,
                inputs: ['default', '^production', '{workspaceRoot}/shared/dates.ts', { externalDependencies: ['cf'] }],
                outputs: ['{projectRoot}/.cloudflare/types'],
                metadata: { technologies: ['cloudflare'], description: 'Generates the Worker binding and runtime types from cloudflare.config.ts with cf workers types', help: { command: 'npx cf workers types --help', example: { args: ['--mode', 'production'] } } },
            },
            clean: { dependsOn: [{ target: 'clean--*', params: 'forward' }], inputs: ['...', 'default', '^default', { externalDependencies: [] }] },
            'clean--cf': {
                executor: '@rasm/nx-distribution:clean',
                options: { targets: ['codegen--cf'] },
                inputs: ['default', '^default', { externalDependencies: [] }],
                metadata: { technologies: ['cloudflare'], description: 'Removes the outputs codegen--cf declare', help: { command: 'npx nx run --help', example: { options: { targets: ['codegen--cf'] } } } },
            },
        });
        expect(Object.keys(projects)).toStrictEqual(['.', 'apps/edge']);
    });

    it('should name a cloudflare.config.ts that fails to load and keep every other project', async ({ tempFs, context }) => {
        const configFiles = await applyFilesToTempFsAndContext(tempFs, { ...files, 'apps/broken/package.json': '{ "name": "broken", "type": "module" }', 'apps/broken/cloudflare.config.ts': "throw new Error('broken on load');\n" });
        await expect(createNodes[1](configFiles, {}, context)).rejects.toThrow(AggregateCreateNodesError);
        await expect(createNodes[1](configFiles, {}, context)).rejects.toHaveProperty(['errors', 0, 0], 'apps/broken/cloudflare.config.ts');
        await expect(createNodes[1](configFiles, {}, context)).rejects.toHaveProperty(['partialResults', 2, 0], 'apps/edge/cloudflare.config.ts');
    });
});
