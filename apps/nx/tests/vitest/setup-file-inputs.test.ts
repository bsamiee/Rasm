import { symlinkSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createNodes } from '@rasm/nx-vitest';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEAD = ['default', '^default'] as const;
const TAIL = [{ externalDependencies: ['vitest'] }, { env: 'CI' }, { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true }] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const tempFs = new TempFs('vitest-setup-file-inputs');
        const previous = process.cwd();
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(previous);
            tempFs.cleanup();
        });
        tempFs.createDirSync('node_modules');
        ['vitest', 'vite'].forEach((name) => symlinkSync(dirname(fileURLToPath(import.meta.resolve(`${name}/package.json`))), join(tempFs.tempDir, 'node_modules', name), 'dir'));
        await tempFs.createFiles({ 'nx.json': '{}', 'package.json': '{"name":"repo","workspaces":["packages/*"]}' });
        return tempFs;
    })
    .extend('collect', ({ tempFs }) => async (vitest: Record<string, unknown>, { projectRoot = 'packages/lib', authoredRoot }: { readonly projectRoot?: string; readonly authoredRoot?: string } = {}) => {
        await tempFs.createFiles({
            [join(projectRoot, 'package.json')]: '{"name":"lib","type":"module","nx":{}}',
            [join(projectRoot, 'vitest.config.ts')]: `export default ${JSON.stringify({ ...(authoredRoot === undefined ? {} : { root: authoredRoot }), test: vitest })};\n`,
        });
        const [[, { projects }] = ['', {}]] = await createNodes[1]([join(projectRoot, 'vitest.config.ts')], {}, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
        return projects?.[projectRoot]?.targets?.['test--vitest']?.inputs;
    });

describe('collectSetupFileInputs', () => {
    it('should declare a setup file that lives outside the project root', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/setup.mts': '' });

        await expect(collect({ setupFiles: ['../../tools/vitest/setup.mts'] })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/setup.mts', ...TAIL]);
    });

    it('should declare the tsconfig vite resolves for the setup file and its extends chain', async ({ tempFs, collect }) => {
        await tempFs.createFiles({
            'tools/vitest/setup.mts': '',
            'tools/vitest/tsconfig.json': JSON.stringify({ extends: '../../tsconfig.shared.json' }),
            'tsconfig.shared.json': JSON.stringify({ compilerOptions: {} }),
        });

        await expect(collect({ setupFiles: ['../../tools/vitest/setup.mts'] })).resolves.toStrictEqual([
            ...HEAD,
            { json: '{workspaceRoot}/tools/vitest/tsconfig.json', fields: ['compilerOptions'] },
            { json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] },
            '{workspaceRoot}/tools/vitest/setup.mts',
            ...TAIL,
        ]);
    });

    it('should walk up to the nearest tsconfig when the setup file has no sibling one', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/nested/setup.mts': '', 'tools/tsconfig.json': JSON.stringify({ compilerOptions: {} }) });

        await expect(collect({ setupFiles: ['../../tools/vitest/nested/setup.mts'] })).resolves.toStrictEqual([...HEAD, { json: '{workspaceRoot}/tools/tsconfig.json', fields: ['compilerOptions'] }, '{workspaceRoot}/tools/vitest/nested/setup.mts', ...TAIL]);
    });

    it('should declare globalSetup, including the single-string form', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/global.mts': '' });

        await expect(collect({ globalSetup: '../../tools/vitest/global.mts' })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/global.mts', ...TAIL]);
    });

    it('should accept an absolute path', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/setup.mts': '' });

        await expect(collect({ setupFiles: [join(tempFs.tempDir, 'tools/vitest/setup.mts')] })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/setup.mts', ...TAIL]);
    });

    it('should not declare a setup file inside the project root, which `default` already covers', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'packages/lib/setup.ts': '' });

        await expect(collect({ setupFiles: ['./setup.ts'] })).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });

    it('should not declare a setup file from node_modules, which the lockfile already covers', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'node_modules/pkg/setup.js': '' });

        await expect(collect({ setupFiles: ['../../node_modules/pkg/setup.js'] })).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });

    it('should skip an entry that does not exist', async ({ collect }) => {
        await expect(collect({ setupFiles: ['../../tools/vitest/missing.mts'] })).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });

    it('should declare nothing for a root project, where `default` covers the workspace', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/setup.mts': '' });

        await expect(collect({ setupFiles: ['./tools/vitest/setup.mts'] }, { projectRoot: '.' })).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });

    it('should resolve against the project root when the config authors no root', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/setup.mts': '' });

        await expect(collect({ setupFiles: ['../../tools/vitest/setup.mts'] })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/setup.mts', ...TAIL]);
    });

    it('should not mistake a workspace-root file for the setup file when no root is authored', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'setup-at-root.mts': '' });

        await expect(collect({ setupFiles: ['./setup-at-root.mts'] })).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });

    it('should resolve a relative test.root against the project root', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'packages/lib/sub/x': '', 'tools/vitest/setup.mts': '' });

        await expect(collect({ root: 'sub', setupFiles: ['../../../tools/vitest/setup.mts'] })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/setup.mts', ...TAIL]);
    });

    it('should honor an absolute authored root', async ({ tempFs, collect }) => {
        await tempFs.createFiles({ 'tools/vitest/setup.mts': '' });

        await expect(collect({ setupFiles: ['../../tools/vitest/setup.mts'] }, { authoredRoot: join(tempFs.tempDir, 'packages/lib') })).resolves.toStrictEqual([...HEAD, '{workspaceRoot}/tools/vitest/setup.mts', ...TAIL]);
    });

    it('should declare nothing when the config has no setup entries', async ({ collect }) => {
        await expect(collect({})).resolves.toStrictEqual([...HEAD, ...TAIL]);
    });
});
