import { Effect, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = (typeof FactScalar)['Type'] | { readonly _tag: 'list'; readonly values: readonly Fact[] } | { readonly _tag: 'record'; readonly fields: Readonly<Record<string, Fact>> };
interface OptionFromOptionalKeyLambda extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRAPHEMES = new Intl.Segmenter(undefined, { granularity: 'grapheme' });

// --- [MODELS] --------------------------------------------------------------------------

const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const Ref = Schema.Natural.annotate({ description: 'Reference to a container, control, or option, the same for every handle and kept while the instance runs; a container shown again keeps its references' });
const Locator = Schema.String.annotate({ description: 'Canonical encoding of a locator, its steps from the application element down' });
const Build = Schema.String.annotate({ description: 'CFBundleVersion of the instance read' });
const Instant = Schema.DateTimeUtcFromString;
const Point = Schema.Tuple([Schema.Finite, Schema.Finite]);
const Size = Point.annotate({ description: 'Width and height' });
const Frame = Schema.Tuple([Point, Size]).annotate({ description: 'Origin and size in screen points' });
const Value = Schema.Union([Schema.Boolean, Schema.Finite, Schema.String]);
const FactScalar = Schema.Union([
    Schema.TaggedStruct('bool', { value: Schema.Boolean }),
    Schema.TaggedStruct('integer', { value: Schema.Int }),
    Schema.TaggedStruct('real', { value: Schema.Finite }),
    Schema.TaggedStruct('text', { value: Schema.String }),
    Schema.TaggedStruct('date', { value: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('url', { value: Schema.String }),
    Schema.TaggedStruct('point', { value: Point }),
    Schema.TaggedStruct('size', { value: Size }),
    Schema.TaggedStruct('rect', { value: Frame }),
    Schema.TaggedStruct('color', { red: Schema.Finite, green: Schema.Finite, blue: Schema.Finite, alpha: Schema.Finite }).annotate({ description: 'CGColor in sRGB components' }),
    Schema.TaggedStruct('range', { location: Schema.Int, length: Schema.Int }),
    Schema.TaggedStruct('element', { id: Schema.Int.annotate({ description: 'Element of the same read' }) }),
    Schema.TaggedStruct('refused', { error: Schema.Int.annotate({ description: 'AXError code' }) }),
]);
const Fact: Schema.Codec<Fact> = Schema.Union([
    ...FactScalar.members,
    Schema.TaggedStruct('list', { values: Schema.Array(Schema.suspend((): Schema.Codec<Fact> => Fact)) }),
    Schema.TaggedStruct('record', {
        fields: Schema.Record(
            Schema.String,
            Schema.suspend((): Schema.Codec<Fact> => Fact),
        ),
    }),
]).annotate({ description: 'Accessibility attribute value by its Core Foundation type, refused with the AXError a read returned' });
const Kind = Schema.Literals(['action', 'toggle', 'choice', 'multi', 'row', 'number', 'measure', 'text', 'color', 'date', 'item', 'region']).annotate({ description: 'What a step on the control does, decided from its role and capability facts' });
const ContainerKind = Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'statusItem', 'menu', 'toolbar', 'web']).annotate({ description: 'Container kind, prompt for a window an application that is not regular declares AXModal, web for an AXWebArea' });
const TextRange = Schema.Struct({ location: Schema.Natural, length: Schema.Natural }).annotate({ description: 'CFRange of characters' });
const SearchKey = Schema.Literals([
    'AXAnyTypeSearchKey',
    'AXButtonSearchKey',
    'AXCheckBoxSearchKey',
    'AXControlSearchKey',
    'AXFrameSearchKey',
    'AXGraphicSearchKey',
    'AXHeadingSearchKey',
    'AXHeadingLevel1SearchKey',
    'AXHeadingLevel2SearchKey',
    'AXHeadingLevel3SearchKey',
    'AXHeadingLevel4SearchKey',
    'AXHeadingLevel5SearchKey',
    'AXHeadingLevel6SearchKey',
    'AXKeyboardFocusableSearchKey',
    'AXLandmarkSearchKey',
    'AXLinkSearchKey',
    'AXListSearchKey',
    'AXLiveRegionSearchKey',
    'AXMisspelledWordSearchKey',
    'AXOutlineSearchKey',
    'AXRadioGroupSearchKey',
    'AXStaticTextSearchKey',
    'AXTableSearchKey',
    'AXTextFieldSearchKey',
]).annotate({ description: 'AXSearchKey of an AXUIElementsForSearchPredicate search, the kind of element the application returns' });
const Label = Schema.Struct({
    text: Schema.String,
    attribute: Schema.Literals(['AXTitle', 'AXDescription', 'recognized', 'AXUserInputLabels', 'AXFilename', 'AXTitleUIElement', 'AXValue', 'AXHelp', 'AXRoleDescription']).annotate({
        description: 'Attribute the label was read from, AXHelp where siblings share a label, recognized where text recognition read it from the window image',
    }),
});
const HostLabel = Schema.Struct({ text: Schema.String }).annotate({ description: 'Label a host source states for its control' });
const Evidence = Schema.Literals(['attribute', 'marked']).annotate({ description: 'How the selection is known: an attribute states it, or exactly one sibling holds a read-only value' });
const KeyName = Schema.Literals([
    'return',
    'enter',
    'tab',
    'space',
    'delete',
    'forwardDelete',
    'escape',
    'clear',
    'home',
    'end',
    'pageUp',
    'pageDown',
    'left',
    'right',
    'down',
    'up',
    'help',
    'contextualMenu',
    'eisu',
    'kana',
    'f1',
    'f2',
    'f3',
    'f4',
    'f5',
    'f6',
    'f7',
    'f8',
    'f9',
    'f10',
    'f11',
    'f12',
    'f13',
    'f14',
    'f15',
    'f16',
    'f17',
    'f18',
    'f19',
]).annotate({ description: 'Special key by name, posted by its virtual key code: enter and clear on the keypad, eisu and kana the JIS keys' });
class KeyStep extends Schema.Opaque<KeyStep>()(
    Schema.Struct({
        key: Schema.Union([
            KeyName,
            Schema.String.check(Schema.makeFilter((text: string) => Iterator.from(GRAPHEMES.segment(text)).take(2).toArray().length === 1, { expected: 'one character or a special key name' })).annotate({ description: 'One character, posted with the key that types it on the current layout' }),
            Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 65_535 })).annotate({ description: 'Virtual key code, posted as is' }),
        ]),
        modifiers: Schema.Array(Schema.Literals(['command', 'shift', 'option', 'control', 'function']))
            .annotate({ description: 'Held while the key posts' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    }).annotate({ description: 'Key and the modifiers held while it posts, the shape of an act key step' }),
) {}
const Presentation = Schema.TaggedUnion({ behind: {}, forward: {}, restored: { back: Schema.Boolean.annotate({ description: 'The application in front before the step is in front again' }) } }).annotate({
    description: 'behind: opened behind the frontmost application, which kept key focus; forward: the target brought forward, as no route reached it behind, and left in front; restored: the target brought forward for the step, then the application in front before it put back, back false where putting it back failed',
});
const controlFacts = {
    subrole: Schema.String,
    label: Label,
    labels: Schema.Array(Schema.String).annotate({ description: 'AXUserInputLabels beyond the label: further names a person calls the control by' }),
    section: Schema.String.annotate({ description: 'Name of the AXSections section of its window that holds the control' }),
    help: Schema.String.annotate({ description: 'Help the element or its host states as written, its AXHelp or host hint: what a click or drag on it does, what each held modifier changes, and its tooltip' }),
    range: Schema.Struct({
        minimum: Schema.optionalKey(Schema.Finite),
        maximum: Schema.optionalKey(Schema.Finite),
        step: Schema.optionalKey(Schema.Finite),
        wraps: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXValueWraps: stepping past one end continues from the other' })),
    }),
    mark: Schema.String.annotate({ description: 'AXMenuItemMarkChar' }),
    shortcut: KeyStep.annotate({ description: 'Key step act posts to press the menu item or take the option, from its AXMenuItemCmd attributes or the shortcut set of its host' }),
    keyEquivalent: Schema.String.annotate({ description: 'Key equivalent a flyout item shows, as text recognition read it from the window image: raw text, not a key step act posts' }),
    separated: Schema.Boolean.annotate({ description: 'A separator precedes the item' }),
};
class Control extends Schema.Opaque<Control>()(
    Schema.Struct({
        ref: Ref,
        locator: Locator,
        kind: Kind,
        role: Schema.String.annotate({ description: 'AXRole' }),
        positional: Schema.Boolean.annotate({ description: 'Keyed by its offset from the nearest keyed sibling' }),
        ...Struct.map(controlFacts, Schema.optionalKey),
        label: Schema.optionalKey(Schema.Union([Label, HostLabel])),
        value: Schema.optionalKey(Fact.annotate({ description: 'AXValue, the AXURL file or link address of a row or menu item' })),
        enabled: Schema.optionalKey(Schema.Boolean),
        focused: Schema.optionalKey(Schema.Boolean),
        selected: Schema.optionalKey(Evidence),
        frame: Schema.optionalKey(Frame),
        options: Schema.optionalKey(Schema.Array(Schema.Struct({ label: Schema.String, ref: Ref, ...Struct.map(Struct.pick(controlFacts, ['help', 'shortcut']), Schema.optionalKey) })).annotate({ description: 'Options of a choice, absent where they list only while open' })),
        opens: Schema.optionalKey(Ref.annotate({ description: 'Menu the control opens, its level read through the menu scope while closed' })),
        alternate: Schema.optionalKey(Ref.annotate({ description: 'Item whose place this one takes while its modifier is held' })),
        placeholder: Schema.optionalKey(Schema.String),
        level: Schema.optionalKey(Schema.Int.annotate({ description: 'AXDisclosureLevel of a row' })),
        index: Schema.optionalKey(Schema.Natural.annotate({ description: 'AXIndex of a row, rows past it read through the rows scope' })),
        row: Schema.optionalKey(Ref.annotate({ description: 'Row whose cells hold the control' })),
        expanded: Schema.optionalKey(Schema.Boolean),
        actions: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Actions beyond the ones a step on its kind performs, by AXUIElementCopyActionDescription for custom actions, else the action name; a press step names one' })),
        alternateUIVisible: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXAlternateUIVisible: the hover controls AXShowAlternateUI reveals are shown' })),
        disclosedBy: Schema.optionalKey(Ref.annotate({ description: 'AXDisclosedByRow: outline row this row sits under' })),
        total: Schema.optionalKey(Schema.Natural.annotate({ description: 'Count of AXRows, or AXARIARowCount of a web grid, rows past those shown read through the rows scope' })),
        sort: Schema.optionalKey(Schema.Literals(['ascending', 'descending']).annotate({ description: 'AXSortDirection of a column header, which a press sorts by' })),
        cells: Schema.optionalKey(Schema.Array(Schema.Struct({ column: Schema.optionalKey(Schema.String.annotate({ description: 'Header of the AXColumnIndexRange column' })), text: Schema.String })).annotate({ description: 'Text of the cells of a row by column' })),
        column: Schema.optionalKey(Schema.String.annotate({ description: 'Header of the column whose cell holds the control' })),
        scroll: Schema.optionalKey(
            Schema.Struct({
                contentSize: Schema.optionalKey(Size.annotate({ description: 'AXContentSize of its scroll area' })),
                horizontalScrollBar: Schema.optionalKey(Schema.Finite.annotate({ description: 'AXValue of the horizontal scroll bar, 0 at the start, 1 at the end' })),
                verticalScrollBar: Schema.optionalKey(Schema.Finite.annotate({ description: 'AXValue of the vertical scroll bar, 0 at the top, 1 at the bottom' })),
            }).annotate({ description: 'Scroll area the control fills' }),
        ),
        selectedTextRanges: Schema.optionalKey(Schema.Array(TextRange).annotate({ description: 'AXSelectedTextRanges, else AXSelectedTextRange: the selection, the insertion point where its length is 0' })),
        numberOfCharacters: Schema.optionalKey(Schema.Natural.annotate({ description: 'AXNumberOfCharacters' })),
        insertionPointLineNumber: Schema.optionalKey(Schema.Natural.annotate({ description: 'AXInsertionPointLineNumber' })),
        runs: Schema.optionalKey(
            Schema.Array(
                Schema.Struct({
                    range: TextRange,
                    text: Schema.String,
                    attributes: Schema.Record(Schema.String, Fact).annotate({ description: 'AXAttributedStringForRange keys of the run: AXFont with AXFontName, AXFontFamily, AXFontSize, AXVisibleName; AXForegroundColor, AXUnderline, AXLink, AXMisspelled, AXNaturalLanguage and the rest the application states' }),
                    frame: Schema.optionalKey(Frame.annotate({ description: 'AXBoundsForRange' })),
                }),
            ).annotate({ description: 'Style runs (AXStyleRangeForIndex) over the visible characters, read through the text scope' }),
        ),
        invalid: Schema.optionalKey(Schema.Literals(['true', 'grammar', 'spelling']).annotate({ description: 'AXInvalid of a web field' })),
        required: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXRequired: the form needs a value' })),
        current: Schema.optionalKey(Schema.Literals(['true', 'page', 'step', 'location', 'date', 'time']).annotate({ description: 'AXARIACurrent: the item stands for the current one of its set' })),
    }),
) {}
const answering = {
    default: Schema.optionalKey(Ref.annotate({ description: 'AXDefaultButton of the container, the control Return presses' })),
    cancel: Schema.optionalKey(Ref.annotate({ description: 'AXCancelButton of the container, the control Escape presses' })),
    close: Schema.optionalKey(Ref.annotate({ description: 'AXCloseButton of the container' })),
};
class Container extends Schema.Opaque<Container>()(
    Schema.Struct({
        ref: Ref,
        locator: Locator,
        kind: ContainerKind,
        title: Schema.optionalKey(Schema.String),
        help: Schema.optionalKey(controlFacts.help),
        document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument URL' })),
        edited: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXEdited of its close button: the document holds unsaved changes' })),
        fullScreen: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXFullScreen: the window fills its own Space' })),
        loaded: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXLoaded of a web area' })),
        loadingProgress: Schema.optionalKey(Schema.Finite.annotate({ description: 'AXLoadingProgress of a web area, 0 to 1' })),
        window: Schema.optionalKey(WindowID),
        frame: Schema.optionalKey(Frame),
        layer: Schema.optionalKey(Schema.Int.annotate({ description: 'Window server layer' })),
        closed: Schema.Boolean.annotate({ description: 'A menu read with no frame on screen, which act presses the items of without opening it' }),
        modal: Schema.Boolean,
        asking: Schema.Boolean.annotate({ description: 'Requires an answer' }),
        focused: Schema.optionalKey(Schema.Boolean),
        parent: Schema.optionalKey(Ref.annotate({ description: 'Container this one sits in' })),
        ...answering,
        text: Schema.Array(Schema.String).annotate({ description: 'Static text that labels no control' }),
        truncated: Schema.Boolean.annotate({ description: 'The read stopped at its node or time bound, and the container scope reads it alone' }),
        unreadable: Schema.Boolean.annotate({ description: 'Accessibility states no content for the container while its window is on screen: its controls are the window area and the lines text recognition read from its image' }),
        controls: Schema.Array(Control),
    }),
) {}
const Cursor = Schema.TaggedUnion({
    system: { name: Schema.String.annotate({ description: 'AppKit NSCursor class property, else WindowServer system cursor, that alone draws the image shown' }) },
    custom: { digest: Schema.String.annotate({ description: 'Base64 SHA-256 of the image shown, stable per cursor' }), hotSpot: Point, size: Size },
}).annotate({ description: 'Cursor the system shows while the application is active and owns the window under the real pointer, named where one system cursor draws its image' });
const Pointer = Schema.Struct({
    tool: Schema.optionalKey(Schema.String.annotate({ description: 'Tool the pointer acts with, as the host source names it' })),
    shortcut: Schema.optionalKey(controlFacts.shortcut),
    help: Schema.optionalKey(controlFacts.help),
    holding: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Objects the pointer holds to place, as the host addresses them' })),
    operations: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Operations in progress, a modal tool state or dialog among them, in the host names, outermost first' })),
    cursor: Schema.optionalKey(Cursor),
    draggingFrame: Schema.optionalKey(Frame.annotate({ description: 'Frame of the image a drag session of the application shows, from its window at the dragging window level' })),
}).annotate({ description: 'Pointer of the application: tool with its shortcut and help, held objects, and operations a host source states, cursor and drag the system states' });
class Model extends Schema.Opaque<Model>()(
    Schema.Struct({
        sequence: Schema.Int.annotate({ description: 'Sequence of the last event the model reflects' }),
        containers: Schema.Array(Container),
        pointer: Schema.optionalKey(Pointer),
    }),
) {}
const Scope = Schema.TaggedUnion({
    application: {},
    container: { ref: Ref },
    menu: { ref: Ref.annotate({ description: 'Menu item or menu bar item whose menu level is read' }) },
    rows: { ref: Ref.annotate({ description: 'Multi control' }), from: Schema.Natural, count: Schema.Int.check(Schema.isGreaterThan(0)) },
    text: { ref: Ref.annotate({ description: 'Text control whose style runs are read' }) },
    search: {
        text: Schema.String.annotate({ description: 'Text the application matches against its elements, empty to match by kind alone' }),
        key: Schema.optionalKey(SearchKey),
        limit: Schema.Int.check(Schema.isGreaterThan(0)),
    },
    menus: {},
}).annotate({ description: 'Every window and the menu bar, one container, one menu level read on demand, rows of a multi, style runs of a text control, the containers of elements the application finds by text and kind (AXUIElementsForSearchPredicate), or the menu bar with every level read so far' });
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});
class Opener extends Schema.Opaque<Opener>()(Schema.Struct({ container: Locator, control: Locator })) {}
const Shape = Schema.Struct({
    ...Struct.pick(Control.fields, ['kind', 'role', 'positional']),
    ...Struct.map(controlFacts, Struct.lambda<OptionFromOptionalKeyLambda>(Schema.OptionFromOptionalKey)),
    options: Schema.String.pipe(Schema.Array, Schema.OptionFromOptionalKey),
    opens: Schema.OptionFromOptionalKey(Locator.annotate({ description: 'Menu the control opens' })),
    alternate: Schema.OptionFromOptionalKey(Locator.annotate({ description: 'Item whose place this one takes while its modifier is held' })),
    parent: Schema.OptionFromOptionalKey(Locator),
});
class Observation extends Schema.Opaque<Observation>()(
    Schema.Struct({
        build: Schema.OptionFromOptionalKey(Build.annotate({ description: 'Absent where the bundle states no CFBundleVersion' })),
        container: Locator,
        kind: ContainerKind,
        title: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Title of a container holding no AXDocument, as a document window titles the document' })),
        modal: Schema.Boolean,
        layer: Schema.OptionFromOptionalKey(Container.fields.layer.schema),
        size: Schema.OptionFromOptionalKey(Size),
        default: Schema.OptionFromOptionalKey(Locator),
        cancel: Schema.OptionFromOptionalKey(Locator),
        close: Schema.OptionFromOptionalKey(Locator),
        opened: Instant.annotate({ description: 'When the container last opened, the epoch of this read' }),
        at: Instant.annotate({ description: 'When this read was made' }),
        truncated: Schema.Boolean.annotate({ description: 'The read stopped at its node or time bound before every element of the container' }),
        opener: Schema.OptionFromOptionalKey(Opener.annotate({ description: 'Control whose step opened the container' })),
        documents: Schema.Array(Schema.String).annotate({ description: 'Documents open in the instance, each as its path and its percent-encoded file URL' }),
        conditions: Schema.Array(Condition),
        controls: Schema.Record(Locator, Shape).annotate({ description: 'Static facts of each control the container holds, rows excluded' }),
        layout: Schema.Record(Locator, Frame.annotate({ description: 'Origin in its container and size, in points, absent where the container states no frame' })),
        values: Schema.Record(Locator, Value),
    }).annotate({ description: 'One read of a container: its static facts by locator, their layout and values, and the conditions it was read under' }),
) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Build, Condition, Container, Control, Cursor, Fact, Frame, HostLabel, Instant, KeyStep, Kind, Locator, Model, Observation, Opener, Point, Pointer, Presentation, Ref, Scope, Shape, Value, WindowID };
