import { HostError, ObjectModel, Reference, Requests, request } from '@rasm/automation';
import { Settings, Source } from '@rasm/typography';
import { UxpManifest } from '@rasm/uxp';
import { Effect, Option, Record, Schema, SchemaTransformation, Struct } from 'effect';
import { RpcMiddleware } from 'effect/rpc';
import { Slice } from '#/internal/slice';
import { Specifier } from '#/internal/specifier';
import { Value } from '#/internal/value';
import { Typesetting } from '#/typesetting';

// --- [TYPES] ---------------------------------------------------------------------------

interface Issue {
    readonly label: string;
    readonly page?: string;
    readonly details: Record.ReadonlyRecord<string, string>;
    readonly rows: readonly Issue[];
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [MEMBERS]

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}
const Collection = Schema.Struct({ type: Schema.String, specifier: Schema.String, length: Schema.Int });
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
const Issue: Schema.Codec<Issue> = Schema.Struct({
    label: Schema.String,
    page: Schema.optionalKey(Schema.String.annotate({ description: 'Page name, PB on the pasteboard' })),
    details: Schema.Record(Schema.String, Schema.String),
    rows: Schema.Array(Schema.suspend((): Schema.Codec<Issue> => Issue)),
}).annotate({ description: 'Category or rule with issue count, or issue object with problem and fix' });

// --- [OBJECTS]

class Layer extends Schema.TaggedClass<Layer, { readonly brand: unique symbol }>()('Layer', {
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    pageItems: Collection,
}) {}
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
const Swatch = Schema.TaggedUnion({
    Swatch: grouped,
    Color: { ...grouped, model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) },
    Tint: { ...grouped, baseColor: Reference, tintValue: Schema.Finite },
    Gradient: { ...grouped, type: Schema.String, gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite })) },
    MixedInk: { ...grouped, baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) },
    MixedInkGroup: grouped,
});
class Page extends Schema.TaggedClass<Page, { readonly brand: unique symbol }>()('Page', {
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    marginPreferences: Schema.Struct({
        ...Struct.Record(['top', 'bottom', 'left', 'right'], Schema.Finite),
        columnsPositions: Schema.Array(Schema.Finite).annotate({ description: 'Left and right edge of each column from left margin' }),
    }).annotate({ description: 'Left is the inside margin with facing pages' }),
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
const Spread = Schema.TaggedUnion({ Spread: { ...spread, allowPageShuffle: Schema.Boolean }, MasterSpread: { ...spread, namePrefix: Schema.String, baseName: Schema.String } });
const styled = {
    group: Schema.Array(Schema.String).annotate({ description: 'Group names, nearest first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Nearest first, root style excluded' }),
    differing: Schema.JsonObject.annotate({ description: 'Attributes differing from base or root style' }),
};
const Style = Schema.TaggedUnion({ ParagraphStyle: { ...styled, nextStyle: Reference, ...ParagraphAttributes }, CharacterStyle: styled, ObjectStyle: styled, TableStyle: styled, CellStyle: styled });
class Story extends Schema.TaggedClass<Story, { readonly brand: unique symbol }>()('Story', {
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(
        Reference.mapFields(
            Struct.assign({
                parentPage: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent on pasteboard' })),
                textFramePreferences: Schema.OptionFromOptionalKey(Schema.Struct({ textColumnCount: Schema.Int })),
                overflows: Schema.Boolean,
                characters: Collection,
            }),
        ),
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
class Font extends Schema.TaggedClass<Font, { readonly brand: unique symbol }>()('Font', {
    ...Reference.fields,
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
}) {}
class PreflightProcess extends Schema.TaggedClass<PreflightProcess, { readonly brand: unique symbol }>()('PreflightProcess', {
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for document-embedded profiles' })),
    aggregatedResults: Schema.Result(Schema.Array(Issue), Schema.Defect()).annotate({ description: 'Issues of finished process, empty without issues, refusal while checking' }),
}) {}

// --- [REQUESTS]

class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.Path.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.Every,
}) {
    static readonly Readings = Schema.Array(ObjectModel.Field.pipe(Schema.fieldsAssign({ value: Schema.Result(Schema.Json, Schema.Defect()) })));
    static readonly Object = Schema.TaggedStruct('Object', {
        ...Reference.fields,
        view: Schema.OptionFromOptionalKey(Schema.Union([Application, Document, Page, Spread, Layer, Style, Swatch, Link, Story, Text, Font, PreflightProcess])),
        fields: Inspect.Readings,
        collections: Schema.Array(Collection.mapFields(Struct.assign({ name: Schema.String }))),
    });
    static readonly Items = Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Inspect.Readings }))) });
    static readonly Result = Schema.Union([Inspect.Object, Inspect.Items]);
    static readonly Extent = Extent;
    static readonly Application = Application;
    static readonly Document = Document;
    static readonly Page = Page;
    static readonly Spread = Spread;
    static readonly Layer = Layer;
    static readonly Style = Style;
    static readonly Swatch = Swatch;
    static readonly Link = Link;
    static readonly Story = Story;
    static readonly Text = Text;
    static readonly Font = Font;
    static readonly PreflightProcess = PreflightProcess;
}
class Change extends Schema.Class<Change, { readonly brand: unique symbol }>('Change')({ target: Specifier.Path, values: Value.Fields }) {
    static readonly Result = Reference.Written;

