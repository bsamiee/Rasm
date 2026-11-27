import type { ProjectConfiguration, ProjectGraph, ProjectGraphProjectNode, TargetConfiguration, TargetDependencyConfig } from '@nx/devkit';
import { Array, Cache, Config, Duration, Effect, Exit, FileSystem, Glob, Match, Option, Path, Predicate, Record, Schema, Trie } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Stated<T> = { readonly [K in keyof T]?: Schema.Top };

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOKEN = /\{(?<token>[^{}]+)\}/u;
const UNRESOLVED = /\{(?:projectRoot|workspaceRoot|options.*)\}/u;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Record(Schema.String, Schema.Unknown);
const Dependency = Schema.Struct({ target: Schema.String, projects: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])), dependencies: Schema.optionalKey(Schema.Boolean) } satisfies Stated<TargetDependencyConfig>);
const Target = Schema.Struct({
    executor: Schema.String,
    options: Schema.optionalKey(Options),
    configurations: Schema.optionalKey(Schema.Record(Schema.String, Options)),
    defaultConfiguration: Schema.optionalKey(Schema.String),
    outputs: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    dependsOn: Schema.Union([Schema.String, Dependency]).pipe(Schema.Array, Schema.optionalKey),
    cache: Schema.optionalKey(Schema.Boolean),
    continuous: Schema.optionalKey(Schema.Boolean),
    metadata: Schema.optionalKey(Schema.Struct({ help: Schema.optionalKey(Schema.Struct({ command: Schema.String })) })),
} satisfies Stated<TargetConfiguration>);
const Node = Schema.Struct({
    name: Schema.String,
    data: Schema.Struct({ root: Schema.String, targets: Schema.optionalKey(Schema.Record(Schema.String, Target)) } satisfies Stated<ProjectConfiguration>),
} satisfies Stated<ProjectGraphProjectNode>);
const Cached = Schema.fromJsonString(Schema.Struct({ nodes: Schema.Record(Schema.String, Node), errors: Schema.optionalKey(Schema.Array(Schema.Unknown).check(Schema.isMaxLength(0))) } satisfies Stated<ProjectGraph & { readonly errors: unknown }>)).annotate({
    description: 'The project graph Nx caches in its workspace data directory, read as far as task rows need and served only where Nx recorded no error building it',
});
class Run extends Schema.Class<Run, { readonly brand: unique symbol }>('Run')(
    {
        command: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])),
        commands: Schema.Union([Schema.String, Schema.Struct({ command: Schema.String })]).pipe(Schema.Array, Schema.optionalKey),
        cwd: Schema.optionalKey(Schema.String),
    },
    { description: 'The options of an nx:run-commands task naming the shell lines it runs and the folder it runs them in' },
) {
    get lines() {
        return this.command === undefined ? (this.commands ?? []).map((each) => (Predicate.isString(each) ? each : each.command)) : [Array.ensure(this.command).join(' ')];
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const valued = (data: Readonly<Record<string, unknown>>, token: string) => {
    const [name = '', ...keys] = token.split('.');
    const value = keys.reduce((held, key) => (Predicate.isObjectOrArray(held) && Object.hasOwn(held, key) ? Reflect.get(held, key) : undefined), Object.hasOwn(data, name) ? data[name] : undefined);
    return Option.fromNullishOr(value).pipe(
        Option.filter((held) => held !== ''),
        Option.map((held) => (token === 'projectRoot' && held === '.' ? '' : globalThis.String(held))),
        Option.getOrElse(() => `{${token}}`),
    );
};

const interpolated = (posix: Path.Path, data: Readonly<Record<string, unknown>>) => (output: string) => {
    const body = output.replace(/^!/u, '');
    const segments = body.split(TOKEN).reduce<readonly string[]>(
        (held, part, at) => {
            const [head = '', ...tail] = at % 2 === 0 ? part.split('/') : [valued(data, part)];
            return [...held.slice(0, -1), `${held.at(-1) ?? ''}${head}`, ...tail];
        },
        [''],
    );
    const path = posix.join(...segments).replace('{workspaceRoot}/', '');
    return Option.liftPredicate(UNRESOLVED.test(path) ? [] : [output === body ? path : `!${path}`], () => body.startsWith('{') && !body.includes('{workspaceRoot}', 1));
};

const addressed = Effect.fnUntraced(function* (posix: Path.Path, { name, data }: typeof Node.Type, target: string, stated: typeof Target.Type, configuration: Option.Option<string>) {
    const { configurations = {}, outputs = [], dependsOn = [] } = stated;
    const options = { ...stated.options, ...configurations[Option.getOrElse(configuration, () => stated.defaultConfiguration ?? '')] };
    const resolved = Option.all(outputs.map(interpolated(posix, { projectRoot: data.root, projectName: name, project: { ...data, name }, options })));
    const globs = dependsOn.flatMap((entry) => (Predicate.isString(entry) ? [entry] : Option.toArray(Option.liftPredicate(entry.target, () => (entry.projects === undefined ? entry.dependencies !== true : Array.ensure(entry.projects).includes(name)))))).map((pattern) => Glob.make(pattern));
    const members = Record.keys(data.targets ?? {}).filter((own) => own !== target && globs.some((glob) => Glob.match(glob, own)));
    const runs = yield* Match.value(stated.executor).pipe(
        Match.when('nx:noop', () => Effect.succeed({ _tag: 'Aggregate', members } as const)),
        Match.when('nx:run-commands', () => Effect.map(Schema.decodeEffect(Run)(options), (run) => ({ _tag: 'Commands', cwd: run.cwd ?? '.', lines: run.lines }) as const)),
        Match.orElse(() => Effect.succeed({ _tag: 'Executor' } as const)),
    );
    return { project: name, target, configuration, root: data.root, options, help: Option.fromUndefinedOr(stated.metadata?.help?.command), cached: stated.cache ?? false, continuous: stated.continuous ?? false, runs, outputs: Array.dedupe(Option.getOrElse(resolved, () => []).flat()) };
});

const graphs = Effect.runSync(
    Cache.makeWith(
        Effect.fnUntraced(function* ([root, file]: readonly [root: string, file: string, modified: number, size: bigint]) {
            const [fs, path, posix] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.provide(Effect.service(Path.Path), Path.layer)]);
            const { nodes } = yield* Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Cached));
            const targets = Record.values(nodes).flatMap((node) => Record.collect(node.data.targets ?? {}, (target, stated) => ({ node, target, stated })));
            const addresses = targets.flatMap(({ node, target, stated }) => [Option.none<string>(), ...Record.keys(stated.configurations ?? {}).map(Option.some)].map((configuration) => [node, target, stated, configuration] as const));
            const rows = yield* Effect.forEach(addresses, (address) => addressed(posix, ...address));
            const roots = Trie.fromIterable(Record.values(nodes).map(({ name, data }) => [path.join(root, data.root, '/'), name] as const));
            return {
                tasks: new Map(rows.map((row) => [[row.project, row.target, ...Option.toArray(row.configuration)].join(':'), row])),
                owner: (at: string) => Option.map(Trie.longestPrefixOf(roots, path.join(path.resolve(root, at), '/')), ([, project]) => project),
            };
        }),
        { capacity: 1, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero), requireServicesAt: 'lookup' },
    ),
);

const tasks = Effect.fnUntraced(function* (root: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* Config.String('NX_WORKSPACE_DATA_DIRECTORY').pipe(
        Config.orElse(() => Config.String('NX_PROJECT_GRAPH_CACHE_DIRECTORY')),
        Config.withDefault('.nx/workspace-data'),
    );
    const file = path.resolve(root, folder, 'project-graph.json');
    const { mtime, size } = yield* fs.stat(file);
    return yield* Cache.get(graphs, [root, file, Option.match(mtime, { onNone: () => 0, onSome: (date) => date.getTime() }), size]);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { tasks };
