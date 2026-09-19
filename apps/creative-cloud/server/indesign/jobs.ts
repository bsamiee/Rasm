// --- [IMPORTS] -------------------------------------------------------------------------

import { GeometryError } from '@rasm/typography/grid';
import { CSS, Measured, MetricsError } from '@rasm/typography/metrics';
import { Array, Effect, Record, Schema, Struct } from 'effect';
import { type AppliedReply, applied, HostRejection, type PreferencesReply, preferences as readable } from '../errors.ts';
import { Execute } from '../frames.ts';
import { Bounds, Dpi, PixelBudget, Region } from '../images.ts';
import { AbsolutePath, OptionalInt, OptionalString, PageIndex } from '../values.ts';
import { Input, Output } from './editorial.ts';
import { GridInput, GridOutput } from './grid.ts';
import { members, preferences } from './indesign.ts';
import { LibraryBodies, LibraryResults } from './library.ts';
import { PublishingInput, PublishingOutput } from './publishing.ts';

// --- [TABLE] ---------------------------------------------------------------------------

const PAIRINGS = {
    editorial: { body: 'Swedish Gothic', serif: 'Ivar Text', display: 'Ivar Headline', mono: 'Geist Mono', persian: 'Scheherazade New', arabic: 'Scheherazade New' },
    swiss: { body: 'Neue Haas Unica W1G', serif: 'Ivar Text', display: 'Neue Haas Unica W1G', mono: 'Geist Mono', persian: 'Noto Sans Arabic', arabic: 'Noto Sans Arabic' },
} as const;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LIMIT = { minimum: 1, maximum: 200 } as const;
const TYPOGRAPHY_ANCHORS = { cap: 0x48, ascender: 0x66, lowercase: 0x78, alif: 0x6_27 } as const;
const TYPOGRAPHY_ROLES = {
    body: {
        family: 'body',
        italic: false,
        bolder: false,
        alignment: { glyph: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase, reference: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase },
    },
    serif: {
        family: 'serif',
        italic: false,
        bolder: false,
        alignment: { glyph: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase, reference: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase },
    },
    mono: {
        family: 'mono',
        italic: false,
        bolder: false,
        axes: { wght: CSS.weight.normal as typeof CSS.weight.normal },
        alignment: { glyph: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase, reference: TYPOGRAPHY_ANCHORS.lowercase as typeof TYPOGRAPHY_ANCHORS.lowercase },
    },
    display: {
        family: 'display',
        italic: false,
        bolder: false,
        alignment: { glyph: TYPOGRAPHY_ANCHORS.cap as typeof TYPOGRAPHY_ANCHORS.cap, reference: TYPOGRAPHY_ANCHORS.cap as typeof TYPOGRAPHY_ANCHORS.cap },
    },
    persian: {
        family: 'persian',
        italic: false,
        bolder: false,
        alignment: { glyph: TYPOGRAPHY_ANCHORS.alif as typeof TYPOGRAPHY_ANCHORS.alif, reference: TYPOGRAPHY_ANCHORS.cap as typeof TYPOGRAPHY_ANCHORS.cap },
    },
    arabic: {
        family: 'arabic',
        italic: false,
        bolder: false,
        alignment: { glyph: TYPOGRAPHY_ANCHORS.alif as typeof TYPOGRAPHY_ANCHORS.alif, reference: TYPOGRAPHY_ANCHORS.cap as typeof TYPOGRAPHY_ANCHORS.cap },
    },
    emphasis: { family: 'body', italic: true, bolder: false },
    strong: { family: 'body', italic: false, bolder: true },
    strongEmphasis: { family: 'body', italic: true, bolder: true },
    quote: { family: 'serif', italic: true, bolder: false },
    persianHeading: { family: 'persian', italic: false, bolder: true },
    arabicHeading: { family: 'arabic', italic: false, bolder: true },
} as const;

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
const FontSource: Schema.Struct<
    Readonly<Record<'postscriptName' | 'name' | 'fontFamily' | 'fontStyleName' | 'fullName' | 'fullNameNative' | 'fontStyleNameNative' | 'location' | 'version', Schema.String>>
