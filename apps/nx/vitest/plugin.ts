import { existsSync } from 'node:fs';
import { createRequire, findPackageJSON } from 'node:module';
import { basename, dirname, isAbsolute, join, matchesGlob, relative, resolve, sep } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, getPackageManagerCommand, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, getNamedInputs, hashFile, hashObject, hashWithWorkspaceContext, mergeTargetConfigurations, type PackageJson, PluginCache, quoteShellArg, retryOnRequireEsmRace, workspaceDataDirectory } from '@nx/devkit/internal';
import { getRootTsConfigFileName } from '@nx/js';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, Equal, FileSystem, identity, Option, Predicate, pipe, Record, Result, Schema } from 'effect';
import type { configDefaults } from 'vitest/config';
import type { parseCLI, resolveConfig, Vite } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;
type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const REPORTS: Record.ReadonlyRecord<string, string> = { html: '.vitest', blob: '.vitest/blob', json: '.vitest/json', junit: '.vitest/junit' };
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const OWN = pipe(
    Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(import.meta.dirname)),
    Effect.map((names) => hashArray(names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(import.meta.dirname, name)))]))),
    Effect.cached,
    Effect.runSync,
);

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), coveragePath: Schema.optionalKey(Schema.String) }).annotate({
    description:
        'outputPath: workspace folder a suite writes its reports to, {projectName} naming the project, absent leaves each reporter its own folder. coveragePath: workspace folder the coverage configuration writes lcov.info to, {projectName} naming the project, absent leaves Vitest its coverage.reportsDirectory',
});
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), stdout: Schema.optionalKey(Schema.Boolean) }).annotate({
    description: 'The file, folder, and stdout options of a resolved [reporter, options] Vitest entry, stdout true writing no file',
});
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

const directed = (to: string) => ['--attachmentsDir', join(to, 'attachments'), '--outputFile.junit', join(to, 'junit.xml')];

