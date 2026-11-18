import { joinPathFragments, type Tree } from '@nx/devkit';
import { Cause, Effect } from 'effect';
import { VersionActions } from 'nx/release';
import { MANIFEST, restated, run, versioned } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), (cause) => new Cause.UnknownError(cause, `${file} is absent from the tree nx release versions`));

// --- [COMPOSITION] ---------------------------------------------------------------------

class CmakeVersionActions extends VersionActions {
    override readonly validManifestFilenames = [MANIFEST];
    override readonly excludeManifestsFromFormatting = true;

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifestPath = joinPathFragments(this.projectGraphNode.data.root, MANIFEST);
        return run(
            Effect.map(
                Effect.flatMap(read(tree, manifestPath), (text) => versioned(text, manifestPath)),
                ({ version }) => ({ currentVersion: version, manifestPath }),
            ),
        );
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return run(
            Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) =>
                Effect.map(
                    Effect.flatMap(read(tree, manifestPath), (text) => restated(text, newVersion, manifestPath)),
                    (text) => {
                        tree.write(manifestPath, text);
                        return `New version ${newVersion} written to manifest: ${manifestPath}`;
                    },
                ),
            ),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default CmakeVersionActions;
