import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, unportable } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const tree = {
    'nested/proj/CMakePresets.json': JSON.stringify({
        version: 10,
        configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/../../out/\${presetName}` }],
        testPresets: [{ name: 'default', configurePreset: 'default' }],
        workflowPresets: [
            {
                name: 'default',
                steps: [
                    { type: 'configure', name: 'default' },
                    { type: 'test', name: 'default' },
                ],
            },
        ],
    }),
    'nested/proj/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\ninclude(../shared/standard.cmake)\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\n',
    'nested/shared/standard.cmake': 'set(CMAKE_CXX_STANDARD 20)\n',
    'nested/proj/libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\nadd_executable(fixture_core_test core.cpp)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\n',
    'nested/proj/libs/core/core.cpp': 'int main() { return 0; }\n',
    'nested/proj/apps/tool/CMakeLists.txt': 'project(fixture_tool LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\ninstall(TARGETS fixture_tool)\n',
    'nested/proj/apps/tool/main.cpp': 'int main() { return 0; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('Windows path separators', { timeout: 120_000 }, () => {
    it('states roots, inputs, outputs, and working folders as workspace-relative posix paths', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['fixture_core', 'fixture_tool']);
        expect(unportable(graph)).toStrictEqual([]);
    });

    it('keeps the static edge a member list file states', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        expect(graph.dependencies['fixture_tool']).toStrictEqual([{ source: 'fixture_tool', target: 'fixture_core', type: 'static' }]);
    });
});
