// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, identity, Record, Schema, Struct } from 'effect';
import { type AppliedReply, applied, type PreferencesReply, preferences as readable } from '../errors.ts';
import { Execute } from '../frames.ts';
import { Bounds, Dpi, PixelBudget, Region } from '../images.ts';
import { AbsolutePath, OptionalString, PageIndex } from '../values.ts';
import { members, preferences } from './indesign.ts';

// --- [TABLE] ---------------------------------------------------------------------------

const FAMILIES = {
    'latin.sans': 'TTCommonsPro-Rg',
    'latin.serif': 'AGaramondPro-Regular',
    'latin.mono': 'LetterGothicStd',
    persian: 'NotoSansArabic-Regular',
    arabic: 'NotoSansArabic-Regular',
} as const;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LIMIT = { minimum: 1, maximum: 200 } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type Format = (typeof Format)['Type'];
type Item = (typeof Item)['Type'];
type Kind = keyof (typeof Bodies)['fields'];
type Body<K extends Kind> = (typeof Bodies)['fields'][K]['Type'];
type Reply<K extends Kind> = (typeof Results)['fields'][K]['Type'];
type Target = (typeof Target)['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const _cursor = PageIndex.pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)));
const _next = Schema.OptionFromNullOr(Schema.Int);
const _names = Schema.Array(Schema.String);
const _off = Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false)));
const _section = Schema.Literals(Struct.keys(preferences));
const _keyed: { readonly path: Schema.String } = { path: Schema.String };

const Target: Schema.toTaggedUnion<
    'kind',
    readonly [
        Schema.Struct<{ readonly kind: Schema.Literal<'page'>; readonly index: Schema.Int; readonly budget: typeof PixelBudget }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'spread'>; readonly index: Schema.Int; readonly budget: typeof PixelBudget }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'region'>; readonly index: Schema.Int; readonly region: typeof Region }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'object'>; readonly itemId: Schema.Int; readonly isolate: Schema.withDecodingDefaultKey<Schema.Boolean> }>,
    ]
> = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('page'), index: PageIndex, budget: PixelBudget }),
    Schema.Struct({ kind: Schema.Literal('spread'), index: PageIndex, budget: PixelBudget }),
    Schema.Struct({ kind: Schema.Literal('region'), index: PageIndex, region: Region }),
    Schema.Struct({ kind: Schema.Literal('object'), itemId: Schema.Int, isolate: _off }),
]).pipe(Schema.toTaggedUnion('kind'));

const Format: Schema.toTaggedUnion<
    'kind',
    readonly [Schema.Struct<{ readonly kind: Schema.Literal<'jpg'> }>, Schema.Struct<{ readonly kind: Schema.Literal<'png'>; readonly transparent: Schema.withDecodingDefaultKey<Schema.Boolean> }>]
> = Schema.Union([Schema.Struct({ kind: Schema.Literal('jpg') }), Schema.Struct({ kind: Schema.Literal('png'), transparent: _off })]).pipe(Schema.toTaggedUnion('kind'));

const Item: Schema.Struct<{ readonly id: Schema.Int; readonly type: Schema.String; readonly name: Schema.String; readonly bounds: typeof Bounds; readonly hasGraphic: Schema.Boolean }> = Schema.Struct(
    {
        id: Schema.Int,
        type: Schema.String,
        name: Schema.String,
        bounds: Bounds,
        hasGraphic: Schema.Boolean,
    },
);

const Applied: AppliedReply<typeof _keyed> = applied(_keyed);

// --- [KINDS] ---------------------------------------------------------------------------

