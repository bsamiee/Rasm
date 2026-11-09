import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type ProjectConfiguration, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Result, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import manifest from '../package.json' with { type: 'json' };
import { CHECKERS, CONFIGS, declared, documents, targets } from './targets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = ReturnType<typeof report>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const FILES = `**/{${[LOCK, 'uv.toml', '.python-version', ...CONFIGS].join(',')}}`;
const { name } = manifest;

// --- [MODELS] --------------------------------------------------------------------------

const Local = Schema.Literals(['editable', 'virtual', 'directory', 'path']);
const Artifact = Schema.Struct({ hashes: Schema.Record(Schema.String, Schema.String) });
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    source: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String })),
    dependency_groups: Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    sdist: Schema.optionalKey(Schema.NullOr(Artifact)),
    wheels: Schema.optionalKey(Schema.NullOr(Schema.Array(Artifact))),
});
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        schema: Schema.Struct({ version: Schema.Literal('preview') }),
        workspace: Schema.Struct({ path: Schema.String, id: Schema.String }),
        members: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.String, id: Schema.String })),
        resolution: Schema.Record(Schema.String, Node),
    }),
);

// --- [ERRORS] --------------------------------------------------------------------------

class UvError extends Schema.TaggedError<UvError, { readonly brand: unique symbol }>()('UvError', { subject: Schema.String, detail: Schema.String }) {
    override get message() {
        return `${this.subject}: ${this.detail}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = (command: string, args: readonly string[], cwd: string) =>
    Effect.scoped(
        Effect.gen(function* () {
            const handle = yield* ChildProcess.make(command, args, { cwd, extendEnv: true, stdin: 'ignore' });
            const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
            return code === 0 ? stdout : yield* UvError.make({ subject: [command, ...args].join(' '), detail: `exited ${code} in ${cwd}: ${stderr.trim()}` });
        }),
    );

const report = (path: Path.Path, workspaceRoot: string, lock: string, metadata: typeof Metadata.Type, held: ReadonlyMap<string, unknown>) => {
    const { resolution } = metadata;
    const local = (node: typeof Node.Type) => Option.map(Record.findFirst(node.source, (at, key) => Schema.is(Local)(key) && Predicate.isString(at)), ([, at]) => path.relative(workspaceRoot, path.normalize(String(at))) || '.');
    const named = (node: typeof Node.Type) => (Option.isSome(local(node)) || node.name === undefined ? Option.none() : Option.some(`pypi:${node.name}@${node.version ?? ''}`));
    const external = (id: string) => Option.flatMap(Option.fromUndefinedOr(resolution[id]), named);
    const target = (id: string) => Option.orElse(external(id), () => Option.flatMap(Option.fromUndefinedOr(resolution[id]), local));
    const reach = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].flatMap((id) => (resolution[id]?.dependencies ?? []).map((dependency) => dependency.id))]);
        return next.size === ids.size ? ids : reach(next);
    };
    const closure = (start: string) => [...reach(new Set([start]))].flatMap((id) => Option.toArray(Option.fromUndefinedOr(resolution[id])));
    const members = metadata.members.map((member) => ({ ...member, root: path.relative(workspaceRoot, member.path) || '.' }));
    const workspace = path.relative(workspaceRoot, metadata.workspace.path) || '.';
    const workspaceGroups = resolution[Option.match(Array.findFirst(members, (member) => member.root === workspace), { onNone: () => metadata.workspace.id, onSome: ({ id }) => id })]?.dependency_groups ?? [];
    const locals = Array.dedupe(Record.values(resolution).flatMap((node) => Option.toArray(local(node))));
    return {
        lock,
        members: members.map((member) => {
            const own = resolution[member.id]?.dependency_groups ?? [];
            const groups = [...own, ...workspaceGroups.filter(({ name: group }) => !own.some((held) => held.name === group))];
            return {
                name: member.name,
                root: member.root,
                workspace,
                packaged: 'editable' in (resolution[member.id]?.source ?? {}),
                nested: members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`))),
                manifests: locals.map((root) => path.join(root, 'pyproject.toml')).toSorted(),
                checks: Record.getSomes(
                    Record.map(CHECKERS, (row, key) => {
                        const holds = (id: string) => closure(id).some((node) => node.name === row.distribution && Option.isNone(local(node)));
                        const selected = Option.orElse(
                            Option.map(
                                Array.findFirst(groups, ({ id }) => holds(id)),
                                ({ name: group, id }) => ({ group: Option.some(group), from: (resolution[id]?.dependencies ?? []).map((dependency) => dependency.id) }),
                            ),
                            () => (holds(member.id) ? Option.some({ group: Option.none<string>(), from: Array.empty<string>() }) : Option.none()),
                        );
                        return Option.flatMap(declared(path, held, member.root, key), (found) =>
                            Option.map(selected, ({ group, from }) => ({
                                ...found,
                                group: Option.getOrUndefined(group),
                                externals: Array.dedupe([
                                    ...Option.toArray(Option.flatMap(Option.fromUndefinedOr(closure(member.id).concat(...from.map(closure)).find((node) => node.name === row.distribution && Option.isNone(local(node)))), named)),
                                    ...from.flatMap((id) => Option.toArray(external(id))),
                                ]).toSorted(),
                                projects: Array.dedupe(from.flatMap((id) => (Option.isSome(external(id)) ? [] : Option.toArray(Option.fromUndefinedOr(resolution[id]?.name))))).toSorted(),
                            })),
                        );
                    }),
                ),
            };
        }),
        externalNodes: Record.map(
            Array.groupBy(
                Record.values(resolution).flatMap((node) => Option.toArray(Option.map(named(node), (key) => ({ key, node })))),
                ({ key }) => key,
            ),
            (group, key): ProjectGraphExternalNode => ({
                type: 'pypi',
                name: key,
                data: {
                    version: group[0].node.version ?? '',
                    packageName: group[0].node.name ?? '',
                    hash: hashArray([JSON.stringify(group[0].node.source), ...group.flatMap(({ node }) => [...Option.toArray(Option.fromNullishOr(node.sdist)), ...(node.wheels ?? [])].flatMap(({ hashes }) => Record.values(hashes))).toSorted()]),
                },
            }),
        ),
        implicit: Array.dedupe(
            members.flatMap((member) =>
                Record.keys(Record.filter(resolution, (node) => node.name === member.name && Option.contains(local(node), member.root))).flatMap((id) =>
                    (resolution[id]?.dependencies ?? []).flatMap((dependency) => Option.toArray(target(dependency.id)).filter((at) => at !== member.root).map((at) => [member.root, at] as const)),
                ),
            ),
        ),
        static: Array.dedupe(
            Record.keys(resolution).flatMap((id) =>
                Option.match(external(id), {
                    onNone: () => [],
                    onSome: (source) => (resolution[id]?.dependencies ?? []).flatMap((dependency) => Option.toArray(external(dependency.id)).filter((sink) => sink !== source).map((sink) => [source, sink] as const)),
                }),
            ),
        ),
    };
};

