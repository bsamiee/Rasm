import { existsSync, mkdtempSync, readdirSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, join } from 'node:path';
import { createNodes } from '@rasm/nx-playwright';
import { Clean, execute } from '@rasm/nx/executors/clean';
import { environment, it as nx } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { assert, describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const drawn = (name: string) =>
    `const { mkdirSync, writeFileSync } = require('node:fs');\nconst { dirname } = require('node:path');\nconst { test } = require('@playwright/test');\n\ntest('draws', ({}, info) => {\n    const at = info.outputPath('${name}.png');\n    mkdirSync(dirname(at), { recursive: true });\n    writeFileSync(at, '${name}');\n});\n`;
const routed = { outputPath: '.artifacts/playwright/{projectName}' };
const COVERED =
    "const { writeFile } = require('node:fs/promises');\nconst { createRequire } = require('node:module');\nconst { test } = require('@playwright/test');\nconst { yazl } = createRequire(createRequire(require.resolve('@playwright/test')).resolve('playwright'))('playwright-core/lib/utilsBundle');\n\ntest('covers', async ({}, info) => {\n    const zip = new yazl.ZipFile();\n    zip.addBuffer(Buffer.from(JSON.stringify({ '/app.js': { path: '/app.js', statementMap: { 0: { start: { line: 1, column: 0 }, end: { line: 1, column: 9 } } }, fnMap: {}, branchMap: {}, s: { 0: 1 }, f: {}, b: {} } })), 'trace.coverage');\n    zip.end();\n    await writeFile(info.outputPath('trace.zip'), Buffer.concat(await zip.outputStream.toArray()));\n    await info.attach('trace', { path: info.outputPath('trace.zip'), contentType: 'application/zip' });\n});\n";
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
        expect(targets['pack--playwright']).toMatchObject({ dependsOn: [...(targets['test--playwright']?.dependsOn ?? []), 'test--playwright'], cache: true, inputs: [...(targets['test--playwright']?.inputs ?? []), { runtime: 'node -p process.platform+process.arch' }] });
    });

    it('declares outputs apart from test--playwright that hold every file its run writes, leaving the files test--playwright wrote as they were', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('../../.artifacts/playwright/e2e/media', "[['html', { open: 'never' }]]") }).graph({ createNodes }, routed);
        const ran = await workspace.run(graph, 'e2e', 'test--playwright');
        const tested = new Map([...workspace.cached(graph, 'e2e', 'test--playwright')].map((file) => [file, readFileSync(join(workspace.root, file), 'utf-8')]));
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'e2e', 'pack--playwright');

        const packed = workspace.cached(graph, 'e2e', 'pack--playwright');
        expect({
            succeeded: [ran.success, success],
            written: [tested.size > 0, packed.size > 0],
            shared: [...packed].filter((file) => workspace.cached(graph, 'e2e', 'test--playwright').has(file)),
            undeclared: [...workspace.files('node_modules', 'tmp').difference(before)].filter((file) => !packed.has(file)),
            changed: [...tested].filter(([file, text]) => !existsSync(join(workspace.root, file)) || readFileSync(join(workspace.root, file), 'utf-8') !== text).map(([file]) => file),
        }).toStrictEqual({ succeeded: [true, true], written: [true, true], shared: [], undeclared: [], changed: [] });
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

    it('infers no test--playwright where every project of the config writes under outputPath, and draws their images with pack--playwright alone', async ({ workspace }) => {
        const graph = await workspace.remove('apps/e2e/smoke.spec.js').write({ 'apps/e2e/playwright.config.js': "module.exports = { reporter: [['list']], projects: [{ name: 'media', testMatch: '**/*.media.js', outputDir: '../../.artifacts/playwright/e2e/media' }] };\n" }).graph({ createNodes }, routed);

        const { success } = await workspace.run(graph, 'e2e', 'pack--playwright');

        expect({
            targets: Object.keys(graph.nodes['e2e']?.data.targets ?? {}).filter((name) => /^(?:test|pack)--playwright$/u.test(name)),
            success,
            imaged: [...workspace.files('node_modules', 'tmp')].filter((file) => file.endsWith('tile.png')).map((file) => file.split('/').slice(0, 4).join('/')),
        }).toStrictEqual({ targets: ['pack--playwright'], success: true, imaged: ['.artifacts/playwright/e2e/media'] });
    });
});

