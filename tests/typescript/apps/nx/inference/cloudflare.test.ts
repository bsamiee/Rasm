import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-cloudflare';
import manifest from '@rasm/nx-cloudflare/package.json' with { type: 'json' };
import tool from 'cf/package.json' with { type: 'json' };
import { Array, Effect, Record } from 'effect';
import { cases, inferred, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCE = 'naxodev/oss@8b43d4a packages/nx-cloudflare/src/plugin.spec.ts dev continuous with readyWhen, deploy uncached, typegen cached';

const subject: Subject = {
    plugin: { name: manifest.name, createNodes },
    packages: [tool.name],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["worker", "queue"] }\n',
        'worker/package.json': '{ "name": "worker", "type": "module" }\n',
        'worker/cloudflare.config.ts': 'export default {};\n',
        'queue/package.json': '{ "name": "queue", "type": "module" }\n',
        'queue/cloudflare.config.ts': 'export default {};\n',
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = (graph: ProjectGraph, verb: string) =>
    Object.values(graph.nodes)
        .flatMap((node) => Record.collect(node.data.targets ?? {}, (name, target) => ({ name, project: node.name, cache: target.cache === true, continuous: target.continuous === true, ready: target.readyWhen !== undefined })))
        .filter(({ name }) => name.startsWith(`${verb}--`))
        .map(({ project, cache, continuous, ready }) => ({ project, cache, continuous, ready }))
        .toSorted((left, right) => left.project.localeCompare(right.project));

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('serves each Worker as an uncached continuous target with a readiness check, and never caches a deploy', () =>
        Effect.map(inferred(subject), (graph) =>
            assert.deepStrictEqual(
                [stated(graph, 'serve'), stated(graph, 'deploy')],
                [
                    [
                        { project: 'queue', cache: false, continuous: true, ready: true },
                        { project: 'worker', cache: false, continuous: true, ready: true },
                    ],
                    [
                        { project: 'queue', cache: false, continuous: false, ready: false },
                        { project: 'worker', cache: false, continuous: false, ready: false },
                    ],
                ],
                SOURCE,
            ),
        ),
    );

    it.effect('caches type generation', () =>
        inferred(subject).pipe(
            Effect.map((graph) => stated(graph, 'codegen')),
            Effect.map(Array.map(({ project, cache }) => ({ project, cache }))),
            Effect.map((generated) =>
                assert.deepStrictEqual(
                    generated,
                    [
                        { project: 'queue', cache: true },
                        { project: 'worker', cache: true },
                    ],
                    SOURCE,
                ),
            ),
        ),
    );
});
