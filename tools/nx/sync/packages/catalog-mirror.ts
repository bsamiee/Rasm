import type { Tree } from '@nx/devkit';
import { Array, Effect, Equal, Order, Predicate, Record, Tuple } from 'effect';
import { Package, Workspace } from '#/eng/typescript/manifest';
import { decodeTree, patched, syncGenerator } from '#/tools/nx/sync/tree';

// --- [OPERATIONS] ----------------------------------------------------------------------

const rows = Effect.fnUntraced(function* (tree: Tree) {
    const mirrored = 'catalog:';
    const [{ catalog }, { devDependencies }] = yield* Effect.all([decodeTree(tree, Workspace.schema, Workspace.file), decodeTree(tree, Package.schema, Package.file)]);
    return [yield* patched(tree, Package.file, { devDependencies }, { devDependencies: Record.fromEntries(Array.sortWith(Record.toEntries({ ...Record.filter(devDependencies, Predicate.not(Equal.equals(mirrored))), ...Record.map(catalog, () => mirrored) }), Tuple.get(0), Order.String)) })];
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const catalogMirror = syncGenerator(`Root devDependencies differ from the ${Workspace.file} catalog`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalogMirror };
