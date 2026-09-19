// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Equivalence, HashMap, HashSet, Option, Order, Schema } from 'effect';
import { Unavailable } from './channel.ts';

// --- [CAD] ----------------------------------------------------------------------------

const _layerPath: Schema.NonEmptyArray<Schema.String> = Schema.NonEmptyArray(Schema.String);

const Cad: Schema.Struct<{
    readonly scope: Schema.Literals<readonly ['selection', 'document']>;
    readonly weights: Schema.NonEmptyArray<Schema.Finite>;
    readonly patterns: Schema.$Array<Schema.Struct<{ readonly from: Schema.NonEmptyString; readonly to: Schema.NonEmptyString }>>;
    readonly layers: Schema.$Array<
        Schema.Struct<{
            readonly path: Schema.NonEmptyArray<Schema.String>;
            readonly color: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number]>;
            readonly visible: Schema.Boolean;
            readonly locked: Schema.Boolean;
            readonly printable: Schema.Boolean;
        }>
    >;
    readonly layerMap: Schema.$Array<Schema.Struct<{ readonly from: Schema.NonEmptyArray<Schema.String>; readonly to: Schema.NonEmptyArray<Schema.String> }>>;
    readonly removeStrays: Schema.Boolean;
    readonly removeEmptyGroups: Schema.Boolean;
}> = Schema.Struct({
    scope: Schema.Literals(['selection', 'document']),
    weights: Schema.NonEmptyArray(Schema.Finite.check(Schema.isGreaterThan(0))).check(Schema.isUnique()),
    patterns: Schema.Array(Schema.Struct({ from: Schema.NonEmptyString, to: Schema.NonEmptyString })).check(
        Schema.makeFilter((rows) => Array.dedupeWith(rows, (a, b) => a.from === b.from).length === rows.length, { message: 'Each source pattern has one destination.' }),
    ),
    layers: Schema.Array(
        Schema.Struct({
            path: _layerPath,
            color: Schema.Number.check(Schema.isFinite(), Schema.isBetween({ minimum: 0, maximum: 255 })).pipe((channel) => Schema.Tuple([channel, channel, channel])),
            visible: Schema.Boolean,
            locked: Schema.Boolean,
            printable: Schema.Boolean,
        }),
    ).check(
        Schema.makeFilter((rows) => Array.dedupeWith(rows, (a, b) => Equivalence.Array(Equivalence.String)(a.path, b.path)).length === rows.length, {
            message: 'Every layer path identifies one taxonomy row.',
        }),
        Schema.makeFilter((rows) => Array.every(rows, (row) => row.path.length === 1 || Array.some(rows, (parent) => Equivalence.Array(Equivalence.String)(parent.path, row.path.slice(0, -1)))), {
            message: 'Every nested layer includes its parent in the taxonomy.',
        }),
    ),
    layerMap: Schema.Array(Schema.Struct({ from: _layerPath, to: _layerPath })).check(
        Schema.makeFilter((rows) => Array.dedupeWith(rows, (a, b) => Equivalence.Array(Equivalence.String)(a.from, b.from)).length === rows.length, {
            message: 'Every source layer has one destination.',
        }),
    ),
    removeStrays: Schema.Boolean,
    removeEmptyGroups: Schema.Boolean,
});

const CadState: Schema.Struct<{
    readonly kind: Schema.Literal<'cad'>;
    readonly layers: Schema.$Array<Schema.NonEmptyArray<Schema.String>>;
    readonly roots: Schema.$Array<Schema.Struct<{ readonly uuid: Schema.NonEmptyString; readonly layer: Schema.NonEmptyArray<Schema.String> }>>;
    readonly paths: Schema.$Array<
        Schema.Struct<{
            readonly uuid: Schema.NonEmptyString;
            readonly points: Schema.Int;
            readonly stroked: Schema.Boolean;
            readonly weight: Schema.Finite;
            readonly pattern: Schema.OptionFromNullOr<Schema.String>;
        }>
    >;
    readonly groups: Schema.$Array<Schema.NonEmptyString>;
    readonly patterns: Schema.$Array<Schema.String>;
    readonly unavailable: Schema.$Array<typeof Unavailable>;
}> = Schema.Struct({
    kind: Schema.Literal('cad'),
    layers: Schema.Array(_layerPath),
    roots: Schema.Array(Schema.Struct({ uuid: Schema.NonEmptyString, layer: _layerPath })),
    paths: Schema.Array(
        Schema.Struct({
            uuid: Schema.NonEmptyString,
            points: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)),
            stroked: Schema.Boolean,
            weight: Schema.Finite,
            pattern: Schema.OptionFromNullOr(Schema.String),
        }),
    ),
    groups: Schema.Array(Schema.NonEmptyString),
    patterns: Schema.Array(Schema.String),
    unavailable: Schema.Array(Unavailable),
});

