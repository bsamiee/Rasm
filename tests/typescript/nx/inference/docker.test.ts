import { chdir, cwd, pid } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it, vi } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { getReadyCheckReadyWhen } from '@nx/devkit/internal';
import { createDependencies, createNodes } from '@rasm/nx-docker';
import manifest from '@rasm/nx-docker/package.json' with { type: 'json' };
import { waitForDevServer } from '@rasm/nx-docker/ready-check';
import { called, ran } from '@rasm/testing/nx/cli';
import { checkout } from '@rasm/testing/nx/inference';
import { Array, Cause, type Duration, Effect, Option, Path, Result, Schedule, Schema } from 'effect';
import { inject } from 'vitest';
import { cases, edges, inferred, roots, running, type Subject } from '#/inference/cases';
import { type Methods, placed, present, SLOW } from '#/inference/workspaces';

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

const SPANS = /(?<kept>"[^"]*")|'[^']*'/gu;
const UNSAFE = /my proj|;touch|\$\(id\)/u;
const STARTED = 'my stack';
const IDLE = '[sleep, infinity]';
const DOWN = ['down', '--rmi', 'local', '--volumes', '--timeout', '0'];
const SOURCE = {
    split: 'nrwl/nx@2a424d3 packages/docker/src/plugins/plugin.spec.ts "should create nodes based on Dockerfile"',
    waited: 'nrwl/nx@2a424d3 packages/vite/src/utils/dev-server-ready-check.spec.ts waitForViteServer expectReadyOnListen under NX_TASK_OPTIONS cwd, docker/compose@v5.7.0 pkg/compose/dependency_wait_test.go TestIsServiceHealthy',
    ordered: 'naxodev/oss@8b43d4a packages/gonx-e2e/src/readiness.spec.ts "waits for a project-defined health check before running its consumer", nrwl/nx@2a424d3 e2e/nx/src/run.test.ts "should wait for the command probe", README [04] serve "starts the Compose services whose Dockerfile it holds, which stay up"',
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Probe = Schema.Struct({ command: Schema.String });
const Ran = Schema.Struct({ command: Schema.optionalKey(Schema.String), commands: Schema.optionalKey(Schema.Array(Schema.String)) });
const Runtime = Schema.Struct({ runtime: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const unquoted = (graph: ProjectGraph) =>
    Object.values(graph.nodes)
        .flatMap((node) => Object.values(node.data.targets ?? {}))
        .flatMap(({ options, readyWhen, inputs = [] }) => [
            ...Array.fromOption(Schema.decodeUnknownOption(Ran)(options)).flatMap(({ command, commands = [] }) => [...Array.fromNullishOr(command), ...commands]),
            ...(Schema.is(Probe)(readyWhen) ? [readyWhen.command] : []),
            ...inputs.filter(Schema.is(Runtime)).map(({ runtime }) => runtime),
        ])
        .filter((line) => UNSAFE.test(line.replaceAll(SPANS, '$<kept>')));

const tasked = (command: string) => (graph: ProjectGraph) => running(command)(graph).map(({ project, target }) => `${project}:${target}`);

const stacked = (name: string, command: string) => ({
    'package.json': '{ "name": "root" }\n',
    [`${STARTED}/Dockerfile`]: 'FROM alpine:3.23\nSTOPSIGNAL SIGKILL\n',
    [`${STARTED}/compose.yaml`]: `name: ${name}\nservices:\n    api:\n        build:\n            context: .\n        command: ${command}\n        scale: 2\n        healthcheck:\n            test: [CMD, test, -f, /tmp/ready]\n            interval: 1s\n            retries: 1\n            start_period: 0s\n    plain:\n        build:\n            context: .\n        command: ${IDLE}\n`,
});

const composing = (folder: string, args: readonly string[]) =>
    Effect.filterOrFail(
        called(folder, 'docker', ['compose', ...args], {}),
        ({ code }) => code === 0,
        ({ output }) => new Cause.UnknownError(output, `docker compose ${args.join(' ')} exited non-zero in ${folder}: ${output}`),
    );

const reported = (folder: string, format: string, expected: string) =>
    Effect.repeat(
        Effect.map(composing(folder, ['ps', '--format', format]), ({ stdout }) => stdout.trim().split('\n').toSorted().join(' ')),
        { until: (states) => states === expected, schedule: Schedule.spaced('250 millis') },
    );

const resolved = (wait: Promise<void>, within: Duration.Input) =>
    Effect.map(
        Effect.timeoutOption(
            Effect.promise(() => wait),
            within,
        ),
        Option.isSome,
    );

const checked = Effect.fnUntraced(function* (label: string) {
    const path = yield* Path.Path;
    const folder = path.join(yield* checkout(stacked(`nx-docker-${pid}-${label}`, IDLE), [], import.meta.url), STARTED);
    yield* Effect.acquireRelease(Effect.sync(cwd), (prior) =>
        Effect.andThen(
            Effect.sync(() => chdir(prior)),
            Effect.ignore(composing(folder, DOWN)),
        ),
    );
    vi.stubEnv('NX_TASK_OPTIONS', yield* Schema.encodeUnknownEffect(Json)({ cwd: folder }));
    return folder;
});

const started = (methods: Methods) => {
    const [held, reached] = ['2 seconds', '30 seconds'] as const;

    methods.effect(
        'keeps waiting where Compose lists no container of the service, and resolves once a container without a healthcheck runs',
        Effect.fnUntraced(function* () {
            const folder = yield* checked('plain');
            const wait = waitForDevServer('plain');
            const absent = yield* resolved(wait, held);
            yield* composing(folder, ['up', '--detach', '--build', 'plain']);
            assert.deepStrictEqual([absent, yield* resolved(wait, reached)], [false, true], `${SOURCE.waited} "no healthcheck with fallback to running"`);
        }),
        SLOW,
    );

    methods.effect(
        'keeps waiting while a container of the service is starting or unhealthy, and resolves once Compose reports every container healthy',
        Effect.fnUntraced(function* () {
            const folder = yield* checked('health');
            yield* composing(folder, ['up', '--detach', '--build', 'api']);
            const wait = waitForDevServer('api');
            const starting = yield* resolved(wait, held);
            yield* reported(folder, '{{.Health}}', 'unhealthy unhealthy');
            const unhealthy = yield* resolved(wait, held);
            yield* composing(folder, ['exec', '--index', '1', 'api', 'touch', '/tmp/ready']);
            yield* reported(folder, '{{.Health}}', 'healthy unhealthy');
            const partial = yield* resolved(wait, held);
            yield* composing(folder, ['exec', '--index', '2', 'api', 'touch', '/tmp/ready']);
            assert.deepStrictEqual([starting, unhealthy, partial, yield* resolved(wait, reached)], [false, false, false, true], `${SOURCE.waited} "healthy container with healthcheck"`);
        }),
        SLOW,
    );

    methods.effect(
        'builds the images as a task of its own, runs a task waiting on serve--compose only once Compose reports every container of its services healthy, and leaves the services running when the run ends',
        Effect.fnUntraced(function* () {
            const [path, name, probe] = [yield* Path.Path, `nx-docker-${pid}-served`, 'waiter/probe.mjs'];
            const waiter = yield* Schema.encodeUnknownEffect(Json)({ name: 'waiter', targets: { probe: { command: `node ${probe}`, dependsOn: [{ projects: [name], target: 'serve--compose', waitFor: 'ready' }] } } });
            const root = yield* placed({
                ...subject,
                files: {
                    ...stacked(name, '[sh, -c, "touch /tmp/ready && exec sleep infinity"]'),
                    'waiter/project.json': waiter,
                    [probe]: `import { execFileSync } from 'node:child_process';\n\nconst health = execFileSync('docker', ['compose', '--project-directory', '${STARTED}', 'ps', '--format', '{{.Health}}', 'api'], { encoding: 'utf8' }).trim().split('\\n');\nprocess.exitCode = health.length === 2 && health.every((state) => state === 'healthy') ? 0 : 1;\n`,
                },
            });
            const folder = path.join(root, STARTED);
            yield* Effect.addFinalizer(() => Effect.ignore(composing(folder, DOWN)));
            const { code, output, tasks } = yield* ran(root, ['run', 'waiter:probe']);
            const finished = Array.intersection(
                [`${name}:build--buildx`, 'waiter:probe'],
                Array.filterMap(tasks, ({ taskId, status }) => (status === 0 ? Result.succeed(taskId) : Result.failVoid)),
            );
            assert.deepStrictEqual([code, finished, yield* reported(folder, '{{.State}}', 'running running running')], [0, [`${name}:build--buildx`, 'waiter:probe'], 'running running running'], `${SOURCE.split} docker:run dependsOn docker:build, ${SOURCE.ordered}\n${output}`);
        }),
        SLOW,
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (methods) => {
    methods.effect('names each Compose project by its top-level name and depends on a project whose build output a build context reads, never on one whose sources an image compiles', () =>
        Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([roots(graph), edges(graph)], [{ api: 'api', stack: 'stack', store: 'store', web: 'web' }, [{ source: 'stack', target: 'web', type: 'static' }]])),
    );

    methods.effect('builds images with docker compose build alone, only where a service states a build, and installs every project with a docker compose up that builds none', () =>
        Effect.map(inferred(subject), (graph) =>
            assert.deepStrictEqual(
                [tasked('docker compose build')(graph), tasked('docker compose up')(graph), tasked('docker compose up --wait --no-build')(graph)],
                [
                    ['api:build--buildx', 'stack:build--compose', 'stack:check--buildx'],
                    ['api:serve--compose', 'stack:install--compose', 'store:install--compose'],
                    ['api:serve--compose', 'stack:install--compose', 'store:install--compose'],
                ],
                `${SOURCE.split} docker:build runs docker build, docker:run starts the built image`,
            ),
        ),
    );

    methods.effect('builds the image of a service in the project holding its Dockerfile, after the other build targets of the project and the builds of its dependencies', () =>
        inferred(subject).pipe(
            Effect.map(running('docker compose build --with-dependencies')),
            Effect.map(Array.map(({ project, target, command, cwd: folder, continuous, after }) => ({ project, target, command, folder, continuous, after }))),
            Effect.map((built) => assert.deepStrictEqual(built, [{ project: 'api', target: 'build--buildx', command: 'docker compose build --with-dependencies api', folder: 'stack', continuous: false, after: ['build--!(compose|buildx)', '^build'] }], `${SOURCE.split} docker:build dependsOn build, ^build`)),
        ),
    );

    methods.effect('serves a service from the project holding its Dockerfile, as a continuous target in the Compose folder that waits on the image build and whose ready check names the service alone', () =>
        inferred(subject).pipe(
            Effect.map(running('docker compose up')),
            Effect.map(Array.filter(({ continuous }) => continuous)),
            Effect.map(Array.map(({ project, target, cwd: folder, after, readyWhen }) => ({ project, target, folder, after, readyWhen }))),
            Effect.map((served) =>
                assert.deepStrictEqual(
                    served,
                    [{ project: 'api', target: 'serve--compose', folder: 'stack', after: ['build--buildx'], readyWhen: getReadyCheckReadyWhen(`${manifest.name}/ready-check`, undefined, 'api') }],
                    `${SOURCE.split} docker:run dependsOn docker:build, packages/devkit/src/utils/readiness.spec.ts getReadyCheckReadyWhen, packages/nx/src/tasks-runner/readiness/ready-when.spec.ts "gives a command check the env and options the producer runs with"`,
                ),
            ),
        ),
    );

    methods.effect('states a project folder that holds a space or a shell metacharacter in a command, a ready check, or a runtime input only inside single quotes', () =>
        Effect.map(
            inferred({
                ...subject,
                files: {
                    'package.json': '{ "name": "root" }\n',
                    'my proj/compose.yaml': 'name: spaced\nservices:\n    app:\n        build:\n            context: .\n',
                    'my proj/Dockerfile': 'FROM scratch\nCOPY compose.yaml /compose.yaml\n',
                    'a;touch pwned;$(id)/compose.yaml': 'name: pwned\nservices:\n    cache:\n        build:\n            context: .\n',
                    'a;touch pwned;$(id)/Dockerfile': 'FROM scratch\nCOPY compose.yaml /compose.yaml\n',
                },
            }),
            (graph) =>
                assert.deepStrictEqual(
                    [tasked('docker compose up')(graph), unquoted(graph)],
                    [['pwned:install--compose', 'pwned:serve--compose', 'spaced:install--compose', 'spaced:serve--compose'], []],
                    'nrwl/nx@2a424d3 packages/docker/src/plugins/plugin.spec.ts image reference generation "should handle spaces in project path", "should replace shell metacharacters in project path"',
                ),
        ),
    );
});

describe.runIf(inject('driven') && present(subject))(`${manifest.name} ready check on a Compose project docker starts`, () => it.layer(layer, { excludeTestServices: true })(started));
