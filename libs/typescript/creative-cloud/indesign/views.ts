import { Reference, Specifier } from '@rasm/object-model/model';
import { Settings } from '@rasm/typography/settings';
import type indesign from 'adobe:indesign';
import { Effect, Option, Record, Schema, SchemaTransformation, Struct } from 'effect';
import { Value } from '#/value';

// --- [TYPES] ---------------------------------------------------------------------------

type Member<Host> = Extract<keyof Host, string>;
type Properties<Host, Computed extends string = never> = Partial<Record.ReadonlyRecord<Member<Host> | Computed, unknown>>;
type FindChangeKind<Key = keyof indesign.Application> = Key extends `findChange${infer Kind}Options` ? Kind : never;

// --- [MODELS] --------------------------------------------------------------------------

// --- [MEMBERS]

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}
const FindChangeMode = Object.assign(Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color', 'Transliterate'] satisfies readonly FindChangeKind[]).annotate({ description: 'Find/change kind, each with its find and change methods and preference objects' }), {
    members: <Kind extends FindChangeKind>(kind: Kind) => ({ find: `find${kind}` as const, change: `change${kind}` as const, preferences: { find: `find${kind}Preferences`, options: `findChange${kind}Options`, change: `change${kind}Preferences` } as const }),
});

class Slice extends Schema.Class<Slice, { readonly brand: unique symbol }>('Slice')({
    offset: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)))
        .annotate({ description: 'Items skipped' }),
    limit: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Items returned, absent returns the rest' })),
}) {
    static readonly Every = Slice.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));

    of<A>(items: readonly A[]): readonly A[] {
        return items.slice(this.offset, Option.getOrUndefined(Option.map(this.limit, (limit) => this.offset + limit)));
    }
}
class Collection extends Schema.Opaque<Collection>()(Schema.Struct({ type: Schema.String, specifier: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Every-item specifier, absent where the host gives the collection no every-item object (InDesign Panels)' })), length: Schema.Int })) {}
const Count = Schema.Struct({ length: Schema.Int }).pipe(Schema.decodeTo(Schema.Int, SchemaTransformation.transform({ decode: Struct.get('length'), encode: (length) => ({ length }) })));
const ParagraphAttributes = {
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference } satisfies Properties<indesign.NestedGrepStyle>)).annotate({ description: 'In order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean } satisfies Properties<indesign.NestedStyle>)),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int } satisfies Properties<indesign.NestedLineStyle>)),
    tabList: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String } satisfies Properties<indesign.TabStop>)),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Struct({ characterValue: Schema.Int.annotate({ description: 'Code point' }) } satisfies Properties<indesign.Bullet>),
    numberingFormat: Schema.Json,
    settings: Settings,
} satisfies Properties<indesign.ParagraphStyle | indesign.Text | indesign.TextDefault, 'settings'>;
class Issue extends Schema.Opaque<Issue>()(
    Schema.Struct({
        label: Schema.String,
        page: Schema.optionalKey(Schema.String.annotate({ description: 'Page name, PB on the pasteboard' })),
        info: Schema.optionalKey(Schema.String.annotate({ description: 'Error info the rule states for the issue' })),
        details: Schema.Record(Schema.String, Schema.String),
        rows: Schema.Array(Schema.suspend((): Schema.Codec<Issue> => Issue)),
    }).annotate({ description: 'Category or rule with issue count, or issue object with problem and fix' }),
) {}

// --- [VIEWS]