const CadPlan: Schema.Struct<{
    readonly mode: Schema.Literal<'write'>;
    readonly layers: typeof Cad.fields.layers;
    readonly paths: Schema.$Array<
        Schema.Struct<{
            readonly uuid: Schema.NonEmptyString;
            readonly remove: Schema.Boolean;
            readonly weight: Schema.OptionFromNullOr<Schema.Number>;
            readonly pattern: Schema.OptionFromNullOr<Schema.String>;
        }>
    >;
    readonly moves: Schema.$Array<Schema.Struct<{ readonly uuid: Schema.NonEmptyString; readonly layer: Schema.NonEmptyArray<Schema.String> }>>;
    readonly groups: Schema.$Array<Schema.NonEmptyString>;
}> = Schema.Struct({
    mode: Schema.Literal('write'),
    layers: Cad.fields.layers,
    paths: Schema.Array(
        Schema.Struct({
            uuid: Schema.NonEmptyString,
            remove: Schema.Boolean,
            weight: Schema.OptionFromNullOr(Schema.Number),
            pattern: Schema.OptionFromNullOr(Schema.String),
        }),
    ),
    moves: Schema.Array(Schema.Struct({ uuid: Schema.NonEmptyString, layer: _layerPath })),
    groups: Schema.Array(Schema.NonEmptyString),
});

const CadRejection: Schema.Struct<{
    readonly reason: Schema.Literals<readonly ['patternMissing', 'sourceLayerMissing', 'sourceLayerAmbiguous', 'targetLayerMissing', 'targetLayerAmbiguous']>;
    readonly path: Schema.$Array<Schema.String>;
}> = Schema.Struct({
    reason: Schema.Literals(['patternMissing', 'sourceLayerMissing', 'sourceLayerAmbiguous', 'targetLayerMissing', 'targetLayerAmbiguous']),
    path: Schema.Array(Schema.String),
});

const planCad = (input: typeof Cad.Type, before: typeof CadState.Type): Effect.Effect<typeof CadPlan.Type, Array.NonEmptyReadonlyArray<typeof CadRejection.Type>> =>
    Effect.gen(function* () {
        const same = Equivalence.Array(Equivalence.String);
        const patterns = HashMap.fromIterable(Array.map(input.patterns, ({ from, to }) => [from, to] as const));
        const removed = HashSet.fromIterable(
            Array.map(
                Array.filter(before.paths, (path) => input.removeStrays && path.points < 2),
                ({ uuid }) => uuid,
            ),
        );
        yield* Array.match(
            [
                ...Array.map(
                    Array.filter(input.patterns, ({ to }) => !Array.contains(before.patterns, to)),
                    ({ to }) => ({ reason: 'patternMissing' as const, path: [to] }),
                ),
                ...Array.flatMap(input.layerMap, ({ from, to }) => {
                    const sources = Array.filter(before.layers, (path) => same(path, from));
                    return [
                        ...(sources.length === 1 ? [] : [{ reason: sources.length === 0 ? ('sourceLayerMissing' as const) : ('sourceLayerAmbiguous' as const), path: from }]),
                        ...(Array.some(input.layers, ({ path }) => same(path, to)) ? [] : [{ reason: 'targetLayerMissing' as const, path: to }]),
                    ];
                }),
                ...Array.map(
                    Array.filter(input.layers, ({ path }) => Array.filter(before.layers, (candidate) => same(candidate, path)).length > 1),
                    ({ path }) => ({ reason: 'targetLayerAmbiguous' as const, path }),
                ),
            ],
            { onEmpty: () => Effect.void, onNonEmpty: Effect.fail },
        );
        return {
            mode: 'write',
            layers: Array.sortWith(input.layers, (row) => row.path.length, Order.Number),
            paths: Array.map(before.paths, (path) => ({
                uuid: path.uuid,
                remove: input.removeStrays && path.points < 2,
                weight: path.stroked
                    ? Option.some(
                          Array.min(
                              input.weights,
                              Order.combine(
                                  Order.mapInput(Order.Number, (weight: number) => Math.abs(weight - path.weight)),
                                  Order.Number,
                              ),
                          ),
                      )
                    : Option.none(),
                pattern: Option.flatMap(path.pattern, (pattern) => HashMap.get(patterns, pattern)),
            })),
            moves: Array.flatMap(
                Array.filter(before.roots, (item) => !HashSet.has(removed, item.uuid)),
                (item: (typeof CadState.Type.roots)[number]) =>
                    Option.map(
                        Array.findFirst(input.layerMap, (row) => same(row.from, item.layer) && !same(row.to, item.layer)),
                        (row) => ({ uuid: item.uuid, layer: row.to }),
                    ).pipe(Option.toArray),
            ),
            groups: input.removeEmptyGroups ? Array.reverse(before.groups) : [],
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Cad, CadPlan, CadRejection, CadState, planCad };
