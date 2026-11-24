import { once } from 'node:events';
import { join, posix, relative } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { emitPluginWorkerLog, getDaemonClientEnvGeneration, getLockFileName, globWithWorkspaceContext, hashObject, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { aggregated, anchored, EXEC, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, PLATFORM, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, flow, Function, MutableRef, Option, Record, Result, Schema, String } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const RUN = /^(?:(?:npx|yarn|bunx?|bun x|pnpm(?: exec)?|pnpx) )?nx (?:run (?<project>[^\s:]+):(?<target>[^\s:]+)|(?<target>\S+) (?<project>[^\s:]+))$/u;
const NX = /(?:^|\s)nx(?:@\S+)?\s/u;
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({
    description:
        'outputPath: workspace folder a suite writes test-results and the files of the html, blob, and perfetto reporters to, {projectName} naming the project, each config project with its own outputDir under it and test files its testMatch finds running as pack--playwright into that outputDir, absent leaves Playwright its own folders and infers no pack--playwright',
});
const Loaded = Schema.Struct({
    edges: Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])),
    environment: Schema.Array(Schema.String),
    scripts: Schema.Array(Schema.String),
    routes: Schema.Array(Schema.String),
    reported: Schema.Array(Schema.String),
    commands: Schema.Array(Schema.String),
    suites: Schema.Array(Schema.Struct({ name: Schema.String, outputDir: Schema.String, testDir: Schema.String, found: Schema.Boolean })),
}).annotate({
    description:
        'What the worker read from a Playwright config through Playwright: the resolved URL, specifier, and importing module URL of every import its load resolved, the environment variables its load and test file listing read, the global setup and teardown scripts, the built-in reporters the run routes under outputPath, the files and folders every other reporter writes, the command of each webServer row whose server Playwright reuses and does not bind, and the name, output folder, test folder, and test file presence of each project',
});
const Task = Schema.Struct({ project: Schema.String, target: Schema.String }).annotate({ description: 'The project and target of an nx run command' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = (flags: readonly string[]) => [['playwright', 'test', '--forbid-only'].map(quoteShellArg).join(' '), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (given) => ['{args}', given.map(quoteShellArg).join(' ')] })].join(' ');

