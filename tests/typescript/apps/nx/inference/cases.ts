import { homedir, tmpdir } from 'node:os';
import { matchesGlob } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it, type Vitest, vi } from '@effect/vitest';
import { type CreateNodes, type CreateNodesResult, getOutputsForTargetAndConfiguration, type NxPlugin, type ProjectGraph } from '@nx/devkit';
import { graphed, workspace } from '@rasm/test-support/nx/inference';
import { replayed, spawned } from '@rasm/test-support/nx/tools';
import { Array, Effect, FileSystem, Function, type Layer, Path, Predicate, Record, Result, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { getTargetInputs, globWithWorkspaceContext, isAggregateCreateNodesError, LoadedNxPlugin, validateOutputs } from 'nx/src/devkit-internals';
import { isProjectConfigurationsError, type ProjectConfigurationsError } from 'nx/src/project-graph/error-types';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { updateContextWithChangedFiles } from 'nx/src/utils/workspace-context';

// --- [TYPES] ---------------------------------------------------------------------------

interface Subject {
    readonly plugin: NxPlugin & { readonly name: string; readonly createNodes: CreateNodes };
    readonly options?: unknown;
    readonly files: Record.ReadonlyRecord<string, string>;
    readonly packages?: readonly string[];
    readonly tools?: readonly string[];
    readonly state?: readonly string[];
    readonly broken?: readonly [file: string, text: string];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = ['src/main.ts', 'README.md', 'docs/guide/index.md', 'lib/data.json', 'assets/logo.svg', 'crates/core/src/lib.rs', 'App/Program.cs', 'Sources/App/main.swift', 'pkg/module.py', 'proto/sheet.proto', 'config/settings.yaml', 'config/settings.toml'];
const PATTERN = /\/?[*{].*$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);
const Environment = Schema.Struct({ env: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = Effect.fnUntraced(function* ({ plugin, options, files, packages = [], tools = [] }: Subject, missing: readonly string[] = []) {
    const path = yield* Path.Path;
    const root = yield* workspace(files, packages, import.meta.url);
    yield* replayed(fileURLToPath(new URL(`recordings/${path.basename(plugin.name)}.json`, import.meta.url)), root, tools, missing);
    return { root, loaded: new LoadedNxPlugin(plugin, { plugin: plugin.name, options }) };
});

const inferred = (subject: Subject) => Effect.flatMap(opened(subject), ({ root, loaded }) => graphed(root, [loaded]));

const edited = (root: string, file: string, text: string) =>
    Effect.andThen(
        Effect.flatMap(Effect.all([FileSystem.FileSystem, Path.Path]), ([fs, path]) => fs.writeFileString(path.join(root, file), text)),
        Effect.promise(() => updateContextWithChangedFiles(root, [], [file], [])),
    );

const listed = Effect.fnUntraced(function* (root: string, loaded: LoadedNxPlugin, files: readonly string[]) {
    const [, read] = yield* Effect.fromNullishOr(loaded.createNodes);
    const results = yield* Effect.promise(() => read([...files], { workspaceRoot: root, nxJsonConfiguration: readNxJson(root) }));
    return Record.fromIterableWith(results, ([, file, result]): readonly [string, CreateNodesResult] => [file, result]);
});

const targeted = (graph: ProjectGraph) =>
    Object.values(graph.nodes)
        .flatMap((node) => Record.collect(node.data.targets ?? {}, (target, configuration) => ({ node, target, configuration, task: `${node.name}:${target}` })))
        .toSorted((left, right) => left.task.localeCompare(right.task));

const running = (command: string) => (graph: ProjectGraph) =>
    Array.filterMap(targeted(graph), ({ node, target, configuration }) => {
        const { command: one, commands = [], cwd }: { readonly command?: string; readonly commands?: readonly string[]; readonly cwd?: string } = configuration.options ?? {};
        const [inputs, dependsOn] = [configuration.inputs ?? [], configuration.dependsOn ?? []];
        return Result.map(
            Result.fromOption(
                Array.findFirst([...Array.fromNullishOr(one), ...commands], (line) => line === command || line.startsWith(`${command} `)),
                Function.constVoid,
            ),
            (line) => ({
                project: node.name,
                target,
                command: line,
                cwd,
                cache: configuration.cache === true,
                continuous: configuration.continuous === true,
                readyWhen: configuration.readyWhen,
                read: inputs.filter(Predicate.isString).map((input) => input.replace('{workspaceRoot}/', '').replace('{projectRoot}', node.data.root)),
                env: inputs.filter(Schema.is(Environment)).map(({ env }) => env),
                after: dependsOn.filter(Predicate.isString),
                waits: dependsOn.filter(Predicate.isObject),
                outputs: Array.dedupe(getOutputsForTargetAndConfiguration({ project: node.name, target }, {}, node).map((output) => output.replace(PATTERN, ''))),
            }),
        );
    });

const roots = (graph: ProjectGraph) => Record.map(graph.nodes, (node) => node.data.root);

const edges = (graph: ProjectGraph) =>
    Object.values(graph.dependencies)
        .flat()
        .filter(({ target }) => target in graph.nodes)
        .toSorted((left, right) => `${left.source} ${left.target}`.localeCompare(`${right.source} ${right.target}`));

const written = (graph: ProjectGraph) =>
    targeted(graph).flatMap(({ node, target, configuration, task }) => (configuration.outputs === undefined ? [] : getOutputsForTargetAndConfiguration({ project: node.name, target }, {}, node).map((output) => ({ task, cache: configuration.cache === true, folder: output.replace(PATTERN, '') }))));

const shared = (outputs: ReturnType<typeof written>) =>
    Array.filterMap(Array.cartesian(outputs, outputs), ([left, right]) =>
        left.task < right.task && (left.folder === right.folder || left.folder.startsWith(`${right.folder}/`) || right.folder.startsWith(`${left.folder}/`)) ? Result.succeed(`${left.task} and ${right.task} write ${left.folder.length < right.folder.length ? right.folder : left.folder}`) : Result.failVoid,
    );

const restored = (outputs: ReturnType<typeof written>, state: readonly string[]) => outputs.filter(({ cache, folder }) => cache && Array.intersection(folder.split('/'), state).length > 0).map(({ task, folder }) => `${task} restores ${folder}`);

const unready = (graph: ProjectGraph) => Array.filterMap(targeted(graph), ({ configuration, task }) => (configuration.continuous === true && configuration.readyWhen === undefined ? Result.succeed(task) : Result.failVoid));

const refused = (refusal: ProjectConfigurationsError, plugin: string, file: string) => ({
    named: refusal.errors.filter(isAggregateCreateNodesError).some(({ errors }) => errors.some(([at, error]) => at === file || error.message.includes(file))),
    kept: Object.values(refusal.partialProjectConfigurationsResult.sourceMaps).some((sources) => Object.values(sources).some(([, owner]) => owner === plugin)),
});

const unnamed = (refusal: unknown, tools: readonly string[]): readonly unknown[] =>
    isProjectConfigurationsError(refusal) ? refusal.errors.flatMap((error) => (isAggregateCreateNodesError(error) ? error.errors.map(([, cause]) => cause.message) : [error.message])).filter((message) => !tools.some((tool) => message.includes(tool))) : [refusal];

const cases = (subject: Subject, stated: (methods: Vitest.MethodsNonLive<Layer.Success<typeof layer>>) => void = Function.constVoid) =>
    it.layer(layer, { excludeTestServices: true })(subject.plugin.name, (methods) => {
        stated(methods);

        methods('matches the files that mark a project and no source file', () =>
            assert.deepStrictEqual(
                SOURCES.filter((file) => matchesGlob(file, subject.plugin.createNodes[0])),
                [],
            ),
        );

        methods.effect(
            'returns projects Nx merges, with outputs under the workspace, inputs Nx expands, a readiness check on every continuous target, and a task graph for every target',
            Effect.fnUntraced(function* () {
                const { root, loaded } = yield* opened(subject);
                const graph = yield* graphed(root, [loaded]);
                const [nx, targets] = [readNxJson(root), targeted(graph)];
                targets.forEach(({ node, target, configuration }) => {
                    validateOutputs(configuration.outputs ?? []);
                    getTargetInputs(nx, node, target);
                });
                createTaskGraph(graph, {}, Record.keys(graph.nodes), Array.dedupe(targets.map(({ target }) => target)), undefined, {});
                assert.deepStrictEqual(unready(graph), []);
            }),
        );

        methods.effect('writes each output path from one target alone and restores no cached output into a folder its tool keeps incremental state in', () => Effect.map(inferred(subject), (graph) => assert.deepStrictEqual([shared(written(graph)), restored(written(graph), subject.state ?? [])], [[], []])));

        methods.effect(
            'states no machine path in the graph',
            Effect.fnUntraced(function* () {
                const { root, loaded } = yield* opened(subject);
                const graph = yield* graphed(root, [loaded]);
                const text = yield* Schema.encodeUnknownEffect(Json)(Record.map(graph.nodes, (node) => node.data));
                assert.notMatch(text, new RegExp([root, homedir(), tmpdir()].map(RegExp.escape).join('|'), 'u'));
            }),
        );

        methods.effect(
            'returns a project for its files, the same for each file whatever order Nx lists the files in',
            Effect.fnUntraced(function* () {
                vi.stubEnv('NX_CACHE_PROJECT_GRAPH', 'false');
                const { root, loaded } = yield* opened(subject);
                const files = yield* Effect.promise(() => globWithWorkspaceContext(root, [subject.plugin.createNodes[0]]));
                const ordered = yield* listed(root, loaded, files);
                assert.deepStrictEqual([Object.values(ordered).some(({ projects = {} }) => !Record.isEmptyRecord(projects)), yield* listed(root, loaded, files.toReversed())], [true, ordered]);
            }),
        );

        methods.effect(
            'reads an unchanged workspace once: a second graph starts no process and equals the first',
            Effect.fnUntraced(function* () {
                const { root, loaded } = yield* opened(subject);
                const first = yield* graphed(root, [loaded]);
                const [second, started] = yield* spawned(graphed(root, [loaded]));
                assert.deepStrictEqual([started, second], [[], first]);
            }),
        );

        methods.effect(
            'reads again where NX_CACHE_PROJECT_GRAPH is false, starting its tools both times',
            Effect.fnUntraced(function* () {
                vi.stubEnv('NX_CACHE_PROJECT_GRAPH', 'false');
                const { root, loaded } = yield* opened(subject);
                const [, first] = yield* spawned(graphed(root, [loaded]));
                const [, second] = yield* spawned(graphed(root, [loaded]));
                assert.deepStrictEqual([second, first.length > 0 || (subject.tools ?? []).length === 0], [first, true]);
            }),
        );

        methods.effect.skipIf(subject.broken === undefined)(
            'refuses a config its tool cannot read by naming the file, keeps the projects of every config it still reads, and reads the config again once repaired',
            Effect.fnUntraced(function* () {
                const { root, loaded } = yield* opened(subject);
                const [file, text] = yield* Effect.fromNullishOr(subject.broken);
                const whole = yield* graphed(root, [loaded]);
                yield* edited(root, file, text);
                const refusal = yield* Effect.flip(graphed(root, [loaded]));
                assert(isProjectConfigurationsError(refusal), `Nx reports ${String(refusal)} where a plugin names the config it could not read`);
                yield* Effect.flatMap(Effect.fromNullishOr(subject.files[file]), (repaired) => edited(root, file, repaired));
                assert.deepStrictEqual([refused(refusal, subject.plugin.name, file), yield* graphed(root, [loaded])], [{ named: true, kept: true }, whole]);
            }),
        );

        methods.effect(
            'starts no process and returns no project where no file matches',
            Effect.fnUntraced(function* () {
                vi.stubEnv('NX_CACHE_PROJECT_GRAPH', 'false');
                const { root, loaded } = yield* opened({ ...subject, files: { 'package.json': '{"name":"root"}', 'README.md': '# root\n' }, packages: [] });
                assert.deepStrictEqual(yield* spawned(listed(root, loaded, [])), [{}, []]);
            }),
        );

        methods.effect.skipIf((subject.tools ?? []).length === 0)(
            'names the missing tool in every refusal, or returns the projects it reads without the tool',
            Effect.fnUntraced(function* () {
                vi.stubEnv('NX_CACHE_PROJECT_GRAPH', 'false');
                const { root, loaded } = yield* opened(subject, subject.tools);
                const refusals = yield* Effect.match(graphed(root, [loaded]), { onSuccess: () => [], onFailure: Array.of });
                assert.deepStrictEqual(
                    Array.flatMap(refusals, (refusal) => unnamed(refusal, subject.tools ?? [])),
                    [],
                );
            }),
        );
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases, edges, inferred, roots, running, type Subject };
