import { basename, join, posix, relative } from 'node:path';
import { normalizePath } from '@nx/devkit';
import { combineGlobPatterns, globWithWorkspaceContext } from '@nx/devkit/internal';
import { above, below } from '@rasm/nx';
import { inc, major, minor, minSatisfying, RANGE_PATTERN } from '@renovatebot/pep440';
import { Array, Cause, Effect, Equivalence, FileSystem, Function, Graph, Option, pipe, Record, Result, Schema, SchemaTransformation, String, Struct, UndefinedOr } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import { CHECKERS, CONFIGS } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Effect.Success<ReturnType<typeof located>>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const SCHEMA = 'preview';
const MANIFEST = 'pyproject.toml';
const LOADERS: ReadonlySet<string> = new Set(Object.values(CHECKERS).flatMap(({ plugins }) => plugins ?? []));
const DECODED = combineGlobPatterns(CONFIGS.map((file) => `**/${file}`));
const CLAUSES = new RegExp(RANGE_PATTERN, 'giu');

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
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Patterns = Schema.Struct({
    python_files: Schema.optionalKey(
        Schema.Union([Schema.Array(Schema.String), Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), SchemaTransformation.transform({ decode: (text): readonly string[] => text.split(/\s+/u).filter(String.isNonEmpty), encode: (files: readonly string[]) => files.join(' ') })))]),
    ),
});
const Patterned = Patterns.mapFields(Struct.assign({ ini_options: Schema.optionalKey(Patterns), pytest: Schema.optionalKey(Patterns) })).annotate({
    description: 'The python_files setting of a pytest configuration table, of its ini_options table in a pyproject.toml, or of the pytest section of an INI file read whole',
});
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    source: Schema.Struct({ editable: Schema.optionalKey(Schema.String), virtual: Schema.optionalKey(Schema.String), directory: Schema.optionalKey(Schema.String), path: Schema.optionalKey(Schema.String) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ id }) => id, encode: (id) => ({ id }) })))),
    dependency_groups: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })))),
    optional_dependencies: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })))),
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

const floor = (range: string) => {
    const stated = range.matchAll(CLAUSES).flatMap(({ groups }) => Array.fromNullishOr(groups?.['version']));
    const near = stated.flatMap((version) => Array.getSomes([version, inc(version, 'patch'), inc(version, 'minor')].map(Option.fromNullOr))).toArray();
    return UndefinedOr.map(minSatisfying(near, range) ?? undefined, (lowest) => `${major(lowest)}.${minor(lowest)}`);
};

