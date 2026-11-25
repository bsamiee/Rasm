import { join } from 'node:path';
import { type ProjectGraph, parseJson } from '@nx/devkit';
import { createProjectRootMappings, findProjectForPath } from '@nx/devkit/internal';
import { tasks } from '@rasm/nx/graph';
import { Array, ByteSize, ConfigProvider, DateTime, Effect, FileSystem, Option, Path, PlatformError, type Record } from 'effect';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { workspaceDataDirectoryForWorkspace } from 'nx/src/utils/cache-directory';
import { describe, expect, it, vi } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

type Files = ReadonlyMap<string, { readonly bytes: Uint8Array; readonly modified: number }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = '/w';
const DATA = { NX_WORKSPACE_DATA_DIRECTORY: '.cache/nx/workspace-data' };
const AT = `${ROOT}/.cache/nx/workspace-data/project-graph.json`;

const mini = {
    version: '6.0',
    futureTopField: { kept: true },
    nodes: {
        docs: { name: 'docs', type: 'lib', data: { root: 'apps/site-docs' } },
        site: {
            name: 'site',
            type: 'app',
            futureNodeField: 1,
            data: {
                root: 'apps/site',
                futureDataField: { a: 1 },
                targets: {
                    lint: { executor: 'nx:noop', dependsOn: ['^lint', 'tool:fmt', 'lint-*', { target: 'fmt', projects: ['tool'] }, { target: 'fmt', dependencies: true }, { target: 'dev', projects: 'site' }], futureTargetField: [1, 2] },
                    every: { executor: 'nx:noop', dependsOn: ['*'] },
                    'lint-a': { executor: 'nx:run-commands', options: { command: ['eslint', '--max-warnings=0', '.'], cwd: 'apps/site' }, cache: true },
                    'lint-b': { executor: 'nx:run-commands', options: { commands: ['stylelint "**/*.css"', { command: 'prettier --check .', forwardAllArgs: false }] } },
                    fmt: { executor: 'nx:run-commands', options: { command: 'oxfmt', commands: ['never run beside command'] }, configurations: { check: { command: 'oxfmt --check' } }, metadata: { help: { command: 'oxfmt --help', example: {} } } },
                    types: { executor: '@scope/plugin:types', options: { command: ['ignored', 'by', 'its', 'executor'] } },
                    build: {
                        executor: '@scope/plugin:build',
                        outputs: [
                            '{projectRoot}/dist',
                            '{workspaceRoot}/apps/site/dist',
                            '{workspaceRoot}/.artifacts/{projectName}',
                            '{options.outputPath}',
                            '{options.missing}/never',
                            '{options.blank}/never',
                            '!{projectRoot}/dist/keep',
                            '{projectRoot}/out.{js,map}',
                            '{projectRoot}/a//b/./c/',
                            '{projectRoot}/{env.HOME}',
                            '{project.name}/legacy',
                            '{options.list.1}',
                            '{projectRoot}/{d.ts,{projectName}}',
                        ],
                        options: { blank: '', list: ['first', 'second/'] },
                        configurations: { release: { outputPath: 'release/site' } },
                    },
                    swift: { executor: '@scope/plugin:swift', defaultConfiguration: 'release', outputs: ['{options.outputs.0}'], options: { outputs: ['base'] }, configurations: { release: { outputs: ['rel'] }, debug: { outputs: ['dbg'] } } },
                    dev: { executor: 'nx:run-commands', options: { command: 'vite' }, continuous: true },
                },
            },
        },
        tool: {
            name: 'tool',
            type: 'lib',
            data: {
                root: '.',
                targets: {
                    fmt: { executor: 'nx:run-commands', options: { command: 'oxfmt' } },
                    gen: { executor: '@scope/plugin:gen', outputs: ['{projectRoot}/generated', '{projectRoot}', '{workspaceRoot}/top/{projectRoot}', '{projectRoot}/x{projectRoot}'] },
                    misplaced: { executor: '@scope/plugin:gen', outputs: ['{projectRoot}/kept', '{projectRoot}/{workspaceRoot}/b'] },
                    refused: { executor: '@scope/plugin:gen', outputs: ['{projectRoot}/kept', 'dist'] },
                },
            },
        },
    },
    externalNodes: {},
    dependencies: { docs: [], site: [{ source: 'site', target: 'tool', type: 'static' }], tool: [] },
    computedAt: 1,
    errors: [],
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const encoded = (graph: unknown) => new TextEncoder().encode(JSON.stringify(graph));

const served = (files: Files) => {
    const found = (method: string, path: string) => Effect.fromNullishOr(files.get(path)).pipe(Effect.mapError(() => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method, pathOrDescriptor: path })));
    return FileSystem.make({
        ...FileSystem.makeNoop({}),
        stat: (path) =>
            Effect.map(found('stat', path), ({ bytes, modified }) => ({
                type: 'File',
                mtime: Option.some(DateTime.toDateUtc(DateTime.makeUnsafe(modified))),
                atime: Option.none(),
                birthtime: Option.none(),
                dev: 0n,
                ino: Option.none(),
                mode: 0o644,
                nlink: Option.none(),
                uid: Option.none(),
                gid: Option.none(),
                rdev: Option.none(),
                size: ByteSize.bytes(bytes.length),
                blksize: Option.none(),
                blocks: Option.none(),
            })),
        readFile: (path) => Effect.map(found('readFile', path), ({ bytes }) => bytes),
    });
};

