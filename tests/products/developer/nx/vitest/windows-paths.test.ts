import { symlinkSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vitest';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('vitest-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        tempFs.createDirSync('node_modules');
        ['vitest', 'vite'].forEach((name) => symlinkSync(dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`))), join(tempFs.tempDir, 'node_modules', name), 'dir'));
        await tempFs.createFiles({
            'nx.json': '{}',
            'package.json': '{"name":"repo","nx":{},"workspaces":["packages/*"]}',
            'package-lock.json': '{}',
            'tsconfig.shared.json': '{"compilerOptions":{}}',
            'tools/vitest/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
            'tools/vitest/setup.mts': '',
            'packages/a/package.json': '{"name":"a","type":"module"}',
            'packages/b/package.json': '{"name":"b","type":"module"}',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-vitest under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx posix setup, tsconfig, attachment, and coverage paths', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'packages/a/vitest.config.ts': "export default { test: { setupFiles: ['../../tools/vitest/setup.mts'] } };\n" });

        const results = await createNodes[1](['packages/a/vitest.config.ts'], { outputPath: '.artifacts/vitest/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' }, context);

        const target = results[0]?.[1].projects?.['packages/a']?.targets?.['test--vitest'];
        expect(target?.options).toStrictEqual({ config: 'packages/a/vitest.config.ts', projects: [], attachmentsDir: '.artifacts/vitest/{projectName}/attachments' });
        expect(target?.inputs).toStrictEqual([
            'default',
            '^default',
            { json: '{workspaceRoot}/tools/vitest/tsconfig.json', fields: ['compilerOptions'] },
            { json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] },
            '{workspaceRoot}/tools/vitest/setup.mts',
            { externalDependencies: ['vitest'] },
            { env: 'CI' },
            { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true },
        ]);
        expect(target?.outputs).toStrictEqual(['{workspaceRoot}/.artifacts/vitest/{projectName}', '{workspaceRoot}/.artifacts/coverage/{projectName}/lcov.info']);
    });

    it('should find the project root of each project a root configuration lists', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n" });

        const results = await createNodes[1](['vitest.config.ts'], {}, context);

        expect(Object.keys({ ...results[0]?.[1].projects }).toSorted()).toStrictEqual(['packages/a', 'packages/b']);
    });
});
