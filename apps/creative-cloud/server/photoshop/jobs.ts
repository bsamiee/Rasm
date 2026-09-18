// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, Schema, Struct } from 'effect';
import { type AppliedReply, applied, type PreferencesReply, preferences } from '../errors.ts';
import { Execute } from '../frames.ts';
import { Bounds, PixelBudget, Region } from '../images.ts';
import { OptionalInt } from '../values.ts';
import { Section, TARGET_ROWS, Writes } from './preferences.ts';

// --- [TABLE] ---------------------------------------------------------------------------

const PRESET_CLASSES = {
    brush: 'brush',
    swatch: 'color',
    gradient: 'gradientClassEvent',
    style: 'styleClass',
    pattern: '$PttR',
    contour: 'shapeCurveType',
    shape: 'customShape',
    tool: 'toolPreset',
} as const;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LIMIT = { minimum: 1, maximum: 500 } as const;
const _DEPTH = { minimum: 0, maximum: 8 } as const;

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = keyof (typeof Bodies)['fields'];
type Body<K extends Kind> = (typeof Bodies)['fields'][K]['Type'];
type Reply<K extends Kind> = (typeof Results)['fields'][K]['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const _off = Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false)));
const _next = Schema.OptionFromNullOr(Schema.Int);
const _keyed: { readonly section: typeof Section; readonly key: Schema.String } = { section: Section, key: Schema.String };

const Target: Schema.toTaggedUnion<
    'kind',
    readonly [
        Schema.Struct<{ readonly kind: Schema.Literal<'layer'>; readonly layerId: Schema.Int }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'document'> }>,
        Schema.Struct<{ readonly kind: Schema.Literal<'selection'> }>,
    ]
> = Schema.Union([
    Schema.Struct({ kind: Schema.Literal('layer'), layerId: Schema.Int }),
    Schema.Struct({ kind: Schema.Literal('document') }),
    Schema.Struct({ kind: Schema.Literal('selection') }),
]).pipe(Schema.toTaggedUnion('kind'));

// --- [KINDS] ---------------------------------------------------------------------------

const Bodies: Schema.Struct<{
    readonly execute: typeof Execute;
    readonly batchPlay: Schema.Struct<{
        readonly descriptors: Schema.NonEmptyArray<Schema.StructWithRest<Schema.Struct<{ readonly _obj: Schema.String }>, readonly [Schema.$Record<Schema.String, Schema.Codec<Schema.Json>>]>>;
        readonly continueOnError: Schema.withDecodingDefaultKey<Schema.Boolean>;
        readonly immediateRedraw: Schema.withDecodingDefaultKey<Schema.Boolean>;
    }>;
    readonly snapshot: Schema.Struct<{
        readonly target: typeof Target;
        readonly documentId: Schema.OptionFromOptionalKey<Schema.Int>;
        readonly region: Schema.OptionFromOptionalKey<typeof Region>;
        readonly budget: typeof PixelBudget;
    }>;
    readonly getDocument: Schema.Struct<{
        readonly documentId: Schema.OptionFromOptionalKey<Schema.Int>;
        readonly limit: Schema.withDecodingDefaultKey<Schema.Int>;
        readonly cursor: Schema.withDecodingDefaultKey<Schema.Int>;
        readonly depth: Schema.withDecodingDefaultKey<Schema.Int>;
    }>;
    readonly getPreferences: Schema.Struct<{ readonly sections: Schema.NonEmptyArray<typeof Section> }>;
    readonly setPreferences: Schema.Struct<{ readonly values: Schema.withDecodingDefaultKey<typeof Writes> }>;
    readonly listPresets: Schema.Struct<{ readonly kind: Schema.Literals<Array<keyof typeof PRESET_CLASSES>> }>;
    readonly runAction: Schema.Struct<{ readonly set: Schema.String; readonly action: Schema.String }>;
}> = Schema.Struct({
    execute: Execute,
    batchPlay: Schema.Struct({
        descriptors: Schema.NonEmptyArray(Schema.StructWithRest(Schema.Struct({ _obj: Schema.String }), [Schema.Record(Schema.String, Schema.Json)])),
        continueOnError: _off,
        immediateRedraw: _off,
    }),
    snapshot: Schema.Struct({ target: Target, documentId: OptionalInt, region: Schema.OptionFromOptionalKey(Region), budget: PixelBudget }),
    getDocument: Schema.Struct({
        documentId: OptionalInt,
        limit: Schema.Int.pipe(Schema.check(Schema.isBetween(_LIMIT)), Schema.withDecodingDefaultKey(Effect.succeed(_LIMIT.maximum))),
        cursor: Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)), Schema.withDecodingDefaultKey(Effect.succeed(0))),
        depth: Schema.Int.pipe(Schema.check(Schema.isBetween(_DEPTH)), Schema.withDecodingDefaultKey(Effect.succeed(_DEPTH.maximum))),
    }),
    getPreferences: Schema.Struct({ sections: Schema.NonEmptyArray(Section) }),
    setPreferences: Schema.Struct({ values: Writes.pipe(Schema.withDecodingDefaultKey(Effect.succeed(TARGET_ROWS))) }),
    listPresets: Schema.Struct({ kind: Schema.Literals(Struct.keys(PRESET_CLASSES)) }),
    runAction: Schema.Struct({ set: Schema.String, action: Schema.String }),
});

