import { basename, dirname, join, normalize, relative } from 'node:path';
import { DependencyType, type RawProjectGraphDependency } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, Equivalence, Option, Predicate, Record, Result, Schema, SchemaTransformation } from 'effect';
import { CHECKERS, declared, PIXI, refused } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Member = Effect.Success<ReturnType<typeof located>>['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const MANIFEST = 'pyproject.toml';
const PRIVATE = 'Private ::';
const SEPARATORS = /[-_.]+/gu;
const UNDERSCORED = /-(?!stubs$)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Declared = Schema.Struct({
    project: Schema.Struct({
        name: Schema.optionalKey(Schema.String),
        'requires-python': Schema.optionalKey(Schema.String),
        classifiers: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        dynamic: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
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
const Tooled = Schema.Struct({ tool: Schema.Struct({ nx: Schema.Unknown.pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) });
const Nx = Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)) });
const Local = Schema.Literals(['editable', 'virtual', 'directory', 'path']);
const Node = Schema.Struct({
    name: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    source: Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    dependencies: Schema.Array(Schema.Struct({ id: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: ({ id }) => id, encode: (id) => ({ id }) })))),
    dependency_groups: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, id: Schema.String })))),
});
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

const sited = (workspaceRoot: string) => (node: typeof Node.Type) =>
    Option.map(
        Record.findFirst(node.source, (at, key) => Schema.is(Local)(key) && Predicate.isString(at)),
        ([, at]) => relative(workspaceRoot, normalize(String(at))) || '.',
    );

const packed = (stated: unknown): readonly string[] =>
    Option.match(Schema.decodeUnknownOption(Backend)(stated), {
        onNone: () => [join('{projectRoot}', '**', '*')],
        onSome: ({ project, tool }) => {
            const backend = tool.uv['build-backend'];
            const modules = Array.ensure(backend['module-name'] ?? normalized(project.name).replace(UNDERSCORED, '_')).map((name) => join(backend['module-root'], ...name.split('.'), '**'));
            const files = [...(Predicate.isString(project.readme) ? [project.readme] : []), ...Array.getSomes([project.readme, project.license].map((held) => Schema.decodeUnknownOption(Filed)(held))).map(({ file }) => file)];
            const data = Object.values(backend.data).map((folder) => join(folder, '**'));
            const excluded = [...(backend['default-excludes'] ? ['__pycache__', '*.pyc', '*.pyo'] : []), ...backend['source-exclude']].map((pattern) => (pattern.startsWith('/') ? pattern.slice(1) : join('**', pattern)));
            return [...[MANIFEST, ...modules, ...backend['source-include'], ...files, ...project['license-files'], ...data].map((glob) => join('{projectRoot}', glob)), ...excluded.flatMap((glob) => [glob, join(glob, '**')]).map((glob) => `!${join('{projectRoot}', glob)}`)];
        },
    });

const folders = (workspaceRoot: string, { workspace, resolution }: typeof Metadata.Type) => Array.dedupe([relative(workspaceRoot, workspace.path) || '.', ...Array.getSomes(Object.values(resolution).map(sited(workspaceRoot)))]);

