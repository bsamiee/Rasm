import { extname } from 'node:path';
import type { Tree } from '@nx/devkit';
import { Array, Effect, Equal, Option, Record, Schema, type Scope, Stream, String, Struct } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import { runCommand } from '#/eng/typescript/run-command';
import { type PluginServices, runPlugin } from '#/tools/nx/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const YAML_LAYOUT = { indent: 4, lineWidth: 0, flowCollectionPadding: false, singleQuote: true } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const treeText = (tree: Tree, file: string) => Effect.fromNullishOr(tree.read(file, 'utf-8'));
const decodeTree = <A>(tree: Tree, schema: Schema.Codec<A, string>, file: string) => Effect.flatMap(treeText(tree, file), Schema.decodeEffect(schema));
const formatted = (tree: Tree, file: string, text: string) => {
    const yaml = ['yamlfmt', '-conf', '.config/yamlfmt.yaml', '-in'] as const;
    const writers: Record.ReadonlyRecord<string, readonly [string, ...string[]]> = { '.toml': ['tombi', 'format', '-', '--quiet', `--stdin-filename=${file}`], '.swift': ['xcrun', 'swift-format', 'format', '--assume-filename', file, '-'], '.yml': yaml, '.yaml': yaml };
    const [command, ...args] = Option.getOrElse(Record.get(writers, extname(file)), () => ['biome', 'check', '--write', `--stdin-file-path=${file}`] as const);
    return runCommand(command, args, { cwd: tree.root, stdin: Stream.encodeText(Stream.make(text)) });
};
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

export { decodeTree, formatted, patched, syncGenerator, treeText, YAML_LAYOUT };
