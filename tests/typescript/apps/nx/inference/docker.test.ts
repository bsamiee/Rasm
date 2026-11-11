import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import manifest from '@rasm/nx-docker/package.json' with { type: 'json' };
import { Array, Effect } from 'effect';
import { cases, edges, inferred, roots, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['docker'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["api"] }\n',
        'api/package.json': '{ "name": "api" }\n',
        'api/Dockerfile': 'FROM scratch\nCOPY package.json /package.json\n',
        'stack/compose.yaml': 'name: stack\nservices:\n    api:\n        build:\n            context: ../api\n        profiles: [app]\n    cache:\n        image: valkey/valkey:8\n',
        'store/compose.yaml': 'name: store\nservices:\n    database:\n        image: postgres:18\n',
    },
    broken: ['store/compose.yaml', 'name: store\nservices:\n    database:\n        image: [postgres\n'],
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const composed = (subcommand: string) => (graph: ProjectGraph) => Array.dedupe(running(`docker compose ${subcommand}`)(graph).map(({ project }) => project));

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each Compose project by its top-level name and depends on the project holding a service build context', () =>
        Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([roots(graph), edges(graph)], [{ api: 'api', stack: 'stack', store: 'store' }, [{ source: 'stack', target: 'api', type: 'static' }]])),
    );

    it.effect('builds with docker compose build only where a service states a build, and starts every project with docker compose up', () => Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([composed('build')(graph), composed('up')(graph)], [['stack'], ['stack', 'store']])));
});
