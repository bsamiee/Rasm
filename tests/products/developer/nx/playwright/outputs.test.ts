import { createNodes } from '@rasm/nx-playwright';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}' });
});

describe('declared outputs of test--playwright', { timeout: 60_000 }, () => {
    const spec = "import { test } from '@playwright/test';\n\ntest('passes', () => {});\n";

    it.for([
        { case: 'in the folders the config states, one outside the project', options: {}, reporter: "[['html', { open: 'never', outputFolder: 'reports/html' }], ['blob', { outputDir: '../../reports/blob' }], ['junit', { outputFile: 'reports/junit.xml' }]]" },
        { case: 'in the default folders of the html and blob reporters', options: {}, reporter: "[['html', { open: 'never' }], ['blob']]" },
        { case: 'under outputPath, whatever folders the config states', options: { outputPath: '.artifacts/playwright/{projectName}' }, reporter: "[['html', { open: 'never', outputFolder: 'reports/html' }], ['blob'], ['junit']]" },
        { case: 'under outputPath and in the file a reporter states', options: { outputPath: '.artifacts/playwright/{projectName}' }, reporter: "[['junit', { outputFile: 'reports/junit.xml' }]]" },
    ])('hold every file a passing run writes $case', async ({ options, reporter }, { workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/package.json': '{"name":"e2e"}', 'apps/e2e/smoke.spec.ts': spec, 'apps/e2e/playwright.config.js': `module.exports = { reporter: ${reporter} };\n` }).graph({ createNodes }, options);
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'e2e', 'test--playwright');

        const written = workspace.files('node_modules', 'tmp').difference(before);
        expect(success).toBe(true);
        expect([...written]).not.toStrictEqual([]);
        expect([...written.difference(workspace.cached(graph, 'e2e', 'test--playwright'))]).toStrictEqual([]);
    });
});
