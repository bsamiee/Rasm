import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import manifest from '@rasm/nx-docker/package.json' with { type: 'json' };
import { Array, Effect, Schema } from 'effect';
import { cases, edges, inferred, roots, running, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['docker'],
    files: {
        'package.json': '{ "name": "root", "workspaces": ["api"] }\n',
        'api/package.json': '{ "name": "api" }\n',
        'api/Dockerfile': 'FROM scratch\nCOPY package.json /package.json\n',
        'web/project.json': '{ "name": "web", "targets": { "build": { "command": "true", "outputs": ["{workspaceRoot}/dist/web"] } } }\n',
        'stack/compose.yaml': 'name: stack\nservices:\n    api:\n        build:\n            context: ../api\n        profiles: [app]\n    cache:\n        image: valkey/valkey:8\n    site:\n        build:\n            context: ../dist/web\n            dockerfile_inline: FROM scratch\n        profiles: [app]\n',
        'store/compose.yaml': 'name: store\nservices:\n    database:\n        image: postgres:18\n',
    },
    broken: ['store/compose.yaml', 'name: store\nservices:\n    database:\n        image: [postgres\n'],
};

// --- [MODELS] --------------------------------------------------------------------------

const Probe = Schema.Struct({ command: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const composed = (subcommand: string) => (graph: ProjectGraph) => Array.dedupe(running(`docker compose ${subcommand}`)(graph).map(({ project }) => project));

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each Compose project by its top-level name and depends on a project whose build output a build context reads, never on one whose sources an image compiles', () =>
        Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([roots(graph), edges(graph)], [{ api: 'api', stack: 'stack', store: 'store', web: 'web' }, [{ source: 'stack', target: 'web', type: 'static' }]])),
    );

    it.effect('builds with docker compose build only where a service states a build, and starts every project with docker compose up', () => Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([composed('build')(graph), composed('up')(graph)], [['stack'], ['stack', 'store']])));

    it.effect('serves a service from the project holding its Dockerfile, as a continuous target ready once Compose reports the container healthy', () =>
        inferred(subject).pipe(
            Effect.map(running('docker compose --project-directory stack up')),
            Effect.map(Array.map(({ project, target, continuous, readyWhen }) => ({ project, target, continuous, ready: Schema.is(Probe)(readyWhen) && readyWhen.command.includes(' ps api ') }))),
            Effect.map((served) => assert.deepStrictEqual(served, [{ project: 'api', target: 'serve--compose', continuous: true, ready: true }])),
        ),
    );
});
