import { execSync } from 'node:child_process';
import { existsSync, globSync } from 'node:fs';
import { join, matchesGlob } from 'node:path';
import process from 'node:process';
import { type CreateNodesContext, readJsonFile } from '@nx/devkit';
import { createNodes } from '@rasm/nx-cmake';
import { TempFs } from '@rasm/testing/temp-fs';
import { Array, Effect, Record, Schema } from 'effect';
import { describe, expect, onTestFinished, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

const presets = (workflows: readonly string[]) =>
    JSON.stringify({
        version: 10,
        configurePresets: workflows.map((name) => ({ name, generator: 'Ninja', binaryDir: `\${sourceDir}/build/\${presetName}` })),
        buildPresets: workflows.map((name) => ({ name, configurePreset: name })),
        testPresets: workflows.map((name) => ({ name, configurePreset: name })),
        packagePresets: workflows.slice(0, 1).map((name) => ({ name, configurePreset: name, generators: ['TGZ'] })),
        workflowPresets: workflows.map((name) => ({ name, steps: ['configure', 'build', 'test'].map((type) => ({ type, name })) })),
    });

const files = {
    'CMakePresets.json': presets(['default']),
    'CMakeLists.txt': `cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nlist(APPEND CMAKE_PROJECT_INCLUDE "\${CMAKE_CURRENT_SOURCE_DIR}/cmake/package.cmake")\nadd_subdirectory(libs/core)\nadd_subdirectory(apps/tool)\n`,
    'cmake/package.cmake': `set(CPACK_PACKAGE_NAME "\${PROJECT_NAME}")\nset(CPACK_INSTALL_CMAKE_PROJECTS "\${PROJECT_BINARY_DIR};\${PROJECT_NAME};ALL;/")\nset(CPACK_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackConfig.cmake")\nset(CPACK_SOURCE_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackSourceConfig.cmake")\ninclude(CPack)\n`,
    'libs/core/CMakeLists.txt': 'project(fixture_core VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\nadd_executable(fixture_core_test core_test.cpp)\ntarget_link_libraries(fixture_core_test PRIVATE fixture_core)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\n',
    'libs/core/core.cpp': 'int one() { return 1; }\n',
    'libs/core/core_test.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
    'apps/tool/CMakeLists.txt': 'project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_core)\ninstall(TARGETS fixture_tool)\n',
    'apps/tool/main.cpp': 'int one();\n\nint main() { return one() == 1 ? 0 : 1; }\n',
};

const Planned = Schema.Struct({ cwd: Schema.String, commands: Schema.Array(Schema.Struct({ command: Schema.String })), outputs: Schema.Array(Schema.String) });

const nested = Record.mapKeys(files, (file) => join('nested/nested/proj', file));

const it = test
    .extend('tempFs', async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('cmake-plugin');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        await tempFs.createFiles(files);
        return tempFs;
    })
    .extend('context', ({ tempFs }): CreateNodesContext => ({
        nxJsonConfiguration: {
            namedInputs: {
                default: ['{projectRoot}/**/*'],
            },
        },
        workspaceRoot: tempFs.tempDir,
    }));

const [glob, createNodesFunction] = createNodes;

