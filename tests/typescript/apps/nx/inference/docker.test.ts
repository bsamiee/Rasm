import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { getReadyCheckReadyWhen } from '@nx/devkit/internal';
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

const QUOTED = /'[^']*'/gu;
const UNSAFE = /my proj|;touch|\$\(id\)/u;

// --- [MODELS] --------------------------------------------------------------------------

const Probe = Schema.Struct({ command: Schema.String });
const Ran = Schema.Struct({ command: Schema.optionalKey(Schema.String), commands: Schema.optionalKey(Schema.Array(Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const unquoted = (graph: ProjectGraph) =>
    Object.values(graph.nodes)
        .flatMap((node) => Object.values(node.data.targets ?? {}))
        .flatMap(({ options, readyWhen }) => [...Array.fromOption(Schema.decodeUnknownOption(Ran)(options)).flatMap(({ command, commands = [] }) => [...Array.fromNullishOr(command), ...commands]), ...(Schema.is(Probe)(readyWhen) ? [readyWhen.command] : [])])
        .filter((line) => UNSAFE.test(line.replaceAll(QUOTED, '')));

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
            Effect.map(Array.map(({ project, target, continuous, readyWhen }) => ({ project, target, continuous, readyWhen }))),
            Effect.map((served) =>
                assert.deepStrictEqual(served, [{ project: 'api', target: 'serve--compose', continuous: true, readyWhen: getReadyCheckReadyWhen(`${manifest.name}/ready-check`, undefined, 'stack', 'api') }], 'nrwl/nx @nx/devkit readiness getReadyCheckReadyWhen: a ready-check module node runs on every platform'),
            ),
        ),
    );

    it.effect('states a project folder that holds a space or a shell metacharacter in a command only inside single quotes', () =>
        Effect.map(
            inferred({
                ...subject,
                files: {
                    'package.json': '{ "name": "root" }\n',
                    'my proj/compose.yaml': 'name: spaced\nservices:\n    app:\n        build:\n            context: .\n',
                    'my proj/Dockerfile': 'FROM scratch\nCOPY compose.yaml /compose.yaml\n',
                    'a;touch pwned;$(id)/compose.yaml': 'name: pwned\nservices:\n    cache:\n        image: valkey/valkey:8\n',
                },
            }),
            (graph) => assert.deepStrictEqual([composed('up')(graph).length, unquoted(graph)], [2, []], 'nrwl/nx@2a424d3 packages/docker/src/plugins/plugin.spec.ts image reference generation "should handle spaces in project path", "should replace shell metacharacters in project path"'),
        ),
    );
});
