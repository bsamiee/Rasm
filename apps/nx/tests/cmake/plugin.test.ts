import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-cmake/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, fromCache, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, FileSystem, Layer, Option, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'CMake file API codemodel-v2 reply of a second build tree, a source path relative where the top-level source directory holds the file and absolute elsewhere; ctest --show-only=json-v1; cmake_language(PRINT_VARIABLES NAMED PROJECT_NAME PROJECT_VERSION)';
const [FOLDER, PRESET, MANIFEST, PACKAGING] = ['build', 'default', 'CMakeLists.txt', 'CPackConfig.cmake'] as const;
const [CORE, MIDDLE, TOOL] = ['libs/core', 'libs/middle', 'apps/tool'] as const;
const SOURCES = { core: 'libs/core/core.cpp', suite: 'libs/core/core_test.cpp', header: 'libs/middle/include/middle.hpp', tool: 'apps/tool/main.cpp' } as const;
const files = {
    '.config/mise.lock': '# tool versions\n',
    'CMakeLists.txt': `cmake_minimum_required(VERSION 4.5)\nproject(Fixture LANGUAGES CXX)\nenable_testing()\nlist(APPEND CMAKE_PROJECT_INCLUDE "\${CMAKE_CURRENT_SOURCE_DIR}/cmake/package.cmake")\nadd_subdirectory(libs/core)\nadd_subdirectory(libs/middle)\nadd_subdirectory(apps/tool)\n`,
    'CMakePresets.json': `{\n    "version": 10,\n    "configurePresets": [\n        { "name": "tree", "hidden": true, "generator": "Ninja", "binaryDir": "\${sourceDir}/${FOLDER}/\${presetName}", "toolchainFile": "\${sourceDir}/cmake/toolchain.cmake" },\n        { "name": "${PRESET}", "inherits": "tree", "cacheVariables": { "CMAKE_BUILD_TYPE": "Debug" } }\n    ],\n    "buildPresets": [ { "name": "${PRESET}", "configurePreset": "${PRESET}" } ],\n    "testPresets": [ { "name": "${PRESET}", "configurePreset": "${PRESET}", "output": { "outputOnFailure": true } } ],\n    "packagePresets": [ { "name": "${PRESET}", "configurePreset": "${PRESET}", "generators": [ "TXZ" ] } ],\n    "workflowPresets": [ { "name": "${PRESET}", "steps": [ { "type": "configure", "name": "${PRESET}" }, { "type": "build", "name": "${PRESET}" }, { "type": "test", "name": "${PRESET}" } ] } ]\n}\n`,
    'cmake/toolchain.cmake': 'set(CMAKE_CXX_STANDARD 23)\nset(CMAKE_CXX_STANDARD_REQUIRED ON)\n',
    'cmake/package.cmake': `set(CPACK_PACKAGE_NAME "\${PROJECT_NAME}")\nset(CPACK_PACKAGE_VERSION "\${PROJECT_VERSION}")\nset(CPACK_SYSTEM_NAME "\${CMAKE_SYSTEM_NAME}-\${CMAKE_SYSTEM_PROCESSOR}")\nset(CPACK_INSTALL_CMAKE_PROJECTS "\${PROJECT_BINARY_DIR};\${PROJECT_NAME};ALL;/")\nset(CPACK_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/${PACKAGING}")\nset(CPACK_SOURCE_OUTPUT_CONFIG_FILE "\${PROJECT_BINARY_DIR}/CPackSourceConfig.cmake")\ninclude(CPack)\n`,
    'libs/core/CMakeLists.txt':
        '# Core of the chain\nproject(fixture_core VERSION 0.1.0 SPDX_LICENSE MIT LANGUAGES CXX)\nadd_library(fixture_core STATIC core.cpp)\ntarget_sources(fixture_core PUBLIC FILE_SET HEADERS FILES core.hpp)\nadd_executable(fixture_core_test core_test.cpp)\ntarget_link_libraries(fixture_core_test PRIVATE fixture_core)\nadd_test(NAME fixture_core_test COMMAND fixture_core_test)\ninstall(TARGETS fixture_core EXPORT fixture_core FILE_SET HEADERS)\ninstall(PACKAGE_INFO fixture_core EXPORT fixture_core)\ninstall(SBOM fixture_core EXPORTS fixture_core)\n',
    'libs/core/core.hpp': 'int one();\n',
    'libs/core/core.cpp': '#include "core.hpp"\n\nint one() { return 1; }\n',
    'libs/core/core_test.cpp': '#include "core.hpp"\n\nint main() { return one() == 1 ? 0 : 1; }\n',
    'libs/middle/CMakeLists.txt': 'project(fixture_middle VERSION 0.1.0 LANGUAGES CXX) # links the core\nadd_library(fixture_middle STATIC src/middle.cpp)\ntarget_include_directories(fixture_middle PUBLIC include)\ntarget_link_libraries(fixture_middle PUBLIC fixture_core)\n',
    'libs/middle/include/middle.hpp': 'int two();\n',
    'libs/middle/src/middle.cpp': '#include "middle.hpp"\n\n#include "core.hpp"\n\nint two() { return one() + 1; }\n',
    'apps/tool/CMakeLists.txt': 'project(fixture_tool VERSION 0.1.0 LANGUAGES CXX)\nadd_executable(fixture_tool main.cpp)\ntarget_link_libraries(fixture_tool PRIVATE fixture_middle)\ninstall(TARGETS fixture_tool)\n',
    'apps/tool/main.cpp': '#include "middle.hpp"\n\nint main() { return two() == 2 ? 0 : 1; }\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Project = Schema.Struct({ name: Schema.String, directoryIndexes: Schema.Array(Schema.Int), targetIndexes: Schema.optionalKey(Schema.Array(Schema.Int)) });
const Directory = Schema.Struct({ source: Schema.String, build: Schema.String, hasInstallRule: Schema.optionalKey(Schema.Boolean) });
const Codemodel = Schema.fromJsonString(Schema.Struct({ configurations: Schema.Array(Schema.Struct({ projects: Schema.Array(Project), directories: Schema.Array(Directory), targets: Schema.Array(Schema.Struct({ id: Schema.String, projectIndex: Schema.Int, jsonFile: Schema.String })) })) })).annotate({
    description: 'CMake file API codemodel-v2 reply: per configuration the projects, their directories and targets',
});
const Target = Schema.fromJsonString(
    Schema.Struct({
        artifacts: Schema.optionalKey(Schema.Array(Schema.Struct({ path: Schema.String }))),
        dependencies: Schema.optionalKey(Schema.Array(Schema.Struct({ id: Schema.String, backtrace: Schema.optionalKey(Schema.Int) }))),
        backtraceGraph: Schema.Struct({ nodes: Schema.Array(Schema.Struct({ command: Schema.optionalKey(Schema.Int) })) }),
        sources: Schema.Array(Schema.Struct({ path: Schema.String, isGenerated: Schema.optionalKey(Schema.Boolean) })),
    }),
).annotate({ description: 'CMake file API target reply: the artifacts, sources, and target dependencies of one target, a dependency whose backtrace node names a command where a command invocation made it' });
const Tests = Schema.fromJsonString(Schema.Struct({ tests: Schema.Array(Schema.Struct({ properties: Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Unknown })) })) })).annotate({
    description: 'JSON ctest --show-only=json-v1 prints: every test of a build tree with its properties',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (
    path: Path.Path,
    tree: {
        readonly projects: readonly (typeof Project.Type)[];
        readonly directories: readonly (typeof Directory.Type)[];
        readonly targets: readonly ({ readonly id: string; readonly projectIndex: number } & typeof Target.Type)[];
        readonly versions: Record.ReadonlyRecord<string, string>;
        readonly packaged: readonly string[];
        readonly tested: readonly string[];
    },
    { name, directoryIndexes, targetIndexes = [] }: typeof Project.Type,
) => {
    const placed = (indexes: readonly number[]) => indexes.slice(0, 1).flatMap((index) => Array.fromNullishOr(tree.directories[index]));
    const owned = targetIndexes.flatMap((index) => Array.fromNullishOr(tree.targets[index]));
    const needed = owned.flatMap(({ dependencies = [], backtraceGraph: { nodes } }) => dependencies.filter(({ backtrace = -1 }) => nodes[backtrace]?.command !== undefined).map(({ id }) => id));
    const linked = tree.targets.filter(({ id, projectIndex }) => needed.includes(id) && tree.projects[projectIndex]?.name !== name).flatMap(({ projectIndex }) => placed(tree.projects[projectIndex]?.directoryIndexes ?? []));
    const listed = owned.flatMap(({ sources }) => sources.filter(({ path: at, isGenerated = false }) => !(isGenerated || path.isAbsolute(at))).map((source) => source.path));
    const written = owned.flatMap(({ artifacts = [] }) => artifacts.map((artifact) => path.join(FOLDER, PRESET, artifact.path)));
    return Array.zip(placed(directoryIndexes), Option.toArray(Record.get(tree.versions, name))).map(([{ source, build, hasInstallRule = false }, version]) => ({
        name,
        version,
        root: source,
        manifest: path.join(source, MANIFEST),
        released: tree.packaged.includes(build),
        verbs: [...(owned.length > 0 ? ['build', 'clean'] : []), ...(tree.tested.includes(build) ? ['test'] : []), ...(hasInstallRule ? ['install'] : []), ...(tree.packaged.includes(build) ? ['pack'] : [])],
        sources: [path.join(source, MANIFEST), ...listed],
        artifacts: written,
        dependencies: Array.dedupe(linked.map((folder) => folder.source)),
    }));
};

const members = Effect.gen(function* () {
    const [{ root, called }, fs, path] = yield* Effect.all([Consumer, FileSystem.FileSystem, Path.Path]);
    const tree = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'cmake-truth-' }), fs.realPath);
    const [query, reply, probe] = [path.join(tree, '.cmake/api/v1/query'), path.join(tree, '.cmake/api/v1/reply'), path.join(tree, 'probe.cmake')];
    yield* fs.makeDirectory(query, { recursive: true });
    yield* fs.writeFileString(path.join(query, 'codemodel-v2'), '');
    yield* fs.writeFileString(probe, 'cmake_language(PRINT_VARIABLES NAMED PROJECT_NAME PROJECT_VERSION)\n');
    const { checked } = yield* called('cmake', ['-S', '.', '-B', tree, '-G', 'Ninja', `-DCMAKE_TOOLCHAIN_FILE=${path.join(root, 'cmake/toolchain.cmake')}`, `-DCMAKE_PROJECT_INCLUDE=${probe}`]);
    const printed = yield* checked;
    const index = yield* Effect.flatMap(fs.readDirectory(reply), (entries) => Effect.fromOption(Array.findFirst(entries, (entry) => entry.startsWith('codemodel-v2'))));
    const { configurations } = yield* Effect.flatMap(fs.readFileString(path.join(reply, index)), Schema.decodeEffect(Codemodel));
    const { projects, directories, targets: stated } = yield* Effect.fromOption(Array.head(configurations));
    const targets = yield* Effect.forEach(stated, ({ id, projectIndex, jsonFile }) => Effect.map(Effect.flatMap(fs.readFileString(path.join(reply, jsonFile)), Schema.decodeEffect(Target)), (target) => ({ id, projectIndex, ...target })));
    const listed = yield* Effect.flatMap(called('ctest', ['--test-dir', tree, '--show-only=json-v1']), ({ stdout }) => Schema.decodeEffect(Tests)(stdout));
    const packaged = yield* Effect.filter(directories, ({ build, hasInstallRule = false }) => Effect.map(fs.exists(path.join(tree, build, PACKAGING)), (configured) => configured && hasInstallRule));
    const versions = Array.fromIterable(printed.matchAll(/PROJECT_NAME = "(?<name>[^"]*)"\s+PROJECT_VERSION = "(?<version>[^"]*)"/gu)).map(({ groups = {} }) => Option.all([Record.get(groups, 'name'), Record.get(groups, 'version')]));
    const read = {
        projects,
        directories,
        targets,
        versions: Record.fromEntries(Array.getSomes(versions)),
        packaged: packaged.map(({ build }) => build),
        tested: listed.tests.flatMap(({ properties }) => properties.filter(({ name }) => name === 'WORKING_DIRECTORY').map(({ value }) => path.relative(tree, String(value)))),
    };
    return projects.flatMap((project) => described(path, read, project));
}).pipe(Effect.scoped, Effect.orDie);

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));

