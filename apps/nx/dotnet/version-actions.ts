import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree, workspaceRoot } from '@nx/devkit';
import { Array, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { spawned } from './plugins/project-graph.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSION = /<Version>([^<]*)<\/Version>/;
const GROUP = /(<PropertyGroup>)(\r?\n)([ \t]*)/;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ projectFile: Schema.String });
const Metadata = Schema.Struct({ registry: Schema.optionalKey(Schema.String) });
const Evaluated = Schema.fromJsonString(Schema.Struct({ Properties: Schema.Struct({ PackageId: Schema.String, Version: Schema.String }) }));
const Searched = Schema.fromJsonString(Schema.Struct({ searchResult: Schema.Array(Schema.Struct({ packages: Schema.Array(Schema.Struct({ version: Schema.String })) })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const evaluated = (project: string) => Effect.flatMap(spawned(['msbuild', project, '-getProperty:PackageId', '-getProperty:Version'], workspaceRoot), Schema.decodeEffect(Evaluated));

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(effect.pipe(Effect.provide(layer)));

// --- [SERVICES] ------------------------------------------------------------------------

class DotnetVersionActions extends VersionActions {
    validManifestFilenames: string[] = [];
    override excludeManifestsFromFormatting = true;

    override init(tree: Tree) {
        return run(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ projectFile }) =>
                Effect.promise(() => ((this.validManifestFilenames = [projectFile]), super.init(tree))),
            ),
        );
    }

    get project() {
        return joinPathFragments(this.projectGraphNode.data.root, this.validManifestFilenames[0] ?? '');
    }

    readCurrentVersionFromSourceManifest() {
        return run(Effect.map(evaluated(this.project), ({ Properties }) => ({ currentVersion: Properties.Version, manifestPath: this.project })));
    }

    readCurrentVersionFromRegistry(_: Tree, currentVersionResolverMetadata: Record<string, unknown> | undefined) {
        return run(
            Effect.gen({ self: this }, function* () {
                const [{ Properties }, { registry }] = yield* Effect.all([evaluated(this.project), Schema.decodeEffect(Metadata)(currentVersionResolverMetadata ?? {})]);
                const { searchResult } = yield* Effect.flatMap(
                    spawned(['package', 'search', Properties.PackageId, '--exact-match', '--prerelease', '--format', 'json', ...(registry === undefined ? [] : ['--source', registry])], workspaceRoot),
                    Schema.decodeEffect(Searched),
                );
                const latest = Array.last(searchResult.flatMap(({ packages }) => packages));
                return {
                    currentVersion: Option.getOrNull(Option.map(latest, ({ version }) => version)),
                    logText: Option.match(latest, { onNone: () => `${Properties.PackageId} has no release on ${registry ?? 'the NuGet.config sources'}`, onSome: ({ version }) => `${Properties.PackageId} ${version}` }),
                };
            }),
        );
    }

    readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        const text = tree.read(this.project, 'utf-8') ?? '';
        tree.write(this.project, VERSION.test(text) ? text.replace(VERSION, `<Version>${newVersion}</Version>`) : text.replace(GROUP, `$1$2$3<Version>${newVersion}</Version>$2$3`));
        return Promise.resolve([`New version ${newVersion} written to manifest: ${this.project}`]);
    }

    updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default DotnetVersionActions;
