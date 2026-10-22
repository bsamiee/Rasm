import { VersionActions } from 'nx/release';

// --- [COMPOSITION] ---------------------------------------------------------------------

class TagVersionActions extends VersionActions {
    override readonly validManifestFilenames = null;

    override readCurrentVersionFromSourceManifest() {
        return Promise.resolve(null);
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion() {
        return Promise.resolve([]);
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default TagVersionActions;