// --- [COMPOSITION] ---------------------------------------------------------------------

const bound = Duration.toMillis('5 minutes');
const options = { timeout: bound, excludeTestServices: true } as const;

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name], release: { projects: ['tag:release'] } }, ignored: [FOLDER], files: { ...files, ...stated } }).pipe(Layer.provideMerge(Packed.layer(plugin.name)), Layer.provideMerge(layer));

conforms({
    workspace,
    members,
    built: ({ artifacts }) => Effect.succeed(artifacts),
    report: REPORT,
    tools: ['cmake'],
    verbs: ['build', 'clean', 'test', 'install', 'pack'],
    shared: ['CMakePresets.json', MANIFEST, 'cmake/toolchain.cmake', 'cmake/package.cmake'],
    unlocked: [],
    scratch: [FOLDER],
    added: Consumer.use(({ wrote }) =>
        wrote({
            [MANIFEST]: files[MANIFEST].replace('add_subdirectory(libs/core)', 'add_subdirectory(libs/extra)\nadd_subdirectory(libs/core)'),
            'libs/extra/CMakeLists.txt': 'project(fixture_extra VERSION 0.1.0 LANGUAGES CXX)\nadd_library(fixture_extra STATIC extra.cpp)\n',
            'libs/extra/extra.cpp': 'int zero() { return 0; }\n',
            'libs/middle/CMakeLists.txt': `${files['libs/middle/CMakeLists.txt']}target_link_libraries(fixture_middle PRIVATE fixture_extra)\n`,
        }),
    ),
    published: false,
    refused: { 'broken/CMakePresets.json': '{ "version": 10, "configurePresets": [\n', 'broken/CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(Broken LANGUAGES CXX)\n' },
    manifests: (next) => ({
        'libs/core/CMakeLists.txt': files['libs/core/CMakeLists.txt'].replace('VERSION 0.1.0', `VERSION ${next}`),
        'libs/middle/CMakeLists.txt': files['libs/middle/CMakeLists.txt'],
        'apps/tool/CMakeLists.txt': files['apps/tool/CMakeLists.txt'].replace('VERSION 0.1.0', `VERSION ${next}`),
    }),
    edits: {
        verbs: ['build'],
        dependent: MIDDLE,
        upstream: (mark) => ({ [SOURCES.core]: `${files[SOURCES.core]}\nint probe_${mark}() { return 7; }\n` }),
        outside: (mark) => ({ [SOURCES.tool]: `${files[SOURCES.tool]}\nint aside_${mark}() { return 7; }\n`, 'libs/middle/notes.md': `${mark}\n` }),
        tested: CORE,
        failing: { [SOURCES.suite]: files[SOURCES.suite].replace('one() == 1', 'one() == 2') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'declares every build output under the folder CMake configures for a preset inheriting a binaryDir of the sourceDir and presetName macros',
        Effect.fnUntraced(function* () {
            const [{ called, cleaned, written, outputs }, path, { name }] = yield* Effect.all([Consumer, Path.Path, member(TOOL)]);
            yield* cleaned;
            const [configured, created] = yield* written(called('cmake', ['--preset', PRESET]));
            assert.strictEqual(configured.code, 0, configured.stderr);
            const cache = yield* Effect.orDie(Effect.fromOption(Array.findFirst(created, (file) => path.basename(file) === 'CMakeCache.txt')));
            const { outputPaths } = yield* outputs(`${name}:build--cmake`);
            assert.isNotEmpty(outputPaths, REPORT);
            assert.deepStrictEqual(
                outputPaths.filter((output) => !output.startsWith(`${path.dirname(cache)}/`)),
                [],
                'cmake-presets(7) "macros are evaluated in the context of the preset being used"; README.md PRINCIPLES "configures the build folder each configure preset states"',
            );
        }),
        bound,
    );

    methods.effect(
        'misses the build cache of a project after an edit to a header no target lists and a source of it includes',
        Effect.fnUntraced(function* () {
            const [{ nx, wrote }, { name }, stated] = yield* Effect.all([Consumer, member(TOOL), members]);
            const [run, hit] = [nx('build', name, '--output-style=static'), fromCache(`${name}:build--`)];
            yield* run;
            assert.match((yield* run).stdout, hit, 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should store and use local cache for builds"');
            assert.notInclude(
                stated.flatMap(({ sources }) => sources),
                SOURCES.header,
                'CMake file API target reply sources[]',
            );
            yield* wrote({ [SOURCES.header]: `${files[SOURCES.header]}int probe();\n` });
            const edited = yield* run;
            yield* wrote({ [SOURCES.header]: files[SOURCES.header] });
            assert.strictEqual(edited.code, 0, `${edited.stdout}${edited.stderr}`);
            assert.notMatch(edited.stdout, hit, 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts "should not serve a cache hit after the dependency changes"; GCC cpp "Search Path": #include "file" reads from the folder of the including file, then each -I folder');
        }),
        bound,
    );

    methods.effect(
        'cleans every member of one build tree in one parallel run',
        Effect.fnUntraced(function* () {
            const { nx } = yield* Consumer;
            yield* nx('run-many', '--targets=build', '--output-style=static');
            const run = yield* nx('run-many', '--targets=clean', '--parallel=8', '--output-style=static');
            assert.strictEqual(run.code, 0, `nrwl/nx astro-docs/src/content/docs/reference/project-configuration.mdoc "Parallelism": tasks sharing a resource state parallelism false, and members of one preset share its build tree: ${run.stdout}${run.stderr}`);
        }),
        bound,
    );

    methods.effect(
        'packs a project into one CPack archive named from its name, version, and system, holding a CPS file and an SPDX SBOM',
        Effect.fnUntraced(function* () {
            const [{ nx, called, written, outputs }, path, { name, version }] = yield* Effect.all([Consumer, Path.Path, member(CORE)]);
            const [packed, created] = yield* written(nx('pack', name, '--skip-nx-cache', '--output-style=static'));
            assert.strictEqual(packed.code, 0, `${packed.stdout}${packed.stderr}`);
            const [system, { outputPaths }] = yield* Effect.all([called('uname', ['-sm']), outputs(`${name}:pack--cpack`)]);
            const archives = created.filter((file) => outputPaths.includes(path.dirname(file)) && path.basename(file) === `${name}-${version}-${system.stdout.trim().replace(' ', '-')}.tar.xz`);
            assert.lengthOf(archives, 1, `README.md DELIVERY "CPack naming it from project, version, and system"; CMake CMAKE_HOST_SYSTEM_NAME "uname -s", CMAKE_HOST_SYSTEM_PROCESSOR "uname -m": ${created.join(', ')}`);
            const listed = yield* called('tar', ['-tJf', ...archives]);
            const held = listed.stdout.split('\n');
            assert.isNotEmpty(
                held.filter((entry) => entry.endsWith(`/${name}.cps`)),
                'README.md DELIVERY "C++ packages release as a CPack archive of their install tree with a CPS file and an SPDX SBOM"; CMake install(PACKAGE_INFO)',
            );
            assert.isNotEmpty(
                held.filter((entry) => entry.endsWith('.spdx.json')),
                'README.md DELIVERY "C++ packages release as a CPack archive of their install tree with a CPS file and an SPDX SBOM"; CMake install(SBOM)',
            );
        }),
        bound,
    );

    methods.effect(
        'refuses a prerelease version, which project(VERSION) cannot state, and changes no manifest',
        Effect.fnUntraced(function* () {
            const { nx, called } = yield* Consumer;
            const versioned = yield* nx('release', 'version', '1.1.0-rc.1');
            const changed = yield* called('git', ['status', '--porcelain']);
            assert.notStrictEqual(versioned.code, 0, 'CMake project() "VERSION: Takes a <version> argument composed of non-negative integer components"');
            assert.include(`${versioned.stdout}${versioned.stderr}`, '1.1.0-rc.1', 'CMake project() "VERSION: Takes a <version> argument composed of non-negative integer components"');
            assert.strictEqual(changed.stdout, '', 'nrwl/nx packages/js/src/release/version-actions.spec.ts "updateProjectVersion"');
        }),
        bound,
    );
});

it.layer(
    workspace({ 'libs/middle/CMakeLists.txt': 'project(fixture_middle LANGUAGES CXX\n' }),
    options,
)((methods) => {
    methods.effect(
        'names the CMakeLists.txt cmake names in its refusal',
        Effect.fnUntraced(function* () {
            const { failures } = yield* Effect.flatMap(Consumer, ({ read }) => read);
            assert.include([...failures, ...failures.flatMap(({ files: named }) => named)].map(({ message }) => message).join('\n'), 'libs/middle/CMakeLists.txt', 'cmake stderr; nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts "should handle partial errors"');
        }),
        bound,
    );
});
