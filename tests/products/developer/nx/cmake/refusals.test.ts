import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { refused } from '@rasm/testing/errors';
import { it, pathed, refusing } from '@rasm/testing/nx';
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
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(libs/core)\n',
    'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
    'libs/core/core.cpp': 'int one() { return 1; }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('cmake refusals', { timeout: 120_000 }, () => {
    it.for(refusing('cmake'))('refuses the presets with the command and $case of cmake', async ({ scripts, refusal }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), scripts);

        await expect(workspace.write(tree).graph(cmake)).rejects.toSatisfy(refused(refusal));
    });

    it('refuses the presets with the list file and line cmake cannot parse', async ({ workspace }) => {
        const pending = workspace.write({ ...tree, 'libs/core/CMakeLists.txt': 'project(fixture_core LANGUAGES CXX\n' }).graph(cmake);

        await expect(pending).rejects.toSatisfy(refused(/^cmake --preset default -S \. exits 1: CMake Error at libs\/core\/CMakeLists\.txt:1:\n {2}Parse error\./u));
    });
});
