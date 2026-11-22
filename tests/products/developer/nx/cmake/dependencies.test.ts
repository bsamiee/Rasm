import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { it } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cmake = { createNodes, createDependencies };
const members = {
    'CMakePresets.json': JSON.stringify({
        version: 10,
        configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
        workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
    }),
    'clients/CMakeLists.txt': 'project(clients LANGUAGES CXX)\nadd_library(clients STATIC clients.cpp)\n',
    'clients/clients.cpp': 'int one() { return 1; }\n',
    'core/CMakeLists.txt': 'project(core LANGUAGES CXX)\nadd_library(core STATIC core.cpp)\n',
    'core/core.cpp': 'int one();\n\nint two() { return one() + 1; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('link edges', { timeout: 120_000 }, () => {
    it('makes a link the member list file states a static edge from that file', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...members,
                'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(clients)\nadd_subdirectory(core)\n',
                'core/CMakeLists.txt': 'project(core LANGUAGES CXX)\nadd_library(core STATIC core.cpp)\ntarget_link_libraries(core PRIVATE clients)\n',
            })
            .graph(cmake);

        expect(graph.dependencies['core']).toStrictEqual([{ source: 'core', target: 'clients', type: 'static' }]);
    });

    it('makes a link an ancestor list file states an implicit edge, as its file lies outside the source project', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...members,
                'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(clients)\nadd_subdirectory(core)\ntarget_link_libraries(core PRIVATE clients)\n',
            })
            .graph(cmake);

        expect(graph.dependencies['core']).toStrictEqual([{ source: 'core', target: 'clients', type: 'implicit' }]);
    });
});
