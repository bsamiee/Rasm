import { type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, DependencyType, hashArray, logger, type ProjectConfiguration, type RawProjectGraphDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, getNamedInputs, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext } from '@nx/devkit/internal';
import { cached, identity, inferred, reported } from '@rasm/nx-devkit/inference';
import { present, printed, ToolError } from '@rasm/nx-devkit/process';
import { major, minor, minSatisfying, validRange } from '@renovatebot/pep440';
import { Array, Effect, Equivalence, Option, Path, Predicate, Record, Result, Schema, SchemaTransformation } from 'effect';
import { CHECKERS, CONFIGS, declared, documents, PIXI, targets } from '#/plugins/targets';

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
const Declared = Schema.Struct({
    project: Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        'requires-python': Schema.optionalKey(Schema.String),
        classifiers: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dynamic: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
});
const Sourced = Schema.Struct({ tool: Schema.Struct({ uv: Schema.Struct({ sources: Schema.Record(Schema.String, Schema.Unknown) }) }) });
const Pixi = Schema.Struct({ tool: Schema.Struct({ pixi: Schema.Record(Schema.String, Schema.Unknown) }) });
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

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    return {
        lock,
        members: members.map((member) => {
            const groups = Array.unionWith(resolution[member.id]?.dependency_groups ?? [], workspaceGroups, Equivalence.Struct({ name: Equivalence.String }));
            const packaged = 'editable' in (resolution[member.id]?.source ?? {});
            const own = Option.getOrUndefined(floor(member.root));
            return {
                name: member.name,
                root: member.root,
                workspace,
                packaged,
                pixi: documented.has(path.join(workspace, PIXI)) || Schema.is(Pixi)(documented.get(path.join(workspace, 'pyproject.toml'))),
                scripted: packaged && Option.exists(manifest(member.root), ({ scripts }) => !Record.isEmptyRecord(scripts)),
                python: Option.getOrUndefined(Option.flatMap(manifest(member.root), (project) => Option.fromUndefinedOr(project['requires-python']))),
                published: packaged && Option.exists(manifest(member.root), ({ classifiers, dynamic }) => !(dynamic.includes('classifiers') || classifiers.some((classifier) => classifier.startsWith(PRIVATE)))),
                lock,
                nested: members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`))),
                checks: Record.getSomes(
                    Record.map(CHECKERS, (_, key) => {
                        const distributed = (node: typeof Node.Type) => node.name === key && Option.isNone(local(node));
                        const provides = (id: string) => Option.exists(Option.fromUndefinedOr(resolution[id]), distributed);
                        const group = Array.findFirst(groups, ({ id }) => (resolution[id]?.dependencies ?? []).some(provides));
                        const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
                        const grouped = { group: Option.getOrUndefined(group)?.name, projects: Array.dedupe(Array.getSomes(from.map(located)).map(({ name }) => name)).toSorted() };
                        const locked = closure(member.id)
                            .concat(...from.map(closure))
                            .some(distributed);
                        return Option.map(declared(path, documented, member.root, key), (project) => ({
                            ...project,
                            ...grouped,
                            locked,
                            version: Option.getOrUndefined(floor(locked ? workspace : path.dirname(project.config))) === own ? undefined : own,
                        }));
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
    const path = yield* Path.Path;
    const locks = files.filter((file) => path.basename(file) === LOCK).toSorted();
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const hash = hashArray([yield* Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [FILES])), plugin.digest, ...files.toSorted()]);
    const cache = yield* cached<readonly ReturnType<typeof report>[]>(plugin, options);
    const held = yield* cache.get(FILES, () => Effect.succeed(hash));
    const installed = Option.isSome(held) || Option.isSome(yield* present('uv', ['--version']));
    const [located, unlocked] = yield* Effect.partition(
        Option.isSome(held) || !installed ? [] : locks,
        (lock) =>
            printed('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: path.join(workspaceRoot, path.dirname(lock)) }).pipe(
                Effect.flatMap(Schema.decodeEffect(Metadata)),
                Effect.mapBoth({ onFailure: (error) => [lock, ToolError.of(`uv workspace metadata for ${lock}`, 'does not decode')(error)] as const, onSuccess: (metadata) => ({ lock, metadata }) }),
            ),
        { concurrency: 'unbounded' },
    );
    yield* Effect.sync(() => {
        if (!installed && locks.length > 0) {
            logger.warn(`uv is absent on this host, so ${locks.join(', ')} keep their workspace targets alone, and their runs report the missing tool`);
        }
    });
    const read = [
        ...located,
        ...(installed ? [] : locks).map((lock) => {
            const at = path.join(workspaceRoot, path.dirname(lock));
            return { lock, metadata: { schema: { version: 'preview' as const }, workspace: { path: at, id: `workspace+${at}` }, members: [], resolution: {} } };
        }),
    ];
    const { documented, unparsed } = yield* documents(
        workspaceRoot,
        read.flatMap(({ metadata }) => [metadata.workspace.path, ...metadata.members.map((member) => member.path)]).map((at) => path.relative(workspaceRoot, at) || '.'),
    );
    const listed =
        Option.isSome(held) || !installed
            ? Result.succeed([])
            : yield* Effect.result(printed('uv', ['python', 'list', 'cpython', '--only-downloads', '--all-versions', '--offline', '--output-format', 'json'], { cwd: workspaceRoot }).pipe(Effect.flatMap(Schema.decodeEffect(Releases)), Effect.mapError(ToolError.of('uv python list', 'does not decode'))));
    const releases = Array.dedupe(Result.getOrElse(listed, () => []).map(({ version }) => version));
    const reports = Option.getOrElse(held, () => read.map(({ lock, metadata }) => report(path, workspaceRoot, lock, metadata, documented, releases)));
    const failures = [...unlocked, ...unparsed, ...Array.zip(locks, Option.toArray(Result.getFailure(listed)))];
    yield* Option.isNone(held) && installed && failures.length === 0 ? cache.set(FILES, reports, hash) : Effect.void;
    const members = reports.flatMap((workspace) => workspace.members);
    const globbed = yield* Effect.forEach(
        members.flatMap(({ root, checks }) => Object.values(checks).map((check) => [root, check] as const)),
        ([root, check]) =>
            Effect.map(
                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [...check.patterns])),
                (matched) => [check, matched.includes(path.join(root, '__init__.py')) ? path.dirname(root) : root, Array.isReadonlyArrayNonEmpty(matched)] as const,
            ),
        { concurrency: 'unbounded' },
    );
    built.set(hashArray([workspaceRoot, hashObject(options)]), reports);
    return { name: plugin.name, cleaner: plugin.cleaner, reports, roots: new Map(members.map((member) => [member.root, member])), found: new Map(globbed.filter(([, , matched]) => matched).map(([check, base]) => [check, base])), failures };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, context: CreateNodesContext) =>
        inferred(
            Effect.gen(function* () {
                const settings = options ?? {};
                const [path, { outputPath }, { name, cleaner, reports, roots, found, failures }] = yield* Effect.all([Path.Path, Schema.decodeEffect(Options)(settings), graph(context.workspaceRoot, settings, files)]);
                return yield* reported(
                    reports.map(({ lock, members }): CreateNodesResultArray[number] => [
                        lock,
                        {
                            projects: Record.fromIterableWith(members, (member): readonly [string, Omit<ProjectConfiguration, 'root'>] => [
                                member.root,
                                {
                                    ...(member.name === undefined ? {} : { name: member.name }),
                                    metadata: { technologies: ['python'] },
                                    targets: { ...targets(path, member, roots, name, cleaner, 'production' in getNamedInputs(member.root, context) ? '^production' : '^default', found, outputPath) },
                                    ...(member.published ? { tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } } : {}),
                                },
                            ]),
                        },
                    ]),
                    failures,
                );
            }),
        ),
];

const createDependencies: CreateDependencies<typeof Options.Encoded> = (options, context) => {
    const names = createProjectRootMappingsFromProjectConfigurations(context.projects);
    return Array.getSomes(
        (built.get(hashArray([context.workspaceRoot, hashObject(options ?? {})])) ?? []).flatMap(({ edges }) => edges).map((edge) => Option.map(Option.all([Option.fromUndefinedOr(names.get(edge.source)), Option.fromUndefinedOr(names.get(edge.target))]), ([source, target]) => ({ ...edge, source, target }))),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, LOCK, type Member, normalized };
