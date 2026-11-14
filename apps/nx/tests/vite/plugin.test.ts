import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import nxVite from '@nx/vite/package.json' with { type: 'json' };
import plugin from '@rasm/nx-vite/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import { Duration, Effect, Formatter, Layer, Path, Predicate, Record, Schema, Struct } from 'effect';
import vite from 'vite/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCHEMA = 'nx/schemas/project-schema.json';
const UNREAD = /(?:^|[\s"'])-{0,2}version\b|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: '5 minutes', test: Duration.toMillis('2 minutes') } as const;
const html = '<!doctype html><script type="module" src="./main.ts"></script>\n';
const entry = 'export const entry = 1;\n';
const library = "lib: { entry: 'index.ts', formats: ['es'] }";
const base = (outDir: string) => `export const base = { build: { outDir: '${outDir}' } };\n`;
const files = {
    'tools/vite/base.ts': base('out'),
    'apps/web/package.json': '{ "name": "web", "type": "module" }\n',
    'apps/web/index.html': html,
    'apps/web/main.ts': entry,
    'apps/web/vite.config.ts': "import { defineConfig } from 'vite';\n\nexport default defineConfig({ server: { port: 4321, strictPort: true }, preview: { port: 4322, strictPort: true } });\n",
    'apps/signaled/package.json': '{ "name": "signaled", "type": "module" }\n',
    'apps/signaled/index.html': html,
    'apps/signaled/main.ts': entry,
    'apps/signaled/vite.config.ts': "import { nxReadySignal } from '@nx/vite/plugins/nx-ready-signal.plugin';\nimport { defineConfig } from 'vite';\n\nexport default defineConfig({ plugins: [nxReadySignal()] });\n",
    'libs/kit/package.json': '{ "name": "kit", "type": "module" }\n',
    'libs/kit/index.ts': entry,
    'libs/kit/vite.config.ts': `import { defineConfig, mergeConfig } from 'vite';\nimport { base } from '../../tools/vite/base.ts';\n\nexport default defineConfig(mergeConfig(base, { build: { ${library} } }));\n`,
    'libs/served/package.json': '{ "name": "served", "type": "module" }\n',
    'libs/served/index.ts': entry,
    'libs/served/vite.config.ts': `import { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { ${library} }, server: { port: 3000 } });\n`,
    'libs/hooked/package.json': '{ "name": "hooked", "type": "module" }\n',
    'libs/hooked/index.ts': entry,
    'libs/hooked/vite.config.ts': `import { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { ${library} }, plugins: [{ name: 'user-plugin', config: () => ({ server: { port: 4200 } }) }] });\n`,
    'libs/builds/package.json': '{ "name": "builds", "type": "module" }\n',
    'libs/builds/index.ts': entry,
    'libs/builds/vite.config.ts': `import { defineConfig } from 'vite';\n\nexport default defineConfig(({ command }) => {\n    if (command === 'serve') {\n        throw new Error('This config only builds');\n    }\n    return { build: { ${library} } };\n});\n`,
    'libs/marked/package.json': '{ "name": "marked", "type": "module" }\n',
    'libs/marked/index.ts': entry,
    'libs/marked/vite.config.ts': `import { writeFileSync } from 'node:fs';\nimport { defineConfig } from 'vite';\n\nexport default defineConfig({ build: { ${library} }, plugins: [{ name: 'marker', buildStart: () => writeFileSync(new URL('./started.marker', import.meta.url), '') }] });\n`,
};

// --- [MODELS] --------------------------------------------------------------------------

const Resolved = Schema.fromJsonString(Schema.Struct({ root: Schema.String, outDir: Schema.String, server: Schema.Finite, preview: Schema.Finite, loaded: Schema.Array(Schema.String) })).annotate({
    description: 'Fields of the configuration vite resolveConfig returns for one command: root, build.outDir, server.port, preview.port, and the files the config file loaded',
});
const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const resolved = Effect.fnUntraced(function* (folder: string, command: 'build' | 'serve') {
    const { evaluated } = yield* Consumer;
    const printed = yield* evaluated(
        `import { resolveConfig } from 'vite';
const { root, build, server, preview, configFileDependencies } = await resolveConfig({ logLevel: 'silent' }, process.argv[1]);
console.log(JSON.stringify({ root, outDir: build.outDir, server: server.port, preview: preview.port, loaded: configFileDependencies }));`,
        [command],
        folder,
    );
    return yield* Schema.decodeEffect(Resolved)(printed);
});

const declared = Effect.fnUntraced(function* (name: string) {
    const { outputs } = yield* Consumer;
    return (yield* outputs(name)).outputPaths;
});

const tooled = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => Record.filter(nodes[project]?.data.targets ?? {}, (_, name) => name.startsWith(`${verb}--`));

const verbs = (graph: Pick<ProjectGraph, 'nodes'>, project: string) => ['build', 'serve', 'preview'].filter((verb) => !Record.isEmptyRecord(tooled(graph, project, verb)));

const task = (graph: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => `${project}:${Record.keys(tooled(graph, project, verb))[0] ?? verb}`;

const workspace = (stated: Readonly<Record<string, string>>) => Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, packages: [vite.name, nxVite.name], ignored: ['dist', 'out', 'emitted'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const spec = 'nrwl/nx packages/vite/src/plugins/plugin.spec.ts';
    const snapshot = 'nrwl/nx packages/vite/src/plugins/__snapshots__/plugin.spec.ts.snap';
    const tasks = 'README.md TASKS';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'a Vite config beside a package manifest makes its folder a project with a build target the build verb aggregates',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const { web } = graph.nodes;
            const names = Record.keys(tooled(graph, 'web', 'build'));
            const { build } = web?.data.targets ?? {};
            const rows = new Set(build?.dependsOn);
            assert.deepStrictEqual(graph.failures, [], `${spec} "not root project > should create nodes": a valid workspace reads without failure`);
            assert.strictEqual(web?.data.root, 'apps/web', `${spec} "not root project > should create nodes": the project root is the folder of vite.config.ts`);
            assert.lengthOf(names, 1, `${principle}: one build--<tool> target runs the Vite build`);
            assert.isTrue(rows.has('build--*') || rows.isSupersetOf(new Set(names)), `${principle}: build depends on its build--<tool> target`);
        }),
        bounds.test,
    );

    methods.effect(
        'an application config gets build, serve, and preview targets',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(verbs(yield* read, 'web'), ['build', 'serve', 'preview'], `${snapshot} "not root project > should create nodes 1": an application holds build, dev server, and preview targets`);
        }),
        bounds.test,
    );

    methods.effect.each([
        ['web', 'application', 'not root project > should create nodes 1'],
        ['kit', 'library', 'Library mode > should exclude serve and preview targets when vite.config.ts is in library mode 1'],
    ] as const)(
        'the project type follows build.lib: %s',
        Effect.fnUntraced(function* ([project, type, title]) {
            assert.strictEqual((yield* read).nodes[project]?.data.projectType, type, `${snapshot} "${title}": projectType is ${type}`);
        }),
        bounds.test,
    );

    methods.effect(
        'a library-mode config stating no server gets a build target and neither serve nor preview',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(verbs(yield* read, 'kit'), ['build'], `${spec} "Library mode > should exclude serve and preview targets when vite.config.ts is in library mode"`);
        }),
        bounds.test,
    );

    methods.effect.each([
        ['served', 'should not exclude serve and preview targets when vite.config.ts is in library mode when user has defined a server config'],
        ['hooked', 'should keep the application targets when a plugin config hook sets the server in both modes'],
    ] as const)(
        'a library-mode config with a server keeps serve and preview: %s',
        Effect.fnUntraced(function* ([project, title]) {
            assert.deepStrictEqual(verbs(yield* read, project), ['build', 'serve', 'preview'], `${spec} "Library mode > ${title}"`);
        }),
        bounds.test,
    );

    methods.effect(
        'a config that throws for the serve command keeps its build target and gets no server target',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const title = 'Library mode > should keep the build target and omit the application targets when the config throws in serve mode';
            assert.deepStrictEqual(graph.failures, [], `${spec} "${title}": a serve-mode throw is no graph failure`);
            assert.deepStrictEqual(verbs(graph, 'builds'), ['build'], `${spec} "${title}"`);
        }),
        bounds.test,
    );

    methods.effect(
        'the build is cached, depends on the builds of its dependencies, and hashes the installed vite',
        Effect.fnUntraced(function* () {
            const [build] = Object.values(tooled(yield* read, 'web', 'build'));
            assert.isTrue(build?.cache, `${snapshot} "not root project > should create nodes 1": build states cache true`);
            assert.include(build?.dependsOn ?? [], '^build', `${snapshot} "not root project > should create nodes 1": build depends on ^build`);
            assert.deepInclude(build?.inputs ?? [], { externalDependencies: ['vite'] }, `${snapshot} "not root project > should create nodes 1": inputs name external dependency vite`);
        }),
        bounds.test,
    );

    methods.effect.each(['web', 'kit'] as const)(
        'the build outputs of %s are the folder Vite resolves as build.outDir',
        Effect.fnUntraced(function* (project) {
            const [{ root }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const { root: folder, outDir } = yield* resolved(graph.nodes[project]?.data.root ?? project, 'build');
            assert.deepStrictEqual(
                yield* declared(task(graph, project, 'build')),
                [path.relative(root, path.resolve(folder, outDir))],
                `${spec} "should keep the application targets when only the serve config has a server, with build outputs from the build config": outputs are the outDir vite resolveConfig returns for command build`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'the development server runs continuously and waits on the port Vite resolves',
        Effect.fnUntraced(function* () {
            const [graph, { server }] = yield* Effect.all([read, resolved('apps/web', 'serve')]);
            const [serve] = Object.values(tooled(graph, 'web', 'serve'));
            assert.isTrue(serve?.continuous, `${snapshot} "not root project > should create nodes 1": the dev server target states continuous true; ${tasks} "serve runs a project's development servers until the run ends"`);
            assert.deepStrictEqual(serve?.readyWhen, { port: server }, `${SCHEMA} readyWhen.port "Ready when a TCP connection to this port succeeds": the port is server.port of vite resolveConfig`);
        }),
        bounds.test,
    );

    methods.effect(
        'the preview runs continuously after the build and waits on the preview port Vite resolves',
        Effect.fnUntraced(function* () {
            const [graph, { preview: port }] = yield* Effect.all([read, resolved('apps/web', 'serve')]);
            const [preview] = Object.values(tooled(graph, 'web', 'preview'));
            assert.isTrue(preview?.continuous, `${snapshot} "not root project > should create nodes 1": the preview target states continuous true`);
            assert.isTrue(
                (preview?.dependsOn ?? []).some((row) => Predicate.isString(row) && (row === 'build' || Record.has(tooled(graph, 'web', 'build'), row))),
                `${tasks} "preview builds a project, then serves its build output as serve does"; ${snapshot}: preview depends on the build target`,
            );
            assert.deepStrictEqual(preview?.readyWhen, { port }, `${SCHEMA} readyWhen.port: the port is preview.port of vite resolveConfig`);
        }),
        bounds.test,
    );

    methods.effect(
        'a config holding nxReadySignal() waits for the signal on its server and its preview',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const title = 'root project > waits for the signal when the config loads nxReadySignal()';
            assert.deepStrictEqual(
                ['serve', 'preview'].map((verb) => Object.values(tooled(graph, 'signaled', verb)).map(Struct.get('readyWhen'))),
                [[{ signal: true }], [{ signal: true }]],
                `${spec} "${title}"`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'every file the config file loads is an input of the build',
        Effect.fnUntraced(function* () {
            const [{ inputs, root }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const { loaded } = yield* resolved('libs/kit', 'build');
            assert.includeMembers(
                [...(yield* inputs(task(graph, 'kit', 'build'))).files],
                loaded.map((file) => path.relative(root, file)),
                `${SCHEMA} inputs; nrwl/nx packages/vitest/src/plugins/setup-file-inputs.spec.ts "should declare a setup file that lives outside the project root": files a config reads outside its project are inputs, here configFileDependencies of vite resolveConfig`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'a change to a file the config loads marks the project affected, and nx affected -t build plans its build',
        Effect.fnUntraced(function* () {
            const [{ nx, planned }, graph] = yield* Effect.all([Consumer, read]);
            const { checked } = yield* nx('show', 'projects', '--affected', '--files=tools/vite/base.ts', '--json');
            const selected = yield* planned('affected', '--targets=build', '--files=tools/vite/base.ts');
            assert.include(Record.keys(selected.tasks), task(graph, 'kit', 'build'), 'nrwl/nx packages/nx/src/native/affected/tasks/selection.rs: only the targets the command asked for are kept, and affectedness never travels along plain dependsOn, so verb build states the inputs of build--<tool>');
            assert.include(yield* Effect.flatMap(checked, Schema.decodeEffect(Names)), 'kit', `${tasks} "--files=<path> limits it to one change's reach": libs/kit/vite.config.ts imports tools/vite/base.ts`);
        }),
        bounds.test,
    );

    methods.effect(
        'reading the graph writes no file and starts no build',
        Effect.fnUntraced(function* () {
            const [, created] = yield* Effect.flatMap(Consumer, ({ written }) => written(read));
            assert.deepStrictEqual(created, [], `${api}: createNodes reads configuration; buildStart of a Vite plugin runs on a build alone, and a graph read is none (history-tests-nx-behaviour catalogue 25: a graph read deleted build output)`);
        }),
        bounds.test,
    );

    methods.effect(
        'plugin isolation on and off give equal projects',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const shown = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'web', '--json']);
            const [isolated, shared] = [yield* shown(true), yield* shown(false)];
            assert.deepStrictEqual([shared.code, shared.stdout], [isolated.code, isolated.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );

    methods.effect(
        'no input of an inferred target is a version command (--version, -version, <tool> version), plugin source, mise.lock, or a credential variable',
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
});

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'vite build writes every file under the declared outputs',
        Effect.fnUntraced(function* () {
            const [{ nx, outputs, written }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const [{ code, stdout, stderr }, created] = yield* written(nx('run', task(graph, 'kit', 'build'), '--output-style=static'));
            const { expandedOutputs: stored } = yield* outputs(task(graph, 'kit', 'build'));
            const under = new RegExp(`^(?:${stored.map(RegExp.escape).join('|')})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(code, 0, `nx run ${task(graph, 'kit', 'build')} exits 0: ${stdout}${stderr}`);
            assert.isNotEmpty(created, 'vite build writes files');
            assert.deepStrictEqual(
                created.filter((file) => !under.test(file)),
                [],
                `${SCHEMA} outputs: every file the build writes lies under a declared output`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'an edit to a file the config loads moves the declared outputs to the folder Vite resolves',
        Effect.fnUntraced(function* () {
            const [{ root, wrote }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            yield* wrote({ 'tools/vite/base.ts': base('emitted') });
            const { root: folder, outDir } = yield* resolved('libs/kit', 'build');
            assert.deepStrictEqual(
                yield* declared(task(graph, 'kit', 'build')),
                [path.relative(root, path.resolve(folder, outDir))],
                'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts: a changed file invalidates what was computed from it (history-tests-nx-behaviour catalogue 18, 24: stale results after a config edit)',
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'apps/broken/package.json': '{ "name": "broken", "type": "module" }\n', 'apps/broken/vite.config.ts': "throw new Error('broken on load');\n" }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a config that fails to load is named in the failure, and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            assert.include(
                graph.failures.flatMap((failure) => failure.files).map(({ file }) => file),
                'apps/broken/vite.config.ts',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.deepStrictEqual(
                ['web', 'kit'].map((project) => verbs(graph, project)),
                [['build', 'serve', 'preview'], ['build']],
                `${errors}: "It allows Nx to recieve partial results and continue processing"`,
            );
        }),
        bounds.test,
    );
});