> = Schema.Struct({
    postscriptName: Schema.String,
    name: Schema.String,
    fontFamily: Schema.String,
    fontStyleName: Schema.String,
    fullName: Schema.String,
    fullNameNative: Schema.String,
    fontStyleNameNative: Schema.String,
    location: Schema.String,
    version: Schema.String,
});
const FontAxes: Schema.Struct<{
    readonly numDesignAxes: Schema.Int;
    readonly designAxesValues: Schema.$Array<Schema.Finite>;
    readonly designAxesRange: Schema.$Array<Schema.Tuple<readonly [Schema.Finite, Schema.Finite]>>;
}> = Schema.Struct({
    numDesignAxes: Schema.Int.pipe(Schema.check(Schema.isGreaterThan(0))),
    designAxesValues: Schema.Array(Schema.Finite),
    designAxesRange: Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])),
}).check(
    Schema.makeFilter(({ numDesignAxes, designAxesValues, designAxesRange }) => numDesignAxes === designAxesValues.length && numDesignAxes === designAxesRange.length, {
        message: 'The native axis count must match its coordinates and ranges.',
    }),
);
const BoundFont: Schema.Struct<typeof Measured.fields & { readonly native: typeof FontSource }> = Schema.Struct({ ...Measured.fields, native: FontSource });
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

const _channel: Schema.Number = Schema.Number.pipe(Schema.check(Schema.isBetween({ minimum: 0, maximum: 255 })));
const _rgb: Schema.Tuple<readonly [Schema.Number, Schema.Number, Schema.Number]> = Schema.Tuple([_channel, _channel, _channel]);
const _swatch: Schema.Struct<{ readonly rgb: typeof _rgb; readonly group: Schema.OptionFromOptionalKey<Schema.NonEmptyString> }> = Schema.Struct({
    rgb: _rgb,
    group: Schema.OptionFromOptionalKey(Schema.NonEmptyString),
});
const _palette: Schema.StructWithRest<
    Schema.$Record<Schema.Literals<readonly ['Accent', 'Ink', 'Field']>, typeof _swatch>,
    readonly [Schema.$Record<Schema.NonEmptyString, typeof _swatch>]
> = Schema.StructWithRest(Schema.Record(Schema.Literals(['Accent', 'Ink', 'Field']), _swatch), [Schema.Record(Schema.NonEmptyString, _swatch)]);

const _languageRoles: Schema.Literals<readonly ['latin', 'persian', 'arabic']> = Schema.Literals(['latin', 'persian', 'arabic']);
const _languageId: Schema.Int = Schema.Int.pipe(Schema.check(Schema.isGreaterThan(0)));
const _ids: Schema.$Array<Schema.Int> = Schema.Array(Schema.Int);
const _dimensions: Schema.Struct<{ readonly width: Schema.Number; readonly height: Schema.Number }> = Schema.Struct({ width: Schema.Number, height: Schema.Number });
const _layouts: Schema.Struct<{
    readonly created: Schema.$Array<Schema.String>;
    readonly excluded: Schema.$Array<Schema.Struct<{ readonly name: Schema.String; readonly errors: Schema.NonEmptyArray<typeof GeometryError> }>>;
}> = Schema.Struct({ created: Schema.Array(Schema.String), excluded: Schema.Array(Schema.Struct({ name: Schema.String, errors: Schema.NonEmptyArray(GeometryError) })) });
const TypographyError: Schema.TaggedUnion<{
    readonly nativeFontSelection: Schema.TaggedStruct<
        'nativeFontSelection',
        {
            readonly role: Schema.Literals<Array<keyof typeof TYPOGRAPHY_ROLES>>;
            readonly postScriptName: Schema.String;
            readonly candidates: Schema.$Array<typeof FontSource>;
        }
    >;
    readonly languageSelection: Schema.TaggedStruct<
        'languageSelection',
        {
            readonly role: Schema.Union<readonly [typeof _languageRoles, Schema.Literal<'code'>]>;
            readonly selection: Schema.Union<readonly [typeof _languageId, Schema.Literal<'[No Language]'>]>;
            readonly candidates: typeof _ids;
        }
    >;
    readonly paletteConflicts: Schema.TaggedStruct<'paletteConflicts', { readonly names: Schema.NonEmptyArray<Schema.String> }>;
    readonly textVariableSelection: Schema.TaggedStruct<'textVariableSelection', { readonly type: Schema.String; readonly candidates: typeof _ids }>;
    readonly nativePageGeometry: Schema.TaggedStruct<'nativePageGeometry', { readonly expected: typeof _dimensions; readonly actual: typeof _dimensions }>;
}> = Schema.TaggedUnion({
    nativeFontSelection: {
        role: Schema.Literals(Struct.keys(TYPOGRAPHY_ROLES)),
        postScriptName: Schema.String,
        candidates: Schema.Array(FontSource),
    },
    languageSelection: { role: Schema.Union([_languageRoles, Schema.Literal('code')]), selection: Schema.Union([_languageId, Schema.Literal('[No Language]')]), candidates: _ids },
    paletteConflicts: { names: Schema.NonEmptyArray(Schema.String) },
    textVariableSelection: { type: Schema.String, candidates: _ids },
    nativePageGeometry: { expected: _dimensions, actual: _dimensions },
});

