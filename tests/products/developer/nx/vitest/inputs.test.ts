import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-vitest';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';
import { it } from '#/installed';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"repo","workspaces":["libs/*","packages/*"]}' };
const LIB = { ...ROOT, 'libs/lib/package.json': '{"name":"lib"}' };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('inputs of test--vitest', () => {
    it.for([
        { case: 'states production', nxJson: '{"namedInputs":{"default":["{projectRoot}/**/*"],"production":["default","!{projectRoot}/**/*.test.ts"]}}', upstream: ['^production'] },
        { case: 'states no production', nxJson: '{}', upstream: ['^default'] },
    ])('hashes dependency sources through the named input nx.json $case', async ({ nxJson, upstream }, { workspace }) => {
        const graph = await workspace.write({ ...LIB, 'nx.json': nxJson, 'libs/lib/vitest.config.ts': 'export default {};\n' }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs?.filter((input) => Predicate.isString(input) && input.startsWith('^'))).toStrictEqual(upstream);
    });

    it('hashes each installed package the configuration imports by version, and a workspace package it imports by its sources', async ({ workspace }) => {
        workspace.write({
            ...LIB,
            'libs/helper/package.json': '{"name":"helper","type":"module","exports":"./index.js"}',
            'libs/helper/index.js': "export const include = ['*.test.ts'];\n",
            'libs/lib/vitest.config.ts': "import { mergeConfig } from 'vite';\nimport { include } from 'helper';\n\nexport default mergeConfig({ test: { include } }, {});\n",
        });
        symlinkSync(join(workspace.root, 'libs/helper'), join(workspace.root, 'node_modules/helper'), 'dir');

        const graph = await workspace.graph({ createNodes });

        const inputs = graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs;

        expect(inputs).toContainEqual({ input: 'default', projects: ['helper'] });
        expect(inputs?.flatMap((input) => (Predicate.hasProperty(input, 'externalDependencies') ? input.externalDependencies : [])).toSorted()).toStrictEqual(['@vitest/coverage-v8', 'vite', 'vitest']);
    });

    it('hashes a file forceRerunTriggers names beyond Vitest defaults', async ({ workspace }) => {
        const graph = await workspace.write({ ...LIB, 'shared/schema.json': '{}', 'libs/lib/vitest.config.ts': "export default { test: { forceRerunTriggers: ['**/shared/schema.json'] } };\n" }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs?.filter((input) => Predicate.isString(input) && input.startsWith('{workspaceRoot}/**'))).toStrictEqual(['{workspaceRoot}/**/shared/schema.json']);
    });

    it('hashes a root configuration outside the package it tests', async ({ workspace }) => {
        const graph = await workspace.write({ ...ROOT, 'vitest.config.ts': "export default { test: { projects: ['packages/*'] } };\n", 'packages/a/package.json': '{"name":"a"}' }).graph({ createNodes });

        expect(graph.nodes['a']?.data.targets?.['test--vitest']?.inputs).toContain('{workspaceRoot}/vitest.config.ts');
    });
});

