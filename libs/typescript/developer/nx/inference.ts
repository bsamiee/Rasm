import { ChildProcess as NodeChildProcess } from 'node:child_process';
import { once } from 'node:events';
import { existsSync, readdirSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { dirname, isAbsolute, join, posix, relative, sep } from 'node:path';
import { cwd } from 'node:process';
import type { Worker } from 'node:worker_threads';
import { layer as pathLayer } from '@effect/platform-node/NodePath';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodesContext, detectPackageManager, type ExecutorContext, hashArray, normalizePath, type NxJsonConfiguration, type PackageManager, readJsonFile, type TargetConfiguration, type TaskGraph } from '@nx/devkit';
import { buildPackageJsonPatterns, buildPackageJsonWorkspacesMatcher, getLockFileName, getNamedInputs, hashFile, hashWithWorkspaceContext, killChildOnHostExit, mergeTargetConfigurations, type PackageJson, PluginCache, parseExecutor, readModulePackageJson, workspaceDataDirectory } from '@nx/devkit/internal';
import { walkTsconfigExtendsChain } from '@nx/js/internal';
import { run as runProcess } from '@rasm/process';
import { Array, Cause, Config, ConfigProvider, Context, Effect, Equal, FileSystem, Filter, Formatter, Function, identity, Layer, Logger, ManagedRuntime, Match, MutableRef, Option, Path, Predicate, pipe, Record, References, Result, Schema, Scope, Stream, String, Struct, Tuple, UndefinedOr } from 'effect';
import { ChildProcessSpawner } from 'effect/process';
import collection from '#/executors.json' with { type: 'json' };
import plugin from '#/package.json' with { type: 'json' };

// --- [TYPES] ---------------------------------------------------------------------------

type Verbs = Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>;
type Stated = typeof Stated.Type;
type Anchor = ReturnType<typeof anchored>;
type Pass<C extends { readonly sources: readonly string[] }> = Effect.Success<ReturnType<typeof caching<C>>>;
type Run<R> = <A, E>(effect: Effect.Effect<A, E, R | NodeServices | Scope.Scope>) => Promise<A>;
type Parse = (file: string, text: string) => { readonly module: { readonly staticImports: readonly { readonly moduleRequest: { readonly value: string }; readonly entries: readonly { readonly isType: boolean }[] }[] } };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const EXEC: Readonly<Record<PackageManager, string>> = { npm: 'npx', pnpm: 'pnpm exec', yarn: 'yarn', bun: 'bun' };
const EXECUTORS = Record.map(collection.executors, (_, executor) => `${plugin.name}:${executor}`);

// --- [MODELS] --------------------------------------------------------------------------

const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Stated = Schema.Record(Schema.String, Schema.Struct({ inputs: Input.pipe(Schema.Array, Schema.mutable, Schema.optionalKey) })).annotate({
    description: "The target rows of a project's package.json nx field or project.json, whose inputs Nx merges over the inferred targets after every plugin",
});
const Overrides = Schema.fromJsonString(Schema.Struct({ targets: Schema.optionalKey(Stated), nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Stated) })) })).annotate({
    description: 'The nx.targets of a package.json and the targets of a project.json, which Nx merges over the inferred targets in that order',
});
const Named = Schema.fromJsonString(Schema.Struct({ name: Schema.String })).annotate({ description: 'The name field of a package.json' });
const Words = Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' });

// --- [ERRORS] --------------------------------------------------------------------------

class NxError extends Schema.TaggedError<NxError, { readonly brand: unique symbol }>()('NxError', {
    reason: Schema.TaggedUnion({
        Failed: { step: Schema.String },
        Invalid: { code: Schema.String },
        Unloaded: { tool: Schema.String },
        Unmatched: { input: Schema.Array(Schema.String), outputs: Schema.Array(Schema.String), tasks: Schema.Array(Schema.String) },
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: NxError['reason'], subject: string) => (cause?: unknown) => NxError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [subject, String.noCase(reason._tag), labeled(Struct.omit(reason, ['_tag'])).join(', ')].filter(String.isNonEmpty).join(' ') });
}

