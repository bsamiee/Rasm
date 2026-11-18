import { Reference } from '@rasm/object-model';
import { Settings } from '@rasm/typography';
import { Effect, Option, type Record, Schema, SchemaTransformation, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Issue {
    readonly label: string;
    readonly page?: string;
    readonly details: Record.ReadonlyRecord<string, string>;
    readonly rows: readonly Issue[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODES = ['Text', 'Grep', 'Glyph', 'Object', 'Color', 'Transliterate'] as const;

// --- [MODELS] --------------------------------------------------------------------------

// --- [MEMBERS]

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}
const Search = Object.assign(Schema.Literals(MODES).annotate({ description: 'Find/change kind, each with its find and change methods and preference objects' }), {
    members: <M extends (typeof MODES)[number]>(mode: M) => ({ find: `find${mode}` as const, change: `change${mode}` as const, preferences: [`find${mode}Preferences`, `findChange${mode}Options`, `change${mode}Preferences`] as const }),
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
const Collection = Schema.Struct({ type: Schema.String, specifier: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Every-item specifier, absent where the host gives the collection no every-item object (InDesign Panels)' })), length: Schema.Int });
const Count = Schema.Struct({ length: Schema.Int }).pipe(Schema.decodeTo(Schema.Int, SchemaTransformation.transform({ decode: Struct.get('length'), encode: (length) => ({ length }) })));
const ParagraphAttributes = {
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'In order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
    tabList: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String })),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Struct({ characterValue: Schema.Int.annotate({ description: 'Code point' }) }),
    numberingFormat: Schema.Json,
    settings: Settings,
};
const Issue: Schema.Codec<Issue> = Schema.Struct({ label: Schema.String, page: Schema.optionalKey(Schema.String.annotate({ description: 'Page name, PB on the pasteboard' })), details: Schema.Record(Schema.String, Schema.String), rows: Schema.Array(Schema.suspend((): Schema.Codec<Issue> => Issue)) }).annotate({
    description: 'Category or rule with issue count, or issue object with problem and fix',
});

// --- [OBJECTS]

class Layer extends Schema.TaggedClass<Layer, { readonly brand: unique symbol }>()('Layer', { index: Schema.Int, ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean), layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }), pageItems: Collection }) {}
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
}) {}
const grouped = { parentColorGroup: Schema.OptionFromOptionalKey(Reference) };
class Page extends Schema.TaggedClass<Page, { readonly brand: unique symbol }>()('Page', {
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    marginPreferences: Schema.Struct({ ...Struct.Record(['top', 'bottom', 'left', 'right'], Schema.Finite), columnsPositions: Schema.Array(Schema.Finite).annotate({ description: 'Left and right edge of each column from left margin' }) }).annotate({ description: 'Left is the inside margin with facing pages' }),
    pageItems: Collection,
    masterPageItems: Count.annotate({ description: 'Parent page items shown' }),
}) {}
const spread = {
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent, or base parent of a parent spread' }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(Page.fields, ['side', 'extent'])))).annotate({ description: 'In order' }),
    pageItems: Collection,
};
const styled = { group: Schema.Array(Schema.String).annotate({ description: 'Group names, nearest first' }), basedOn: Schema.Array(Reference).annotate({ description: 'Nearest first, root style excluded' }), differing: Schema.JsonObject.annotate({ description: 'Attributes differing from base or root style' }) };
class Story extends Schema.TaggedClass<Story, { readonly brand: unique symbol }>()('Story', {
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(
        Reference.mapFields(Struct.assign({ parentPage: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent on pasteboard' })), textFramePreferences: Schema.OptionFromOptionalKey(Schema.Struct({ textColumnCount: Schema.Int })), overflows: Schema.Boolean, characters: Collection })),
    ).annotate({ description: 'Text frames and paths in thread order' }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Applied paragraph style names' }),
}) {}
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
            }),
        ),
    ).annotate({ description: 'Slice style ranges in order' }),
    ...ParagraphAttributes,
}) {}
class Document extends Schema.TaggedClass<Document, { readonly brand: unique symbol }>()('Document', {
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    fullName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'File path, absent before first save' })),
    windows: Schema.Array(Reference),
    documentPreferences: Schema.Struct({ intent: Schema.String, facingPages: Schema.Boolean, pageWidth: Schema.Finite, pageHeight: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    viewPreferences: Schema.Struct({ horizontalMeasurementUnits: Schema.String, verticalMeasurementUnits: Schema.String, rulerOrigin: Schema.String }).annotate({ description: 'Ruler units and origin, results in points' }),
    pages: Collection,
    spreads: Collection,
    masterSpreads: Schema.Array(Reference.mapFields(Struct.assign({ pages: Collection }))),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ pageStart: Reference, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(Layer.fields, ['visible', 'locked', 'printable'])))),
    stories: Collection,
    overset: Schema.Array(Schema.String).annotate({ description: 'Overset story specifiers' }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })),
    swatches: Schema.Array(Reference),
    ...Struct.Record(['allParagraphStyles', 'allCharacterStyles', 'allObjectStyles', 'allTableStyles', 'allCellStyles'], Count.annotate({ description: 'Count with grouped styles' })),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightScope', 'preflightWhichLayers'], Schema.String),
        preflightWorkingProfile: Schema.Union([Schema.String, Reference]),
    }),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Absent with live preflight off' }),
    textDefaults: Schema.Struct(ParagraphAttributes),
}) {}
class Application extends Schema.TaggedClass<Application, { readonly brand: unique symbol }>()('Application', {
    version: Schema.String,
    toolBoxTools: Schema.Struct({ currentTool: Schema.String }),
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
}) {}
class Font extends Schema.TaggedClass<Font, { readonly brand: unique symbol }>()(
    'Font',
    Reference.mapFields(
        Struct.assign({
            fontFamily: Schema.String,
            status: Schema.String,
            uses: Schema.Array(Schema.String).annotate({ description: 'Current document text and styles set in the face' }),
            installed: Schema.OptionFromOptionalKey(
                Schema.Struct({
                    ...Struct.Record(['fontStyleName', 'postscriptName', 'fullName', 'version', 'fontType'], Schema.String),
                    location: Schema.String.annotate({ description: 'File path, or label without readable file' }),
                    styles: Schema.Array(Schema.String).annotate({ description: 'Style names of the family' }),
                    axes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Finite })).annotate({ description: 'Instance axes by InDesign name in fvar order, values to 2 decimals' })),
                    features: Schema.Array(Schema.String).annotate({ description: 'OpenTypeFeature members in GSUB or GPOS' }),
                    ...Struct.Record(['allowOutlines', 'allowPDFEmbedding', 'allowEditableEmbedding', 'restrictedPrinting'], Schema.Boolean),
                }),
            ),
        }),
    ),
) {}
class PreflightProcess extends Schema.TaggedClass<PreflightProcess, { readonly brand: unique symbol }>()('PreflightProcess', {
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for document-embedded profiles' })),
    aggregatedResults: Schema.Result(Schema.Array(Issue), Schema.Defect()).annotate({ description: 'Issues of finished process, empty without issues, refusal while checking' }),
}) {}

