import { joinPathFragments, type Tree } from '@nx/devkit';
import { Array, Cause, Effect, Option, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { routed } from '#/host-packages';
import { run } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), (cause) => new Cause.UnknownError(cause, `${at} is missing`));
    const { host, fields } = yield* Effect.flatMap(routed(at, text), (found) => Effect.mapError(Effect.fromOption(found), (cause) => new Cause.UnknownError(cause, `${at} is no host manifest`)));
    return { host, text, version: Option.fromUndefinedOr(fields.version) };
});

// --- [SERVICES] ------------------------------------------------------------------------

class HostVersionActions extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    override get validManifestFilenames() {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return run(
            Effect.gen({ self: this }, function* () {
                const at = joinPathFragments(this.projectGraphNode.data.root, yield* Effect.mapError(Effect.fromOption(Array.head(this.validManifestFilenames)), (cause) => new Cause.UnknownError(cause, `${this.projectGraphNode.name} names no manifest in its versionActionsOptions`)));
                const { version } = yield* hosted(tree, at);
                return { currentVersion: yield* Effect.mapError(Effect.fromOption(version), (cause) => new Cause.UnknownError(cause, `${at} states no version`)), manifestPath: at };
            }),
        );
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, version: string) {
        return run(
            Effect.gen({ self: this }, function* () {
                const texts = yield* Effect.forEach(
                    this.manifestsToUpdate,
                    Effect.fnUntraced(function* ({ manifestPath }) {
                        const { host, text: before } = yield* hosted(tree, manifestPath);
                        const writer = yield* Effect.mapError(Effect.fromNullishOr(host.write), (cause) => new Cause.UnknownError(cause, `${manifestPath} takes its version from its build, not from nx release`));
                        const text = yield* writer(before, version, manifestPath);
                        const stated = Option.flatMap(yield* host.read(text, manifestPath), (fields) => Option.fromUndefinedOr(fields.version));
                        return Option.contains(stated, version) ? { manifestPath, text } : yield* new Cause.UnknownError(stated, `Cannot update ${manifestPath}: its version resolves to ${Option.getOrElse(stated, () => 'nothing')} instead of ${version}`);
                    }),
                );
                return texts.map(({ manifestPath, text }) => {
                    tree.write(manifestPath, text);
                    return `New version ${version} written to manifest: ${manifestPath}`;
                });
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default HostVersionActions;
