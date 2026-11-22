import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, pathed } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const tree = {
    'CMakePresets.json': JSON.stringify({
        version: 10,
        configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
        workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
    }),
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\n',
    'cmake/tests.cmake': '',
    'libs/core/CMakeLists.txt': `project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\ninclude(\${PROJECT_SOURCE_DIR}/../../cmake/tests.cmake)\n`,
    'libs/core/core.cpp': 'int one() { return 1; }\n',
    'libs/core/test.cpp': 'int main() { return 0; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('stored readings', { timeout: 120_000 }, () => {
    it('serves the stored reading while no file cmake read changes, with cmake gone from PATH', async ({ workspace }) => {
        await workspace.write(tree).graph(cmake);
        pathed(join(workspace.root, '.bin'), {});

        const graph = await workspace.graph(cmake);

        expect(graph.nodes['fixture_core']?.data.targets).toHaveProperty('build--cmake');
    });

    it('reads the tree again after a member list file changes', async ({ workspace }) => {
        const before = await workspace.write(tree).graph(cmake);
        workspace.write({ 'libs/core/CMakeLists.txt': `project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\nadd_executable(core_test \${PROJECT_SOURCE_DIR}/test.cpp)\nadd_test(NAME core_test COMMAND core_test)\n` });

        const after = await workspace.graph(cmake);

        expect(before.nodes['fixture_core']?.data.targets).not.toHaveProperty('test--ctest');
        expect(after.nodes['fixture_core']?.data.targets).toHaveProperty('test--ctest');
    });

    it('reads the tree again after a file a list file includes changes, though no plugin glob matches it', async ({ workspace }) => {
        const before = await workspace.write(tree).graph(cmake);
        workspace.write({ 'cmake/tests.cmake': `add_executable(core_test \${PROJECT_SOURCE_DIR}/test.cpp)\nadd_test(NAME core_test COMMAND core_test)\n` });

        const after = await workspace.graph(cmake);

        expect(before.nodes['fixture_core']?.data.targets).not.toHaveProperty('test--ctest');
        expect(after.nodes['fixture_core']?.data.targets).toHaveProperty('test--ctest');
    });
});
