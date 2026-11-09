import { matchesGlob } from 'node:path';
import { joinPathFragments, type Tree } from '@nx/devkit';
import { Array, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { DistributionError, HOSTS } from '#/plugins/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = Effect.fnUntraced(function* (tree: Tree, root: string, options: unknown) {
    const { manifest } = yield* Schema.decodeUnknownEffect(Options)(options);
    const at = joinPathFragments(root, manifest);
    const host = yield* Effect.fromOption(
        Array.findFirst(HOSTS, (candidate) => candidate.write !== undefined && matchesGlob(manifest, candidate.manifest)),
        () => DistributionError.make({ subject: at, detail: 'is no manifest whose version this plugin writes' }),
    );
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => DistributionError.make({ subject: at, detail: 'is missing' })));
    const version = yield* Effect.flatMap(host.read(text, at), (fields) =>
        Effect.fromOption(
            Option.flatMap(fields, ({ version: declared }) => Option.fromUndefinedOr(declared)),
            () => DistributionError.make({ subject: at, detail: 'states no version' }),
        ),
    );
    return { at, text, host, version };
});

const written = Effect.fnUntraced(function* (tree: Tree, root: string, options: unknown, version: string) {
    const { at, text, host } = yield* located(tree, root, options);
    tree.write(at, yield* Effect.fromNullishOr(host.write).pipe(Effect.flatMap((write) => write(text, version))));
    return [`New version ${version} written to manifest: ${at}`];
});

// --- [SERVICES] ------------------------------------------------------------------------

class HostVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    override init(tree: Tree) {
        return Effect.runPromise(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [manifest];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return Effect.runPromise(Effect.map(located(tree, this.projectGraphNode.data.root, this.finalConfigForProject.versionActionsOptions), ({ at, version }) => ({ currentVersion: version, manifestPath: at })));
    }

    override readCurrentVersionFromRegistry() {
        return Effect.runPromise(DistributionError.make({ subject: this.projectGraphNode.name, detail: 'publishes its host packages as GitHub release files, which hold no registry version; use the disk or git-tag current version resolver' }));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return written(tree, this.projectGraphNode.data.root, this.finalConfigForProject.versionActionsOptions, newVersion).pipe(Effect.runPromise);
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default HostVersionActions;
