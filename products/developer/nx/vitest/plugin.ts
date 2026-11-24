import { once } from 'node:events';
import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, matchesGlob, posix, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, detectPackageManager, hashArray, normalizePath, type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, findProjectForPath, hashObject, type PackageJson, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { getLockFileName, getRootTsConfigFileName } from '@nx/js';
import { above, aggregated, EXEC, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, parsed, recalled, run, stamped, tokened, type Verbs, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, FileSystem, flow, MutableRef, Option, Record, Schema } from 'effect';
import type { Vite } from 'vitest/node';

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = `vitest.config.${EXTENSIONS}`;
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const ATTACHMENTS = 'attachments';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({ outputPath: Folder, coveragePath: Folder }).annotate({
        description:
            'outputPath: workspace folder a suite writes its reports to, {projectName} naming the project, absent leaves each reporter its own folder. coveragePath: workspace folder the coverage configuration writes lcov.info to, {projectName} naming the project, absent leaves Vitest its coverage.reportsDirectory',
    }),
) {}
const Surveyed = Schema.TaggedUnion({
    Plain: { dependencies: Schema.Array(Schema.String) },
    Suite: {
        root: Schema.String,
        reportsDirectory: Schema.String,
        providers: Schema.Array(Schema.String),
        attachmentsDir: Schema.String,
        reports: Schema.Array(Schema.String),
        triggers: Schema.Array(Schema.String),
        listed: Schema.Boolean,
        dependencies: Schema.Array(Schema.String),
        projects: Schema.Array(Schema.Struct({ project: Schema.String, base: Schema.String, setup: Schema.Array(Schema.String), written: Schema.Array(Schema.String), providers: Schema.Array(Schema.String) })),
    },
}).annotate({
    description:
        'What the worker read from a Vite or Vitest config under the TEST, VITEST, and NODE_ENV values vitest sets: the files a Vite config stating no test block loaded, or the resolved Vitest root, coverage folder and providers, attachments folder, reporter outputs, added rerun triggers, whether it lists projects, the files its load read, and each project it declares with its root, setup files, browser output folders, and browser providers',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = <A>(file: string, entry: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(entry).resolve(specifier)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* (workspaceRoot: string, file: string, { root, reportsDirectory, providers, attachmentsDir, reports, triggers, listed, dependencies, projects }: Extract<typeof Surveyed.Type, { readonly _tag: 'Suite' }>) {
    const fs = yield* FileSystem.FileSystem;
    const entry = join(workspaceRoot, file);
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const workspaces = buildPackageJsonPatterns(workspaceRoot, (at) => readJsonFile<PackageJson>(join(workspaceRoot, at)));
    const member = buildPackageJsonWorkspacesMatcher(workspaces);
    const probes = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ project, base, setup, written, providers: hosted }) {
            const manifests = (listed ? above(posix.join(local(base), '.')) : [dirname(file)]).flatMap((at) => [posix.join(at, 'package.json'), posix.join(at, 'project.json')]);
            const manifest = yield* Effect.findFirst(
                manifests.filter((at) => basename(at) === 'project.json' || member(at)),
                (at) => fs.exists(join(workspaceRoot, at)),
            );
            yield* Effect.when(Effect.fail(new Cause.UnknownError(base, `${file} declares a project under ${local(base)}, which no project root holds`)), Effect.succeed(listed && Option.isNone(manifest)));
            return Option.map(manifest, (at) => ({ owner: dirname(at), project, manifests, setup: setup.map(local).filter(within), written, providers: hosted }));
        }),
    ).pipe(Effect.map(Array.getSomes));
    const selectable = Array.dedupe(probes.map(({ project }) => project));
    const depended = Array.dedupe(dependencies.map(local)).filter(within);
    const vite = yield* loaded<typeof Vite>(file, entry, 'vite');
    const imports = yield* parsed(vite.parseSync, workspaceRoot, depended);
    const grouped = Array.groupBy(probes, ({ owner }) => owner);
    const homes = new Map(Record.keys(grouped).flatMap((owner) => (owner === '.' ? [] : [[owner, owner] as const])));
    const suites = yield* Effect.forEach(
        Record.toEntries(grouped),
        Effect.fnUntraced(function* ([owner, group]) {
            const selected = Array.dedupe(group.map(({ project }) => project));
            const held = new Set(depended.filter((at) => (findProjectForPath(at, homes) ?? owner) === owner));
            const { packages } = yield* packaged([...providers.map((specifier) => [specifier, entry] as const), ...imports.filter(([, from]) => held.has(local(from)))]);
            return {
                root: owner,
                projects: selected.length === selectable.length ? [] : selected,
                setup: Array.dedupe(group.flatMap(({ setup }) => setup)).filter((at) => !at.startsWith(`${owner}/`)),
                providers: Array.dedupe(group.flatMap(({ providers: hosted }) => hosted)),
                coverage: local(resolve(root, reportsDirectory)),
                written: Array.dedupe(group.flatMap(({ written }) => written).map((at) => local(resolve(root, at)))),
                loaded: [...held],
                packages,
            };
        }),
    );
    return {
        suites,
        triggers: triggers.map((glob) => (isAbsolute(glob) ? local(glob) : glob)),
        shared: Array.dedupe([attachmentsDir, ...reports].map((at) => local(resolve(root, at)))),
        sources: Array.dedupe([...depended, 'package.json', 'pnpm-workspace.yaml', 'lerna.json', ...(listed ? workspaces.positive : []), ...probes.flatMap((probe) => probe.manifests)]),
    };
});

