import { parse, patch } from '@decimalturn/toml-patch';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type ProjectGraph, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { clean, explain } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { EXTENSION, spawned, UvError } from '#/plugins/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PYPROJECT = 'pyproject.toml';
const PRERELEASES = { a: 'alpha', b: 'beta', rc: 'rc' } as const;
const REGISTRY = 'https://pypi.org/pypi';

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Versioned = Schema.Struct({ version: Schema.String });
const Project = Schema.Struct({ project: Schema.Struct({ name: Schema.String, version: Schema.String }) });
const Released = Schema.Struct({ info: Schema.Struct({ version: Schema.String }) });
const Metadata = Schema.Struct({ registry: Schema.optionalKey(Schema.String) });
const Options = Schema.Struct({ manifest: Schema.Literals([PYPROJECT, EXTENSION]) });
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

const MANIFESTS = {
    [PYPROJECT]: { table: Option.some('project'), read: semver, write: pep440 },
    [EXTENSION]: { table: Option.none<string>(), read: Effect.succeed<string>, write: Effect.succeed<string> },
} as const;

const document = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => UvError.make({ subject: at, detail: 'is missing' })));
    const held = yield* Effect.try({ try: (): unknown => parse(text), catch: (cause) => UvError.make({ subject: at, detail: String(cause) }) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Table)));
    return { text, held };
});

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | HttpClient.HttpClient>) => Effect.runPromise(effect.pipe(Effect.provide([layer, FetchHttpClient.layer])));

// --- [SERVICES] ------------------------------------------------------------------------

class UvVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    get manifest() {
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => ({ manifest, at: joinPathFragments(this.projectGraphNode.data.root, manifest), ...MANIFESTS[manifest] }));
    }

    override init(tree: Tree) {
        return run(
            Effect.flatMap(this.manifest, ({ manifest }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [manifest];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const located = this.manifest;
        return run(
            Effect.gen(function* () {
                const { at, table, read } = yield* located;
                const { held } = yield* document(tree, at);
                const { version } = yield* Schema.decodeUnknownEffect(Versioned)(Option.match(table, { onNone: () => held, onSome: (key) => held[key] }));
                return { currentVersion: yield* read(version), manifestPath: at };
            }),
        );
    }

    override readCurrentVersionFromRegistry(tree: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        const located = this.manifest;
        return run(
            Effect.gen(function* () {
                const { manifest, at } = yield* located;
                yield* manifest === EXTENSION ? UvError.make({ subject: at, detail: 'names a Blender extension, which publishes through its host channel and has no registry version; use the disk or git-tag current version resolver' }) : Effect.void;
                const [{ project }, { registry = REGISTRY }] = yield* Effect.all([Effect.flatMap(document(tree, at), ({ held }) => Schema.decodeUnknownEffect(Project)(held)), Schema.decodeEffect(Metadata)(currentVersionResolverMetadata ?? {})]);
                return yield* HttpClientResponse.matchStatus(yield* HttpClient.get(`${registry}/${project.name}/json`), {
                    404: () => Effect.succeed({ currentVersion: null, logText: `${project.name} has no release on ${registry}` }),
                    orElse: (response) =>
                        HttpClientResponse.filterStatusOk(response).pipe(
                            Effect.flatMap(HttpClientResponse.schemaBodyJson(Released)),
                            Effect.flatMap(({ info }) => Effect.map(semver(info.version), (currentVersion) => ({ currentVersion, logText: `${project.name} ${info.version} on ${registry}` }))),
                        ),
                });
            }),
        );
    }

    override readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        return run(
            Effect.gen(function* () {
                const node = yield* Effect.fromNullishOr(projectGraph.nodes[dependencyProjectName]);
                const { project } = yield* Effect.flatMap(document(tree, joinPathFragments(node.data.root, PYPROJECT)), ({ held }) => Schema.decodeUnknownEffect(Project)(held));
                return { currentVersion: yield* semver(project.version), dependencyCollection: 'dependencies' };
            }),
        );
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const located = this.manifest;
        return run(
            Effect.gen(function* () {
                const { at, table, write } = yield* located;
                const [{ text, held }, version] = yield* Effect.all([document(tree, at), write(newVersion)]);
                const next = yield* Option.match(table, {
                    onNone: () => Effect.succeed({ ...held, version }),
                    onSome: (key) => Effect.map(Schema.decodeUnknownEffect(Table)(held[key]), (nested) => ({ ...held, [key]: { ...nested, version } })),
                });
                tree.write(at, patch(text, next));
                return [`New version ${version} written to manifest: ${at}`];
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const afterAllProjectsVersioned: AfterAllProjectsVersioned = (cwd, { dryRun }) =>
    run(
        Effect.gen(function* () {
            const [path, fs] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
            const locks = dryRun === true ? [] : yield* Effect.promise(() => globWithWorkspaceContext(cwd, ['**/uv.lock']));
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
