import { DependencyType, type RawProjectGraphDependency } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { major, minor, minSatisfying, validRange } from '@renovatebot/pep440';
import { Array, Effect, Equivalence, Option, Path, Predicate, Record, Schema, SchemaTransformation } from 'effect';
import { CHECKERS, declared, PIXI } from '#/targets';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof located>>;
type Member = Workspace['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCK = 'uv.lock';
const PRIVATE = 'Private ::';
const SEPARATORS = /[-_.]+/gu;

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

const located = Effect.fnUntraced(function* (workspaceRoot: string, { members, ...workspace }: ReturnType<typeof report>) {
    const path = yield* Path.Path;
    const checked = members.flatMap(({ root, checks }) => Record.collect(checks, (key, check) => ({ root, key, check })));
    const matched = yield* Effect.forEach(checked, ({ check }) => Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [...check.patterns])), { concurrency: 'unbounded' });
    const based = Array.groupBy(
        Array.zipWith(checked, matched, ({ root, key, check }, files) => ({ root, key, check: { ...check, ...(Array.isReadonlyArrayNonEmpty(files) && { base: files.includes(path.join(root, '__init__.py')) ? path.dirname(root) : root }) } })),
        ({ root }) => root,
    );
    return { ...workspace, members: members.map((member) => ({ ...member, checks: Record.fromIterableWith(based[member.root] ?? [], ({ key, check }) => [key, check] as const) })) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { LOCK, located, type Member, Metadata, normalized, Releases, report, type Workspace };
