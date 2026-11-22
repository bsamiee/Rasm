import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-playwright';
import { it as nx } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const drawn = (name: string) =>
    `const { mkdirSync, writeFileSync } = require('node:fs');\nconst { dirname } = require('node:path');\nconst { test } = require('@playwright/test');\n\ntest('draws', ({}, info) => {\n    const at = info.outputPath('${name}.png');\n    mkdirSync(dirname(at), { recursive: true });\n    writeFileSync(at, '${name}');\n});\n`;
const routed = { outputPath: '.artifacts/playwright/{projectName}' };
const config = (outputDir: string, reporter = "[['list']]") => `module.exports = { reporter: ${reporter}, projects: [{ name: 'suite' }, { name: 'media', testMatch: '**/*.media.js', outputDir: '${outputDir}' }] };\n`;

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({ 'package.json': '{"name":"repo","workspaces":["apps/*"]}', 'package-lock.json': '{}', 'apps/e2e/package.json': '{"name":"e2e"}', 'apps/e2e/smoke.spec.js': drawn('smoke'), 'apps/e2e/store.media.js': drawn('tile') });
});

describe('pack--playwright of config projects whose own outputDir sits under outputPath', { timeout: 60_000 }, () => {
    it('writes the images of those projects alone into its outputs, after test--playwright, which leaves them unrun and undeclared', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media') }).graph({ createNodes }, routed);
        const targets = graph.nodes['e2e']?.data.targets ?? {};

        const tested = await workspace.run(graph, 'e2e', 'test--playwright');
        const before = workspace.files('node_modules', 'tmp');
        const packed = await workspace.run(graph, 'e2e', 'pack--playwright');
        const written = workspace.files('node_modules', 'tmp').difference(before);

        expect({
            succeeded: [tested.success, packed.success],
            imagedByTest: [...before].filter((file) => file.endsWith('.png')).map((file) => file.split('/').slice(0, 4).join('/')),
            imagedByPack: [...written].filter((file) => file.endsWith('.png')),
            cachedByPack: [...workspace.cached(graph, 'e2e', 'pack--playwright')].filter((file) => file.endsWith('.png')),
            cachedByTest: [...workspace.cached(graph, 'e2e', 'test--playwright')].filter((file) => file.endsWith('tile.png')),
        }).toStrictEqual({
            succeeded: [true, true],
            imagedByTest: ['.artifacts/playwright/e2e/test-results'],
            imagedByPack: [expect.stringMatching(/^\.artifacts\/playwright\/e2e\/media\/[^/]+\/tile\.png$/u)],
            cachedByPack: [expect.stringMatching(/^\.artifacts\/playwright\/e2e\/media\/[^/]+\/tile\.png$/u)],
            cachedByTest: [],
        });
        expect(targets['pack--playwright']).toMatchObject({ dependsOn: ['test--playwright'], cache: true, inputs: targets['test--playwright']?.inputs });
    });

    it('leaves no file an earlier run wrote in its outputs, as playwright test empties each outputDir before it writes', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media') }).graph({ createNodes }, routed);
        await workspace.run(graph, 'e2e', 'test--playwright');
        workspace.write({ '.artifacts/playwright/e2e/media/retired.png': 'retired' });

        await workspace.run(graph, 'e2e', 'pack--playwright');

        expect([...workspace.files('node_modules', 'tmp')].filter((file) => file.endsWith('retired.png'))).toStrictEqual([]);
    });

    it('declares outputs apart from test--playwright that hold every file its run writes, leaving the files test--playwright wrote as they were', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media', "[['html', { open: 'never' }]]") }).graph({ createNodes }, routed);
        await workspace.run(graph, 'e2e', 'test--playwright');
        const tested = new Map([...workspace.cached(graph, 'e2e', 'test--playwright')].map((file) => [file, readFileSync(join(workspace.root, file), 'utf-8')]));
        const before = workspace.files('node_modules', 'tmp');

        await workspace.run(graph, 'e2e', 'pack--playwright');

        const packed = workspace.cached(graph, 'e2e', 'pack--playwright');
        expect({
            shared: [...packed].filter((file) => workspace.cached(graph, 'e2e', 'test--playwright').has(file)),
            undeclared: [...workspace.files('node_modules', 'tmp').difference(before)].filter((file) => !packed.has(file)),
            changed: [...tested].filter(([file, text]) => !existsSync(join(workspace.root, file)) || readFileSync(join(workspace.root, file), 'utf-8') !== text).map(([file]) => file),
        }).toStrictEqual({ shared: [], undeclared: [], changed: [] });
    });

    it('keeps under test--playwright a project inheriting the config outputDir test--playwright routes its results to', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media').replace('module.exports = {', "module.exports = { outputDir: '../../.artifacts/playwright/e2e/test-results',") }).graph({ createNodes }, routed);
        const targets = graph.nodes['e2e']?.data.targets ?? {};

        expect(targets).toHaveProperty(['test--playwright', 'options', 'command'], expect.stringContaining('--project=suite'));
        expect(targets).toHaveProperty(['pack--playwright', 'options', 'command'], expect.not.stringContaining('--project=suite'));
    });

    it('infers no pack--playwright where no file matches the testMatch of the project under outputPath, and runs test--playwright', async ({ workspace }) => {
        const graph = await workspace
            .remove('apps/e2e/store.media.js')
            .write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media') })
            .graph({ createNodes }, routed);

        const { success } = await workspace.run(graph, 'e2e', 'test--playwright');

        expect(success).toBe(true);
        expect(Object.keys(graph.nodes['e2e']?.data.targets ?? {})).not.toContain('pack--playwright');
    });

    it.for([
        { case: 'an outputDir outside outputPath', outputDir: 'media', options: routed },
        { case: 'no outputPath', outputDir: '../../.artifacts/playwright/e2e/media', options: {} },
    ])('infers no pack--playwright and runs every project under test--playwright for $case', async ({ outputDir, options }, { workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config(outputDir) }).graph({ createNodes }, options);

        const { success } = await workspace.run(graph, 'e2e', 'test--playwright');

        expect(success).toBe(true);
        expect(Object.keys(graph.nodes['e2e']?.data.targets ?? {})).not.toContain('pack--playwright');
        expect([...workspace.files('node_modules', 'tmp')].filter((file) => file.endsWith('tile.png'))).toHaveLength(1);
    });
});
