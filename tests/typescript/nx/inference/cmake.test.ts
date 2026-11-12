import { assert } from '@effect/vitest';
import { createDependencies, createNodes } from '@rasm/nx-cmake';
import manifest from '@rasm/nx-cmake/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRADLE = 'nrwl/nx@2a424d3 packages/gradle/src/plugin/dependencies.spec.ts';
const LIBRARY = (name: string, links: string) => `cmake_minimum_required(VERSION 4.5)\nproject(${name} VERSION 0.1.0 LANGUAGES CXX)\nadd_library(${name} ${name}.cpp)\nadd_library(sheets::${name} ALIAS ${name})\n${links}`;

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    state: ['build'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'CMakePresets.json':
            '{ "version": 13, "configurePresets": [{ "name": "default" }], "buildPresets": [{ "name": "default", "configurePreset": "default" }], "testPresets": [{ "name": "default", "configurePreset": "default" }], "workflowPresets": [{ "name": "default", "steps": [{ "type": "configure", "name": "default" }, { "type": "build", "name": "default" }, { "type": "test", "name": "default" }] }] }\n',
        'CMakeLists.txt': 'cmake_minimum_required(VERSION 4.5)\nproject(sheets LANGUAGES CXX)\nenable_testing()\nadd_subdirectory(libs/core)\nadd_subdirectory(libs/layout)\n',
        'libs/core/CMakeLists.txt': LIBRARY('core', ''),
        'libs/core/core.cpp': 'auto sheets() -> int { return 1; }\n',
        'libs/layout/CMakeLists.txt': LIBRARY('layout', 'target_link_libraries(layout PUBLIC sheets::core)\n'),
        'libs/layout/layout.cpp': 'auto sheets() -> int;\nauto laid() -> int { return sheets(); }\n',
    },
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names a project per project() call at the folder holding it', () => Effect.map(inferred(subject), (graph) => assert.deepOwnInclude(roots(graph), { core: 'libs/core', layout: 'libs/layout' })));

    it.effect('links a project to the project owning a target it links, once, at the file stating the link', () =>
        inferred(subject).pipe(
            Effect.map(edges),
            Effect.map(Array.filter(({ source }) => source === 'layout')),
            Effect.map((linked) => assert.deepStrictEqual(linked, [{ source: 'layout', target: 'core', type: 'static' }], `${GRADLE} "should create dependencies from gradle project dependencies"`)),
        ),
    );
});