describe('setup file inputs', () => {
    it.for([
        { case: 'a setup file outside the project', config: { test: { setupFiles: ['../../tools/vitest/setup.mts'] } }, files: { 'tools/vitest/setup.mts': '' }, outside: ['{workspaceRoot}/tools/vitest/setup.mts'] },
        { case: 'a globalSetup string', config: { test: { globalSetup: '../../tools/vitest/global.mts' } }, files: { 'tools/vitest/global.mts': '' }, outside: ['{workspaceRoot}/tools/vitest/global.mts'] },
        { case: 'an absolute setup path', config: { test: { setupFiles: ['<root>/tools/vitest/setup.mts'] } }, files: { 'tools/vitest/setup.mts': '' }, outside: ['{workspaceRoot}/tools/vitest/setup.mts'] },
        {
            case: 'a setup file whose folder holds a tsconfig chain',
            config: { test: { setupFiles: ['../../tools/vitest/setup.mts'] } },
            files: { 'tools/vitest/setup.mts': '', 'tools/vitest/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}', 'tsconfig.shared.json': '{"compilerOptions":{}}' },
            outside: [{ json: '{workspaceRoot}/tools/vitest/tsconfig.json', fields: ['compilerOptions'] }, { json: '{workspaceRoot}/tsconfig.shared.json', fields: ['compilerOptions'] }, '{workspaceRoot}/tools/vitest/setup.mts'],
        },
        {
            case: 'a setup file below the nearest tsconfig',
            config: { test: { setupFiles: ['../../tools/vitest/nested/setup.mts'] } },
            files: { 'tools/vitest/nested/setup.mts': '', 'tools/tsconfig.json': '{"compilerOptions":{}}' },
            outside: [{ json: '{workspaceRoot}/tools/tsconfig.json', fields: ['compilerOptions'] }, '{workspaceRoot}/tools/vitest/nested/setup.mts'],
        },
        { case: 'a relative test.root', config: { test: { root: 'sub', setupFiles: ['../../../tools/vitest/setup.mts'] } }, files: { 'libs/lib/sub/a.test.ts': '', 'tools/vitest/setup.mts': '' }, outside: ['{workspaceRoot}/tools/vitest/setup.mts'] },
        { case: 'a setup file inside the project', config: { test: { setupFiles: ['./setup.ts'] } }, files: { 'libs/lib/setup.ts': '' }, outside: [] },
        { case: 'a setup file in node_modules', config: { test: { setupFiles: ['../../node_modules/pkg/setup.js'] } }, files: { 'node_modules/pkg/setup.js': '' }, outside: [] },
        { case: 'a relative setup path only the workspace root holds', config: { test: { setupFiles: ['./setup.mts'] } }, files: { 'setup.mts': '' }, outside: [] },
        { case: 'an authored root above the project', config: { root: '..', test: { setupFiles: ['../tools/vitest/setup.mts'] } }, files: { 'tools/vitest/setup.mts': '' }, outside: ['{workspaceRoot}/tools/vitest/setup.mts'] },
    ])('hashes the files outside the project of $case', async ({ config, files, outside }, { workspace }) => {
        const graph = await workspace.write({ ...LIB, ...files, 'libs/lib/vitest.config.ts': `export default ${JSON.stringify(config).replaceAll('<root>', workspace.root)};\n` }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs?.filter((input) => (Predicate.isString(input) && input.startsWith('{workspaceRoot}/')) || Predicate.hasProperty(input, 'json'))).toStrictEqual(outside);
    });

    it('hashes no setup file of a root project, whose default input holds the workspace', async ({ workspace }) => {
        const graph = await workspace.write({ 'project.json': '{"name":"repo"}', 'tools/vitest/setup.mts': '', 'tools/tsconfig.json': '{"compilerOptions":{}}', 'vitest.config.ts': "export default { test: { setupFiles: ['./tools/vitest/setup.mts'] } };\n" }).graph({ createNodes });

        expect(graph.nodes['repo']?.data.targets?.['test--vitest']?.inputs?.filter((input) => (Predicate.isString(input) && input.startsWith('{workspaceRoot}/')) || Predicate.hasProperty(input, 'json'))).toStrictEqual([]);
    });
});

describe('cached inference', () => {
    it('declares the outputs of the plugin options of the latest run', async ({ workspace }) => {
        await workspace.write({ ...LIB, 'libs/lib/vitest.config.ts': 'export default {};\n' }).graph({ createNodes }, { outputPath: '.artifacts/first' });

        const graph = await workspace.graph({ createNodes }, { outputPath: '.artifacts/second' });

        expect(graph.nodes['lib']?.data.targets?.['test--vitest']?.outputs?.filter((output) => output.startsWith('{workspaceRoot}/.artifacts/'))).toStrictEqual(['{workspaceRoot}/.artifacts/second']);
    });

    it('re-reads the configuration after a file it imports changes', async ({ workspace }) => {
        await workspace.write({ ...LIB, 'tools/vitest/setup.mts': '', 'shared/settings.ts': 'export const setupFiles: string[] = [];\n', 'libs/lib/vitest.config.ts': "import { setupFiles } from '../../shared/settings.ts';\n\nexport default { test: { setupFiles } };\n" }).graph({ createNodes });

        const graph = await workspace.write({ 'shared/settings.ts': "export const setupFiles = ['../../tools/vitest/setup.mts'];\n" }).graph({ createNodes });

        expect(graph.nodes['lib']?.data.targets?.['test--vitest']?.inputs).toContain('{workspaceRoot}/tools/vitest/setup.mts');
    });
});
