import { HostError, ObjectModel, Reference, Requests, request } from '@rasm/automation/requests';
import { Typesetting } from '@rasm/automation/typesetting';
import { Settings } from '@rasm/typography/settings';
import { Source } from '@rasm/typography/source';
import { Manifest } from '@rasm/uxp/manifest';
import { Effect, Option, Record, Schema, Struct } from 'effect';
import { RpcMiddleware } from 'effect/rpc';
import { Slice } from '#/dom/slice';
import { Specifier } from '#/dom/specifier';
import { Value } from '#/dom/value';

// --- [TYPES] ---------------------------------------------------------------------------

interface Issue {
    readonly label: string;
    readonly page?: string;
    readonly details: Record.ReadonlyRecord<string, string>;
    readonly rows: readonly Issue[];
}

// --- [MODELS] --------------------------------------------------------------------------

// --- [GEOMETRY]

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}

// --- [DOCUMENT_VIEWS]

class LayerView extends Schema.TaggedClass<LayerView, { readonly brand: unique symbol }>()('Layer', {
    ...Reference.fields,
    index: Schema.Int,
    ...Struct.Record(['visible', 'locked', 'printable', 'ignoreWrap'], Schema.Boolean),
    layerColor: Schema.Json.annotate({ description: 'UIColors member or RGB values' }),
    items: Schema.Int,
}) {}
class LinkView extends Schema.TaggedClass<LinkView, { readonly brand: unique symbol }>()('Link', {
    ...Reference.fields,
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
class SwatchView extends Schema.TaggedClass<SwatchView, { readonly brand: unique symbol }>()('Swatch', {
    ...Reference.fields,
    group: Schema.OptionFromOptionalKey(Schema.String).annotate({ description: 'Color group name' }),
    value: Schema.TaggedUnion({
        Swatch: {},
        Color: { model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) },
        Tint: { baseColor: Reference, tintValue: Schema.Finite },
        Gradient: { type: Schema.String, gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite })) },
        MixedInk: { baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) },
    }).annotate({ description: 'Empty Swatch for None and mixed ink groups' }),
}) {}
class PageView extends Schema.TaggedClass<PageView, { readonly brand: unique symbol }>()('Page', {
    ...Reference.fields,
    documentOffset: Schema.Int,
    side: Schema.String,
    extent: Extent,
    parent: Reference.annotate({ description: 'Spread or parent spread' }),
    appliedMaster: Schema.OptionFromOptionalKey(Reference),
    appliedSection: Reference,
    margins: Schema.Struct({
        ...Struct.Record(['top', 'bottom', 'left', 'right'], Schema.Finite),
        columns: Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Column left and right edges from left margin' }),
    }).annotate({ description: 'Left is the inside margin with facing pages' }),
    items: Schema.Struct({ own: Schema.Int, inherited: Schema.Int }).annotate({ description: 'Own and parent page item counts' }),
}) {}
class SpreadView extends Schema.TaggedClass<SpreadView, { readonly brand: unique symbol }>()('Spread', {
    ...Reference.fields,
    index: Schema.Int,
    showMasterItems: Schema.Boolean,
    appliedMaster: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Applied parent, or base parent of a parent spread' }),
    kind: Schema.TaggedUnion({ Spread: { allowPageShuffle: Schema.Boolean }, MasterSpread: { namePrefix: Schema.String, baseName: Schema.String } }),
    pages: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(PageView.fields, ['side', 'extent'])))).annotate({ description: 'In order' }),
    items: Schema.Int,
}) {}

// --- [TEXT_VIEWS]

