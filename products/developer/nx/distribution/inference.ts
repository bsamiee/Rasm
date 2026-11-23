import { ChildProcess as NodeChildProcess } from 'node:child_process';
import { existsSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, posix, relative, sep } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodesContext, type ExecutorContext, getOutputsForTargetAndConfiguration, hashArray, normalizePath, type NxJsonConfiguration, type ProjectConfiguration, readJsonFile, type TargetConfiguration, type TaskGraph } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, getNamedInputs, hashFile, hashWithWorkspaceContext, killChildOnHostExit, mergeTargetConfigurations, type PackageJson, type PluginCache } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Cause, Config, ConfigProvider, Effect, Equal, FileSystem, Formatter, Glob, identity, Layer, ManagedRuntime, Match, Option, Predicate, Record, Result, Schema, type Scope, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Stated = Readonly<Record<string, Pick<TargetConfiguration, 'inputs'>>>;
type Run<R> = <A, E>(effect: Effect.Effect<A, E, R | NodeServices | Scope.Scope>) => Promise<A>;
type Parse = (file: string, text: string) => { readonly module: { readonly staticImports: readonly { readonly moduleRequest: { readonly value: string }; readonly entries: readonly { readonly isType: boolean }[] }[] } };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Folder = Schema.optionalKey(Schema.String);
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Targets = Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) })).annotate({
    description: "The target rows of a project's package.json nx field or project.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Overrides = Schema.fromJsonString(Schema.Struct({ targets: Schema.optionalKey(Targets), nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Targets) })) })).annotate({
    description: 'The nx.targets of a package.json and the targets of a project.json, which Nx merges over the inferred targets in that order',
});
const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json' });
const Words = Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const legible = (error: unknown): Error =>
    Match.value(error).pipe(
        Match.when(
            Match.instanceOf(AggregateCreateNodesError),
            (aggregate) =>
                new AggregateCreateNodesError(
                    aggregate.errors.map(([file, each]) => [file, legible(each)]),
                    aggregate.partialResults,
                ),
        ),
        Match.when(
            (each: unknown): each is Error => Predicate.isError(each) && Object.hasOwn(each, 'message') && Predicate.isString(each.message) && String.isNonEmpty(each.message),
            (kept): Error => kept,
        ),
        Match.when(Predicate.isError, (each) => new Cause.UnknownError(each, Predicate.isString(each.message) && String.isNonEmpty(each.message) ? each.message : (each.stack ?? Formatter.format(each)))),
        Match.orElse((each) => new Cause.UnknownError(each, Formatter.format(each))),
    );

const runner = <R>(services: Layer.Layer<R>): Run<R> => {
    const runtime = ManagedRuntime.make(Layer.merge(services, layer));
    return <A, E>(effect: Effect.Effect<A, E, R | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(Effect.mapError(effect, legible)).pipe(Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnv())));
};

const run = runner(Layer.empty);

const dried = (dryRun = false) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun || stated);

const executor = <S extends Schema.Decoder<unknown>, A>(options: S, body: (stated: S['Type'], context: ExecutorContext) => Effect.fn.Return<A, Error, NodeServices | Scope.Scope>) => {
    const stated = Effect.fnUntraced(body);
    return (raw: unknown, context: ExecutorContext) => run(Effect.flatMap(Schema.decodeUnknownEffect(options)(raw), (decoded) => stated(decoded, context)));
};

const inferring = <S extends Schema.Decoder<unknown>, A, R>(options: S, entered: <B, E>(effect: Effect.Effect<B, E, R>) => Promise<B>, body: (files: readonly string[], settings: S['Type'], context: CreateNodesContext) => Effect.fn.Return<A, Error, NoInfer<R>>) => {
    const inferred = Effect.fnUntraced(body);
    return (files: readonly string[], raw: unknown, context: CreateNodesContext) => entered(Effect.flatMap(Schema.decodeEffect(options)(raw ?? {}), (settings) => inferred(files, settings, context)));
};

