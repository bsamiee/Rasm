import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Effect, Layer } from 'effect';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { Composed, SLOW } from '#/workspace/graph';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('release configuration', (methods) => {
    methods.effect(
        'gives Nx a release configuration it accepts over the projects every plugin tags',
        Effect.fnUntraced(function* () {
            const { projectGraph, nxJson } = yield* Composed;
            const files = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(projectGraph));
            const { error } = yield* Effect.promise(() => createNxReleaseConfig(projectGraph, files, nxJson.release));
            assert.deepStrictEqual(error, null, 'nrwl/nx@2a424d3 packages/nx/src/command-line/release/config/config.ts createNxReleaseConfig, config.spec.ts');
        }),
        SLOW,
    );
});