class ParagraphAttributes extends Schema.Class<ParagraphAttributes, { readonly brand: unique symbol }>('ParagraphAttributes')({
    nestedGrepStyles: Schema.Array(Schema.Struct({ grepExpression: Schema.String, appliedCharacterStyle: Reference })).annotate({ description: 'In order' }),
    nestedStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, delimiter: Schema.Json, repetition: Schema.Int, inclusive: Schema.Boolean })),
    nestedLineStyles: Schema.Array(Schema.Struct({ appliedCharacterStyle: Reference, lineCount: Schema.Int, repeatLast: Schema.Int })),
    tabList: Schema.Array(Schema.Struct({ position: Schema.Finite, alignment: Schema.String, leader: Schema.String, alignmentCharacter: Schema.String })),
    bulletsAndNumberingListType: Schema.String,
    numberingLevel: Schema.Int,
    bulletChar: Schema.Int.annotate({ description: 'Code point' }),
    numberingFormat: Schema.Json,
    settings: Settings,
}) {}
class StyleView extends Schema.TaggedClass<StyleView, { readonly brand: unique symbol }>()('Style', {
    ...Reference.fields,
    group: Schema.Array(Schema.String).annotate({ description: 'Group names, nearest first' }),
    basedOn: Schema.Array(Reference).annotate({ description: 'Nearest first, root style excluded' }),
    nextStyle: Schema.OptionFromOptionalKey(Reference),
    differing: Schema.JsonObject.annotate({ description: 'Attributes differing from base or root style' }),
    paragraphAttributes: Schema.OptionFromOptionalKey(ParagraphAttributes),
}) {}
class StoryView extends Schema.TaggedClass<StoryView, { readonly brand: unique symbol }>()('Story', {
    ...Reference.fields,
    length: Schema.Int,
    overflows: Schema.Boolean,
    overset: Schema.Int.annotate({ description: 'Characters no container shows' }),
    textContainers: Schema.Array(Reference.mapFields(Struct.assign({ page: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name, absent on pasteboard' })), columns: Schema.OptionFromOptionalKey(Schema.Int), overflows: Schema.Boolean, characters: Schema.Int }))).annotate({
        description: 'Text frames and paths in thread order',
    }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Applied paragraph style names' }),
}) {}
const Run = Reference.mapFields(
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
);
class TextView extends Schema.TaggedClass<TextView, { readonly brand: unique symbol }>()('Text', {
    ...Reference.fields,
    parentStory: Reference,
    contents: Schema.Json.annotate({ description: 'Text or SpecialCharacters member' }),
    length: Schema.Int,
    appliedParagraphStyle: Reference,
    appliedCharacterStyle: Reference,
    overrides: Schema.Struct(Struct.Record(['paragraph', 'character'], Schema.Boolean)).annotate({ description: 'Text differs from paragraph or character style' }),
    ranges: Schema.Int.annotate({ description: 'Style range count' }),
    runs: Schema.Array(Run).annotate({ description: 'Slice style ranges in order' }),
    paragraphAttributes: ParagraphAttributes,
}) {}
class DocumentView extends Schema.TaggedClass<DocumentView, { readonly brand: unique symbol }>()('Document', {
    ...Reference.fields,
    saved: Schema.Boolean,
    modified: Schema.Boolean,
    path: Schema.OptionFromOptionalKey(Schema.String),
    windows: Schema.Array(Schema.String),
    intent: Schema.String,
    facingPages: Schema.Boolean,
    size: Schema.Struct({ width: Schema.Finite, height: Schema.Finite }).annotate({ description: 'Default page size in points' }),
    units: Schema.Struct({ horizontal: Schema.String, vertical: Schema.String, origin: Schema.String }).annotate({ description: 'Ruler units and origin, results in points' }),
    pages: Schema.Int,
    spreads: Schema.Int,
    parents: Schema.Array(Reference.mapFields(Struct.assign({ pages: Schema.Int }))).annotate({ description: 'Parent (master) spreads' }),
    sections: Schema.Array(Reference.mapFields(Struct.assign({ start: Schema.String, length: Schema.Int, pageNumberStart: Schema.Int, continueNumbering: Schema.Boolean }))),
    layers: Schema.Array(Reference.mapFields(Struct.assign(Struct.pick(LayerView.fields, ['visible', 'locked', 'printable'])))),
    stories: Schema.Struct({ count: Schema.Int, overset: Schema.Array(Schema.String) }),
    links: Schema.Record(Schema.String, Schema.Int).annotate({ description: 'Count per LinkStatus member' }),
    fonts: Schema.Array(Schema.Struct({ name: Schema.String, status: Schema.String })).annotate({ description: 'FontStatus per font' }),
    swatches: Schema.Array(Reference),
    styles: Schema.Struct(Struct.Record(['paragraph', 'character', 'object', 'table', 'cell'], Schema.Int)).annotate({ description: 'Counts with grouped styles' }),
    preflightOptions: Schema.Struct({
        ...Struct.Record(['preflightOff', 'preflightIncludeObjectsOnPasteboard', 'preflightIncludeNonprintingObjects'], Schema.Boolean),
        ...Struct.Record(['preflightProfilePolicy', 'preflightWorkingProfile', 'preflightScope', 'preflightWhichLayers'], Schema.String),
    }),
    activeProcess: Schema.OptionFromOptionalKey(Reference).annotate({ description: 'Absent with live preflight off' }),
    textDefaults: ParagraphAttributes,
}) {
    static readonly Summary = Schema.Struct(Struct.pick(DocumentView.fields, ['type', 'specifier', 'name', 'valid', 'saved', 'modified', 'path', 'windows']));
}
class ApplicationView extends Schema.TaggedClass<ApplicationView, { readonly brand: unique symbol }>()('Application', {
    ...Reference.fields,
    version: Schema.String,
    tool: Schema.String,
    panels: Schema.Array(Schema.String),
    documents: Schema.Array(DocumentView.Summary),
    current: Schema.OptionFromOptionalKey(
        Schema.Struct({
            document: Schema.String,
            window: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Absent for a document open without window' })),
            selection: Schema.Array(Reference.mapFields(Struct.assign({ contents: Schema.OptionFromOptionalKey(Schema.Json), parentStory: Schema.OptionFromOptionalKey(Schema.String), overrides: Schema.OptionFromOptionalKey(TextView.fields.overrides) }))),
            overset: Schema.Array(Schema.String),
            undo: Schema.Struct({ mode: Schema.String, name: Schema.String, history: Schema.Array(Schema.String) }),
        }),
    ).annotate({ description: 'Single open document, else document of the active window' }),
}) {}

// --- [FONTS]

const Installed = Schema.Struct({
    postscriptName: Schema.String,
    fullName: Schema.String,
    version: Schema.String,
    type: Schema.String,
    location: Schema.String.annotate({ description: 'File path, or label without readable file' }),
    styles: Schema.Array(Schema.String).annotate({ description: 'Style names' }),
    axes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ name: Schema.String, value: Schema.Finite })).annotate({ description: 'Instance axes by InDesign name in fvar order, values to 2 decimals' })),
    features: Schema.Array(Schema.String).annotate({ description: 'OpenTypeFeature members in GSUB or GPOS' }),
    allows: Schema.Struct(Struct.Record(['allowOutlines', 'allowPDFEmbedding', 'allowEditableEmbedding', 'restrictedPrinting'], Schema.Boolean)),
});
class FontView extends Schema.TaggedClass<FontView, { readonly brand: unique symbol }>()('Font', {
    ...Reference.fields,
    family: Schema.String,
    style: Schema.OptionFromOptionalKey(Schema.String),
    status: Schema.String,
    uses: Schema.Array(Schema.String).annotate({ description: 'Current document text and styles set in the face' }),
    installed: Schema.OptionFromOptionalKey(Installed),
}) {
    static readonly Installed = Installed;
}
class Font extends Schema.Class<Font, { readonly brand: unique symbol }>('Font')({
    name: Schema.String.annotate({ description: 'Font, PostScript, or full name, family name selects its Regular or first face' }),
    text: Schema.String.annotate({ description: 'Subset characters for unreadable font files' }),
    file: Schema.String,
}) {
    static readonly View = FontView;
    static readonly Result = Schema.Struct({
        view: FontView,
        source: Schema.OptionFromOptionalKey(Source.mapFields(Struct.omit(['data'])).mapFields(Struct.assign({ file: Schema.String.annotate({ description: 'Installed face file, or subset written to the requested file' }) }))),
    });
}

