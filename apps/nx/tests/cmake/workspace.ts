import { assert } from '@effect/vitest';
import { Consumer } from '@rasm/testing/nx/consumer';
import { Array, Effect, FileSystem, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROBE = 'message(STATUS "version:${PROJECT_NAME}=${PROJECT_VERSION}")\n';
const files = {
    'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.0)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\nadd_subdirectory(libs/middle)\nadd_subdirectory(apps/tool)\n',
    'CMakePresets.json':
        '{\n    "version": 10,\n    "configurePresets": [ { "name": "default", "generator": "Ninja", "binaryDir": "${sourceDir}/build/${presetName}", "toolchainFile": "${sourceDir}/cmake/toolchain.cmake", "cacheVariables": { "CMAKE_BUILD_TYPE": "Debug" } } ],\n    "buildPresets": [ { "name": "default", "configurePreset": "default" } ],\n    "testPresets": [ { "name": "default", "configurePreset": "default", "output": { "outputOnFailure": true } } ],\n    "workflowPresets": [ { "name": "default", "steps": [ { "type": "configure", "name": "default" }, { "type": "build", "name": "default" }, { "type": "test", "name": "default" } ] } ]\n}\n',
    'cmake/toolchain.cmake': 'set(CMAKE_CXX_STANDARD 23)\nset(CMAKE_CXX_STANDARD_REQUIRED ON)\n',
    'libs/core/CMakeLists.txt':
        '# Core of the chain\nproject(fixture_core VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\ntarget_include_directories(fixture_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})\nadd_executable(fixture_core_test core_test.cpp)\ntarget_link_libraries(fixture_core_test PRIVATE fixture_core)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\n',
    'libs/core/core.hpp': 'int one();\n',
    'libs/core/core.cpp': '#include "core.hpp"\n\nint one() { return 1; }\n',
    'libs/core/core_test.cpp': '#include "core.hpp"\n\nint main() { return one() == 1 ? 0 : 1; }\n',
    'libs/middle/CMakeLists.txt': 'project(fixture_middle VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_middle STATIC middle.cpp)\ntarget_include_directories(fixture_middle PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})\ntarget_link_libraries(fixture_middle PUBLIC fixture_core)\n',
    'libs/middle/middle.hpp': 'int two();\n',
    'libs/middle/middle.cpp': '#include "middle.hpp"\n\n#include "core.hpp"\n\nint two() { return one() + 1; }\n',
    'apps/tool/CMakeLists.txt': 'project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_middle)\ninstall(TARGETS fixture_tool)\n',
    'apps/tool/main.cpp': '#include "middle.hpp"\n\nint main() { return two() == 2 ? 0 : 1; }\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Codemodel = Schema.fromJsonString(
    Schema.Struct({
        configurations: Schema.Array(
            Schema.Struct({
                projects: Schema.Array(Schema.Struct({ name: Schema.String, directoryIndexes: Schema.Array(Schema.Int), targetIndexes: Schema.optionalKey(Schema.Array(Schema.Int)) })),
                directories: Schema.Array(Schema.Struct({ source: Schema.String, build: Schema.String, hasInstallRule: Schema.optionalKey(Schema.Boolean) })),
                targets: Schema.Array(Schema.Struct({ id: Schema.String, projectIndex: Schema.Int, jsonFile: Schema.String })),
            }),
        ),
    }),
).annotate({ description: 'CMake file API codemodel-v2 reply: per configuration the projects, their directories and targets' });
const Target = Schema.fromJsonString(
    Schema.Struct({
        type: Schema.String,
        artifacts: Schema.optionalKey(Schema.Array(Schema.Struct({ path: Schema.String }))),
        dependencies: Schema.optionalKey(Schema.Array(Schema.Struct({ id: Schema.String }))),
        sources: Schema.Array(Schema.Struct({ path: Schema.String, isGenerated: Schema.optionalKey(Schema.Boolean) })),
    }),
).annotate({ description: 'CMake file API target reply: the type, artifacts, sources, and target dependencies of one target' });
const Inputs = Schema.fromJsonString(Schema.Struct({ inputs: Schema.Array(Schema.Struct({ path: Schema.String, isExternal: Schema.optionalKey(Schema.Boolean), isGenerated: Schema.optionalKey(Schema.Boolean) })) })).annotate({
    description: 'CMake file API cmakeFiles-v1 reply: every file CMake read to configure the tree',
});
const Tests = Schema.fromJsonString(Schema.Struct({ tests: Schema.Array(Schema.Struct({ name: Schema.String, properties: Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Unknown })) })) })).annotate({
    description: 'JSON ctest --show-only=json-v1 prints: every test of a build tree with its properties',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Effect.gen(function* () {
    const [{ root, called }, fs, path] = yield* Effect.all([Consumer, FileSystem.FileSystem, Path.Path]);
    const tree = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'cmake-truth-' }), fs.realPath);
    const [query, reply, probe] = [path.join(tree, '.cmake/api/v1/query'), path.join(tree, '.cmake/api/v1/reply'), path.join(tree, 'probe.cmake')];
    yield* fs.makeDirectory(query, { recursive: true });
    yield* Effect.forEach(['codemodel-v2', 'cmakeFiles-v1'], (kind) => fs.writeFileString(path.join(query, kind), ''));
    yield* fs.writeFileString(probe, PROBE);
    const { checked } = yield* called('cmake', ['-S', '.', '-B', tree, '-G', 'Ninja', `-DCMAKE_TOOLCHAIN_FILE=${path.join(root, 'cmake/toolchain.cmake')}`, `-DCMAKE_PROJECT_INCLUDE=${probe}`]);
    const printed = yield* checked;
    const replies = yield* fs.readDirectory(reply);
    const answered = (prefix: string) => Effect.flatMap(Effect.fromNullishOr(replies.find((entry) => entry.startsWith(prefix))), (entry) => fs.readFileString(path.join(reply, entry)));
    const [codemodel] = (yield* Effect.flatMap(answered('codemodel-v2'), Schema.decodeEffect(Codemodel))).configurations;
    const { projects, directories, targets } = yield* Effect.fromNullishOr(codemodel);
    const read = yield* Effect.forEach(targets, ({ id, projectIndex, jsonFile }) => Effect.map(Effect.flatMap(fs.readFileString(path.join(reply, jsonFile)), Schema.decodeEffect(Target)), (target) => ({ id, projectIndex, ...target })));
    const { inputs } = yield* Effect.flatMap(answered('cmakeFiles-v1'), Schema.decodeEffect(Inputs));
    const listed = yield* Effect.flatMap(called('ctest', ['--test-dir', tree, '--show-only=json-v1']), ({ stdout }) => Schema.decodeEffect(Tests)(stdout));
    const folders = listed.tests.flatMap(({ properties }) => properties.filter(({ name }) => name === 'WORKING_DIRECTORY').map(({ value }) => path.relative(tree, String(value))));
    const versions = Record.fromEntries(Array.fromIterable(printed.matchAll(/version:(?<name>[^=\n]+)=(?<version>[^\n]*)/gu)).map(({ groups }) => [groups?.['name'] ?? '', groups?.['version'] ?? ''] as const));
    return {
        read: inputs.filter(({ isExternal = false, isGenerated = false }) => !(isExternal || isGenerated)).map(({ path: file }) => file),
        members: projects.flatMap(({ name, directoryIndexes, targetIndexes = [] }) => {
            const [folder] = directoryIndexes.map((index) => directories[index]);
            const owned = targetIndexes.flatMap((index) => Array.fromNullishOr(read[index]));
            const needed = owned.flatMap(({ dependencies = [] }) => dependencies.map((dependency) => dependency.id));
            return folder === undefined || folder.source === '.'
                ? []
                : [
                      {
                          name,
                          version: versions[name] ?? '',
                          root: folder.source,
                          manifest: path.join(folder.source, 'CMakeLists.txt'),
                          installed: folder.hasInstallRule ?? false,
                          tested: folders.includes(folder.build),
                          sources: owned.flatMap(({ sources }) => sources.filter(({ isGenerated = false }) => !isGenerated).map(({ path: file }) => file)),
                          artifacts: owned.flatMap(({ artifacts = [] }) => artifacts.map(({ path: file }) => file)),
                          dependencies: Array.dedupe(read.filter(({ id, projectIndex }) => needed.includes(id) && projects[projectIndex]?.name !== name).flatMap(({ projectIndex }) => (projects[projectIndex]?.directoryIndexes ?? []).slice(0, 1).flatMap((index) => Array.fromNullishOr(directories[index]?.source)))),
                      },
                  ];
        }),
    };
}).pipe(Effect.scoped, Effect.orDie);

