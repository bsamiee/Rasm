import { join } from 'node:path';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { refused } from '@rasm/testing/errors';
import { it, pathed, unaggregated, unportable } from '@rasm/testing/nx';
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
            { name: 'release', inherits: 'default' },
        ],
        testPresets: [
            { name: 'default', configurePreset: 'default' },
            { name: 'release', configurePreset: 'release' },
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
                name: 'release',
                steps: [
                    { type: 'configure', name: 'release' },
                    { type: 'test', name: 'release' },
                ],
            },
        ],
    }),
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\nadd_subdirectory(docs)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\nadd_executable(fixture_core_test core.cpp)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\n',
    'apps/tool/CMakeLists.txt': `project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\ninstall(TARGETS fixture_tool)\nset(CPACK_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackConfig.cmake")\nset(CPACK_SOURCE_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackSourceConfig.cmake")\ninclude(CPack)\n`,
    'apps/tool/vcpkg.json': '{ "name": "fixture-tool", "dependencies": [] }\n',
    'package.json': '{ "name": "repo", "private": true, "workspaces": ["libs/*"] }',
    'libs/core/package.json': '{ "name": "fixture_core", "nx": { "targets": { "build--cmake": { "inputs": ["default", "{projectRoot}/data/**/*"] } } } }',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('inferred projects', () => {
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

    it('gives the top-level project() no build target, as it aggregates the members of its tree', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_library(fixture STATIC libs/core/core.cpp)\nadd_subdirectory(libs/core)\n' }).graph(cmake);

        expect(Object.keys(graph.nodes).toSorted()).toStrictEqual(['fixture_core', 'fixture_tool']);
    });

    it('folds a project() call inside a member folder into that member, as a consumer tree the member tests', async ({ workspace }) => {
        const graph = await workspace.write({ ...tree, 'apps/tool/consumer/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(consumer LANGUAGES CXX)\nadd_executable(consumer main.cpp)\n' }).graph(cmake);

        expect(Record.map(graph.nodes, ({ data }) => data.root)).toStrictEqual({ fixture_core: 'libs/core', fixture_tool: 'apps/tool' });
    });

    it('infers a member whose folder name starts with two dots as its own project, owning its sources and the links naming it', async ({ workspace }) => {
        const graph = await workspace
            .write({
                'cpp/CMakePresets.json': presets,
                'cpp/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(..core)\nadd_subdirectory(tool)\n',
                'cpp/..core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
                'cpp/..core/core.cpp': 'int one() { return 1; }\n',
                'cpp/tool/CMakeLists.txt': 'project(fixture_tool LANGUAGES CXX)\ninclude(..cmake/flags.cmake)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\n',
                'cpp/tool/..cmake/flags.cmake': 'set(CMAKE_CXX_STANDARD 20)\n',
                'cpp/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
            })
            .graph(cmake);

        expect({
            roots: Record.map(graph.nodes, ({ data }) => data.root),
            sources: graph.nodes['fixture_core']?.data.namedInputs?.['default']?.filter(Predicate.isString).toSorted(),
            links: graph.dependencies['fixture_tool'],
        }).toStrictEqual({
            roots: { fixture_core: 'cpp/..core', fixture_tool: 'cpp/tool' },
            sources: ['{projectRoot}/**/*', '{workspaceRoot}/cpp/..core/CMakeLists.txt', '{workspaceRoot}/cpp/CMakeLists.txt', '{workspaceRoot}/cpp/CMakePresets.json'],
            links: [{ source: 'fixture_tool', target: 'fixture_core', type: 'static' }],
        });
    });

    it('infers the same projects, targets, and dependencies with no cmake, ninja, or compiler on PATH', async ({ workspace }) => {
        pathed(join(workspace.root, '.bin'), {});
        const bare = await workspace.write(rich).graph(cmake, { outputPath: '.artifacts/cmake/{projectName}' });
        vi.unstubAllEnvs();

        const tooled = await workspace.graph(cmake, { outputPath: '.artifacts/cmake/{projectName}' });

        expect(Object.keys(bare.nodes).toSorted()).toStrictEqual(['fixture_core', 'fixture_tool']);
        expect({ nodes: bare.nodes, dependencies: bare.dependencies }).toStrictEqual({ nodes: tooled.nodes, dependencies: tooled.dependencies });
    });

    it('refuses configure presets inheriting each other in a cycle, as cmake does', async ({ workspace }) => {
        const cyclic = JSON.stringify({
            version: 10,
            configurePresets: [
                { name: 'default', inherits: 'base', generator: 'Ninja' },
                { name: 'base', inherits: 'default' },
            ],
            workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
        });

        const read = workspace.write({ ...tree, 'CMakePresets.json': cyclic }).graph(cmake);

        await expect(read).rejects.toSatisfy(refused(/CMakePresets\.json lists configure presets inheriting each other in a cycle/u));
    });

    it('infers nothing from a CMakePresets.json with no CMakeLists.txt beside it', async ({ workspace }) => {
        const graph = await workspace.write({ 'CMakePresets.json': presets, 'lib/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(lib LANGUAGES CXX)\nadd_library(lib STATIC lib.cpp)\n', 'lib/lib.cpp': 'int one() { return 1; }\n' }).graph(cmake);

        expect(graph.nodes).toStrictEqual({});
    });
});

describe('files Nx hands the plugin', () => {
    it('are the presets files alone, so a list file, module, or vcpkg manifest edit is no project configuration change', async ({ workspace }) => {
        workspace.write({ ...rich, 'apps/tool/vcpkg-configuration.json': '{}\n', 'apps/tool/consumer/CMakePresets.json': presets });

        const matched = await globWithWorkspaceContext(workspace.root, [createNodes[0]]);

        expect(matched.toSorted()).toStrictEqual(['CMakePresets.json', 'apps/tool/consumer/CMakePresets.json']);
    });
});

describe('hashing', () => {
    it('hashes the member folder and each list file cmake reads for it, sibling members left to their own projects', async ({ workspace }) => {
        const graph = await workspace.write(tree).graph(cmake);

        expect(graph.nodes['fixture_tool']?.data.namedInputs?.['default']?.filter(Predicate.isString).toSorted()).toStrictEqual(['{projectRoot}/**/*', '{workspaceRoot}/CMakeLists.txt', '{workspaceRoot}/CMakePresets.json', '{workspaceRoot}/apps/tool/CMakeLists.txt', '{workspaceRoot}/cmake/standard.cmake']);
    });
});

describe('graph laws', () => {
    it('hashes every tool input in its verb and forwards the verb to each tool, package.json overrides included', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cmake, { outputPath: '.artifacts/cmake/{projectName}' });

        expect(graph.nodes['fixture_core']?.data.targets?.['build--cmake']?.inputs).toContain('{projectRoot}/data/**/*');
        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('states every root, input, output, and working folder as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cmake, { outputPath: '.artifacts/cmake/{projectName}' });

        expect(Object.keys(graph.nodes['fixture_tool']?.data.targets ?? {})).toStrictEqual(expect.arrayContaining(['check--clang-tidy', 'pack--cpack', 'upgrade--vcpkg', 'install--cmake']));
        expect(unportable(graph)).toStrictEqual([]);
    });
});
