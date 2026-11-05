import { Array, Data, Option, Record, Result, Struct } from 'effect';
import type { Row } from '#/row';

// --- [TYPES] ---------------------------------------------------------------------------

interface Pair {
    readonly before: Row;
    readonly after: Row;
}
type Change = Data.TaggedEnum<{
    Added: { readonly row: Row };
    Removed: { readonly row: Row };
    Signature: Pair;
    Exposure: Pair;
    Body: Pair;
    Renamed: Pair;
    Moved: Pair;
    Unknown: { readonly path: string };
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Change = Data.taggedEnum<Change>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const declarations = (rows: readonly Row[]) => rows.flatMap((row) => [row, ...row.members]);
const identity = (row: Row) => JSON.stringify([Option.getOrNull(row.owner), row.name]);
const counted = (rows: readonly Row[], key: (row: Row) => string) => {
    const groups = Array.groupBy(rows, key);
    return (row: Row) => groups[key(row)]?.length ?? 0;
};
const pair = (before: readonly Row[], after: readonly Row[], same: (left: Row, right: Row) => boolean) => {
    const [rest, matched] = Array.mapAccum(before, after, (open, row): [readonly Row[], Result.Result<Pair, Row>] =>
        Option.match(
            Array.findFirstIndex(open, (other) => same(row, other)),
            {
                onNone: () => [open, Result.fail(row)],
                onSome: (index) => [Array.remove(open, index), Result.succeed({ before: row, after: Array.getUnsafe(open, index) })],
            },
        ),
    );
    const [pairs, unpaired] = Array.separate(matched);
    return { pairs, before: unpaired, after: rest };
};
const classify = (before: readonly Row[], after: readonly Row[]) => {
    const exact = pair(before, after, (left, right) => left.signature === right.signature && left.public === right.public && left.body === right.body);
    const kept = pair(exact.before, exact.after, (left, right) => left.signature === right.signature);
    const rest = pair(kept.before, kept.after, () => true);
    return { changes: [...kept.pairs.map((held) => (held.before.public === held.after.public ? Change.Body(held) : Change.Exposure(held))), ...rest.pairs.map(Change.Signature)], before: rest.before, after: rest.after };
};
const compare = (before: readonly Row[], after: readonly Row[]): readonly Change[] => {
    const [old, next] = [declarations(before), declarations(after)];
    const present = new Set(next.map(Struct.get('path')));
    const unknown = new Set([...old.filter((row) => !present.has(row.path)), ...[...old, ...next].filter((row) => row.symbolType === 'null')].map(Struct.get('path')));
    const key = (row: Row) => JSON.stringify([row.path, row.symbolType, Option.getOrNull(row.owner), row.name]);
    const known = (rows: readonly Row[]) =>
        Array.groupBy(
            rows.filter((row) => !unknown.has(row.path)),
            key,
        );
    const [was, now] = [known(old), known(next)];
    const sides = Array.union(Record.keys(was), Record.keys(now)).map((held) => classify(was[held] ?? [], now[held] ?? []));
    const retyped = pair(sides.flatMap(Struct.get('before')), sides.flatMap(Struct.get('after')), (left, right) => left.path === right.path && identity(left) === identity(right));
    const [once, twice] = [counted(old, identity), counted(next, identity)];
    const moved = pair(retyped.before, retyped.after, (left, right) => left.path !== right.path && identity(left) === identity(right) && once(left) === 1 && twice(right) === 1);
    const shape = (row: Row) => JSON.stringify([row.symbolType, row.shape]);
    const [removed, added] = [counted(moved.before, shape), counted(moved.after, shape)];
    const renamed = pair(moved.before, moved.after, (left, right) => shape(left) === shape(right) && removed(left) === 1 && added(right) === 1);
    return [
        ...[...unknown].map((path) => Change.Unknown({ path })),
        ...sides.flatMap(Struct.get('changes')),
        ...retyped.pairs.map(Change.Signature),
        ...moved.pairs.map(Change.Moved),
        ...renamed.pairs.map(Change.Renamed),
        ...renamed.before.map((row) => Change.Removed({ row })),
        ...renamed.after.map((row) => Change.Added({ row })),
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Change, compare };