const Bodies: Schema.Struct<{
    readonly execute: typeof Execute;
    readonly listEnums: Schema.Struct<{ readonly name: Schema.OptionFromOptionalKey<Schema.String> }>;
    readonly snapshot: Schema.Struct<{ readonly target: typeof Target; readonly format: typeof Format; readonly directory: typeof AbsolutePath }>;
    readonly getLayout: Schema.Struct<{
        readonly includeItems: Schema.withDecodingDefaultKey<Schema.Boolean>;
        readonly pageCursor: Schema.withDecodingDefaultKey<Schema.Int>;
        readonly itemCursor: Schema.withDecodingDefaultKey<Schema.Int>;
        readonly limit: Schema.withDecodingDefaultKey<Schema.Int>;
    }>;
    readonly findKeyStrings: Schema.Struct<{ readonly text: Schema.String }>;
    readonly getPreferences: Schema.Struct<{ readonly sections: Schema.NonEmptyArray<Schema.Literals<Array<keyof typeof preferences>>> }>;
    readonly setPreferences: Schema.Struct<{
        readonly values: Schema.NonEmptyArray<
            Schema.Struct<{ readonly section: Schema.Literals<Array<keyof typeof preferences>>; readonly key: Schema.String; readonly value: Schema.Codec<Schema.Json> }>
        >;
    }>;
    readonly setTextDefaults: Schema.Struct<{
        readonly scope: Schema.Literals<readonly ['application', 'document']>;
        readonly values: Schema.NonEmptyArray<Schema.Struct<{ readonly key: Schema.Literals<string[]>; readonly value: Schema.Codec<Schema.Json> }>>;
    }>;
}> = Schema.Struct({
    execute: Execute,
    listEnums: Schema.Struct({ name: OptionalString }),
    snapshot: Schema.Struct({ target: Target, format: Format, directory: AbsolutePath }),
    getLayout: Schema.Struct({
        includeItems: _off,
        pageCursor: _cursor,
        itemCursor: _cursor,
        limit: Schema.Int.pipe(Schema.check(Schema.isBetween(_LIMIT)), Schema.withDecodingDefaultKey(Effect.succeed(_LIMIT.maximum))),
    }),
    findKeyStrings: Schema.Struct({ text: Schema.String }),
    getPreferences: Schema.Struct({ sections: Schema.NonEmptyArray(_section) }),
    setPreferences: Schema.Struct({ values: Schema.NonEmptyArray(Schema.Struct({ section: _section, key: Schema.String, value: Schema.Json })) }),
    setTextDefaults: Schema.Struct({
        scope: Schema.Literals(['application', 'document']),
        values: Schema.NonEmptyArray(Schema.Struct({ key: Schema.Literals(Struct.keys(Record.filter(members.TextDefault, identity))), value: Schema.Json })),
    }),
});