// --- [RESULTS]

const Called = Reference.mapFields(Struct.assign({ result: Schema.OptionFromOptionalKey(Schema.Json) }));
const MenuActions = Schema.Array(Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String }));

// --- [INSPECTION]

const Readings = Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Result(Schema.Json, Schema.Defect()) }));
const View = Schema.Union([Application, Document, Page, Layer, Link, Story, Text, Font, PreflightProcess]).pipe(Schema.toTaggedUnion('_tag'));
const Spread = Schema.TaggedUnion({ Spread: { ...spread, allowPageShuffle: Schema.Boolean }, MasterSpread: { ...spread, namePrefix: Schema.String, baseName: Schema.String } });
const Style = Schema.TaggedUnion({ ParagraphStyle: { ...styled, nextStyle: Reference, ...ParagraphAttributes }, CharacterStyle: styled, ObjectStyle: styled, TableStyle: styled, CellStyle: styled });
const Swatch = Schema.TaggedUnion({
    Swatch: grouped,
    Color: { ...grouped, model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) },
    Tint: { ...grouped, baseColor: Reference, tintValue: Schema.Finite },
    Gradient: { ...grouped, type: Schema.String, gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite })) },
    MixedInk: { ...grouped, baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) },
    MixedInkGroup: grouped,
});
const Viewed = Schema.TaggedStruct('Object', Reference.fields).mapFields(Struct.assign({ view: Schema.OptionFromOptionalKey(Schema.Union([View, Spread, Style, Swatch])), fields: Readings, collections: Schema.Array(Collection.mapFields(Struct.assign({ name: Schema.String }))) }));
const Items = Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Readings }))) });
const Inspection = Object.assign(Schema.Union([Viewed, Items]), { Readings, View, Spread, Style, Swatch, Object: Viewed, Items, Collection, Issues: Schema.Array(Issue) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Called, Document, Extent, Font, Inspection, type Issue, Layer, Link, MenuActions, Page, PreflightProcess, Search, Slice, Story, Text };
