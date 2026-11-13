import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import plugin from '@rasm/nx-docker/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Effect, Formatter, Layer, Path, Predicate, Record, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREAD = /--version|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: '5 minutes', test: 120_000 } as const;
const files = {
    'shared/compose.base.yaml': 'services:\n    store:\n        image: valkey/valkey:8\n',
    'infra/package.json': '{ "name": "infra", "type": "module" }\n',
    'infra/compose.yaml':
        'include:\n    - ../shared/compose.base.yaml\nservices:\n    api:\n        build:\n            context: ../apps/api\n        ports:\n            - "8080:8080"\n        environment:\n            API_TOKEN: $API_TOKEN\n        depends_on:\n            - store\n    traces:\n        image: jaegertracing/jaeger:2\n        profiles:\n            - observed\n',
    'ops/package.json': '{ "name": "ops", "type": "module" }\n',
    'ops/compose.yaml': 'services:\n    proxy:\n        image: traefik:v3\n',
    'apps/api/package.json': '{ "name": "api", "type": "module" }\n',
    'apps/api/Dockerfile': 'FROM scratch\nCOPY package.json /package.json\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Model = Schema.fromJsonString(Schema.Struct({ services: Schema.Record(Schema.String, Schema.Struct({ build: Schema.optionalKey(Schema.Struct({ context: Schema.String })), image: Schema.optionalKey(Schema.String) })) })).annotate({
    description: 'Fields of the model docker compose config --format json prints: each service with the build context it resolves, or its image',
});
const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const composed = Effect.fnUntraced(function* (folder: string, ...args: readonly string[]) {
    const { called } = yield* Consumer;
    const { checked } = yield* called('docker', ['compose', 'config', ...args], folder);
    return yield* checked;
});

