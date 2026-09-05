// Version actions for a project versioned by its git tag alone, MinVer and eng/scripts/publish.py read the tag at build time

// --- [IMPORTS] -------------------------------------------------------------------------

import type { ProjectGraph, Tree } from '@nx/devkit';
import { Effect, Layer, ManagedRuntime } from 'effect';
import { VersionActions } from 'nx/release';

// --- [COMPOSITION] ---------------------------------------------------------------------

// Nx consumes a promise from every method, the runtime translates each constant answer at that boundary
const _runtime = ManagedRuntime.make(Layer.empty);

// biome-ignore lint/style/noDefaultExport: Nx loads the class from the default export of the versionActions module
export default class GitTagVersionActions extends VersionActions {
    // No manifest holds the version, and a null here keeps nx release from reading or writing one
    override validManifestFilenames = null;

    // A project without a tag has never been released, and its first release bumps from 0.0.0
    override readCurrentVersionFromSourceManifest(_tree: Tree): Promise<{ currentVersion: string; manifestPath: string }> {
        return _runtime.runPromise(Effect.succeed({ currentVersion: '0.0.0', manifestPath: this.projectGraphNode.data.root }));
    }

    override readCurrentVersionFromRegistry(_tree: Tree, _metadata: Record<string, unknown> | undefined): Promise<null> {
        return _runtime.runPromise(Effect.succeed(null));
    }

    override readCurrentVersionOfDependency(
        _tree: Tree,
        _projectGraph: ProjectGraph,
        _dependencyProjectName: string,
    ): Promise<{ currentVersion: null; dependencyCollection: null }> {
        return _runtime.runPromise(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    override updateProjectVersion(_tree: Tree, newVersion: string): Promise<string[]> {
        return _runtime.runPromise(Effect.succeed([`${this.projectGraphNode.name} takes ${newVersion} from its git tag, no manifest changes`]));
    }

    override updateProjectDependencies(_tree: Tree, _projectGraph: ProjectGraph, _dependencies: Record<string, string>): Promise<string[]> {
        return _runtime.runPromise(Effect.succeed([]));
    }
}