const Results: Schema.Struct<{
    readonly execute: Schema.Codec<Schema.Json>;
    readonly listEnums: Schema.Struct<{
        readonly kind: Schema.Literal<'enums'>;
        readonly enums: Schema.$Array<
            Schema.Struct<{ readonly name: Schema.String; readonly constants: Schema.$Array<Schema.Struct<{ readonly name: Schema.String; readonly value: Schema.OptionFromNullOr<Schema.Int> }>> }>
        >;
        readonly functions: Schema.$Array<Schema.String>;
    }>;
    readonly snapshot: Schema.Struct<{
        readonly kind: Schema.Literal<'image'>;
        readonly path: typeof AbsolutePath;
        readonly widthPx: Schema.Int;
        readonly heightPx: Schema.Int;
        readonly contentWidthPx: Schema.Int;
        readonly contentHeightPx: Schema.Int;
        readonly dpi: typeof Dpi;
        readonly effectivePpi: Schema.OptionFromNullOr<Schema.Number>;
        readonly isolated: Schema.Boolean;
        readonly overlaps: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String; readonly type: Schema.String }>>;
    }>;
    readonly getLayout: Schema.Struct<{
        readonly kind: Schema.Literal<'layout'>;
        readonly facingPages: Schema.Boolean;
        readonly measurementUnits: Schema.Struct<{ readonly horizontal: Schema.String; readonly vertical: Schema.String }>;
        readonly pages: Schema.$Array<
            Schema.Struct<{
                readonly index: Schema.Int;
                readonly id: Schema.Int;
                readonly name: Schema.String;
                readonly documentOffset: Schema.Int;
                readonly side: Schema.String;
                readonly bounds: typeof Bounds;
                readonly margins: Schema.Struct<{ readonly top: Schema.Number; readonly bottom: Schema.Number; readonly inside: Schema.Number; readonly outside: Schema.Number }>;
                readonly contentArea: typeof Bounds;
                readonly guides: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly orientation: Schema.String; readonly location: Schema.Number }>>;
                readonly items: Schema.OptionFromOptionalKey<Schema.$Array<typeof Item>>;
            }>
        >;
        readonly pageCount: Schema.Int;
        readonly pageCursor: Schema.OptionFromNullOr<Schema.Int>;
        readonly itemCount: Schema.Int;
        readonly itemCursor: Schema.OptionFromNullOr<Schema.Int>;
    }>;
    readonly findKeyStrings: Schema.Struct<{ readonly kind: Schema.Literal<'keys'>; readonly keys: Schema.$Array<Schema.String>; readonly translated: Schema.String }>;
    readonly getPreferences: PreferencesReply<typeof _keyed>;
    readonly setPreferences: Schema.Union<
        readonly [
            typeof Applied,
            Schema.Struct<{
                readonly kind: Schema.Literal<'rejected'>;
                readonly reason: Schema.TaggedUnion<{ readonly documentOpen: Schema.TaggedStruct<'documentOpen', { readonly count: Schema.Int }> }>;
            }>,
        ]
    >;
    readonly setTextDefaults: typeof Applied;
}> = Schema.Struct({
    execute: Schema.Json,
    listEnums: Schema.Struct({
        kind: Schema.Literal('enums'),
        enums: Schema.Array(Schema.Struct({ name: Schema.String, constants: Schema.Array(Schema.Struct({ name: Schema.String, value: _next })) })),
        functions: _names,
    }),
    snapshot: Schema.Struct({
        kind: Schema.Literal('image'),
        path: AbsolutePath,
        widthPx: Schema.Int,
        heightPx: Schema.Int,
        contentWidthPx: Schema.Int,
        contentHeightPx: Schema.Int,
        dpi: Dpi,
        effectivePpi: Schema.OptionFromNullOr(Schema.Number),
        isolated: Schema.Boolean,
        overlaps: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, type: Schema.String })),
    }),
    getLayout: Schema.Struct({
        kind: Schema.Literal('layout'),
        facingPages: Schema.Boolean,
        measurementUnits: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String }),
        pages: Schema.Array(
            Schema.Struct({
                index: Schema.Int,
                id: Schema.Int,
                name: Schema.String,
                documentOffset: Schema.Int,
                side: Schema.String,
                bounds: Bounds,
                margins: Schema.Struct({ top: Schema.Number, bottom: Schema.Number, inside: Schema.Number, outside: Schema.Number }),
                contentArea: Bounds,
                guides: Schema.Array(Schema.Struct({ id: Schema.Int, orientation: Schema.String, location: Schema.Number })),
                items: Schema.OptionFromOptionalKey(Schema.Array(Item)),
            }),
        ),
        pageCount: Schema.Int,
        pageCursor: _next,
        itemCount: Schema.Int,
        itemCursor: _next,
    }),
    findKeyStrings: Schema.Struct({ kind: Schema.Literal('keys'), keys: _names, translated: Schema.String }),
    getPreferences: readable(_keyed),
    setPreferences: Schema.Union([Applied, Schema.Struct({ kind: Schema.Literal('rejected'), reason: Schema.TaggedUnion({ documentOpen: { count: Schema.Int } }) })]),
    setTextDefaults: Applied,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Body, Format, Item, Kind, Reply, Target };
export { Applied, Bodies, FAMILIES, Results };