const batched = <S extends Schema.Decoder<unknown>, A, E>(options: S, body: (graph: TaskGraph, inputs: Readonly<Record<string, S['Type']>>, context: ExecutorContext) => Stream.Stream<A, E, NodeServices>) => {
    const decode = Schema.decodeUnknownEffect(Schema.Record(Schema.String, options));
    return (graph: TaskGraph, raw: unknown, _: unknown, context: ExecutorContext) => Stream.toReadableStream(Stream.provide(Stream.unwrap(Effect.map(decode(raw), (inputs) => body(graph, inputs, context))), layer)).values();
};

const fingerprinted = (folder: string) =>
    Effect.runSync(
        Effect.cached(
            Effect.map(
                FileSystem.FileSystem.use((fs) => fs.readDirectory(folder)),
                (names) => hashArray([...Array.fromNullishOr(hashFile(import.meta.filename)), ...names.toSorted().flatMap((name) => [name, ...Array.fromNullishOr(hashFile(join(folder, name)))])]),
            ),
        ),
    );

const ran = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const invoked = CommandLine.display(argv);
    const handle = yield* Effect.mapError(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false }), (error) => new Cause.UnknownError(error, `${invoked} cannot start: ${error.message}`));
    const tracked = Object.assign(new NodeChildProcess(), { pid: handle.pid });
    yield* Effect.acquireRelease(
        Effect.sync(() => killChildOnHostExit(tracked)),
        () => Effect.sync(() => tracked.emit('exit')),
    );
    const [stdout, said, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return { invoked, stdout, said, code };
}, Effect.scoped);

const spawned = (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) =>
    Effect.flatMap(ran(argv, options), ({ invoked, stdout, said, code }) => (code === 0 ? Effect.succeed(stdout) : Effect.fail(new Cause.UnknownError(code, [`${invoked} exits ${code}`, ...Option.toArray(Array.findFirst([said.trim(), stdout.trim()], String.isNonEmpty))].join(': ')))));

const within = (at: string) => !(at.startsWith('..') || at.split('/').includes('node_modules'));

const tokened = (root: string) => (at: string) => (posix.relative(root, at).startsWith('..') ? posix.join('{workspaceRoot}', at) : posix.join('{projectRoot}', posix.relative(root, at)));

const namedInput = (root: string, context: CreateNodesContext) => ('production' in getNamedInputs(root, context) ? 'production' : 'default');

const tagged = ({ release }: NxJsonConfiguration) => Array.dedupe([...Array.ensure(release?.projects ?? []), ...Object.values(release?.groups ?? {}).flatMap(({ projects }) => Array.ensure(projects))].filter((selector) => selector.startsWith('tag:')).map((selector) => selector.slice('tag:'.length)));

const anchored = (workspaceRoot: string) => {
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (file: string) => readJsonFile<PackageJson>(join(workspaceRoot, file))));
    return (root: string) => existsSync(join(workspaceRoot, root, 'project.json')) || (member(posix.join(root, 'package.json')) && existsSync(join(workspaceRoot, root, 'package.json')));
};

const above = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(posix.dirname(at), (up) => up !== at)] as const),
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
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter(within);
};

const stamped = (workspaceRoot: string) => (sources: readonly string[]) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...sources]));

const recalled = <A extends { readonly sources: readonly string[]; readonly stamp: string }>(cache: PluginCache<A>, key: string, stamp: (sources: readonly string[]) => Effect.Effect<string>) =>
    Effect.findFirst(Array.fromNullishOr(cache.get(key)), ({ sources, stamp: held }) => Effect.map(stamp(sources), Equal.equals(held)));

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const read = (file: string) =>
        fs.readFileString(join(workspaceRoot, root, file)).pipe(
            Effect.flatMap(Schema.decodeEffect(Overrides)),
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Overrides.make({}))),
        );
    const [manifest, project] = yield* Effect.all([read('package.json'), read('project.json')], { concurrency: 'unbounded' });
    return Record.union(manifest.nx?.targets ?? {}, project.targets ?? {}, (fromPackage, fromProject) => mergeTargetConfigurations(fromProject, fromPackage));
});