// --- [SERVICES] ------------------------------------------------------------------------

const Root = Context.Reference<string>('@rasm/nx/Root', { defaultValue: cwd });
const hostPath = Path.Path.pipe(Effect.service, Effect.provide(pathLayer), Effect.runSync);
const hosted = Layer.provideMerge(
    Layer.effect(ChildProcessSpawner.ChildProcessSpawner)(
        Effect.gen(function* () {
            const node = yield* ChildProcessSpawner.ChildProcessSpawner;
            return ChildProcessSpawner.make(
                Effect.fnUntraced(function* (command) {
                    const handle = yield* node.spawn(command);
                    const tracked = Object.assign(new NodeChildProcess(), { pid: handle.pid });
                    yield* Scope.addFinalizer(
                        yield* Effect.scope,
                        Effect.sync(() => tracked.emit('exit')),
                    );
                    yield* Effect.sync(() => killChildOnHostExit(tracked));
                    return handle;
                }),
            );
        }),
    ),
    layer,
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const legible = (error: unknown): Error =>
    Match.value(error).pipe(
        Match.when(Match.instanceOf(AggregateCreateNodesError), (aggregate) => new AggregateCreateNodesError(aggregate.errors.map(Tuple.evolve([undefined, legible])), aggregate.partialResults)),
        Match.when(
            (each: unknown): each is Error => Predicate.isError(each) && Object.hasOwn(each, 'message') && Predicate.isString(each.message) && String.isNonEmpty(each.message),
            (each) => Array.match(Array.ensure(each.cause).filter(Predicate.isError), { onEmpty: () => each, onNonEmpty: (causes) => new Cause.UnknownError(each, [each.message, ...causes.map((cause) => Formatter.format(cause))].join(': ')) }),
        ),
        Match.when(Predicate.isError, (each) => new Cause.UnknownError(each, Predicate.isString(each.message) && String.isNonEmpty(each.message) ? each.message : (each.stack ?? Formatter.format(each)))),
        Match.orElse((each) => new Cause.UnknownError(each, Formatter.format(each))),
    );

const formatted = (value: unknown) => (Predicate.isString(value) ? value : Formatter.format(value));

const labeled = (fields: object): readonly string[] =>
    Object.entries(fields).flatMap(([label, value]: readonly [string, unknown]) => (value === true ? [String.noCase(label)] : Array.match(Array.ensure(value).filter(Predicate.isTruthy), { onEmpty: Array.empty, onNonEmpty: (items) => [`${String.noCase(label)}: ${items.map(formatted).join(' ')}`] })));

const reported = Logger.withConsoleLog(Logger.make(({ message, cause, fiber }) => [...Array.ensure<unknown>(message).map(formatted), labeled(fiber.getRef(References.CurrentLogAnnotations)).join(', '), ...(cause.reasons.length > 0 ? [Cause.pretty(cause)] : [])].filter(String.isNonEmpty).join(' ')));

const runner = <R>(services: Layer.Layer<R>): Run<R> => {
    const runtime = ManagedRuntime.make(Layer.mergeAll(services, hosted, Logger.layer([reported])));
    return <A, E>(effect: Effect.Effect<A, E, R | NodeServices | Scope.Scope>) => runtime.runPromise(effect.pipe(Effect.mapError(legible), Effect.scoped, Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnv())));
};

const run = runner(Layer.empty);

const dried = (dryRun = false) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun || stated);

const executor = <S extends Schema.Decoder<unknown>, A>(options: S, body: (stated: S['Type'], context: ExecutorContext) => Effect.fn.Return<A, Error, NodeServices | Scope.Scope>) => {
    const stated = Effect.fnUntraced(body);
    return (raw: unknown, context: ExecutorContext) => run(Effect.flatMap(Schema.decodeUnknownEffect(options)(raw), (decoded) => stated(decoded, context)));
};

