import { createRequire, findPackageJSON, registerHooks } from 'node:module';
import { matchesGlob } from 'node:path';
import { env } from 'node:process';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, detectPackageManager, hashArray, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Config, Effect, FileSystem, Option, Path, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Markers = Partial<Record<keyof typeof MARKERS, string>>;
type Read = Effect.Success<ReturnType<typeof read>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARKERS = {
    vite: 'vite.config.{js,mjs,ts,cjs,mts,cts}',
    vitest: 'vitest.config.{js,mjs,ts,cjs,mts,cts}',
    storybook: '.storybook/main.{js,ts,jsx,tsx,mjs,mts,cjs,cts}',
    cloudflare: 'cloudflare.config.ts',
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'Plugin options from the nx.json row' });
const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'The file and folder options of a resolved [reporter, options] Vitest entry' });
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const outermost = (folders: readonly string[]) => Array.dedupe(folders).filter((at, _, all) => !all.some((other) => at.startsWith(`${other}/`)));

const placed = (path: Path.Path, root: string, outputPath: Option.Option<string>, tool: string) => Option.map(outputPath, (at) => path.relative(root, path.join(at, tool, '{projectName}')));

const loaded = <A>(folder: string, specifier: string) => Effect.tryPromise((): Promise<A> => import(pathToFileURL(createRequire(`${folder}/package.json`).resolve(specifier)).href));

const isolated = <A, E, R>(launch: Record.ReadonlyRecord<string, string>, effect: Effect.Effect<A, E, R>) =>
    Effect.acquireUseRelease(
        Effect.sync(() => ({ ...env })),
        () =>
            Effect.andThen(
                Effect.sync(() => Object.assign(env, launch)),
                effect,
            ),
        (before) =>
            Effect.sync(() => {
                Object.keys(env)
                    .filter((key) => !(key in before))
                    .forEach((key) => Reflect.deleteProperty(env, key));
                Object.assign(env, before);
            }),
    );

const bundled = Effect.fnUntraced(function* (folder: string, file: string, outDir: Option.Option<string>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { resolveConfig } = yield* loaded<typeof import('vite')>(folder, 'vite');
    const config = yield* Effect.tryPromise(() => resolveConfig({ configFile: file, root: folder, logLevel: 'silent', build: Record.getSomes({ outDir }) }, 'build', 'production', 'production'));
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* fs.exists(path.join(config.root, 'index.html')));
    return {
        folders: Option.liftPredicate(
            builds.map((build) => path.resolve(config.root, build.outDir)),
            () => buildable,
        ),
        declared: Predicate.hasProperty(config, 'test'),
    };
});

const collected = Effect.fnUntraced(function* (folder: string, file: string) {
    const path = yield* Path.Path;
    const { createVitest } = yield* loaded<typeof import('vitest/node')>(folder, 'vitest/node');
    const opened = yield* Effect.acquireRelease(
        Effect.tryPromise(() => createVitest('test', { config: file, root: folder, watch: false, run: true }, { logLevel: 'silent' })),
        (instance) => Effect.promise(() => instance.close()),
    );
    const specifications = yield* Effect.tryPromise(() => opened.globTestSpecifications());
    const { root, coverage, attachmentsDir, outputFile, reporters } = opened.config;
    const stated = Option.liftPredicate(Predicate.isString);
    const keyed: Record.ReadonlyRecord<string, string | undefined> = Predicate.isString(outputFile) ? {} : (outputFile ?? {});
    const reported = reporters.flatMap((reporter) => {
        const [name, options] = Array.isArray(reporter) ? reporter : [reporter, {}];
        const decoded = Schema.decodeUnknownOption(Reported)(options);
        const named = Option.flatMap(stated(name), (key) => Option.fromUndefinedOr(keyed[key]));
        return Array.getSomes([Option.firstSomeOf([Option.flatMap(decoded, ({ outputFile: own }) => Option.fromUndefinedOr(own)), stated(outputFile), named]), Option.flatMap(decoded, ({ outputDir }) => Option.fromUndefinedOr(outputDir))]);
    });
    const traced = opened.projects.flatMap(({ config }) => (config.browser.enabled ? Option.toArray(Option.fromUndefinedOr(config.browser.trace.tracesDir)) : []));
    return Option.liftPredicate(
        [coverage.reportsDirectory, attachmentsDir, ...reported, ...traced].map((at) => path.resolve(root, at)),
        () => specifications.length > 0,
    );
}, Effect.scoped);