const parsed = Effect.fnUntraced(function* (parse: Parse, workspaceRoot: string, files: readonly string[]) {
    const texts = yield* Effect.forEach(files, (at) => FileSystem.FileSystem.use((fs) => fs.readFileString(join(workspaceRoot, at))), { concurrency: 'unbounded' });
    const statements = Array.zipWith(files, texts, (at, text) => parse(at, text).module.staticImports.map((statement) => [statement, join(workspaceRoot, at)] as const)).flat();
    return statements.filter(([{ entries }]) => entries.length === 0 || entries.some(({ isType }) => !isType)).map(([{ moduleRequest }, from]) => [moduleRequest.value, from] as const);
});

const packaged = Effect.fnUntraced(function* (imports: readonly (readonly [specifier: string, from: string])[]) {
    const fs = yield* FileSystem.FileSystem;
    const found = Array.getSomes(imports.filter(([specifier]) => !(specifier.startsWith('.') || specifier.startsWith('#') || isAbsolute(specifier) || URL.canParse(specifier))).map(([specifier, from]) => Option.flatMap(Option.liftThrowable(findPackageJSON)(specifier, from), Option.fromUndefinedOr)));
    const manifests = yield* Effect.forEach(Array.dedupe(found), (at) => fs.realPath(at), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(Array.dedupe(manifests), (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail({ name, folder: dirname(at) }))), { concurrency: 'unbounded' });
    const [packages, linked] = Array.partition(read, identity);
    return { packages: Array.dedupe(packages), linked: Array.dedupe(linked.map(({ name }) => name)), folders: linked.map(({ folder }) => folder) };
});

const produced = (projects: Readonly<Record<string, ProjectConfiguration>>, paths: Array.NonEmptyReadonlyArray<string>): Readonly<Record<string, readonly string[]>> => {
    const declared = Record.collect(projects, (name, data) =>
        Record.collect(
            Record.filter(data.targets ?? {}, ({ outputs }) => outputs !== undefined),
            (target, { configurations = {} }) => ({ name, data, target, configurations }),
        ),
    ).flat();
    const outputs = declared.flatMap(({ name, data, target, configurations }) =>
        [{}, ...Record.keys(configurations).map((configuration) => ({ configuration }))]
            .flatMap((selected) => getOutputsForTargetAndConfiguration({ project: name, target, ...selected }, {}, { name, type: 'lib', data }))
            .filter((output) => !output.startsWith('!'))
            .map((output) => ({ name, output, covering: Glob.make(`${output}{,/**}`, { dot: true }) })),
    );
    const matched = Array.cartesian(Array.dedupe(paths), outputs).filter(([at, { output, covering }]) => Glob.match(covering, at) || !posix.relative(at, output).startsWith('..'));
    return Record.map(
        Array.groupBy(matched, ([at]) => at),
        (pairs) => Array.dedupe(pairs.map(([, { name }]) => name)),
    );
};

const aggregated = (verbs: Verbs, stated: Stated, technology: string): Record<string, TargetConfiguration> =>
    Record.fromEntries(
        Record.collect(Record.filter(verbs, Predicate.not(Record.isEmptyRecord)), (verb, tools): readonly (readonly [string, TargetConfiguration])[] => [
            [
                verb,
                {
                    dependsOn: [{ target: `${verb}--*`, params: 'forward' }],
                    inputs: [
                        '...',
                        ...Array.dedupe(
                            Record.collect(tools, (tool, target) => mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? ['default', '^default'] }).inputs ?? [])
                                .flat()
                                .map((entry) => (Predicate.isObject(entry) && 'dependentTasksOutputFiles' in entry ? { ...entry, transitive: true } : entry)),
                        ),
                    ],
                },
            ],
            ...Record.collect(tools, (tool, target): readonly [string, TargetConfiguration] => [
                `${verb}--${tool}`,
                { ...target, ...(verb === 'clean' && { configurations: { ...target.configurations, cache: { targets: [], commands: [], folders: [], ...Option.getOrUndefined(Option.liftPredicate(Predicate.isObject)(target.configurations?.['cache'])) } } }), metadata: { technologies: [technology], ...target.metadata } },
            ]),
        ]).flat(),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { above, aggregated, anchored, batched, dried, executor, extended, fingerprinted, Folder, inferring, manifested, namedInput, packaged, parsed, PLATFORM, produced, ran, recalled, type Run, run, runner, spawned, type Stated, stamped, tagged, Targets, tokened, type Verbs, Words, within };
