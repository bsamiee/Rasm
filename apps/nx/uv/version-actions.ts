import { parse, patch } from '@decimalturn/toml-patch';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type ProjectGraph, readCachedProjectGraph, type Tree } from '@nx/devkit';
import { clean, compare, explain } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, SemVer, UndefinedOr } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { LOCK, normalized, spawned, UvError } from '#/plugins/project-graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PYPROJECT = 'pyproject.toml';
const PRERELEASES = { a: 'alpha', b: 'beta', rc: 'rc' } as const;
const NAME = /^\s*(?<name>[a-z0-9](?:[a-z0-9._-]*[a-z0-9])?)/iu;
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
const Configured = Schema.Struct({ tool: Schema.Struct({ uv: Indexes }) });
const SETTINGS = {
    'uv.toml': (parsed: unknown) => Option.map(Schema.decodeUnknownOption(Indexes)(parsed), (settings) => settings.index),
    [PYPROJECT]: (parsed: unknown) => Option.map(Schema.decodeUnknownOption(Configured)(parsed), (settings) => settings.tool.uv.index),
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
    Schema.decodeUnknownEffect(Semantic)(explain(version)).pipe(
        Effect.flatMap(({ release: [major, minor = 0, micro = 0], pre }) => Effect.fromResult(SemVer.fromInput({ major, minor, patch: micro, prerelease: pre === null ? [] : [PRERELEASES[pre[0]], pre[1]] }))),
        Effect.mapBoth({ onFailure: (cause) => UvError.make({ subject: version, detail: 'has an epoch, post, dev, or local part, or more than three release numbers, which SemVer cannot state', cause }), onSuccess: SemVer.format }),
    );

const pep440 = (version: string) =>
    Effect.fromOption(
        Option.filter(Option.fromNullishOr(clean(version)), (cleaned) => explain(cleaned)?.post === null),
        () => UvError.make({ subject: version, detail: 'has no PEP 440 form; use a prerelease id of alpha, beta, or rc' }),
    );

const named = (requirement: string) =>
    Option.map(
        Option.flatMap(Option.fromNullishOr(NAME.exec(requirement)?.groups), ({ name }) => Option.fromNullishOr(name)),
        normalized,
    );

const document = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => UvError.make({ subject: at, detail: 'is missing' })));
    const parsed = yield* Effect.try({ try: (): unknown => parse(text), catch: (cause) => UvError.make({ subject: at, detail: 'does not parse as TOML', cause }) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Table)));
    return { text, parsed };
});

const project = Effect.fnUntraced(function* (tree: Tree, root: string) {
    const { text, parsed } = yield* document(tree, joinPathFragments(root, PYPROJECT));
    const [decoded, raw] = yield* Effect.all([Schema.decodeUnknownEffect(Project)(parsed), Schema.decodeUnknownEffect(Raw)(parsed)]);
    return { text, parsed, project: decoded.project, table: raw.project };
});

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | HttpClient.HttpClient>) => effect.pipe(Effect.provide([layer, FetchHttpClient.layer]), Effect.runPromise);

// --- [SERVICES] ------------------------------------------------------------------------

class UvVersionActions extends VersionActions {
    override readonly validManifestFilenames = [PYPROJECT];
    override readonly excludeManifestsFromFormatting = true;

