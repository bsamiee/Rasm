import { join, posix, relative } from 'node:path';
import { DependencyType, normalizePath, type RawProjectGraphDependency } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { above } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, Equivalence, FileSystem, Glob, Graph, Option, Predicate, Record, Result, Schema, SchemaTransformation } from 'effect';
import { Ini, Toml } from 'effect/encoding';
import { CHECKERS, CONFIGS, declared, PIXI } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Effect.Success<ReturnType<typeof located>>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const SCHEMA = 'preview';
const MANIFEST = 'pyproject.toml';
const LOADERS: ReadonlySet<string> = new Set(Object.values(CHECKERS).flatMap(({ plugins }) => plugins ?? []));

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
const Filed = Schema.Struct({ file: Schema.String });
const Globs = Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const Backend = Schema.Struct({
    'build-system': Schema.Struct({ 'build-backend': Schema.Literal('uv_build') }),
    project: Schema.Struct({ name: Schema.String, readme: Schema.optionalKey(Schema.Unknown), license: Schema.optionalKey(Schema.Unknown), 'license-files': Globs }),
    tool: Schema.Struct({
        uv: Schema.Struct({
            'build-backend': Schema.Struct({
                'module-root': Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed('src'))),
                'module-name': Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])),
                namespace: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))),
                'source-include': Globs,
                'source-exclude': Globs,
                'default-excludes': Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(true))),
                data: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
}).annotate({ description: "pyproject.toml tables naming files uv_build packs into a member's source distribution, each absent setting at its documented uv default" });
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

const packed = (stated: unknown): readonly string[] =>
    Option.match(Schema.decodeUnknownOption(Backend)(stated), {
        onNone: () => [posix.join('{projectRoot}', '**', '*')],
        onSome: ({ project, tool }) => {
            const backend = tool.uv['build-backend'];
            const modules = Array.ensure(backend['module-name'] ?? normalized(project.name).replace(backend.namespace ? /-/gu : /-(?!stubs$)/gu, '_')).map((name) => posix.join(backend['module-root'], ...name.split('.'), '**'));
            const files = [...(Predicate.isString(project.readme) ? [project.readme] : []), ...Array.getSomes([project.readme, project.license].map((held) => Schema.decodeUnknownOption(Filed)(held))).map(({ file }) => file)];
            const data = Object.values(backend.data).map((folder) => posix.join(folder, '**'));
            const excluded = [...(backend['default-excludes'] ? ['__pycache__', '*.pyc', '*.pyo'] : []), ...backend['source-exclude']].map((pattern) => (pattern.startsWith('/') ? pattern.slice(1) : posix.join('**', pattern)));
            return [...[MANIFEST, ...modules, ...backend['source-include'], ...files, ...project['license-files'], ...data].map((glob) => posix.join('{projectRoot}', glob)), ...excluded.flatMap((glob) => [glob, posix.join(glob, '**')]).map((glob) => `!${posix.join('{projectRoot}', glob)}`)];
        },
    });

const located = Effect.fnUntraced(function* (workspaceRoot: string, files: ReadonlySet<string>, lock: string, metadata: Metadata) {
    const fs = yield* FileSystem.FileSystem;
    const { resolution } = metadata;
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
    const present = Array.cartesianWith(Array.dedupe([workspace, ...Record.values(sited)].flatMap(above)), CONFIGS, posix.join).filter((at) => files.has(at));
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
    const members = metadata.members.map((member) => ({ ...member, root: rooted(member.path) }));
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
            sources: [...packed(stated(member.root)), posix.join('{workspaceRoot}', workspace, MANIFEST)],
            index,
            pixi,
            scripted: packaged && project !== undefined && !Record.isEmptyRecord(project.scripts),
            locals: named([...reach([member.id])]),
            python: project?.['requires-python'],
            published: packaged && project !== undefined && !(project.dynamic.includes('classifiers') || project.classifiers.some((classifier) => classifier.startsWith('Private ::'))),
            edges: (owned[member.root] ?? [])
                .filter(([id]) => resolution[id]?.name === member.name)
                .flatMap(([id]) => Array.getSomes((resolution[id]?.dependencies ?? []).map(resolved)))
                .map(({ root: target }): RawProjectGraphDependency => ({ source: member.root, target, type: DependencyType.implicit })),
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
    const patterns = Array.dedupe(shaped.flatMap(({ checks }) => checks.flatMap((check) => check.patterns)));
    const listed = yield* Array.match(patterns, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: (globs) => Effect.promise(() => globWithWorkspaceContext(workspaceRoot, globs)),
    });
    const matched = new Set(
        patterns.filter((pattern) => {
            const glob = Glob.make(pattern, { dot: true });
            return listed.some((file) => Glob.match(glob, file));
        }),
    );
    return {
        failures: Array.filterMap(read, ([at, parsed]) => Result.map(Result.flip(parsed), (cause) => new Cause.UnknownError(cause, `${at} does not read as TOML or INI`))),
        members: shaped.map((member) => ({ ...member, checks: member.checks.filter((check) => !matched.isDisjointFrom(new Set(check.patterns))) })),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { LOCK, located, MANIFEST, type Member, Metadata, normalized, SCHEMA };
