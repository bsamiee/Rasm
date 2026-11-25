import { existsSync, globSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, outputs } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const workflows = (names: readonly string[]) =>
    JSON.stringify({
        version: 10,
        configurePresets: names.map((name) => ({ name, generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}`, cacheVariables: { CMAKE_INSTALL_PREFIX: `\${sourceDir}/stage` } })),
        buildPresets: names.map((name) => ({ name, configurePreset: name })),
        workflowPresets: names.map((name) => ({
            name,
            steps: [
                { type: 'configure', name },
                { type: 'build', name },
            ],
        })),
    });
const tree = {
    'nested/proj/CMakePresets.json': workflows(['default', 'release']),
    'nested/proj/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\n',
    'nested/proj/libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
    'nested/proj/libs/core/core.cpp': 'int one() { return 1; }\n',
    'nested/proj/apps/tool/CMakeLists.txt': 'project(fixture_tool LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\ninstall(TARGETS fixture_tool)\n',
    'nested/proj/apps/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('build--cmake', () => {
    it.for([
        { configuration: undefined, folder: 'nested/proj/build/default', member: 'nested/proj/build/default/apps/tool', executable: 'nested/proj/build/default/apps/tool/fixture_tool' },
        { configuration: 'release', folder: 'nested/proj/build/release', member: 'nested/proj/build/release/apps/tool', executable: 'nested/proj/build/release/apps/tool/fixture_tool' },
    ])('writes the executable inside the member build folder it declares under configuration $configuration, and nothing outside the build tree of that workflow', async ({ configuration, folder, member, executable }, { workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);
        const before = workspace.files();

        const { success } = await workspace.run(graph, 'fixture_tool', 'build--cmake', configuration);

        expect([success, outputs(graph, 'fixture_tool', 'build--cmake', configuration), existsSync(join(workspace.root, executable)), [...workspace.files().difference(before)].filter((file) => !file.startsWith(folder))]).toStrictEqual([true, [member], true, []]);
    });

    it('hands cmake a preset name holding shell syntax as one word', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'nested/proj/CMakePresets.json': workflows(['x && touch pwned']) }).graph(cmake);

        const { success } = await workspace.run(graph, 'fixture_core', 'build--cmake');

        expect(success).toBe(true);
        expect(globSync('**/pwned', { cwd: workspace.root })).toStrictEqual([]);
    });
});

describe('install--cmake', () => {
    it('installs nothing for a member whose folder states no install rules', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        const { success } = await workspace.run(graph, 'fixture_core', 'install--cmake');

        expect([success, existsSync(join(workspace.root, 'nested/proj/stage'))]).toStrictEqual([true, false]);
    });

    it('builds the member and installs its executable under bin of the prefix its configure preset states', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        const { success } = await workspace.run(graph, 'fixture_tool', 'install--cmake');

        expect(success).toBe(true);
        expect(existsSync(join(workspace.root, 'nested/proj/stage/bin/fixture_tool'))).toBe(true);
    });
});
