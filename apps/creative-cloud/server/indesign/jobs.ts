// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, identity, Record, Schema, Struct } from 'effect';
import { Execute } from '../frames.ts';
import { Dpi, PixelBudget, Region } from '../images.ts';
import { AbsolutePath, PageIndex } from '../values.ts';
import { members, preferences } from './indesign.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LIMIT = { minimum: 1, maximum: 200 } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type Rejection = (typeof Rejection)['Type'];
type Target = (typeof Target)['Type'];
type Format = (typeof Format)['Type'];
type Item = (typeof _Item)['Type'];
type Kind = (typeof Kind)['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const _box: { readonly top: Schema.Number; readonly left: Schema.Number; readonly bottom: Schema.Number; readonly right: Schema.Number } = {
    top: Schema.Number,
    left: Schema.Number,
    bottom: Schema.Number,
    right: Schema.Number,
};
const _named: { readonly path: Schema.String } = { path: Schema.String };
const _Box: Schema.Struct<typeof _box> = Schema.Struct(_box);
const _Item: Schema.Struct<{ readonly id: Schema.Int; readonly type: Schema.String; readonly name: Schema.String; readonly bounds: typeof _Box; readonly hasGraphic: Schema.Boolean }> = Schema.Struct({
    id: Schema.Int,
    type: Schema.String,
    name: Schema.String,
    bounds: _Box,
    hasGraphic: Schema.Boolean,
});
const _cursor: Schema.withDecodingDefaultKey<Schema.Int> = PageIndex.pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)));
const _next: Schema.OptionFromNullOr<Schema.Int> = Schema.OptionFromNullOr(Schema.Int);

const Section: Schema.Literals<Array<keyof typeof preferences>> = Schema.Literals(Struct.keys(preferences));

const TextDefaultKey: Schema.Literals<string[]> = Schema.Literals(Struct.keys(Record.filter(members.TextDefault, identity)));

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
    Schema.Struct({ kind: Schema.Literal('object'), itemId: Schema.Int, isolate: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }),
]).pipe(Schema.toTaggedUnion('kind'));

const Format: Schema.toTaggedUnion<
    'kind',
    readonly [Schema.Struct<{ readonly kind: Schema.Literal<'jpg'> }>, Schema.Struct<{ readonly kind: Schema.Literal<'png'>; readonly transparent: Schema.withDecodingDefaultKey<Schema.Boolean> }>]
> = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('jpg') }),
    Schema.Struct({ kind: Schema.Literal('png'), transparent: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }),
]).pipe(Schema.toTaggedUnion('kind'));

const Rejection: Schema.TaggedUnion<{
    readonly unknownKey: Schema.TaggedStruct<'unknownKey', Record<never, never>>;
    readonly readOnly: Schema.TaggedStruct<'readOnly', Record<never, never>>;
    readonly threw: Schema.TaggedStruct<'threw', { readonly cause: Schema.Defect }>;
    readonly unchanged: Schema.TaggedStruct<'unchanged', Record<never, never>>;
}> = Schema.TaggedUnion({ unknownKey: {}, readOnly: {}, threw: { cause: Schema.Defect() }, unchanged: {} });

// --- [BODIES] --------------------------------------------------------------------------

const ListEnums: Schema.Struct<{ readonly name: Schema.OptionFromOptionalKey<Schema.String> }> = Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String) });

const Capture: Schema.Struct<{ readonly target: typeof Target; readonly format: typeof Format }> = Schema.Struct({ target: Target, format: Format });

const Snapshot: Schema.Struct<(typeof Capture)['fields'] & { readonly directory: typeof AbsolutePath }> = Schema.Struct({ ...Capture.fields, directory: AbsolutePath });

const GetLayout: Schema.Struct<{
    readonly includeItems: Schema.withDecodingDefaultKey<Schema.Boolean>;
    readonly pageCursor: typeof _cursor;
    readonly itemCursor: typeof _cursor;
    readonly limit: Schema.withDecodingDefaultKey<Schema.Int>;
}> = Schema.Struct({
    includeItems: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))),
    pageCursor: _cursor,
    itemCursor: _cursor,
    limit: Schema.Int.pipe(Schema.check(Schema.isBetween(_LIMIT)), Schema.withDecodingDefaultKey(Effect.succeed(_LIMIT.maximum))),
});

const FindKeyStrings: Schema.Struct<{ readonly text: Schema.String }> = Schema.Struct({ text: Schema.String });

const GetPreferences: Schema.Struct<{ readonly sections: Schema.NonEmptyArray<typeof Section> }> = Schema.Struct({ sections: Schema.NonEmptyArray(Section) });

const SetPreferences: Schema.Struct<{
    readonly values: Schema.NonEmptyArray<Schema.Struct<{ readonly section: typeof Section; readonly key: Schema.String; readonly value: Schema.Codec<Schema.Json> }>>;
}> = Schema.Struct({ values: Schema.NonEmptyArray(Schema.Struct({ section: Section, key: Schema.String, value: Schema.Json })) });

const SetTextDefaults: Schema.Struct<{
    readonly scope: Schema.Literals<readonly ['application', 'document']>;
    readonly values: Schema.NonEmptyArray<Schema.Struct<{ readonly key: typeof TextDefaultKey; readonly value: Schema.Codec<Schema.Json> }>>;
}> = Schema.Struct({ scope: Schema.Literals(['application', 'document']), values: Schema.NonEmptyArray(Schema.Struct({ key: TextDefaultKey, value: Schema.Json })) });

