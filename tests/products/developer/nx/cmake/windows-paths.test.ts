import { type CreateNodesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('node:path'), async (actual) => {
    const { windows } = await import('@rasm/testing/path');
    return windows(await actual());
});
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = {
    'nested/proj/CMakePresets.json': JSON.stringify({
        version: 10,
        configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
        buildPresets: [{ name: 'default', configurePreset: 'default' }],
        testPresets: [{ name: 'default', configurePreset: 'default' }],
        workflowPresets: [{ name: 'default', steps: ['configure', 'build', 'test'].map((type) => ({ type, name: 'default' })) }],
    }),
    'nested/proj/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\n',
    'nested/proj/libs/core/CMakeLists.txt': 'project(fixture_core VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\n',
    'nested/proj/libs/core/core.cpp': 'int one() { return 1; }\n',
    'nested/proj/apps/tool/CMakeLists.txt': 'project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\n',
    'nested/proj/apps/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cmake-windows-paths');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({ nxJsonConfiguration: { namedInputs: { default: ['{projectRoot}/**/*'] } }, workspaceRoot: tempFs.tempDir }));

describe('@rasm/nx-cmake under Windows path separators', { timeout: 60_000 }, () => {
    it('should hand Nx workspace-relative posix roots, inputs, and outputs', async ({ context }) => {
        const nodes = await createNodes[1](Object.keys(files), undefined, context);

        const projects = nodes[0]?.[1].projects;
        expect(Object.keys({ ...projects })).toStrictEqual(['nested/proj/libs/core', 'nested/proj/apps/tool']);
        expect(projects?.['nested/proj/apps/tool']?.namedInputs?.['default']).toStrictEqual([
            '{projectRoot}/**/*',
            '{workspaceRoot}/nested/proj/CMakeLists.txt',
            '{workspaceRoot}/nested/proj/apps/tool/CMakeLists.txt',
            '{workspaceRoot}/nested/proj/CMakePresets.json',
            { runtime: 'node -p process.platform+process.arch' },
        ]);
        expect(projects?.['nested/proj/apps/tool']?.targets?.['build--cmake']?.options).toMatchObject({ cwd: 'nested/proj', outputs: ['{workspaceRoot}/nested/proj/build/default/apps/tool/fixture_tool'] });
    });

    it('should name the workspace-relative posix list file as the source file of an edge', async ({ tempFs, context }) => {
        const nodes = await createNodes[1](Object.keys(files), undefined, context);

        const result = await createDependencies(
            {},
            {
                workspaceRoot: tempFs.tempDir,
                projects: Record.fromEntries(Record.collect({ ...nodes[0]?.[1].projects }, (root, project) => [project.name ?? root, { ...project, root }] as const)),
                externalNodes: {},
                fileMap: { projectFileMap: {}, nonProjectFiles: [] },
                filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
                nxJsonConfiguration: {},
            },
        );

        expect(result).toStrictEqual([{ source: 'fixture_tool', target: 'fixture_core', type: DependencyType.static, sourceFile: 'nested/proj/apps/tool/CMakeLists.txt' }]);
    });
});
