import { joinPathFragments, type Tree } from '@nx/devkit';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { Effect, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { VersionActionsOptions, versioned } from '#/targets';

// --- [OPERATIONS] ----------------------------------------------------------------------

const edited = (tree: Tree, at: string, newVersion?: string) =>
    Effect.flatMap(
        Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), () => ToolError.make({ subject: at, detail: 'is missing' })),
        (text) => versioned(tree.root, at, text, newVersion),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class AppleVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    get manifest() {
        return Effect.map(Schema.decodeUnknownEffect(VersionActionsOptions)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => ({ file: manifest, at: joinPathFragments(this.projectGraphNode.data.root, manifest) }));
    }

    override init(tree: Tree) {
        return runtime.runPromise(
            Effect.flatMap(this.manifest, ({ file }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [file];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const { manifest } = this;
        return runtime.runPromise(
            Effect.gen(function* () {
                const { at } = yield* manifest;
                const { version } = yield* edited(tree, at);
                const currentVersion = yield* Effect.mapError(Effect.fromNullishOr(version), () => ToolError.make({ subject: at, detail: 'states no version' }));
                return { currentVersion, manifestPath: at };
            }),
        );
    }

    override readCurrentVersionFromRegistry() {
        return Promise.resolve(null);
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const { manifest } = this;
        return runtime.runPromise(
            Effect.gen(function* () {
                const { at } = yield* manifest;
                const { project } = yield* edited(tree, at, newVersion);
                tree.write(at, project);
                return [`New version ${newVersion} written to manifest: ${at}`];
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default AppleVersionActions;