const read = Effect.fnUntraced(function* (workspaceRoot: string, root: string, markers: Markers, outputPath: Option.Option<string>) {
    const path = yield* Path.Path;
    const folder = path.join(workspaceRoot, root);
    const located = (file: string | undefined) => Option.map(Option.fromUndefinedOr(file), (at) => path.join(workspaceRoot, at));
    const vite = yield* Effect.transposeOption(Option.map(located(markers.vite), (file) => isolated({}, bundled(folder, file, placed(path, root, outputPath, 'vite')))));
    const environment = yield* Config.String('NODE_ENV').pipe(Config.withDefault('test'));
    const declared = Option.exists(vite, (config) => config.declared);
    const vitest = Option.orElse(located(markers.vitest), () => Option.filter(located(markers.vite), () => declared));
    const tested = yield* Effect.transposeOption(Option.map(vitest, (file) => isolated({ TEST: 'true', VITEST: 'true', NODE_ENV: environment }, collected(folder, file))));
    const relative = (folders: readonly string[]) => outermost(folders.map((at) => path.relative(workspaceRoot, at)));
    return {
        bundles: pipe(
            vite,
            Option.flatMap(({ folders }) => folders),
            Option.map(relative),
            Option.getOrUndefined,
        ),
        suite: pipe(tested, Option.flatten, Option.map(relative), Option.getOrUndefined),
    };
});

const chained = (path: Path.Path, workspaceRoot: string, root: string) => {
    const jsonCache = new Map();
    const folders = [root, ...Array.unfold(root, (at) => (at === '.' ? Option.none() : Option.some([path.dirname(at), path.dirname(at)] as const)))];
    const visited: string[] = [];
    folders.forEach((at) =>
        walkTsconfigExtendsChain(
            path.join(workspaceRoot, at, 'tsconfig.json'),
            (file) => {
                visited.push(path.relative(workspaceRoot, file));
                return 'continue';
            },
            { jsonCache },
        ),
    );
    return Array.dedupe(visited).filter((at) => !(at.startsWith('..') || at.startsWith(`${root}/`) || at.split(path.sep).includes('node_modules')));
};

const reached = (path: Path.Path, edges: ReadonlyMap<string | undefined, ReadonlySet<string>>, workspaceRoot: string, root: string) => {
    const inside = `${pathToFileURL(path.join(workspaceRoot, root)).href}/`;
    const walk = (seen: ReadonlySet<string>, frontier: readonly string[]): ReadonlySet<string> =>
        Array.match(Array.dedupe(frontier.flatMap((url) => [...(edges.get(url) ?? [])]).filter((url) => !seen.has(url) && url.startsWith('file:') && !url.includes('/node_modules/'))), { onEmpty: () => seen, onNonEmpty: (next) => walk(new Set([...seen, ...next]), next) });
    return [...walk(new Set(), Array.getSomes([...edges.keys()].map((url) => Option.filter(Option.fromUndefinedOr(url), (at) => at.startsWith(inside)))))].map((url) => path.relative(workspaceRoot, fileURLToPath(url)));
};

const targets = (path: Path.Path, root: string, markers: Markers, entry: Read, chain: readonly string[], outputPath: Option.Option<string>): Record.ReadonlyRecord<string, TargetConfiguration> => {
    const command = (argv: readonly string[], flags: readonly string[], technology: string, description: string, example: readonly string[]) => ({
        executor: 'nx:run-commands',
        options: { command: line([...argv, ...flags]), cwd: root },
        cache: true,
        metadata: { technologies: [technology], description, help: { command: line([...argv, '--help']), example: { args: [...example] } } },
    });
    const inputs = (runner: string, ...extra: NonNullable<TargetConfiguration['inputs']>): NonNullable<TargetConfiguration['inputs']> => ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), ...extra, { externalDependencies: [runner] }];
    const written = (folders: readonly string[]) => folders.map((at) => path.join('{workspaceRoot}', at));
    const output = (tool: string, flag: string, ...rest: readonly string[]) => Option.match(placed(path, root, outputPath, tool), { onNone: () => [], onSome: (at) => [flag, at, ...rest] });
    const built: Record.ReadonlyRecord<string, TargetConfiguration> = {
        ...Record.fromIterableWith(Option.toArray(Option.fromUndefinedOr(entry.bundles)), (folders): [string, TargetConfiguration] => [
            'build--vite',
            { ...command(['vite', 'build'], output('vite', '--outDir', '--emptyOutDir'), 'vite', 'Builds every environment of the Vite config with vite build', ['--mode', 'production']), inputs: inputs('vite'), outputs: written(folders) },
        ]),
        ...(markers.storybook === undefined
            ? {}
            : {
                  'build--storybook': {
                      ...command(['storybook', 'build'], output('storybook', '--output-dir'), 'storybook', 'Builds the static Storybook of the project stories', ['--test']),
                      inputs: inputs('storybook'),
                      outputs: [Option.match(placed(path, root, outputPath, 'storybook'), { onNone: () => '{projectRoot}/storybook-static', onSome: (at) => path.join('{workspaceRoot}', root, at) })],
                  },
              }),
        ...(markers.cloudflare === undefined
            ? {}
            : {
                  'build--cf': {
                      ...command(['cf', 'workers', 'types'], [], 'cloudflare', 'Generates the Worker binding and runtime types from cloudflare.config.ts', ['--mode', 'production']),
                      inputs: ['default', '^default', { externalDependencies: ['cf'] }],
                      outputs: ['{projectRoot}/.cloudflare/types'],
                  },
              }),
    };
    return {
        ...built,
        ...Record.fromIterableWith(Option.toArray(Option.fromUndefinedOr(entry.suite)), (folders): [string, TargetConfiguration] => [
            'test',
            { ...command(['vitest', 'run'], [], 'vitest', 'Runs every Vitest project of the suite with vitest run', ['--project', '<name>']), inputs: inputs('vitest', { env: 'CI' }, { runtime: 'node -p process.platform+process.arch' }), outputs: written(folders) },
        ]),
        ...(Record.isEmptyRecord(built) ? {} : { build: { executor: 'nx:noop', dependsOn: ['build--*'], inputs: ['...', ...Array.dedupe(Object.values(built).flatMap((target) => target.inputs ?? []))], metadata: { technologies: ['vite'], description: 'Runs every build--* target' } } }),
        ...(markers.cloudflare === undefined ? {} : { 'check--tsc': { dependsOn: ['...', 'build--cf'], inputs: ['...', { dependentTasksOutputFiles: '**/*.d.ts' }] } }),
    };
};

