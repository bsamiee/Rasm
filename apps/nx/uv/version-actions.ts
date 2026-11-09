import { parse, patch } from '@decimalturn/toml-patch';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type ProjectGraph, readCachedProjectGraph, type Tree } from '@nx/devkit';
import { clean, compare, explain } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { spawned, UvError } from '#/plugins/project-graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PYPROJECT = 'pyproject.toml';
const PRERELEASES = { a: 'alpha', b: 'beta', rc: 'rc' } as const;
const PYPI = 'https://pypi.org/simple';
const SIMPLE = 'application/vnd.pypi.simple.v1+json';
const NAME = /^\s*(?<name>[a-z0-9](?:[a-z0-9._-]*[a-z0-9])?)/iu;
const SEPARATORS = /[-_.]+/gu;
const TRAILING = /\/+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Project = Schema.Struct({
    project: Schema.Struct({
        name: Schema.String,
        version: Schema.String,
        dependencies: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        'optional-dependencies': Schema.Record(Schema.String, Schema.Array(Schema.String)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
});
const Indexes = Schema.Struct({ index: Schema.Array(Schema.Struct({ name: Schema.String, url: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });
const SETTINGS = {
    'uv.toml': (held: unknown) => Option.map(Schema.decodeUnknownOption(Indexes)(held), (found) => found.index),
    [PYPROJECT]: (held: unknown) => Option.map(Schema.decodeUnknownOption(Schema.Struct({ tool: Schema.Struct({ uv: Indexes }) }))(held), (found) => found.tool.uv.index),
} as const;
const Simple = Schema.Struct({ versions: Schema.Array(Schema.String) });
const Locked = Schema.Struct({
    package: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            metadata: Schema.optionalKey(Schema.Struct({ 'requires-dist': Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.String, extras: Schema.optionalKey(Schema.Array(Schema.String)), marker: Schema.optionalKey(Schema.String) }))) })),
        }),
    ),
});
const Raw = Schema.Struct({ project: Table });
const Metadata = Schema.Struct({ index: Schema.optionalKey(Schema.String) });
const Options = Schema.Struct({ lock: Schema.String });
const Semantic = Schema.Struct({
    epoch: Schema.Literal(0),
    release: Schema.Tuple([Schema.Int, Schema.optionalKey(Schema.Int), Schema.optionalKey(Schema.Int)]),
    pre: Schema.NullOr(Schema.Tuple([Schema.Literals(Record.keys(PRERELEASES)), Schema.Int])),
    post: Schema.Null,
    dev: Schema.Null,
    local: Schema.Null,
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const semver = (version: string) =>
    Effect.mapBoth(Schema.decodeUnknownEffect(Semantic)(explain(version)), {
        onFailure: () => UvError.make({ subject: version, detail: 'has an epoch, post, dev, or local part, or more than three release numbers, which SemVer cannot state' }),
        onSuccess: ({ release: [major, minor = 0, micro = 0], pre }) => [`${major}.${minor}.${micro}`, ...(pre === null ? [] : [`${PRERELEASES[pre[0]]}.${pre[1]}`])].join('-'),
    });

const pep440 = (version: string) => Effect.fromOption(Option.filter(Option.fromNullishOr(clean(version)), (cleaned) => explain(cleaned)?.post === null)).pipe(Effect.mapError(() => UvError.make({ subject: version, detail: 'has no PEP 440 form; use a prerelease id of alpha, beta, or rc' })));

const normalized = (name: string) => name.toLowerCase().replaceAll(SEPARATORS, '-');

const named = (requirement: string) =>
    Option.map(
        Option.flatMap(Option.fromNullishOr(NAME.exec(requirement)?.groups), ({ name }) => Option.fromNullishOr(name)),
        normalized,
    );

const document = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => UvError.make({ subject: at, detail: 'is missing' })));
    const held = yield* Effect.try({ try: (): unknown => parse(text), catch: (cause) => UvError.make({ subject: at, detail: String(cause) }) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Table)));
    return { text, held };
});

