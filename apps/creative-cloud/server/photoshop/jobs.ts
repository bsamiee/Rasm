// --- [IMPORTS] -------------------------------------------------------------------------

import { Effect, Schema, Struct } from 'effect';
import { Execute } from '../frames.ts';
import { Bounds, PixelBudget, Region } from '../images.ts';
import { Section, Writes } from './preferences.ts';

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

type Rejection = (typeof Rejection)['Type'];
type Target = (typeof Target)['Type'];
type PresetKind = (typeof PresetKind)['Type'];
type LayerRow = (typeof _LayerRow)['Type'];
type Kind = (typeof Kind)['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const _keyed: { readonly section: typeof Section; readonly key: Schema.String } = { section: Section, key: Schema.String };
const _optionalInt = Schema.OptionFromOptionalKey(Schema.Int);
const _off = Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false)));

const Descriptor: Schema.StructWithRest<Schema.Struct<{ readonly _obj: Schema.String }>, readonly [Schema.$Record<Schema.String, Schema.Codec<Schema.Json>>]> = Schema.StructWithRest(
    Schema.Struct({ _obj: Schema.String }),
    [Schema.Record(Schema.String, Schema.Json)],
);

const Target: Schema.Literals<readonly ['document', 'selection', 'layer']> = Schema.Literals(['document', 'selection', 'layer']);

const PresetKind: Schema.Literals<Array<keyof typeof PRESET_CLASSES>> = Schema.Literals(Struct.keys(PRESET_CLASSES));

const Rejection: Schema.TaggedUnion<{
    readonly unknownKey: Schema.TaggedStruct<'unknownKey', Record<never, never>>;
    readonly threw: Schema.TaggedStruct<'threw', { readonly cause: Schema.Defect }>;
    readonly unchanged: Schema.TaggedStruct<'unchanged', Record<never, never>>;
}> = Schema.TaggedUnion({ unknownKey: {}, threw: { cause: Schema.Defect() }, unchanged: {} });

const _LayerRow: Schema.Struct<{
    readonly id: Schema.Int;
    readonly name: Schema.String;
    readonly kind: Schema.String;
    readonly visible: Schema.Boolean;
    readonly depth: Schema.Int;
    readonly parentId: Schema.OptionFromNullOr<Schema.Int>;
    readonly children: Schema.Int;
}> = Schema.Struct({ id: Schema.Int, name: Schema.String, kind: Schema.String, visible: Schema.Boolean, depth: Schema.Int, parentId: Schema.OptionFromNullOr(Schema.Int), children: Schema.Int });

// --- [BODIES] --------------------------------------------------------------------------

const BatchPlay: Schema.Struct<{
    readonly descriptors: Schema.NonEmptyArray<typeof Descriptor>;
    readonly continueOnError: Schema.withDecodingDefaultKey<Schema.Boolean>;
    readonly immediateRedraw: Schema.withDecodingDefaultKey<Schema.Boolean>;
}> = Schema.Struct({
    descriptors: Schema.NonEmptyArray(Descriptor),
    continueOnError: _off,
    immediateRedraw: _off,
});

const Snapshot: Schema.Struct<{
    readonly target: typeof Target;
    readonly documentId: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly layerId: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly region: Schema.OptionFromOptionalKey<typeof Region>;
    readonly budget: typeof PixelBudget;
}> = Schema.Struct({ target: Target, documentId: _optionalInt, layerId: _optionalInt, region: Schema.OptionFromOptionalKey(Region), budget: PixelBudget });

const GetDocument: Schema.Struct<{
    readonly documentId: Schema.OptionFromOptionalKey<Schema.Int>;
    readonly limit: Schema.withDecodingDefaultKey<Schema.Int>;
    readonly cursor: Schema.withDecodingDefaultKey<Schema.Int>;
    readonly depth: Schema.withDecodingDefaultKey<Schema.Int>;
}> = Schema.Struct({
    documentId: _optionalInt,
    limit: Schema.Int.pipe(Schema.check(Schema.isBetween(_LIMIT)), Schema.withDecodingDefaultKey(Effect.succeed(_LIMIT.maximum))),
    cursor: Schema.Int.pipe(Schema.check(Schema.isGreaterThanOrEqualTo(0)), Schema.withDecodingDefaultKey(Effect.succeed(0))),
    depth: Schema.Int.pipe(Schema.check(Schema.isBetween(_DEPTH)), Schema.withDecodingDefaultKey(Effect.succeed(_DEPTH.maximum))),
});

const GetPreferences: Schema.Struct<{ readonly sections: Schema.NonEmptyArray<typeof Section> }> = Schema.Struct({ sections: Schema.NonEmptyArray(Section) });

const SetPreferences: Schema.Struct<{ readonly values: typeof Writes }> = Schema.Struct({ values: Writes });

const ListPresets: Schema.Struct<{ readonly kind: typeof PresetKind }> = Schema.Struct({ kind: PresetKind });

const RunAction: Schema.Struct<{ readonly set: Schema.String; readonly action: Schema.String }> = Schema.Struct({ set: Schema.String, action: Schema.String });

// --- [RESULTS] -------------------------------------------------------------------------

const Descriptors: Schema.Struct<{
    readonly kind: Schema.Literal<'descriptors'>;
    readonly results: Schema.$Array<Schema.Codec<Schema.Json>>;
    readonly failed: Schema.$Array<Schema.Struct<{ readonly index: Schema.Int; readonly result: Schema.Number; readonly message: Schema.String }>>;
}> = Schema.Struct({
    kind: Schema.Literal('descriptors'),
    results: Schema.Array(Schema.Json),
    failed: Schema.Array(Schema.Struct({ index: Schema.Int, result: Schema.Number, message: Schema.String })),
});

