// Version actions for a project versioned by its git tag alone, MinVer and eng/scripts/publish.py read the tag at build time

// --- [IMPORTS] -------------------------------------------------------------------------

import type { ProjectGraph, Tree } from '@nx/devkit';
import { VersionActions } from 'nx/release';

// --- [COMPOSITION] ---------------------------------------------------------------------

// biome-ignore lint/style/noDefaultExport: Nx loads the class from the default export of the versionActions module
export default class GitTagVersionActions extends VersionActions {
    // No manifest holds the version, and a null here keeps nx release from reading or writing one
    override readonly validManifestFilenames = null;

    // A project without a tag has never been released, and its first release bumps from 0.0.0
    override readCurrentVersionFromSourceManifest(_tree: Tree): Promise<{ currentVersion: string; manifestPath: string }> {
        return Promise.resolve({ currentVersion: '0.0.0', manifestPath: this.projectGraphNode.data.root });
    }

    override readCurrentVersionFromRegistry(_tree: Tree, _metadata: Record<string, unknown> | undefined): Promise<null> {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency(
        _tree: Tree,
        _projectGraph: ProjectGraph,
        _dependencyProjectName: string,
    ): Promise<{ currentVersion: null; dependencyCollection: null }> {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(_tree: Tree, newVersion: string): Promise<string[]> {
        return Promise.resolve([`${this.projectGraphNode.name} takes ${newVersion} from its git tag, no manifest changes`]);
    }

    override updateProjectDependencies(_tree: Tree, _projectGraph: ProjectGraph, _dependencies: Record<string, string>): Promise<string[]> {
        return Promise.resolve([]);
    }
}