const project = (tree: Tree, root: string) =>
    Effect.flatMap(document(tree, joinPathFragments(root, PYPROJECT)), ({ text, held }) => Effect.map(Effect.all([Schema.decodeUnknownEffect(Project)(held), Schema.decodeUnknownEffect(Raw)(held)]), ([found, raw]) => ({ text, held, project: found.project, table: raw.project })));

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | HttpClient.HttpClient>) => Effect.runPromise(effect.pipe(Effect.provide([layer, FetchHttpClient.layer])));

// --- [SERVICES] ------------------------------------------------------------------------

class UvVersionActions extends VersionActions {
    override readonly validManifestFilenames = [PYPROJECT];
    override readonly excludeManifestsFromFormatting = true;

    get lock() {
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ lock }) => lock);
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const { root } = this.projectGraphNode.data;
        return run(Effect.flatMap(project(tree, root), (found) => Effect.map(semver(found.project.version), (currentVersion) => ({ currentVersion, manifestPath: joinPathFragments(root, PYPROJECT) }))));
    }

    override readCurrentVersionFromRegistry(tree: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        const [located, root] = [this.lock, this.projectGraphNode.data.root];
        return run(
            Effect.gen(function* () {
                const [lock, { project: declared }, { index }] = yield* Effect.all([located, project(tree, root), Schema.decodeEffect(Metadata)(currentVersionResolverMetadata ?? {})]);
                const configured = yield* Effect.forEach(
                    Record.toEntries(SETTINGS).filter(([file]) => tree.exists(joinPathFragments(lock, '..', file))),
                    ([file, form]) => Effect.map(document(tree, joinPathFragments(lock, '..', file)), ({ held }) => Option.getOrElse(form(held), Array.empty)),
                );
                const url = yield* Option.match(Option.fromUndefinedOr(index), {
                    onNone: () => Effect.succeed(PYPI),
                    onSome: (name) => Effect.mapBoth(Effect.fromOption(Array.findFirst(configured.flat(), (entry) => entry.name === name)), { onFailure: () => UvError.make({ subject: name, detail: `names no uv index in the uv.toml or pyproject.toml beside ${lock}` }), onSuccess: (entry) => entry.url }),
                });
                const response = yield* HttpClient.execute(HttpClientRequest.get(`${url.replace(TRAILING, '')}/${normalized(declared.name)}/`).pipe(HttpClientRequest.setHeader('Accept', SIMPLE)));
                const versions = yield* HttpClientResponse.matchStatus(response, {
                    404: () => Effect.succeed<readonly string[]>([]),
                    orElse: (found) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(found), HttpClientResponse.schemaBodyJson(Simple)), (page) => page.versions),
                });
                return yield* Option.match(Array.last(versions.toSorted(compare)), {
                    onNone: () => Effect.succeed<{ readonly currentVersion: string | null; readonly logText: string }>({ currentVersion: null, logText: `${declared.name} has no release on ${url}` }),
                    onSome: (newest) => Effect.map(semver(newest), (currentVersion) => ({ currentVersion, logText: `${declared.name} ${newest} on ${url}` })),
                });
            }),
        );
    }

    override readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        const { root } = this.projectGraphNode.data;
        return run(
            Effect.gen(function* () {
                const node = yield* Effect.fromNullishOr(projectGraph.nodes[dependencyProjectName]);
                const [{ project: dependent }, { project: dependency }] = yield* Effect.all([project(tree, root), project(tree, node.data.root)]);
                const collections = [['dependencies', dependent.dependencies] as const, ...Record.toEntries(dependent['optional-dependencies']).map(([extra, rows]) => [`optional-dependencies.${extra}`, rows] as const)];
                return yield* Option.match(
                    Array.findFirst(collections, ([, rows]) => rows.some((row) => Option.contains(named(row), normalized(dependency.name)))),
                    {
                        onNone: () => Effect.succeed<{ readonly currentVersion: string | null; readonly dependencyCollection: string | null }>({ currentVersion: null, dependencyCollection: null }),
                        onSome: ([dependencyCollection]) => Effect.map(semver(dependency.version), (currentVersion) => ({ currentVersion, dependencyCollection })),
                    },
                );
            }),
        );
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const { root } = this.projectGraphNode.data;
        const at = joinPathFragments(root, PYPROJECT);
        return run(
            Effect.gen(function* () {
                const [{ text, held, table }, version] = yield* Effect.all([project(tree, root), pep440(newVersion)]);
                tree.write(at, patch(text, { ...held, project: { ...table, version } }));
                return [`New version ${version} written to manifest: ${at}`];
            }),
        );
    }

    override updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const [located, { root }] = [this.lock, this.projectGraphNode.data];
        return run(
            Effect.gen(function* () {
                const lock = yield* located;
                const [{ text, held, table, project: dependent }, { package: locked }] = yield* Effect.all([project(tree, root), Effect.flatMap(document(tree, lock), (found) => Schema.decodeUnknownEffect(Locked)(found.held))]);
                const declared = Option.getOrElse(
                    Option.flatMap(
                        Array.findFirst(locked, (entry) => entry.name === normalized(dependent.name)),
                        (entry) => Option.fromUndefinedOr(entry.metadata?.['requires-dist']),
                    ),
                    Array.empty,
                );
                const released = yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([dependency, version]) => Effect.all([Effect.flatMap(Effect.fromNullishOr(projectGraph.nodes[dependency]), (node) => project(tree, node.data.root)), pep440(version)]));
                const floors = new Map(released.map(([found, version]) => [normalized(found.project.name), version] as const));
                const floored = (row: string) =>
                    Option.getOrElse(
                        Option.map(
                            Option.flatMap(named(row), (name) => Option.all([Option.fromUndefinedOr(floors.get(name)), Array.findFirst(declared, (entry) => entry.name === name)])),
                            ([floor, entry]) => `${entry.name}${(entry.extras ?? []).length === 0 ? '' : `[${(entry.extras ?? []).join(',')}]`}>=${floor}${entry.marker === undefined ? '' : `; ${entry.marker}`}`,
                        ),
                        () => row,
                    );
                const optional = Record.map(dependent['optional-dependencies'], (rows) => rows.map(floored));
                tree.write(joinPathFragments(root, PYPROJECT), patch(text, { ...held, project: { ...table, ...('dependencies' in table ? { dependencies: dependent.dependencies.map(floored) } : {}), ...(Record.isEmptyRecord(optional) ? {} : { 'optional-dependencies': optional }) } }));
                return [...floors].map(([name, floor]) => `Floor ${name}>=${floor} written to ${joinPathFragments(root, PYPROJECT)}`);
            }),
        );
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const afterAllProjectsVersioned: AfterAllProjectsVersioned = (cwd, { dryRun }) =>
    run(
        Effect.gen(function* () {
            const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
            const locks = dryRun === true ? [] : Array.dedupe(Record.values(readCachedProjectGraph().nodes).flatMap((node) => Option.toArray(Option.map(Schema.decodeUnknownOption(Options)(node.data.release?.version?.versionActionsOptions), ({ lock }) => lock))));
            const changed = yield* Effect.forEach(locks, (lock) =>
                Effect.gen(function* () {
                    const before = yield* fs.readFileString(path.join(cwd, lock));
                    yield* spawned('uv', ['lock'], path.join(cwd, path.dirname(lock)));
                    return (yield* fs.readFileString(path.join(cwd, lock))) === before ? Option.none() : Option.some(lock);
                }),
            );
            return { changedFiles: Array.getSomes(changed), deletedFiles: [] };
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default UvVersionActions;
