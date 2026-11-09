import { findPackageJSON } from 'node:module';
import process, { env } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, detectPackageManager, hashArray, type TargetConfiguration } from '@nx/devkit';
import { calculateHashesForCreateNodes, getLockFileName, hashObject, loadConfigFile, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import type { PlaywrightTestConfig } from '@playwright/test';
import { Array, Effect, FileSystem, Option, Path, Predicate, pipe, Record, Result, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORTS: Record.ReadonlyRecord<string, string> = { html: 'playwright-report', blob: 'blob-report' };

// --- [MODELS] --------------------------------------------------------------------------

const Reported = Schema.Struct({ outputFile: Schema.optionalKey(Schema.String), outputDir: Schema.optionalKey(Schema.String), outputFolder: Schema.optionalKey(Schema.String) }).annotate({
    description: 'The output options of a Playwright reporter entry, read in the order @nx/playwright reads them',
});
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The package.json closest to this module, its own package in the tree and in dist alike' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = ({ outputDir = 'test-results', projects = [], reporter = [] }: PlaywrightTestConfig) => [
    outputDir,
    ...Array.getSomes(projects.map((project) => Option.fromUndefinedOr(project.outputDir))),
    ...Array.getSomes(
        (Predicate.isString(reporter) ? [[reporter, {}] as const] : reporter).map(([name, options]) =>
            Option.firstSomeOf([Option.flatMap(Schema.decodeUnknownOption(Reported)(options), ({ outputFile, outputDir: folder, outputFolder }) => Option.fromUndefinedOr(outputFile ?? folder ?? outputFolder)), Record.get(REPORTS, name)]),
        ),
    ),
];

const read = Effect.fnUntraced(function* (path: Path.Path, workspaceRoot: string, root: string, file: string) {
    const before = yield* Effect.sync(() => {
        Reflect.deleteProperty(process, '__pw_initiator__');
        return { ...env };
    });
    const config = yield* Effect.tryPromise(() => loadConfigFile<PlaywrightTestConfig>(path.join(workspaceRoot, file))).pipe(
        Effect.ensuring(
            Effect.sync(() => {
                Object.keys(env)
                    .filter((key) => !(key in before))
                    .forEach((key) => Reflect.deleteProperty(env, key));
                Object.assign(env, before);
            }),
        ),
    );
    return written(config).map((at) => path.relative(workspaceRoot, path.resolve(workspaceRoot, root, at)));
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

const tested = (path: Path.Path, root: string, chain: readonly string[], folders: readonly string[]): TargetConfiguration => ({
    executor: 'nx:run-commands',
    options: { command: 'playwright test', cwd: root },
    cache: true,
    inputs: ['default', '^default', ...chain.map((at) => path.join('{workspaceRoot}', at)), { env: 'CI' }, { externalDependencies: ['@playwright/test'] }],
    outputs: Array.dedupe(folders)
        .filter((at, _, all) => !all.some((other) => at.startsWith(`${other}/`)))
        .map((at) => path.join('{workspaceRoot}', at)),
    metadata: { technologies: ['playwright'], description: 'Runs every Playwright project of the suite with playwright test', help: { command: 'playwright test --help', example: { args: ['--debug=cli'] } } },
});

const inferred = Effect.fnUntraced(function* (files: readonly string[], options: unknown, context: CreateNodesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { workspaceRoot } = context;
    const found = yield* Effect.filter(
        files.map((file) => ({ file, root: path.dirname(file) })),
        ({ root }) => fs.exists(path.join(workspaceRoot, root, 'package.json')),
        { concurrency: 'unbounded' },
    );
    const projects = found.map((project) => ({ ...project, chain: chained(path, workspaceRoot, project.root) }));
    const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
    const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
    const source = hashArray(yield* Effect.forEach((yield* fs.readDirectory(import.meta.dirname)).toSorted(), (module) => fs.readFileString(path.join(import.meta.dirname, module))));
    const lockfile = getLockFileName(detectPackageManager(workspaceRoot));
    const hashes = yield* Effect.promise(() =>
        calculateHashesForCreateNodes(
            projects.map(({ root }) => root),
            { options, source },
            context,
            projects.map(({ chain }) => [lockfile, ...chain]),
        ),
    );
    const cache = new PluginCache<readonly string[]>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options ?? {})}.hash`));
    const results = yield* Effect.forEach(Array.zip(projects, hashes), ([{ file, root }, hash]) => pipe(Option.fromNullishOr(cache.get(hash)), Option.match({ onSome: (outputs) => Effect.succeed<readonly string[]>(outputs), onNone: () => read(path, workspaceRoot, root, file) }), Effect.result));
    const fresh = Array.filterMap(Array.zip(hashes, results), ([hash, result]) => Result.map(result, (outputs) => ({ hash, outputs })));
    yield* Effect.sync(() => fresh.forEach(({ hash, outputs }) => cache.set(hash, outputs)));
    yield* Effect.sync(() => cache.writeToDisk());
    const nodes = Array.getSomes(Array.zipWith(projects, hashes, ({ file, root, chain }, hash) => Option.map(Option.fromNullishOr(cache.get(hash)), (folders): CreateNodesResultArray[number] => [file, { projects: { [root]: { targets: { test: tested(path, root, chain, folders) } } } }])));
    const failures = Array.filterMap(Array.zip(projects, results), ([{ file }, result]) => Result.map(Result.flip(result), (failure): [string, Error] => [file, failure]));
    return yield* Array.match(failures, { onEmpty: () => Effect.succeed(nodes), onNonEmpty: (errors) => Effect.fail(new AggregateCreateNodesError(errors, nodes)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    '**/playwright.config.{js,ts,cjs,cts,mjs,mts}',
    (files, options, context) =>
        inferred(files, options, context).pipe(
            Effect.mapError((error) => (error instanceof AggregateCreateNodesError ? error : new AggregateCreateNodesError([[null, error]], []))),
            Effect.provide(layer),
            Effect.runPromise,
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
