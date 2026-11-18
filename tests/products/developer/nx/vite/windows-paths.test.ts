import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { type CreateNodesContext, getOutputsForTargetAndConfiguration } from '@nx/devkit';
import { createNodes } from '@rasm/nx-vite';
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
        const tempFs = new TempFs('vite-windows-paths');
        process.chdir(tempFs.tempDir);
        symlinkSync(join(import.meta.dirname, 'node_modules'), join(tempFs.tempDir, 'node_modules'), 'dir');
        onCleanup(() => {
            tempFs.cleanup();
            process.chdir(cwd);
        });
        await tempFs.createFiles({
            'package-lock.json': '{}',
            'package.json': JSON.stringify({ name: 'repo', workspaces: ['apps/web'], nx: {} }),
            'apps/web/package.json': '{"type":"module"}',
            'apps/web/index.html': '',
            'apps/web/vite.config.mts': 'export default {};',
            'apps/site/project.json': '{}',
            'apps/site/index.html': '',
            'apps/site/vite.config.mts': 'export default {};',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-vite under Windows path separators', () => {
    it('should declare build outputs Nx reads as a posix path under the workspace root', async ({ context }) => {
        const results = await createNodes[1](['apps/site/vite.config.mts'], { outputPath: '.artifacts/vite/{projectName}' }, context);

        const data = results[0]?.[1].projects?.['apps/site'];
        expect(data?.targets?.['build--vite']).toMatchObject({ options: { cwd: 'apps/site' }, outputs: ['{workspaceRoot}/.artifacts/vite/{projectName}'], command: "vite build --outDir '../../.artifacts/vite/{projectName}' --emptyOutDir" });
        expect(getOutputsForTargetAndConfiguration({ project: 'site', target: 'build--vite' }, {}, { name: 'site', type: 'app', data: { root: 'apps/site', ...data } })).toStrictEqual(['.artifacts/vite/site']);
    });

    it('should infer a project its package.json makes a package manager workspace member', async ({ context }) => {
        const results = await createNodes[1](['apps/web/vite.config.mts'], {}, context);

        expect(results.map(([file, { projects }]) => [file, Object.keys({ ...projects })])).toStrictEqual([['apps/web/vite.config.mts', ['apps/web']]]);
    });
});
