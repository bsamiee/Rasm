import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesResultArray, DependencyType, hashArray, logger, type ProjectConfiguration, type ProjectGraphExternalNode, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { major, minor, minSatisfying, validRange } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Path, Predicate, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { CHECKERS, CONFIGS, declared, documents, targets } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = ReturnType<typeof report>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const PRIVATE = 'Private ::';
const RELEASE = 'release';
const FILES = `**/{${[LOCK, 'uv.toml', '.python-version', ...CONFIGS].join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) });
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
const Declared = Schema.Struct({
    project: Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        'requires-python': Schema.optionalKey(Schema.String),
        classifiers: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dynamic: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    }),
});
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
const Releases = Schema.fromJsonString(Schema.Array(Schema.Struct({ version: Schema.String })));
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

const report = (path: Path.Path, workspaceRoot: string, lock: string, metadata: typeof Metadata.Type, held: ReadonlyMap<string, unknown>, releases: readonly string[]) => {
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
    const manifest = (root: string) => Option.map(Schema.decodeUnknownOption(Declared)(held.get(path.join(root, 'pyproject.toml'))), ({ project }) => project);
    const floor = (root: string) =>
        Option.flatMap(
            Option.flatMap(manifest(root), (project) => Option.fromUndefinedOr(project['requires-python'])),
            (requires) =>
                Option.map(Option.fromNullishOr(validRange(requires) ? minSatisfying([...releases], requires) : null), (lowest) => `${major(lowest)}.${minor(lowest)}`),
        );
    const listed = metadata.members.map((member) => ({ ...member, root: path.relative(workspaceRoot, member.path) || '.' }));
    const members = listed.some((member) => member.root === workspace)
        ? listed
        : [
              ...listed,
              {
                  name: Option.getOrUndefined(Option.flatMap(manifest(workspace), (project) => Option.fromUndefinedOr(project.name))),
                  path: metadata.workspace.path,
                  id: metadata.workspace.id,
                  root: workspace,
              },
          ];
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
                published: packaged && Option.exists(manifest(member.root), ({ classifiers, dynamic }) => !(dynamic.includes('classifiers') || classifiers.some((classifier) => classifier.startsWith(PRIVATE)))),
                floor: Option.getOrUndefined(Option.filter(floor(member.root), (own) => !Option.contains(floor(workspace), own))),
                lock,
                nested: members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`))),
                manifests: Array.dedupe([workspace, ...locals])
                    .map((root) => path.join(root, 'pyproject.toml'))
                    .toSorted(),
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
    const [located, unlocked] = yield* Effect.partition(
        Option.isSome(cached) ? [] : locks,
        (lock) =>
            spawned('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata', '--quiet'], path.join(workspaceRoot, path.dirname(lock))).pipe(
                Effect.flatMap(Schema.decodeEffect(Metadata)),
                Effect.catchTag('SchemaError', (error) => UvError.make({ subject: `uv workspace metadata for ${lock}`, detail: error.message })),
                Effect.mapBoth({ onFailure: (error) => ({ lock, error }), onSuccess: (metadata) => ({ lock, metadata }) }),
            ),
        { concurrency: 'unbounded' },
    );
    yield* Effect.forEach(unlocked, ({ error }) => Effect.sync(() => logger.warn(`${error.message}; its root keeps its workspace targets alone until uv reads it`)));
    const read = [
        ...located,
        ...unlocked.map(({ lock }) => {
            const at = path.join(workspaceRoot, path.dirname(lock));
            return { lock, metadata: { schema: { version: 'preview' as const }, workspace: { path: at, id: `workspace+${at}` }, members: [], resolution: {} } };
        }),
    ];
    const held = yield* documents(
        workspaceRoot,
        read.flatMap(({ metadata }) => [metadata.workspace.path, ...metadata.members.map((member) => member.path)]).map((at) => path.relative(workspaceRoot, at) || '.'),
    );
    const releases = Option.isSome(cached)
        ? []
        : Array.dedupe(
              (yield* spawned('uv', ['python', 'list', 'cpython', '--only-downloads', '--all-versions', '--offline', '--output-format', 'json'], workspaceRoot).pipe(
                  Effect.flatMap(Schema.decodeEffect(Releases)),
                  Effect.catchTag('SchemaError', (error) => UvError.make({ subject: 'uv python list', detail: error.message })),
              )).map(({ version }) => version),
          );
    const reports = Option.getOrElse(cached, () => read.map(({ lock, metadata }) => report(path, workspaceRoot, lock, metadata, held, releases)));
    yield* Option.isNone(cached) && unlocked.length === 0
        ? Effect.sync(() => {
              cache.set(hash, reports);
              cache.writeToDisk();
          })
        : Effect.void;
    const globbed = yield* Effect.forEach(
        reports.flatMap(({ members }) => members).flatMap((member) => Record.values(member.checks)),
        (check) =>
            Effect.map(
                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [...check.patterns])),
                (matched) => [check, Array.isReadonlyArrayNonEmpty(matched)] as const,
            ),
        { concurrency: 'unbounded' },
    );
    built.set(hashArray([workspaceRoot, hashObject(options)]), reports);
    return { name, reports, present: new Set(globbed.filter(([, matched]) => matched).map(([check]) => check)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const settings = options ?? {};
                const [path, { outputPath }, { name, reports, present }] = yield* Effect.all([Path.Path, Schema.decodeEffect(Options)(settings), graph(context.workspaceRoot, settings, files)]).pipe(Effect.mapError((error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])));
                return reports.map(({ lock, members, externalNodes }): CreateNodesResultArray[number] => [
                    lock,
                    {
                        projects: Record.fromEntries(
                            members.map((member): readonly [string, Omit<ProjectConfiguration, 'root'>] => [
                                member.root,
                                {
                                    ...(member.name === undefined ? {} : { name: member.name }),
                                    metadata: { technologies: ['python'] },
                                    targets: { ...targets(path, member, name, 'production' in getNamedInputs(member.root, context) ? '^production' : '^default', present, outputPath) },
                                    ...(member.published ? { tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } } : {}),
                                },
                            ]),
                        ),
                        externalNodes: { ...externalNodes },
                    },
                ]);
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

export { createDependencies, createNodes, type Member, spawned, UvError };
