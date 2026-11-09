import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { joinPathFragments, type Tree } from '@nx/devkit';
import { Effect, Schema, Stream } from 'effect';
import { Xml } from 'effect/encoding';
import { applyEdits, modify } from 'jsonc-parser';
import { VersionActions } from 'nx/release';
import { PACKAGES } from '#/plugins/adobe-packages';
import { TypeScriptError } from '#/plugins/project-graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NUMERIC = /^\d+\.\d+\.\d+$/u;
const BUNDLE = /(?<open>\bExtensionBundleVersion=")[^"]*(?<close>")/u;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.Literals([PACKAGES.uxp.manifest, PACKAGES.zxpsigncmd.manifest]) });
const Uxp = Schema.fromJsonString(Schema.Struct({ version: Schema.String }));
const Cep = Schema.Struct({ ExtensionBundleVersion: Schema.String });

// --- [TABLES] --------------------------------------------------------------------------

const MANIFESTS = {
    [PACKAGES.uxp.manifest]: {
        read: (text: string) => Effect.map(Schema.decodeEffect(Uxp)(text), ({ version }) => version),
        write: (text: string, version: string) => applyEdits(text, modify(text, ['version'], version, {})),
    },
    [PACKAGES.zxpsigncmd.manifest]: {
        read: (text: string) =>
            Stream.make(text).pipe(
                Stream.pipeThroughChannel(Xml.decodeString()),
                Stream.pipeThroughChannel(Xml.elements(['ExtensionManifest'])),
                Stream.runHead,
                Effect.flatMap(Effect.fromOption),
                Effect.flatMap(({ attributes }) => Schema.decodeUnknownEffect(Cep)(attributes)),
                Effect.map(({ ExtensionBundleVersion }) => ExtensionBundleVersion),
            ),
        write: (text: string, version: string) => text.replace(BUNDLE, `$<open>${version}$<close>`),
    },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices>) => Effect.runPromise(effect.pipe(Effect.provide(layer)));

const located = Effect.fnUntraced(function* (tree: Tree, root: string, options: unknown) {
    const { manifest } = yield* Schema.decodeUnknownEffect(Options)(options);
    const at = joinPathFragments(root, manifest);
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => TypeScriptError.make({ message: `${at} is missing` })));
    return { at, text, ...MANIFESTS[manifest] };
});

// --- [SERVICES] ------------------------------------------------------------------------

class AdobeVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    override init(tree: Tree) {
        return run(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [manifest];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const { root } = this.projectGraphNode.data;
        const options = this.finalConfigForProject.versionActionsOptions;
        return run(
            Effect.gen(function* () {
                const { at, text, read } = yield* located(tree, root, options);
                return { currentVersion: yield* Effect.mapError(read(text), (cause) => TypeScriptError.make({ message: `${at} states no version`, cause })), manifestPath: at };
            }),
        );
    }

    override readCurrentVersionFromRegistry() {
        return run(Effect.fail(TypeScriptError.make({ message: `${this.projectGraphNode.name} ships through Adobe's portals and GitHub release files, which hold no registry version; use the disk or git-tag current version resolver` })));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const { root } = this.projectGraphNode.data;
        const options = this.finalConfigForProject.versionActionsOptions;
        return run(
            Effect.gen(function* () {
                const { at, text, write } = yield* located(tree, root, options);
                yield* NUMERIC.test(newVersion) ? Effect.void : TypeScriptError.make({ message: `${at} takes a major.minor.patch version alone, which ${newVersion} is not` });
                tree.write(at, write(text, newVersion));
                return [`New version ${newVersion} written to manifest: ${at}`];
            }),
        );
    }

    override updateProjectDependencies() {
        return Promise.resolve([]);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export default AdobeVersionActions;
