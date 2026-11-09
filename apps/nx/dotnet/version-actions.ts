import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree, workspaceRoot } from '@nx/devkit';
import { Array, Effect, Option, Result, Schema, SemVer, String } from 'effect';
import { VersionActions } from 'nx/release';
import { isValid, sortReversed } from 'verkit';
import { analyzer, DotnetError, spawned } from '#/plugins/project-graph';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ projectFile: Schema.String, releases: Schema.Array(Schema.Struct({ shipped: Schema.String, unshipped: Schema.String })) });
const Metadata = Schema.UndefinedOr(Schema.Struct({ registry: Schema.optionalKey(Schema.String) }));
const Evaluated = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Struct({ PackageId: Schema.String, Version: Schema.String }) }));
const Searched = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ version: Schema.String })) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluated = (project: string) => Effect.flatMap(spawned(['msbuild', project, '-getProperty:PackageId', '-getProperty:Version'], workspaceRoot), Schema.decodeEffect(Evaluated));

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(effect.pipe(Effect.provide(layer)));

const searched = Effect.fnUntraced(function* (project: string, metadata: Readonly<Record<string, unknown>> | undefined) {
    const [{ Properties }, decoded] = yield* Effect.all([evaluated(project), Schema.decodeEffect(Metadata)(metadata)]);
    const registry = decoded?.registry;
    const { searchResult } = yield* Effect.flatMap(spawned(['package', 'search', Properties.PackageId, '--exact-match', '--prerelease', '--format', 'json', ...(registry === undefined ? [] : ['--source', registry])], workspaceRoot), Schema.decodeEffect(Searched));
    const latest = Array.head(sortReversed(searchResult.flatMap(({ packages }) => packages.map(({ version }) => version)).filter((version) => isValid(version))));
    return {
        currentVersion: Option.getOrNull(latest),
        logText: Option.match(latest, { onNone: () => `${Properties.PackageId} has no release on ${registry ?? 'the NuGet.config sources'}`, onSome: (version) => `${Properties.PackageId} ${version}` }),
    };
});

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), () => DotnetError.make({ subject: file, detail: 'is missing' }));

const shipped = Effect.fnUntraced(function* (tree: Tree, version: string, files: (typeof Options.Type)['releases'][number]) {
    const [recorded, pending, release] = yield* Effect.all([read(tree, files.shipped), read(tree, files.unshipped), Effect.mapError(Effect.fromResult(SemVer.parse(version)), (cause) => DotnetError.make({ subject: version, detail: 'is no semantic version', cause }))]);
    const [rows, comments] = Array.partition(pending.split('\n'), (line) => (line.trimStart().startsWith(';') ? Result.fail(line) : Result.succeed(line)));
    const moved = Option.filter(Option.liftPredicate(rows.join('\n').trim(), String.isNonEmpty), () => !SemVer.isPrerelease(release));
    return yield* Option.match(moved, {
        onNone: () => Effect.succeed([]),
        onSome: (lines) =>
            Effect.sync(() => {
                const section = `## Release ${SemVer.format(SemVer.release(release))}\n\n${lines}\n`;
                tree.write(files.shipped, Option.match(Option.liftPredicate(recorded.trimEnd(), String.isNonEmpty), { onNone: () => section, onSome: (existing) => `${existing}\n\n${section}` }));
                tree.write(files.unshipped, comments.map((line) => `${line}\n`).join(''));
                return [`Unshipped rules of ${files.unshipped} moved to ${files.shipped} under release ${version}`];
            }),
    });
});

// --- [SERVICES] ------------------------------------------------------------------------

class DotnetVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;
    #releases: (typeof Options.Type)['releases'] = [];

    override init(tree: Tree) {
        return run(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ projectFile, releases }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [projectFile];
                    this.#releases = releases;
                    return super.init(tree);
                }),
            ),
        );
    }

    get project() {
        return Effect.map(Effect.fromOption(Array.head(this.validManifestFilenames)), (file) => joinPathFragments(this.projectGraphNode.data.root, file));
    }

    override readCurrentVersionFromSourceManifest() {
        const resolved = this.project;
        return run(
            Effect.gen(function* () {
                const project = yield* resolved;
                const { Properties } = yield* evaluated(project);
                return { currentVersion: Properties.Version, manifestPath: project };
            }),
        );
    }

    override readCurrentVersionFromRegistry(_: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        return run(Effect.flatMap(this.project, (project) => searched(project, currentVersionResolverMetadata)));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const [resolved, releases] = [this.project, this.#releases];
        return run(
            Effect.gen(function* () {
                const project = yield* resolved;
                tree.write(project, yield* spawned([yield* analyzer, 'version', newVersion], workspaceRoot, yield* read(tree, project)));
                return [`New version ${newVersion} written to manifest: ${project}`, ...(yield* Effect.forEach(releases, (files) => shipped(tree, newVersion, files))).flat()];
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
