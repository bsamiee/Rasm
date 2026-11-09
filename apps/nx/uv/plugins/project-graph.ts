import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, DependencyType, hashArray, logger, type ProjectConfiguration, type RawProjectGraphDependency, validateDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { major, minor, minSatisfying, validRange } from '@renovatebot/pep440';
import { Array, Effect, Equivalence, FileSystem, Formatter, flow, Option, Path, Predicate, Record, Schema, SchemaTransformation, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { CHECKERS, CONFIGS, declared, documents, targets } from '#/plugins/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = ReturnType<typeof report>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const PRIVATE = 'Private ::';
const RELEASE = 'release';
const FILES = `**/{${[LOCK, 'uv.toml', '.python-version', ...CONFIGS].join(',')}}`;
const SEPARATORS = /[-_.]+/gu;

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
const Sourced = Schema.Struct({ tool: Schema.Struct({ uv: Schema.Struct({ sources: Schema.Record(Schema.String, Schema.Unknown) }) }) });
const Local = Schema.Literals(['editable', 'virtual', 'directory', 'path']);
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    source: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ id }) => id, encode: (id) => ({ id }) })))),
    dependency_groups: Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
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

class UvError extends Schema.TaggedError<UvError, { readonly brand: unique symbol }>()('UvError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spawned = Effect.fnUntraced(function* (command: string, args: readonly string[], cwd: string) {
    const handle = yield* ChildProcess.make(command, args, { cwd, extendEnv: true, stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* UvError.make({ subject: [command, ...args].join(' '), detail: `exited ${code} in ${cwd}: ${stderr.trim()}` });
}, Effect.scoped);

const normalized = (name: string) => name.toLowerCase().replaceAll(SEPARATORS, '-');

const report = (path: Path.Path, workspaceRoot: string, lock: string, metadata: typeof Metadata.Type, documented: ReadonlyMap<string, unknown>, releases: readonly string[]) => {
    const { resolution } = metadata;
    const local = (node: typeof Node.Type) =>
        Option.map(
            Record.findFirst(node.source, (at, key) => Schema.is(Local)(key) && Predicate.isString(at)),
            ([, at]) => path.relative(workspaceRoot, path.normalize(String(at))) || '.',
        );
    const workspace = path.relative(workspaceRoot, metadata.workspace.path) || '.';
    const located = (id: string) => Option.flatMap(Option.fromUndefinedOr(resolution[id]), (node) => Option.all({ root: local(node), name: Option.fromUndefinedOr(node.name) }));
    const reach = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].flatMap((id) => resolution[id]?.dependencies ?? [])]);
        return next.size === ids.size ? ids : reach(next);
    };
    const closure = (start: string) => Array.getSomes([...reach(new Set([start]))].map((id) => Option.fromUndefinedOr(resolution[id])));
    const sourced = (root: string) => new Set(Option.match(Schema.decodeUnknownOption(Sourced)(documented.get(path.join(root, 'pyproject.toml'))), { onNone: () => [], onSome: ({ tool }) => Record.keys(tool.uv.sources).map(normalized) }));
    const manifest = (root: string) => Option.map(Schema.decodeUnknownOption(Declared)(documented.get(path.join(root, 'pyproject.toml'))), ({ project }) => project);
    const floor = (root: string) =>
        Option.flatMap(
            Option.flatMap(manifest(root), (project) => Option.fromUndefinedOr(project['requires-python'])),
            (requires) => Option.map(Option.fromNullishOr(validRange(requires) ? minSatisfying([...releases], requires) : null), (lowest) => `${major(lowest)}.${minor(lowest)}`),
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
    const locals = Array.dedupe(Array.getSomes(Object.values(resolution).map(local)));
    return {
        lock,
        members: members.map((member) => {
            const groups = Array.unionWith(resolution[member.id]?.dependency_groups ?? [], workspaceGroups, Equivalence.Struct({ name: Equivalence.String }));
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
                        const provides = (id: string) => Option.exists(Option.fromUndefinedOr(resolution[id]), distributed);
                        const group = Array.findFirst(groups, ({ id }) => (resolution[id]?.dependencies ?? []).some(provides));
                        const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
                        const grouped = { group: Option.getOrUndefined(group)?.name, projects: Array.dedupe(Array.getSomes(from.map(located)).map(({ name }) => name)).toSorted() };
                        return Option.map(
                            Option.filter(declared(path, documented, member.root, key), () =>
                                closure(member.id)
                                    .concat(...from.map(closure))
                                    .some(distributed),
                            ),
                            (project) => ({ ...project, ...grouped }),
                        );
                    }),
                ),
            };
        }),
        edges: members.flatMap((member) => {
            const own = sourced(member.root);
            return Object.values(Record.filter(resolution, (node) => node.name === member.name && Option.contains(local(node), member.root)))
                .flatMap((node) => Array.getSomes(node.dependencies.map(located)))
                .map(({ root, name }): RawProjectGraphDependency => (own.has(normalized(name)) ? { source: member.root, target: root, type: DependencyType.static, sourceFile: path.join(member.root, 'pyproject.toml') } : { source: member.root, target: root, type: DependencyType.implicit }));
        }),
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
                Effect.catchTag('SchemaError', (cause) => UvError.make({ subject: `uv workspace metadata for ${lock}`, detail: 'does not decode', cause })),
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
    const documented = yield* documents(
        workspaceRoot,
        read.flatMap(({ metadata }) => [metadata.workspace.path, ...metadata.members.map((member) => member.path)]).map((at) => path.relative(workspaceRoot, at) || '.'),
    );
    const releases = Option.isSome(cached)
        ? []
        : Array.dedupe(
              (yield* spawned('uv', ['python', 'list', 'cpython', '--only-downloads', '--all-versions', '--offline', '--output-format', 'json'], workspaceRoot).pipe(
                  Effect.flatMap(Schema.decodeEffect(Releases)),
                  Effect.catchTag('SchemaError', (cause) => UvError.make({ subject: 'uv python list', detail: 'does not decode', cause })),
              )).map(({ version }) => version),
          );
    const reports = Option.getOrElse(cached, () => read.map(({ lock, metadata }) => report(path, workspaceRoot, lock, metadata, documented, releases)));
    yield* Option.isNone(cached) && unlocked.length === 0
        ? Effect.sync(() => {
              cache.set(hash, reports);
              cache.writeToDisk();
          })
        : Effect.void;
    const globbed = yield* Effect.forEach(
        reports.flatMap(({ members }) => members).flatMap((member) => Object.values(member.checks)),
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
    flow(
        Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
            const settings = options ?? {};
            const [path, { outputPath }, { name, reports, present }] = yield* Effect.all([Path.Path, Schema.decodeEffect(Options)(settings), graph(context.workspaceRoot, settings, files)]).pipe(Effect.mapError((error) => new AggregateCreateNodesError([[null, error]], [])));
            return reports.map(({ lock, members }): CreateNodesResultArray[number] => [
                lock,
                {
                    projects: Record.fromIterableWith(members, (member): readonly [string, Omit<ProjectConfiguration, 'root'>] => [
                        member.root,
                        {
                            ...(member.name === undefined ? {} : { name: member.name }),
                            metadata: { technologies: ['python'] },
                            targets: { ...targets(path, member, name, 'production' in getNamedInputs(member.root, context) ? '^production' : '^default', present, outputPath) },
                            ...(member.published ? { tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } } : {}),
                        },
                    ]),
                },
            ]);
        }),
        Effect.provide(layer),
        Effect.runPromise,
    ),
];

const createDependencies: CreateDependencies<typeof Options.Encoded> = (options, context) => {
    const names = createProjectRootMappingsFromProjectConfigurations(context.projects);
    return Array.getSomes(
        (built.get(hashArray([context.workspaceRoot, hashObject(options ?? {})])) ?? [])
            .flatMap(({ edges }) => edges)
            .map((edge) =>
                Option.map(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))]), ([source, target]) => {
                    const dependency = { ...edge, source, target };
                    validateDependency(dependency, context);
                    return dependency;
                }),
            ),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, LOCK, type Member, normalized, spawned, UvError };
