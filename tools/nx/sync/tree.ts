import type { Tree } from '@nx/devkit';
import { Array, Effect, Schema, type Scope, Struct } from 'effect';
import { parseAllDocuments } from 'yaml';
import { type PluginServices, runPlugin, WorkspaceError } from '#/workspace.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const YAML_LAYOUT = { indent: 4, lineWidth: 0, flowCollectionPadding: false, singleQuote: true } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const treeText = (tree: Tree, file: string) => Effect.fromNullishOr(tree.read(file, 'utf-8'));
const decodeTree = <A>(tree: Tree, schema: Schema.Codec<A, string>, file: string) => Effect.flatMap(treeText(tree, file), Schema.decodeEffect(schema));
const yamlDocuments = (text: string, file: string) => {
    const documents = parseAllDocuments(text);
    const errors = 'errors' in documents ? documents.errors : documents.flatMap((document) => document.errors);
    return Array.match(errors, { onEmpty: () => Effect.succeed(documents), onNonEmpty: (failures) => Effect.fail(WorkspaceError.unparsed(file, failures.map(({ message }) => message).join('\n'))) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const syncGenerator =
    <E>(outOfSyncMessage: string, rows: (tree: Tree) => Effect.Effect<readonly { readonly file: string; readonly next?: string }[], E, Scope.Scope | PluginServices>) =>
    (tree: Tree) =>
        runPlugin(
            Effect.map(rows(tree), (desired) => {
                const drifted = desired.filter(({ file, next }) => (tree.read(file, 'utf-8') ?? undefined) !== next);
                drifted.forEach(({ file, next }) => (next === undefined ? tree.delete(file) : tree.write(file, next)));
                return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage, outOfSyncDetails: changed.map(Struct.get('file')) }) });
            }),
        );

// --- [EXPORTS] -------------------------------------------------------------------------

export { decodeTree, syncGenerator, treeText, YAML_LAYOUT, yamlDocuments };
