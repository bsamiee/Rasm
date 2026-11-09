import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree, workspaceRoot } from '@nx/devkit';
import { Array, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { isValid, sortReversed } from 'verkit';
import { assembly, DotnetError, spawned } from '#/plugins/project-graph';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ projectFile: Schema.String });
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

const written = Effect.fnUntraced(function* (text: string, version: string) {
    return yield* spawned([yield* assembly(workspaceRoot), 'version', version], workspaceRoot, text);
});

// --- [SERVICES] ------------------------------------------------------------------------

class DotnetVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    override init(tree: Tree) {
        return run(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ projectFile }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [projectFile];
                    return super.init(tree);
                }),
            ),
        );
    }

    get project() {
        return joinPathFragments(this.projectGraphNode.data.root, this.validManifestFilenames[0] ?? '');
    }

    override readCurrentVersionFromSourceManifest() {
        return run(Effect.map(evaluated(this.project), ({ Properties }) => ({ currentVersion: Properties.Version, manifestPath: this.project })));
    }

    override readCurrentVersionFromRegistry(_: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        return run(searched(this.project, currentVersionResolverMetadata));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const { project } = this;
        return run(
            Effect.gen(function* () {
                const text = yield* Effect.fromNullishOr(tree.read(project, 'utf-8')).pipe(Effect.mapError(() => DotnetError.make({ subject: project, detail: 'is missing' })));
                tree.write(project, yield* written(text, newVersion));
                return [`New version ${newVersion} written to manifest: ${project}`];
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