const _buildErrors: Schema.NonEmptyArray<Schema.Union<readonly [typeof TypographyError, typeof MetricsError, typeof GeometryError, typeof HostRejection]>> = Schema.NonEmptyArray(
    Schema.Union([TypographyError, MetricsError, GeometryError, HostRejection]),
);

const TypographyBuild: Schema.Struct<{
    readonly pairing: Schema.Literals<Array<keyof typeof PAIRINGS>>;
    readonly scope: Schema.Literals<readonly ['default', 'catalogue']>;
    readonly directory: typeof AbsolutePath;
    readonly palette: typeof _palette;
    readonly fonts: Schema.Struct<Readonly<Record<keyof typeof TYPOGRAPHY_ROLES, typeof BoundFont>>>;
    readonly languages: Schema.$Record<typeof _languageRoles, typeof _languageId>;
}> = Schema.Struct({
    pairing: Schema.Literals(Struct.keys(PAIRINGS)),
    scope: Schema.Literals(['default', 'catalogue']),
    directory: AbsolutePath,
    palette: _palette,
    fonts: Schema.Struct(Record.map(TYPOGRAPHY_ROLES, () => BoundFont)),
    languages: Schema.Record(_languageRoles, _languageId),
});

// --- [KINDS] ---------------------------------------------------------------------------

