import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import plugin from '@rasm/nx-vitest/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Duration, Effect, Formatter, Layer, Path, Record, Schema } from 'effect';
import vite from 'vite/package.json' with { type: 'json' };
import vitest from 'vitest/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPEC = 'nrwl/nx packages/vitest/src/plugins/plugin-vitest.spec.ts';
const SCHEMA = 'nx/schemas/project-schema.json';
const UNREAD = /--version|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: '5 minutes', test: Duration.toMillis('3 minutes') } as const;
const passing = "import { expect, test } from 'vitest';\n\ntest('adds', () => expect(1 + 1).toBe(2));\n";
const listing = (...names: readonly string[]) => {
    const rows = names.map((name) => `{ root: 'libs/${name}', test: { name: '${name}' } }`);
    return `import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { projects: [${rows.join(', ')}] } });\n`;
};
const configured = {
    'tsconfig.shared.json': '{ "compilerOptions": { "strict": true } }\n',
    'tools/vitest/setup.ts': 'export {};\n',
    'tools/vitest/tsconfig.json': '{ "extends": "../../tsconfig.shared.json" }\n',
    'libs/math/package.json': '{ "name": "math", "type": "module" }\n',
    'libs/math/tsconfig.json': '{ "extends": "../../tsconfig.shared.json" }\n',
    'libs/math/sum.test.ts': passing,
    'libs/math/vitest.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { setupFiles: ['../../tools/vitest/setup.ts'] } });\n",
    'libs/viteful/package.json': '{ "name": "viteful", "type": "module" }\n',
    'libs/viteful/sum.test.ts': passing,
    'libs/viteful/vite.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { include: ['*.test.ts'] } });\n",
    'libs/plain/package.json': '{ "name": "plain", "type": "module" }\n',
    'libs/plain/index.ts': 'export const plain = 1;\n',
    'libs/plain/vite.config.ts': "import { defineConfig } from 'vite';\n\nexport default defineConfig({});\n",
    'libs/marked/package.json': '{ "name": "marked", "type": "module" }\n',
    'libs/marked/sum.test.ts': passing,
    'libs/marked/vitest.config.ts': "import { writeFileSync } from 'node:fs';\nimport { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ plugins: [{ name: 'marker', configureServer: () => writeFileSync(new URL('./served.marker', import.meta.url), '') }] });\n",
};
const rooted = {
    'vitest.config.ts': listing('one', 'two'),
    'libs/one/package.json': '{ "name": "one", "type": "module" }\n',
    'libs/one/one.test.ts': passing,
    'libs/two/package.json': '{ "name": "two", "type": "module" }\n',
    'libs/two/two.test.ts': "import { expect, test } from 'vitest';\n\ntest('fails', () => expect(1).toBe(2));\n",
    'libs/three/package.json': '{ "name": "three", "type": "module" }\n',
    'libs/three/three.test.ts': passing,
};

// --- [MODELS] --------------------------------------------------------------------------