class Layer extends Schema.TaggedClass<Layer, { readonly brand: unique symbol }>()('Layer', {
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'] satisfies readonly Member<indesign.Layer>[], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    pageItems: Collection,
} satisfies Properties<indesign.Layer>) {}
class Link extends Schema.TaggedClass<Link, { readonly brand: unique symbol }>()('Link', {
    status: Schema.String,
    filePath: Schema.String,
    linkResourceURI: Schema.String,
    linkType: Schema.String,
    needed: Schema.Boolean,
    edited: Schema.Boolean,
    size: Schema.Finite.annotate({ description: 'Source file bytes' }),
    date: Schema.String.annotate({ description: 'Source file modification date' }),
    parent: Reference.annotate({ description: 'Placed graphic or story' }),
} satisfies Properties<indesign.Link>) {}
class Page extends Schema.TaggedClass<Page, { readonly brand: unique symbol }>()('Page', {
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    marginPreferences: Schema.Struct({ top: Schema.Finite, bottom: Schema.Finite, left: Schema.Finite, right: Schema.Finite, columnsPositions: Schema.Array(Schema.Finite).annotate({ description: 'Left and right edge of each column from left margin' }) } satisfies Properties<indesign.MarginPreference>).annotate({
        description: 'Left is the inside margin with facing pages',
    }),
    pageItems: Collection,
    masterPageItems: Count.annotate({ description: 'Parent page items shown' }),
} satisfies Properties<indesign.Page, 'extent'>) {}
const spreadFields = {
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent, or base parent of a parent spread' }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(Page.fields, ['side', 'extent'])))).annotate({ description: 'In order' }),
    pageItems: Collection,
} satisfies Properties<indesign.Spread | indesign.MasterSpread>;
class Spread extends Schema.TaggedClass<Spread, { readonly brand: unique symbol }>()('Spread', { ...spreadFields, allowPageShuffle: Schema.Boolean } satisfies Properties<indesign.Spread>) {}
class MasterSpread extends Schema.TaggedClass<MasterSpread, { readonly brand: unique symbol }>()('MasterSpread', { ...spreadFields, namePrefix: Schema.String, baseName: Schema.String } satisfies Properties<indesign.MasterSpread>) {}
class Story extends Schema.TaggedClass<Story, { readonly brand: unique symbol }>()('Story', {
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(
        Reference.mapFields(
            Struct.assign({
                parentPage: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent on pasteboard and for text paths' })),
                textFramePreferences: Schema.OptionFromOptionalKey(Schema.Struct({ textColumnCount: Schema.Int } satisfies Properties<indesign.TextFramePreference>).annotate({ description: 'Absent for text paths' })),
                overflows: Schema.Boolean,
                characters: Collection,
            } satisfies Properties<indesign.TextFrame>),
        ),
    ).annotate({ description: 'Text frames and paths in thread order' }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Applied paragraph style names' }),
} satisfies Properties<indesign.Story, 'overset' | 'styles'>) {}
class Text extends Schema.TaggedClass<Text, { readonly brand: unique symbol }>()('Text', {
    parentStory: Reference,
    contents: Schema.Json.annotate({ description: 'Text or SpecialCharacters member' }),
    length: Schema.Int,
    appliedParagraphStyle: Reference,
    appliedCharacterStyle: Reference,
    overrides: Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Text differs from paragraph or character style' }),
    textStyleRanges: Collection,
    runs: Schema.Array(
        Reference.mapFields(
            Struct.assign({
                contents: Schema.Json,
                length: Schema.Int,
                appliedCharacterStyle: Reference,
                appliedFont: Reference,
                pointSize: Schema.Finite,
                designAxes: Schema.Result(Schema.Array(Schema.Finite), Schema.Defect()).annotate({ description: 'Axis values in fvar order, empty for static faces, refused at some face sizes' }),
                opentypeFeatures: Schema.Array(Schema.Json).annotate({ description: 'Tag and value pairs' }),
                kerningValue: Schema.OptionFromOptionalKey(Schema.Finite).annotate({ description: 'Absent under metrics or optical kerning' }),
                appliedConditions: Schema.Array(Reference),
                rubyFlag: Schema.Boolean,
                rubyString: Schema.String,
                settings: Settings,
                differing: Schema.JsonObject.annotate({ description: 'Attributes differing from character style, else paragraph style' }),
            } satisfies Properties<indesign.TextStyleRange, 'settings' | 'differing'>),
        ),
    ).annotate({ description: 'Slice style ranges in order' }),
    ...ParagraphAttributes,
} satisfies Properties<indesign.Text, 'overrides' | 'runs'>) {}
class Document extends Schema.TaggedClass<Document, { readonly brand: unique symbol }>()('Document', {
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    fullName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'File path, absent before first save' })),
    windows: Schema.Array(Reference),
    documentPreferences: Schema.Struct({ intent: Schema.String, facingPages: Schema.Boolean, pageWidth: Schema.Finite, pageHeight: Schema.Finite } satisfies Properties<indesign.DocumentPreference>).annotate({ description: 'Default page size in points' }),
    viewPreferences: Schema.Struct({ horizontalMeasurementUnits: Schema.String, verticalMeasurementUnits: Schema.String, rulerOrigin: Schema.String } satisfies Properties<indesign.ViewPreference>).annotate({ description: 'Ruler units and origin, results in points' }),
    pages: Collection,
    spreads: Collection,
    masterSpreads: Schema.Array(Reference.mapFields(Struct.assign({ pages: Collection } satisfies Properties<indesign.MasterSpread>))),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ pageStart: Reference, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean } satisfies Properties<indesign.Section>))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(Layer.fields, ['visible', 'locked', 'printable'])))),
    stories: Collection,
    overset: Schema.Array(Schema.String).annotate({ description: 'Overset story specifiers' }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String } satisfies Properties<indesign.Font>)),
    swatches: Schema.Array(Reference),
    placeGuns: Schema.Struct({ loaded: Schema.Boolean, pageItems: Collection } satisfies Properties<indesign.PlaceGun>),
    ...Struct.Record(['allParagraphStyles', 'allCharacterStyles', 'allObjectStyles', 'allTableStyles', 'allCellStyles'] satisfies readonly Member<indesign.Document>[], Count.annotate({ description: 'Count including styles in style groups' })),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'] satisfies readonly Member<indesign.PreflightOption>[], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightScope', 'preflightWhichLayers'] satisfies readonly Member<indesign.PreflightOption>[], Schema.String),
        preflightWorkingProfile: Schema.Union([Schema.String, Reference]),
    } satisfies Properties<indesign.PreflightOption>),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Absent with live preflight off' }),
    textDefaults: Schema.Struct(ParagraphAttributes),
} satisfies Properties<indesign.Document, 'overset'>) {}
class Application extends Schema.TaggedClass<Application, { readonly brand: unique symbol }>()('Application', {
    version: Schema.String,
    toolBoxTools: Schema.Struct({ currentTool: Schema.String, currentToolName: Schema.String, currentToolHint: Schema.String, tools: Schema.Array(Schema.String) } satisfies Properties<indesign.ToolBox, 'tools'>),
    panels: Schema.Array(Schema.String).annotate({ description: 'Visible panel names' }),
    documents: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(Document.fields, ['saved', 'modified', 'fullName', 'windows'])))),
    current: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Schema.String,
            window: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Absent for a document open without window' })),
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Reference), overrides: Schema.OptionFromOptionalKey(Text.fields.overrides) }))),
            overset: Document.fields.overset,
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ).annotate({ description: 'Single open document, else document of the active window' }),
} satisfies Properties<indesign.Application, 'current'>) {}
class Font extends Schema.TaggedClass<Font, { readonly brand: unique symbol }>()(
    'Font',
    Reference.mapFields(
        Struct.assign({
            fontFamily: Schema.String,
            status: Schema.String,
            uses: Schema.Array(Schema.String).annotate({ description: 'Current document text and styles set in the face' }),
            installed: Schema.OptionFromOptionalKey(
                Schema.Struct({
                    ...Struct.Record(['fontStyleName', 'postscriptName', 'fullName', 'version', 'fontType'] satisfies readonly Member<indesign.Font>[], Schema.String),
                    location: Schema.String.annotate({ description: 'File path, or label without readable file' }),
                    styles: Schema.Array(Schema.String).annotate({ description: 'Style names of the family' }),
                    axes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Finite })).annotate({ description: 'Instance axes by InDesign name in fvar order, values to 2 decimals' })),
                    features: Schema.Array(Schema.String).annotate({ description: 'OpenTypeFeature members in GSUB or GPOS' }),
                    ...Struct.Record(['allowOutlines', 'allowPDFEmbedding', 'allowEditableEmbedding', 'restrictedPrinting'] satisfies readonly Member<indesign.Font>[], Schema.Boolean),
                } satisfies Properties<indesign.Font, 'styles' | 'axes' | 'features'>),
            ),
        } satisfies Properties<indesign.Font, 'uses' | 'installed'>),
    ),
) {}
const styleFields = {
    group: Schema.Array(Schema.String).annotate({ description: 'Group names, nearest first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Nearest first, root style excluded' }),
    differing: Schema.JsonObject.annotate({ description: 'Attributes differing from base or root style' }),
} satisfies Properties<indesign.ParagraphStyle | indesign.CharacterStyle | indesign.ObjectStyle | indesign.TableStyle | indesign.CellStyle, 'group' | 'differing'>;
class ParagraphStyle extends Schema.TaggedClass<ParagraphStyle, { readonly brand: unique symbol }>()('ParagraphStyle', { ...styleFields, nextStyle: Reference, ...ParagraphAttributes } satisfies Properties<indesign.ParagraphStyle, 'group' | 'differing'>) {}
class CharacterStyle extends Schema.TaggedClass<CharacterStyle, { readonly brand: unique symbol }>()('CharacterStyle', styleFields) {}
class ObjectStyle extends Schema.TaggedClass<ObjectStyle, { readonly brand: unique symbol }>()('ObjectStyle', styleFields) {}
class TableStyle extends Schema.TaggedClass<TableStyle, { readonly brand: unique symbol }>()('TableStyle', styleFields) {}
class CellStyle extends Schema.TaggedClass<CellStyle, { readonly brand: unique symbol }>()('CellStyle', styleFields) {}
const swatchFields = { parentColorGroup: Schema.OptionFromOptionalKey(Reference) } satisfies Properties<indesign.AnySwatch>;
class Swatch extends Schema.TaggedClass<Swatch, { readonly brand: unique symbol }>()('Swatch', swatchFields) {}
class Color extends Schema.TaggedClass<Color, { readonly brand: unique symbol }>()('Color', { ...swatchFields, model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) } satisfies Properties<indesign.Color>) {}
class Tint extends Schema.TaggedClass<Tint, { readonly brand: unique symbol }>()('Tint', { ...swatchFields, baseColor: Reference, tintValue: Schema.Finite } satisfies Properties<indesign.Tint>) {}
class Gradient extends Schema.TaggedClass<Gradient, { readonly brand: unique symbol }>()('Gradient', {
    ...swatchFields,
    type: Schema.String,
    gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite } satisfies Properties<indesign.GradientStop>)),
} satisfies Properties<indesign.Gradient>) {}
class MixedInk extends Schema.TaggedClass<MixedInk, { readonly brand: unique symbol }>()('MixedInk', { ...swatchFields, baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) } satisfies Properties<indesign.MixedInk>) {}
class MixedInkGroup extends Schema.TaggedClass<MixedInkGroup, { readonly brand: unique symbol }>()('MixedInkGroup', swatchFields) {}
class PreflightProcess extends Schema.TaggedClass<PreflightProcess, { readonly brand: unique symbol }>()('PreflightProcess', {
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for document-embedded profiles' })),
    aggregatedResults: Schema.Result(Schema.Array(Issue), Schema.Defect()).annotate({ description: 'Issues of finished process, empty without issues, refusal while checking' }),
} satisfies Properties<indesign.PreflightProcess>) {}