const Jpeg: Schema.Struct<{
    readonly kind: Schema.Literal<'jpeg'>;
    readonly base64: Schema.String;
    readonly widthPx: Schema.Int;
    readonly heightPx: Schema.Int;
    readonly level: Schema.Int;
    readonly scale: Schema.Number;
    readonly sourceBounds: typeof Bounds;
    readonly colorProfile: Schema.String;
}> = Schema.Struct({
    kind: Schema.Literal('jpeg'),
    base64: Schema.String,
    widthPx: Schema.Int,
    heightPx: Schema.Int,
    level: Schema.Int,
    scale: Schema.Number,
    sourceBounds: Bounds,
    colorProfile: Schema.String,
});

const Active: Schema.Struct<{
    readonly id: Schema.Int;
    readonly mode: Schema.String;
    readonly bitsPerChannel: Schema.String;
    readonly colorProfileName: Schema.String;
    readonly width: Schema.Number;
    readonly height: Schema.Number;
    readonly resolution: Schema.Number;
    readonly layers: Schema.$Array<typeof _LayerRow>;
    readonly layerCount: Schema.Int;
    readonly layerCursor: Schema.OptionFromNullOr<Schema.Int>;
}> = Schema.Struct({
    id: Schema.Int,
    mode: Schema.String,
    bitsPerChannel: Schema.String,
    colorProfileName: Schema.String,
    width: Schema.Number,
    height: Schema.Number,
    resolution: Schema.Number,
    layers: Schema.Array(_LayerRow),
    layerCount: Schema.Int,
    layerCursor: Schema.OptionFromNullOr(Schema.Int),
});

const DocumentState: Schema.Struct<{
    readonly kind: Schema.Literal<'document'>;
    readonly documents: Schema.$Array<Schema.Struct<{ readonly id: Schema.Int; readonly name: Schema.String; readonly path: Schema.String; readonly saved: Schema.Boolean }>>;
    readonly active: Schema.OptionFromNullOr<typeof Active>;
}> = Schema.Struct({
    kind: Schema.Literal('document'),
    documents: Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, path: Schema.String, saved: Schema.Boolean })),
    active: Schema.OptionFromNullOr(Active),
});

const Played: Schema.Struct<{ readonly played: Schema.String }> = Schema.Struct({ played: Schema.String });

const Preferences: Schema.Struct<{
    readonly kind: Schema.Literal<'preferences'>;
    readonly values: Schema.$Record<Schema.String, Schema.$Record<Schema.String, Schema.Codec<Schema.Json>>>;
    readonly unreadable: Schema.$Array<Schema.Struct<typeof _keyed & { readonly cause: Schema.Defect }>>;
}> = Schema.Struct({
    kind: Schema.Literal('preferences'),
    values: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Json)),
    unreadable: Schema.Array(Schema.Struct({ ..._keyed, cause: Schema.Defect() })),
});

const Applied: Schema.Struct<{
    readonly kind: Schema.Literal<'applied'>;
    readonly applied: Schema.$Array<Schema.Struct<typeof _keyed & { readonly from: Schema.Codec<Schema.Json>; readonly to: Schema.Codec<Schema.Json> }>>;
    readonly rejected: Schema.$Array<Schema.Struct<typeof _keyed & { readonly reason: typeof Rejection }>>;
}> = Schema.Struct({
    kind: Schema.Literal('applied'),
    applied: Schema.Array(Schema.Struct({ ..._keyed, from: Schema.Json, to: Schema.Json })),
    rejected: Schema.Array(Schema.Struct({ ..._keyed, reason: Rejection })),
});

const Presets: Schema.Struct<{ readonly kind: Schema.Literal<'presets'>; readonly groupIndex: Schema.Int; readonly names: Schema.$Array<Schema.String>; readonly count: Schema.Int }> = Schema.Struct({
    kind: Schema.Literal('presets'),
    groupIndex: Schema.Int,
    names: Schema.Array(Schema.String),
    count: Schema.Int,
});

// --- [KINDS] ---------------------------------------------------------------------------

const Bodies: Schema.Struct<{
    readonly execute: typeof Execute;
    readonly batchPlay: typeof BatchPlay;
    readonly snapshot: typeof Snapshot;
    readonly getDocument: typeof GetDocument;
    readonly getPreferences: typeof GetPreferences;
    readonly setPreferences: typeof SetPreferences;
    readonly listPresets: typeof ListPresets;
    readonly runAction: typeof RunAction;
}> = Schema.Struct({
    execute: Execute,
    batchPlay: BatchPlay,
    snapshot: Snapshot,
    getDocument: GetDocument,
    getPreferences: GetPreferences,
    setPreferences: SetPreferences,
    listPresets: ListPresets,
    runAction: RunAction,
});

const Kind: Schema.Literals<Array<keyof (typeof Bodies)['fields']>> = Schema.Literals(Struct.keys(Bodies.fields));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Kind, LayerRow, PresetKind, Rejection, Target };
export {
    Active,
    Applied,
    BatchPlay,
    Bodies,
    Descriptors,
    DocumentState,
    GetDocument,
    GetPreferences,
    Jpeg,
    ListPresets,
    Played,
    PRESET_CLASSES,
    Preferences,
    Presets,
    RunAction,
    SetPreferences,
    Snapshot,
};
