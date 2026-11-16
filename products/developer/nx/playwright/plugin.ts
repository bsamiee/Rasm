import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON, registerHooks } from 'node:module';
import { dirname, extname, isAbsolute, join, relative, resolve, sep } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import {
    applyDaemonEnvFromClient,
    buildPackageJsonPatterns,
    buildPackageJsonWorkspacesMatcher,
    calculateHashesForCreateNodes,
    emitPluginWorkerLog,
    getAppliedDaemonClientEnv,
    getDaemonClientEnvGeneration,
    getLockFileName,
    getNamedInputs,
    hashDaemonClientEnv,
    hashFile,
    hashObject,
    hashWithWorkspaceContext,
    mergeTargetConfigurations,
    type PackageJson,
    PluginCache,
    quoteShellArg,
    workspaceDataDirectory,
} from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Clock, Effect, Equal, FileSystem, identity, Option, Predicate, pipe, Record, Result, Schema, Semaphore } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUN = /^(?:(?:npx|yarn|bunx?|bun x|pnpm(?: exec)?|pnpx) )?nx (?:run (?<project>[^\s:]+):(?<target>[^\s:]+)(?::\S*)?|(?<target>\S+) (?<project>\S+))$/u;
const NX = /(?:^|\s)nx(?:@\S+)?\s/u;
const REPORTS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', junit: 'junit.xml', json: 'report.json', perfetto: 'perfetto.json' };
const FOLDERS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', coverage: 'coverage' };
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const OWN = pipe(
    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(import.meta.dirname)),
    Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
    Effect.cached,
    Effect.runSync,
);
const LOADING = Semaphore.makeUnsafe(1);
const MODULES = createRequire(import.meta.url).cache;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
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
const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json' });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, FileSystem.FileSystem>) => Effect.runPromise(Effect.provide(effect, layer));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const aggregated = (verbs: Verbs, stated: Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [`${verb}--*`],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((input) => (Predicate.isObject(input) && 'dependentTasksOutputFiles' in input ? { ...input, transitive: true } : input)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [`${verb}--${tool}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]),
        ]).flat(),
    );

const above = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );

const extended = (workspaceRoot: string, root: string): readonly string[] => {
    const files: string[] = [];
    Option.toArray(
        Array.findFirst(
            above(root).map((at) => join(workspaceRoot, at, 'tsconfig.json')),
            existsSync,
        ),
    ).forEach((config) =>
        walkTsconfigExtendsChain(config, (at) => {
            files.push(at);
            return 'continue';
        }),
    );
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter(kept);
};

const directed = (to: string) => ({ flags: [`--output=${join(to, 'test-results')}`], env: Record.mapEntries(REPORTS, (at, name) => [`PLAYWRIGHT_${name.toUpperCase()}_OUTPUT_${extname(at) === '' ? 'DIR' : 'FILE'}`, join(to, at)]) });

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const kept = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const bare = (specifier: string) => !(specifier.startsWith('.') || specifier.startsWith('#') || isAbsolute(specifier) || URL.canParse(specifier));

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, output: Option.Option<string>, lock: string) {
    const fs = yield* FileSystem.FileSystem;
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
                        return path !== undefined && kept(local(path)) && context.conditions.includes('import') ? { ...resolved, url: `${resolved.url}?${fresh}` } : resolved;
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
                const owned = edges.filter(({ path }) => kept(local(path))).map(({ path }): readonly [string, object] => [path, {}]);
                const client = Option.filter(Option.fromUndefinedOr(getAppliedDaemonClientEnv()), ({ sequence }) => sequence !== applied);
                return Effect.forEach(
                    [
                        () => hooks.deregister(),
                        ...owned.map(
                            ([path]) =>
                                () =>
                                    Reflect.deleteProperty(MODULES, path),
                        ),
                        () => cc.addToCompilationCache({ sourceMaps: [], memoryCache: owned, fileDependencies: [] }),
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
    const loaded = Array.dedupe([file, ...edges.map(({ path }) => local(path))]).filter(kept);
    const imported = edges.filter(({ specifier, parent }) => bare(specifier) && Predicate.isString(parent) && parent.startsWith('file:') && kept(local(fileURLToPath(parent))));
    const manifests = Array.dedupe(Array.getSomes(imported.map(({ url }) => Option.flatMap(Option.liftThrowable(findPackageJSON)(url), Option.fromUndefinedOr))));
    const named = yield* Effect.forEach(manifests, (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail(name))), { concurrency: 'unbounded' });
    const [packages, linked] = Array.partition(named, identity);
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
    yield* Effect.forEach(unread, (command) => Effect.sync(() => emitPluginWorkerLog('warn', `${PLUGIN} reads no Nx task from webServer command "${command}" in ${file}, so test--playwright starts no server task and runs alone`)), { discard: true });
    const sources = [lock, ...loaded];
    return {
        packages: Array.dedupe(packages),
        linked: Array.dedupe(linked),
        loaded,
        servers: Array.dedupeWith(Array.getSomes(tasks), (left, right) => left.project === right.project && left.target === right.target),
        parallelism: tasks.length > 0 && tasks.every(Option.isSome),
        folders: Array.dedupe([...projects.map(({ project }) => project.outputDir), ...reports].map(local)),
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
    };
});

const targets = (home: string, { loaded: configs, packages, linked, servers, parallelism, folders }: Resolved, chain: readonly string[], upstream: readonly string[], { outputPath }: typeof Options.Type, exec: string): Verbs => {
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
            ...(!parallelism && { parallelism }),
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
            outputs: [...held, ...folders.filter((at) => !held.some((folder) => at === folder || at.startsWith(`${folder}/`)))].map((at) => (relative(home, at).startsWith('..') ? join('{workspaceRoot}', at) : join('{projectRoot}', relative(home, at)))),
            metadata: {
                description: 'Runs every Playwright project of the config with playwright test after the Nx task each reused webServer row runs is ready, beside other tasks only where an Nx task starts every reused web server',
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
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(key))), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => read(context.workspaceRoot, file, Option.fromUndefinedOr(settings.outputPath), getLockFileName(manager)) });
    cache.set(key, resolved);
    const stated = yield* manifested(context.workspaceRoot, home);
    const project: ProjectConfiguration = {
        root: home,
        metadata: { technologies: ['playwright'] },
        targets: aggregated(targets(home, resolved, extended(context.workspaceRoot, home), 'production' in getNamedInputs(home, context) ? ['^production', '^{projectRoot}/tsconfig*.json'] : ['^default'], settings, getPackageManagerCommand(manager).exec), stated, 'playwright'),
    };
    return { projects: { [home]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(context.workspaceRoot, (file) => readJsonFile<PackageJson>(join(context.workspaceRoot, file))));
    const configs = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(file), 'project.json')) || (member(join(dirname(file), 'package.json')) && existsSync(join(context.workspaceRoot, dirname(file), 'package.json'))));
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