    get label(): string {
        return `Change ${Record.keys(this.values).join(', ')} of ${this.target.leaf}`;
    }
}
class Call extends Schema.Class<Call, { readonly brand: unique symbol }>('Call')({
    target: Specifier.Path.annotate({ description: 'Object, or owner of collection' }),
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
    mode: Schema.Literals(['Text', 'Grep', 'Glyph', 'Object', 'Color', 'Transliterate']),
    find: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find preference fields (pattern, format criteria)' })),
    change: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Change preference fields for every match, absent to find only' })),
    options: Schema.OptionFromOptionalKey(Value.Fields.annotate({ description: 'Find/change option fields' })),
    scope: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Document, story, text, or page item to search, current document when absent' })),
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
class FontFile extends Schema.Class<FontFile, { readonly brand: unique symbol }>('FontFile')({
    name: Schema.String.annotate({ description: 'Font, PostScript, or full name, family name selects its Regular or first face' }),
    text: Schema.String.annotate({ description: 'Subset characters for unreadable font files' }),
    file: Schema.String,
}) {
    static readonly Result = Schema.Struct({
        font: Font,
        source: Schema.OptionFromOptionalKey(Source.mapFields(Struct.omit(['data'])).mapFields(Struct.assign({ file: Schema.String.annotate({ description: 'Installed face file, or subset written to the requested file' }) }))),
    });
}
const Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String });
class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Area in any case' })) }) {
    static readonly Result = Schema.Array(Action);
}
class Invoke extends Schema.Class<Invoke, { readonly brand: unique symbol }>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }) {
    static readonly Result = Action;

    get label(): string {
        return `Invoke ${this.action}`;
    }
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Schema.Array(Issue) }));
}

// --- [SERVICES] ------------------------------------------------------------------------

class ScriptPreferences extends RpcMiddleware.Service<ScriptPreferences>()('@rasm/indesign/contract/ScriptPreferences', { error: HostError }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const InDesign = {
    identifier: 'com.adobe.InDesign',
    Contract: Requests.add(request('inspect', Inspect), request('change', Change), request('call', Call), request('find_change', FindChange), request('font', FontFile), request('menu', Menu), request('invoke', Invoke), request('preflight', Preflight)).merge(Typesetting).middleware(ScriptPreferences),
    ScriptPreferences,
    manifest: { host: { app: 'ID', minVersion: '18.5.0' }, requiredPermissions: { localFileSystem: 'fullAccess', allowCodeGenerationFromStrings: true } },
    target: UxpManifest.target,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Call, Change, FindChange, FontFile, InDesign, Inspect, Invoke, Menu, Preflight };