const built = new Map<string, readonly ReturnType<typeof report>[]>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: object, files: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const locks = files.filter((file) => path.basename(file) === LOCK).toSorted();
    const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module));
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])), ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))), hashObject(options), ...files.toSorted()]);
    const cache = new PluginCache<readonly ReturnType<typeof report>[]>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options)}.hash`));
    const analyzed = yield* Option.match(Option.fromNullishOr(cache.get(hash)), {
        onSome: (reports) => Effect.succeed({ reports, failures: [] }),
        onNone: Effect.fnUntraced(function* () {
            const results = yield* Effect.forEach(
                locks,
                (lock) =>
                    Effect.result(
                        Effect.flatMap(spawned('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata', '--quiet', '--locked'], path.join(workspaceRoot, path.dirname(lock))), (text) =>
                            Effect.mapError(Schema.decodeEffect(Metadata)(text), (error) => UvError.make({ subject: `uv workspace metadata for ${lock}`, detail: error.message })),
                        ),
                    ),
                { concurrency: 'unbounded' },
            );
            const located = Array.zip(locks, results).flatMap(([lock, result]) => (Result.isSuccess(result) ? [{ lock, metadata: result.success }] : []));
            const held = yield* documents(
                workspaceRoot,
                located.flatMap(({ metadata }) => metadata.members.map((member) => path.relative(workspaceRoot, member.path) || '.')),
            );
            const failures = Array.zip(locks, results).flatMap(([lock, result]) => (Result.isFailure(result) ? [[lock, result.failure] as const] : []));
            const reports = located.map(({ lock, metadata }) => report(path, workspaceRoot, lock, metadata, held));
            yield* failures.length === 0 ? Effect.sync(() => (cache.set(hash, reports), cache.writeToDisk())) : Effect.void;
            return { reports, failures };
        }),
    });
    built.set(`${workspaceRoot}:${hashObject(options)}`, analyzed.reports);
    return analyzed;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<object> = [
    FILES,
    (files, options = {}, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const path = yield* Path.Path;
                const { reports, failures } = yield* graph(context.workspaceRoot, options, files);
                const tested = new Set(
                    yield* Effect.forEach(
                        reports.flatMap(({ members }) => members.flatMap((member) => Option.toArray(Option.map(Option.fromUndefinedOr(member.checks.pytest), ({ patterns }) => ({ member, patterns }))))),
                        ({ member, patterns }) => Effect.map(Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, patterns.map((pattern) => path.join(member.root, '**', pattern)))), (found) => (found.length === 0 ? [] : [member.root])),
                        { concurrency: 'unbounded' },
                    ).pipe(Effect.map(Array.flatten)),
                );
                const nodes: CreateNodesResultArray = reports.map(({ lock, members, externalNodes }) => [
                    lock,
                    {
                        projects: Record.fromEntries(
                            members.map((member): readonly [string, Omit<ProjectConfiguration, 'root'>] => [
                                member.root,
                                {
                                    name: member.name,
                                    metadata: { technologies: ['python'] },
                                    targets: { ...targets(path, member, 'production' in getNamedInputs(member.root, context) ? '^production' : '^default', tested.has(member.root)) },
                                    ...(member.packaged ? { release: { version: { versionActions: `${name}/release/version-actions` } } } : {}),
                                },
                            ]),
                        ),
                        externalNodes: { ...externalNodes },
                    },
                ]);
                return failures.length === 0 ? nodes : yield* Effect.fail(new AggregateCreateNodesError(failures.map(([file, error]) => [file, new Error(error.message)]), nodes));
            }).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<object> = (options = {}, { workspaceRoot, projects, externalNodes = {} }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const resolved = (at: string) => (at in externalNodes ? Option.some(at) : Option.fromUndefinedOr(names.get(at)));
    return (built.get(`${workspaceRoot}:${hashObject(options)}`) ?? []).flatMap(({ implicit, static: fixed }): readonly RawProjectGraphDependency[] => [
        ...implicit.flatMap(([source, sink]) =>
            Option.toArray(Option.all([Option.fromUndefinedOr(names.get(source)), resolved(sink)])).flatMap(([from, to]): readonly RawProjectGraphDependency[] => (from === to ? [] : [{ source: from, target: to, type: DependencyType.implicit }])),
        ),
        ...fixed.filter(([source, sink]) => source in externalNodes && sink in externalNodes).map(([source, sink]): RawProjectGraphDependency => ({ source, target: sink, type: DependencyType.static })),
    ]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, type Member, name, spawned, UvError };