const reader =
    (files: Files, environment: Record.ReadonlyRecord<string, string> = DATA) =>
    (root: string) =>
        tasks(root).pipe(Effect.provideService(FileSystem.FileSystem, served(files)), Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromUnknown(environment)), Effect.provide(Path.layer));

const graphOf = (graph: unknown, environment?: Record.ReadonlyRecord<string, string>, at = AT) => Effect.runPromise(reader(new Map([[at, { bytes: encoded(graph), modified: 1 }]]), environment)(ROOT));

const failure = (files: Files) => Effect.runPromise(Effect.flip(reader(files)(ROOT)));

const addresses = (graph: ProjectGraph) =>
    Object.values(graph.nodes).flatMap((node) => Object.entries(node.data.targets ?? {}).flatMap(([target, stated]) => [undefined, ...Object.keys(stated.configurations ?? {})].map((configuration) => ({ id: [node.name, target, ...Array.fromNullishOr(configuration)].join(':'), node, target, stated, configuration }))));

const planned = (graph: ProjectGraph, { node, target, stated, configuration }: ReturnType<typeof addresses>[number]) => {
    const built = Option.liftThrowable(() => createTaskGraph(graph, {}, [node.name], [target], configuration, {}))();
    const own = (id: string) => Option.flatMap(built, ({ tasks: ran }) => Option.filter(Option.fromUndefinedOr(ran[id]), (task) => task.target.project === node.name));
    const task = Option.flatMap(built, ({ tasks: ran }) =>
        Option.flatMap(
            Array.findFirst(Object.keys(ran), (id) => Option.exists(own(id), (found) => found.target.target === target)),
            own,
        ),
    );
    const edges = Option.match(Option.all([built, task]), { onNone: () => [], onSome: ([{ dependencies, continuousDependencies }, { id }]) => [...(dependencies[id] ?? []), ...(continuousDependencies[id] ?? [])] });
    return {
        cached: Option.getOrElse(
            Option.map(task, ({ cache }) => cache),
            () => stated.cache === true,
        ),
        continuous: Option.getOrElse(
            Option.map(task, ({ continuous }) => continuous === true),
            () => stated.continuous === true,
        ),
        outputs: Option.getOrElse(
            Option.map(task, ({ outputs }) => outputs.toSorted()),
            () => [],
        ),
        members: stated.executor === 'nx:noop' ? Array.dedupe(edges.flatMap(({ id }) => Option.toArray(own(id)).map((found) => found.target.target))).toSorted() : undefined,
    };
};

const shown = (row: Awaited<ReturnType<typeof graphOf>>['tasks'] extends ReadonlyMap<string, infer Row> ? Row | undefined : never) => ({
    cached: row?.cached,
    continuous: row?.continuous,
    outputs: row?.outputs.toSorted(),
    members: row?.runs._tag === 'Aggregate' ? row.runs.members.toSorted() : undefined,
});

