import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Equal, Layer, ManagedRuntime, Record, Schema, type Scope, Stream, String, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { runCommand } from '#/eng/typescript/run-command';

// --- [TYPES] ---------------------------------------------------------------------------

type PluginServices = ManagedRuntime.ManagedRuntime.Services<typeof pluginRuntime>;

// --- [COMPOSITION] ---------------------------------------------------------------------

const pluginRuntime = ManagedRuntime.make(Layer.merge(layer, layerPosix));
const runPlugin = <A, E>(effect: Effect.Effect<A, E, Scope.Scope | PluginServices>) => pluginRuntime.runPromise(Effect.scoped(effect));
const treeText = (tree: Tree, file: string) => Effect.fromNullishOr(tree.read(file, 'utf-8'));
const decodeTree = <A>(tree: Tree, schema: Schema.Codec<A, string>, file: string) => Effect.flatMap(treeText(tree, file), Schema.decodeEffect(schema));
const formatted = (tree: Tree, file: string, text: string) => runCommand('biome', ['check', '--write', `--stdin-file-path=${file}`], { cwd: tree.root, stdin: Stream.encodeText(Stream.make(text)) });
const patched = Effect.fnUntraced(function* (tree: Tree, file: string, current: Readonly<Record<string, unknown>>, rows: Readonly<Record<string, unknown>>) {
    const text = yield* treeText(tree, file);
    const moved = Record.toEntries(rows).filter(([key, value]) => !Equal.equals(current[key], value));
    const next = moved.reduce((held, [key, value]) => applyEdits(held, modify(held, [key], value, { getInsertionIndex: (properties) => properties.findLastIndex(String.startsWith(Array.headNonEmpty(String.split(key, '.')))) + 1 })), text);
    return { file, next: Array.isArrayNonEmpty(moved) ? yield* formatted(tree, file, next) : text };
});
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

export { decodeTree, formatted, patched, pluginRuntime, runPlugin, syncGenerator, treeText };
