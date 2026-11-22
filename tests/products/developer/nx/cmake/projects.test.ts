import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it, unaggregated, unportable } from '@rasm/testing/nx';
import { Predicate, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const presets = JSON.stringify({
    version: 10,
    configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
    workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
});
const tree = {
    'CMakePresets.json': presets,
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\nadd_subdirectory(docs)\n',
    'cmake/standard.cmake': 'set(CMAKE_CXX_STANDARD 20)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
    'libs/core/core.cpp': 'int one() { return 1; }\n',
    'apps/tool/CMakeLists.txt': `project(fixture_tool LANGUAGES CXX)\ninclude(\${PROJECT_SOURCE_DIR}/../../cmake/standard.cmake)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\n`,
    'apps/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
    'docs/CMakeLists.txt': 'project(fixture_docs NONE)\n',
};
const rich = {
    ...tree,
    'CMakePresets.json': JSON.stringify({
        version: 10,
        configurePresets: [
            { name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}`, cacheVariables: { CMAKE_CXX_CLANG_TIDY: 'clang-tidy' } },
            { name: 'coverage', inherits: 'default' },
        ],
        testPresets: [
            { name: 'default', configurePreset: 'default' },
            { name: 'coverage', configurePreset: 'coverage' },
        ],
        packagePresets: [{ name: 'default', configurePreset: 'default', generators: ['TGZ'] }],
        workflowPresets: [
            {
                name: 'default',
                steps: [
                    { type: 'configure', name: 'default' },
                    { type: 'test', name: 'default' },
                ],
            },
            {
                name: 'coverage',
                steps: [
                    { type: 'configure', name: 'coverage' },
                    { type: 'test', name: 'coverage' },
                ],
            },
        ],
    }),
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\nadd_subdirectory(docs)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\nadd_executable(fixture_core_test core.cpp)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\n',
    'apps/tool/CMakeLists.txt': `project(fixture_tool LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\ninstall(TARGETS fixture_tool)\nset(CPACK_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackConfig.cmake")\nset(CPACK_SOURCE_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackSourceConfig.cmake")\ninclude(CPack)\n`,
    'apps/tool/vcpkg.json': '{ "name": "fixture-tool", "dependencies": [] }\n',
    'package.json': '{ "name": "repo", "private": true, "workspaces": ["libs/*"] }',
    'libs/core/package.json': '{ "name": "fixture_core", "nx": { "targets": { "build--cmake": { "inputs": ["default", "{projectRoot}/data/**/*"] } } } }',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('inferred projects', { timeout: 120_000 }, () => {
    it('infers a project for each project() call that builds something, named by the call and rooted at its folder', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        expect(Record.map(graph.nodes, ({ data }) => data.root)).toStrictEqual({ fixture_core: 'libs/core', fixture_tool: 'apps/tool' });
    });

    it('infers a project that builds nothing where it owns a vcpkg manifest, holding only its upgrade', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'docs/vcpkg.json': '{ "name": "fixture-docs", "dependencies": [] }\n' }).graph(cmake);

        expect(Object.keys(graph.nodes['fixture_docs']?.data.targets ?? {}).toSorted()).toStrictEqual(['upgrade', 'upgrade--vcpkg']);
    });

    it('gives upgrade--vcpkg to the member nearest the manifest, run from that member on the manifest folder', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'apps/tool/third_party/vcpkg.json': '{ "name": "tool-deps", "dependencies": [] }\n' }).graph(cmake);

        expect(graph.nodes['fixture_tool']?.data.targets?.['upgrade--vcpkg']).toMatchObject({
            options: { cwd: 'apps/tool', commands: [{ apply: ['vcpkg', 'x-update-baseline', '--x-manifest-root=third_party', '--x-install-root=../../build/default/vcpkg_installed'] }] },
        });
    });

    it('infers nothing from a CMakePresets.json with no CMakeLists.txt beside it', async ({ workspace }) => {
        const graph = await workspace.write({ 'CMakePresets.json': presets, 'lib/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(lib LANGUAGES CXX)\nadd_library(lib STATIC lib.cpp)\n', 'lib/lib.cpp': 'int one() { return 1; }\n' }).graph(cmake);

        expect(graph.nodes).toStrictEqual({});
    });
});

describe('hashing', { timeout: 120_000 }, () => {
    it('hashes the member folder and each list file cmake reads for it, sibling members left to their own projects', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        expect(graph.nodes['fixture_tool']?.data.namedInputs?.['default']?.filter(Predicate.isString).toSorted()).toStrictEqual(['{projectRoot}/**/*', '{workspaceRoot}/CMakeLists.txt', '{workspaceRoot}/CMakePresets.json', '{workspaceRoot}/apps/tool/CMakeLists.txt', '{workspaceRoot}/cmake/standard.cmake']);
    });
});

describe('graph laws', { timeout: 120_000 }, () => {
    it('hashes every tool input in its verb and forwards the verb to each tool, package.json overrides included', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cmake, { outputPath: '.artifacts/cmake/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' });

        expect(graph.nodes['fixture_core']?.data.targets?.['build--cmake']?.inputs).toContain('{projectRoot}/data/**/*');
        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('states every root, input, output, and working folder as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cmake, { outputPath: '.artifacts/cmake/{projectName}', coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.keys(graph.nodes['fixture_tool']?.data.targets ?? {})).toStrictEqual(expect.arrayContaining(['check--clang-tidy', 'pack--cpack', 'upgrade--vcpkg', 'install--cmake']));
        expect(unportable(graph)).toStrictEqual([]);
    });
});
