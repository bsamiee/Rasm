import { findPackageJSON } from 'node:module';
import { fileURLToPath } from 'node:url';
import { AggregateCreateNodesError, type CreateNodesResultArray, detectPackageManager, getPackageManagerCommand, hashArray, parseJson, type TargetConfiguration } from '@nx/devkit';
import { hashObject, hashWithWorkspaceContext, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, type ManagedRuntime, Option, Path, Record, Schema, type Scope } from 'effect';
import { runtime } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Verb = 'build' | 'check' | 'fix' | 'test' | 'install' | 'deploy' | 'upgrade' | 'clean';
type Members = { readonly [V in Verb]?: Readonly<Record<string, TargetConfiguration>> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';
const PLATFORM = { runtime: 'node -p process.platform+process.arch' } as const;
const UNSTATED = ['default', '^default'] as const satisfies NonNullable<TargetConfiguration['inputs']>;
const GENERATED = { dependsOn: ['build--buf', '^build--buf'], inputs: [{ dependentTasksOutputFiles: '**/*', transitive: true }] } as const satisfies Pick<TargetConfiguration, 'dependsOn' | 'inputs'>;

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String, version: Schema.String, dependencies: Dependencies, optionalDependencies: Dependencies }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const line = (argv: readonly string[]) => argv.map(quoteShellArg).join(' ');

const ancestors = (path: Path.Path, folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(path.dirname(at), () => at !== '.')] as const),
    );

const launcher = (workspaceRoot: string) => getPackageManagerCommand(detectPackageManager(workspaceRoot)).exec;

const aggregated = (members: Members, technologies: readonly string[], clean?: { readonly executor: string; readonly tool: string }): Readonly<Record<string, TargetConfiguration>> => {
    const named = (tools: Readonly<Record<string, Readonly<Record<string, TargetConfiguration>>>>) => Record.collect(tools, (verb, group) => Record.collect(group, (tool, target) => ({ verb, name: `${verb}--${tool}`, target }))).flat();
    const written = named(members)
        .filter(({ target }) => (target.outputs ?? []).length > 0)
        .map(({ name }) => name);
    const stated = clean === undefined ? undefined : members.clean?.[clean.tool];
    const cleaning: Members =
        clean === undefined || written.length === 0 ? {} : { clean: { ...members.clean, [clean.tool]: { metadata: { technologies: [...technologies], description: `Removes the outputs ${written.join(', ')} declare` }, ...stated, executor: clean.executor, options: { ...stated?.options, targets: written } } } };
    const bound = named({ ...members, ...cleaning }).map(({ verb, name, target }) => ({ verb, name, target: target.cache === true ? { ...target, inputs: Array.dedupe([...(target.inputs ?? UNSTATED), PLATFORM]) } : target }));
    return Record.fromEntries([
        ...bound.map(({ name, target }) => [name, target] as const),
        ...Record.collect(
            Array.groupBy(bound, ({ verb }) => verb),
            (verb, group): readonly [string, TargetConfiguration] => [verb, { executor: 'nx:noop', dependsOn: [`${verb}--*`], inputs: ['...', ...Array.dedupe(group.flatMap(({ target }) => target.inputs ?? UNSTATED))], metadata: { technologies: [...technologies], description: `Runs every ${verb}--* target` } }],
        ),
    ]);
};

const cached = Effect.fn('cached')(function* <V>(plugin: { readonly name: string }, options: unknown) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = path.basename(plugin.name);
    const file = `${base}-${hashObject({ options: options ?? {} })}.hash`;
    const held = new PluginCache<{ readonly value: V; readonly digest: string }>(path.join(workspaceDataDirectory, file));
    const live = new Map<string, { readonly value: V; readonly digest: string }>();
    const written = Effect.gen(function* () {
        yield* Effect.sync(() => new PluginCache(path.join(workspaceDataDirectory, file), Object.fromEntries(live), [...live.keys()]).writeToDisk());
        const stale = (yield* fs.readDirectory(workspaceDataDirectory)).filter((name) => name !== file && new RegExp(`^${RegExp.escape(base)}(-\\d+)?\\.hash$`, 'u').test(name));
        yield* Effect.forEach(stale, (name) => fs.remove(path.join(workspaceDataDirectory, name), { force: true }), { discard: true });
    });
    const changed = Effect.sync(() => [...live].some(([key, entry]) => held.get(key) !== entry));
    yield* Effect.addFinalizer(() => Effect.ignore(Effect.when(written, changed), { log: true, message: `Writing ${file}` }));
    return {
        get: Effect.fnUntraced(function* <R>(key: string, digest: (value: V) => Effect.Effect<string, never, R>) {
            const entry = yield* Effect.fromNullishOr(held.get(key));
            yield* Effect.filterOrFail(digest(entry.value), (now) => now === entry.digest);
            yield* Effect.sync(() => live.set(key, entry));
            return entry.value;
        }, Effect.catchNoSuchElement),
        set: (key: string, value: V, digest: string) => Effect.sync(() => live.set(key, { value, digest })),
    };
});

const packaged = Effect.fnUntraced(function* (module: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const file = yield* Effect.fromNullishOr(findPackageJSON(module));
    return { file, folder: path.dirname(file), ...(yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Manifest))) };
});

const identity = Effect.fn('identity')(function* (module: string, workspaceRoot: string) {
    const path = yield* Path.Path;
    const [plugin, shared] = yield* Effect.all([packaged(module), packaged(import.meta.url)]);
    const packages = [plugin.name, ...Record.keys(plugin.optionalDependencies), shared.name];
    const modules = (url: string, pattern: string) => path.join(path.relative(workspaceRoot, path.dirname(fileURLToPath(url))), `${pattern}${path.extname(url)}`);
    const source = yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [modules(module, '**/*'), modules(import.meta.url, '*')]));
    const external = Array.difference(Array.dedupe([plugin, shared].flatMap(({ dependencies }) => Record.keys(dependencies))), packages);
    const inputs: NonNullable<TargetConfiguration['inputs']> = plugin.file.split(path.sep).includes('node_modules') ? [{ externalDependencies: packages }] : [{ input: 'default', projects: packages }, { externalDependencies: external }];
    return { name: plugin.name, folder: plugin.folder, digest: hashArray([source, plugin.version, shared.version]), inputs, cleaner: `${shared.name}:clean` };
});

const detached = (nodes: CreateNodesResultArray) => parseJson<CreateNodesResultArray>(JSON.stringify(nodes));

const reported = (nodes: CreateNodesResultArray, failures: readonly (readonly [file: string, error: Error])[]) =>
    Array.match(failures, {
        onEmpty: () => Effect.succeed(nodes),
        onNonEmpty: (errors) =>
            Effect.fail(
                new AggregateCreateNodesError(
                    errors.map(([file, error]) => [file, error]),
                    detached(nodes),
                ),
            ),
    });

const inferred = <E extends Error>(nodes: Effect.Effect<CreateNodesResultArray, E, ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope>) => runtime.runPromise(Effect.scoped(Effect.map(nodes, detached)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { aggregated, ancestors, CONFIG, cached, GENERATED, identity, inferred, launcher, line, type Members, reported };