const Projects = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, root: Schema.String, setup: Schema.Array(Schema.String), coverage: Schema.String, cache: Schema.String }))).annotate({
    description: 'Name, root, setup files, coverage folder, and Vite cache folder of each project createVitest of vitest/node resolves from the config of the working directory',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const projects = Effect.fnUntraced(function* (folder: string) {
    const { evaluated } = yield* Consumer;
    const printed = yield* evaluated(
        `import { createVitest } from 'vitest/node';
const vitest = await createVitest('test', { watch: false }, { logLevel: 'silent' });
const projects = vitest.projects.map(({ name, config }) => ({ name, root: config.root, setup: config.setupFiles, coverage: vitest.config.coverage.reportsDirectory, cache: vitest.vite.config.cacheDir }));
await vitest.close();
console.log(JSON.stringify(projects));`,
        [],
        folder,
    );
    return yield* Schema.decodeEffect(Projects)(printed);
});

const tested = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string) => Record.filter(nodes[project]?.data.targets ?? {}, (_, name) => name.startsWith('test--'));

const task = (graph: Pick<ProjectGraph, 'nodes'>, project: string) => `${project}:${Record.keys(tested(graph, project))[0] ?? 'test'}`;

const suites = ({ nodes }: Pick<ProjectGraph, 'nodes'>) =>
    Object.values(nodes)
        .filter(({ name }) => !Record.isEmptyRecord(tested({ nodes }, name)))
        .map(({ data }) => data.root)
        .toSorted();

const workspace = (stated: Readonly<Record<string, string>>) =>
    Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, packages: [vitest.name, vite.name], ignored: ['.artifacts', 'coverage', '*.marker'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(configured), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-vitest over one config per project', (methods) => {
    const snapshot = 'nrwl/nx packages/vitest/src/plugins/__snapshots__/plugin-vitest.spec.ts.snap "root project > should create nodes 1"';
    const inputsSpec = 'nrwl/nx packages/vitest/src/plugins/setup-file-inputs.spec.ts';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file and starts no Vite server',
        Effect.fnUntraced(function* () {
            const [, created] = yield* Effect.flatMap(Consumer, ({ written }) => written(read));
            assert.deepStrictEqual(created, [], `${api}: createNodes reads configuration; configureServer of a Vite plugin runs when a server starts, and a graph read starts none (history-tests-nx-behaviour catalogue 3: a cold graph read launched a browser)`);
        }),
        bounds.test,
    );

    methods.effect(
        'a Vitest config beside a package manifest gives its project a test target the test verb aggregates',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const { math } = graph.nodes;
            const names = Record.keys(tested(graph, 'math'));
            const { test } = math?.data.targets ?? {};
            const rows = new Set(test?.dependsOn);
            assert.deepStrictEqual(graph.failures, [], `${SPEC} "root project > should create nodes": a valid workspace reads without failure`);
            assert.strictEqual(math?.data.root, 'libs/math', `${SPEC} "root project > should create nodes": the project root is the folder of vitest.config.ts`);
            assert.lengthOf(names, 1, `${principle}: one test--<tool> target runs Vitest`);
            assert.isTrue(rows.has('test--*') || rows.isSupersetOf(new Set(names)), `${principle}: test depends on its test--<tool> target`);
        }),
        bounds.test,
    );

    methods.effect(
        'a Vitest config states no project type',
        Effect.fnUntraced(function* () {
            const { math } = (yield* read).nodes;
            assert.isUndefined(math?.data.projectType, `${SPEC} "root project > should not set projectType so it does not override the type from other plugins"`);
        }),
        bounds.test,
    );

    methods.effect(
        'a Vite config stating test gets a test target, and one stating none gets no test target',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(Array.intersection(suites(yield* read), ['libs/viteful', 'libs/plain']), ['libs/viteful'], `nrwl/nx packages/vitest/src/plugins/plugin.ts "if file is vitest.config or vite.config has definition for test, create targets for test"`);
        }),
        bounds.test,
    );

    methods.effect(
        'the test target is cached and hashes the installed vitest and variable CI',
        Effect.fnUntraced(function* () {
            const [run] = Object.values(tested(yield* read, 'math'));
            assert.isTrue(run?.cache, `${snapshot}: test states cache true`);
            assert.deepInclude(run?.inputs ?? [], { externalDependencies: ['vitest'] }, `${snapshot}: inputs name external dependency vitest`);
            assert.deepInclude(run?.inputs ?? [], { env: 'CI' }, `${snapshot}: inputs name environment variable CI`);
        }),
        bounds.test,
    );

    methods.effect(
        'a setup file outside the project root is an input of the test target',
        Effect.fnUntraced(function* () {
            const [{ inputs, root }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const setup = (yield* projects('libs/math')).flatMap((project) => project.setup).map((file) => path.relative(root, file));
            assert.deepStrictEqual(setup, ['tools/vitest/setup.ts'], 'createVitest resolves the setup file of libs/math');
            assert.includeMembers([...(yield* inputs(task(graph, 'math'))).files], setup, `${inputsSpec} "should declare a setup file that lives outside the project root"`);
        }),
        bounds.test,
    );

    methods.effect(
        'the tsconfig of an outside setup file and each config it extends are inputs of the test target',
        Effect.fnUntraced(function* () {
            const [{ inputs }, graph] = yield* Effect.all([Consumer, read]);
            assert.includeMembers([...(yield* inputs(task(graph, 'math'))).files], ['tools/vitest/tsconfig.json', 'tsconfig.shared.json'], `${inputsSpec} "should declare the tsconfig vite resolves for the setup file and its extends chain"`);
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
        'a run writes every file under the declared outputs, which hold the coverage folder Vitest resolves',
        Effect.fnUntraced(function* () {
            const [{ nx, outputs, root, written }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const [{ code, stdout, stderr }, created] = yield* written(nx('run', task(graph, 'math'), '--output-style=static'));
            const [{ outputPaths: folders, expandedOutputs }, resolved] = [yield* outputs(task(graph, 'math')), yield* projects('libs/math')];
            const stored = [...expandedOutputs, ...resolved.map(({ cache }) => path.relative(root, cache))];
            const under = new RegExp(`^(?:${stored.map(RegExp.escape).join('|')})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(code, 0, `nx run ${task(graph, 'math')} exits 0: ${stdout}${stderr}`);
            assert.includeMembers(
                [...folders],
                resolved.map((project) => path.relative(root, path.resolve(project.root, project.coverage))),
                `${snapshot}: outputs hold the coverage folder, here coverage.reportsDirectory of createVitest`,
            );
            assert.deepStrictEqual(
                created.filter((file) => !under.test(file)),
                [],
                `${SCHEMA} outputs: every file the run writes lies under a declared output, or in cacheDir of the Vite config, which is a cache and no output`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace(rooted), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-vitest over one root config listing projects', (methods) => {
    methods.effect(
        'each project Vitest resolves gives the Nx project at its root a test target, and no other project gets one',
        Effect.fnUntraced(function* () {
            const [{ root }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const folders = (yield* projects('.')).map((project) => path.relative(root, project.root)).toSorted();
            assert.deepStrictEqual(folders, ['libs/one', 'libs/two'], 'createVitest resolves the two projects of the root config');
            assert.deepStrictEqual(suites(graph), folders, `${SPEC} "workspace config with projects > should NOT create a project for root config with projects array": the root config is no project, each project it lists is`);
        }),
        bounds.test,
    );

    methods.effect(
        'the root config is an input of each test target',
        Effect.fnUntraced(function* () {
            const [{ inputs }, graph] = yield* Effect.all([Consumer, read]);
            assert.include((yield* inputs(task(graph, 'one'))).files, 'vitest.config.ts', `${SCHEMA} inputs: the config file Vitest loads for the project is hashed`);
        }),
        bounds.test,
    );

    methods.effect(
        'the test target of one project runs the tests of that project alone',
        Effect.fnUntraced(function* () {
            const [{ nx }, graph] = yield* Effect.all([Consumer, read]);
            const [one, two] = [yield* nx('run', task(graph, 'one'), '--output-style=static'), yield* nx('run', task(graph, 'two'), '--output-style=static')];
            assert.strictEqual(one.code, 0, `the run of project one holds libs/one/one.test.ts alone, which passes: ${one.stdout}${one.stderr}`);
            assert.notStrictEqual(two.code, 0, `the run of project two holds libs/two/two.test.ts, which fails: ${two.stdout}${two.stderr}`);
        }),
        bounds.test,
    );

    methods.effect(
        'a project added to the root config gets a test target on the next read',
        Effect.fnUntraced(function* () {
            const [{ root, wrote }, path] = yield* Effect.all([Consumer, Path.Path]);
            yield* wrote({ 'vitest.config.ts': listing('one', 'two', 'three') });
            assert.deepStrictEqual(
                suites(yield* read),
                (yield* projects('.')).map((project) => path.relative(root, project.root)).toSorted(),
                'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts: a changed file invalidates what was computed from it (history-tests-nx-behaviour catalogue 18, 24: stale results after a config edit)',
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...configured, 'libs/broken/package.json': '{ "name": "broken", "type": "module" }\n', 'libs/broken/vitest.config.ts': "throw new Error('broken on load');\n" }), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-vitest with a config that fails to load', (methods) => {
    methods.effect(
        'the failure names the config file and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            assert.include(
                graph.failures.flatMap((failure) => failure.files).map(({ file }) => file),
                'libs/broken/vitest.config.ts',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.includeMembers(suites(graph), ['libs/math', 'libs/viteful'], `${errors}: "It allows Nx to recieve partial results and continue processing"`);
        }),
        bounds.test,
    );
});
