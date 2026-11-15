import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON, registerHooks } from 'node:module';
import { dirname, extname, isAbsolute, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Clock, Effect, Equal, FileSystem, identity, Option, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const RUN = /(?:^|\s)nx run (?<project>[^\s:]+):(?<target>[^\s:]+)$/u;
const REPORTS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', junit: 'junit.xml', json: 'report.json', perfetto: 'perfetto.json' };
const FOLDERS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report', coverage: 'coverage' };
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const OWN = pipe(
    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(import.meta.dirname)),
    Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
    Effect.cached,
    Effect.runSync,
);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'outputPath: workspace folder a suite writes test-results and the files of each built-in reporter to, {projectName} naming the project, absent leaves Playwright its own folders',
});
const Loader = Schema.Struct({
    configLoader: Schema.Struct({ loadConfigFromFile: Schema.instanceOf(Function) }),
}).annotate({ description: 'The configLoader member of playwright/lib/common, the loader playwright test resolves its configuration with' });
const Server = Schema.Struct({
    command: Schema.optional(Schema.String),
    reuseExistingServer: Schema.optional(Schema.Boolean),
    env: Schema.optional(Schema.Record(Schema.String, Schema.Unknown)),
    wait: Schema.optional(Schema.Unknown),
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

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    return (yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, root, 'package.json')), Schema.decodeEffect(Overrides))).nx?.targets ?? {};
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
    const { configLoader } = yield* Effect.flatMap(
        Effect.tryPromise({
            try: (): Promise<unknown> => import(pathToFileURL(createRequire(createRequire(entry).resolve('@playwright/test')).resolve('playwright/lib/common')).href),
            catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no playwright/lib/common through @playwright/test`),
        }),
        Schema.decodeUnknownEffect(Loader),
    );
    const edges: { readonly specifier: string; readonly parent: string | undefined; readonly url: string }[] = [];
    const fresh = yield* Clock.currentTimeNanos;
    const config = yield* Effect.acquireUseRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, context, next) => {
                    const resolved = next(specifier, context);
                    const at = resolved.url.startsWith('file:') ? Option.some(fileURLToPath(resolved.url)) : Option.none();
                    edges.push({ specifier, parent: context.parentURL, url: resolved.url });
                    return Option.exists(at, (path) => kept(local(path)) && context.conditions.includes('import')) ? { ...resolved, url: `${resolved.url}?${fresh}` } : resolved;
                },
            }),
        ),
        () =>
            Effect.tryPromise({
                try: () => configLoader.loadConfigFromFile(entry, Option.match(output, { onNone: () => ({}), onSome: (to) => ({ outputDir: resolve(workspaceRoot, to, 'test-results') }) })),
                catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Playwright config`),
            }),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const { configDir, webServers, config: resolvedConfig, projects } = yield* Schema.decodeUnknownEffect(Loaded)(config);
    const files = edges.filter(({ url }) => url.startsWith('file:')).map((edge) => ({ ...edge, path: fileURLToPath(edge.url) }));
    const loaded = Array.dedupe([file, ...files.map(({ path }) => local(path))]).filter(kept);
    const imported = files.filter(({ specifier, parent }) => bare(specifier) && Predicate.isString(parent) && parent.startsWith('file:') && kept(local(fileURLToPath(parent))));
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
    const servers = [...webServers, ...projects.flatMap(({ project }) => project.webServer)].map((server) =>
        Option.filter(
            Option.flatMap(Option.fromNullishOr(server.command), (command) => Schema.decodeUnknownOption(Task)(RUN.exec(command)?.groups)),
            () => server.reuseExistingServer === true && server.wait === undefined && Record.isEmptyRecord(server.env ?? {}),
        ),
    );
    const sources = [lock, ...loaded];
    return {
        packages: Array.dedupe(packages),
        linked: Array.dedupe(linked),
        loaded,
        servers: Array.dedupeWith(Array.getSomes(servers), (left, right) => left.project === right.project && left.target === right.target),
        parallelism: servers.every(Option.isSome),
        folders: Array.dedupe([...projects.map(({ project }) => project.outputDir), ...reports].map(local)),
        sources,
        stamp: yield* stamped(workspaceRoot, sources),
    };
});

const targets = (home: string, { loaded: configs, packages, linked, servers, parallelism, folders }: Resolved, chain: readonly string[], upstream: string, { outputPath }: typeof Options.Type, exec: string): Verbs => {
    const held = Array.fromNullishOr(outputPath);
    const rooted = getRootTsConfigFileName();
    const { flags, env } = outputPath === undefined ? { flags: [], env: {} } : directed(relative(home, outputPath));
    const outside = Record.filter(
        Record.fromEntries([
            ...chain.map((at): readonly [string, typeof Input.Type] => [at, { json: join('{workspaceRoot}', at), fields: ['compilerOptions'], ...(at === rooted && { excludeFields: ['compilerOptions.paths'] }) }]),
            ...configs.map((at): readonly [string, typeof Input.Type] => [at, join('{workspaceRoot}', at)]),
        ]),
        (_input, at) => home !== '.' && !at.startsWith(`${home}/`),
    );
    const test = {
        playwright: {
            command: [['playwright', 'test'].map(quoteShellArg).join(' '), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (given) => ['{args}', given.map(quoteShellArg).join(' ')] })].join(' '),
            options: { cwd: home, ...(Record.isEmptyRecord(env) ? {} : { env }) },
            dependsOn: ['codegen--*', '^codegen', ...servers.map(({ project, target }) => ({ projects: [project], target, waitFor: 'ready' as const }))],
            cache: true,
            parallelism,
            inputs: [
                'default',
                upstream,
                ...Array.match(linked, {
                    onEmpty: () => [],
                    onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }],
                }),
                ...Object.values(outside),
                { dependentTasksOutputFiles: '**/*', transitive: true },
                { env: 'CI' },
                { externalDependencies: Array.dedupe(['@playwright/test', ...packages]) },
            ],
            outputs: [...held, ...folders.filter((at) => !held.some((folder) => at === folder || at.startsWith(`${folder}/`)))].map((at) => join('{workspaceRoot}', at)),
            metadata: {
                description: 'Runs every Playwright project of the config with playwright test after the Nx task each webServer row names is ready, beside other tasks only where Nx starts every web server',
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
        targets: aggregated(targets(home, resolved, extended(context.workspaceRoot, home), 'production' in getNamedInputs(home, context) ? '^production' : '^default', settings, getPackageManagerCommand(manager).exec), stated, 'playwright'),
    };
    return { projects: { [home]: project } };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const configs = files.filter((file) => existsSync(join(context.workspaceRoot, dirname(file), 'package.json')));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `playwright-${hashObject(options ?? {})}.hash`));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            settings,
            context,
            configs.map(() => [lock]),
        ),
    );
    return yield* Effect.ensuring(
        Effect.tryPromise({
            try: () => createNodesFromFiles((file, _options, _context, index) => run(Effect.catchCause(inferred(cache, context, settings, `${hashes[index]}`, file), rendered)), configs, options, context),
            catch: (cause) => (cause instanceof AggregateCreateNodesError ? cause : new AggregateCreateNodesError([[null, new Cause.UnknownError(cause, `${PLUGIN} infers no project`)]], [])),
        }),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = ['**/playwright.config.{js,mjs,ts,cjs,mts,cts}', (files, options, context) => run(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
