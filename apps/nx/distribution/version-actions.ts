import { matchesGlob } from 'node:path';
import { joinPathFragments, type Tree } from '@nx/devkit';
import { Array, Cause, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { run } from '#/executor';
import { HOSTS } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = (manifest: string) => Effect.mapError(Effect.fromOption(Array.findFirst(HOSTS, (host) => matchesGlob(manifest, host.manifest))), (cause) => new Cause.UnknownError(cause, `${manifest} is no host manifest`));

const opened = (tree: Tree, at: string) => Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), (cause) => new Cause.UnknownError(cause, `${at} is missing`));

const located = Effect.fnUntraced(function* (tree: Tree, root: string, manifest: string) {
    const at = joinPathFragments(root, manifest);
    const [{ read }, text] = yield* Effect.all([hosted(manifest), opened(tree, at)]);
    const stated = Option.flatMap(yield* read(text, at), ({ version }) => Option.fromUndefinedOr(version));
    return { currentVersion: yield* Effect.mapError(Effect.fromOption(stated), (cause) => new Cause.UnknownError(cause, `${at} states no version`)), manifestPath: at };
});

const written = Effect.fnUntraced(function* (tree: Tree, manifest: string, manifests: readonly { readonly manifestPath: string }[], version: string) {
    const { write } = yield* hosted(manifest);
    const writer = yield* Effect.mapError(Effect.fromNullishOr(write), (cause) => new Cause.UnknownError(cause, `${manifest} takes its version from its build, not from nx release`));
    return yield* Effect.forEach(
        manifests,
        Effect.fnUntraced(function* ({ manifestPath }) {
            tree.write(manifestPath, yield* writer(yield* opened(tree, manifestPath), version));
            return `New version ${version} written to manifest: ${manifestPath}`;
        }),
    );
});

// --- [SERVICES] ------------------------------------------------------------------------

class HostVersionActions extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    override get validManifestFilenames() {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    get #manifest() {
        return Effect.mapError(Effect.fromOption(Array.head(this.validManifestFilenames)), (cause) => new Cause.UnknownError(cause, `${this.projectGraphNode.name} names no manifest in its versionActionsOptions`));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return run(Effect.flatMap(this.#manifest, (manifest) => located(tree, this.projectGraphNode.data.root, manifest)));
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return run(Effect.flatMap(this.#manifest, (manifest) => written(tree, manifest, this.manifestsToUpdate, newVersion)));
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default HostVersionActions;
