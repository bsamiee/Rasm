import { join, posix, relative } from 'node:path';
import { normalizePath } from '@nx/devkit';
import { combineGlobPatterns, globWithWorkspaceContext } from '@nx/devkit/internal';
import { above } from '@rasm/nx';
import { Array, Cause, Effect, Equivalence, FileSystem, Graph, Option, Predicate, Record, Result, Schema, SchemaTransformation } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import { CHECKERS, CONFIGS, declared, PIXI } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Effect.Success<ReturnType<typeof located>>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const SCHEMA = 'preview';
const MANIFEST = 'pyproject.toml';
const LOADERS: ReadonlySet<string> = new Set(Object.values(CHECKERS).flatMap(({ plugins }) => plugins ?? []));
const DECODED = combineGlobPatterns(CONFIGS.map((file) => `**/${file}`));

// --- [MODELS] --------------------------------------------------------------------------

const Declared = Schema.Struct({
    project: Schema.Struct({
        'requires-python': Schema.optionalKey(Schema.String),
        classifiers: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dynamic: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        'entry-points': Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.String)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
});
const Indexed = Schema.Struct({ tool: Schema.Struct({ uv: Schema.Struct({ index: Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String), 'publish-url': Schema.optionalKey(Schema.String) })) }) }) });
const Pixi = Schema.Struct({ tool: Schema.Struct({ pixi: Schema.Record(Schema.String, Schema.Unknown) }) });
const Local = Schema.Literals(['editable', 'virtual', 'directory', 'path']);
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    source: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ id }) => id, encode: (id) => ({ id }) })))),
    dependency_groups: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })))),
});
class Metadata extends Schema.Opaque<Metadata, unknown>()(
    Schema.fromJsonString(
        Schema.Struct({
            schema: Schema.Struct({ version: Schema.Literal(SCHEMA) }),
            workspace: Schema.Struct({ path: Schema.String, id: Schema.String }),
            members: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.String, id: Schema.String })),
            resolution: Schema.Record(Schema.String, Node),
        }),
    ),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const normalized = (name: string) => name.toLowerCase().replaceAll(/[-_.]+/gu, '-');

