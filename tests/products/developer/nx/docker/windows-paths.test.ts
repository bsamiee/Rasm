import { type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-docker';
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
        const tempFs = new TempFs('docker-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            tempFs.cleanup();
            process.chdir(cwd);
        });
        await tempFs.createFiles({
            'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
            'infra/compose.yaml':
                'include:\n    - ../shared/compose.base.yaml\nservices:\n    api:\n        build:\n            context: ../apps/api\n            dockerfile: Dockerfile\n        env_file: api.env\n    site:\n        build:\n            context: ../libs/site/dist\n            dockerfile: ../../../infra/site.Dockerfile\n',
            'infra/api.env': 'PORT=8080\n',
            'infra/site.Dockerfile': 'FROM scratch\nCOPY . /site\n',
            'infra/project.json': '{}',
            'apps/api/Dockerfile': 'FROM node:18',
            'apps/api/project.json': '{}',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-docker under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx workspace-relative posix roots and inputs', async ({ context }) => {
        const results = await createNodes[1](['infra/compose.yaml'], {}, context);

        const projects = results[0]?.[1].projects;
        expect(Object.keys({ ...projects })).toStrictEqual(['apps/api', 'infra']);
        expect(projects?.['infra']?.targets?.['check--compose']?.inputs).toStrictEqual([
            '{workspaceRoot}/apps/api/Dockerfile',
            '{workspaceRoot}/infra/.env',
            '{workspaceRoot}/infra/api.env',
            '{workspaceRoot}/infra/compose.yaml',
            '{workspaceRoot}/infra/site.Dockerfile',
            '{workspaceRoot}/shared/compose.base.yaml',
            { externalDependencies: [] },
        ]);
    });

    it('should match a build context to the posix output of the project producing it', async ({ tempFs, context }) => {
        const results = await createNodes[1](['infra/compose.yaml'], {}, context);

        const result = await createDependencies(
            {},
            {
                workspaceRoot: tempFs.tempDir,
                projects: { infra: { root: 'infra', ...results[0]?.[1].projects?.['infra'] }, site: { root: 'libs/site', targets: { build: { command: 'true', outputs: ['{projectRoot}/dist'] } } } },
                externalNodes: {},
                fileMap: { projectFileMap: {}, nonProjectFiles: [] },
                filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
                nxJsonConfiguration: context.nxJsonConfiguration,
            },
        );

        expect(result).toStrictEqual([{ source: 'infra', target: 'site', type: DependencyType.static, sourceFile: 'infra/compose.yaml' }]);
    });
});