const Results: Schema.Struct<{
    readonly execute: Schema.Codec<Schema.Json>;
    readonly batchPlay: Schema.Struct<{
        readonly kind: Schema.Literal<'descriptors'>;
        readonly results: Schema.$Array<Schema.Codec<Schema.Json>>;
        readonly failed: Schema.$Array<Schema.Struct<{ readonly index: Schema.Int; readonly result: Schema.Number; readonly message: Schema.String }>>;
    }>;
    readonly snapshot: Schema.Struct<{
        readonly kind: Schema.Literal<'jpeg'>;
        readonly base64: Schema.String;
        readonly widthPx: Schema.Int;
        readonly heightPx: Schema.Int;
        readonly level: Schema.Int;
        readonly scale: Schema.Number;
        readonly sourceBounds: typeof Bounds;
        readonly colorProfile: Schema.String;
    }>;
    readonly getDocument: Schema.Struct<{
        readonly kind: Schema.Literal<'document'>;
        readonly documents: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String; readonly path: Schema.String; readonly saved: Schema.Boolean }>>;
        readonly active: Schema.OptionFromNullOr<
            Schema.Struct<{
                readonly id: Schema.Int;
                readonly mode: Schema.String;
                readonly bitsPerChannel: Schema.String;
                readonly colorProfileName: Schema.String;
                readonly width: Schema.Number;
                readonly height: Schema.Number;
                readonly resolution: Schema.Number;
                readonly layers: Schema.$Array<
                    Schema.Struct<{
                        readonly id: Schema.Int;
                        readonly name: Schema.String;
                        readonly kind: Schema.String;
                        readonly visible: Schema.Boolean;
                        readonly depth: Schema.Int;
                        readonly parentId: Schema.OptionFromNullOr<Schema.Int>;
                        readonly children: Schema.Int;
                    }>
                >;
                readonly layerCount: Schema.Int;
                readonly layerCursor: Schema.OptionFromNullOr<Schema.Int>;
            }>
        >;
    }>;
    readonly getPreferences: PreferencesReply<typeof _keyed>;
    readonly setPreferences: AppliedReply<typeof _keyed>;
    readonly listPresets: Schema.Struct<{ readonly kind: Schema.Literal<'presets'>; readonly groupIndex: Schema.Int; readonly names: Schema.$Array<Schema.String>; readonly count: Schema.Int }>;
    readonly runAction: Schema.Null;
}> = Schema.Struct({
    execute: Schema.Json,
    batchPlay: Schema.Struct({
        kind: Schema.Literal('descriptors'),
        results: Schema.Array(Schema.Json),
        failed: Schema.Array(Schema.Struct({ index: Schema.Int, result: Schema.Number, message: Schema.String })),
    }),
    snapshot: Schema.Struct({
        kind: Schema.Literal('jpeg'),
        base64: Schema.String,
        widthPx: Schema.Int,
        heightPx: Schema.Int,
        level: Schema.Int,
        scale: Schema.Number,
        sourceBounds: Bounds,
        colorProfile: Schema.String,
    }),
    getDocument: Schema.Struct({
        kind: Schema.Literal('document'),
        documents: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, path: Schema.String, saved: Schema.Boolean })),
        active: Schema.OptionFromNullOr(
            Schema.Struct({
                id: Schema.Int,
                mode: Schema.String,
                bitsPerChannel: Schema.String,
                colorProfileName: Schema.String,
                width: Schema.Number,
                height: Schema.Number,
                resolution: Schema.Number,
                layers: Schema.Array(
                    Schema.Struct({
                        id: Schema.Int,
                        name: Schema.String,
                        kind: Schema.String,
                        visible: Schema.Boolean,
                        depth: Schema.Int,
                        parentId: _next,
                        children: Schema.Int,
                    }),
                ),
                layerCount: Schema.Int,
                layerCursor: _next,
            }),
        ),
    }),
    getPreferences: preferences(_keyed),
    setPreferences: applied(_keyed),
    listPresets: Schema.Struct({ kind: Schema.Literal('presets'), groupIndex: Schema.Int, names: Schema.Array(Schema.String), count: Schema.Int }),
    runAction: Schema.Null,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Body, Kind, Reply };
export { Bodies, PRESET_CLASSES, Results };