const located = Effect.fnUntraced(function* (workspaceRoot: string, lock: string, metadata: Pick<Metadata, 'workspace' | 'members' | 'resolution'>) {
    const fs = yield* FileSystem.FileSystem;
    const { resolution } = metadata;
    const files = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [DECODED])));
    const rooted = (at: string) => normalizePath(relative(workspaceRoot, at)) || '.';
    const sited = Record.getSomes(
        Record.map(resolution, ({ source }) =>
            Option.map(
                Record.findFirst(source, (at, key): at is string => Schema.is(Local)(key) && Predicate.isString(at)),
                ([, at]) => rooted(at),
            ),
        ),
    );
    const grouped = new Set(Record.values(resolution).flatMap(({ dependency_groups }) => (dependency_groups ?? []).map(({ id }) => id)));
    const owned = Array.groupBy(
        Record.toEntries(sited).filter(([id]) => !grouped.has(id)),
        ([, root]) => root,
    );
    const workspace = rooted(metadata.workspace.path);
    const members = metadata.members.map((member) => ({ ...member, root: rooted(member.path) }));
    const present = Array.cartesianWith(Array.dedupe([workspace, ...members.map(({ root }) => root), ...Record.values(sited)].flatMap(above)), CONFIGS, posix.join).filter((at) => files.has(at));
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [at, posix.extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    const documented = new Map(read.map(([at, parsed]) => [at, Result.getOrElse(parsed, Record.empty)] as const));
    const stated = (root: string) => documented.get(posix.join(root, MANIFEST));
    const entries = Record.toEntries(resolution);
    const indexed = new Map(entries.map(([id], index) => [id, index] as const));
    const indices = (ids: Iterable<string>) => Array.getSomes(Array.fromIterable(ids).map((id) => Option.fromUndefinedOr(indexed.get(id))));
    const graph = Graph.fromSnapshot({ type: 'directed', nodes: entries.map(([data], index) => ({ index, data })), edges: entries.flatMap(([, { dependencies }], source) => indices(dependencies).map((target) => ({ source, target, data: undefined }))).map((edge, index) => ({ ...edge, index })) });
    const reach = (ids: Iterable<string>): ReadonlySet<string> => new Set(Graph.values(Graph.dfs(graph, { start: indices(ids) })));
    const resolved = (id: string) => Option.all({ root: Record.get(sited, id), name: Option.fromUndefinedOr(resolution[id]?.name) });
    const named = (ids: readonly string[]) => Array.dedupe(Array.getSomes(ids.map(resolved)).map(({ name }) => name)).toSorted();
    const distributed = (name: string) => (id: string) => resolution[id]?.name === name && !Record.has(sited, id);
    const offering =
        (name: string) =>
        ({ id }: { readonly id: string }) =>
            (resolution[id]?.dependencies ?? []).some(distributed(name));
    const declaring = (id: string) => Option.flatMap(Record.get(sited, id), (root) => Option.map(Schema.decodeUnknownOption(Declared)(stated(root)), ({ project }) => project));
    const loaded = (group: string | undefined) => (id: string) => Option.match(declaring(id), { onNone: () => true, onSome: (project) => Record.isEmptyRecord(project['entry-points']) || Record.keys(project['entry-points']).some((kind) => kind === group || !LOADERS.has(kind)) });
    const packages = new Set(
        yield* Effect.filter(
            members.map(({ root }) => root),
            (root) => fs.exists(join(workspaceRoot, root, '__init__.py')),
            { concurrency: 'unbounded' },
        ),
    );
    const shared =
        resolution[
            Array.findFirst(members, ({ root }) => root === workspace).pipe(
                Option.map(({ id }) => id),
                Option.getOrElse(() => metadata.workspace.id),
            )
        ]?.dependency_groups ?? [];
    const index = Option.getOrUndefined(Option.flatMap(Schema.decodeUnknownOption(Indexed)(stated(workspace)), ({ tool }) => Array.findFirst(tool.uv.index, (row) => row['publish-url'] !== undefined)))?.name;
    const pixi = documented.has(posix.join(workspace, PIXI)) || Schema.is(Pixi)(stated(workspace));
    const manifests = documented
        .keys()
        .filter((file) => posix.basename(file) === MANIFEST)
        .map((file) => posix.join('{workspaceRoot}', file))
        .toArray();
    const shaped = members.map((member) => {
        const groups = Array.unionWith(resolution[member.id]?.dependency_groups ?? [], shared, Equivalence.Struct({ name: Equivalence.String }));
        const packaged = 'editable' in (resolution[member.id]?.source ?? {});
        const project = Option.getOrUndefined(declaring(member.id));
        const base = packages.has(member.root) ? posix.dirname(member.root) : member.root;
        const nested = members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`)));
        return {
            name: member.name,
            distribution: `${normalized(member.name).replaceAll('-', '_')}-${resolution[member.id]?.version ?? '*'}`,
            root: member.root,
            workspace,
            lock,
            packaged,
            manifests,
            index,
            pixi,
            scripted: packaged && project !== undefined && !Record.isEmptyRecord(project.scripts),
            locals: named([...reach([member.id])]),
            python: project?.['requires-python'],
            published: packaged && project !== undefined && !(project.dynamic.includes('classifiers') || project.classifiers.some((classifier) => classifier.startsWith('Private ::'))),
            linked: Array.dedupe(
                (owned[member.root] ?? [])
                    .filter(([id]) => resolution[id]?.name === member.name)
                    .flatMap(([id]) => Array.getSomes((resolution[id]?.dependencies ?? []).map(resolved)))
                    .map(({ root }) => root),
            ),
            checks: Array.filterMap(nested ? [] : Record.toEntries(CHECKERS), ([key, row]) => {
                const group = Array.findFirst(groups, offering(key));
                const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
                const installed = reach([member.id, ...from]);
                const cwd = row.folder(workspace, base);
                return installed.values().some(distributed(key))
                    ? Result.succeed({ key, cwd, ...declared(documented, { cwd, root: member.root }, key), group: Option.getOrUndefined(group)?.name, projects: named(from.filter(loaded(row.plugins))), covered: row.coverage !== undefined && installed.values().some(distributed(row.coverage)) })
                    : Result.failVoid;
            }),
        };
    });
    const failures = Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), (cause) => new Cause.UnknownError(cause, `${at} does not read as TOML or INI`)));
    return yield* Array.match(failures, {
        onEmpty: () => Effect.succeed({ lock, sources: [lock, DECODED, ...members.map(({ root }) => posix.join(root, '__init__.py'))], members: shaped }),
        onNonEmpty: (errors) => Effect.fail(new Cause.UnknownError(errors, `${lock}: ${errors.map(({ message }) => message).join('; ')}`)),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { LOCK, located, MANIFEST, type Member, Metadata, normalized, SCHEMA };