const report = (workspaceRoot: string, lock: string, metadata: typeof Metadata.Type, documented: ReadonlyMap<string, unknown>) => {
    const { resolution } = metadata;
    const local = sited(workspaceRoot);
    const workspace = relative(workspaceRoot, metadata.workspace.path) || '.';
    const located = (id: string) => Option.flatMap(Option.fromUndefinedOr(resolution[id]), (node) => Option.all({ root: local(node), name: Option.fromUndefinedOr(node.name) }));
    const reach = (ids: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([...ids, ...[...ids].flatMap((id) => resolution[id]?.dependencies ?? [])]);
        return next.size === ids.size ? ids : reach(next);
    };
    const distributed = (name: string) => (id: string) => Option.exists(Option.fromUndefinedOr(resolution[id]), (node) => node.name === name && Option.isNone(local(node)));
    const stated = (root: string) => documented.get(join(root, MANIFEST));
    const manifest = (root: string) => Option.map(Schema.decodeUnknownOption(Declared)(stated(root)), ({ project }) => project);
    const members = metadata.members.map((member) => ({ ...member, root: relative(workspaceRoot, member.path) || '.' }));
    const workspaceGroups =
        resolution[
            Option.match(
                Array.findFirst(members, (member) => member.root === workspace),
                { onNone: () => metadata.workspace.id, onSome: ({ id }) => id },
            )
        ]?.dependency_groups ?? [];
    const tables = members.map((member) => {
        const file = join(member.root, MANIFEST);
        return [file, Result.flatMap(Schema.decodeUnknownResult(Tooled)(documented.get(file)), ({ tool }) => Schema.decodeUnknownResult(Nx, { onExcessProperty: 'error' })(tool.nx))] as const;
    });
    const index = Option.flatMap(Schema.decodeUnknownOption(Indexed)(stated(workspace)), ({ tool }) => Array.findFirst(tool.uv.index, (row) => row['publish-url'] !== undefined));
    const manifests = [...documented.keys()].filter((file) => basename(file) === MANIFEST);
    return {
        failures: Array.filterMap(tables, ([file, table]) => Result.map(Result.flip(table), refused(file, 'does not decode'))),
        members: Array.zipWith(members, tables, (member, [, table]) => {
            const groups = Array.unionWith(resolution[member.id]?.dependency_groups ?? [], workspaceGroups, Equivalence.Struct({ name: Equivalence.String }));
            const packaged = 'editable' in (resolution[member.id]?.source ?? {});
            return {
                name: member.name,
                root: member.root,
                workspace,
                packaged,
                manifests,
                sources: [...packed(stated(member.root)), join('{workspaceRoot}', workspace, MANIFEST)],
                index: Option.getOrUndefined(Option.flatMap(index, ({ name }) => Option.fromUndefinedOr(name))),
                pixi: documented.has(join(workspace, PIXI)) || Schema.is(Pixi)(stated(workspace)),
                scripted: packaged && Option.exists(manifest(member.root), ({ scripts }) => !Record.isEmptyRecord(scripts)),
                python: Option.getOrUndefined(Option.flatMap(manifest(member.root), (project) => Option.fromUndefinedOr(project['requires-python']))),
                implicit: Result.getOrUndefined(table)?.implicitDependencies,
                published: packaged && Option.exists(manifest(member.root), ({ classifiers, dynamic }) => !(dynamic.includes('classifiers') || classifiers.some((classifier) => classifier.startsWith(PRIVATE)))),
                lock,
                nested: members.some((other) => other.root !== member.root && (member.root === '.' || other.root.startsWith(`${member.root}/`))),
                checks: Record.map(CHECKERS, (row, key) => {
                    const group = Array.findFirst(groups, ({ id }) => (resolution[id]?.dependencies ?? []).some(distributed(key)));
                    const from = Option.match(group, { onNone: () => [], onSome: ({ id }) => resolution[id]?.dependencies ?? [] });
                    const installed = [...reach(new Set([member.id, ...from]))];
                    return {
                        ...declared(documented, member.root, key),
                        group: Option.getOrUndefined(group)?.name,
                        projects: Array.dedupe(Array.getSomes(from.map(located)).map(({ name }) => name)).toSorted(),
                        locked: installed.some(distributed(key)),
                        covered: row.coverage !== undefined && installed.some(distributed(row.coverage)),
                    };
                }),
            };
        }),
        edges: members.flatMap((member) => {
            const sourceFile = join(member.root, MANIFEST);
            const own = Record.keys(Record.filter(resolution, (node) => node.name === member.name && Option.contains(local(node), member.root)));
            const targeted = [...own.flatMap((id) => Array.getSomes((resolution[id]?.dependencies ?? []).map(located))).map(({ root }) => root)];
            return targeted.map((target): RawProjectGraphDependency => ({ source: member.root, target, type: DependencyType.static, sourceFile }));
        }),
    };
};

const located = Effect.fnUntraced(function* (workspaceRoot: string, { members, ...workspace }: ReturnType<typeof report>) {
    const checked = members.flatMap(({ root, checks }) => Record.collect(checks, (key, check) => ({ root, key, check })));
    const matched = yield* Effect.forEach(checked, ({ check }) => Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [...check.patterns])), { concurrency: 'unbounded' });
    const based = Array.groupBy(
        Array.zipWith(checked, matched, ({ root, key, check }, files) => ({ root, key, check: { ...check, ...(Array.isReadonlyArrayNonEmpty(files) && { base: files.includes(join(root, '__init__.py')) ? dirname(root) : root }) } })),
        ({ root }) => root,
    );
    return { ...workspace, members: members.map((member) => ({ ...member, checks: Record.fromIterableWith(based[member.root] ?? [], ({ key, check }) => [key, check] as const) })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { folders, LOCK, located, MANIFEST, type Member, Metadata, normalized, report };
