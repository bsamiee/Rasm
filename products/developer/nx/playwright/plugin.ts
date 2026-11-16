import { createRequire, registerHooks } from 'node:module';
import { dirname, extname, join, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { applyDaemonEnvFromClient, calculateHashesForCreateNodes, emitPluginWorkerLog, getAppliedDaemonClientEnv, getDaemonClientEnvGeneration, getLockFileName, hashDaemonClientEnv, hashObject, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { aggregated, anchored, extended, fingerprinted, Folder, type Input, manifested, namedInput, packaged, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Clock, Effect, Option, Predicate, Record, Schema, Semaphore } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUN = /^(?:(?:npx|yarn|bunx?|bun x|pnpm(?: exec)?|pnpx) )?nx (?:run (?<project>[^\s:]+):(?<target>[^\s:]+)(?::\S*)?|(?<target>\S+) (?<project>\S+))$/u;
const NX = /(?:^|\s)nx(?:@\S+)?\s/u;
const REPORTS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', junit: 'junit.xml', json: 'report.json', perfetto: 'perfetto.json' };
const FOLDERS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', coverage: 'coverage' };
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const OWN = fingerprinted(import.meta.dirname);
const LOADING = Semaphore.makeUnsafe(1);
const MODULES = createRequire(import.meta.url).cache;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder }).annotate({
    description: 'outputPath: workspace folder a suite writes test-results and the files of each built-in reporter to, {projectName} naming the project, absent leaves Playwright its own folders',
});
const Loader = Schema.Struct({
    configLoader: Schema.Struct({ loadConfigFromFile: Schema.declare((input: unknown): input is (file: string, overrides: { readonly outputDir?: string }) => Promise<unknown> => Predicate.isFunction(input)) }),
    cc: Schema.Struct({
        addToCompilationCache: Schema.declare((input: unknown): input is (payload: { readonly sourceMaps: readonly []; readonly memoryCache: readonly (readonly [string, object])[]; readonly fileDependencies: readonly [] }) => void => Predicate.isFunction(input)),
    }),
}).annotate({ description: 'The configLoader and compilation cache members of playwright/lib/common, the loader playwright test resolves its configuration with and the per-process cache of the code it compiled for each file' });
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
    projects: Schema.Array(Schema.Struct({ project: Schema.Struct({ outputDir: Schema.String, webServer: Schema.Array(Server) }) })),
}).annotate({ description: 'The FullConfigInternal loadConfigFromFile returns: config folder, webServer rows, resolved reporters, and the output folder and webServer rows of each project' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The file and folder options of a resolved [reporter, options] Playwright entry',
});
const Task = Schema.Struct({ project: Schema.String, target: Schema.String }).annotate({ description: 'The project and target of an nx run command' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const directed = (to: string) => ({ flags: [`--output=${join(to, 'test-results')}`], env: Record.mapEntries(REPORTS, (at, name) => [`PLAYWRIGHT_${name.toUpperCase()}_OUTPUT_${extname(at) === '' ? 'DIR' : 'FILE'}`, join(to, at)]) });

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, output: Option.Option<string>, lock: string) {
    const [home, entry] = [dirname(file), join(workspaceRoot, file)];
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const { configLoader, cc } = yield* Effect.flatMap(
        Effect.tryPromise({
            try: (): Promise<unknown> => import(pathToFileURL(createRequire(createRequire(entry).resolve('@playwright/test')).resolve('playwright/lib/common')).href),
            catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no playwright/lib/common through @playwright/test`),
        }),
        Schema.decodeUnknownEffect(Loader),
    );
    const edges: { readonly specifier: string; readonly parent: string | undefined; readonly url: string; readonly path: string }[] = [];
    const fresh = yield* Clock.currentTimeNanos;
    const {
        configDir,
        webServers,
        config: resolvedConfig,
        projects,
    } = yield* LOADING.withPermit(
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
                        try: () => configLoader.loadConfigFromFile(entry, Option.match(output, { onNone: () => ({}), onSome: (to) => ({ outputDir: resolve(workspaceRoot, to, 'test-results') }) })),
                        catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Playwright config`),
                    }),
                    Schema.decodeUnknownEffect(Loaded),
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
    const reports = Array.getSomes(
        resolvedConfig.reporter.map(([name, options]) => {
            const stated = Option.flatMap(Schema.decodeUnknownOption(Reported)(options), ({ outputFile, outputDir, outputFolder }) => Option.fromUndefinedOr(outputFile ?? outputDir ?? outputFolder));
            const placed = Option.firstSomeOf([Option.map(stated, (at) => resolve(configDir, at)), Option.map(Record.get(FOLDERS, name), (at) => join(workspaceRoot, home, at))]);
            return Option.filter(placed, () => Option.isNone(output) || !(name in REPORTS));
        }),
    );
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
        folders: Array.dedupe([...projects.map(({ project }) => project.outputDir), ...reports].map(local)),
        sources,
        stamp: yield* stamped(workspaceRoot)(sources),
    };
});

const targets = (home: string, { loaded: configs, packages, linked, servers, folders }: Resolved, chain: readonly string[], upstream: readonly string[], { outputPath }: typeof Options.Type, exec: string): Verbs => {
    const held = Array.fromNullishOr(outputPath);
    const rooted = getRootTsConfigFileName();
    const routed = outputPath === undefined ? { flags: [], env: {} } : directed(relative(home, outputPath));
    const outside = Record.filter(
        Record.fromEntries([...chain.map((at): readonly [string, typeof Input.Type] => [at, { json: join('{workspaceRoot}', at), fields: ['compilerOptions', 'extends'] }]), ...configs.map((at): readonly [string, typeof Input.Type] => [at, join('{workspaceRoot}', at)])]),
        (_input, at) => home !== '.' && !at.startsWith(`${home}/`) && at !== rooted,
    );
    const test = {
        playwright: {
            command: [['playwright', 'test'].map(quoteShellArg).join(' '), ...Array.match(routed.flags, { onEmpty: () => [], onNonEmpty: (given) => ['{args}', given.map(quoteShellArg).join(' ')] })].join(' '),
            options: { cwd: home, ...(Record.isEmptyRecord(routed.env) ? {} : { env: routed.env }) },
            dependsOn: [
                'codegen--*',
                '^codegen',
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
            outputs: [...held, ...folders.filter((at) => !held.some((folder) => at === folder || at.startsWith(`${folder}/`)))].map(tokened(home)),
            metadata: {
                description: 'Runs every Playwright project of the config with playwright test after the Nx task each reused webServer row runs is ready',
                help: { command: `${exec} playwright test --help`, example: { args: ['--project', '<name>'] } },
            },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(test, (tool) => `test--${tool}`);
    return {
        test,
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

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, context: CreateNodesContext, settings: typeof Options.Type, hash: string, file: string) {
    const home = dirname(file);
    const key = hashArray([hash, yield* OWN, file]);
    const manager = detectPackageManager(context.workspaceRoot);
    const held = yield* recalled(cache, key, stamped(context.workspaceRoot));
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => read(context.workspaceRoot, file, Option.fromUndefinedOr(settings.outputPath), getLockFileName(manager)) });
    cache.set(key, resolved);
    const stated = yield* manifested(context.workspaceRoot, home);
    const project: ProjectConfiguration = {
        root: home,
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
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `playwright-${hashObject(options ?? {})}.hash`));
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
