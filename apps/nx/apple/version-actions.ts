import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree } from '@nx/devkit';
import { Effect, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { AppleError, core, spawned } from '#/plugins/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });
const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(effect.pipe(Effect.provide(layer)));

const versioned = Effect.fnUntraced(function* (tree: Tree, at: string, args: readonly string[]) {
    const [text, { analyzer }] = yield* Effect.all([Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => AppleError.make({ subject: at, detail: 'is missing' }))), core]);
    return yield* Effect.flatMap(spawned(yield* analyzer, ['version', ...args], { cwd: tree.root, input: text }), Schema.decodeEffect(Versioned));
});

// --- [SERVICES] ------------------------------------------------------------------------

class AppleVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    get manifest() {
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => ({ manifest, at: joinPathFragments(this.projectGraphNode.data.root, manifest) }));
    }

    override init(tree: Tree) {
        return run(
            Effect.flatMap(this.manifest, ({ manifest }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [manifest];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const located = this.manifest;
        return run(
            Effect.gen(function* () {
                const { at } = yield* located;
                const { version } = yield* versioned(tree, at, []);
                return { currentVersion: yield* Effect.fromNullishOr(version).pipe(Effect.mapError(() => AppleError.make({ subject: at, detail: 'sets no project-level MARKETING_VERSION' }))), manifestPath: at };
            }),
        );
    }

    override readCurrentVersionFromRegistry() {
        return run(Effect.flatMap(this.manifest, ({ at }) => AppleError.make({ subject: at, detail: 'names an app, which publishes through its host channel and has no registry version; use the disk or git-tag current version resolver' })));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const located = this.manifest;
        return run(
            Effect.gen(function* () {
                const { at } = yield* located;
                const { project } = yield* versioned(tree, at, ['--marketing-version', newVersion]);
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
