import { Effect } from 'effect';
import { VersionActions } from 'nx/release';

// --- [COMPOSITION] ---------------------------------------------------------------------

class TagVersionActions extends VersionActions {
    override readonly validManifestFilenames = null;

    override readCurrentVersionFromSourceManifest() {
        return Effect.runPromise(Effect.succeed(null));
    }

    override readCurrentVersionFromRegistry() {
        return Effect.runPromise(Effect.succeed(null));
    }

    override readCurrentVersionOfDependency() {
        return Effect.runPromise(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    override updateProjectVersion() {
        return Effect.runPromise(Effect.succeed([]));
    }

    override updateProjectDependencies() {
        return Effect.runPromise(Effect.succeed([]));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default TagVersionActions;