const read = Effect.fnUntraced(function* (workspaceRoot: string, file: string, outputPath: string | undefined, stamp: (sources: readonly string[]) => Effect.Effect<string>) {
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${file} does not load as a Vitest config`);
    const config = yield* Effect.flatMap(
        Effect.acquireUseRelease(
            Effect.sync(
                () =>
                    new Worker(WORKER, {
                        workerData: { file, entry: join(workspaceRoot, file), name: PLUGIN, suite: matchesGlob(basename(file), SUITE), ...(outputPath !== undefined && { attachmentsDir: join(workspaceRoot, outputPath, ATTACHMENTS) }) },
                        env: { ...env, ...MODE, NODE_ENV: env['NODE_ENV'] ?? MODE.NODE_ENV },
                    }),
            ),
            (worker) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), flow(failed, Effect.fail))),
            (worker) => Effect.promise(() => worker.terminate()),
        ),
        ([message]) => Schema.decodeUnknownEffect(Surveyed)(message),
    );
    const report = yield* Surveyed.match(config, {
        Plain: ({ dependencies }): ReturnType<typeof surveyed> => Effect.succeed({ suites: [], triggers: [], shared: [], sources: dependencies.map((at) => normalizePath(relative(workspaceRoot, at))).filter(within) }),
        Suite: (suite) => surveyed(workspaceRoot, file, suite),
    });
    return { ...report, stamp: yield* stamp(report.sources) };
});

const targets = (file: string, { triggers, shared, suites }: Resolved, { root, projects, setup, coverage, written, loaded: configs, packages }: Resolved['suites'][number], chain: readonly string[], upstream: string, { outputPath, coveragePath }: Options, exec: string): Verbs => {
    const rooted = getRootTsConfigFileName();
    const outside = Record.filter(
        { ...Record.fromIterableWith(chain, (at) => [at, { json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] }]), ...Record.fromIterableWith([...configs, ...setup], (at) => [at, posix.join('{workspaceRoot}', at)]) },
        (_input, at) => root !== '.' && !at.startsWith(`${root}/`) && at !== rooted,
    );
    const test = {
        vitest: {
            executor: `${PLUGIN}:test`,
            options: { config: file, projects, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(outputPath), (at) => ({ attachmentsDir: posix.join(at, ATTACHMENTS) }))) },
            dependsOn: ['codegen--*', '^codegen--*'],
            cache: true,
            inputs: ['default', upstream, ...Object.values(outside), ...triggers.map((at) => posix.join('{workspaceRoot}', at)), { externalDependencies: Array.dedupe(['vitest', ...packages]) }, { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true }],
            outputs: [...Array.dedupe([...Array.fromNullishOr(outputPath), ...written, ...(suites.length === 1 ? shared : [])]).filter((at, _index, all) => !all.some((folder) => at.startsWith(`${folder}/`))), posix.join(coveragePath ?? coverage, 'lcov.info')].map(tokened(root)),
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
                options: { targets: removed, commands: [['vitest', '--clearCache']], cwd: dirname(file) },
                inputs: ['default', '^default', { externalDependencies: ['vitest'] }],
                metadata: { description: `Removes the outputs ${removed.join(', ')} declare and every Vitest cache with vitest --clearCache`, help: { command: `${exec} vitest --help`, example: { args: ['--clearCache'] } } },
            },
        },
    };
};

const inferred = Effect.fnUntraced(function* (pass: { readonly cache: PluginCache<Resolved>; readonly stored: MutableRef.MutableRef<boolean>; readonly stamp: Parameters<typeof read>[3]; readonly exec: string }, context: CreateNodesContext, settings: Options, file: string) {
    const resolved = yield* Option.match(yield* recalled(pass.cache, file, pass.stamp), {
        onSome: Effect.succeed,
        onNone: () =>
            Effect.tap(read(context.workspaceRoot, file, settings.outputPath, pass.stamp), (fresh) =>
                Effect.sync(() => {
                    pass.cache.set(file, fresh);
                    MutableRef.set(pass.stored, true);
                }),
            ),
    });
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
                targets: aggregated(targets(file, resolved, { ...suite, setup }, chain, `^${namedInput(suite.root, context)}`, settings, pass.exec), stated, 'vitest'),
            };
            return [suite.root, project] as const;
        }),
    );
    return { projects: Record.fromEntries(projects) };
});

const nodes = inferring(Options, run, function* (files, settings, context) {
    const configs = files.filter((file) => matchesGlob(basename(file), SUITE) || !files.some((other) => dirname(other) === dirname(file) && matchesGlob(basename(other), SUITE)));
    const manager = detectPackageManager(context.workspaceRoot);
    const [lock, digest] = [getLockFileName(manager), hashArray([hashObject(settings), yield* OWN])];
    const pass = {
        cache: new PluginCache<Resolved>(join(workspaceDataDirectory, 'vitest.hash')),
        stored: MutableRef.make(false),
        stamp: (sources: readonly string[]) => Effect.map(stamped(context.workspaceRoot)([lock, ...sources]), (held) => hashArray([held, digest])),
        exec: EXEC[manager],
    };
    return yield* Effect.ensuring(
        Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, context, settings, file)), configs, settings, context)),
        Effect.when(
            Effect.sync(() => pass.cache.writeToDisk()),
            Effect.sync(() => MutableRef.get(pass.stored)),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/{vite,vitest}.config.${EXTENSIONS}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, loaded, MODE };
