import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-distribution';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = {
    'pnpm-lock.yaml': '',
    'products/apps/rhino/Arches/manifest.yml': 'name: arches\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Arches\nurl: https://example.com\n',
    'products/apps/adobe/Bridge/manifest.json': '{ "manifestVersion": 5, "id": "com.example.bridge", "name": "Bridge", "version": "1.2.3", "host": { "app": "PS", "minVersion": "24.0.0" } }\n',
    'products/web/site/.github/workflows/ci.yml': 'on: push\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const tempFs = new TempFs('distribution-windows-paths');
        onCleanup(() => {
            vi.unstubAllEnvs();
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        vi.stubEnv('PATH', tempFs.tempDir);
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { release: { projects: ['tag:release'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-distribution under Windows path separators', () => {
    it('should hand Nx workspace-relative posix roots, manifests, and inputs', async ({ context }) => {
        const results = await createNodes[1](
            Record.keys(files).filter((file) => file !== 'pnpm-lock.yaml'),
            {},
            context,
        );

        const projects = Record.fromEntries(results.flatMap(([, { projects: held }]) => Record.toEntries({ ...held })));
        expect(Record.keys(projects).toSorted()).toStrictEqual(['products/apps/adobe/Bridge', 'products/apps/rhino/Arches', 'products/web/site']);
        expect(Record.map(projects, ({ root }) => root)).toStrictEqual({ 'products/apps/adobe/Bridge': 'products/apps/adobe/Bridge', 'products/apps/rhino/Arches': 'products/apps/rhino/Arches', 'products/web/site': 'products/web/site' });
        expect(projects['products/apps/adobe/Bridge']?.release?.version?.versionActionsOptions).toStrictEqual({ manifest: 'manifest.json' });
        expect(projects['products/apps/rhino/Arches']?.targets?.['build--yak']?.options).toStrictEqual({ outputPath: '{projectRoot}' });
        expect(projects['products/web/site']?.targets?.['upgrade--jactionlint']?.inputs).toStrictEqual(['{projectRoot}/.github/**/*', '{projectRoot}/**/action.{yml,yaml}', { externalDependencies: [] }]);
    });
});
