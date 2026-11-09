import { joinPathFragments, type Tree } from '@nx/devkit';
import ast from '@xml-tools/ast';
import xml, { type DocumentCstNode } from '@xml-tools/parser';
import { Array, Effect, Option, pipe, Schema, SemVer } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { VersionActions } from 'nx/release';
import { PACKAGES, TypeScriptError } from '#/plugins/adobe-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ manifest: Schema.Literals([PACKAGES.uxp.manifest, PACKAGES.zxpsigncmd.manifest]) });
const Uxp = Schema.fromJsonString(Schema.Struct({ version: Schema.String }));

// --- [TABLES] --------------------------------------------------------------------------

const MANIFESTS = {
    [PACKAGES.uxp.manifest]: (text: string) => Option.map(Schema.decodeOption(Uxp)(text), ({ version }) => ({ version, write: (next: string) => applyEdits(text, modify(text, ['version'], next, {})) })),
    [PACKAGES.zxpsigncmd.manifest]: (text: string) => {
        const { cst, tokenVector } = xml.parse(text);
        return pipe(
            Option.liftPredicate(cst, (node): node is DocumentCstNode => node.name === 'document'),
            Option.flatMap((document) => Option.fromNullOr(ast.buildAst(document, tokenVector).rootElement)),
            Option.flatMap(({ attributes }) => Array.findFirst(attributes, ({ key, value, syntax }) => (key === 'ExtensionBundleVersion' && value !== null && syntax.value !== undefined ? Option.some({ version: value, at: syntax.value }) : Option.none()))),
            Option.map(({ version, at }) => ({ version, write: (next: string) => `${text.slice(0, at.startOffset + 1)}${next}${text.slice(at.endOffset)}` })),
        );
    },
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const located = Effect.fnUntraced(function* (tree: Tree, root: string, options: unknown) {
    const { manifest } = yield* Schema.decodeUnknownEffect(Options)(options);
    const at = joinPathFragments(root, manifest);
    const text = yield* Effect.fromNullishOr(tree.read(at, 'utf-8')).pipe(Effect.mapError(() => TypeScriptError.make({ subject: at, detail: 'is missing' })));
    return { at, ...(yield* Effect.fromOption(MANIFESTS[manifest](text), () => TypeScriptError.make({ subject: at, detail: 'states no version' }))) };
});

// --- [SERVICES] ------------------------------------------------------------------------

class AdobeVersionActions extends VersionActions {
    override validManifestFilenames: string[] = [];
    override readonly excludeManifestsFromFormatting = true;

    override init(tree: Tree) {
        return Effect.runPromise(
            Effect.flatMap(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) =>
                Effect.promise(() => {
                    this.validManifestFilenames = [manifest];
                    return super.init(tree);
                }),
            ),
        );
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return Effect.runPromise(Effect.map(located(tree, this.projectGraphNode.data.root, this.finalConfigForProject.versionActionsOptions), ({ at, version }) => ({ currentVersion: version, manifestPath: at })));
    }

    override readCurrentVersionFromRegistry() {
        return Effect.runPromise(Effect.fail(TypeScriptError.make({ subject: this.projectGraphNode.name, detail: `ships through Adobe's portals and GitHub release files, which hold no registry version; use the disk or git-tag current version resolver` })));
    }

    override readCurrentVersionOfDependency() {
        return Promise.resolve({ currentVersion: null, dependencyCollection: null });
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        const { root } = this.projectGraphNode.data;
        const options = this.finalConfigForProject.versionActionsOptions;
        return Effect.runPromise(
            Effect.gen(function* () {
                const { at, write } = yield* located(tree, root, options);
                yield* Effect.filterOrFail(
                    Effect.fromResult(SemVer.parse(newVersion)),
                    (version) => !(SemVer.isPrerelease(version) || SemVer.hasBuild(version)),
                    () => TypeScriptError.make({ subject: at, detail: `takes a major.minor.patch version alone, which ${newVersion} is not` }),
                );
                tree.write(at, write(newVersion));
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