    get lock() {
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ lock }) => lock);
    }

    override readDependencies(_tree: Tree, projectGraph: ProjectGraph) {
        return Promise.resolve((projectGraph.dependencies[this.projectGraphNode.name] ?? []).filter(({ target }) => target in projectGraph.nodes));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const { root } = this.projectGraphNode.data;
        return run(
            project(tree, root).pipe(
                Effect.flatMap((manifest) => semver(manifest.project.version)),
                Effect.map((currentVersion) => ({ currentVersion, manifestPath: joinPathFragments(root, PYPROJECT) })),
            ),
        );
    }

    override readCurrentVersionFromRegistry(tree: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        const [located, root] = [this.lock, this.projectGraphNode.data.root];
        return run(
            Effect.gen(function* () {
                const [lock, { project: declared }, { index }] = yield* Effect.all([located, project(tree, root), Schema.decodeEffect(Metadata)(currentVersionResolverMetadata ?? {})]);
                const configured = yield* Effect.forEach(
                    Record.toEntries(SETTINGS).filter(([file]) => tree.exists(joinPathFragments(lock, '..', file))),
                    ([file, form]) => Effect.map(document(tree, joinPathFragments(lock, '..', file)), ({ parsed }) => Option.getOrElse(form(parsed), Array.empty)),
                );
                const url = yield* UndefinedOr.match(index, {
                    onUndefined: () => Effect.succeed('https://pypi.org/simple'),
                    onDefined: (name) => Effect.mapBoth(Effect.fromOption(Array.findFirst(configured.flat(), (entry) => entry.name === name)), { onFailure: () => UvError.make({ subject: name, detail: `names no uv index in the uv.toml or pyproject.toml beside ${lock}` }), onSuccess: (entry) => entry.url }),
                });
                const response = yield* HttpClient.execute(HttpClientRequest.get(`${url.replace(TRAILING, '')}/${normalized(declared.name)}/`).pipe(HttpClientRequest.setHeader('Accept', 'application/vnd.pypi.simple.v1+json')));
                const versions = yield* HttpClientResponse.matchStatus(response, {
                    404: () => Effect.succeed<readonly string[]>([]),
                    orElse: (listing) => Effect.map(Effect.flatMap(HttpClientResponse.filterStatusOk(listing), HttpClientResponse.schemaBodyJson(Simple)), (page) => page.versions),
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
                const collections = [['dependencies', dependent.dependencies] as const, ...Record.collect(dependent['optional-dependencies'], (extra, rows) => [`optional-dependencies.${extra}`, rows] as const)];
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
                const [{ text, parsed, table }, version] = yield* Effect.all([project(tree, root), pep440(newVersion)]);
                tree.write(at, patch(text, { ...parsed, project: { ...table, version } }));
                return [`New version ${version} written to manifest: ${at}`];
            }),
        );
    }

    override updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const [located, { root }] = [this.lock, this.projectGraphNode.data];
        return run(
            Effect.gen(function* () {
                const lock = yield* located;
                const [{ text, parsed, table, project: dependent }, { package: locked }] = yield* Effect.all([project(tree, root), Effect.flatMap(document(tree, lock), (lockfile) => Schema.decodeUnknownEffect(Locked)(lockfile.parsed))]);
                const declared = Option.getOrElse(
                    Option.flatMap(
                        Array.findFirst(locked, (entry) => entry.name === normalized(dependent.name)),
                        (entry) => Option.fromUndefinedOr(entry.metadata?.['requires-dist']),
                    ),
                    Array.empty,
                );
                const released = yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([dependency, version]) => Effect.all([Effect.flatMap(Effect.fromNullishOr(projectGraph.nodes[dependency]), (node) => project(tree, node.data.root)), pep440(version)]));
                const floors = new Map(released.map(([manifest, version]) => [normalized(manifest.project.name), version] as const));
                const floored = (row: string) =>
                    Option.getOrElse(
                        Option.map(
                            Option.flatMap(named(row), (name) => Option.all([Option.fromUndefinedOr(floors.get(name)), Array.findFirst(declared, (entry) => entry.name === name)])),
                            ([floor, entry]) => {
                                const pinned = Option.match(Option.liftPredicate(entry.extras ?? [], Array.isReadonlyArrayNonEmpty), { onNone: () => `${entry.name}>=${floor}`, onSome: (extras) => `${entry.name}[${extras.join(',')}]>=${floor}` });
                                return UndefinedOr.match(entry.marker, { onUndefined: () => pinned, onDefined: (marker) => `${pinned}; ${marker}` });
                            },
                        ),
                        () => row,
                    );
                const optional = Record.map(dependent['optional-dependencies'], (rows) => rows.map(floored));
                tree.write(joinPathFragments(root, PYPROJECT), patch(text, { ...parsed, project: { ...table, ...('dependencies' in table ? { dependencies: dependent.dependencies.map(floored) } : {}), ...(Record.isEmptyRecord(optional) ? {} : { 'optional-dependencies': optional }) } }));
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
            const locks =
                dryRun === true
                    ? []
                    : Array.dedupe(
                          Object.values(readCachedProjectGraph().nodes).flatMap((node) =>
                              Schema.decodeUnknownOption(Options)(node.data.release?.version?.versionActionsOptions).pipe(
                                  Option.map(({ lock }) => lock),
                                  Option.filter((lock) => path.basename(lock) === LOCK),
                                  Option.toArray,
                              ),
                          ),
                      );
            const changed = yield* Effect.filter(locks, (lock) => Effect.zipWith(fs.readFileString(path.join(cwd, lock)), Effect.andThen(spawned('uv', ['lock'], path.join(cwd, path.dirname(lock))), fs.readFileString(path.join(cwd, lock))), (before, after) => before !== after));
            return { changedFiles: changed, deletedFiles: [] };
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default UvVersionActions;
