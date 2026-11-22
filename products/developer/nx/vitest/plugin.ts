import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, matchesGlob, posix, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, calculateHashesForCreateNodes, getLockFileName, type PackageJson, PluginCache, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { above, aggregated, extended, fingerprinted, Folder, type Input, manifested, namedInput, packaged, parsed, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';
import type { configDefaults } from 'vitest/config';
import type { parseCLI, resolveConfig, Vite } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const ATTACHMENTS = 'attachments';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Folder, coveragePath: Folder }).annotate({
    description:
        'outputPath: workspace folder a suite writes its reports to, {projectName} naming the project, absent leaves each reporter its own folder. coveragePath: workspace folder the coverage configuration writes lcov.info to, {projectName} naming the project, absent leaves Vitest its coverage.reportsDirectory',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The file and folder options of a resolved [reporter, options] Vitest entry',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const directed = (to: string) => ['--attachmentsDir', posix.join(to, ATTACHMENTS)];

const testMode = Effect.acquireRelease(
    Effect.sync(() => {
        const before = { ...env };
        const { NODE_ENV = MODE.NODE_ENV } = before;
        Object.assign(env, { ...MODE, NODE_ENV });
        return before;
    }),
    (before) =>
        Effect.sync(() => {
            Record.keys(MODE).forEach((name) => {
                Reflect.deleteProperty(env, name);
            });
            Object.assign(env, before);
        }),
);

const loaded = <A>(file: string, entry: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(entry).resolve(specifier)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* (workspaceRoot: string, file: string, flags: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [folder, entry] = [join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const [runner, defaults, vite] = yield* Effect.all([loaded<{ readonly parseCLI: typeof parseCLI; readonly resolveConfig: typeof resolveConfig }>(file, entry, 'vitest/node'), loaded<{ readonly configDefaults: typeof configDefaults }>(file, entry, 'vitest/config'), loaded<typeof Vite>(file, entry, 'vite')]);
    const config = yield* Effect.tryPromise({
        try: () =>
            retryOnRequireEsmRace(() =>
                runner.resolveConfig(
                    { ...runner.parseCLI(['vitest', 'run', ...flags]).options, config: entry, watch: false },
                    {
                        logLevel: 'silent',
                        plugins: [{ name: PLUGIN, enforce: 'pre', config: { order: 'pre', handler: ({ root: base, test }) => ({ root: resolve(folder, base ?? '.'), ...(test?.root !== undefined && { test: { root: resolve(folder, test.root) } }) }) } }],
                    },
                ),
            ),
        catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Vitest config`),
    });
    const { root, coverage, attachmentsDir, outputFile, reporters, forceRerunTriggers, projects: listed, resolvedProjects } = config.test;
    const declared = resolvedProjects.filter(({ inline, viteConfig: { configFile } }) => inline === true || configFile === undefined || configFile === entry);
    const parents = new Map(declared.filter(({ hidden }) => hidden === true).map(({ viteConfig, projectConfig }) => [viteConfig, projectConfig.name] as const));
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (at) => readJsonFile<PackageJson>(join(workspaceRoot, at))));
    const lists = Option.isSome(Option.fromNullishOr(listed));
    const probes = yield* Effect.forEach(
        declared.filter(({ hidden }) => hidden !== true),
        Effect.fnUntraced(function* ({ viteConfig, projectConfig: { name, root: base, setupFiles, globalSetup, browser } }) {
            const manifests = (lists ? above(posix.join(local(base), '.')) : [dirname(file)]).flatMap((at) => [posix.join(at, 'package.json'), posix.join(at, 'project.json')]);
            const manifest = yield* Effect.findFirst(
                manifests.filter((at) => basename(at) === 'project.json' || member(at)),
                (at) => fs.exists(join(workspaceRoot, at)),
            );
            yield* Effect.when(Effect.fail(new Cause.UnknownError(base, `${file} declares a project under ${local(base)}, which no project root holds`)), Effect.succeed(lists && Option.isNone(manifest)));
            return Option.map(manifest, (at) => ({
                owner: dirname(at),
                project: parents.get(viteConfig) ?? name,
                manifests,
                setup: [...setupFiles, ...Array.ensure(globalSetup)].map(local).filter(within),
                written: browser.enabled ? Array.getSomes([Option.fromUndefinedOr(browser.trace.tracesDir), Option.fromNullishOr(browser.screenshotDirectory)]) : [],
                providers: browser.enabled ? Array.fromNullishOr(browser.provider?.name) : [],
            }));
        }),
    ).pipe(Effect.map(Array.getSomes));
    const selectable = Array.dedupe(probes.map(({ project }) => project));
    const suites = Record.collect(
        Array.groupBy(probes, ({ owner }) => owner),
        (owner, group) => {
            const selected = Array.dedupe(group.map(({ project }) => project));
            return {
                root: owner,
                projects: selected.length === selectable.length ? [] : selected,
                setup: Array.dedupe(group.flatMap(({ setup }) => setup)).filter((at) => !at.startsWith(`${owner}/`)),
                providers: Array.dedupe(group.flatMap(({ providers }) => providers)),
                coverage: local(resolve(root, coverage.reportsDirectory)),
                written: Array.dedupe(group.flatMap(({ written }) => written).map((at) => local(resolve(root, at)))),
            };
        },
    );
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isObject(outputFile) ? outputFile : {};
    const reports = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const built = Option.liftPredicate(name, Predicate.isString);
        const pointed = Option.flatMap(built, (key) => Option.fromUndefinedOr(keyed[key]));
        return Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: direct }) => Option.fromUndefinedOr(direct)), Option.liftPredicate(outputFile, Predicate.isString), pointed]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
    });
    const depended = Array.dedupe([entry, ...config.configFileDependencies, ...declared.flatMap(({ viteConfig }) => viteConfig.configFileDependencies)].map(local)).filter(within);
    const provider = coverage.provider === 'custom' ? Array.fromNullishOr(coverage.customProviderModule) : [`@vitest/coverage-${coverage.provider}`];
    const { packages, linked } = yield* packaged([...provider.map((specifier) => [specifier, entry] as const), ...(yield* parsed(vite.parseSync, workspaceRoot, depended))]);
    return {
        suites,
        packages,
        linked,
        loaded: depended,
        triggers: Array.difference(forceRerunTriggers, defaults.configDefaults.forceRerunTriggers).map((glob) => (isAbsolute(glob) ? local(glob) : glob)),
        shared: Array.dedupe([attachmentsDir, ...reports].map((at) => local(resolve(root, at)))),
        sources: Array.dedupe([...depended, 'package.json', 'pnpm-workspace.yaml', 'lerna.json', ...probes.flatMap((probe) => probe.manifests)]),
    };
});

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, flags: readonly string[], lock: string) {
    const [folder, entry] = [join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const plain = yield* Effect.when(
        Effect.flatMap(loaded<typeof Vite>(file, entry, 'vite'), (vite) =>
            Effect.tryPromise({ try: () => retryOnRequireEsmRace(() => vite.resolveConfig({ configFile: entry, root: folder, logLevel: 'silent' }, 'serve', 'test')), catch: (cause) => new Cause.UnknownError(cause, `${file} does not load for vite serve`) }),
        ),
        Effect.succeed(!matchesGlob(basename(file), SUITE)),
    );
    const report = yield* Option.match(
        Option.filter(plain, (resolved) => !Object.hasOwn(resolved, 'test')),
        {
            onNone: () => surveyed(workspaceRoot, file, flags),
            onSome: ({ configFileDependencies }): Effect.Effect<Effect.Success<ReturnType<typeof surveyed>>> =>
                Effect.succeed({ suites: [], packages: [], linked: [], loaded: [], triggers: [], shared: [], sources: [file, ...configFileDependencies.map((at) => normalizePath(relative(workspaceRoot, at))).filter(within)] }),
        },
    );
    const sources = [lock, ...report.sources];
    return { ...report, sources, stamp: yield* stamped(workspaceRoot)(sources) };
});

const targets = (file: string, { loaded: configs, packages, linked, triggers, shared, suites }: Resolved, { root, projects, setup, coverage, written }: Resolved['suites'][number], chain: readonly string[], upstream: string, { outputPath, coveragePath }: typeof Options.Type, exec: string): Verbs => {
    const [home, held] = [dirname(file), Array.fromNullishOr(outputPath)];
    const rooted = getRootTsConfigFileName();
    const outside = Record.filter(
        Record.fromEntries([...chain.map((at): readonly [string, typeof Input.Type] => [at, { json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] }]), ...[...configs, ...setup].map((at): readonly [string, typeof Input.Type] => [at, posix.join('{workspaceRoot}', at)])]),
        (_input, at) => root !== '.' && !at.startsWith(`${root}/`) && at !== rooted,
    );
    const test = {
        vitest: {
            executor: `${PLUGIN}:test`,
            options: { config: file, projects, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(outputPath), (at) => ({ attachmentsDir: posix.join(at, ATTACHMENTS) }))) },
            dependsOn: ['codegen--*', '^codegen--*'],
            cache: true,
            inputs: [
                'default',
                upstream,
                ...Array.match(linked, {
                    onEmpty: () => [],
                    onNonEmpty: (owners) => [{ input: 'default', projects: [...owners] }],
                }),
                ...Object.values(outside),
                ...triggers.map((at) => posix.join('{workspaceRoot}', at)),
                { externalDependencies: Array.dedupe(['vitest', ...packages]) },
                { env: 'CI' },
                { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true },
            ],
            outputs: [...Array.dedupe([...held, ...written, ...(suites.length === 1 ? shared : [])]).filter((at, _index, all) => !all.some((folder) => at.startsWith(`${folder}/`))), posix.join(coveragePath ?? coverage, 'lcov.info')].map(tokened(root)),
            configurations: {
                coverage: { coverage: { enabled: true, reporter: 'lcovonly', ...(coveragePath === undefined ? {} : { reportsDirectory: coveragePath }) } },
            },
            metadata: { description: 'Runs every Vitest project of the suite with vitest run, in one run with every other suite of the configuration the same nx run tests', help: { command: `${exec} vitest run --help`, example: { args: ['--reporter', 'verbose'] } } },
        },
    } satisfies Record<string, TargetConfiguration>;
    const removed = Record.collect(test, (tool) => `test--${tool}`);
    return {
        test,
        clean: {
            vitest: {
                executor: `${distribution.name}:clean`,
                options: { targets: removed, commands: [['vitest', '--clearCache']], cwd: home },
                inputs: ['default', '^default', { externalDependencies: ['vitest'] }],
                metadata: { description: `Removes the outputs ${removed.join(', ')} declare and every Vitest cache with vitest --clearCache`, help: { command: `${exec} vitest --help`, example: { args: ['--clearCache'] } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved & { readonly digest: string }>, context: CreateNodesContext, settings: typeof Options.Type, hash: string, file: string) {
    const digest = hashArray([hash, yield* OWN]);
    const manager = detectPackageManager(context.workspaceRoot);
    const flags = Array.fromNullishOr(settings.outputPath).flatMap((at) => directed(posix.relative(dirname(file), at)));
    const held = Option.filter(yield* recalled(cache, file, stamped(context.workspaceRoot)), (cached) => cached.digest === digest);
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => read(context.workspaceRoot, file, flags, getLockFileName(manager)) });
    cache.set(file, { ...resolved, digest });
    const projects = yield* Effect.forEach(
        resolved.suites,
        Effect.fnUntraced(function* (suite) {
            const stated = yield* manifested(context.workspaceRoot, suite.root);
            const setup = suite.setup.filter((at) => existsSync(join(context.workspaceRoot, at)));
            const chain = [suite.root, ...setup.map(dirname)].flatMap((at) => extended(context.workspaceRoot, at));
            const project: ProjectConfiguration = {
                root: suite.root,
                ...(suite.providers.length > 0 && { tags: ['host:browser'] }),
                metadata: { technologies: ['vitest', ...suite.providers] },
                targets: aggregated(targets(file, resolved, { ...suite, setup }, chain, `^${namedInput(suite.root, context)}`, settings, getPackageManagerCommand(manager).exec), stated, 'vitest'),
            };
            return [suite.root, project] as const;
        }),
    );
    return { projects: Record.fromEntries(projects) };
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const settings = yield* Schema.decodeEffect(Options)(options ?? {});
    const configs = files.filter((file) => matchesGlob(basename(file), SUITE) || !files.some((other) => dirname(other) === dirname(file) && matchesGlob(basename(other), SUITE)));
    const lock = getLockFileName(detectPackageManager(context.workspaceRoot));
    const cache = new PluginCache<Resolved & { readonly digest: string }>(join(workspaceDataDirectory, 'vitest.hash'));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            configs.map(dirname),
            settings,
            context,
            configs.map(() => [lock]),
        ),
    );
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file, _options, _context, index) => run(inferred(cache, context, settings, `${hashes[index]}`, file)), configs, options, context)),
        Effect.sync(() => cache.writeToDisk()),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [`**/{vite,vitest}.config.${EXTENSIONS}`, (files, options, context) => run(Effect.andThen(testMode, nodes(files, options, context)))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, testMode };