const Bodies: Schema.Struct<
    typeof LibraryBodies.fields & {
        readonly applyGrid: typeof GridInput;
        readonly execute: typeof Execute;
        readonly editDocument: typeof Input;
        readonly fontSources: Schema.Struct<Record<never, never>>;
        readonly publishing: typeof PublishingInput;
        readonly buildTypography: typeof TypographyBuild;
        readonly listEnums: Schema.Struct<{ readonly name: Schema.OptionFromOptionalKey<Schema.String> }>;
        readonly snapshot: Schema.Struct<{ readonly target: typeof Target; readonly format: typeof Format; readonly directory: typeof AbsolutePath; readonly documentId: typeof OptionalInt }>;
        readonly getLayout: Schema.Struct<{
            readonly documentId: typeof OptionalInt;
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
    }
> = Schema.Struct({
    ...LibraryBodies.fields,
    applyGrid: GridInput,
    execute: Execute,
    editDocument: Input,
    fontSources: Schema.Struct({}),
    publishing: PublishingInput,
    buildTypography: TypographyBuild,
    listEnums: Schema.Struct({ name: OptionalString }),
    snapshot: Schema.Struct({ target: Target, format: Format, directory: AbsolutePath, documentId: OptionalInt }),
    getLayout: Schema.Struct({
        documentId: OptionalInt,
        includeItems: _off,
        pageCursor: _cursor,
        itemCursor: _cursor,
        limit: Schema.Int.pipe(Schema.check(Schema.isBetween(_LIMIT)), Schema.withDecodingDefaultKey(Effect.succeed(_LIMIT.maximum))),
    }),
    findKeyStrings: Schema.Struct({ text: Schema.String }),
    getPreferences: Schema.Struct({ sections: Schema.NonEmptyArray(_section) }),
    setPreferences: Schema.Struct({
        values: Schema.NonEmptyArray(Schema.Struct({ section: _section, key: Schema.String, value: Schema.Json })).pipe(
            Schema.check(
                Schema.makeFilter((values) => Array.dedupe(Array.map(values, Struct.pick(['section', 'key']))).length === values.length || 'A preference can be assigned once in each request'),
            ),
        ),
    }),
    setTextDefaults: Schema.Struct({
        scope: Schema.Literals(['application', 'document']),
        values: Schema.NonEmptyArray(
            Schema.Struct({
                key: Schema.Literals(Struct.keys(Record.filter(members.TextDefault, (writable, key) => writable && !Record.has<string, boolean>(members.Preference, key)))),
                value: Schema.Json,
            }),
        ).pipe(Schema.check(Schema.makeFilter((values) => Array.dedupe(Array.map(values, Struct.get('key'))).length === values.length || 'A text default can be assigned once in each request'))),
    }),
});

const Results: Schema.Struct<
    typeof LibraryResults.fields & {
        readonly applyGrid: typeof GridOutput;
        readonly execute: Schema.Codec<Schema.Json>;
        readonly editDocument: typeof Output;
        readonly fontSources: Schema.Struct<{
            readonly kind: Schema.Literal<'fonts'>;
            readonly fonts: Schema.$Array<Schema.Struct<typeof FontSource.fields & { readonly axes: Schema.OptionFromNullOr<typeof FontAxes> }>>;
            readonly rejected: Schema.$Array<Schema.Struct<{ readonly index: typeof PageIndex; readonly reason: typeof HostRejection }>>;
        }>;
        readonly publishing: typeof PublishingOutput;
        readonly buildTypography: Schema.toTaggedUnion<
            'kind',
            readonly [
                Schema.Struct<{
                    readonly kind: Schema.Literal<'completed'>;
                    readonly files: Schema.$Array<
                        Schema.Struct<{
                            readonly name: Schema.String;
                            readonly outputs: Schema.NonEmptyArray<Schema.Struct<{ readonly kind: Schema.Literals<readonly ['document', 'template']>; readonly path: typeof AbsolutePath }>>;
                            readonly width: Schema.Number;
                            readonly height: Schema.Number;
                            readonly leading: Schema.Number;
                            readonly styles: Schema.$Record<Schema.String, Schema.$Array<Schema.String>>;
                            readonly layouts: typeof _layouts;
                        }>
                    >;
                    readonly book: Schema.OptionFromNullOr<typeof AbsolutePath>;
                    readonly failures: Schema.$Array<Schema.Struct<{ readonly name: Schema.String; readonly errors: typeof _buildErrors }>>;
                }>,
                Schema.Struct<{
                    readonly kind: Schema.Literal<'rejected'>;
                    readonly errors: typeof _buildErrors;
                }>,
            ]
        >;
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
            readonly document: Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String }>;
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
                    readonly guides: Schema.$Array<
                        Schema.Struct<{
                            readonly id: Schema.Int;
                            readonly line: Schema.$Record<Schema.Literals<readonly ['start', 'end']>, Schema.Struct<Readonly<Record<'x' | 'y', Schema.Finite>>>>;
                        }>
                    >;
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
    }
> = Schema.Struct({
    ...LibraryResults.fields,
    applyGrid: GridOutput,
    execute: Schema.Json,
    editDocument: Output,
    fontSources: Schema.Struct({
        kind: Schema.Literal('fonts'),
        fonts: Schema.Array(Schema.Struct({ ...FontSource.fields, axes: Schema.OptionFromNullOr(FontAxes) })),
        rejected: Schema.Array(Schema.Struct({ index: PageIndex, reason: HostRejection })),
    }),
    publishing: PublishingOutput,
    buildTypography: Schema.Union([
        Schema.Struct({
            kind: Schema.Literal('completed'),
            files: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    outputs: Schema.NonEmptyArray(Schema.Struct({ kind: Schema.Literals(['document', 'template']), path: AbsolutePath })),
                    width: Schema.Number,
                    height: Schema.Number,
                    leading: Schema.Number,
                    styles: Schema.Record(Schema.String, Schema.Array(Schema.String)),
                    layouts: _layouts,
                }),
            ),
            book: Schema.OptionFromNullOr(AbsolutePath),
            failures: Schema.Array(Schema.Struct({ name: Schema.String, errors: _buildErrors })),
        }),
        Schema.Struct({ kind: Schema.Literal('rejected'), errors: _buildErrors }),
    ]).pipe(Schema.toTaggedUnion('kind')),
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
        document: Schema.Struct({ id: Schema.Int, name: Schema.String }),
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
                guides: Schema.Array(Schema.Struct({ id: Schema.Int, line: Schema.Record(Schema.Literals(['start', 'end']), Schema.Struct({ x: Schema.Finite, y: Schema.Finite })) })),
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
export { Applied, Bodies, FontAxes, FontSource, PAIRINGS, Results, TYPOGRAPHY_ANCHORS, TYPOGRAPHY_ROLES, TypographyError };