const settled = (result: PromiseSettledResult<Awaited<ReturnType<typeof graphOf>>>) => (result.status === 'fulfilled' ? result.value.tasks.has('site:lint') : String(result.reason));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('task rows', () => {
    it('read cache, continuous, outputs, and aggregate members of every address as createTaskGraph resolves them', async () => {
        const graph = parseJson<ProjectGraph>(JSON.stringify(mini));
        const { tasks: rows } = await graphOf(graph);
        const expected = addresses(graph).map((address) => [address.id, planned(graph, address)] as const);
        expect([...rows.keys()].toSorted()).toStrictEqual(expected.map(([id]) => id).toSorted());
        expect(expected.map(([id]) => [id, shown(rows.get(id))])).toStrictEqual(expected);
    });

    it('state the shell lines and folder run-commands runs, the options a configuration merges, and the help command', async () => {
        const { tasks: rows } = await graphOf(mini);
        expect(Object.fromEntries(['site:lint-a', 'site:lint-b', 'site:fmt', 'site:fmt:check', 'site:types', 'site:build:release'].map((id) => [id, rows.get(id)]))).toMatchObject({
            'site:lint-a': { runs: { _tag: 'Commands', cwd: 'apps/site', lines: ['eslint --max-warnings=0 .'] }, help: Option.none() },
            'site:lint-b': { runs: { _tag: 'Commands', cwd: '.', lines: ['stylelint "**/*.css"', 'prettier --check .'] } },
            'site:fmt': { runs: { _tag: 'Commands', cwd: '.', lines: ['oxfmt'] }, help: Option.some('oxfmt --help') },
            'site:fmt:check': { runs: { _tag: 'Commands', cwd: '.', lines: ['oxfmt --check'] }, help: Option.some('oxfmt --help') },
            'site:types': { runs: { _tag: 'Executor' } },
            'site:build:release': { options: { blank: '', list: ['first', 'second/'], outputPath: 'release/site' } },
        });
    });

    it('name the project owning each path as findProjectForPath does, and none outside the workspace', async () => {
        const graph = parseJson<ProjectGraph>(JSON.stringify(mini));
        const { owner } = await graphOf(graph);
        const roots = createProjectRootMappings(graph.nodes);
        const paths = ['apps/site', 'apps/site/src/main.ts', 'apps/site-docs/guide.md', 'apps/sites/x', 'apps', '.', 'README.md'];
        expect(paths.map((at) => [at, Option.getOrUndefined(owner(`${ROOT}/${at}`)), Option.getOrUndefined(owner(at))])).toStrictEqual(paths.map((at) => [at, findProjectForPath(at, roots), findProjectForPath(at, roots)]));
        expect([owner('/elsewhere/apps/site/x'), owner('../apps/site')]).toStrictEqual([Option.none(), Option.none()]);
    });
});

describe('the cached graph', () => {
    it('is read where Nx places its workspace data under each setting', async () => {
        const settings = [{ NX_WORKSPACE_DATA_DIRECTORY: '/elsewhere/data' }, { NX_WORKSPACE_DATA_DIRECTORY: 'cache/data' }, { NX_PROJECT_GRAPH_CACHE_DIRECTORY: 'legacy' }, { NX_WORKSPACE_DATA_DIRECTORY: 'first', NX_PROJECT_GRAPH_CACHE_DIRECTORY: 'second' }, {}];
        const placed = settings.map((environment: Partial<Record<'NX_WORKSPACE_DATA_DIRECTORY' | 'NX_PROJECT_GRAPH_CACHE_DIRECTORY', string>>) => {
            vi.stubEnv('NX_WORKSPACE_DATA_DIRECTORY', environment.NX_WORKSPACE_DATA_DIRECTORY);
            vi.stubEnv('NX_PROJECT_GRAPH_CACHE_DIRECTORY', environment.NX_PROJECT_GRAPH_CACHE_DIRECTORY);
            return join(workspaceDataDirectoryForWorkspace(ROOT), 'project-graph.json');
        });
        vi.unstubAllEnvs();
        const read = await Promise.allSettled(settings.map((environment, at) => graphOf(mini, environment, placed[at])));
        expect(Array.zip(settings, read.map(settled))).toStrictEqual(settings.map((environment) => [environment, true]));
    });

    it('is read again exactly when its modification time or size changes, and a failed read is never kept', async () => {
        const whole = encoded(mini);
        const files = new Map([[AT, { bytes: whole, modified: 1 }]]);
        const read = reader(files);
        const first = await Effect.runPromise(read(ROOT));
        const unchanged = await Effect.runPromise(read(ROOT));
        files.set(AT, { bytes: whole, modified: 2 });
        const touched = await Effect.runPromise(read(ROOT));
        files.set(AT, { bytes: encoded({ ...mini, nodes: { tool: mini.nodes.tool } }), modified: 2 });
        const resized = await Effect.runPromise(read(ROOT));
        files.set(AT, { bytes: Uint8Array.from([...whole.subarray(0, -1), 0x20]), modified: 3 });
        const torn = await Effect.runPromise(Effect.flip(read(ROOT)));
        files.set(AT, { bytes: whole, modified: 3 });
        const mended = await Effect.runPromise(read(ROOT));
        expect([unchanged === first, touched === first, touched.tasks.has('site:lint'), resized.tasks.has('site:lint'), torn._tag, mended.tasks.has('site:lint')]).toStrictEqual([true, false, true, false, 'SchemaError', true]);
    });

    it('fails where Nx cached none or recorded errors building it', async () => {
        expect([await failure(new Map()), await failure(new Map([[AT, { bytes: encoded({ ...mini, errors: [{ message: 'plugin failed' }] }), modified: 1 }]]))]).toMatchObject([{ _tag: 'PlatformError', reason: { _tag: 'NotFound' } }, { _tag: 'SchemaError' }]);
    });
});
