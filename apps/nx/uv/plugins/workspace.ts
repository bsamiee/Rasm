import { findPackageJSON } from 'node:module';
import { parse } from '@decimalturn/toml-patch';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, type ProjectConfiguration, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { CHECKERS, CONFIGS, declared, documents, targets } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = ReturnType<typeof report>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const EXTENSION = 'blender_manifest.toml';
const PRIVATE = 'Private ::';
const RELEASE = 'release';
const FILES = `**/{${[LOCK, 'uv.toml', '.python-version', EXTENSION, ...CONFIGS].join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) });
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
const Classified = Schema.Struct({ project: Schema.Struct({ classifiers: Schema.Array(Schema.String) }) });
const Extension = Schema.Struct({ id: Schema.String, version: Schema.String, platforms: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });
const Local = Schema.Literals(['editable', 'virtual', 'directory', 'path']);
const Artifact = Schema.Struct({ hashes: Schema.Record(Schema.String, Schema.String) });
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    source: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ id }) => id, encode: (id) => ({ id }) })))),
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

const report = (path: Path.Path, workspaceRoot: string, lock: string, metadata: typeof Metadata.Type, held: ReadonlyMap<string, unknown>, extensions: ReadonlyMap<string, typeof Extension.Type>) => {
    const { resolution } = metadata;
    const local = (node: typeof Node.Type) =>
        Option.map(
            Record.findFirst(node.source, (at, key) => Schema.is(Local)(key) && Predicate.isString(at)),
            ([, at]) => path.relative(workspaceRoot, path.normalize(String(at))) || '.',
        );
    const workspace = path.relative(workspaceRoot, metadata.workspace.path) || '.';
    const named = (node: typeof Node.Type) => (Option.isSome(local(node)) || node.name === undefined ? Option.none() : Option.some(`pypi:${path.join(workspace, node.name)}@${node.version ?? ''}`));
    const external = (id: string) => Option.flatMap(Option.fromUndefinedOr(resolution[id]), named);
    const target = (id: string) => Option.orElse(external(id), () => Option.flatMap(Option.fromUndefinedOr(resolution[id]), local));
    const reach = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].flatMap((id) => resolution[id]?.dependencies ?? [])]);
        return next.size === ids.size ? ids : reach(next);
    };
    const closure = (start: string) => [...reach(new Set([start]))].flatMap((id) => Option.toArray(Option.fromUndefinedOr(resolution[id])));
    const members = metadata.members.map((member) => ({ ...member, root: path.relative(workspaceRoot, member.path) || '.' }));
    const workspaceGroups =
        resolution[
            Option.match(
                Array.findFirst(members, (member) => member.root === workspace),
                { onNone: () => metadata.workspace.id, onSome: ({ id }) => id },
            )
        ]?.dependency_groups ?? [];
    const locals = Array.dedupe(Record.values(resolution).flatMap((node) => Option.toArray(local(node))));
    return {
        lock,
        members: members.map((member) => {
            const groups = Array.unionWith(resolution[member.id]?.dependency_groups ?? [], workspaceGroups, (own, shared) => own.name === shared.name);
            const packaged = 'editable' in (resolution[member.id]?.source ?? {});
            return {
                name: member.name,
                root: member.root,
                workspace,
                packaged,
                published: packaged && !Option.exists(Schema.decodeUnknownOption(Classified)(held.get(path.join(member.root, 'pyproject.toml'))), ({ project }) => project.classifiers.some((classifier) => classifier.startsWith(PRIVATE))),
                extension: extensions.get(member.root),
                nested: members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`))),
                manifests: locals.map((root) => path.join(root, 'pyproject.toml')).toSorted(),
                checks: Record.getSomes(
                    Record.map(CHECKERS, (_, key) => {
                        const distributed = (node: typeof Node.Type) => node.name === key && Option.isNone(local(node));
                        const group = Array.findFirst(groups, ({ id }) => closure(id).some(distributed));
                        const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
                        const projects = Array.dedupe(from.flatMap((id) => (Option.isSome(external(id)) ? [] : Option.toArray(Option.fromUndefinedOr(resolution[id]?.name))))).toSorted();
                        return Option.map(Option.all([declared(path, held, member.root, key), Array.findFirst(closure(member.id).concat(...from.map(closure)), distributed)]), ([found, tool]) => ({
                            ...found,
                            group: Option.getOrUndefined(group)?.name,
                            externals: Array.dedupe([...Option.toArray(named(tool)), ...Array.getSomes(from.map(external))]).toSorted(),
                            projects,
                        }));
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
                Record.values(Record.filter(resolution, (node) => node.name === member.name && Option.contains(local(node), member.root)))
                    .flatMap((node) => Array.getSomes(node.dependencies.map(target)))
                    .filter((at) => at !== member.root)
                    .map((at) => [member.root, at] as const),
            ),
        ),
        static: Array.dedupe(
            Record.values(resolution).flatMap((node) =>
                Option.match(named(node), {
                    onNone: () => [],
                    onSome: (source) =>
                        Array.getSomes(node.dependencies.map(external))
                            .filter((sink) => sink !== source)
                            .map((sink) => [source, sink] as const),
                }),
            ),
        ),
    };
};

const built = new Map<string, readonly ReturnType<typeof report>[]>();

