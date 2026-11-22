import { createRequire, registerHooks } from 'node:module';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { applyDaemonEnvFromClient, calculateHashesForCreateNodes, emitPluginWorkerLog, getAppliedDaemonClientEnv, getDaemonClientEnvGeneration, getLockFileName, hashDaemonClientEnv, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { aggregated, anchored, extended, fingerprinted, Folder, type Input, manifested, namedInput, packaged, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Clock, Effect, Function, Option, Predicate, Record, Result, Schema, Semaphore } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUN = /^(?:(?:npx|yarn|bunx?|bun x|pnpm(?: exec)?|pnpx) )?nx (?:run (?<project>[^\s:]+):(?<target>[^\s:]+)(?::\S*)?|(?<target>\S+) (?<project>\S+))$/u;
const NX = /(?:^|\s)nx(?:@\S+)?\s/u;
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const OWN = fingerprinted(import.meta.dirname);
const LOADING = Semaphore.makeUnsafe(1);
const MODULES = createRequire(import.meta.url).cache;
const REPORTS: Readonly<Record<string, string>> = { html: 'playwright-report', blob: 'blob-report' };

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({
    description:
        'outputPath: workspace folder a suite writes test-results and the files of each built-in reporter to, {projectName} naming the project, each config project with its own outputDir under it and test files its testMatch finds running as pack--playwright into that outputDir, absent leaves Playwright its own folders and infers no pack--playwright',
});
const Loader = Schema.Struct({
    configLoader: Schema.Struct({ loadConfigFromFile: Schema.declare((input: unknown): input is (file: string, overrides: { readonly outputDir?: string }) => Promise<unknown> => Predicate.isFunction(input)) }),
    cc: Schema.Struct({
        addToCompilationCache: Schema.declare((input: unknown): input is (payload: { readonly sourceMaps: readonly []; readonly memoryCache: readonly (readonly [string, object])[]; readonly fileDependencies: readonly [] }) => void => Predicate.isFunction(input)),
    }),
    resolveReporterOutputPath: Schema.declare((input: unknown): input is (defaultValue: string, configDir: string, configValue: undefined) => string => Predicate.isFunction(input)),
    projectUtils: Schema.Struct({ collectFilesForProject: Schema.declare((input: unknown): input is (project: unknown) => Promise<readonly string[]> => Predicate.isFunction(input)) }),
}).annotate({
    description:
        'The configLoader and compilation cache members of playwright/lib/common, the loader playwright test resolves its configuration with and the per-process cache of the code it compiled for each file, the resolveReporterOutputPath member of playwright/lib/util, which places the default folder of a built-in reporter beside the package.json nearest the config, and the projectUtils member of playwright/lib/runner, whose collectFilesForProject lists the test files a project runs through its testDir, testMatch, and testIgnore',
});
const Listed = Schema.Struct({ projects: Schema.Array(Schema.Unknown) }).annotate({ description: 'The FullProjectInternal rows of a loaded config, each passed whole to collectFilesForProject' });
const Server = Schema.Struct({
    command: Schema.optional(Schema.String),
    reuseExistingServer: Schema.optional(Schema.Boolean),
    env: Schema.optional(Schema.Record(Schema.String, Schema.Unknown)),
    wait: Schema.optional(Schema.Struct({ stdout: Schema.optional(Schema.Unknown), stderr: Schema.optional(Schema.Unknown) })),
}).annotate({ description: 'A webServer row: the command Playwright starts, whether it reuses a server already listening, and the env and output wait that bind it to the process Playwright starts' });
const Loaded = Schema.Struct({
    configDir: Schema.String,
    webServers: Schema.Array(Server),
    config: Schema.Struct({ reporter: Schema.Array(Schema.Tuple([Schema.String, Schema.optionalKey(Schema.Unknown)])) }),
    projects: Schema.Array(Schema.Struct({ project: Schema.Struct({ name: Schema.String, outputDir: Schema.String, webServer: Schema.Array(Server) }) })),
}).annotate({ description: 'The FullConfigInternal loadConfigFromFile returns: config folder, webServer rows, resolved reporters, and the name, output folder, and webServer rows of each project' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The file and folder options of a resolved [reporter, options] Playwright entry',
});
const Task = Schema.Struct({ project: Schema.String, target: Schema.String }).annotate({ description: 'The project and target of an nx run command' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const invocation = (flags: readonly string[]) => [['playwright', 'test'].map(quoteShellArg).join(' '), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (given) => ['{args}', given.map(quoteShellArg).join(' ')] })].join(' ');

const directed = (to: string, reporters: readonly string[]) => {
    const [results, reports] = [posix.join(to, 'test-results'), Record.fromIterableWith(reporters, (name) => [`PLAYWRIGHT_${name.toUpperCase()}_OUTPUT_DIR`, posix.join(to, name)])];
    return { flags: [`--output=${results}`], env: reports, folders: [results, ...Object.values(reports)] };
};

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, output: Option.Option<string>, lock: string) {
    const entry = join(workspaceRoot, file);
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const playwright = createRequire(createRequire(entry).resolve('@playwright/test'));
    const { configLoader, cc, resolveReporterOutputPath, projectUtils } = yield* Effect.flatMap(
        Effect.forEach(['playwright/lib/common', 'playwright/lib/util', 'playwright/lib/runner'], (at) => Effect.tryPromise({ try: (): Promise<unknown> => import(pathToFileURL(playwright.resolve(at)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${at} through @playwright/test`) })),
        (modules) => Schema.decodeUnknownEffect(Loader)(Object.assign({}, ...modules)),
    );
    const edges: { readonly specifier: string; readonly parent: string | undefined; readonly url: string; readonly path: string }[] = [];
    const fresh = yield* Clock.currentTimeNanos;
    const [{ configDir, webServers, config: resolvedConfig, projects }, listed] = yield* LOADING.withPermit(
        Effect.acquireUseRelease(
            Effect.sync(() => ({
                hooks: registerHooks({
                    resolve: (specifier, context, nextResolve) => {
                        const resolved = nextResolve(specifier, context);
                        const path = resolved.url.startsWith('file:') ? fileURLToPath(resolved.url) : undefined;
                        edges.push(...(path === undefined ? [] : [{ specifier, parent: context.parentURL, url: resolved.url, path }]));
                        return path !== undefined && within(local(path)) && context.conditions.includes('import') ? { ...resolved, url: `${resolved.url}?${fresh}` } : resolved;
                    },
                }),
                ambient: { ...env },
                applied: getAppliedDaemonClientEnv()?.sequence,
            })),
            () =>
                Effect.flatMap(
                    Effect.tryPromise({
                        try: () => configLoader.loadConfigFromFile(entry, {}),
                        catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Playwright config`),
                    }),
                    (full) => Effect.all([Schema.decodeUnknownEffect(Loaded)(full), Schema.decodeUnknownEffect(Listed)(full)]),
                ),
            ({ hooks, ambient, applied }) => {
                const compiled = edges.filter(({ path }) => within(local(path))).map(({ path }): readonly [string, object] => [path, {}]);
                const client = Option.filter(Option.fromUndefinedOr(getAppliedDaemonClientEnv()), ({ sequence }) => sequence !== applied);
                return Effect.forEach(
                    [
                        () => hooks.deregister(),
                        ...compiled.map(
                            ([path]) =>
                                () =>
                                    Reflect.deleteProperty(MODULES, path),
                        ),
                        () => cc.addToCompilationCache({ sourceMaps: [], memoryCache: compiled, fileDependencies: [] }),
                        ...Object.keys(env)
                            .filter((name) => !(name in ambient))
                            .map((name) => () => Reflect.deleteProperty(env, name)),
                        () => {
                            Object.assign(env, ambient);
                        },
                        ...Option.toArray(client).map(
                            ({ env: next }) =>
                                () =>
                                    applyDaemonEnvFromClient(next),
                        ),
                    ],
                    Effect.sync,
                    { discard: true },
                );
            },
        ),
    );
    const collected = yield* Effect.forEach(listed.projects, (project) => Effect.tryPromise({ try: () => projectUtils.collectFilesForProject(project), catch: (cause) => new Cause.UnknownError(cause, `${file} lists no test files of its projects through Playwright`) }));
    const loaded = Array.dedupe([file, ...edges.map(({ path }) => local(path))]).filter(within);
    const { packages, linked } = yield* packaged(
        Array.getSomes(
            edges.map(({ specifier, parent }) =>
                Option.map(
                    Option.filter(Option.liftPredicate(parent, Predicate.isString), (at) => at.startsWith('file:') && within(local(fileURLToPath(at)))),
                    (at) => [specifier, at] as const,
                ),
            ),
        ),
    );
    const reports = resolvedConfig.reporter.flatMap(([name, options]) => {
        const { outputFile, outputDir, outputFolder } = Option.getOrElse(Schema.decodeUnknownOption(Reported)(options), (): typeof Reported.Type => ({}));
        const stated = Option.map(Option.fromUndefinedOr(outputDir ?? outputFolder), (at) => resolve(configDir, at));
        const fallback = Option.map(Record.get(REPORTS, name), (at) => resolveReporterOutputPath(at, configDir, undefined));
        const folder = Option.filter(
            Option.orElse(stated, () => fallback),
            () => Option.isNone(output),
        );
        return Option.toArray(
            Option.orElse(
                Option.map(Option.fromUndefinedOr(outputFile), (at) => resolve(configDir, at)),
                () => folder,
            ),
        );
    });
    const commands = [...webServers, ...projects.flatMap(({ project }) => project.webServer)]
        .filter(({ reuseExistingServer }) => reuseExistingServer === true)
        .map((server) => Option.filter(Option.fromNullishOr(server.command), () => server.wait?.stdout === undefined && server.wait?.stderr === undefined && Record.isEmptyRecord(server.env ?? {})));
    const tasks = commands.map(Option.flatMap((command) => Schema.decodeUnknownOption(Task)(RUN.exec(command)?.groups)));
    const unread = Array.getSomes(Array.zipWith(commands, tasks, (command, task) => Option.filter(command, (text) => Option.isNone(task) && NX.test(text))));
    yield* Effect.forEach(unread, (command) => Effect.sync(() => emitPluginWorkerLog('warn', `${PLUGIN} reads no Nx task from webServer command "${command}" in ${file}, so test--playwright waits on no server task`)), { discard: true });
    const sources = [lock, ...loaded];
    return {
        packages,
        linked,
        loaded,
        servers: Array.dedupeWith(Array.getSomes(tasks), (left, right) => left.project === right.project && left.target === right.target),
        reported: Array.dedupe(reports.map(local)),
        suites: Array.zipWith(projects, collected, ({ project: { name, outputDir } }, files) => ({ name, outputDir: local(outputDir), found: Array.isReadonlyArrayNonEmpty(files) })),
        reporters: resolvedConfig.reporter.map(([name]) => name),
        sources,
        stamp: yield* stamped(workspaceRoot)(sources),
    };
});