const members = Effect.map(configured, (tree) => tree.members);

const placed = Effect.gen(function* () {
    const { read } = yield* Consumer;
    const { nodes, dependencies, failures } = yield* read;
    const projects = Object.values(nodes).filter(({ data }) => data.root !== '.');
    const roots = Record.fromIterableWith(projects, ({ name, data }) => [name, data.root]);
    const targets = projects.flatMap(({ name, data }) => Record.keys(data.targets ?? {}).map((target) => ({ project: name, root: data.root, target, verb: target.split('--')[0] ?? target })));
    return { projects, roots, targets, failures, edges: projects.flatMap(({ name }) => (dependencies[name] ?? []).filter(({ target }) => target in roots)) };
});

const named = Effect.fnUntraced(function* (root: string) {
    const { projects } = yield* placed;
    const found = projects.find(({ data }) => data.root === root);
    assert.exists(found, `Nx holds a project at ${root}`);
    return found?.name ?? root;
});

const ran = (...args: readonly string[]) => Effect.flatMap(Consumer, ({ nx }) => Effect.map(nx(...args, '--output-style=static'), ({ code, stdout, stderr }) => ({ code: Number(code), output: `${stdout}${stderr}` })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>> = {}, release: object = {}) => Consumer.layer({ resolved: import.meta.url, settings: { plugins: ['@rasm/nx-cmake'], release: { projects: ['tag:release'], ...release } }, ignored: ['build'], files: { ...files, ...stated } });

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, files, members, named, placed, ran, workspace };