const batched = <S extends Schema.Decoder<unknown>, A, E>(options: S, body: (graph: TaskGraph, inputs: Readonly<Record<string, S['Type']>>, context: ExecutorContext) => Stream.Stream<A, E, NodeServices>) => {
    const decode = Schema.decodeUnknownEffect(Schema.Record(Schema.String, options));
    return (graph: TaskGraph, raw: unknown, _: unknown, context: ExecutorContext) =>
        decode(raw)
            .pipe(
                Effect.map((inputs) => body(graph, inputs, context)),
                Stream.unwrap,
                Stream.provide(hosted),
                Stream.toReadableStream(),
            )
            .values();
};

const fingerprinted = (folder: string) => Effect.succeed(hashArray([...Array.fromNullishOr(hashFile(import.meta.filename)), ...Array.flatMap(readdirSync(folder).toSorted(), (name) => Array.fromNullishOr(hashFile(join(folder, name))).flatMap((hash) => [name, hash]))]));

const spawned = (argv: Array.NonEmptyReadonlyArray<string>, options?: Parameters<typeof runProcess>[1]) => Effect.map(runProcess(argv, options), Struct.get('stdout'));

const request = <S extends Schema.Decoder<unknown>>(start: Effect.Effect<Worker | NodeChildProcess>, reply: S, failed: (cause: unknown) => NxError) =>
    Effect.acquireDisposable(start).pipe(
        Effect.flatMap((peer) => Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(peer, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(peer, 'exit', { signal }), catch: failed }), failed))),
        Effect.flatMap(([message]) => Schema.decodeUnknownEffect(reply)(message)),
        Effect.scoped,
    );

const inside = (folder: string) => (file: string) => Option.isSome(hostPath.resolveWithin(folder, hostPath.relative(folder, file)));

const below = (folder: string) => Predicate.and(inside(folder), (file: string) => relative(folder, file) !== '');

const within = (at: string) => inside('.')(at) && !at.split('/').includes('node_modules');

const owned = (root: string) => (at: string) => posix.relative(root, posix.dirname(at)) === '';

const tokened = (root: string) => (at: string) => (root !== '.' && inside(root)(at) ? posix.join('{projectRoot}', posix.relative(root, at)) : posix.join('{workspaceRoot}', at));

const namedInput = (root: string, context: CreateNodesContext) => ('production' in getNamedInputs(root, context) ? 'production' : 'default');

const tagged = ({ release }: NxJsonConfiguration) => Array.dedupe([...Array.ensure(release?.projects ?? []), ...Object.values(release?.groups ?? {}).flatMap(({ projects }) => Array.ensure(projects))].filter((selector) => selector.startsWith('tag:')).map((selector) => selector.slice('tag:'.length)));

const anchored = (workspaceRoot: string) => {
    const member = buildPackageJsonWorkspacesMatcher(buildPackageJsonPatterns(workspaceRoot, (file: string) => readJsonFile<PackageJson>(join(workspaceRoot, file))));
    return { workspaceRoot, holds: (root: string) => existsSync(join(workspaceRoot, root, 'project.json')) || (member(posix.join(root, 'package.json')) && existsSync(join(workspaceRoot, root, 'package.json'))) } as const;
};

const above = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(posix.dirname(at), (up) => up !== at)] as const),
    );

const extended = (workspaceRoot: string, root: string): readonly string[] => {
    const files: string[] = [];
    Option.toArray(Array.findFirst(above(root), (at) => Option.liftPredicate(join(workspaceRoot, at, 'tsconfig.json'), existsSync))).forEach((config) =>
        walkTsconfigExtendsChain(config, (at) => {
            files.push(at);
            return 'continue';
        }),
    );
    return files.map((at) => normalizePath(relative(workspaceRoot, at))).filter(within);
};

const stamped = (workspaceRoot: string) => (sources: readonly string[]) => Array.match(sources, { onEmpty: () => Effect.succeed(hashArray([])), onNonEmpty: (globs) => Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [...globs])) });