const stamped = (workspaceRoot: string, sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const kept = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const loaded = <A>(file: string, entry: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(entry).resolve(specifier)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* (workspaceRoot: string, file: string, flags: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const [folder, entry] = [join(workspaceRoot, dirname(file)), join(workspaceRoot, file)];
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const [runner, defaults, vite] = yield* Effect.all([loaded<{ readonly parseCLI: typeof parseCLI; readonly resolveConfig: typeof resolveConfig }>(file, entry, 'vitest/node'), loaded<{ readonly configDefaults: typeof configDefaults }>(file, entry, 'vitest/config'), loaded<typeof Vite>(file, entry, 'vite')]);
    const config = yield* Effect.tryPromise({
        try: () => retryOnRequireEsmRace(() => runner.resolveConfig({ ...runner.parseCLI(['vitest', 'run', ...flags]).options, config: entry, root: folder, watch: false }, { logLevel: 'silent' })),
        catch: (cause) => new Cause.UnknownError(cause, `${file} does not load as a Vitest config`),
    });
    const { root, coverage, attachmentsDir, outputFile, reporters, forceRerunTriggers, resolvedProjects } = config.test;
    const declared = resolvedProjects.filter(({ inline, viteConfig }) => inline === true || viteConfig.configFile === entry);
    const parents = new Map(declared.filter(({ hidden }) => hidden === true).map(({ viteConfig, projectConfig }) => [viteConfig, projectConfig.name] as const));
    const probes = yield* Effect.forEach(
        declared.filter(({ hidden }) => hidden !== true),
        Effect.fnUntraced(function* ({ viteConfig, projectConfig: { name, root: base, setupFiles, globalSetup, browser } }) {
            const manifests = above(local(base) || '.').map((at) => join(at, 'package.json'));
            const manifest = yield* Effect.mapError(
                Effect.flatMap(
                    Effect.findFirst(manifests, (at) => fs.exists(join(workspaceRoot, at))),
                    Effect.fromOption,
                ),
                (cause) => new Cause.UnknownError(cause, `${file} declares a project under ${local(base) || '.'}, which no package holds`),
            );
            const owner = dirname(manifest);
            const setup = [...setupFiles, ...Array.ensure(globalSetup)].map(local).filter((at) => kept(at) && !at.startsWith(`${owner}/`));
            return {
                owner,
                project: parents.get(viteConfig) ?? name,
                manifests,
                setup,
                tsconfigs: setup.flatMap((at) => extended(workspaceRoot, dirname(at))),
                browser: browser.enabled,
                written: browser.enabled ? Array.getSomes([Option.fromUndefinedOr(browser.trace.tracesDir), Option.fromNullishOr(browser.screenshotDirectory)]) : [],
            };
        }),
    );
    const selectable = Array.dedupe(probes.map(({ project }) => project));
    const suites = Record.collect(
        Array.groupBy(probes, ({ owner }) => owner),
        (owner, group) => {
            const selected = Array.dedupe(group.map(({ project }) => project));
            return {
                root: owner,
                projects: selected.length === selectable.length ? [] : selected,
                setup: Array.dedupe(group.flatMap(({ setup }) => setup)),
                tsconfigs: Array.dedupe(group.flatMap(({ tsconfigs }) => tsconfigs)),
                parallelism: group.every(({ browser }) => !browser),
                coverage: local(resolve(root, coverage.reportsDirectory)),
            };
        },
    );
    const keyed: Readonly<Record<string, string | undefined>> = Predicate.isObject(outputFile) ? outputFile : {};
    const reports = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const built = Option.liftPredicate(name, Predicate.isString);
        const named = Option.flatMap(built, (key) => Option.fromUndefinedOr(keyed[key]));
        const stated = Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: direct }) => Option.fromUndefinedOr(direct)), Option.liftPredicate(outputFile, Predicate.isString), named]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
        return Array.isReadonlyArrayNonEmpty(stated) || Option.exists(decoded, ({ stdout }) => stdout === true) ? stated : Option.toArray(Option.flatMap(built, (key) => Record.get(REPORTS, key)));
    });
    const depended = Array.dedupe([entry, ...config.configFileDependencies, ...declared.flatMap(({ viteConfig }) => viteConfig.configFileDependencies)].map(local)).filter(kept);
    const texts = yield* Effect.forEach(depended, (at) => fs.readFileString(join(workspaceRoot, at)), { concurrency: 'unbounded' });
    const statements = Array.zipWith(depended, texts, (at, text) => vite.parseSync(at, text).module.staticImports.map(({ moduleRequest, entries }) => ({ specifier: moduleRequest.value, from: join(workspaceRoot, at), entries }))).flat();
    const imported = statements.filter(({ entries }) => entries.length === 0 || entries.some(({ isType }) => !isType)).map(({ specifier, from }) => [specifier, from] as const);
    const provider = coverage.provider === 'custom' ? Array.fromNullishOr(coverage.customProviderModule) : [`@vitest/coverage-${coverage.provider}`];
    const found = Array.getSomes([...provider.map((specifier) => [specifier, entry] as const), ...imported].map(([specifier, from]) => Option.flatMap(Option.liftThrowable(findPackageJSON)(specifier, pathToFileURL(from)), Option.fromUndefinedOr)));
    const manifests = yield* Effect.forEach(
        Array.dedupe(found).filter((at) => at.split(sep).includes('node_modules')),
        (at) => fs.realPath(at),
        { concurrency: 'unbounded' },
    );
    const named = yield* Effect.forEach(Array.dedupe(manifests), (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail(name))), { concurrency: 'unbounded' });
    const [packages, linked] = Array.partition(named, identity);
    return {
        suites,
        packages: Array.dedupe(packages),
        linked: Array.dedupe(linked),
        loaded: depended,
        triggers: Array.difference(forceRerunTriggers, defaults.configDefaults.forceRerunTriggers).map((glob) => (isAbsolute(glob) ? local(glob) : glob)),
        folders: Array.dedupe([attachmentsDir, ...reports, ...probes.flatMap((probe) => probe.written)].map((written) => local(resolve(root, written)))),
        sources: Array.dedupe([...depended, ...probes.flatMap((probe) => [...probe.manifests, ...probe.setup, ...probe.tsconfigs])]),
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
                Effect.succeed({ suites: [], packages: [], linked: [], loaded: [], triggers: [], folders: [], sources: [file, ...configFileDependencies.map((at) => normalizePath(relative(workspaceRoot, at))).filter(kept)] }),
        },
    );
    const sources = [lock, ...report.sources];
    return { ...report, sources, stamp: yield* stamped(workspaceRoot, sources) };
});