const located = Effect.fnUntraced(function* (workspaceRoot: string, lock: string, metadata: Pick<Metadata, 'workspace' | 'members' | 'resolution'>) {
    const fs = yield* FileSystem.FileSystem;
    const { resolution } = metadata;
    const files = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [DECODED])));
    const rooted = (at: string) => normalizePath(relative(workspaceRoot, at)) || '.';
    const sited = Record.filterMap(resolution, ({ source }) => Result.fromNullishOr(UndefinedOr.map(source.editable ?? source.virtual ?? source.directory ?? source.path, rooted), Function.constVoid));
    const grouped = new Set(Record.values(resolution).flatMap(({ dependency_groups }) => (dependency_groups ?? []).map(({ id }) => id)));
    const owned = Array.groupBy(
        Record.toEntries(sited).filter(([id]) => !grouped.has(id)),
        ([, root]) => root,
    );
    const workspace = rooted(metadata.workspace.path);
    const listed = metadata.members.map(({ name, id, path }) => ({ name, id, root: rooted(path), listed: true }));
    const members = listed.some(({ root }) => root === workspace) ? listed : [...listed, { name: basename(join(workspaceRoot, workspace)), id: metadata.workspace.id, root: workspace, listed: false }];
    const present = Array.cartesianWith(Array.dedupe([workspace, ...members.map(({ root }) => root), ...Record.values(sited)].flatMap(above)), CONFIGS, posix.join).filter((at) => files.has(at));
    const read = yield* Effect.forEach(present, (at) => Effect.map(fs.readFileString(join(workspaceRoot, at)), (text) => [at, posix.extname(at) === '.toml' ? Toml.parse(text) : Result.succeed<unknown>(Ini.parse(text))] as const), { concurrency: 'unbounded' });
    const documented = new Map(
        yield* Effect.validate(read, ([at, parsed]) => Effect.fromResult(Result.mapBoth(parsed, { onSuccess: (table) => [at, table] as const, onFailure: (cause) => new Cause.UnknownError(cause, `${at} does not read as TOML or INI`) }))).pipe(
            Effect.mapError((errors) => new Cause.UnknownError(errors, `${lock}: ${errors.map(({ message }) => message).join('; ')}`)),
        ),
    );
    const stated = (root: string) => documented.get(posix.join(root, MANIFEST));
    const drilled = (at: string, keys: readonly string[]) => keys.reduce<Option.Option<unknown>>((table, key) => table.pipe(Option.flatMap(Schema.decodeUnknownOption(Table)), Option.flatMap(Record.get(key))), Option.fromUndefinedOr(documented.get(at)));
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
    const shared = members.flatMap(({ root, id }) => (root === workspace ? (resolution[id]?.dependency_groups ?? []) : []));
    const index = Option.getOrUndefined(Option.flatMap(Schema.decodeUnknownOption(Indexed)(stated(workspace)), ({ tool }) => Array.findFirst(tool.uv.index, (row) => row['publish-url'] !== undefined)))?.name;
    const manifests = Array.filterMap(documented.keys(), (file) => (posix.basename(file) === MANIFEST ? Result.succeed(posix.join('{workspaceRoot}', file)) : Result.failVoid));
    const holding = (root: string) => members.some((other) => below(root)(other.root));
    const leaves = listed.filter(({ root }) => !holding(root));
    const placed = members.map((member) => {
        const [owning, leaf] = [member.root === workspace, member.listed && !holding(member.root)];
        return {
            ...member,
            owning,
            leaf,
            alone: leaf ? [member] : [],
            base: packages.has(member.root) ? posix.dirname(member.root) : member.root,
            groups: Array.unionWith(resolution[member.id]?.dependency_groups ?? [], shared, Equivalence.Struct({ name: Equivalence.String })),
            installs: [member.id, ...(owning ? listed.map(({ id }) => id) : [])].flatMap((id) => [id, ...(resolution[id]?.optional_dependencies ?? []).map(Struct.get('id'))]),
            every: owning ? leaves : [],
        };
    });
    const checks = Array.groupBy(
        Array.filterMap(Array.cartesian(placed, Record.toEntries(CHECKERS)), ([member, [key, row]]) => {
            const group = Array.findFirst(member.groups, offering(key));
            const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
            const installed = reach([...member.installs, ...from]);
            const [cwd, checked] = [row.folder(workspace, member.base), row.once ? member.every : member.alone];
            const settings = Array.findFirst(Array.cartesian(above(member.root), Record.toEntries(row.settings)), ([folder, [file, keys]]) => Option.flatMap(drilled(posix.join(folder, file), keys), Schema.decodeUnknownOption(Patterned)));
            const matches = Option.getOrElse(
                Option.flatMapNullishOr(settings, (held) => held.python_files ?? held.ini_options?.python_files ?? held.pytest?.python_files),
                (): readonly string[] => row.matches,
            );
            const config = Array.findFirst(Array.cartesian(Array.fromNullishOr(row.table), Array.dedupe([...above(member.root), ...above(workspace)])), ([table, folder]) => Option.isSome(drilled(posix.join(folder, MANIFEST), ['tool', table])));
            return Array.isReadonlyArrayNonEmpty(checked) && installed.values().some(distributed(key))
                ? Result.succeed({
                      root: member.root,
                      key,
                      cwd,
                      config: Option.getOrUndefined(Option.map(config, ([, folder]) => posix.join(folder, MANIFEST))),
                      paths: checked.map(({ root }) => root),
                      patterns: Array.cartesianWith(checked, matches, ({ root }, file) => posix.join(root, '**', file)),
                      group: Option.getOrUndefined(group)?.name,
                      projects: Array.dedupe([...checked.map(({ name }) => name), ...named(from.filter(loaded(row.plugins)))]),
                      covered: row.coverage !== undefined && installed.values().some(distributed(row.coverage)),
                  })
                : Result.failVoid;
        }),
        ({ root }) => root,
    );
    const shaped = placed.map((member) => {
        const packaged = resolution[member.id]?.source.editable !== undefined;
        const project = Option.getOrUndefined(declaring(member.id));
        return {
            name: member.name,
            listed: member.listed,
            leaf: member.leaf,
            distribution: `${normalized(member.name).replaceAll('-', '_')}-${resolution[member.id]?.version ?? '*'}`,
            root: member.root,
            workspace,
            lock,
            selection: member.owning ? ['--all-packages'] : ['--package', member.name],
            packaged,
            manifests,
            index,
            scripted: packaged && project !== undefined && !Record.isEmptyRecord(project.scripts),
            locals: named([...reach([member.id])]),
            python: project?.['requires-python'],
            floor: UndefinedOr.map(project?.['requires-python'], floor),
            published: packaged && project !== undefined && !(project.dynamic.includes('classifiers') || project.classifiers.some((classifier) => classifier.startsWith('Private ::'))),
            linked: pipe(
                Array.filter(owned[member.root] ?? [], ([id]) => resolution[id]?.name === member.name),
                Array.flatMap(([id]) => Array.getSomes((resolution[id]?.dependencies ?? []).map(resolved)).map(Struct.get('root'))),
                Array.dedupe,
            ),
            checks: Option.toArray(Record.get(checks, member.root)).flat(),
        };
    });
    return { lock, sources: [lock, DECODED, ...members.map(({ root }) => posix.join(root, '__init__.py'))], members: shaped };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { LOCK, located, MANIFEST, type Member, Metadata, normalized, SCHEMA };