const recalled = Effect.fnUntraced(function* <A extends { readonly sources: readonly string[] }, E, R>(cache: PluginCache<A & { readonly stamp: string }>, key: string, stamp: (sources: readonly string[], entry: A) => Effect.Effect<string>, fresh: Effect.Effect<NoInfer<A>, E, R>, stored: () => unknown) {
    return yield* Option.match(yield* Effect.findFirst(Array.fromNullishOr(cache.get(key)), (held) => Effect.map(stamp(held.sources, held), Equal.equals(held.stamp))), {
        onSome: Effect.succeed,
        onNone: Effect.fnUntraced(function* () {
            const read = yield* fresh;
            const entry = { ...read, stamp: yield* stamp(read.sources, read) };
            cache.set(key, entry);
            stored();
            return entry;
        }),
    });
});

const caching = Effect.fnUntraced(function* <C extends { readonly sources: readonly string[] }>(name: string, digest: string, extra: (entry: C) => Effect.Effect<readonly string[]> = () => Effect.succeed([]), steady: () => unknown = Function.constUndefined) {
    const root = yield* Root;
    const manager = detectPackageManager(root);
    const [stored, lock, reading] = [MutableRef.make(false), getLockFileName(manager), steady()];
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<C & { readonly stamp: string }>(join(workspaceDataDirectory, name))),
        (opened) => Effect.sync(() => opened.writeToDisk()).pipe(Effect.when(Effect.sync(() => MutableRef.get(stored) && steady() === reading))),
    );
    return {
        recall: <E, R>(key: string, fresh: Effect.Effect<C, E, R>) =>
            recalled(
                cache,
                hashArray([digest, key]),
                (sources, entry) => Effect.zipWith(stamped(root)([lock, ...sources]), extra(entry), (held, more) => hashArray([held, ...more])),
                fresh,
                () => MutableRef.set(stored, true),
            ),
        exec: EXEC[manager],
    };
});

const inferring = <S extends Schema.Decoder<unknown>, A, R>(options: S, entered: <B, E>(effect: Effect.Effect<B, E, R>) => Promise<B>, body: (files: readonly string[], settings: S['Type'], context: CreateNodesContext, cached: typeof caching) => Effect.fn.Return<A, Error, NoInfer<R>>) => {
    const inferred = Effect.fnUntraced(body);
    return (files: readonly string[], raw: unknown, context: CreateNodesContext) => entered(Effect.flatMap(Schema.decodeEffect(options)(raw ?? {}), (settings) => inferred(files, settings, context, caching)).pipe(Effect.provideService(Root, context.workspaceRoot)));
};

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
    const fs = yield* FileSystem.FileSystem;
    const read = yield* Effect.forEach(files, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [parse(at, text).module.staticImports, join(workspaceRoot, at)] as const), { concurrency: 'unbounded' });
    return read.flatMap(([statements, from]) => statements.filter(({ entries }) => entries.length === 0 || !entries.every(Struct.get('isType'))).map(({ moduleRequest }) => [moduleRequest.value, from] as const));
});

const packaged = Effect.fnUntraced(function* (imports: readonly (readonly [specifier: string, from: string])[]) {
    const fs = yield* FileSystem.FileSystem;
    const found = Array.filterMap(imports, ([specifier, from]) => (specifier.startsWith('.') || specifier.startsWith('#') || isAbsolute(specifier) || URL.canParse(specifier) ? Result.failVoid : Result.fromNullishOr(UndefinedOr.liftThrowable(findPackageJSON)(specifier, from), Function.constVoid)));
    const manifests = yield* Effect.forEach(Array.dedupe(found), (at) => fs.realPath(at), { concurrency: 'unbounded' });
    const read = yield* Effect.forEach(Array.dedupe(manifests), (at) => Effect.map(Effect.flatMap(fs.readFileString(at), Schema.decodeEffect(Named)), ({ name }) => (at.split(sep).includes('node_modules') ? Result.succeed(name) : Result.fail({ name, folder: dirname(at) }))), { concurrency: 'unbounded' });
    const [packages, linked] = Array.partition(read, identity);
    return { packages: Array.dedupe(packages), linked: Array.dedupe(linked.map(({ name }) => name)), folders: linked.map(({ folder }) => folder) };
});