// --- [INSPECTION]

const Readings = Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Result(Schema.Json, Schema.Defect()) }));
const View = Schema.Union([Application, Document, Page, Spread, MasterSpread, Layer, Link, Story, Text, Font, ParagraphStyle, CharacterStyle, ObjectStyle, TableStyle, CellStyle, Swatch, Color, Tint, Gradient, MixedInk, MixedInkGroup, PreflightProcess]).pipe(Schema.toTaggedUnion('_tag'));
class ObjectView extends Schema.Opaque<ObjectView>()(Schema.TaggedStruct('Object', { ...Reference.fields, view: Schema.OptionFromOptionalKey(View), fields: Readings, collections: Schema.Array(Collection.mapFields(Struct.assign({ name: Schema.String }))) })) {}
class Items extends Schema.Opaque<Items>()(Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Readings }))) })) {}
const Inspection = Object.assign(Schema.Union([ObjectView, Items]), { Readings, View, Object: ObjectView, Items, Issues: Schema.Array(Issue) });

// --- [REQUESTS]

class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.FromString.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.Every,
}) {}
class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')({ target: Specifier.FromString, values: Value.Fields }) {
    get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${this.target.leaf}`;
    }
}
class Call extends Schema.Class<Call, { readonly brand: unique symbol }>('Call')({
    target: Specifier.FromString.annotate({ description: 'Object, or owner of collection' }),
    collection: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Collection field of target, its method called in place of the object method' })),
    method: Schema.String.annotate({ description: 'Method name (add, remove, place, duplicate, move, save, close, exportFile)' }),
    arguments: Schema.Array(Value)
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])))
        .annotate({ description: 'Positional arguments' }),
    properties: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Fields written onto returned object' })),
}) {
    static readonly Result = Reference.mapFields(Struct.assign({ result: Schema.OptionFromOptionalKey(Schema.Json) }));

    get label(): string {
        return `Call ${[...Option.toArray(this.collection), this.method].join(' ')} of ${this.target.leaf}`;
    }
}
class FindChange extends Schema.Class<FindChange, { readonly brand: unique symbol }>('FindChange')({
    mode: FindChangeMode,
    find: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find preference fields (pattern, format criteria)' })),
    change: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Change preference fields for every match, absent to find only' })),
    options: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find/change option fields' })),
    scope: Schema.OptionFromOptionalKey(Specifier.FromString.annotate({ description: 'Document, story, text, or page item to search, current document when absent' })),
    slice: Slice.Every,
}) {
    static readonly Result = Schema.Struct({
        count: Schema.Struct({ matches: Schema.Int, changes: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Absent without change' })) }),
        matches: Schema.Array(Schema.Struct({ specifier: Schema.String, text: Schema.OptionFromOptionalKey(Schema.String), page: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Matches with text before changes, empty for Color' }),
    });

    get label(): string {
        return `${Option.isSome(this.change) ? 'Change' : 'Find'} ${this.mode}`;
    }
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.FromString.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Inspection.Issues }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Call, Change, Collection, Extent, FindChange, FindChangeMode, Font, Inspect, Inspection, type Issue, Items, ObjectView, Preflight, Slice };
