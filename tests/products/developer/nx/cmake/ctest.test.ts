import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, outputs, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const presets = {
    version: 10,
    configurePresets: [
        { name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` },
        { name: 'coverage', inherits: 'default', cacheVariables: { CMAKE_CXX_COMPILER: 'clang++', CMAKE_CXX_FLAGS: '-fprofile-instr-generate -fcoverage-mapping' } },
    ],
    buildPresets: [
        { name: 'default', configurePreset: 'default' },
        { name: 'coverage', configurePreset: 'coverage' },
    ],
    testPresets: [
        { name: 'default', configurePreset: 'default' },
        { name: 'coverage', configurePreset: 'coverage' },
    ],
    workflowPresets: [
        {
            name: 'default',
            steps: [
                { type: 'configure', name: 'default' },
                { type: 'build', name: 'default' },
                { type: 'test', name: 'default' },
            ],
        },
        {
            name: 'coverage',
            steps: [
                { type: 'configure', name: 'coverage' },
                { type: 'build', name: 'coverage' },
                { type: 'test', name: 'coverage' },
            ],
        },
    ],
};
const tree = {
    'CMakePresets.json': JSON.stringify(presets),
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(passing)\nadd_subdirectory(failing)\nadd_subdirectory(untested)\n',
    'passing/CMakeLists.txt': 'project(passing LANGUAGES CXX)\nadd_executable(passing_test test.cpp)\nadd_test(NAME passing_test COMMAND passing_test)\n',
    'passing/test.cpp': 'int twice(int value) { return value * 2; }\n\nint main() { return twice(2) == 4 ? 0 : 1; }\n',
    'failing/CMakeLists.txt': 'project(failing LANGUAGES CXX)\nadd_executable(failing_test test.cpp)\nadd_test(NAME failing_test COMMAND failing_test)\n',
    'failing/test.cpp': 'int main() { return 1; }\n',
    'untested/CMakeLists.txt': 'project(untested LANGUAGES CXX)\nadd_library(untested STATIC untested.cpp)\n',
    'untested/untested.cpp': 'int two() { return 2; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('test--ctest', { timeout: 120_000 }, () => {
    it('runs for a member whose folder adds tests, after that member builds, and for no other member', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        const { tasks } = taskGraph(graph, ['passing'], ['test']);

        expect(Object.keys(tasks).toSorted()).toStrictEqual(['passing:build--cmake', 'passing:test', 'passing:test--ctest']);
        expect(
            Object.keys(graph.nodes)
                .filter((name) => 'test--ctest' in (graph.nodes[name]?.data.targets ?? {}))
                .toSorted(),
        ).toStrictEqual(['failing', 'passing']);
    });

    it('passes for a member whose tests pass, running no test of another member', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);
        await workspace.run(graph, 'passing', 'build--cmake');

        const { success } = await workspace.run(graph, 'passing', 'test--ctest');

        expect(success).toBe(true);
    });

    it('fails for a member whose test fails', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);
        await workspace.run(graph, 'failing', 'build--cmake');

        const { success } = await workspace.run(graph, 'failing', 'test--ctest');

        expect(success).toBe(false);
    });

    it('fails where the test preset selects none of the member tests, which ctest alone reports as success', async ({ workspace }) => {
        const filtered = {
            ...presets,
            testPresets: [
                { name: 'default', configurePreset: 'default', filter: { include: { name: 'absent' } } },
                { name: 'coverage', configurePreset: 'coverage' },
            ],
        };
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': JSON.stringify(filtered) }).graph(cmake);
        await workspace.run(graph, 'passing', 'build--cmake');

        const { success } = await workspace.run(graph, 'passing', 'test--ctest');

        expect(success).toBe(false);
    });

    it('writes the lcov report of the member test binaries to <coveragePath>/lcov.info under configuration coverage', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake, { coveragePath: '.artifacts/coverage/{projectName}' });
        await workspace.run(graph, 'passing', 'build--cmake', 'coverage');

        const { success } = await workspace.run(graph, 'passing', 'test--ctest', 'coverage');

        const declared = outputs(graph, 'passing', 'test--ctest', 'coverage');
        const report = readFileSync(join(workspace.root, '.artifacts/coverage/passing/lcov.info'), 'utf-8');
        expect(success).toBe(true);
        expect(declared).toStrictEqual(['.artifacts/coverage/passing/lcov.info']);
        expect(report).toContain(`SF:${join(workspace.root, 'passing/test.cpp')}\n`);
        expect(report).toMatch(/^DA:1,1$/mu);
    });
});
