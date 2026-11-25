import { Buffer } from 'node:buffer';
import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { mkdirSync, mkdtempSync, readdirSync, realpathSync, rmSync, symlinkSync, unlinkSync, writeFileSync } from 'node:fs';
import { createServer } from 'node:http';
import { constants } from 'node:http2';
import { tmpdir } from 'node:os';
import { basename, delimiter, dirname, join, relative } from 'node:path';
import { chdir, cwd, env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { type ExecutorContext, getOutputsForTargetAndConfiguration, type NxPlugin, type ProjectGraph, readJsonFile, readProjectsConfigurationFromProjectGraph } from '@nx/devkit';
import { below } from '@rasm/nx';
import { Array, Cause, type Duration, Effect, Function, Match, Option, Predicate, Record, Schema, UndefinedOr } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { LoadedNxPlugin, resolveNxTokensInOptions, retrieveProjectConfigurations } from 'nx/src/devkit-internals';
import { runCommands } from 'nx/src/executors/run-commands/run-commands.impl';
import { getFilesForOutputsBatch } from 'nx/src/native';
import { createNodes as packageJsonNodes, name as packageJsonName } from 'nx/src/plugins/package-json';
import { ProjectJsonProjectsPlugin } from 'nx/src/plugins/project-json/build-nodes/project-json';
import { buildProjectGraphUsingProjectFileMap } from 'nx/src/project-graph/build-project-graph';
import { retrieveWorkspaceFiles } from 'nx/src/project-graph/utils/retrieve-workspace-files';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { getReadyWhen } from 'nx/src/tasks-runner/readiness/ready-when';
import { resetWorkspaceContext } from 'nx/src/utils/workspace-context';
import { setWorkspaceRoot, workspaceRoot } from 'nx/src/utils/workspace-root';
import { expect, onTestFinished, test, vi } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Asked {
    readonly authorized: boolean;
    readonly held: Buffer | undefined;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const inherited = Record.filter(env, (value, name): value is string => Predicate.isString(value) && !['NX_', 'VITEST', 'npm_'].some((prefix) => name.startsWith(prefix)) && !['NODE_ENV', 'NO_COLOR', 'VIRTUAL_ENV', 'GITHUB_ACTIONS'].includes(name));
const typescript = ['typescript', import.meta.resolve('@typescript/native/package.json')] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Configured = Schema.Struct({
    options: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)),
    configurations: Schema.optionalKey(Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Unknown))),
}).annotate({ description: 'The options and configurations of a target as the project graph holds them' });

const Runnable = Schema.Struct({
    executor: Schema.optionalKey(Schema.Literal('nx:run-commands')),
    command: Schema.optionalKey(Schema.String),
}).annotate({ description: 'The executor and command of an nx:run-commands target as the project graph holds them' });

const Manifest = Schema.Struct({
    name: Schema.String,
    bin: Schema.optionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
}).annotate({ description: 'The name and bin field of an installed package.json, a string bin naming one command after the unscoped package name' });