describe('@rasm/nx-cmake/plugin', { timeout: 60_000 }, () => {
    it('should create nodes', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);

        expect(nodes).toHaveLength(1);
        expect(nodes[0]?.[0]).toBe('CMakePresets.json');
        expect(Object.keys({ ...nodes[0]?.[1].projects })).toStrictEqual(['libs/core', 'apps/tool']);
        expect(nodes[0]?.[1].projects?.['apps/tool']).toMatchObject({
            root: 'apps/tool',
            name: 'fixture_tool',
            metadata: { technologies: ['cmake'] },
            tags: ['release'],
            release: { version: { versionActions: '@rasm/nx-cmake/release/version-actions' } },
        });
        expect(nodes[0]?.[1].projects?.['libs/core']).toMatchObject({ root: 'libs/core', name: 'fixture_core', metadata: { technologies: ['cmake'] } });
    });

    it('should create the inputs of each member from the files cmake reads', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const projects = nodes[0]?.[1].projects;

        expect(projects?.['apps/tool']?.namedInputs).toMatchInlineSnapshot(`
          {
            "default": [
              "{projectRoot}/**/*",
              "{workspaceRoot}/CMakeLists.txt",
              "{workspaceRoot}/apps/tool/CMakeLists.txt",
              "{workspaceRoot}/cmake/package.cmake",
              "{workspaceRoot}/CMakePresets.json",
              {
                "runtime": "node -p process.platform+process.arch",
              },
            ],
          }
        `);
        expect(projects?.['libs/core']?.namedInputs).toMatchInlineSnapshot(`
          {
            "default": [
              "{projectRoot}/**/*",
              "{workspaceRoot}/CMakeLists.txt",
              "{workspaceRoot}/cmake/package.cmake",
              "{workspaceRoot}/libs/core/CMakeLists.txt",
              "{workspaceRoot}/CMakePresets.json",
              {
                "runtime": "node -p process.platform+process.arch",
              },
            ],
          }
        `);
        expect(projects?.['libs/core']).not.toHaveProperty('tags');
    });

    it('should create the build and attach targets of a packaged member', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const targets = nodes[0]?.[1].projects?.['apps/tool']?.targets;

        expect(targets?.['build']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "build--*",
            ],
            "inputs": [
              "...",
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
          }
        `);
        expect(targets?.['build--cmake']).toMatchInlineSnapshot(`
          {
            "cache": true,
            "executor": "nx:run-commands",
            "inputs": [
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Builds the targets of fixture_tool in the tree its workflow preset configures with cmake --build",
              "help": {
                "command": "cmake --build --help",
                "example": {
                  "args": [
                    "--verbose",
                  ],
                },
              },
              "technologies": [
                "cmake",
              ],
            },
            "options": {
              "commands": [
                {
                  "command": "cmake --preset default -S .",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake --build build/default --preset default --target fixture_tool",
                  "forwardAllArgs": true,
                },
              ],
              "cwd": ".",
              "outputs": [
                "{workspaceRoot}/build/default/apps/tool/fixture_tool",
              ],
              "parallel": false,
            },
            "outputs": [
              "{options.outputs.0}",
            ],
            "parallelism": false,
          }
        `);
        expect(targets?.['attach']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "attach--*",
            ],
            "inputs": [
              "...",
              {
                "dependentTasksOutputFiles": "**/*",
                "transitive": true,
              },
              {
                "externalDependencies": [],
              },
            ],
          }
        `);
        expect(targets?.['attach--gh']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "pack--cpack",
            ],
            "executor": "@rasm/nx-distribution:attach",
            "inputs": [
              {
                "dependentTasksOutputFiles": "**/*",
              },
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Uploads the files pack--cpack wrote to the GitHub release of the version the project's manifest states with gh release upload",
              "help": {
                "command": "gh release upload --help",
                "example": {
                  "options": {
                    "dryRun": true,
                  },
                },
              },
              "technologies": [
                "cmake",
              ],
            },
          }
        `);
    });

    it('should create the pack and install targets of a packaged member', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const targets = nodes[0]?.[1].projects?.['apps/tool']?.targets;

        expect(targets?.['pack']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "pack--*",
            ],
            "inputs": [
              "...",
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
          }
        `);
        expect(targets?.['pack--cpack']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset default -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake --build build/default --preset default --target fixture_tool",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake -E rm -rf build/default/apps/tool/packages",
                "forwardAllArgs": false,
              },
              {
                "command": "cpack --preset default --config apps/tool/CPackConfig.cmake -B apps/tool -D CPACK_OUTPUT_FILE_PREFIX=apps/tool/packages",
                "forwardAllArgs": true,
              },
            ],
            "cwd": ".",
            "parallel": false,
          }
        `);
        expect(targets?.['pack--cpack']?.outputs).toMatchInlineSnapshot(`
          [
            "{workspaceRoot}/build/default/apps/tool/packages",
          ]
        `);
        expect(targets?.['install']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "install--*",
            ],
            "inputs": [
              "...",
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
          }
        `);
        expect(targets?.['install--cmake']).toMatchInlineSnapshot(`
          {
            "executor": "nx:run-commands",
            "inputs": [
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Builds fixture_tool, then installs it under the install prefix its configure preset states with cmake --install",
              "help": {
                "command": "cmake --install --help",
                "example": {
                  "args": [
                    "--verbose",
                  ],
                },
              },
              "technologies": [
                "cmake",
              ],
            },
            "options": {
              "commands": [
                {
                  "command": "cmake --preset default -S .",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake --build build/default --preset default --target fixture_tool",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake --install build/default/apps/tool",
                  "forwardAllArgs": true,
                },
              ],
              "cwd": ".",
              "parallel": false,
            },
            "parallelism": false,
          }
        `);
    });

    it('should create the clean targets of a packaged member', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const targets = nodes[0]?.[1].projects?.['apps/tool']?.targets;

        expect(targets?.['clean']).toMatchInlineSnapshot(`
          {
            "dependsOn": [
              "clean--*",
            ],
            "inputs": [
              "...",
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
          }
        `);
        expect(targets?.['clean--cmake']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              [
                "cmake",
                "--preset",
                "default",
                "-S",
                ".",
              ],
              [
                "cmake",
                "--build",
                "build/default",
                "--preset",
                "default",
                "--target",
                "clean",
              ],
            ],
            "cwd": ".",
            "targets": [
              "build--cmake",
              "pack--cpack",
            ],
          }
        `);
        expect(targets?.['clean--cmake']?.metadata).toMatchInlineSnapshot(`
          {
            "description": "Removes the outputs build--cmake, pack--cpack declare and the build products of fixture_tool with cmake --build --target clean",
            "help": {
              "command": "cmake --build --help",
              "example": {
                "args": [
                  "--verbose",
                ],
              },
            },
            "technologies": [
              "cmake",
            ],
          }
        `);
    });

    it('should create the build, test, and clean targets of a tested member', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const targets = nodes[0]?.[1].projects?.['libs/core']?.targets;

        expect(Object.keys({ ...targets })).toStrictEqual(['build', 'build--cmake', 'test', 'test--ctest', 'clean', 'clean--cmake']);
        expect(targets?.['build--cmake']).toMatchInlineSnapshot(`
          {
            "cache": true,
            "executor": "nx:run-commands",
            "inputs": [
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Builds the targets of fixture_core in the tree its workflow preset configures with cmake --build",
              "help": {
                "command": "cmake --build --help",
                "example": {
                  "args": [
                    "--verbose",
                  ],
                },
              },
              "technologies": [
                "cmake",
              ],
            },
            "options": {
              "commands": [
                {
                  "command": "cmake --preset default -S .",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake --build build/default --preset default --target fixture_core fixture_core_test",
                  "forwardAllArgs": true,
                },
              ],
              "cwd": ".",
              "outputs": [
                "{workspaceRoot}/build/default/libs/core/libfixture_core.a",
                "{workspaceRoot}/build/default/libs/core/fixture_core_test",
              ],
              "parallel": false,
            },
            "outputs": [
              "{options.outputs.0}",
              "{options.outputs.1}",
            ],
            "parallelism": false,
          }
        `);
        expect(targets?.['test--ctest']).toMatchInlineSnapshot(`
          {
            "cache": true,
            "dependsOn": [
              "build--cmake",
            ],
            "executor": "nx:run-commands",
            "inputs": [
              "default",
              "^default",
              {
                "externalDependencies": [],
              },
            ],
            "metadata": {
              "description": "Runs the tests of fixture_core with ctest",
              "help": {
                "command": "ctest --help",
                "example": {
                  "args": [
                    "--verbose",
                  ],
                },
              },
              "technologies": [
                "cmake",
              ],
            },
            "options": {
              "commands": [
                {
                  "command": "cmake --preset default -S .",
                  "forwardAllArgs": false,
                },
                {
                  "command": "ctest --preset default --test-dir build/default/libs/core --no-tests=error --output-junit Testing/default.xml",
                  "forwardAllArgs": true,
                },
              ],
              "cwd": ".",
              "outputs": [
                "{workspaceRoot}/build/default/libs/core/Testing/default.xml",
              ],
              "parallel": false,
            },
            "outputs": [
              "{options.outputs.0}",
            ],
            "parallelism": false,
          }
        `);
        expect(targets?.['clean--cmake']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              [
                "cmake",
                "--preset",
                "default",
                "-S",
                ".",
              ],
              [
                "cmake",
                "--build",
                "build/default",
                "--preset",
                "default",
                "--target",
                "clean",
              ],
            ],
            "cwd": ".",
            "targets": [
              "build--cmake",
              "test--ctest",
            ],
          }
        `);
    });

    it('should write every declared output of build--cmake and nothing outside its build tree', async ({ tempFs, context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);
        const { cwd, commands, outputs } = await Effect.runPromise(Schema.decodeUnknownEffect(Planned)(nodes[0]?.[1].projects?.['libs/core']?.targets?.['build--cmake']?.options));
        const before = globSync('**', { cwd: tempFs.tempDir });

        commands.forEach(({ command }) => {
            execSync(command, { cwd: join(tempFs.tempDir, cwd), stdio: 'ignore' });
        });

        expect(Array.difference(globSync('**', { cwd: tempFs.tempDir }), before).filter((file) => !file.startsWith('build/default/'))).toStrictEqual([]);
        expect(outputs.map((output) => output.replace('{workspaceRoot}/', '')).filter((output) => !existsSync(join(tempFs.tempDir, output)))).toStrictEqual([]);
    });
});

