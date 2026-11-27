import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, taskGraph } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const presets = {
    version: 10,
    configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
    buildPresets: [{ name: 'default', configurePreset: 'default' }],
    testPresets: [{ name: 'default', configurePreset: 'default' }],
    workflowPresets: [
        {
            name: 'default',
            steps: [
                { type: 'configure', name: 'default' },
                { type: 'build', name: 'default' },
                { type: 'test', name: 'default' },
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

describe('test--ctest', () => {
    it('runs for each member after that member builds, running the build of no other member', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        const { tasks } = taskGraph(graph, ['passing'], ['test']);

        expect(Object.keys(tasks).toSorted()).toStrictEqual(['passing:build--cmake', 'passing:test', 'passing:test--ctest']);
        expect(
            Object.keys(graph.nodes)
                .filter((name) => 'test--ctest' in (graph.nodes[name]?.data.targets ?? {}))
                .toSorted(),
        ).toStrictEqual(['failing', 'passing', 'untested']);
    });

    it.for([
        { case: 'adds no tests', member: 'untested', tests: presets.testPresets, passed: true },
        { case: 'holds a passing test', member: 'passing', tests: presets.testPresets, passed: true },
        { case: 'holds a failing test', member: 'failing', tests: presets.testPresets, passed: false },
        { case: 'holds tests its test preset filters out, stating no tests an error', member: 'passing', tests: [{ name: 'default', configurePreset: 'default', filter: { include: { name: 'absent' } }, execution: { noTestsAction: 'error' } }], passed: false },
    ])('decides ($passed) a built member that $case, running no test of another member', async ({ member, tests, passed }, { workspace }) => {
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': JSON.stringify({ ...presets, testPresets: tests }) }).graph(cmake);
        const built = await workspace.run(graph, member, 'build--cmake');

        const { success } = await workspace.run(graph, member, 'test--ctest');

        expect([built.success, success]).toStrictEqual([true, passed]);
    });
});