const inferred = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const [fs, path, decoded] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
    const { workspaceRoot } = context;
    const found = Array.getSomes(
        files.map((file) =>
            pipe(
                Array.findFirst(Record.toEntries(MARKERS), ([, pattern]) => matchesGlob(file, `**/${pattern}`)),
                Option.map(([kind, pattern]) => ({ kind, file, root: path.join(file, ...Array.replicate('..', pattern.split('/').length)) })),
            ),
        ),
    );
    const grouped = yield* Effect.filter(Record.toEntries(Array.groupBy(found, ({ root }) => root)), ([root]) => fs.exists(path.join(workspaceRoot, root, 'package.json')), { concurrency: 'unbounded' });
    const projects = grouped.map(([root, group]) => ({ root, file: Array.headNonEmpty(group).file, markers: Record.fromIterableWith(group, ({ kind, file }) => [kind, file]), chain: chained(path, workspaceRoot, root) }));
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const source = hashArray(yield* Effect.forEach((yield* fs.readDirectory(import.meta.dirname)).toSorted(), (module) => fs.readFileString(path.join(import.meta.dirname, module))));
    const lockfile = getLockFileName(detectPackageManager(workspaceRoot));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            projects.map(({ root }) => root),
            { ...decoded, source },
            context,
            projects.map(({ chain }) => [lockfile, ...chain]),
        ),
    );
    const cache = new PluginCache<Read & { readonly loaded: readonly string[]; readonly digest: string }>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(decoded)}.hash`));
    const digest = (sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));
    const held = Array.zipWith(projects, hashes, (project, hash) => ({ ...project, hash, held: Option.fromNullishOr(cache.get(hash)) }));
    const current = yield* Effect.forEach(held, (project) => digest(Option.match(project.held, { onNone: () => [], onSome: ({ loaded: modules }) => modules })));
    const keyed = Array.zipWith(held, current, (project, now) => ({ ...project, entry: Option.filter(project.held, ({ digest: then }) => then === now) }));
    const missed = keyed.filter(({ entry }) => Option.isNone(entry));
    const edges = new Map<string | undefined, Set<string>>();
    const results = yield* Effect.acquireUseRelease(
        Effect.sync(() =>
            registerHooks({
                resolve: (specifier, hookContext, next) => {
                    const resolution = next(specifier, hookContext);
                    edges.set(hookContext.parentURL, (edges.get(hookContext.parentURL) ?? new Set<string>()).add(resolution.url));
                    return resolution;
                },
            }),
        ),
        () => Effect.forEach(missed, ({ root, markers }) => Effect.result(read(workspaceRoot, root, markers, Option.fromUndefinedOr(decoded.outputPath)))),
        (hooks) => Effect.sync(() => hooks.deregister()),
    );
    const fresh = Array.zipWith(missed, results, (project, result) => ({ ...project, result }));
    const stored = yield* Effect.forEach(
        Array.filterMap(fresh, ({ result, ...project }) => Result.map(result, (entry) => ({ ...project, entry }))),
        Effect.fnUntraced(function* ({ entry, ...project }) {
            const tracked = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, reached(path, edges, workspaceRoot, project.root)));
            const complete = { ...entry, loaded: tracked, digest: yield* digest(tracked) };
            yield* Effect.sync(() => cache.set(project.hash, complete));
            return { ...project, entry: complete };
        }),
    );
    yield* Effect.sync(() => cache.writeToDisk());
    const nodes = [...Array.getSomes(keyed.map(({ entry, ...project }) => Option.map(entry, (value) => ({ ...project, entry: value })))), ...stored].map(({ root, file, markers, chain, entry }): CreateNodesResultArray[number] => [
        file,
        { projects: { [root]: { targets: targets(path, root, markers, entry, chain, Option.fromUndefinedOr(decoded.outputPath)) } } },
    ]);
    const failures = Array.filterMap(fresh, ({ file, result }) => Result.map(Result.flip(result), (failure): [string, Error] => [file, failure]));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(nodes), onNonEmpty: (errors) => Effect.fail(new AggregateCreateNodesError(errors, nodes)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    `**/{${Object.values(MARKERS).join(',')}}`,
    (files, options, context) =>
        inferred(files, options, context).pipe(
            Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, error]], []))),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