describe('@rasm/nx-cmake/plugin/nodes', { timeout: 60_000 }, () => {
    it('should create nodes based on cmake for nested project root', async ({ tempFs, context }) => {
        await tempFs.createFiles(nested);

        const nodes = await createNodesFunction(Object.keys(nested), undefined, context);

        expect(Object.keys({ ...nodes[0]?.[1].projects })).toStrictEqual(['nested/nested/proj/libs/core', 'nested/nested/proj/apps/tool']);
        expect(nodes[0]?.[1].projects?.['nested/nested/proj/apps/tool']?.targets?.['build--cmake']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset default -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake --build build/default --preset default --target fixture_tool",
                "forwardAllArgs": true,
              },
            ],
            "cwd": "nested/nested/proj",
            "outputs": [
              "{workspaceRoot}/nested/nested/proj/build/default/apps/tool/fixture_tool",
            ],
            "parallel": false,
          }
        `);
    });

    it('should create reports and archives under outputPath if it is specified', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), { outputPath: '.artifacts/cmake/{projectName}' }, context);

        expect(nodes[0]?.[1].projects?.['libs/core']?.targets?.['test--ctest']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset default -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "ctest --preset default --test-dir build/default/libs/core --no-tests=error --output-junit '../../../../.artifacts/cmake/{projectName}/ctest/default.xml'",
                "forwardAllArgs": true,
              },
            ],
            "cwd": ".",
            "outputs": [
              "{workspaceRoot}/.artifacts/cmake/{projectName}/ctest/default.xml",
            ],
            "parallel": false,
          }
        `);
        expect(nodes[0]?.[1].projects?.['apps/tool']?.targets?.['pack--cpack']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset default -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake --build build/default --preset default --target fixture_tool",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake -E rm -rf '.artifacts/cmake/{projectName}/cpack'",
                "forwardAllArgs": false,
              },
              {
                "command": "cpack --preset default --config apps/tool/CPackConfig.cmake -B apps/tool -D 'CPACK_OUTPUT_FILE_PREFIX=../../.artifacts/cmake/{projectName}/cpack'",
                "forwardAllArgs": true,
              },
            ],
            "cwd": ".",
            "parallel": false,
          }
        `);
        expect(nodes[0]?.[1].projects?.['apps/tool']?.targets?.['pack--cpack']?.outputs).toMatchInlineSnapshot(`
          [
            "{workspaceRoot}/.artifacts/cmake/{projectName}/cpack",
          ]
        `);
    });

    it('should not create reports and archives under outputPath if it is not specified', async ({ context }) => {
        const nodes = await createNodesFunction(Object.keys(files), undefined, context);

        expect(nodes[0]?.[1].projects?.['libs/core']?.targets?.['test--ctest']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset default -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "ctest --preset default --test-dir build/default/libs/core --no-tests=error --output-junit Testing/default.xml",
                "forwardAllArgs": true,
              },
            ],
            "cwd": ".",
            "outputs": [
              "{workspaceRoot}/build/default/libs/core/Testing/default.xml",
            ],
            "parallel": false,
          }
        `);
        expect(nodes[0]?.[1].projects?.['apps/tool']?.targets?.['pack--cpack']?.outputs).toMatchInlineSnapshot(`
          [
            "{workspaceRoot}/build/default/apps/tool/packages",
          ]
        `);
    });

    it('should export coverage under coveragePath if it is specified', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'CMakePresets.json': presets(['default', 'coverage']) });

        const nodes = await createNodesFunction(Object.keys(files), { coveragePath: '.artifacts/coverage/{projectName}' }, context);

        expect(nodes[0]?.[1].projects?.['libs/core']?.targets?.['test--ctest']?.outputs).toMatchInlineSnapshot(`
          [
            "{options.outputs.0}",
            "{options.outputs.1}",
          ]
        `);
        expect(nodes[0]?.[1].projects?.['libs/core']?.targets?.['test--ctest']?.configurations).toMatchInlineSnapshot(`
          {
            "coverage": {
              "commands": [
                {
                  "command": "cmake --preset coverage -S .",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake -E rm -rf build/coverage/libs/core/profiles",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake -E make_directory '.artifacts/coverage/{projectName}'",
                  "forwardAllArgs": false,
                },
                {
                  "command": "cmake -E env "LLVM_PROFILE_FILE=\${PWD}/build/coverage/libs/core/profiles/%m-%p.profraw" ctest --preset coverage --test-dir build/coverage/libs/core --output-junit Testing/coverage.xml",
                  "forwardAllArgs": true,
                },
                {
                  "command": "llvm-profdata merge --sparse --output=build/coverage/libs/core/coverage.profdata build/coverage/libs/core/profiles",
                  "forwardAllArgs": false,
                },
                {
                  "command": "llvm-cov export --format=lcov --instr-profile=build/coverage/libs/core/coverage.profdata --object=build/coverage/libs/core/fixture_core_test > '.artifacts/coverage/{projectName}/lcov.info'",
                  "forwardAllArgs": false,
                },
              ],
              "cwd": ".",
              "outputs": [
                "{workspaceRoot}/build/coverage/libs/core/Testing/coverage.xml",
                "{workspaceRoot}/.artifacts/coverage/{projectName}/lcov.info",
              ],
              "parallel": false,
            },
          }
        `);
    });
});