const tooled = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => Record.filter(nodes[project]?.data.targets ?? {}, (_, name) => name.startsWith(`${verb}--`));

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>>) => Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, ignored: [], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-docker over two Compose projects and one image', (methods) => {
    const spec = 'nrwl/nx packages/docker/src/plugins/plugin.spec.ts';
    const tasks = 'README.md TASKS "nx install @rasm/infra starts local services of infra/compose.yaml, -c <profile> adds the services of one profile"';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads configuration, builds no image, and starts no service`);
        }),
        bounds.test,
    );

    methods.effect(
        'a Compose file beside a package manifest gives its project an install target the install verb aggregates',
        Effect.fnUntraced(function* () {
            const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const graph = yield* read;
            const names = Record.keys(tooled(graph, 'infra', 'install'));
            const { infra } = graph.nodes;
            const { install } = infra?.data.targets ?? {};
            const rows = new Set(install?.dependsOn);
            assert.deepStrictEqual(graph.failures, [], 'a valid workspace reads without failure');
            assert.isNotEmpty(names, `${tasks}; ${principle}: an install--<tool> target starts the services`);
            assert.isTrue(rows.has('install--*') || rows.isSupersetOf(new Set(names)), `${principle}: install depends on each install--<tool> target`);
        }),
        bounds.test,
    );

    methods.effect(
        'each Compose profile is a configuration of the install target',
        Effect.fnUntraced(function* () {
            const [graph, profiles] = yield* Effect.all([read, composed('infra', '--profiles')]);
            assert.deepStrictEqual(profiles.trim().split('\n'), ['observed'], 'docker compose config --profiles lists profile observed');
            assert.isTrue(
                Object.values(tooled(graph, 'infra', 'install')).every(({ configurations = {} }) => 'observed' in configurations),
                tasks,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'the project holding the Dockerfile of a service gets the image build and the service start',
        Effect.fnUntraced(function* () {
            const [{ root }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const { services } = yield* Effect.flatMap(composed('infra', '--format', 'json'), Schema.decodeEffect(Model));
            const contexts = Array.filterMap(Object.values(services), ({ build }) => (build === undefined ? Result.failVoid : Result.succeed(path.relative(root, build.context))));
            assert.deepStrictEqual(contexts, ['apps/api'], 'docker compose config resolves the build context of service api');
            assert.deepStrictEqual(
                Object.values(graph.nodes)
                    .filter(({ name }) => !(Record.isEmptyRecord(tooled(graph, name, 'build')) || Record.isEmptyRecord(tooled(graph, name, 'serve'))))
                    .map(({ data }) => data.root),
                contexts,
                `${spec} "should create nodes based on Dockerfile": the Dockerfile folder holds the image build; README.md TASKS "serve ... starts the Compose services whose Dockerfile it holds, which stay up"`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'the start of a service depends on its image build',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const built = Record.keys(tooled(graph, 'api', 'build'));
            assert.isTrue(
                Object.values(tooled(graph, 'api', 'serve'))
                    .map(({ dependsOn = [] }) => dependsOn)
                    .every(Array.some((row) => row === 'build' || built.includes(Predicate.isString(row) ? row : row.target))),
                `${spec} "dependency handling > should set run target to depend on build target"`,
            );
        }),
        bounds.test,
    );

    methods.effect.each(['shared/compose.base.yaml', 'apps/api/Dockerfile'] as const)(
        'a change to %s marks the Compose project affected',
        Effect.fnUntraced(function* (file) {
            const { nx } = yield* Consumer;
            const { checked } = yield* nx('show', 'projects', '--affected', `--files=${file}`, '--json');
            assert.include(yield* Effect.flatMap(checked, Schema.decodeEffect(Names)), 'infra', `README.md TASKS "--files=<path> limits it to one change's reach": infra/compose.yaml includes shared/compose.base.yaml and builds apps/api`);
        }),
        bounds.test,
    );

    methods.effect(
        'plugin isolation on and off give equal projects',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'api', '--json']);
            const [isolated, shared] = [yield* printed(true), yield* printed(false)];
            assert.deepStrictEqual([shared.code, shared.stdout], [isolated.code, isolated.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'no input of an inferred target is a --version command, plugin source, mise.lock, or a credential variable',
        Effect.fnUntraced(function* () {
            const rows = Object.values((yield* read).nodes)
                .flatMap(({ data }) => Object.values(data.targets ?? {}))
                .flatMap(({ inputs = [] }) => inputs)
                .map((row) => Formatter.format(row));
            assert.deepStrictEqual(
                rows.filter((row) => UNREAD.test(row)),
                [],
                'README.md PRINCIPLES "Task inputs hold the files a task\'s tool reads: plugin and executor source, .config/mise.lock, tool --version output, and credentials are no input"',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'two reads of one workspace give equal projects, and a file no config reads changes none',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            const [first, second] = [yield* read, yield* read];
            yield* wrote({ 'docs/notes.md': '# Notes\n' });
            assert.deepStrictEqual(second.nodes, first.nodes, `${api}: one set of files gives one set of nodes`);
            assert.deepStrictEqual((yield* read).nodes, first.nodes, `${api}: a file no config reads changes no node`);
        }),
        bounds.test,
    );

    methods.effect(
        'a service given a build context gets its project the image build on the next read',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* wrote({ 'ops/compose.yaml': 'services:\n    proxy:\n        build:\n            context: ../apps/edge\n', 'apps/edge/package.json': '{ "name": "edge", "type": "module" }\n', 'apps/edge/Dockerfile': 'FROM scratch\n' });
            assert.isNotEmpty(Record.keys(tooled(yield* read, 'edge', 'build')), 'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts: a changed file invalidates what was computed from it (history-tests-nx-behaviour catalogue 18, 24: stale results after a config edit)');
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'ops/compose.yaml': 'services:\n    proxy:\n        image: [traefik\n' }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-docker with a Compose file that fails to load', (methods) => {
    methods.effect(
        'the failure names the Compose file and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            const graph = yield* read;
            assert.include(
                graph.failures.flatMap(({ files: failed }) => failed).map(({ file }) => file),
                'ops/compose.yaml',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.isNotEmpty(Record.keys(tooled(graph, 'infra', 'install')), `${errors}: "It allows Nx to recieve partial results and continue processing"`);
            assert.isNotEmpty(Record.keys(tooled(graph, 'api', 'build')), `${errors}: partial results hold every project whose file loaded`);
        }),
        bounds.test,
    );
});
