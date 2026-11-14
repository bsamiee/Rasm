import { matchesGlob } from 'node:path';
import { joinPathFragments, type ProjectGraphProjectNode, type Tree } from '@nx/devkit';
import { Array, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { refused, run } from '#/executor';
import { HOSTS } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = Effect.fnUntraced(function* (tree: Tree, { name, data: { root } }: ProjectGraphProjectNode, manifests: readonly string[]) {
    const manifest = yield* Effect.mapError(Effect.fromOption(Array.head(manifests)), refused(name, 'names no manifest in its versionActionsOptions'));
    const at = joinPathFragments(root, manifest);
    const { read, write } = yield* Effect.mapError(Effect.fromOption(Array.findFirst(HOSTS, (host) => matchesGlob(manifest, host.manifest))), refused(at, 'is no host manifest'));
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), refused(at, 'is missing'));
    const stated = Option.flatMap(yield* read(text, at), ({ version }) => Option.fromUndefinedOr(version));
    return { at, text, version: yield* Effect.mapError(Effect.fromOption(stated), refused(at, 'states no version')), write: yield* Effect.mapError(Effect.fromNullishOr(write), refused(at, 'takes its version from its build, not from nx release')) };
});

const written = Effect.fnUntraced(function* (tree: Tree, node: ProjectGraphProjectNode, manifests: readonly string[], version: string) {
    const { at, text, write } = yield* located(tree, node, manifests);
    tree.write(at, yield* write(text, version));
    return [`New version ${version} written to manifest: ${at}`];
});

// --- [SERVICES] ------------------------------------------------------------------------

class HostVersionActions extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    override get validManifestFilenames() {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return run(Effect.map(located(tree, this.projectGraphNode, this.validManifestFilenames), ({ at, version }) => ({ currentVersion: version, manifestPath: at })));
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return run(written(tree, this.projectGraphNode, this.validManifestFilenames, newVersion));
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default HostVersionActions;
