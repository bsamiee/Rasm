import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-storybook';
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
        const tempFs = new TempFs('storybook-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'repo' }),
            'package-lock.json': '{}',
            'tsconfig.shared.json': '{"compilerOptions":{}}',
            'apps/site/project.json': JSON.stringify({ name: 'site' }),
            'apps/site/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.shared.json' }),
            'apps/site/.storybook/main.ts': `export default ${JSON.stringify({ stories: ['../src/**/*.stories.tsx'], framework: { name: '@storybook/react-vite', options: {} } })};\n`,
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'], production: ['default'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-storybook under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx posix story globs, tsconfig inputs, and output folders', async ({ context }) => {
        const results = await createNodes[1](['apps/site/.storybook/main.ts'], { outputPath: '.artifacts/storybook/{projectName}' }, context);

        const targets = results[0]?.[1].projects?.['apps/site']?.targets;
        expect(targets?.['build--storybook']?.command).toBe("storybook build --output-dir '../../.artifacts/storybook/{projectName}'");
        expect(targets?.['build--storybook']?.inputs).toContainEqual('{workspaceRoot}/apps/site/src/**/*.stories.tsx');
        expect(targets?.['build--storybook']?.inputs).toContainEqual({ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] });
        expect(targets?.['build--storybook']?.outputs?.[0]).toBe('{workspaceRoot}/.artifacts/storybook/{projectName}');
    });

    it('should serve the default build folder from a posix workspace path', async ({ context }) => {
        const results = await createNodes[1](['apps/site/.storybook/main.ts'], {}, context);

        expect(results[0]?.[1].projects?.['apps/site']?.targets?.['preview--storybook']?.options).toStrictEqual({ buildTarget: 'build--storybook', staticFilePath: 'apps/site/storybook-static' });
    });
});