const removable = (tools: Verbs) => Record.collect(tools, (verb, held) => Record.keys(Record.filter(held, ({ outputs = [] }) => outputs.length > 0)).map((tool) => `${verb}--${tool}`)).flat();

const aggregated = (verbs: Verbs, stated: Stated, technology: string, { workspaceRoot, holds }: Anchor, own: Readonly<Record<string, TargetConfiguration>> = {}): Record<string, TargetConfiguration> => {
    const unstated = ['default', '^default'];
    const held = (specifier: string, from: string) =>
        Option.liftThrowable(readModulePackageJson)(specifier, [from]).pipe(
            Option.map(({ path, packageJson }) => ({ packageJson, folder: dirname(path) })),
            Option.filter(Predicate.mapInput(Predicate.and(within, holds), ({ folder }) => normalizePath(relative(workspaceRoot, folder)))),
        );
    const provided = (target: TargetConfiguration): TargetConfiguration =>
        Option.fromUndefinedOr(target.executor).pipe(
            Option.map(parseExecutor),
            Option.flatMap(([provider]) => held(provider, workspaceRoot)),
            Option.map(({ packageJson, folder }) => [packageJson.name, ...pipe(Record.keys(packageJson.dependencies ?? {}), Array.filterMap(Filter.fromPredicateOption((name: string) => Option.map(held(name, folder), (linked) => linked.packageJson.name))))]),
            Option.match({ onNone: () => target, onSome: (projects) => ({ ...target, inputs: [...(target.inputs ?? unstated), { input: 'default', projects }] }) }),
        );
    return {
        ...pipe(
            verbs,
            Record.filter(Predicate.not(Record.isEmptyRecord)),
            Record.map((tools, verb) =>
                Record.collect(Record.map(tools, provided), (tool, target) => ({
                    name: `${verb}--${tool}`,
                    target: {
                        ...target,
                        ...(verb === 'clean' && { configurations: { ...target.configurations, cache: { targets: [], commands: [], folders: [], ...Option.getOrUndefined(Option.liftPredicate(Predicate.isObject)(target.configurations?.['cache'])) } } }),
                        metadata: { technologies: [technology], ...target.metadata },
                    },
                    inputs: (mergeTargetConfigurations(stated[`${verb}--${tool}`] ?? {}, { ...target, inputs: target.inputs ?? unstated }).inputs ?? []).map((entry) => (Predicate.hasProperty(entry, 'dependentTasksOutputFiles') ? { ...entry, transitive: true } : entry)),
                })),
            ),
            Record.collect((verb, rows): readonly (readonly [string, TargetConfiguration])[] => [[verb, { dependsOn: [{ target: `${verb}--*`, params: 'forward' }], inputs: ['...', ...Array.dedupe(rows.flatMap(Struct.get('inputs')))] }], ...rows.map(({ name, target }) => [name, target] as const)]),
            Array.flatten,
            Record.fromEntries,
        ),
        ...Record.map(own, provided),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    above,
    aggregated,
    type Anchor,
    anchored,
    batched,
    below,
    dried,
    EXEC,
    EXECUTORS,
    executor,
    extended,
    fingerprinted,
    inferring,
    inside,
    manifested,
    Named,
    namedInput,
    NxError,
    owned,
    packaged,
    parsed,
    type Pass,
    PLATFORM,
    recalled,
    removable,
    request,
    type Run,
    run,
    runner,
    spawned,
    Stated,
    stamped,
    tagged,
    tokened,
    type Verbs,
    Words,
    within,
};
