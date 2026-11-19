import { mkdirSync, symlinkSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CreateNodesContext } from '@nx/devkit';
import { createNodes } from '@rasm/nx-playwright';
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
        const tempFs = new TempFs('playwright-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles({
            'package.json': JSON.stringify({ name: 'repo', workspaces: ['apps/*'], nx: {} }),
            'package-lock.json': '{}',
            'tsconfig.base.json': '{}',
            'tsconfig.shared.json': JSON.stringify({ extends: './tsconfig.base.json' }),
            'apps/e2e/package.json': '{}',
            'apps/e2e/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.shared.json' }),
            'apps/e2e/playwright.config.js': "module.exports = { reporter: [['html']] }",
        });
        mkdirSync(join(tempFs.tempDir, 'node_modules/@playwright'), { recursive: true });
        symlinkSync(dirname(fileURLToPath(import.meta.resolve('@playwright/test/package.json'))), join(tempFs.tempDir, 'node_modules/@playwright/test'), 'dir');
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-playwright under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx and Playwright posix output folders and tsconfig inputs', async ({ context }) => {
        const results = await createNodes[1](['apps/e2e/playwright.config.js'], { outputPath: '.artifacts/playwright/{projectName}' }, context);

        const target = results[0]?.[1].projects?.['apps/e2e']?.targets?.['test--playwright'];
        expect(target?.command).toBe("playwright test {args} '--output=../../.artifacts/playwright/{projectName}/test-results'");
        expect(target?.options).toStrictEqual({ cwd: 'apps/e2e', env: { PLAYWRIGHT_HTML_OUTPUT_DIR: '../../.artifacts/playwright/{projectName}/html' } });
        expect(target?.inputs).toContainEqual({ json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions', 'extends'] });
        expect(target?.outputs).toStrictEqual(['{workspaceRoot}/.artifacts/playwright/{projectName}']);
    });
});