describe('clean--playwright', { timeout: 60_000 }, () => {
    it('keeps the transform cache a passing run wrote under a plain clean and empties it under configuration cache', async ({ workspace, onTestFinished }) => {
        const cache = mkdtempSync(join(tmpdir(), 'playwright-transform-'));
        onTestFinished(() => {
            vi.unstubAllEnvs();
            rmSync(cache, { recursive: true, force: true });
        });
        vi.stubEnv('PLAYWRIGHT_TRANSFORM_CACHE_DIR', cache);
        vi.stubEnv('PATH', environment(workspace.root).PATH);
        const graph = await workspace.write({ 'apps/e2e/typed.spec.ts': "import { test } from '@playwright/test';\n\ntest('typed', () => {});\n", 'apps/e2e/playwright.config.js': config('media') }).graph({ createNodes });
        const target = graph.nodes['e2e']?.data.targets?.['clean--playwright'];
        const cleaned = async (configuration?: 'cache') => {
            const stated: unknown = { commands: [], cwd: '.', folders: [], ...target?.options, ...(configuration === undefined ? {} : target?.configurations?.[configuration]) };
            assert(Schema.is(Clean)(stated), 'clean--playwright states clean options');
            await execute(stated, workspace.context(graph, 'e2e', 'clean--playwright', configuration));
            return existsSync(cache) ? readdirSync(cache, { recursive: true, withFileTypes: true }).filter((entry) => entry.isFile()).length : 0;
        };

        const { success } = await workspace.run(graph, 'e2e', 'test--playwright');
        const written = readdirSync(cache, { recursive: true, withFileTypes: true }).filter((entry) => entry.isFile()).length;
        const kept = await cleaned();
        const left = await cleaned('cache');

        expect({ success, wrote: written > 0, kept: kept === written, left }).toStrictEqual({ success: true, wrote: true, kept: true, left: 0 });
    });
});

describe('declared outputs of test--playwright', { timeout: 60_000 }, () => {
    it.for([
        { case: 'in the folders the config states, one outside the project', options: {}, reporter: "[['html', { open: 'never', outputFolder: 'reports/html' }], ['blob', { outputDir: '../../reports/blob' }], ['junit', { outputFile: 'reports/junit.xml' }]]", files: {}, writes: 'junit.xml' },
        { case: 'in the default folders of the html and blob reporters', options: {}, reporter: "[['html', { open: 'never' }], ['blob']]", files: {}, writes: 'index.html' },
        { case: 'under outputPath, whatever folders the config states', options: routed, reporter: "[['html', { open: 'never', outputFolder: 'reports/html' }], ['blob']]", files: {}, writes: 'index.html' },
        { case: 'under outputPath and in the file a reporter states', options: routed, reporter: "[['junit', { outputFile: 'reports/junit.xml' }]]", files: {}, writes: 'junit.xml' },
        { case: 'in the default file of the perfetto reporter', options: {}, reporter: "[['perfetto']]", files: {}, writes: 'perfetto.json' },
        { case: 'under outputPath for the perfetto reporter', options: routed, reporter: "[['perfetto']]", files: {}, writes: 'perfetto.json' },
        { case: 'in the default folder of the coverage reporter', options: {}, reporter: "[['coverage']]", files: { 'apps/e2e/covered.spec.js': COVERED }, writes: 'lcov.info' },
        { case: 'in the folder the coverage reporter states', options: routed, reporter: "[['coverage', { outputDir: 'reports/coverage' }]]", files: { 'apps/e2e/covered.spec.js': COVERED }, writes: 'lcov.info' },
    ])('hold every file a passing run writes $case', async ({ options, reporter, files, writes }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'apps/e2e/playwright.config.js': config('media', reporter) }).graph({ createNodes }, options);
        const before = workspace.files('node_modules', 'tmp');

        const { success } = await workspace.run(graph, 'e2e', 'test--playwright');

        const written = workspace.files('node_modules', 'tmp').difference(before);
        expect({ success, reached: [...written].some((file) => basename(file) === writes), undeclared: [...written.difference(workspace.cached(graph, 'e2e', 'test--playwright'))] }).toStrictEqual({ success: true, reached: true, undeclared: [] });
    });

    it('routes no folder under outputPath to the json and junit reporters, which print to stdout when no file name reaches them', async ({ workspace }) => {
        const graph = await workspace.write({ 'apps/e2e/playwright.config.js': config('media', "[['html', { open: 'never' }], ['json'], ['junit']]") }).graph({ createNodes }, routed);
        const target = graph.nodes['e2e']?.data.targets?.['test--playwright'];

        expect(target).toHaveProperty('options.env', { PLAYWRIGHT_HTML_OUTPUT_DIR: '../../.artifacts/playwright/e2e/html' });
        expect(target?.outputs).toStrictEqual(['{workspaceRoot}/.artifacts/playwright/{projectName}/test-results', '{workspaceRoot}/.artifacts/playwright/{projectName}/html']);
    });
});
