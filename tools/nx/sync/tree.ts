import type { Tree } from '@nx/devkit';
import { Array, Effect, Equal, Record, Schema, type Scope, Stream, String, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { parseAllDocuments } from 'yaml';
import { runCommand } from '#/eng/typescript/run-command';
import { type PluginServices, runPlugin, WorkspaceError } from '#/tools/nx/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const treeText = (tree: Tree, file: string) => Effect.fromNullishOr(tree.read(file, 'utf-8'));
const decodeTree = <A>(tree: Tree, schema: Schema.Codec<A, string>, file: string) => Effect.flatMap(treeText(tree, file), Schema.decodeEffect(schema));
const yamlDocuments = (text: string, file: string) => {
    const documents = parseAllDocuments(text);
    const errors = 'errors' in documents ? documents.errors : documents.flatMap((document) => document.errors);
    return Array.match(errors, { onEmpty: () => Effect.succeed(documents), onNonEmpty: (failures) => Effect.fail(WorkspaceError.unparsed(file, failures.map(({ message }) => message).join('\n'))) });
};
const formatted = (tree: Tree, file: string, text: string) => runCommand('biome', ['check', '--write', `--stdin-file-path=${file}`], { cwd: tree.root, stdin: Stream.encodeText(Stream.make(text)) });
const patched = Effect.fnUntraced(function* (tree: Tree, file: string, current: Readonly<Record<string, unknown>>, rows: Readonly<Record<string, unknown>>) {
    const text = yield* treeText(tree, file);
    const moved = Record.toEntries(rows).filter(([key, value]) => !Equal.equals(current[key], value));
    const next = moved.reduce((held, [key, value]) => applyEdits(held, modify(held, [key], value, { getInsertionIndex: (properties) => properties.findLastIndex(String.startsWith(Array.headNonEmpty(String.split(key, '.')))) + 1 })), text);
    return { file, next: Array.isArrayNonEmpty(moved) ? yield* formatted(tree, file, next) : text };
});

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

export { decodeTree, formatted, patched, syncGenerator, treeText, yamlDocuments };