const Listening = Schema.Struct({ port: Schema.Int }).annotate({ description: 'The address a TCP server returns once listening, its port the one the system chose' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const environment = (root: string, set: Record.ReadonlyRecord<string, string> = {}) => ({
    ...inherited,
    NX_DAEMON: 'false',
    NX_TUI: 'false',
    FORCE_COLOR: '0',
    PATH: [join(root, 'node_modules/.bin'), ...Option.toArray(Record.get(inherited, 'PATH'))].join(delimiter),
    ...set,
});

const refusing = (tool: string) => [
    { case: 'a missing executable', scripts: {}, refusal: new RegExp(`^${RegExp.escape(tool)} .*cannot start: `, 'u') },
    { case: 'a non-zero exit', scripts: { [tool]: 'echo refused >&2\nexit 3' }, refusal: new RegExp(`^${RegExp.escape(tool)} .*exits 3: refused$`, 'u') },
];

const scripted = (folder: string, scripts: Record.ReadonlyRecord<string, string>, path: string) => {
    mkdirSync(folder, { recursive: true });
    Record.toEntries(scripts).forEach(([name, body]) => writeFileSync(join(folder, name), `#!/bin/sh\n${body}\n`, { mode: 0o755 }));
    vi.stubEnv('PATH', path);
    onTestFinished(() => {
        vi.unstubAllEnvs();
    });
};

const pathed = (folder: string, scripts: Record.ReadonlyRecord<string, string>) => scripted(folder, scripts, folder);

const shimmed = (folder: string, scripts: Record.ReadonlyRecord<string, string>) => scripted(folder, scripts, Array.dedupe([folder, ...(env['PATH']?.split(delimiter) ?? [])]).join(delimiter));

const answered = Match.type<Asked>().pipe(
    Match.when({ authorized: false }, () => constants.HTTP_STATUS_UNAUTHORIZED),
    Match.when({ held: Match.undefined }, () => constants.HTTP_STATUS_NOT_FOUND),
    Match.orElse(() => constants.HTTP_STATUS_OK),
);

const remote = Effect.fnUntraced(function* (token: string, answer: (asked: Asked) => Option.Option<number>) {
    const [held, requests] = [new Map<string, Buffer>(), Array.empty<string>()];
    const server = createServer((request, response) => {
        const chunks: Buffer[] = [];
        requests.push(`${request.method} ${request.url}`);
        request.on('data', (chunk: Buffer) => {
            chunks.push(chunk);
        });
        request.on('end', () => {
            const body = request.method === 'PUT' ? held.set(String(request.url), Buffer.concat(chunks)).get(String(request.url)) : held.get(String(request.url));
            Option.match(answer({ authorized: request.headers.authorization === `Bearer ${token}`, held: body }), {
                onNone: Function.constVoid,
                onSome: (status) => response.writeHead(status).end(request.method === 'GET' ? body : undefined),
            });
        });
    });
    yield* Effect.promise(() => once(server.listen(0, '127.0.0.1'), 'listening')).pipe(Effect.as(server), Effect.acquireDisposable);
    yield* Effect.addFinalizer(() => Effect.sync(() => server.closeAllConnections()));
    const { port } = yield* Schema.decodeUnknownEffect(Listening)(server.address());
    return { address: `http://127.0.0.1:${port}`, requests };
}, Effect.orDie);

const graphed = Effect.fnUntraced(function* <T>(root: string, { createNodes, createDependencies }: Pick<NxPlugin<T>, 'createNodes' | 'createDependencies'>, options: T | undefined) {
    resetWorkspaceContext();
    const bound: NxPlugin = {
        name: 'plugin-under-test',
        ...(createNodes === undefined ? {} : { createNodes: [createNodes[0], (files, _, context) => createNodes[1](files, options, context)] }),
        ...(createDependencies === undefined ? {} : { createDependencies: (_, context) => createDependencies(options, context) }),
    };
    const loaded = new LoadedNxPlugin(bound, { plugin: bound.name });
    const defaults = [new LoadedNxPlugin({ name: packageJsonName, createNodes: packageJsonNodes }, { plugin: packageJsonName }), new LoadedNxPlugin(ProjectJsonProjectsPlugin, { plugin: ProjectJsonProjectsPlugin.name })];
    const read = yield* Effect.promise(() => retrieveProjectConfigurations({ specifiedPlugins: [loaded], defaultPlugins: defaults }, root, readNxJson(root)));
    const { fileMap, rustReferences } = yield* Effect.promise(() => retrieveWorkspaceFiles(root, read.projectRootMap));
    return (yield* Effect.promise(() => buildProjectGraphUsingProjectFileMap(read.projects, read.externalNodes, fileMap, rustReferences, null, [loaded], read.sourceMaps))).projectGraph;
});

const configured = Effect.fnUntraced(function* (graph: ProjectGraph, project: string, target: string, configuration: string | undefined) {
    const found = Option.flatMapNullishOr(Option.fromNullishOr(graph.nodes[project]), (node) => UndefinedOr.map(node.data.targets?.[target], (stated) => ({ node, stated })));
    const { node, stated } = yield* Effect.fromOption(found, () => new Cause.NoSuchElementError(`${project} has no target ${target} in the graph`));
    const { options = {}, configurations = {} } = yield* Schema.decodeEffect(Configured)(stated);
    return { node, stated, options: { ...options, ...(configuration === undefined ? {} : configurations[configuration]) } };
});

const ran = Effect.fnUntraced(function* (context: ExecutorContext, graph: ProjectGraph, project: string, target: string) {
    const { node, stated, options } = yield* configured(graph, project, target, context.configurationName);
    const { command } = yield* Schema.decodeUnknownEffect(Runnable)(stated);
    const task = yield* Effect.promise(() => runCommands(resolveNxTokensInOptions({ __unparsed__: [], ...(command === undefined ? {} : { command }), ...options }, node.data, `${project}:${target}`), context));
    const { code, terminalOutput } = yield* Effect.promise(() => task.getResults());
    return { success: code === 0, terminalOutput };
});

const taskGraph = (graph: ProjectGraph, projects: readonly string[], targets: readonly string[], configuration?: string, overrides: Record.ReadonlyRecord<string, unknown> = {}) => createTaskGraph(graph, {}, [...projects], [...targets], configuration, { ...overrides });

const outputs = (graph: ProjectGraph, project: string, target: string, configuration?: string, overrides: Record.ReadonlyRecord<string, unknown> = {}) =>
    configured(graph, project, target, configuration).pipe(
        Effect.map(({ node }) => getOutputsForTargetAndConfiguration({ project, target, ...(configuration === undefined ? {} : { configuration }) }, { ...overrides }, node)),
        Effect.runSync,
    );

const targetOptions = <A>(schema: Schema.Decoder<A>, graph: ProjectGraph, project: string, target: string, configuration?: string) =>
    configured(graph, project, target, configuration).pipe(
        Effect.flatMap(({ node, options }) => Schema.decodeEffect(schema)(resolveNxTokensInOptions(options, node.data, `${project}:${target}`))),
        Effect.runPromise,
    );

const listen = (port = 0) => {
    const server = createServer().listen(port, '127.0.0.1');
    const close = () => (server.listening ? server[Symbol.asyncDispose]() : Promise.resolve());
    onTestFinished(close);
    return Effect.promise(() => once(server, 'listening')).pipe(
        Effect.flatMap(() => Schema.decodeUnknownEffect(Listening)(server.address())),
        Effect.map(({ port: bound }) => ({ port: bound, close })),
        Effect.runPromise,
    );
};

const spanned = (from: number, to: number): unknown => {
    const instant = Schema.Finite.check(Schema.isBetween({ minimum: from, maximum: to }));
    return expect.schemaMatching(Schema.toStandardSchemaV1(Schema.Struct({ startTime: instant, endTime: instant }).check(Schema.makeFilter(({ startTime, endTime }) => startTime <= endTime || `ends at ${endTime} before it starts at ${startTime}`))));
};

const unaggregated = (graph: ProjectGraph) =>
    Object.values(graph.nodes)
        .flatMap(({ name: project, data: { targets = {} } }) => Record.collect(targets, (target, { inputs = [] }) => ({ project, target, inputs, targets, verb: target.slice(0, target.indexOf('--')) })))
        .filter(({ target }) => target.includes('--'))
        .map(({ project, target, inputs, targets, verb }) => ({
            project,
            target,
            runs: (targets[verb]?.dependsOn ?? []).some((on) => Predicate.isObject(on) && on.target === `${verb}--*` && on.params === 'forward'),
            unhashed: inputs.filter((input) => Predicate.isString(input) && !(targets[verb]?.inputs ?? []).includes(input)),
        }))
        .filter(({ runs, unhashed }) => !runs || unhashed.length > 0);

const unportable = (graph: ProjectGraph) => {
    const stated = Object.values(graph.nodes).flatMap(({ name, data: { root, targets = {} } }) => [
        { project: name, field: 'root', path: root },
        ...Record.collect(targets, (target, { inputs = [], outputs: declared = [], options }) => [
            ...[...inputs.filter(Predicate.isString), ...inputs.flatMap((input) => (Predicate.hasProperty(input, 'fileset') && Predicate.isString(input.fileset) ? [input.fileset] : []))].map((path) => ({ project: name, field: `${target}.inputs`, path })),
            ...declared.map((path) => ({ project: name, field: `${target}.outputs`, path })),
            ...(Predicate.hasProperty(options, 'cwd') && Predicate.isString(options.cwd) ? [{ project: name, field: `${target}.options.cwd`, path: options.cwd }] : []),
        ]).flat(),
    ]);
    return stated.filter(({ path }) => path.includes('\\') || path.startsWith('/') || /^[A-Za-z]:/u.test(path));
};

// --- [SERVICES] ------------------------------------------------------------------------

class Workspace {
    readonly root = realpathSync(mkdtempSync(join(tmpdir(), 'nx-workspace-')));

    // --- [FILES]

    write(files: Readonly<Record<string, string>>) {
        Record.toEntries(files).forEach(([path, content]) => {
            mkdirSync(dirname(join(this.root, path)), { recursive: true });
            writeFileSync(join(this.root, path), content);
        });
        return this;
    }
    remove(...paths: readonly string[]) {
        paths.forEach((path) => unlinkSync(join(this.root, path)));
        return this;
    }
    install(...manifests: readonly (string | readonly [name: string, manifest: string])[]) {
        const [modules, bins] = [join(this.root, 'node_modules'), join(this.root, 'node_modules/.bin')];
        mkdirSync(bins, { recursive: true });
        manifests.forEach((entry) => {
            const [named, manifest] = Predicate.isString(entry) ? [undefined, fileURLToPath(entry)] : [entry[0], fileURLToPath(entry[1])];
            const { name: published, bin = {} } = Effect.runSync(Schema.decodeUnknownEffect(Manifest)(readJsonFile(manifest)));
            const at = join(modules, named ?? published);
            mkdirSync(dirname(at), { recursive: true });
            symlinkSync(dirname(manifest), at, 'dir');
            Record.toEntries(Predicate.isString(bin) ? { [basename(published)]: bin } : bin).forEach(([command, path]) => symlinkSync(relative(bins, join(at, path)), join(bins, command)));
        });
        return this;
    }
    files(...ignored: readonly string[]) {
        return new Set(
            readdirSync(this.root, { recursive: true, withFileTypes: true })
                .filter((entry) => entry.isFile())
                .map((entry) => relative(this.root, join(entry.parentPath, entry.name)))
                .filter((file) => !ignored.some((folder) => below(folder)(file))),
        );
    }

    // --- [GRAPH]

    graph<T>(plugin: Pick<NxPlugin<T>, 'createNodes' | 'createDependencies'>, options?: T): Promise<ProjectGraph> {
        return Effect.runPromise(graphed(this.root, plugin, options));
    }
    cached(graph: ProjectGraph, project: string, target: string, configuration?: string) {
        return new Set(getFilesForOutputsBatch(this.root, [outputs(graph, project, target, configuration)]).flat());
    }
    ready(graph: ProjectGraph, project: string, target: string, overrides: Record.ReadonlyRecord<string, unknown> = {}) {
        return UndefinedOr.map(taskGraph(graph, [project], [target], undefined, overrides).tasks[`${project}:${target}`], (task) => getReadyWhen(task, graph, readProjectsConfigurationFromProjectGraph(graph).projects, this.root, env)) ?? null;
    }
    poll(graph: ProjectGraph, project: string, target: string, overrides: Record.ReadonlyRecord<string, unknown> = {}) {
        const probe = this.ready(graph, project, target, overrides);
        const check = spawn(probe?.kind === 'command' ? probe.command : 'false', { shell: true, cwd: this.root, env: probe?.kind === 'command' ? probe.env : env, stdio: 'ignore' });
        const exit = once(check, 'exit').then(([code]: readonly unknown[]) => code);
        onTestFinished(() => {
            check.kill();
        });
        return (wait: Duration.Input) => Effect.promise(() => exit).pipe(Effect.timeoutOption(wait), Effect.runPromise);
    }

    // --- [EXECUTION]

    context(graph: ProjectGraph, projectName: string, targetName: string, configurationName?: string): ExecutorContext {
        return { root: this.root, cwd: this.root, isVerbose: false, projectName, targetName, ...(configurationName === undefined ? {} : { configurationName }), projectGraph: graph, projectsConfigurations: readProjectsConfigurationFromProjectGraph(graph), nxJsonConfiguration: readNxJson(this.root) };
    }
    run(graph: ProjectGraph, projectName: string, targetName: string, configurationName?: string) {
        return ran(this.context(graph, projectName, targetName, configurationName), graph, projectName, targetName).pipe(Effect.runPromise);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const opened = (finished: (cleanup: () => void) => void) => {
    const [previous, workspace] = [{ cwd: cwd(), root: workspaceRoot }, new Workspace()];
    chdir(workspace.root);
    setWorkspaceRoot(workspace.root);
    env['NX_DAEMON'] = 'false';
    finished(() => {
        chdir(previous.cwd);
        setWorkspaceRoot(previous.root);
        rmSync(workspace.root, { recursive: true, force: true, maxRetries: 5 });
        resetWorkspaceContext();
    });
    return workspace;
};

const it = test.extend('workspace', ({ onTestFinished: finished }) => opened(finished)).extend('fileWorkspace', { scope: 'file' }, ({}, { onCleanup }) => opened(onCleanup));

// --- [EXPORTS] -------------------------------------------------------------------------

export { answered, environment, it, listen, outputs, pathed, refusing, remote, shimmed, spanned, targetOptions, taskGraph, typescript, unaggregated, unportable, Workspace };