const templated = (folder: string) => new RegExp(`^${folder.split('{projectName}').map(RegExp.escape).join('.+')}(?:/|$)`, 'u');

const targets = (home: string, { loaded: configs, packages, linked, servers, reported, reporters, suites }: Resolved, chain: readonly string[], upstream: readonly string[], { outputPath }: typeof Options.Type, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const routed = outputPath === undefined ? { flags: [], env: {}, folders: [] } : directed(posix.relative(home, outputPath), reporters);
    const held = routed.folders.map((at) => posix.join(home, at));
    const under = Option.map(Option.fromUndefinedOr(outputPath), templated);
    const unheld = (at: string) => !held.map(templated).some((folder) => folder.test(at));
    const [packed, tested] = Array.partition(suites, (suite) => Result.liftPredicate(suite, ({ outputDir, found }) => found && unheld(outputDir) && Option.exists(under, (folder) => folder.test(outputDir)), Function.identity));
    const selected = (chosen: readonly (typeof suites)[number][]) => chosen.map(({ name }) => `--project=${name}`);
    const outside = Record.filter(
        Record.fromEntries([...chain.map((at): readonly [string, typeof Input.Type] => [at, { json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions', 'extends'] }]), ...configs.map((at): readonly [string, typeof Input.Type] => [at, posix.join('{workspaceRoot}', at)])]),
        (_input, at) => home !== '.' && !at.startsWith(`${home}/`) && at !== rooted,
    );
    const test = {
        playwright: {
            command: invocation([...Array.match(packed, { onEmpty: () => [], onNonEmpty: () => selected(tested) }), ...routed.flags]),
            options: { cwd: home, ...(Record.isEmptyRecord(routed.env) ? {} : { env: routed.env }) },
            dependsOn: [
                'codegen--*',
                '^codegen--*',
                '^build--*',
                ...Record.collect(
                    Array.groupBy(servers, ({ target }) => target),
                    (target, rows) => ({ projects: rows.map(({ project }) => project), target, waitFor: 'ready' as const }),
                ),
            ],
            cache: true,
            inputs: [
                'default',
                ...upstream,
                ...Array.match(linked, {
                    onEmpty: () => [],
                    onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }],
                }),
                ...Object.values(outside),
                { dependentTasksOutputFiles: '**/*', transitive: true },
                { env: 'CI' },
                { externalDependencies: Array.dedupe(['@playwright/test', ...packages]) },
            ],
            outputs: Array.dedupe([...held, ...[...(outputPath === undefined ? tested.map(({ outputDir }) => outputDir) : []), ...reported].filter(unheld)]).map(tokened(home)),
            metadata: {
                description: 'Runs every Playwright project of the config whose own outputDir sits outside outputPath with playwright test after the build targets of each project the suite depends on and once the Nx task each reused webServer row runs is ready',
                help: { command: `${exec} playwright test --help`, example: { args: ['--project', '<name>'] } },
            },
        },
    } satisfies Record<string, TargetConfiguration>;
    const pack: Record<string, TargetConfiguration> = Array.match(packed, {
        onEmpty: () => ({}),
        onNonEmpty: (chosen) => ({
            playwright: {
                command: invocation([...selected(chosen), '--no-deps', '--reporter=list']),
                options: { cwd: home },
                dependsOn: ['test--playwright'],
                cache: true,
                inputs: test.playwright.inputs,
                outputs: chosen.map(({ outputDir }) => outputDir).map(tokened(home)),
                metadata: {
                    description: `Writes the files of the Playwright projects ${chosen.map(({ name }) => name).join(', ')} into each one's own outputDir under outputPath, which playwright test empties first, after test--playwright passes`,
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

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved & { readonly digest: string }>, context: CreateNodesContext, settings: typeof Options.Type, hash: string, file: string) {
    const home = dirname(file);
    const digest = hashArray([hash, yield* OWN]);
    const manager = detectPackageManager(context.workspaceRoot);
    const held = Option.filter(yield* recalled(cache, file, stamped(context.workspaceRoot)), (cached) => cached.digest === digest);
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => read(context.workspaceRoot, file, Option.fromUndefinedOr(settings.outputPath), getLockFileName(manager)) });
    cache.set(file, { ...resolved, digest });
    const stated = yield* manifested(context.workspaceRoot, home);
    const project: ProjectConfiguration = {
        root: home,
        tags: ['host:browser'],
        metadata: { technologies: ['playwright'] },
        targets: aggregated(targets(home, resolved, extended(context.workspaceRoot, home), namedInput(home, context) === 'production' ? ['^production', '^{projectRoot}/tsconfig*.json'] : ['^default'], settings, getPackageManagerCommand(manager).exec), stated, 'playwright'),
    };
    return { projects: { [home]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const project = anchored(context.workspaceRoot);
    const configs = files.filter((file) => project(dirname(file)));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const cache = new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'playwright.hash'));
    const [ambient, generation] = [hashDaemonClientEnv(), getDaemonClientEnvGeneration()];
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            { ...settings, ambient },
            context,
            configs.map(() => [lock]),
        ),
    );
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file, _options, _context, index) => run(inferred(cache, context, settings, `${hashes[index]}`, file)), configs, options, context)),
        Effect.sync(() => getDaemonClientEnvGeneration() === generation && hashDaemonClientEnv() === ambient && cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/playwright.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
