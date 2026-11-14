import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import plugin from '@rasm/nx-typescript/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import tool from 'cf/package.json' with { type: 'json' };
import { Effect, Layer, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: 180_000 } as const;
const worker = (name: string) => `import { defineConfig, defineWorker } from 'cf/config';\nimport { compatibilityDate } from '../../shared/dates.ts';\n\nexport default defineConfig({ worker: defineWorker({ name: '${name}', entrypoint: './worker.ts', compatibilityDate }) });\n`;
const fetched = "export default { fetch: () => new Response('ok') };\n";
const files = {
    'shared/dates.ts': "export const compatibilityDate = '2026-10-01';\n",
    'apps/edge/package.json': '{ "name": "edge", "type": "module" }\n',
    'apps/edge/cloudflare.config.ts': worker('edge'),
    'apps/edge/worker.ts': fetched,
    'apps/direct/package.json': '{ "name": "direct", "type": "module" }\n',
    'apps/direct/cloudflare.config.ts': worker('direct'),
    'apps/direct/worker.ts': fetched,
    'apps/plain/package.json': '{ "name": "plain", "type": "module" }\n',
    'apps/plain/index.ts': 'export const plain = 1;\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const generated = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string) => Record.keys(nodes[project]?.data.targets ?? {}).filter((name) => name.startsWith('codegen--'));

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>>) => Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, packages: [tool.name], ignored: ['.cloudflare', '.wrangler'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';
    const codegen = 'README.md TASKS "codegen writes source code a project\'s tools derive from its schemas and configuration"';

    methods.effect(
        'reading the graph writes no file',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads configuration and generates no types`);
        }),
        bounds.test,
    );

    methods.effect(
        'a cloudflare.config.ts beside a package manifest gives its project a codegen target the codegen verb aggregates, and a package without one gets none',
        Effect.fnUntraced(function* () {
            const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const graph = yield* read;
            const names = generated(graph, 'edge');
            const { edge } = graph.nodes;
            const { codegen: verb } = edge?.data.targets ?? {};
            const rows = new Set(verb?.dependsOn);
            assert.deepStrictEqual(graph.failures, [], 'a valid workspace reads without failure');
            assert.lengthOf(names, 1, `${codegen}; ${principle}: one codegen--<tool> target generates the Worker types`);
            assert.isTrue(rows.has('codegen--*') || rows.isSupersetOf(new Set(names)), `${principle}: codegen depends on its codegen--<tool> target`);
            assert.isEmpty(generated(graph, 'plain'), `${api}: a project holding no cloudflare.config.ts gets no codegen target`);
        }),
        bounds.test,
    );

    methods.effect(
        'a change to a file the config imports marks the project affected',
        Effect.fnUntraced(function* () {
            const { nx } = yield* Consumer;
            const { checked } = yield* nx('show', 'projects', '--affected', '--files=shared/dates.ts', '--json');
            assert.includeMembers([...(yield* Effect.flatMap(checked, Schema.decodeEffect(Names)))], ['edge', 'direct'], 'README.md TASKS "--files=<path> limits it to one change\'s reach": each cloudflare.config.ts imports shared/dates.ts');
        }),
        bounds.test,
    );

    methods.effect(
        'plugin isolation on and off give equal projects',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'edge', '--json']);
            const [isolated, shared] = [yield* printed(true), yield* printed(false)];
            assert.deepStrictEqual([shared.code, shared.stdout], [isolated.code, isolated.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'the codegen target writes the files cf workers types writes, each under a declared output or in the .wrangler cache',
        Effect.fnUntraced(function* () {
            const [{ called, nx, outputs, written }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const [name = 'codegen'] = generated(graph, 'edge');
            const [direct, wrote] = yield* written(called('cf', ['workers', 'types'], 'apps/direct'));
            const [ran, inferred] = yield* written(nx('run', `edge:${name}`, '--output-style=static'));
            const { expandedOutputs } = yield* outputs(`edge:${name}`);
            const held = new RegExp(`^(?:${[...expandedOutputs, 'apps/edge/.wrangler'].map((folder) => RegExp.escape(folder)).join('|')})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(direct.code, 0, `cf workers types exits 0 in apps/direct: ${direct.stdout}${direct.stderr}`);
            assert.strictEqual(ran.code, 0, `nx run edge:${name} exits 0: ${ran.stdout}${ran.stderr}`);
            assert.deepStrictEqual(inferred.map((file) => path.relative('apps/edge', file)).toSorted(), wrote.map((file) => path.relative('apps/direct', file)).toSorted(), `${codegen}: the target writes what cf workers types writes for the same config`);
            assert.deepStrictEqual(
                inferred.filter((file) => !held.test(file)),
                [],
                'nx/schemas/project-schema.json outputs: every file the target writes lies under a declared output, or under .wrangler, which holds the cache and local state of the tool',
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'apps/broken/package.json': '{ "name": "broken", "type": "module" }\n', 'apps/broken/cloudflare.config.ts': "throw new Error('broken on load');\n" }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a cloudflare.config.ts that fails to load loses no other project its codegen target, and a failure names that file alone',
        Effect.fnUntraced(function* () {
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            const graph = yield* read;
            const named = graph.failures.flatMap(({ files: failed }) => failed).map(({ file }) => file);
            assert.isNotEmpty(generated(graph, 'edge'), `${errors}: "It allows Nx to recieve partial results and continue processing"`);
            assert.deepStrictEqual(
                named.filter((file) => file !== 'apps/broken/cloudflare.config.ts'),
                [],
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
        }),
        bounds.test,
    );
});