const targets = (
    home: string,
    { loaded: configs, packages, linked, triggers, folders }: Resolved,
    { root, projects, setup, tsconfigs, parallelism, coverage }: Resolved['suites'][number],
    chain: readonly string[],
    upstream: string,
    { outputPath, coveragePath }: typeof Options.Type,
    flags: readonly string[],
    exec: string,
): Verbs => {
    const held = Array.fromNullishOr(outputPath);
    const rooted = getRootTsConfigFileName();
    const outside = Record.filter(
        Record.fromEntries([
            ...[...chain, ...tsconfigs].map((at): readonly [string, typeof Input.Type] => [at, { json: join('{workspaceRoot}', at), fields: ['compilerOptions'], ...(at === rooted && { excludeFields: ['compilerOptions.paths'] }) }]),
            ...[...configs, ...setup].map((at): readonly [string, typeof Input.Type] => [at, join('{workspaceRoot}', at)]),
        ]),
        (_input, at) => root !== '.' && !at.startsWith(`${root}/`),
    );
    const test = {
        vitest: {
            command: [['vitest', 'run', ...projects.flatMap((name) => ['--project', name])].map(quoteShellArg).join(' '), ...Array.match(flags, { onEmpty: () => [], onNonEmpty: (given) => ['{args}', given.map(quoteShellArg).join(' ')] })].join(' '),
            options: { cwd: home },
            dependsOn: ['codegen--*', '^codegen'],
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
                ...triggers.map((at) => join('{workspaceRoot}', at)),
                { dependentTasksOutputFiles: '**/*', transitive: true },
                { env: 'CI' },
                { externalDependencies: Array.dedupe(['vitest', ...packages]) },
            ],
            outputs: [...[...held, ...folders.filter((at) => !held.some((folder) => at.startsWith(`${folder}/`)))].map((at) => join('{workspaceRoot}', at)), '{options.outputs.0}'],
            configurations: {
                coverage: { 'coverage.enabled': true, 'coverage.reporter': 'lcovonly', ...(coveragePath === undefined ? {} : { 'coverage.reportsDirectory': relative(home, coveragePath) }), outputs: [join('{workspaceRoot}', coveragePath ?? coverage, 'lcov.info')] },
            },
            metadata: { description: 'Runs every Vitest project of the suite with vitest run, beside no other task where a project runs in a browser, since Chromium fails queued requests when a host address changes', help: { command: `${exec} vitest run --help`, example: { args: ['--project', '<name>'] } } },
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

const inferred = Effect.fnUntraced(function* (cache: PluginCache<Resolved>, context: CreateNodesContext, settings: typeof Options.Type, hash: string, file: string) {
    const home = dirname(file);
    const key = hashArray([hash, yield* OWN, file]);
    const manager = detectPackageManager(context.workspaceRoot);
    const flags = Array.fromNullishOr(settings.outputPath).flatMap((at) => directed(relative(home, at)));
    const held = yield* Effect.findFirst(Option.toArray(Option.fromUndefinedOr(cache.get(key))), ({ sources, stamp }) => Effect.map(stamped(context.workspaceRoot, sources), Equal.equals(stamp)));
    const resolved = yield* Option.match(held, { onSome: Effect.succeed, onNone: () => read(context.workspaceRoot, file, flags, getLockFileName(manager)) });
    cache.set(key, resolved);
    const projects = yield* Effect.forEach(
        resolved.suites,
        Effect.fnUntraced(function* (suite) {
            const stated = yield* manifested(context.workspaceRoot, suite.root);
            const project: ProjectConfiguration = {
                root: suite.root,
                metadata: { technologies: ['vitest'] },
                targets: aggregated(targets(home, resolved, suite, extended(context.workspaceRoot, suite.root), 'production' in getNamedInputs(suite.root, context) ? '^production' : '^default', settings, flags, getPackageManagerCommand(manager).exec), stated, 'vitest'),
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
    const cache = new PluginCache<Resolved>(join(workspaceDataDirectory, `vitest-${hashObject(options ?? {})}.hash`));
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

const createNodes: CreateNodes<typeof Options.Encoded> = [
    `**/{vite,vitest}.config.${EXTENSIONS}`,
    (files, options, context) =>
        run(
            Effect.acquireUseRelease(
                Effect.sync(() => {
                    const before = { ...env };
                    const { NODE_ENV = MODE.NODE_ENV } = before;
                    Object.assign(env, { ...MODE, NODE_ENV });
                    return before;
                }),
                () => nodes(files, options, context),
                (before) =>
                    Effect.sync(() => {
                        Record.keys(MODE).forEach((name) => {
                            Reflect.deleteProperty(env, name);
                        });
                        Object.assign(env, before);
                    }),
            ),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