// --- [RESULTS] -------------------------------------------------------------------------

const Enums: Schema.Struct<{
    readonly kind: Schema.Literal<'enums'>;
    readonly enums: Schema.$Array<
        Schema.Struct<{ readonly name: Schema.String; readonly constants: Schema.$Array<Schema.Struct<{ readonly name: Schema.String; readonly value: Schema.OptionFromNullOr<Schema.Int> }>> }>
    >;
}> = Schema.Struct({
    kind: Schema.Literal('enums'),
    enums: Schema.Array(Schema.Struct({ name: Schema.String, constants: Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.OptionFromNullOr(Schema.Int) })) })),
});

const Image: Schema.Struct<{
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
}> = Schema.Struct({
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
});

const Layout: Schema.Struct<{
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
            readonly bounds: typeof _Box;
            readonly margins: Schema.Struct<{ readonly top: Schema.Number; readonly bottom: Schema.Number; readonly inside: Schema.Number; readonly outside: Schema.Number }>;
            readonly contentArea: typeof _Box;
            readonly guides: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly orientation: Schema.String; readonly location: Schema.Number }>>;
            readonly items: Schema.OptionFromOptionalKey<Schema.$Array<typeof _Item>>;
        }>
    >;
    readonly pageCount: Schema.Int;
    readonly pageCursor: typeof _next;
    readonly itemCount: Schema.Int;
    readonly itemCursor: typeof _next;
}> = Schema.Struct({
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
            bounds: _Box,
            margins: Schema.Struct({ top: Schema.Number, bottom: Schema.Number, inside: Schema.Number, outside: Schema.Number }),
            contentArea: _Box,
            guides: Schema.Array(Schema.Struct({ id: Schema.Int, orientation: Schema.String, location: Schema.Number })),
            items: Schema.OptionFromOptionalKey(Schema.Array(_Item)),
        }),
    ),
    pageCount: Schema.Int,
    pageCursor: _next,
    itemCount: Schema.Int,
    itemCursor: _next,
});

const Keys: Schema.Struct<{ readonly kind: Schema.Literal<'keys'>; readonly keys: Schema.$Array<Schema.String>; readonly translated: Schema.String }> = Schema.Struct({
    kind: Schema.Literal('keys'),
    keys: Schema.Array(Schema.String),
    translated: Schema.String,
});

const Preferences: Schema.Struct<{
    readonly kind: Schema.Literal<'preferences'>;
    readonly values: Schema.$Record<Schema.String, Schema.$Record<Schema.String, Schema.Codec<Schema.Json>>>;
    readonly unreadable: Schema.$Array<Schema.Struct<typeof _named & { readonly cause: Schema.Defect }>>;
}> = Schema.Struct({
    kind: Schema.Literal('preferences'),
    values: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Json)),
    unreadable: Schema.Array(Schema.Struct({ ..._named, cause: Schema.Defect() })),
});

const Applied: Schema.Struct<{
    readonly kind: Schema.Literal<'applied'>;
    readonly applied: Schema.$Array<Schema.Struct<typeof _named & { readonly from: Schema.Codec<Schema.Json>; readonly to: Schema.Codec<Schema.Json> }>>;
    readonly rejected: Schema.$Array<Schema.Struct<typeof _named & { readonly reason: typeof Rejection }>>;
}> = Schema.Struct({
    kind: Schema.Literal('applied'),
    applied: Schema.Array(Schema.Struct({ ..._named, from: Schema.Json, to: Schema.Json })),
    rejected: Schema.Array(Schema.Struct({ ..._named, reason: Rejection })),
});

const Rejected: Schema.Struct<{
    readonly kind: Schema.Literal<'rejected'>;
    readonly reason: Schema.TaggedUnion<{ readonly documentOpen: Schema.TaggedStruct<'documentOpen', { readonly count: Schema.Int }> }>;
}> = Schema.Struct({ kind: Schema.Literal('rejected'), reason: Schema.TaggedUnion({ documentOpen: { count: Schema.Int } }) });

const Settings: Schema.Union<readonly [typeof Applied, typeof Rejected]> = Schema.Union([Applied, Rejected]);

// --- [KINDS] ---------------------------------------------------------------------------

const Bodies: Schema.Struct<{
    readonly execute: typeof Execute;
    readonly listEnums: typeof ListEnums;
    readonly snapshot: typeof Snapshot;
    readonly getLayout: typeof GetLayout;
    readonly findKeyStrings: typeof FindKeyStrings;
    readonly getPreferences: typeof GetPreferences;
    readonly setPreferences: typeof SetPreferences;
    readonly setTextDefaults: typeof SetTextDefaults;
}> = Schema.Struct({
    execute: Execute,
    listEnums: ListEnums,
    snapshot: Snapshot,
    getLayout: GetLayout,
    findKeyStrings: FindKeyStrings,
    getPreferences: GetPreferences,
    setPreferences: SetPreferences,
    setTextDefaults: SetTextDefaults,
});

const Kind: Schema.Literals<Array<keyof (typeof Bodies)['fields']>> = Schema.Literals(Struct.keys(Bodies.fields));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Format, Item, Rejection, Target };
export {
    Applied,
    Bodies,
    Capture,
    Enums,
    FindKeyStrings,
    GetLayout,
    GetPreferences,
    Image,
    Keys,
    Kind,
    Layout,
    ListEnums,
    Preferences,
    Rejected,
    Section,
    SetPreferences,
    SetTextDefaults,
    Settings,
    Snapshot,
    TextDefaultKey,
};