describe('@rasm/nx-cmake - createNodes', { timeout: 60_000 }, () => {
    describe('mergeUserTargetConfigurations', () => {
        it('should merge the inputs a package.json nx.targets row states into its verb target', async ({ tempFs, context }) => {
            await tempFs.createFiles({ 'libs/core/package.json': JSON.stringify({ nx: { targets: { 'build--cmake': { inputs: ['default', '{projectRoot}/data/**/*'] } } } }) });

            const nodes = await createNodesFunction(Object.keys(files), undefined, context);

            expect(nodes[0]?.[1].projects?.['libs/core']?.targets?.['build']?.inputs).toStrictEqual(['...', 'default', '{projectRoot}/data/**/*']);
        });
    });

    describe('config file glob', () => {
        it.each(['CMakePresets.json', 'CMakeLists.txt', 'libs/core/CMakeLists.txt', 'vcpkg.json', 'libs/core/vcpkg.json', 'vcpkg-configuration.json'])('should match %s', (path) => {
            expect(matchesGlob(path, glob)).toBe(true);
        });

        it.each(['libs/core/core.cpp', 'cmake/package.cmake', 'CMakeUserPresets.json', 'package.json'])('should not match %s', (path) => {
            expect(matchesGlob(path, glob)).toBe(false);
        });
    });
});

