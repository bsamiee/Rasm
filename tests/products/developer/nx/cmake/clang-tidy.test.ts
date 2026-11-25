import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const launched = (cacheVariables: Readonly<Record<string, string>>) =>
    JSON.stringify({
        version: 10,
        configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}`, cacheVariables }],
        workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
    });
const tree = {
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(libs/core)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
    'libs/core/core.cpp': 'int *none() { return 0; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('clang-tidy targets', () => {
    it.for([
        { case: 'sets a clang-tidy launcher', cacheVariables: { CMAKE_CXX_CLANG_TIDY: 'clang-tidy' }, inferred: true },
        { case: 'sets no clang-tidy launcher', cacheVariables: {}, inferred: false },
    ])('exist only where the configure preset of a workflow $case', async ({ cacheVariables, inferred }, { workspace }) => {
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': launched(cacheVariables) }).graph(cmake);

        const targets = graph.nodes['fixture_core']?.data.targets ?? {};
        expect(['check--clang-tidy' in targets, 'fix--clang-tidy' in targets]).toStrictEqual([inferred, inferred]);
    });

    it.for([
        { case: 'the file its --config-file names', launcher: 'clang-tidy;--config-file=$env{PWD}/tools/tidy.yaml', files: ['{workspaceRoot}/tools/tidy.yaml'] },
        { case: 'each .clang-tidy above or inside the member folder', launcher: 'clang-tidy', files: ['{workspaceRoot}/.clang-tidy', '{workspaceRoot}/libs/.clang-tidy', '{workspaceRoot}/libs/core/**/.clang-tidy'] },
    ])('hashes in check--clang-tidy $case', async ({ launcher, files }, { workspace }) => {
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': launched({ CMAKE_CXX_CLANG_TIDY: launcher }) }).graph(cmake);

        const { inputs = [] } = graph.nodes['fixture_core']?.data.targets?.['check--clang-tidy'] ?? {};
        expect(
            inputs
                .filter(Predicate.isString)
                .filter((input) => input.startsWith('{workspaceRoot}'))
                .toSorted(),
        ).toStrictEqual(files);
    });

    it('fix--clang-tidy rewrites the member source with the fixes clang-tidy exports', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': launched({ CMAKE_CXX_CLANG_TIDY: 'clang-tidy;--checks=-*,modernize-use-nullptr' }) }).graph(cmake);

        const { success } = await workspace.run(graph, 'fixture_core', 'fix--clang-tidy');

        expect(success).toBe(true);
        expect(readFileSync(join(workspace.root, 'libs/core/core.cpp'), 'utf-8')).toBe('int *none() { return nullptr; }\n');
    });
});
