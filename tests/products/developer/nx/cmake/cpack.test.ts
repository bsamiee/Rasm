import { readdirSync } from 'node:fs';
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
        { name: 'other', inherits: 'default' },
    ],
    packagePresets: [{ name: 'default', configurePreset: 'default', generators: ['TGZ'] }],
    workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
};
const tree = {
    'CMakePresets.json': JSON.stringify(presets),
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\ninstall(TARGETS fixture_core)\n',
    'libs/core/core.cpp': 'int one() { return 1; }\n',
    'apps/tool/CMakeLists.txt': [
        'project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)',
        'add_executable(fixture_tool main.cpp)',
        'target_link_libraries(fixture_tool PRIVATE fixture_core)',
        'install(TARGETS fixture_tool)',
        `set(CPACK_PACKAGE_NAME \${PROJECT_NAME})`,
        `set(CPACK_PACKAGE_VERSION \${PROJECT_VERSION})`,
        `set(CPACK_INSTALL_CMAKE_PROJECTS "\${PROJECT_BINARY_DIR};\${PROJECT_NAME};ALL;/")`,
        `set(CPACK_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackConfig.cmake")`,
        `set(CPACK_SOURCE_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackSourceConfig.cmake")`,
        'include(CPack)',
        '',
    ].join('\n'),
    'apps/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('release of a packaged member', () => {
    it('releases a member whose project() states VERSION, attaching after it packs, and leaves an installing member stating none', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        const { tasks } = taskGraph(graph, ['fixture_tool'], ['attach']);

        expect(graph.nodes['fixture_tool']?.data).toMatchObject({ tags: ['release'], release: { version: { versionActions: '@rasm/nx-cmake/release/version-actions' } } });
        expect(Object.keys(tasks).toSorted()).toStrictEqual(['fixture_tool:attach', 'fixture_tool:attach--gh', 'fixture_tool:pack--cpack']);
        expect(graph.nodes['fixture_core']?.data.targets).toHaveProperty('install--cmake');
        expect(graph.nodes['fixture_core']?.data.tags).not.toContain('release');
        expect(graph.nodes['fixture_core']?.data.targets).not.toHaveProperty('pack--cpack');
    });

    it('packs no member where the package preset configures a tree no workflow preset builds', async ({ workspace }) => {
        const elsewhere = { ...presets, packagePresets: [{ name: 'other', configurePreset: 'other', generators: ['TGZ'] }] };
        const graph = await workspace.write({ ...tree, 'CMakePresets.json': JSON.stringify(elsewhere) }).graph(cmake);

        expect(graph.nodes['fixture_tool']?.data.targets).toHaveProperty('install--cmake');
        expect(graph.nodes['fixture_tool']?.data.targets).not.toHaveProperty('pack--cpack');
    });
});

describe('pack--cpack', () => {
    it.for([
        { case: 'packages in the member build folder without outputPath', options: {}, folder: 'build/default/apps/tool/packages' },
        { case: '<outputPath>/cpack with outputPath', options: { outputPath: '.artifacts/cmake/{projectName}' }, folder: '.artifacts/cmake/fixture_tool/cpack' },
    ])('writes the archive to $case, the folder it declares', async ({ options, folder }, { workspace }) => {
        const graph = await workspace.write(tree).graph(cmake, options);

        const { success } = await workspace.run(graph, 'fixture_tool', 'pack--cpack');

        expect(success).toBe(true);
        expect(outputs(graph, 'fixture_tool', 'pack--cpack')).toStrictEqual([folder]);
        expect(readdirSync(join(workspace.root, folder))).toContainEqual(expect.stringMatching(/^fixture_tool-0\.1\.0-.+\.tar\.gz$/u));
    });
});
