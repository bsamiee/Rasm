import { existsSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, isAbsolute, join, posix, relative, resolve } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Worker } from 'node:worker_threads';
import { type CreateNodes, type CreateNodesContext, createNodesFromFiles, hashArray, normalizePath, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import { buildPackageJsonPatterns, findProjectForPath, globWithWorkspaceContext, hashObject, type PackageJson } from '@nx/devkit/internal';
import { above, type Anchor, aggregated, anchored, below, EXECUTORS, extended, fingerprinted, inferring, manifested, namedInput, owned, packaged, parsed, type Pass, request, run, tokened, within } from '@rasm/nx/inference';
import { Array, Cause, Effect, Function, Glob, Option, Predicate, Record, Schema } from 'effect';
import type { Vite } from 'vitest/node';
import plugin from '#/package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Resolved = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENSIONS = '{js,mjs,ts,cjs,mts,cts}';
const SUITE = Glob.make(`vitest.config.${EXTENSIONS}`);
const MODE = { TEST: 'true', VITEST: 'true', NODE_ENV: 'test' } as const;
const ATTACHMENTS = 'attachments';
const WORKER = fileURLToPath(import.meta.resolve('#/worker'));
const OWN = fingerprinted(import.meta.dirname);

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options>()(Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder a suite writes its reports to, {projectName} naming the project, absent leaves each reporter its own folder' })) {}
const Surveyed = Schema.TaggedUnion({
    Plain: { dependencies: Schema.Array(Schema.String) },
    Suite: {
        root: Schema.String,
        attachmentsDir: Schema.String,
        reports: Schema.Array(Schema.String),
        triggers: Schema.Array(Schema.String),
        listed: Schema.Boolean,
        dependencies: Schema.Array(Schema.String),
        projects: Schema.Array(Schema.Struct({ project: Schema.String, base: Schema.String, setup: Schema.Array(Schema.String), written: Schema.Array(Schema.String), providers: Schema.Array(Schema.String) })),
    },
}).annotate({
    description:
        'What the worker read from a Vite or Vitest config under the TEST, VITEST, and NODE_ENV values vitest sets: the files a Vite config stating no test block loaded, or the resolved Vitest root, attachments folder, reporter outputs, added rerun triggers, whether it lists projects, the files its load read, and each project it declares with its root, setup files, browser output folders, and browser providers',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const loaded = <A>(file: string, entry: string, specifier: string) => Effect.tryPromise({ try: (): Promise<A> => import(pathToFileURL(createRequire(entry).resolve(specifier)).href), catch: (cause) => new Cause.UnknownError(cause, `${file} resolves no ${specifier}`) });

const surveyed = Effect.fnUntraced(function* ({ workspaceRoot, holds }: Anchor, file: string, { root, attachmentsDir, reports, triggers, listed, dependencies, projects }: typeof Surveyed.cases.Suite.Type) {
    const entry = join(workspaceRoot, file);
    const local = (at: string) => normalizePath(relative(workspaceRoot, at));
    const workspaces = buildPackageJsonPatterns(workspaceRoot, (at) => readJsonFile<PackageJson>(join(workspaceRoot, at)));
    const probes = yield* Effect.forEach(
        projects,
        Effect.fnUntraced(function* ({ project, base, setup, written, providers }) {
            const home = listed ? posix.join(local(base), '.') : dirname(file);
            const folders = listed ? above(home) : [home];
            const owner = Array.findFirst(folders, holds);
            yield* Effect.when(new Cause.UnknownError(base, `${file} declares a project under ${local(base)}, which no project root holds`), Effect.succeed(listed && Option.isNone(owner)));
            return Option.map(owner, (at) => ({ owner: at, home, project, folders, setup: setup.map(local).filter(within), written, providers }));
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
            const { packages } = yield* packaged(imports.filter(([, from]) => held.has(local(from))));
            return {
                root: owner,
                projects: selected.length === selectable.length ? [] : selected,
                setup: Array.dedupe(group.flatMap(({ setup }) => setup)).filter(Predicate.not(owned(owner))),
                providers: Array.dedupe(group.flatMap(({ providers }) => providers)),
                written: Array.dedupe(group.flatMap(({ written }) => written).map((at) => local(resolve(root, at)))),
                loaded: [...held, ...Array.dedupe(group.map(({ home }) => home)).flatMap((at) => (at === owner ? [] : [posix.join(at, '**/*')]))],
                packages,
            };
        }),
    );
    return {
        suites,
        triggers: triggers.map((glob) => posix.join('{workspaceRoot}', isAbsolute(glob) ? local(glob) : glob)),
        shared: Array.dedupe([attachmentsDir, ...reports].map((at) => local(resolve(root, at)))),
        sources: Array.dedupe([...depended, 'package.json', 'pnpm-workspace.yaml', 'lerna.json', ...(listed ? workspaces.positive : []), ...[dirname(file), ...probes.flatMap(({ folders }) => folders)].flatMap((at) => [posix.join(at, 'package.json'), posix.join(at, 'project.json')])]),
    };
});

const read = (anchor: Anchor, file: string, outputPath: string | undefined) => {
    const workerData = { file, entry: join(anchor.workspaceRoot, file), name: plugin.name, suite: Glob.match(SUITE, basename(file)), ...(outputPath !== undefined && { attachmentsDir: join(anchor.workspaceRoot, outputPath, ATTACHMENTS) }) };
    return request(
        Effect.sync(() => new Worker(WORKER, { workerData, env: { ...env, ...MODE, NODE_ENV: env['NODE_ENV'] ?? MODE.NODE_ENV } })),
        Surveyed,
        `${file} does not load as a Vitest config`,
    ).pipe(
        Effect.flatMap(
            Surveyed.match({
                Plain: ({ dependencies }): ReturnType<typeof surveyed> => Effect.succeed({ suites: [], triggers: [], shared: [], sources: dependencies.map((at) => normalizePath(relative(anchor.workspaceRoot, at))).filter(within) }),
                Suite: (suite) => surveyed(anchor, file, suite),
            }),
        ),
    );
};

const inferred = Effect.fnUntraced(function* (pass: Pass<Resolved>, anchor: Anchor, context: CreateNodesContext, { outputPath }: Options, file: string) {
    const { suites, triggers, shared } = yield* pass.recall(file, read(anchor, file, outputPath));
    const manifests = yield* Effect.forEach(suites, ({ root }) => manifested(context.workspaceRoot, root));
    const projects = Array.zipWith(suites, manifests, ({ root, projects: selected, setup: declared, providers, written, loaded: configs, packages }, stated) => {
        const setup = declared.filter((at) => existsSync(join(context.workspaceRoot, at)));
        const chain = [root, ...setup.map(dirname)].flatMap((at) => extended(context.workspaceRoot, at));
        const outside = Record.filter({ ...Record.fromIterableWith(chain, (at) => [at, { json: posix.join('{workspaceRoot}', at), fields: ['compilerOptions'] }]), ...Record.fromIterableWith([...configs, ...setup], (at) => [at, posix.join('{workspaceRoot}', at)]) }, (_input, at) => !owned(root)(at));
        const test = {
            vitest: {
                executor: `${plugin.name}:${providers.length > 0 ? 'browser' : 'test'}`,
                options: { config: file, projects: selected, ...(outputPath !== undefined && { attachmentsDir: posix.join(outputPath, ATTACHMENTS) }) },
                dependsOn: ['codegen--*', '^codegen--*'],
                cache: true,
                inputs: ['default', `^${namedInput(root, context)}`, ...Object.values(outside), ...triggers, { externalDependencies: Array.dedupe(['vitest', ...packages]) }, { dependentTasksOutputFiles: '**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}', transitive: true }],
                outputs: Array.dedupe([...Array.fromNullishOr(outputPath), ...written, ...(suites.length === 1 ? shared : [])])
                    .filter((at, _index, all) => !all.some(Function.flip(below)(at)))
                    .map(tokened(root)),
                metadata: {
                    description: 'Runs every Vitest project of the suite with vitest run, a suite without a browser provider in one run with every other such suite of the configuration the same nx run tests, as a batch holds every other Nx task until it ends',
                    help: { command: `${pass.exec} vitest run --help`, example: { args: ['--reporter', 'verbose'] } },
                },
            },
        } satisfies Record<string, TargetConfiguration>;
        const removed = Record.collect(test, (tool) => `test--${tool}`);
        const clean = {
            vitest: {
                executor: EXECUTORS.clean,
                options: { targets: removed, commands: [['vitest', '--clearCache']], cwd: dirname(file) },
                inputs: ['default', '^default', { externalDependencies: ['vitest'] }],
                metadata: { description: `Removes the outputs ${removed.join(', ')} declare and every Vitest cache with vitest --clearCache`, help: { command: `${pass.exec} vitest --help`, example: { args: ['--clearCache'] } } },
            },
        };
        return { root, ...(providers.length > 0 && { tags: ['host:browser'] }), metadata: { technologies: ['vitest', ...providers] }, targets: aggregated({ test, clean }, stated, 'vitest', anchor) };
    });
    return { projects: Record.fromIterableBy(projects, ({ root }) => root) };
});

const nodes = inferring(Options, run, function* (matched, settings, context, cached) {
    const files = Array.dedupe([...matched, ...(yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [`**/${SUITE.pattern}`])))]);
    const configs = files.filter((file) => Glob.match(SUITE, basename(file)) || !files.some((other) => dirname(other) === dirname(file) && Glob.match(SUITE, basename(other))));
    const [pass, anchor] = [yield* cached<Resolved>('vitest.hash', hashArray([hashObject(settings), yield* OWN])), anchored(context.workspaceRoot)];
    return yield* Effect.promise(() => createNodesFromFiles((file) => run(inferred(pass, anchor, context, settings, file)), configs, settings, context));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/vite.config.${EXTENSIONS}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes, loaded, MODE };