const graph = Effect.fnUntraced(function* (workspaceRoot: string, options: object, files: readonly string[]) {
    const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const locks = files.filter((file) => path.basename(file) === LOCK).toSorted();
    const { name } = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url)).pipe(Effect.flatMap(fs.readFileString), Effect.flatMap(Schema.decodeEffect(Manifest)));
    const modules = (yield* fs.readDirectory(import.meta.dirname)).toSorted().map((module) => path.join(import.meta.dirname, module));
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])), ...(yield* Effect.forEach(modules, (module) => fs.readFileString(module))), hashObject(options), ...files.toSorted()]);
    const cache = new PluginCache<readonly ReturnType<typeof report>[]>(path.join(workspaceDataDirectory, `${path.basename(name)}-${hashObject(options)}.hash`));
    const cached = Option.fromNullishOr(cache.get(hash));
    const [extensions, invalid] = yield* Effect.partition(
        Option.isSome(cached) ? [] : files.filter((file) => path.basename(file) === EXTENSION),
        (at) =>
            fs.readFileString(path.join(workspaceRoot, at)).pipe(
                Effect.flatMap((text) => Effect.try({ try: (): unknown => parse(text), catch: (cause) => UvError.make({ subject: at, detail: String(cause) }) })),
                Effect.flatMap(Schema.decodeUnknownEffect(Extension)),
                Effect.mapBoth({ onFailure: (error) => [at, UvError.make({ subject: at, detail: error.message })] as const, onSuccess: (extension) => [path.dirname(at), extension] as const }),
            ),
        { concurrency: 'unbounded' },
    );
    const [located, unlocked] = yield* Effect.partition(
        Option.isSome(cached) ? [] : locks,
        (lock) =>
            spawned('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata', '--quiet', '--locked'], path.join(workspaceRoot, path.dirname(lock))).pipe(
                Effect.flatMap(Schema.decodeEffect(Metadata)),
                Effect.catchTag('SchemaError', (error) => UvError.make({ subject: `uv workspace metadata for ${lock}`, detail: error.message })),
                Effect.mapBoth({ onFailure: (error) => [lock, error] as const, onSuccess: (metadata) => ({ lock, metadata }) }),
            ),
        { concurrency: 'unbounded' },
    );
    const held = yield* documents(
        workspaceRoot,
        located.flatMap(({ metadata }) => metadata.members).map((member) => path.relative(workspaceRoot, member.path) || '.'),
    );
    const reports = Option.getOrElse(cached, () => located.map(({ lock, metadata }) => report(path, workspaceRoot, lock, metadata, held, new Map(extensions))));
    const failures = [...unlocked, ...invalid];
    yield* Option.isNone(cached) && failures.length === 0
        ? Effect.sync(() => {
              cache.set(hash, reports);
              cache.writeToDisk();
          })
        : Effect.void;
    built.set(hashArray([workspaceRoot, hashObject(options)]), reports);
    return { name, reports, failures };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const settings = options ?? {};
                const [path, { outputPath }, { name, reports, failures }] = yield* Effect.all([Path.Path, Schema.decodeEffect(Options)(settings), graph(context.workspaceRoot, settings, files)]).pipe(Effect.mapError((error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])));
                const found = new Map(
                    yield* Effect.forEach(
                        reports.flatMap(({ members }) => members),
                        (member) =>
                            Effect.map(
                                Effect.filter(
                                    Record.toEntries(member.checks),
                                    ([, { patterns }]) =>
                                        Effect.map(
                                            Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [...patterns])),
                                            Array.isReadonlyArrayNonEmpty,
                                        ),
                                    { concurrency: 'unbounded' },
                                ),
                                (held) => [member, new Set(held.map(([key]) => key))] as const,
                            ),
                        { concurrency: 'unbounded' },
                    ),
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
                                    targets: { ...targets(path, member, name, 'production' in getNamedInputs(member.root, context) ? '^production' : '^default', found.get(member) ?? new Set(), outputPath) },
                                    ...(member.published || member.extension !== undefined ? { tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { manifest: member.extension === undefined ? 'pyproject.toml' : EXTENSION } } } } : {}),
                                },
                            ]),
                        ),
                        externalNodes: { ...externalNodes },
                    },
                ]);
                return failures.length === 0
                    ? nodes
                    : yield* Effect.fail(
                          new AggregateCreateNodesError(
                              failures.map(([file, error]) => [file, new Error(error.message)]),
                              nodes,
                          ),
                      );
            }).pipe(Effect.provide(layer)),
        ),
];

const createDependencies: CreateDependencies<typeof Options.Encoded> = (options, { workspaceRoot, projects, externalNodes = {} }) => {
    const names = createProjectRootMappingsFromProjectConfigurations(projects);
    const resolved = (at: string) => (at in externalNodes ? Option.some(at) : Option.fromUndefinedOr(names.get(at)));
    return (built.get(hashArray([workspaceRoot, hashObject(options ?? {})])) ?? []).flatMap(({ implicit, static: fixed }): readonly RawProjectGraphDependency[] => [
        ...implicit.flatMap(([source, sink]) => Option.toArray(Option.all([Option.fromUndefinedOr(names.get(source)), resolved(sink)])).flatMap(([from, to]): readonly RawProjectGraphDependency[] => (from === to ? [] : [{ source: from, target: to, type: DependencyType.implicit }]))),
        ...fixed.filter(([source, sink]) => source in externalNodes && sink in externalNodes).map(([source, sink]): RawProjectGraphDependency => ({ source, target: sink, type: DependencyType.static })),
    ]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, EXTENSION, type Member, spawned, UvError };
