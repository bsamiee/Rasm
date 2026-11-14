import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import web from '@nx/web/package.json' with { type: 'json' };
import plugin from '@rasm/nx-storybook/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import framework from '@storybook/react-vite/package.json' with { type: 'json' };
import { Effect, Formatter, Layer, Path, Predicate, Record } from 'effect';
import react from 'react/package.json' with { type: 'json' };
import dom from 'react-dom/package.json' with { type: 'json' };
import storybook from 'storybook/package.json' with { type: 'json' };
import vite from 'vite/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREAD = /(?:^|[\s"'])-{0,2}version\b|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: '5 minutes', test: 300_000 } as const;
const main = "export default { stories: ['../*.stories.tsx'], framework: { name: '@storybook/react-vite', options: {} } };\n";
const story = "export default { title: 'Label', component: () => 'label' };\n\nexport const Shown = {};\n";
const files = {
    'apps/docs/package.json': '{ "name": "docs", "type": "module" }\n',
    'apps/docs/.storybook/main.ts': main,
    'apps/docs/Label.stories.tsx': story,
    'apps/scripted/package.json': '{ "name": "scripted", "type": "module" }\n',
    'apps/scripted/.storybook/main.js': main,
    'apps/scripted/Label.stories.tsx': story,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const tooled = ({ nodes }: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => Record.filter(nodes[project]?.data.targets ?? {}, (_, name) => name.startsWith(`${verb}--`));

const verbs = (graph: Pick<ProjectGraph, 'nodes'>, project: string) => ['build', 'serve', 'preview'].filter((verb) => !Record.isEmptyRecord(tooled(graph, project, verb)));

const task = (graph: Pick<ProjectGraph, 'nodes'>, project: string, verb: string) => `${project}:${Record.keys(tooled(graph, project, verb))[0] ?? verb}`;

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>>) =>
    Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name] }, packages: [storybook.name, framework.name, react.name, dom.name, vite.name, web.name], ignored: ['storybook-static'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const spec = 'nrwl/nx packages/storybook/src/plugins/plugin.spec.ts';
    const principle = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
    const tasks = 'README.md TASKS';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads configuration and builds no Storybook`);
        }),
        bounds.test,
    );

    methods.effect.each([
        ['docs', 'should create nodes'],
        ['scripted', 'should support main.js'],
    ] as const)(
        'a main config in .storybook gives the project of the parent folder build, serve, and preview targets: %s',
        Effect.fnUntraced(function* ([project, title]) {
            const graph = yield* read;
            assert.deepStrictEqual(graph.failures, [], `${spec} "${title}": a valid workspace reads without failure`);
            assert.strictEqual(graph.nodes[project]?.data.root, `apps/${project}`, `${spec} "${title}": the project root is the parent of .storybook`);
            assert.deepStrictEqual(verbs(graph, project), ['build', 'serve', 'preview'], `${spec} "${title}": build-storybook, serve-storybook, and static-storybook; ${principle}`);
        }),
        bounds.test,
    );

    methods.effect(
        'each verb depends on its tool target',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            const { docs } = graph.nodes;
            assert.deepStrictEqual(
                ['build', 'serve', 'preview'].filter((verb) => {
                    const rows = new Set(docs?.data.targets?.[verb]?.dependsOn);
                    return rows.has(`${verb}--*`) || rows.isSupersetOf(new Set(Record.keys(tooled(graph, 'docs', verb))));
                }),
                ['build', 'serve', 'preview'],
                principle,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'the build is cached, hashes the installed storybook, and writes to storybook-static',
        Effect.fnUntraced(function* () {
            const [{ outputs }, graph] = yield* Effect.all([Consumer, read]);
            const [build] = Object.values(tooled(graph, 'docs', 'build'));
            assert.isTrue(build?.cache, `${spec} "should create nodes": build-storybook states cache true`);
            assert.isTrue(
                (build?.inputs ?? []).some((row) => Predicate.isObject(row) && 'externalDependencies' in row && row.externalDependencies.includes('storybook')),
                `${spec} "should create nodes": inputs name external dependency storybook`,
            );
            assert.include((yield* outputs(task(graph, 'docs', 'build'))).outputPaths, 'apps/docs/storybook-static', `${spec} "should create nodes": outputs hold {projectRoot}/storybook-static, the default --output-dir of storybook build`);
        }),
        bounds.test,
    );

    methods.effect(
        'the development server runs continuously and states how Nx detects it ready',
        Effect.fnUntraced(function* () {
            const [serve] = Object.values(tooled(yield* read, 'docs', 'serve'));
            assert.isTrue(serve?.continuous, `${spec} "should create nodes": serve-storybook states continuous true; ${tasks} "serve runs a project's development servers until the run ends"`);
            assert.isObject(serve?.readyWhen, `${spec} "should create nodes": serve-storybook states readyWhen`);
        }),
        bounds.test,
    );

    methods.effect(
        'the preview serves the build output through the Nx file server after the build',
        Effect.fnUntraced(function* () {
            const [{ outputs }, graph] = yield* Effect.all([Consumer, read]);
            const [preview] = Object.values(tooled(graph, 'docs', 'preview'));
            const built = Record.keys(tooled(graph, 'docs', 'build'));
            const title = 'should create nodes';
            assert.strictEqual(preview?.executor, '@nx/web:file-server', `${spec} "${title}": static-storybook runs @nx/web:file-server`);
            assert.isTrue(preview?.continuous, `${spec} "${title}": static-storybook states continuous true`);
            assert.isTrue(
                (preview?.dependsOn ?? []).some((row) => row === 'build' || built.includes(Predicate.isString(row) ? row : row.target)),
                `${spec} "${title}": static-storybook depends on build-storybook; ${tasks} "preview builds a project, then serves its build output as serve does"`,
            );
            assert.include((yield* outputs(task(graph, 'docs', 'build'))).outputPaths, preview?.options?.staticFilePath, `${spec} "${title}": staticFilePath is the output folder of the build`);
        }),
        bounds.test,
    );

    methods.effect(
        'plugin isolation on and off give equal projects',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'docs', '--json']);
            const [isolated, shared] = [yield* printed(true), yield* printed(false)];
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

    methods.effect(
        'storybook build writes every file under the declared outputs',
        Effect.fnUntraced(function* () {
            const [{ nx, outputs, written }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const [{ code, stdout, stderr }, built] = yield* written(nx('run', task(graph, 'docs', 'build'), '--output-style=static'));
            const { expandedOutputs } = yield* outputs(task(graph, 'docs', 'build'));
            const stored = new RegExp(`^(?:${expandedOutputs.map(RegExp.escape).join('|')})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(code, 0, `nx run ${task(graph, 'docs', 'build')} exits 0: ${stdout}${stderr}`);
            assert.include(built, 'apps/docs/storybook-static/index.html', 'storybook build writes storybook-static/index.html');
            assert.deepStrictEqual(
                built.filter((file) => !(file.includes('node_modules') || stored.test(file))),
                [],
                'nx/schemas/project-schema.json outputs: every file the build writes outside a node_modules cache lies under a declared output',
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'apps/broken/package.json': '{ "name": "broken", "type": "module" }\n', 'apps/broken/.storybook/main.ts': "throw new Error('broken on load');\n" }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a config that fails to load is named in the failure, and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            const graph = yield* read;
            assert.include(
                graph.failures.flatMap(({ files: failed }) => failed).map(({ file }) => file),
                'apps/broken/.storybook/main.ts',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.deepStrictEqual(verbs(graph, 'docs'), ['build', 'serve', 'preview'], `${errors}: "It allows Nx to recieve partial results and continue processing"`);
        }),
        bounds.test,
    );
});