// --- [PREFLIGHT]

const Issue: Schema.Codec<Issue> = Schema.Struct({
    label: Schema.String,
    page: Schema.optionalKey(Schema.String.annotate({ description: 'Page name, PB on the pasteboard' })),
    details: Schema.Record(Schema.String, Schema.String),
    rows: Schema.Array(Schema.suspend((): Schema.Codec<Issue> => Issue)),
}).annotate({ description: 'Category or rule with issue count, or issue object with problem and fix' });
class PreflightView extends Schema.TaggedClass<PreflightView, { readonly brand: unique symbol }>()('PreflightProcess', {
    ...Reference.fields,
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for document-embedded profiles' })),
    aggregatedResults: Schema.Result(Schema.Array(Issue), Schema.Defect()).annotate({ description: 'Issues of finished process, empty without issues, refusal while checking' }),
}) {}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly View = PreflightView;
    static readonly Issue = Issue;
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Schema.Array(Issue) }));
}

// --- [REQUESTS]

class Inspect extends Schema.Class<Inspect, { readonly brand: unique symbol }>('Inspect')({
    target: Specifier.Path.annotate({ description: 'Object or collection specifier, / for application' }),
    fields: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Fields to read, collections of viewed objects or every field when absent' })),
    slice: Slice.Every,
}) {
    static readonly View = Schema.Union([ApplicationView, DocumentView, PageView, SpreadView, LayerView, StyleView, SwatchView, LinkView, StoryView, TextView, Font.View, Preflight.View]);
    static readonly Readings = Schema.Array(ObjectModel.Field.pipe(Schema.fieldsAssign({ value: Schema.Result(Schema.Json, Schema.Defect()) })));
    static readonly Object = Schema.TaggedStruct('Object', {
        ...Reference.fields,
        view: Schema.OptionFromOptionalKey(Inspect.View),
        fields: Inspect.Readings,
        collections: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, count: Schema.Int, specifier: Schema.String })),
    });
    static readonly Items = Schema.TaggedStruct('Items', { specifier: Schema.String, total: Schema.Int, items: Schema.Array(Reference.mapFields(Struct.assign({ fields: Inspect.Readings }))) });
    static readonly Result = Schema.Union([Inspect.Object, Inspect.Items]);
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
const Action = Schema.Struct({ id: Schema.Int, name: Schema.String, area: Schema.String });
class Invoke extends Schema.Class<Invoke, { readonly brand: unique symbol }>('Invoke')({ action: Schema.Union([Schema.Int, Schema.String]).annotate({ description: 'Menu action id or name' }) }) {
    static readonly Result = Action;

