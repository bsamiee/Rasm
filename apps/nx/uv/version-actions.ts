import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { parse, patch } from '@decimalturn/toml-patch';
import { joinPathFragments, type ProjectGraph, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { clean, explain } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { spawned, UvError } from './plugins/workspace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'pyproject.toml';
const PRERELEASES = { a: 'alpha', b: 'beta', rc: 'rc' } as const;
const REGISTRY = 'https://pypi.org/pypi';

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.Struct({ project: Schema.Struct({ name: Schema.String, version: Schema.String }) });
const Released = Schema.Struct({ info: Schema.Struct({ version: Schema.String }) });
const Metadata = Schema.Struct({ registry: Schema.optionalKey(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const semver = (version: string) =>
    Option.match(
        Option.filter(Option.fromNullishOr(explain(version)), (held) => held.epoch === 0 && held.post === null && held.dev === null && held.local === null && held.release.length <= 3),
        {
            onNone: () => Effect.fail(UvError.make({ subject: version, detail: 'has an epoch, post, dev, or local part, or more than three release numbers, which SemVer cannot state' })),
            onSome: (held) =>
                Effect.succeed(
                    [
                        Array.pad(held.release, 3, 0).join('.'),
                        ...(held.pre === null ? [] : [`${PRERELEASES[held.pre[0] as keyof typeof PRERELEASES]}.${held.pre[1]}`]),
                    ].join('-'),
                ),
        },
    );

const pep440 = (version: string) =>
    Effect.fromOption(Option.filter(Option.fromNullishOr(clean(version)), (cleaned) => explain(cleaned)?.post === null)).pipe(
        Effect.mapError(() => UvError.make({ subject: version, detail: 'has no PEP 440 form; use a prerelease id of alpha, beta, or rc' })),
    );

const manifest = (tree: Tree, root: string) =>
    Effect.flatMap(
        Effect.try({ try: () => parse(tree.read(joinPathFragments(root, MANIFEST), 'utf-8') ?? '') as unknown, catch: (cause) => UvError.make({ subject: joinPathFragments(root, MANIFEST), detail: String(cause) }) }),
        Schema.decodeUnknownEffect(Manifest),
    );

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | HttpClient.HttpClient>) => Effect.runPromise(effect.pipe(Effect.provide([layer, FetchHttpClient.layer])));

// --- [SERVICES] ------------------------------------------------------------------------

class UvVersionActions extends VersionActions {
    validManifestFilenames = [MANIFEST];
    override excludeManifestsFromFormatting = true;

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const root = this.projectGraphNode.data.root;
        return run(Effect.flatMap(manifest(tree, root), ({ project }) => Effect.map(semver(project.version), (currentVersion) => ({ currentVersion, manifestPath: joinPathFragments(root, MANIFEST) }))));
    }

    readCurrentVersionFromRegistry(tree: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        return run(
            Effect.gen({ self: this }, function* () {
                const [{ project }, { registry = REGISTRY }] = yield* Effect.all([manifest(tree, this.projectGraphNode.data.root), Schema.decodeEffect(Metadata)(currentVersionResolverMetadata ?? {})]);
                const response = yield* HttpClient.get(`${registry}/${project.name}/json`);
                return response.status === 404
                    ? { currentVersion: null, logText: `${project.name} has no release on ${registry}` }
                    : yield* Effect.flatMap(Effect.flatMap(HttpClientResponse.filterStatusOk(response), HttpClientResponse.schemaBodyJson(Released)), ({ info }) =>
                          Effect.map(semver(info.version), (currentVersion) => ({ currentVersion, logText: `${project.name} ${info.version} on ${registry}` })),
                      );
            }),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        return run(
            Effect.gen(function* () {
                const node = yield* Effect.fromNullishOr(projectGraph.nodes[dependencyProjectName]);
                const { project } = yield* manifest(tree, node.data.root);
                return { currentVersion: yield* semver(project.version), dependencyCollection: 'dependencies' };
            }),
        );
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        const at = joinPathFragments(this.projectGraphNode.data.root, MANIFEST);
        return run(
            Effect.gen(function* () {
                const [text, version] = yield* Effect.all([Effect.fromNullishOr(tree.read(at, 'utf-8')), pep440(newVersion)]);
                const held = parse(text) as typeof Manifest.Encoded;
                tree.write(at, patch(text, { ...held, project: { ...held.project, version } }));
                return [`New version ${version} written to manifest: ${at}`];
            }),
        );
    }

    updateProjectDependencies() {
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
