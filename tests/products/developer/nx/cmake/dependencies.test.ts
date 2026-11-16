import process from 'node:process';
import { type CreateDependenciesContext, DependencyType } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import { TempFs } from '@rasm/testing/temp-fs';
import { Record } from 'effect';
import { describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

const presets = JSON.stringify({
    version: 10,
    configurePresets: [{ name: 'default', generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` }],
    workflowPresets: [{ name: 'default', steps: [{ type: 'configure', name: 'default' }] }],
});

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cmake-dependencies');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        return tempFs;
    })
    .extend('contextWith', ({ tempFs }) => async (files: Readonly<Record<string, string>>, projects: Readonly<Record<string, string>>) => {
        await tempFs.createFiles({ 'CMakePresets.json': presets, ...files });
        await createNodes[1](['CMakePresets.json', ...Object.keys(files)], undefined, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
        return {
            workspaceRoot: tempFs.tempDir,
            projects: Record.map(projects, (root, name) => ({ name, root })),
            externalNodes: {},
            fileMap: { projectFileMap: {}, nonProjectFiles: [] },
            filesToProcess: { projectFileMap: {}, nonProjectFiles: [] },
            nxJsonConfiguration: {},
        } satisfies CreateDependenciesContext;
    });

describe('@rasm/nx-cmake/plugin/dependencies', { timeout: 60_000 }, () => {
    it('records an edge as static when the source project owns the build file', async ({ contextWith }) => {
        const context = await contextWith(
            {
                'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(clients)\nadd_subdirectory(app)\n',
                'clients/CMakeLists.txt': 'project(clients LANGUAGES CXX)\nadd_library(clients STATIC clients.cpp)\n',
                'clients/clients.cpp': 'int one() { return 1; }\n',
                'app/CMakeLists.txt': 'project(app LANGUAGES CXX)\nadd_executable(app main.cpp)\ntarget_link_libraries(app PRIVATE clients)\n',
                'app/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
            },
            { app: 'app', clients: 'clients' },
        );

        const result = await createDependencies({}, context);

        expect(result).toStrictEqual([
            expect.objectContaining({
                source: 'app',
                target: 'clients',
                type: DependencyType.static,
                sourceFile: 'app/CMakeLists.txt',
            }),
        ]);
    });

    it('records a nested project that owns its build file as static', async ({ contextWith }) => {
        const context = await contextWith(
            {
                'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(clients)\nadd_subdirectory(api-checker)\n',
                'clients/CMakeLists.txt': 'project(clients LANGUAGES CXX)\nadd_library(clients STATIC clients.cpp)\n',
                'clients/clients.cpp': 'int one() { return 1; }\n',
                'api-checker/CMakeLists.txt': 'project(api_checker LANGUAGES CXX)\nadd_library(api_checker STATIC checker.cpp)\nadd_subdirectory(core)\n',
                'api-checker/checker.cpp': 'int two() { return 2; }\n',
                'api-checker/core/CMakeLists.txt': 'project(api_checker_core LANGUAGES CXX)\nadd_library(api_checker_core STATIC core.cpp)\ntarget_link_libraries(api_checker_core PRIVATE clients)\n',
                'api-checker/core/core.cpp': 'int one();\n\nint three() { return one() + 2; }\n',
            },
            { api_checker: 'api-checker', api_checker_core: 'api-checker/core', clients: 'clients' },
        );

        const result = await createDependencies({}, context);

        expect(result[0]).toStrictEqual(expect.objectContaining({ type: DependencyType.static, sourceFile: 'api-checker/core/CMakeLists.txt' }));
    });

    it('records an ancestor-configured project as implicit, since Nx rejects a sourceFile outside the source project', async ({ contextWith }) => {
        const context = await contextWith(
            {
                'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nadd_subdirectory(clients)\nadd_subdirectory(core)\ntarget_link_libraries(core PRIVATE clients)\n',
                'clients/CMakeLists.txt': 'project(clients LANGUAGES CXX)\nadd_library(clients STATIC clients.cpp)\n',
                'clients/clients.cpp': 'int one() { return 1; }\n',
                'core/CMakeLists.txt': 'project(core LANGUAGES CXX)\nadd_library(core STATIC core.cpp)\n',
                'core/core.cpp': 'int one();\n\nint two() { return one() + 1; }\n',
            },
            { core: 'core', clients: 'clients' },
        );

        const result = await createDependencies({}, context);

        expect(result).toStrictEqual([
            {
                source: 'core',
                target: 'clients',
                type: DependencyType.implicit,
            },
        ]);
        expect(result[0]).not.toHaveProperty('sourceFile');
    });
});