    get label(): string {
        return `Invoke ${this.action}`;
    }
}
class Menu extends Schema.Class<Menu, { readonly brand: unique symbol }>('Menu')({ area: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Area in any case' })) }) {
    static readonly Invoke = Invoke;
    static readonly Result = Schema.Array(Action);
}

// --- [SERVICES] ------------------------------------------------------------------------

class ScriptPreferences extends RpcMiddleware.Service<ScriptPreferences>()('@rasm/indesign/contract/ScriptPreferences', { error: HostError }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const InDesign = {
    identifier: 'com.adobe.InDesign',
    Contract: Requests.add(request('inspect', Inspect), request('change', Change), request('call', Call), request('find_change', FindChange), request('font', Font), request('menu', Menu), request('invoke', Menu.Invoke), request('preflight', Preflight)).merge(Typesetting).middleware(ScriptPreferences),
    ScriptPreferences,
    manifest: { host: { app: 'ID', minVersion: '18.5.0' }, requiredPermissions: { localFileSystem: 'fullAccess', allowCodeGenerationFromStrings: true } },
    target: Manifest.target,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationView, Call, Change, DocumentView, Extent, FindChange, Font, InDesign, Inspect, LayerView, LinkView, Menu, PageView, ParagraphAttributes, Preflight, SpreadView, StoryView, StyleView, SwatchView, TextView };