describe('CMake Analyzer', { timeout: 60_000 }, () => {
    it('should run CMake configure with a file API query for the codemodel and its input files', async ({ tempFs, context }) => {
        await createNodesFunction(Object.keys(files), undefined, context);

        expect(readJsonFile(join(tempFs.tempDir, 'build/default/.cmake/api/v1/query/client-nx-cmake/query.json'))).toStrictEqual({
            requests: [
                { kind: 'codemodel', version: 2 },
                { kind: 'cmakeFiles', version: 1 },
            ],
        });
    });

    it('should handle CMake process failure', async ({ tempFs, context }) => {
        await tempFs.createFiles({ 'libs/core/CMakeLists.txt': 'project(fixture_core VERSION 0.1.0 LANGUAGES CXX\n' });

        await expect(createNodesFunction(Object.keys(files), undefined, context)).rejects.toMatchObject({
            errors: [['CMakePresets.json', expect.objectContaining({ message: 'cmake --preset default -S . exits 1: CMake Error at libs/core/CMakeLists.txt:1:\n  Parse error.  Function missing ending ")".  End of file reached.' })]],
        });
    });

    it('should handle spawn error', async ({ tempFs, context }) => {
        vi.stubEnv('PATH', join(tempFs.tempDir, 'apps'));
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });

        await expect(createNodesFunction(Object.keys(files), undefined, context)).rejects.toMatchObject({
            errors: [['CMakePresets.json', expect.objectContaining({ message: 'NotFound: ChildProcess.spawn (cmake --preset default -S .)' })]],
        });
    });
});

describe('command injection (NXC-4659)', { timeout: 60_000 }, () => {
    it('should pass a preset name as a literal argument, never through a shell', async ({ tempFs, context }) => {
        const malicious = 'x && touch pwned';
        await tempFs.createFiles({ 'CMakePresets.json': presets([malicious]) });

        const nodes = await createNodesFunction(Object.keys(files), undefined, context);

        await expect(tempFs.readFile('pwned')).rejects.toThrow('ENOENT');
        expect(nodes[0]?.[1].projects?.['apps/tool']?.targets?.['build--cmake']?.options).toMatchInlineSnapshot(`
          {
            "commands": [
              {
                "command": "cmake --preset 'x && touch pwned' -S .",
                "forwardAllArgs": false,
              },
              {
                "command": "cmake --build 'build/x && touch pwned' --preset 'x && touch pwned' --target fixture_tool",
                "forwardAllArgs": true,
              },
            ],
            "cwd": ".",
            "outputs": [
              "{workspaceRoot}/build/x && touch pwned/apps/tool/fixture_tool",
            ],
            "parallel": false,
          }
        `);
    });
});
