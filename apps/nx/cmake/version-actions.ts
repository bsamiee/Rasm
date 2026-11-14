import { join } from 'node:path';
import type { Tree } from '@nx/devkit';
import { Effect } from 'effect';
import { VersionActions } from 'nx/release';
import { MANIFEST, refused, restated, versioned } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (tree: Tree, file: string) => Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), refused(file, 'is missing'));

// --- [SERVICES] ------------------------------------------------------------------------

class CmakeVersionActions extends VersionActions {
    override readonly validManifestFilenames = [MANIFEST];
    override readonly excludeManifestsFromFormatting = true;

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifestPath = join(this.projectGraphNode.data.root, MANIFEST);
        return Effect.runPromise(
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
        return Effect.runPromise(
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
