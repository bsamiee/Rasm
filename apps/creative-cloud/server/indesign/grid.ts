// --- [IMPORTS] -------------------------------------------------------------------------

import { Count, GridDefinition, GridProblem, GridResolution } from '@rasm/typography/grid';
import { Schema, Struct } from 'effect';
import { OptionalInt } from '../values.ts';

// --- [CONTRACT] ------------------------------------------------------------------------

const _Ids: Schema.NonEmptyArray<Schema.Int> = Schema.NonEmptyArray(Schema.Int).check(Schema.isUnique());

const GridInput: Schema.Struct<{
    readonly documentId: typeof OptionalInt;
    readonly source: Schema.TaggedUnion<{
        readonly geometry: Schema.TaggedStruct<'geometry', { readonly definition: typeof GridDefinition }>;
        readonly presetGeometry: Schema.TaggedStruct<'presetGeometry', { readonly text: Schema.String }>;
    }>;
    readonly baselineStart: Schema.OptionFromOptionalKey<Schema.Finite>;
    readonly layerId: Schema.Int;
    readonly locked: Schema.Boolean;
    readonly replaceGuideIds: Schema.$Array<Schema.Int>;
    readonly targets: Schema.NonEmptyArray<
        Schema.Struct<{
            readonly layout: typeof Count;
            readonly scope: Schema.TaggedUnion<{
                readonly pages: Schema.TaggedStruct<'pages', { readonly ids: Schema.NonEmptyArray<Schema.Int> }>;
                readonly parents: Schema.TaggedStruct<'parents', { readonly ids: Schema.NonEmptyArray<Schema.Int> }>;
            }>;
        }>
    >;
}> = Schema.Struct({
    documentId: OptionalInt,
    source: Schema.TaggedUnion({ geometry: { definition: GridDefinition }, presetGeometry: { text: Schema.String } }),
    baselineStart: Schema.OptionFromOptionalKey(Schema.Finite),
    layerId: Schema.Int,
    locked: Schema.Boolean,
    replaceGuideIds: Schema.Array(Schema.Int).check(Schema.isUnique()),
    targets: Schema.NonEmptyArray(
        Schema.Struct({
            layout: Count,
            scope: Schema.TaggedUnion({ pages: { ids: _Ids }, parents: { ids: _Ids } }),
        }),
    ),
});

const _Page: Schema.Struct<{
    readonly id: Schema.Int;
    readonly layout: typeof Count;
    readonly width: Schema.Finite;
    readonly height: Schema.Finite;
    readonly margins: typeof GridResolution.fields.layouts.value.fields.margins;
    readonly columns: Schema.$Array<Schema.Finite>;
    readonly guides: Schema.$Array<
        Schema.Struct<{ readonly id: Schema.Int; readonly axis: Schema.Literals<readonly ['columns', 'rows']>; readonly location: Schema.Finite; readonly locked: Schema.Boolean }>
    >;
}> = Schema.Struct({
    id: Schema.Int,
    layout: Count,
    width: Schema.Finite,
    height: Schema.Finite,
    margins: GridResolution.fields.layouts.value.fields.margins,
    columns: Schema.Array(Schema.Finite),
    guides: Schema.Array(Schema.Struct({ id: Schema.Int, axis: Schema.Literals(['columns', 'rows']), location: Schema.Finite, locked: Schema.Boolean })),
});

const GridOutput: Schema.Union<
    readonly [
        Schema.Struct<{
            readonly kind: Schema.Literal<'gridApplied'>;
            readonly documentId: Schema.Int;
            readonly geometry: Schema.Struct<Omit<typeof GridResolution.fields, 'layouts'>>;
            readonly pages: Schema.$Array<typeof _Page>;
            readonly differences: Schema.$Array<typeof GridProblem>;
        }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'rejected'>; readonly errors: Schema.NonEmptyArray<typeof GridProblem> }>,
    ]
> = Schema.Union([
    Schema.Struct({
        kind: Schema.Literal('gridApplied'),
        documentId: Schema.Int,
        geometry: Schema.Struct(Struct.omit(GridResolution.fields, ['layouts'])),
        pages: Schema.Array(_Page),
        differences: Schema.Array(GridProblem),
    }),
    Schema.Struct({ kind: Schema.Literal('rejected'), errors: Schema.NonEmptyArray(GridProblem) }),
]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { GridInput, GridOutput };