const directed = (to: string, reporters: readonly string[]) => {
    const [results, reports] = [posix.join(to, 'test-results'), Record.fromIterableWith(reporters, (name) => [`PLAYWRIGHT_${name.toUpperCase()}_OUTPUT_DIR`, posix.join(to, name)])];
    return { flags: [`--output=${results}`], env: reports, folders: [results, ...Object.values(reports)] };
};

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, routed: boolean, lock: string) {
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load as a Playwright config`);
    const listened = Effect.acquireUseRelease(
        Effect.sync(() => new Worker(WORKER, { workerData: [join(workspaceRoot, file), routed] })),
        (worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), flow(failed, Effect.fail))),
        (worker) => Effect.promise(() => worker.terminate()),
    );
    const { edges, environment, scripts, routes, reported, commands, suites } = yield* Effect.flatMap(listened, ([message]) => Schema.decodeUnknownEffect(Loaded)(message));
    const workspaced = (url: string) => Option.filter(Option.map(Option.liftPredicate(url, String.startsWith('file:')), flow(fileURLToPath, local)), within);
    const loaded = Array.dedupe([file, ...Array.getSomes(edges.map(([url]) => workspaced(url)))]);
    const { packages } = yield* packaged(Array.getSomes(edges.map(([, specifier, parent]) => Option.as(workspaced(parent), [specifier, parent] as const))));
    const tasks = commands.map((command) => Schema.decodeUnknownOption(Task)(RUN.exec(command)?.groups));
    const unread = Array.getSomes(Array.zipWith(commands, tasks, (command, task) => Option.liftPredicate(command, (text) => Option.isNone(task) && NX.test(text))));
    yield* Effect.forEach(unread, (command) => Effect.sync(() => emitPluginWorkerLog('warn', `${PLUGIN} reads no Nx task from webServer command "${command}" in ${file}, so test--playwright waits on no server task`)), { discard: true });
    const scanned = Array.dedupe(suites.map(({ testDir }) => posix.join(local(testDir), '**/*'))).filter(within);
    const sources = [lock, ...loaded];
    return {
        packages,
        environment,
        loaded: [...loaded, ...scripts.map(local).filter(within)],
        scanned,
        servers: Array.dedupeWith(Array.getSomes(tasks), (left, right) => left.project === right.project && left.target === right.target),
        reported: Array.dedupe(reported.map(local)),
        suites: suites.map(({ name, outputDir, found }) => ({ name, outputDir: local(outputDir), found })),
        routes,
        sources,
        stamp: yield* stamped(workspaceRoot)(sources),
    };
});

const templated = (folder: string) => new RegExp(`^${folder.split('{projectName}').map(RegExp.escape).join('.+')}(?:/|$)`, 'u');

const targets = (home: string, { loaded: configs, scanned, packages, servers, reported, routes, suites }: Resolved, chain: readonly string[], upstream: readonly string[], outputPath: string | undefined, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const routed = outputPath === undefined ? { flags: [], env: {}, folders: [] } : directed(posix.relative(home, outputPath), routes);
    const held = routed.folders.map((at) => posix.join(home, at));
    const under = Option.map(Option.fromUndefinedOr(outputPath), templated);
    const holding = held.map(templated);
    const unheld = (at: string) => !holding.some((folder) => folder.test(at));
    const [packed, tested] = Array.partition(suites, (suite) => Result.liftPredicate(suite, ({ outputDir, found }) => found && unheld(outputDir) && Option.exists(under, (folder) => folder.test(outputDir)), Function.identity));
    const selected = (chosen: readonly (typeof suites)[number][]) => chosen.map(({ name }) => `--project=${name}`);
    const outside = Record.filter(
        { ...Record.fromIterableWith(chain, (at) => [at, { json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions', 'extends'] }]), ...Record.fromIterableWith([...configs, ...scanned], (at) => [at, posix.join('{workspaceRoot}', at)]) },
        (_input, at) => home !== '.' && !at.startsWith(`${home}/`) && at !== rooted,
    );
    const needs = {
        dependsOn: [
            'codegen--*',
            '^codegen--*',
            '^build--vite',
            ...Record.collect(
                Array.groupBy(servers, ({ target }) => target),
                (target, rows) => ({ projects: rows.map(({ project }) => project), target, waitFor: 'ready' as const }),
            ),
        ],
        cache: true,
        inputs: ['default', ...upstream, ...Object.values(outside), { dependentTasksOutputFiles: '**/*', transitive: true }, { externalDependencies: Array.dedupe(['@playwright/test', ...packages]) }],
    };
    const test: Record<string, TargetConfiguration> = Array.match(tested, {
        onEmpty: () => ({}),
        onNonEmpty: (chosen) => ({
            playwright: {
                command: invocation([...Array.match(packed, { onEmpty: () => [], onNonEmpty: () => selected(chosen) }), ...routed.flags]),
                options: { cwd: home, ...(Record.isEmptyRecord(routed.env) ? {} : { env: routed.env }) },
                ...needs,
                outputs: Array.dedupe([...held, ...[...(outputPath === undefined ? chosen.map(({ outputDir }) => outputDir) : []), ...reported].filter(unheld)]).map(tokened(home)),
                metadata: {
                    description: 'Runs every Playwright project of the config whose own outputDir sits outside outputPath with playwright test after the Vite build of each project the suite depends on and once the Nx task each reused webServer row runs is ready',
                    help: { command: `${exec} playwright test --help`, example: { args: ['--project', '<name>'] } },
                },
            },
        }),
    });
    const pack: Record<string, TargetConfiguration> = Array.match(packed, {
        onEmpty: () => ({}),
        onNonEmpty: (chosen) => ({
            playwright: {
                command: invocation([...selected(chosen), '--no-deps', '--reporter=list']),
                options: { cwd: home },
                ...needs,
                inputs: [...needs.inputs, PLATFORM],
                dependsOn: Record.isEmptyRecord(test) ? needs.dependsOn : [...needs.dependsOn, 'test--playwright'],
                outputs: chosen.map(({ outputDir }) => outputDir).map(tokened(home)),
                metadata: {
                    description: `Writes the files of the Playwright projects ${chosen.map(({ name }) => name).join(', ')} into each one's own outputDir under outputPath, which playwright test empties first, once test--playwright passes where the config holds other projects`,
                    help: { command: `${exec} playwright test --help`, example: { args: ['--project', '<name>'] } },
                },
            },
        }),
    });
    const removed = Record.collect({ test, pack }, (verb, tools) => Record.keys(tools).map((tool) => `${verb}--${tool}`)).flat();
    return {
        test,
        pack,
        clean: {
            playwright: {
                executor: `${distribution.name}:clean`,
                options: { targets: removed, commands: [['playwright', 'clear-cache']], cwd: home },
                inputs: ['default', '^default', { externalDependencies: ['@playwright/test'] }],
                metadata: { description: `Removes the outputs ${removed.join(', ')} declare and the Playwright transform cache with playwright clear-cache`, help: { command: `${exec} playwright clear-cache --help`, example: { args: ['--config', '<file>'] } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (
    { outputPath, ...pass }: { readonly cache: PluginCache<Resolved & { readonly digest: string }>; readonly stored: MutableRef.MutableRef<boolean>; readonly digest: string; readonly lock: string; readonly exec: string; readonly outputPath: string | undefined },
    context: CreateNodesContext,
    file: string,
) {
    const [home, key] = [posix.dirname(file), hashArray([pass.digest, file])];
    const listed = (entry: Resolved) => Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [...entry.scanned])).pipe(Effect.map((names) => ({ ...entry, digest: hashArray([hashObject(Record.fromIterableWith(entry.environment, (name) => [name, env[name] ?? null])), ...names]) })));
    const held = yield* Effect.findFirst(Option.toArray(yield* recalled(pass.cache, key, stamped(context.workspaceRoot))), (cached) => Effect.map(listed(cached), ({ digest }) => digest === cached.digest));
    const resolved = yield* Option.match(held, {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(Effect.flatMap(read(context.workspaceRoot, file, outputPath !== undefined, pass.lock), listed), (entry) =>
                Effect.sync(() => {
                    pass.cache.set(key, entry);
                    MutableRef.set(pass.stored, true);
                }),
            ),
    });
    const stated = yield* manifested(context.workspaceRoot, home);
    const project: ProjectConfiguration = {
        root: home,
        tags: ['host:browser'],
        metadata: { technologies: ['playwright'] },
        targets: aggregated(targets(home, resolved, extended(context.workspaceRoot, home), namedInput(home, context) === 'production' ? ['^production', '^{projectRoot}/tsconfig*.json'] : ['^default'], outputPath, pass.exec), stated, 'playwright'),
    };
    return { projects: { [home]: project } };
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const project = anchored(context.workspaceRoot);
    const configs = files.filter((file) => project(posix.dirname(file)));
    const manager = detectPackageManager(context.workspaceRoot);
    const generation = getDaemonClientEnvGeneration();
    const pass = {
        cache: new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'playwright.hash')),
        stored: MutableRef.make(false),
        digest: hashArray([hashObject(settings), yield* OWN]),
        lock: getLockFileName(manager),
        exec: EXEC[manager],
        outputPath: settings.outputPath,
    };
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, context, file)), configs, settings, context)),
        Effect.when(
            Effect.sync(() => pass.cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(pass.stored) && getDaemonClientEnvGeneration() === generation),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = ['**/playwright.config.{js,mjs,ts,cjs,mts,cts}', nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
