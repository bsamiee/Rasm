import { extname } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree } from '@nx/devkit';
import { Effect, Schema } from 'effect';
import { VersionActions } from 'nx/release';
import { AppleError, core, spawned } from '#/plugins/targets';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORT_VERSION = 'CFBundleShortVersionString';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.String });
const Kind = Schema.Literals(['.plist', '.xcproj']);
const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [TABLES] --------------------------------------------------------------------------

const EDITORS = {
    '.plist': {
        command: Effect.succeed('plutil'),
        args: (newVersion?: string) => (newVersion === undefined ? ['-extract', SHORT_VERSION, 'raw', '-o', '-', '-'] : ['-replace', SHORT_VERSION, '-string', newVersion, '-o', '-', '-']),
        read: (printed: string, newVersion?: string) => Effect.succeed({ version: newVersion ?? printed.trim(), project: printed }),
    },
    '.xcproj': {
        command: Effect.flatMap(core, ({ analyzer }) => analyzer),
        args: (newVersion?: string) => ['version', ...(newVersion === undefined ? [] : ['--marketing-version', newVersion])],
        read: (printed: string) => Schema.decodeEffect(Versioned)(printed),
    },
} as const satisfies Record<typeof Kind.Type, unknown>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => effect.pipe(Effect.provide(layer), Effect.runPromise);

const versioned = Effect.fnUntraced(function* (root: string, at: string, text: string, newVersion?: string) {
    const editor = EDITORS[yield* Effect.mapError(Schema.decodeUnknownEffect(Kind)(extname(at)), () => AppleError.make({ subject: at, detail: `is no ${Kind.literals.join(' or ')} manifest` }))];
    const printed = yield* spawned(yield* editor.command, [...editor.args(newVersion)], { cwd: root, input: text });
    return yield* editor.read(printed, newVersion);
});

const edited = (tree: Tree, at: string, newVersion?: string) =>
    Effect.flatMap(
        Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), () => AppleError.make({ subject: at, detail: 'is missing' })),
        (text) => versioned(tree.root, at, text, newVersion),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class AppleVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    get manifest() {
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => ({ file: manifest, at: joinPathFragments(this.projectGraphNode.data.root, manifest) }));
    }

    override init(tree: Tree) {
        return run(
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
        return run(
            Effect.gen(function* () {
                const { at } = yield* manifest;
                const { version } = yield* edited(tree, at);
                const currentVersion = yield* Effect.mapError(Effect.fromNullishOr(version), () => AppleError.make({ subject: at, detail: 'states no version' }));
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
        return run(
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
export { Options, versioned };
