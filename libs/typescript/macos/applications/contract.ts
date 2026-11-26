import { Unmatched } from '@rasm/search/unmatched';
import { Array, DateTime, Duration, Effect, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = (typeof FactScalar)['Type'] | { readonly _tag: 'list'; readonly values: readonly Fact[] } | { readonly _tag: 'record'; readonly fields: Readonly<Record<string, Fact>> };
type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | TypedRecord | Specifier | typeof Descriptor.Type | { readonly _tag?: never; readonly [term: string]: AppleEventValue };
interface OptionFromOptionalKeyLambda extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPLY_DEADLINE = 120_000;
const PLANE = 2_147_483_647;

// --- [MODELS] --------------------------------------------------------------------------

const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String.check(Schema.makeFilter((key: string) => !Object.hasOwn(fields, key))), Schema.Json)]);
const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
class TypedRecord extends Schema.Opaque<TypedRecord>()(Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' })) {}
class Specifier extends Schema.Opaque<Specifier>()(
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class, derived as prop for property form' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Nested.annotate({ description: 'Selection data, first, middle, last, any, or every for index form, or next or previous for relative form' }),
        from: Schema.optionalKey(Nested.annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
) {}
const Descriptor = Schema.Union([
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('opaque', { type: Code, data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({ description: 'Descriptor no other case decodes, sent back unchanged' }),
]);
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or enumerator, class, date, or file URL per parameter type' }),
    Schema.Array(Nested),
    TypedRecord,
    Specifier,
    Descriptor,
    Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)]).annotate({ description: 'Plain record by property name or code' }),
]);
const DictionaryKind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Term = Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as scripting definition spells it' }) });
const Command = Term.mapFields(
    Struct.assign({
        parameters: Schema.Array(
            Term.mapFields(
                Struct.assign({
                    types: Schema.Array(Schema.String).annotate({ description: 'Types the scripting definition declares for it' }),
                    list: Schema.Boolean.annotate({ description: 'Takes a list' }),
                    enumerators: Schema.Array(Term).annotate({ description: 'Enumerators of the enumeration typing it' }),
                }),
            ),
        ),
    }),
);
class ScriptingDictionary extends Schema.Class<ScriptingDictionary, { readonly brand: unique symbol }>('ScriptingDictionary')(
    {
        ...Struct.Record(DictionaryKind.literals, Schema.Array(Term)),
        command: Schema.Array(Command),
        script: Schema.OptionFromOptionalKey(Command.annotate({ description: 'Command a direct text parameter runs as source: the one taking a list of text arguments, else the do script event' })),
        open: Schema.OptionFromOptionalKey(Command.annotate({ description: 'Command of the open documents event, its parameters the names launch takes in opening' })),
    },
    { description: 'Every declaration by kind, name, and code, commands with their parameters, and the script and open commands' },
) {
    static readonly Kind = DictionaryKind;
}
const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side of an image in pixels, never upscaled' });
const Ref = Schema.Natural.annotate({ description: 'Interned reference to a container, control, or option, shared by every handle of the server and kept while the server runs; a container shown again keeps its references' });
const Locator = Schema.String.annotate({ description: 'Canonical encoding of a locator, its steps from the application element down' });
const Point = Schema.Tuple([Schema.Finite, Schema.Finite]);
const Size = Schema.Tuple([Schema.Finite, Schema.Finite]);
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
]).annotate({
    description: 'Accessibility attribute value by its Core Foundation type, refused with the AXError a read returned',
});
const Kind = Schema.Literals(['action', 'toggle', 'choice', 'multi', 'row', 'number', 'measure', 'text', 'color', 'date', 'item', 'region']).annotate({ description: 'What a step on the control does, decided from its role and capability facts' });
const ContainerKind = Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'statusItem', 'menu', 'toolbar', 'web']).annotate({ description: 'Container kind, prompt for a dialog of an application that is not regular, web for an AXWebArea' });
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
]).annotate({ description: 'AXSearchKey of an AXUIElementsForSearchPredicate search, the kind of element the engine returns' });
const Label = Schema.Struct({
    text: Schema.String,
    attribute: Schema.Literals(['AXTitle', 'AXDescription', 'recognized', 'AXUserInputLabels', 'AXFilename', 'AXTitleUIElement', 'AXValue', 'AXHelp', 'AXRoleDescription']).annotate({
        description: 'Attribute the label was read from, AXHelp where siblings share a label, recognized where text recognition read it from the window image',
    }),
});
const HostLabel = Schema.Struct({ text: Schema.String }).annotate({ description: 'Label a host source states for its control' });
const Evidence = Schema.Literals(['attribute', 'marked']).annotate({ description: 'How the selection is known: an attribute states it, or exactly one sibling carries a read-only value' });
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
const Modifier = Schema.Literals(['command', 'shift', 'option', 'control', 'function']);
class KeyStep extends Schema.Opaque<KeyStep>()(
    Schema.Struct({
        key: Schema.Union([KeyName, Schema.String.annotate({ description: 'One character, posted with the key that types it on the current layout' }), Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 65_535 })).annotate({ description: 'Virtual key code, posted as is' })]),
        modifiers: Schema.Array(Modifier)
            .annotate({ description: 'Held while the key posts' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    }).annotate({ description: 'Key and the modifiers held while it posts, the shape of an act key step' }),
) {}
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
    keyEquivalent: Schema.String.annotate({ description: 'Key equivalent a flyout item shows, as text recognition read it from the window image; raw text, not a key step act posts' }),
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
        actions: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Actions beyond press and stepping, by AXUIElementCopyActionDescription for custom actions, else the action name; a press step names one' })),
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
        fullScreen: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXFullScreen: the window fills its own space' })),
        loaded: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXLoaded of a web area' })),
        loadingProgress: Schema.optionalKey(Schema.Finite.annotate({ description: 'AXLoadingProgress of a web area, 0 to 1' })),
        window: Schema.optionalKey(WindowID),
        frame: Schema.optionalKey(Frame),
        layer: Schema.optionalKey(Schema.Int.annotate({ description: 'Window server layer' })),
        modal: Schema.Boolean,
        asking: Schema.Boolean.annotate({ description: 'Requires an answer' }),
        focused: Schema.optionalKey(Schema.Boolean),
        parent: Schema.optionalKey(Ref.annotate({ description: 'Container this one sits in' })),
        ...answering,
        text: Schema.Array(Schema.String).annotate({ description: 'Static text that labels no control' }),
        truncated: Schema.Boolean.annotate({ description: 'The read stopped at its node or time bound, model of the container reads it alone' }),
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
    holding: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Objects the pointer carries to place, as the host addresses them' })),
    operations: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Operations in progress, a modal tool state or dialog among them, in the host names, outermost first' })),
    cursor: Schema.optionalKey(Cursor),
    draggingFrame: Schema.optionalKey(Frame.annotate({ description: 'Frame of the image a drag session of the application shows, from its window at the dragging window level' })),
}).annotate({ description: 'Pointer of the application: tool with its shortcut and help, carried objects, and operations a host source states, cursor and drag the system states' });
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
const Build = Schema.String.annotate({ description: 'CFBundleVersion of the instance read' });
const Instant = Schema.DateTimeUtcFromString;
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});
class Opener extends Schema.Opaque<Opener>()(Schema.Struct({ container: Locator, control: Locator })) {}
const Shape = Schema.Struct({
    ...Struct.pick(Control.fields, ['kind', 'role', 'positional']),
    ...Struct.map(controlFacts, Struct.lambda<OptionFromOptionalKeyLambda>(Schema.OptionFromOptionalKey)),
    options: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
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
const Window = Schema.Struct({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Container.fields.layer.schema,
    band: Schema.Literals(['normal', 'floating', 'modalPanel', 'utility', 'dock', 'mainMenu', 'status', 'popUpMenu', 'overlay', 'help', 'dragging', 'screenSaver', 'assistiveTechHigh', 'cursor']).annotate({
        description: 'CGWindowLevelKey whose level is the highest at or below the layer',
    }),
    parent: Schema.optionalKey(WindowID.annotate({ description: 'Window this child window is attached to, from the window server' })),
    spaces: Schema.Array(Schema.Int).annotate({ description: 'Spaces the window is on, empty while never ordered in' }),
    onScreen: Schema.Boolean,
    hidden: Schema.Boolean.annotate({ description: 'Ordered out while on a current Space: its application hidden, or the window ordered out or minimized' }),
    shared: Schema.Boolean.annotate({ description: 'kCGWindowSharingState is not kCGWindowSharingNone: false where the window refuses capture' }),
    remote: Schema.optionalKey(WindowID.annotate({ description: 'Window of a view service the application is responsible for, at the same frame and title, whose content this window shows, as a file panel' })),
    container: Schema.optionalKey(Ref.annotate({ description: 'Container the window shows' })),
    kind: Schema.optionalKey(ContainerKind),
    document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument URL' })),
    asking: Schema.Boolean.annotate({ description: 'Requires an answer, or blocks its application at modal layer' }),
    presented: Schema.Boolean.annotate({ description: 'A container describes it, or it is on screen' }),
}).annotate({ description: 'Window server row, asking rows first' });
const ProcessStatus = Schema.Literals(['running', 'runnable', 'throttled', 'uninterruptible', 'sleeping', 'stopped', 'traced']).annotate({
    description:
        'Main thread state: running after time on a processor, runnable while waiting for one, throttled or suspended by the kernel under resource starvation, uninterruptible while waiting in the kernel on a page-in or disk read, sleeping while waiting on an event, stopped by a signal or suspension, traced while a debugger holds it stopped',
});
const Status = Schema.TaggedUnion({ launching: {}, responding: {}, unresponsive: { thread: ProcessStatus } }).annotate({ description: 'Launching, responding to Accessibility reads, or unresponsive with the state of its main thread' });
const Usage = Schema.Struct({
    cpu: Schema.Finite.annotate({ description: 'Processor seconds' }),
    runnable: Schema.Finite.annotate({ description: 'Seconds ready to run while waiting for a processor' }),
    pageWait: Schema.Finite.annotate({ description: 'Seconds waiting on page faults' }),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    footprint: Schema.Finite.annotate({ description: 'Physical footprint bytes' }),
    threads: Schema.Int,
    runningThreads: Schema.Int.annotate({ description: 'Threads running or waiting for a processor at the reading, none while every thread waits on an event or lock' }),
    assertions: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Name of each power assertion the process holds, the long work it declares' })),
    assertionsFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Power assertion read failure' })),
}).annotate({ description: 'proc_pid_rusage counters since launch, with the footprint, threads, and power assertions at the reading' });
const UsageInterval = Schema.Struct({
    cause: Schema.Literals(['running', 'runnable', 'paging', 'reading', 'writing', 'idle']).annotate({
        description: 'The processor, runnable, or page wait time covering half of the interval, else disk reads or writes, else idle',
    }),
    duration: Schema.Finite.annotate({ description: 'Seconds between the two readings' }),
    ...Struct.pick(Usage.fields, ['cpu', 'runnable', 'pageWait', 'diskBytesRead', 'diskBytesWritten']),
    end: Usage.annotate({ description: 'Reading at the end of the interval' }),
}).annotate({ description: 'Growth of proc_pid_rusage counters between two readings, with its cause' });
const Load = Schema.Struct({
    idle: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })).annotate({ description: 'Share of processor ticks idle, absent where no tick passed' })),
    pageins: Schema.optionalKey(Schema.Finite.annotate({ description: 'Pages read in from disk, absent where host_statistics64 served the earlier reading again, as it rate-limits callers outside the system' })),
    swapins: Schema.optionalKey(Schema.Finite.annotate({ description: 'Pages swapped in from compressor segments, absent where host_statistics64 served the earlier reading again, as it rate-limits callers outside the system' })),
    swapouts: Schema.optionalKey(Schema.Finite.annotate({ description: 'Pages swapped out to compressor segments, absent where host_statistics64 served the earlier reading again, as it rate-limits callers outside the system' })),
    decompressions: Schema.optionalKey(Schema.Finite.annotate({ description: 'Pages decompressed, absent where host_statistics64 served the earlier reading again, as it rate-limits callers outside the system' })),
    deviceUtilization: Schema.optionalKey(Schema.Int.annotate({ description: 'GPU Device Utilization %, the busiest accelerator, absent where the driver states none' })),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    diskTime: Schema.Finite.annotate({ description: 'Seconds block storage drivers spent reading and writing' }),
    availableCapacityForImportantUsage: Schema.optionalKey(Schema.Finite.annotate({ description: 'Bytes the startup volume holds for important work' })),
}).annotate({ description: 'host_processor_info processor ticks, host_statistics64 paging counters, block storage statistics, and GPU utilization' });
class Running extends Schema.Opaque<Running>()(
    Schema.Struct({
        active: Schema.Boolean,
        status: Status,
        pendingEventAge: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds the oldest event the window server holds for the instance has waited unread, present while the window server holds it unresponsive' })),
        usage: Schema.optionalKey(UsageInterval.annotate({ description: 'Growth of the instance usage since it turned unresponsive, with its cause: computing, waiting for a processor, paging, reading, or writing' })),
        usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
        windows: Schema.Array(Window),
        statusItems: Schema.optionalKey(Schema.Natural.annotate({ description: 'Status items the application shows in the menu bar' })),
        statusItemsFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Status item read failure' })),
    }),
) {}
const failure = { reason: Schema.String.annotate({ description: 'Failure case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) };
const Failed = open(failure);
const failed = open({ _tag: Schema.tag('failed'), ...failure });
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u))
    .pipe(Schema.brand('ProcessInstance'))
    .annotate({ description: 'Process ID and kernel start time as <pid>@<start>, unequal for a later process reusing the ID' });
const DiagnosticReport = Schema.Struct({
    url: Schema.URLFromString.annotate({ description: 'Report file in ~/Library/Logs/DiagnosticReports or /Library/Logs/DiagnosticReports' }),
    kind: Schema.Literals(['crash', 'hang', 'diag', 'spin']).annotate({ description: 'crash for an .ips report of bug_type 309, else the spindump text report extension' }),
    date: Schema.String.annotate({ description: 'ISO 8601 capture time: captureTime of a crash report, Date/Time of a text report' }),
    pid: Pid,
    launched: Schema.optionalKey(Schema.String.annotate({ description: 'ISO 8601 launch time to the millisecond, procLaunch of a crash report' })),
    name: Schema.optionalKey(Schema.String.annotate({ description: 'Process name' })),
    executable: Schema.optionalKey(Schema.String.annotate({ description: 'Executable path, user components redacted to * under /Users, /Volumes, and /private/tmp' })),
    coalition: Schema.optionalKey(Schema.String.annotate({ description: 'Resource coalition name of a crash report, the responsible app or job grouping the process' })),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    event: Schema.optionalKey(Schema.String.annotate({ description: 'Text report Event line: hang, cpu usage, disk writes, wakeups' })),
    exception: Schema.optionalKey(
        Schema.Struct({
            type: Schema.String,
            signal: Schema.optionalKey(Schema.String),
            subtype: Schema.optionalKey(Schema.String),
            message: Schema.optionalKey(Schema.String),
            rawCodes: Schema.optionalKey(Schema.Array(Schema.Finite).annotate({ description: 'Mach exception codes: the kern_return_t, then the faulting address for EXC_BAD_ACCESS' })),
        }).annotate({ description: 'Mach exception of a crash report: type (EXC_BAD_ACCESS), signal, subtype with the faulting address, message, codes' }),
    ),
    termination: Schema.optionalKey(
        Schema.Struct({
            namespace: Schema.String,
            code: Schema.optionalKey(Schema.Finite),
            indicator: Schema.optionalKey(Schema.String),
            byProc: Schema.optionalKey(Schema.String.annotate({ description: 'Process that terminated it' })),
            reasons: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'dyld or code signing lines: library not loaded, symbol not found, launch constraint violation' })),
            details: Schema.optionalKey(Schema.Array(Schema.String)),
        }).annotate({ description: 'Kernel termination reason, CODESIGNING with Launch Constraint Violation for a refused launch, DYLD for a failed load' }),
    ),
    applicationSpecificInformation: Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Application Specific Information of a crash report (asi) by image: abort() called, a malloc corruption, a fork-safety violation' })),
    virtualMemoryRegionInfo: Schema.optionalKey(Schema.String.annotate({ description: 'Memory regions around the faulting address of a crash report (vmRegionInfo)' })),
    faultingThread: Schema.optionalKey(
        Schema.Array(Schema.Struct({ image: Schema.optionalKey(Schema.String), imageOffset: Schema.Finite, symbol: Schema.optionalKey(Schema.String), symbolLocation: Schema.optionalKey(Schema.Finite) })).annotate({
            description: 'Frames of the thread that triggered the crash, innermost first, each in its image with its symbol where the report resolved one',
        }),
    ),
});
const LogEntry = Schema.Struct({
    timestamp: Schema.String.annotate({ description: 'ISO 8601 time the event was logged' }),
    messageType: Schema.Literals(['Debug', 'Info', 'Default', 'Error', 'Fault']).annotate({ description: 'Log level as log names it, Default for notice' }),
    processID: Pid,
    processImagePath: Schema.String.annotate({ description: 'Executable of the logging process' }),
    senderImagePath: Schema.String.annotate({ description: 'Library or executable whose code logged the event' }),
    subsystem: Schema.String,
    category: Schema.String,
    eventMessage: Schema.String.annotate({ description: 'Message with its arguments composed' }),
}).annotate({ description: 'Unified log event as log show and log stream write it' });
const reported = {
    report: Schema.optionalKey(DiagnosticReport),
    reportFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report read failure case: timedOut after 60 seconds, cancelled, corrupt for a report outside its documented format, or foundation or system for a failed read' })),
};
const ended = {
    detail: Schema.optionalKey(Schema.Array(Schema.Literals(['memory', 'codeSigning', 'decryption'])).annotate({ description: 'Kernel exit detail: memory limit, invalid code signature, failed decryption' })),
    reporting: Schema.optionalKey(Schema.Boolean.annotate({ description: 'A crash report is on its way' })),
    ...reported,
};
const Exited = Schema.TaggedUnion({
    status: { status: Schema.Int.annotate({ description: 'Exit status' }), ...ended },
    signal: { signal: Schema.Int.annotate({ description: 'Terminating signal number' }), ...ended },
    statusFailure: { statusFailure: Schema.String.annotate({ description: 'Exit status read failure' }), ...reported },
}).annotate({ description: 'Exit status or terminating signal with the crash report of the exit, else why the status went unread' });
const InstanceState = Object.assign(
    Schema.Union([Schema.TaggedStruct('running', Running.fields), failed, Schema.TaggedStruct('exited', { termination: Exited })])
        .annotate({ description: 'Running with every window, failed read, or exited with its cause' })
        .pipe(Schema.toTaggedUnion('_tag')),
    { Exited, Status },
);
const Written = Schema.Result(Schema.Array(Schema.URL), Schema.ErrorInstance()).annotate({ description: 'Documents the instance holds open, by AXDocument and the open command, that FSEvents saw written during the step, else the watch failure' });
class Diagnostics extends Schema.Opaque<Diagnostics>()(
    Schema.Struct({
        application: Schema.optionalKey(Schema.Struct({ url: Schema.URL.annotate({ description: 'Bundle of the installation' }), instance: Schema.optionalKey(ProcessInstance) }).annotate({ description: 'Reports and unified log of the instance, else of the installation executable; absent reads the server itself' })),
        folders: Schema.Array(Schema.Struct({ url: Schema.URL.annotate({ description: 'Folder of log files' }), glob: Schema.optionalKey(Schema.String.annotate({ description: 'fnmatch pattern of file names, every file when absent' })) })).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        since: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds back from now, else since the instance launched' })),
        limit: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Most reports, log lines, and unified log events returned' }),
        find: Schema.optionalKey(Schema.String.annotate({ description: 'Text a log line or unified log event holds, ignoring case, searched before the limit applies' })),
    }),
) {
    static readonly Result = Schema.Struct({
        reports: Schema.Array(DiagnosticReport).annotate({ description: 'Newest first' }),
        samples: Schema.Array(Schema.URLFromString).annotate({ description: 'sample reports of the instance taken as it turned unresponsive, newest first' }),
        logs: Schema.Array(Schema.Struct({ file: Schema.String.annotate({ description: 'File URL of the log file' }), text: Schema.String })).annotate({ description: 'Newest first across folders, by the JSON timestamp of a line, else file modification time' }),
        unified: Schema.Result(Schema.Array(LogEntry).annotate({ description: 'log show events, newest first' }), Schema.ErrorInstance()),
        unreadable: Schema.Array(Schema.String).annotate({ description: 'Report files, log folders, and log lines that failed to read, with cause' }),
    });
}
const Route = Schema.TaggedUnion({
    source: {
        member: Schema.String.annotate({ description: 'Host member that drew the step on its canvas, with the tool it drew with' }),
        approximated: Schema.Array(Schema.String).annotate({ description: 'What of the step the host member does not carry, such as pressure drawn as a taper, tilt, timing, or modifiers' }),
    },
    process: {},
    window: { number: WindowID },
    focused: { number: WindowID },
    system: {},
}).annotate({
    description:
        'Route the step took: source a host member drew it on the canvas of a linked source with no event, pointer, or activation, process posts keys to the key window, window adds the routing fields of a background window, focused brackets the window route with focus and defocus records, system posts at the HID tap to the frontmost process',
});
const Presentation = Schema.TaggedUnion({ behind: {}, forward: {} }).annotate({
    description: 'behind: menus opened behind the frontmost application, its key focus never leaving it; forward: the application brought forward to open its menu, as no route opened it behind, and left there',
});
const Rung = Schema.Literals(['source', 'accessibility', 'process', 'window', 'focused', 'system']).annotate({
    description: 'Rung of the act ladder in climbing order: the host drawing of a linked source on its canvas, an Accessibility call, then events by each route, system last as it brings the instance forward',
});
const Delivery = Schema.Struct({
    proof: Schema.Literals(['answer', 'change', 'value', 'posting', 'unanswered', 'exit']).annotate({
        description:
            'What proved the step: answer an Accessibility call answering success, change an event of the instance after the posted events, value the attribute read back, posting events posted with nothing to read back, unanswered no answer within the messaging timeout so the step may have run, exit the instance exited during the step, which ends the run',
    }),
    window: Schema.optionalKey(WindowID.annotate({ description: 'Window the step reached' })),
    route: Schema.optionalKey(Route.annotate({ description: 'Route its events took, absent where an Accessibility call alone proved the step' })),
    tried: Schema.Array(Schema.Struct({ rung: Rung, failure: Schema.String })).annotate({ description: 'Rungs below the proving one, each with why its read-back failed' }),
    presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the step presented the menu it opened or the instance it reached on the system rung, absent where it stayed behind with no menu' })),
    pointer: Schema.optionalKey(Point.annotate({ description: 'Screen point the events of the step left the posted pointer at' })),
    popUps: Schema.Array(Schema.Struct({ window: WindowID, closed: Schema.Boolean })).annotate({
        description: 'Windows at the pop-up menu level or above the step opened behind the frontmost application, each closed before act returned and read back gone, else left open; a call whose last step is inside one leaves it open',
    }),
});
const Milliseconds = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: REPLY_DEADLINE })).annotate({ description: 'Milliseconds up to the 120 s reply deadline' });
const PenPressure = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })).annotate({ description: 'Pressure' });
const Tilt = Schema.Finite.check(Schema.isBetween({ minimum: -1, maximum: 1 }));
const Sample = Schema.Union([Point, Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure]), Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure, Tilt.annotate({ description: 'Tilt x' }), Tilt.annotate({ description: 'Tilt y' })])]).annotate({
    description: 'Point with the pressure from 0 to 1 and tilt from -1 to 1 of a pen stroke',
});
const paced = Schema.makeFilter(
    ({ hold = 0, every, jitter = 0, count = 1 }: { readonly hold?: number; readonly every?: number; readonly jitter?: number; readonly count?: number }) => (every ?? hold + jitter) >= hold + jitter && (every !== undefined || jitter === 0) && (count - 1) * (every ?? 0) + hold <= REPLY_DEADLINE,
    { expected: 'every at least the hold plus jitter, jitter only with every, and every press inside the 120 s reply deadline' },
);
const pointed = Schema.makeFilter(
    ({ pointer, path = [], button, by, count = 1 }: { readonly pointer?: unknown; readonly path?: readonly unknown[]; readonly button?: string; readonly by?: readonly [number, number]; readonly count?: number }) => {
        const points = [pointer, ...path].filter((position): position is readonly number[] => globalThis.Array.isArray(position));
        const pens = new Set(points.map((point) => point.length > 2));
        const planar = points.every(([x = 0, y = 0]) => Math.abs(x) <= PLANE && Math.abs(y) <= PLANE);
        const reach = by === undefined ? 1 : Math.round(Math.max(Math.abs(by[0]), Math.abs(by[1])));
        return planar && reach >= 1 && reach < PLANE && (path.length === 0 || (by === undefined && count === 1)) && (!pens.has(true) || (points.length === path.length + 1 && !pens.has(false) && by === undefined && (button ?? 'left') === 'left'));
    },
    { expected: 'points and a by of at least one whole unit inside ±2³¹, a path only on one press with no by, and pressure or tilt on every point of a left-button stroke or on none' },
);
const timing = {
    hold: Schema.optionalKey(Milliseconds.annotate({ description: 'Held between down and up, a drag or scroll spread across it, a key repeating past the key repeat delay' })),
    count: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: REPLY_DEADLINE })).annotate({ description: 'Presses, the click state counting up where they fall inside the double-click interval' })),
    every: Schema.optionalKey(Milliseconds.annotate({ description: 'Start to start of presses, when absent the hold with the double-click interval for a pointer or the key repeat interval for a key' })),
    jitter: Schema.optionalKey(Milliseconds.annotate({ description: 'Random spread of each press start around every' })),
    edge: Schema.optionalKey(Schema.Literals(['down', 'up']).annotate({ description: 'The one edge posted, a later step posting the other: a press held across steps' })),
};
const input = <Position extends Schema.Top>(position: Position) =>
    Schema.Struct({
        pointer: position.annotate({ description: 'Element pressed at its activation point or center, or a point' }),
        path: Schema.optionalKey(Schema.Array(position).annotate({ description: 'Points the pointer moves through after the first: a drag, a stroke, or a hover path' })),
        button: Schema.optionalKey(Schema.Literals(['left', 'right', 'middle', 'none']).annotate({ description: 'left when absent, none a hover' })),
        by: Schema.optionalKey(Point.annotate({ description: 'Scroll by horizontal and vertical distance, at least one whole unit' })),
        unit: Schema.optionalKey(Schema.Literals(['pixel', 'line']).annotate({ description: 'Unit of by, pixel when absent' })),
        modifiers: KeyStep.fields.modifiers,
        ...timing,
    })
        .check(paced, pointed)
        .annotate({ description: 'Pointer step: a press, drag, stroke, hover, or scroll' });
const steps = <Target extends Schema.Top, Fields extends Schema.Struct.Fields, Pointing extends Schema.Struct.Fields>(target: Target, fields: Fields, pointing: Pointing) =>
    Schema.NonEmptyArray(
        Schema.Union([
            Schema.Struct({
                at: target,
                action: Schema.optionalKey(
                    Schema.String.check(Schema.makeFilter((named: string) => named !== 'AXRaise', { expected: 'an action other than AXRaise, which takes focus from the frontmost application' })).annotate({
                        description: 'Element action by name, AXPress where no to or range is given',
                    }),
                ),
                to: Schema.optionalKey(
                    Schema.Union([Value, Frame]).annotate({
                        description: 'Value the control takes by its kind: a toggle state, number, ISO 8601 date, text, option label of a choice, or frame of a window, read back, each refused rung giving way to the next',
                    }),
                ),
                range: Schema.optionalKey(Schema.Union([Schema.Tuple([Schema.Natural, Schema.Natural]), Schema.String]).annotate({ description: 'Text selected, by location and length or by the text it holds, replaced by to where given' })),
                ...fields,
            }),
            Schema.Struct({ ...KeyStep.fields, at: Schema.optionalKey(target.annotate({ description: 'Element the key goes to, focused through Accessibility or a click, else the key window' })), ...timing, ...fields }).check(paced),
            input(Schema.Union([target, Sample])).mapFields((held) => ({ ...held, ...pointing, ...fields }), { unsafePreserveChecks: true }),
        ]),
    ).annotate({ description: 'Run in order until one fails, every step refused before any runs where one is malformed' });
class Capture extends Schema.Opaque<Capture>()(
    Schema.Struct({
        source: Schema.TaggedUnion({
            windows: { numbers: Schema.NonEmptyArray(WindowID).annotate({ description: 'Each window an image of its own with its child windows' }) },
            element: { ref: Ref.annotate({ description: 'Control or container cropped from its window, with the windows of the application above it' }) },
            region: { frame: Frame.annotate({ description: 'Screen region the windows of the application show' }) },
        }),
        longestSide: Schema.OptionFromOptionalKey(LongestSide.annotate({ description: 'Longest side in pixels of each image, never upscaled, full resolution when absent' })),
    }),
) {
    static readonly Screenshot = Schema.Struct({
        png: Schema.Uint8Array.annotate({ description: 'PNG image' }),
        width: Schema.Int.annotate({ description: 'Image width in pixels' }),
        height: Schema.Int.annotate({ description: 'Image height in pixels' }),
        frame: Frame.annotate({ description: 'Screen area the image shows' }),
        windows: Schema.Array(WindowID).annotate({ description: 'Windows the image composites' }),
        backingStore: Schema.Boolean.annotate({
            description: 'Whether the image is the backing store the window server keeps for a window off screen (hidden, minimized, or on another Space), its last drawn content read through CGSHWCaptureWindowList in place of ScreenCaptureKit',
        }),
        document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument file URL of the first window showing one' })),
        presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the menus the image shows were opened for it, absent when it opened none' })),
    });
    static readonly Result = Schema.Array(Schema.Result(Capture.Screenshot, Schema.ErrorInstance())).annotate({ description: 'One image per window of a windows source in its order, else one' });
}
class Recording extends Schema.Opaque<Recording>()(
    Schema.Struct({
        ...Capture.fields,
        frames: Schema.Int.check(Schema.isGreaterThanOrEqualTo(2)).annotate({ description: 'Most key frames kept: the first state and each state the source settled in for a beat, the smallest change merged into the next where more settle' }),
    }),
) {
    static readonly Marker = Schema.Union([Schema.TaggedStruct('step', { index: Schema.Int.annotate({ description: 'Step of the act call, counted from 0' }), at: Instant.annotate({ description: 'When the step began' }) }), Schema.suspend(() => Schema.TaggedStruct('event', Event.fields))]).annotate({
        description: 'Step begun or instance event read after a key frame appeared and before the next',
    });
    static readonly Result = Schema.Struct({
        keyFrames: Schema.Array(
            Schema.Struct({
                at: Instant.annotate({ description: 'When the source first showed the state, on the clock of event and read times' }),
                changed: Schema.optionalKey(Frame.annotate({ description: 'Screen area that differs from the key frame before, absent on the first' })),
                screenshot: Capture.Screenshot.annotate({ description: 'The whole source on the first and last key frames, the changed area alone on those between' }),
                markers: Schema.Array(Recording.Marker).annotate({ description: 'Steps begun and instance events read from when this state appeared until the next did, in order' }),
            }),
        ).annotate({
            description: 'The state when recording began, then each state the source settled in, the windows the application opened meanwhile included; a state the source left and returned to with no step or event between is dropped, and a frame showing what the one before shows is merged into it',
        }),
        presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the menus the frames show were opened for them, absent when they opened none' })),
    });
    static readonly leads = ({ keyFrames }: typeof Recording.Result.Type) =>
        Array.match(keyFrames, {
            onEmpty: Array.empty,
            onNonEmpty: (frames) => {
                const first = Array.headNonEmpty(frames);
                const [[x, y], [across, down]] = first.screenshot.frame;
                const begun = frames.map(({ markers }) => markers.flatMap((marker) => (marker._tag === 'step' ? [marker.index + 1] : [])));
                return frames.map(({ at, changed, screenshot: { width, height } }, index) => [
                    `+${Duration.format(DateTime.distance(first.at, at))}`,
                    ...Array.match(begun[index - 1] ?? [], { onEmpty: Array.empty, onNonEmpty: (listed) => [`after step ${listed.join(', ')}`] }),
                    ...Array.fromNullishOr(changed).map(([[left, top], [wide, high]]) => `changed [${[((left - x) * first.screenshot.width) / across, ((top - y) * first.screenshot.height) / down, (wide * first.screenshot.width) / across, (high * first.screenshot.height) / down].map(Math.round).join(', ')}]`),
                    ...(width === first.screenshot.width && height === first.screenshot.height ? [] : ['cropped to the change']),
                    ...Array.match(index === frames.length - 1 ? (begun[index] ?? []) : [], { onEmpty: Array.empty, onNonEmpty: (listed) => [`then step ${listed.join(', ')} changing nothing shown`] }),
                ]);
            },
        });
}
const Act = {
    Rung,
    Route,
    Input: input(Sample),
    steps,
    Result: Schema.Struct({
        outcomes: Schema.Array(Schema.Result(Delivery, Schema.ErrorInstance())).annotate({ description: 'One per step run, the first failure ending the run' }),
        state: InstanceState.annotate({ description: 'State once the instance answered: responding, unresponsive with its main thread state (stopped, traced, or leaving an Accessibility read unanswered within the messaging timeout), or quiet for one beat' }),
        usage: Schema.optionalKey(Schema.Result(UsageInterval, Schema.ErrorInstance()).annotate({ description: 'Growth of the instance usage over the steps and its cause, why a busy instance has not answered yet, else the read failure; absent for steps a linked source runs' })),
        load: Schema.optionalKey(Schema.Result(Load, Schema.ErrorInstance()).annotate({ description: 'System load over the steps, else the read failure; absent for steps a linked source runs' })),
        written: Schema.optionalKey(Written.annotate({ description: 'Documents the instance holds open, by AXDocument and the open command, that FSEvents saw written during the steps, else the watch failure; absent for steps a linked source runs' })),
        recording: Schema.optionalKey(Schema.Result(Recording.Result, Schema.ErrorInstance()).annotate({ description: 'Key frames of the recorded source from before the first step until the instance answered, else why recording failed while the steps ran; absent when not asked' })),
    }),
};
const QuitMode = Schema.Literals(['asking', 'saving', 'discarding', 'forcing']).annotate({ description: 'Save option the quit event states where the scripting definition declares one on quit, forcing ending the process with no event' });
class Session extends Schema.Opaque<Session>()(
    Schema.Struct({
        onConsole: Schema.Boolean.annotate({ description: 'kCGSessionOnConsoleKey, false while another user session holds the screen' }),
        locked: Schema.Boolean.annotate({ description: 'The lock screen shows' }),
        asleep: Schema.Boolean.annotate({ description: 'The main display sleeps' }),
        secureInput: Schema.Boolean.annotate({ description: 'A process holds secure event input' }),
        spaces: Schema.Array(Schema.Int).annotate({ description: 'Current Space of every display, as SLSCopySpaces reads it for the spaces of a window, each change read from activeSpaceDidChange' }),
    }).annotate({ description: 'Login session and screens; view and act refuse a session off the console, a locked screen, or a sleeping display' }),
) {}
class Pressure extends Schema.Opaque<Pressure>()(
    Schema.Struct({
        memory: Schema.Literals(['normal', 'warning', 'critical']).annotate({ description: 'kern.memorystatus_vm_pressure_level' }),
        available: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 100 })).annotate({ description: 'kern.memorystatus_level, percent of memory available' }),
        thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]).annotate({ description: 'ProcessInfo thermalState' }),
        lowPower: Schema.Boolean.annotate({ description: 'Low Power Mode' }),
        advisory: Schema.optionalKey(
            Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])])).annotate({
                description: 'IOCopySystemLoadAdvisoryDetailed levels by key: CombinedLevel, the system verdict on running optional work now, and the UserLevel, BatteryLevel, and ThermalLevel constraints; absent where the call answered NULL',
            }),
        ),
    }).annotate({ description: 'System pressure on subscribe and each memory, thermal, power, or load advisory change, the raw value for a level this build names no case for' }),
) {}
class Pasteboard extends Schema.Opaque<Pasteboard>()(
    Schema.Struct({
        locator: Locator,
        changeCount: Schema.Int.annotate({ description: 'NSPasteboard changeCount, rising with each write by any process' }),
        types: Schema.Array(Schema.String).annotate({ description: 'Pasteboard types the items declare, read with no alert' }),
        access: Schema.Union([Schema.Literals(['default', 'ask', 'alwaysAllow', 'alwaysDeny']), Schema.Int.annotate({ description: 'Raw value of a case this build names none for' })]).annotate({
            description: "NSPasteboard accessBehavior: the owner's Paste from Other Apps setting for the server, default until a read first raised the system's paste alert, which lists the server in that setting",
        }),
        patterns: Schema.Array(Schema.String).annotate({ description: 'NSPasteboardDetectionPattern values the system detects in the content, read with no alert' }),
        contentType: Schema.optionalKey(Schema.String.annotate({ description: 'Uniform type of the file the pasteboard references, read with no alert' })),
        content: Schema.optionalKey(
            Schema.TaggedUnion({ text: { value: Schema.String }, withheld: {} }).annotate({
                description: 'Plain text, read only where access is alwaysAllow, else withheld as a read would raise the alert or meet the denial; absent where no item holds plain text',
            }),
        ),
    }).annotate({ description: 'General pasteboard, the clipboard a person copies to and pastes from' }),
) {}
const ActivationPolicy = Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'Regular in Dock, accessory for menu bar or helper process, prohibited for background only' });
const LaunchedInstance = Schema.Struct({
    pid: Pid,
    instance: ProcessInstance,
    identifier: Schema.String.annotate({ description: 'Bundle identifier' }),
    created: Schema.Boolean.annotate({ description: 'Launch started the instance, false when it reused a running one' }),
    replaced: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Reused instance runs code replaced on disk since it started' })),
});
const containerFields = { container: Ref, kind: ContainerKind, title: Container.fields.title };
const controlFields = { control: Ref, kind: Kind };
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), name: Schema.optionalKey(Schema.String) },
    opened: containerFields,
    closed: containerFields,
    asking: {
        ...containerFields,
        text: Container.fields.text,
        answers: Schema.Array(Schema.Struct({ ref: Ref, label: Schema.optionalKey(Schema.String) }).annotate({ description: 'Action control answering the container, with its label' })),
        ...answering,
    },
    focused: controlFields,
    value: { ...controlFields, fact: Fact },
    selection: { ...controlFields, evidence: Evidence },
    moved: { ...Struct.omit(containerFields, ['title']), frame: Frame },
    frontmost: { active: Schema.Boolean },
    hidden: { hidden: Schema.Boolean },
    window: {
        change: Schema.Union([Schema.Literals(['created', 'destroyed', 'moved', 'resized', 'levelChanged', 'shown', 'hidden']), Schema.String.annotate({ description: 'WindowNotification label of a case this build names none for' })]).annotate({
            description: 'Window-server notification: created, destroyed, moved, or resized for a window no container describes, its level changed, or it was shown or hidden',
        }),
        window: Window.annotate({ description: 'Row after the change, the last row read for a destroyed window' }),
    },
    status: { status: Status },
    announced: {
        text: Schema.String.annotate({ description: 'AXAnnouncementKey: text the application asked assistive technology to speak, as a live region does on each change' }),
        priority: Schema.optionalKey(Schema.Union([Schema.Literals(['low', 'medium', 'high']), Schema.Int.annotate({ description: 'Raw AXPriority value this build names no case for' })]).annotate({ description: 'AXPriorityKey the application posted with it' })),
    },
    dragging: {
        notification: Schema.Literals(['AXDraggingSourceDragBegan', 'AXDraggingSourceDragEnded', 'AXDraggingDestinationDropAllowed', 'AXDraggingDestinationDropNotAllowed', 'AXDraggingDestinationDragAccepted', 'AXDraggingDestinationDragNotAccepted']).annotate({
            description: 'AppKit dragging notification: a drag began or ended at its source, a drop over a destination became allowed or refused, or the destination accepted or refused the drop',
        }),
        control: Schema.optionalKey(Ref),
        kind: Schema.optionalKey(Kind),
    },
    logged: { entry: LogEntry.annotate({ description: 'Fault the instance logged, streamed while a handle drives it' }) },
    reported: { report: DiagnosticReport.annotate({ description: 'Hang, spin, or diagnostic report written for the instance while a handle drives it, a crash report arriving with exited' }) },
    sampled: { report: Schema.URLFromString.annotate({ description: 'sample report of the main thread and every other thread, taken as the instance turned unresponsive while a handle drives it' }) },
    exited: { termination: Exited },
}).annotate({
    description:
        'Change of a selected instance: started, container opened, closed, or asking, focus, value, or selection of a control, a moved container, frontmost, hidden, a window-server window change, status, an announcement, a drag at a control, a logged fault, a written diagnostic report, a sample taken on a hang, exited with its crash report',
});
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: ProcessInstance, sequence: Schema.Int.annotate({ description: 'Per instance, rising with each change' }), at: Schema.String.annotate({ description: 'ISO 8601 time the change was read' }), change: Change })) {}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL.annotate({ description: 'Application bundle' }),
    executable: Schema.OptionFromOptionalKey(Schema.URL.annotate({ description: 'Main executable of the bundle' })),
    identifier: LaunchedInstance.fields.identifier,
    name: Schema.String.annotate({ description: 'Display name of the bundle, as Finder shows it' }),
    bundleName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleName, the name system and crash dialogs show' })),
    version: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleShortVersionString' })),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.OptionFromOptionalKey(Schema.Boolean.annotate({ description: 'Whether the application takes Apple events through a scripting dictionary, absent when its read failed' })),
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    activationPolicy: ActivationPolicy.annotate({ description: 'From LSBackgroundOnly and LSUIElement' }),
    documentTypes: Schema.OptionFromOptionalKey(
        Schema.Array(Schema.Struct({ role: Schema.optionalKey(Schema.String.annotate({ description: 'CFBundleTypeRole' })), extensions: Schema.Array(Schema.String) })).annotate({
            description: 'CFBundleDocumentTypes by role, extensions as declared and as the system states them for each declared content type',
        }),
    ),
    urlSchemes: Schema.OptionFromOptionalKey(Schema.Array(Schema.String).annotate({ description: 'CFBundleURLSchemes of every CFBundleURLTypes entry' })),
    exportedTypes: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ identifier: Schema.String, extensions: Schema.Array(Schema.String) })).annotate({ description: 'UTExportedTypeDeclarations' })),
    multipleInstancesProhibited: Schema.OptionFromOptionalKey(Schema.Boolean.annotate({ description: 'LSMultipleInstancesProhibited' })),
    environment: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'LSEnvironment' })),
    architectures: Schema.OptionFromOptionalKey(Schema.Array(Schema.String).annotate({ description: 'Architectures of the main executable' })),
    quarantine: Schema.OptionFromOptionalKey(
        Schema.Struct({
            agentName: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineAgentName, application that downloaded the bundle' })),
            type: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineType' })),
            originURL: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineOriginURL, page the download came from' })),
            timeStamp: Schema.optionalKey(Schema.String.annotate({ description: 'LSQuarantineTimeStamp, ISO 8601 download time' })),
        }).annotate({ description: 'Quarantine attribute Gatekeeper checks before a first launch, which a failed launch names as its cause' }),
    ),
    claim: Schema.OptionFromOptionalKey(
        Schema.Struct({ prefixes: Schema.Array(Schema.String).annotate({ description: 'Bundle identifier prefixes' }), server: Schema.String.annotate({ description: 'MCP server name' }), scope: Schema.String }).annotate({
            description: 'ApplicationServerClaim: applications the server of this bundle drives',
        }),
    ),
    appIntents: Schema.OptionFromOptionalKey(
        Schema.Array(
            Schema.Struct({
                identifier: Schema.String,
                title: Schema.optionalKey(Schema.String.annotate({ description: 'Title in the interface language, as Shortcuts shows it' })),
                description: Schema.optionalKey(Schema.String),
                openAppWhenRun: Schema.Boolean.annotate({ description: 'Whether running the action opens the application, as it declares' }),
                supportedModes: Schema.Array(Schema.Literals(['background', 'foreground', 'deferred', 'dynamic'])).annotate({
                    description: 'IntentModes it runs in: background without opening the application, foreground opening it at once, deferred opening it later in the run, dynamic deciding as it runs',
                }),
                isDiscoverable: Schema.Boolean.annotate({ description: 'Whether Shortcuts and Spotlight offer it' }),
                parameters: Schema.Array(Schema.Struct({ name: Schema.String, title: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String), isOptional: Schema.Boolean })),
            }),
        ).annotate({ description: 'App Intents actions from the Metadata.appintents of the bundle, its extensions, and the framework packages they include, which Shortcuts and Siri run, absent when their read failed' }),
    ),
    unread: Schema.optionalKey(Schema.Array(Failed).annotate({ description: 'Facts read after the Info.plist that failed, each with its cause, the file as subject: scriptable and quarantine, architectures, appIntents' })),
}) {
    static readonly runs =
        ({ url }: Pick<Installation, 'url'>) =>
        (instance: InstanceSnapshot) =>
            instance.url.href === url.href && !InstanceState.isAnyOf(['exited'])(instance.state);
}
class InstanceSnapshot extends Schema.Class<InstanceSnapshot, { readonly brand: unique symbol }>('InstanceSnapshot')({
    ...Struct.pick(LaunchedInstance.fields, ['pid', 'instance', 'identifier']),
    url: Installation.fields.url,
    activationPolicy: ActivationPolicy,
    language: Schema.optionalKey(Schema.String.annotate({ description: 'Interface language of the instance, on the reply of watch' })),
    state: InstanceState,
    listening: Schema.Array(Schema.Int).annotate({ description: 'TCP ports the process listens on, read at each state, empty unless running or on a failed read' }),
    listeningFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Listening ports read failure' })),
}) {}
const LaunchEvent = Schema.Union([Schema.TaggedStruct('launched', LaunchedInstance.fields), Schema.TaggedStruct('state', InstanceSnapshot.fields), Schema.TaggedStruct('opened', { reply: AppleEventValue }), failed])
    .annotate({ description: 'Launch events: launched first, then states, the reply of the open command once sent, ending idle or asking' })
    .pipe(Schema.toTaggedUnion('_tag'));
const Listing = Schema.Struct({
    installations: Schema.Array(Installation),
    unreadable: Schema.Array(Failed).annotate({ description: 'Bundles that failed to read and application folders whose file events failed to start, each with its cause, the path as subject' }),
});
const Identifiers = { identifiers: Schema.optionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: "Bundle identifiers of the instances monitored, each matching the server's applications, every application the server matches when absent" })) };
class ApplicationFailure extends Schema.TaggedError<ApplicationFailure, { readonly brand: unique symbol }>()('ApplicationFailure', {
    reason: Schema.String.annotate({ description: 'ApplicationFailure.Reason case' }),
    subject: Schema.String,
    message: Schema.String,
    grant: Schema.optionalKey(Schema.Literals(['accessibility', 'screenCapture', 'postEvent']).annotate({ description: 'Privacy grant of noGrant, which the server lacks, or of notResponsible, which its responsible process lacks' })),
    settings: Schema.optionalKey(Schema.String.annotate({ description: 'x-apple.systempreferences URL of the Privacy & Security switch that gives the grant' })),
}) {}
class AppleEventFailure extends Schema.TaggedError<AppleEventFailure, { readonly brand: unique symbol }>()('AppleEventFailure', {
    reason: Schema.String.annotate({ description: 'AppleEventFailure.Reason case, unconsented for a denied or undecided Automation switch' }),
    subject: Schema.String,
    message: Schema.String,
    status: Schema.optionalKey(Schema.Int.annotate({ description: 'OSStatus of the send or reply' })),
}) {}
const Failure = Schema.Union([Unmatched, ApplicationFailure, AppleEventFailure, Schema.ErrorInstance()]);
const ApplicationRpcs = RpcGroup.make(
    Rpc.make('installations', {
        payload: {
            identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
            prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every application the server matches when both are absent' })),
        },
        success: Listing.annotate({ description: 'Applications folders and running regular applications, on subscribe, each Spotlight update, and each regular launch or exit' }),
        stream: true,
    }),
    Rpc.make('installation', { payload: { url: Installation.fields.url.annotate({ description: 'Application bundle, listed or not' }) }, success: Installation, error: Failure }),
    Rpc.make('instances', {
        payload: Identifiers,
        success: Schema.Result(Schema.Array(InstanceSnapshot), Schema.ErrorInstance()).annotate({
            description: 'On subscribe and each change, every instance not background only, an instance watch or a request started a Watch for in its Accessibility state, exited instances kept until next change, else the listing failure',
        }),
        stream: true,
    }),
    Rpc.make('events', {
        payload: Identifiers,
        success: Schema.Result(Event, Schema.ErrorInstance()).annotate({
            description: 'Events of each instance a watch or a request started a Watch for, and of each process outside the Dock raising a modal panel, else the failure of the modal panel scan',
        }),
        stream: true,
    }),
    Rpc.make('observations', {
        payload: Identifiers,
        success: Schema.Struct({ instance: ProcessInstance, identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), language: Schema.String.annotate({ description: 'Interface language of the instance' }), observation: Observation }).annotate({
            description: 'Each container read whose static facts differ from the last sent, or that opened again',
        }),
        stream: true,
    }),
    Rpc.make('session', { success: Schema.Result(Session, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each change of console, lock, display sleep, secure input, or current Space, or the window server refusing its secure input notices' }), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('pasteboard', {
        payload: { text: Schema.optionalKey(Schema.String.annotate({ description: 'Plain text written as the only item before the read' })) },
        success: Pasteboard.annotate({ description: 'General pasteboard once the write, if any, landed' }),
        error: Failure,
    }),
    Rpc.make('watch', {
        payload: { instance: ProcessInstance },
        success: InstanceSnapshot.pipe(Schema.refine((snapshot): snapshot is InstanceSnapshot & { readonly language: string } => snapshot.language !== undefined, { expected: 'an instance snapshot stating its interface language' })).annotate({
            description: 'State and interface language of the instance, its Accessibility events, states, and observations streamed from this call on',
        }),
        error: Failure,
    }),
    Rpc.make('release', { payload: { instance: ProcessInstance.annotate({ description: 'Instance no handle drives any longer, its Watch ended until a request drives it again' }) } }),
    Rpc.make('model', {
        payload: {
            instance: ProcessInstance,
            scope: Scope,
            known: Schema.optionalKey(
                Schema.Struct({
                    build: Schema.OptionFromOptionalKey(Build.annotate({ description: 'Absent where the bundle states no CFBundleVersion' })),
                    containers: Schema.Array(Locator).annotate({ description: 'Containers the interface map holds as read in this build: a window the watch holds open among them is checked in place of read whole' }),
                    labels: Schema.Array(Schema.String).annotate({ description: 'Labels the interface map holds for menus, the readings text recognition picks among the candidates Vision gives for each flyout line' }),
                }),
            ),
        },
        success: Model,
        error: Failure,
    }),
    Rpc.make('intern', {
        payload: { instance: ProcessInstance, locators: Schema.Array(Locator) },
        success: Schema.Array(Ref).annotate({ description: 'Reference of each locator in order, from the table the Watch of the instance keys its own locators by' }),
        error: Failure,
    }),
    Rpc.make('handlers', {
        payload: { document: Schema.URL.annotate({ description: 'File or URL' }) },
        success: Listing.annotate({ description: "Applications the server matches that open the document, the system's default first, then by the system's suitability" }),
        error: Failure,
    }),
    Rpc.make('launch', {
        payload: {
            url: Installation.fields.url,
            documents: Schema.Array(Schema.URL)
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Files the application opens' }),
            arguments: Schema.Array(Schema.String)
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Arguments of a new process, refused where a running instance would receive them' }),
            environment: Schema.Record(Schema.String, Schema.String)
                .pipe(Schema.withConstructorDefault(Effect.succeed({})))
                .annotate({ description: 'Environment variables of a new process, refused as arguments are' }),
            new: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Start an instance beside the running ones' }),
            opening: Schema.Record(Schema.String, AppleEventValue)
                .pipe(Schema.withConstructorDefault(Effect.succeed({})))
                .annotate({ description: 'Parameters of the open command by name or code, sent with the documents to the instance once it turns idle behind the frontmost application' }),
            wait: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Events continue until the instance exits' }),
        },
        success: LaunchEvent,
        error: Failure,
        stream: true,
    }),
    Rpc.make('act', {
        payload: {
            instance: ProcessInstance,
            steps: steps(
                Ref,
                { from: Schema.optionalKey(Rung.pick(['accessibility', 'process', 'window', 'focused', 'system']).annotate({ description: 'Rung the step starts at, the one that last proved it in this build' })) },
                { windows: Schema.optionalKey(Schema.Array(WindowID).annotate({ description: 'Windows the image the points were read from composites, the window under the first point among them taking the events' })) },
            ),
            record: Schema.optionalKey(Recording.annotate({ description: 'Source recorded while the steps run, its key frames returned with the outcomes' })),
        },
        success: Act.Result,
        error: Failure,
    }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, mode: QuitMode.pipe(Schema.withConstructorDefault(Effect.succeed('asking'))) }, success: InstanceState.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('dictionary', { payload: { url: Installation.fields.url }, success: ScriptingDictionary, error: Failure }),
    Rpc.make('execute', {
        payload: {
            instance: ProcessInstance,
            arguments: Schema.Record(Schema.String, AppleEventValue)
                .annotate({ description: 'Parameters of the script command by name or code, direct for direct parameter, accepts reply values' })
                .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        },
        success: Schema.Struct({ reply: AppleEventValue.annotate({ description: 'Direct parameter of the reply, decoded by the scripting dictionary' }), written: Written }),
        error: Failure,
    }),
    Rpc.make('capture', {
        payload: {
            instance: ProcessInstance,
            ...Capture.fields,
            marks: Schema.Array(Schema.Struct({ frame: Frame, label: Schema.String }))
                .pipe(Schema.withConstructorDefault(Effect.succeed([])))
                .annotate({ description: 'Frames drawn into each image they meet, each label set beside its frame (above, below, right, then left) where it stays inside the image and covers no other frame, else inside its top left corner' }),
        },
        success: Capture.Result,
        error: Failure,
    }),
    Rpc.make('record', {
        payload: {
            instance: ProcessInstance,
            ...Recording.fields,
            seconds: Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds recorded, the windows the application opens meanwhile on that display included' }),
            movie: Schema.optionalKey(Schema.URL.annotate({ description: 'File URL SCRecordingOutput writes the movie to, its extension naming a type it writes (mp4, mov); no movie when absent' })),
        },
        success: Recording.Result,
        error: Failure,
    }),
    Rpc.make('scale', {
        payload: { image: Schema.Uint8Array.annotate({ description: 'Image bytes in any format ImageIO reads' }), longestSide: LongestSide },
        success: Schema.Struct(Struct.pick(Capture.Screenshot.fields, ['png', 'width', 'height'])),
        error: Failure,
    }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    Act,
    AppleEventFailure,
    AppleEventValue,
    ApplicationFailure,
    ApplicationRpcs,
    Build,
    Capture,
    Condition,
    Container,
    Control,
    Cursor,
    Diagnostics,
    Event,
    Fact,
    Failure,
    Frame,
    HostLabel,
    Installation,
    InstanceSnapshot,
    InstanceState,
    Instant,
    Kind,
    KeyStep,
    LaunchEvent,
    Locator,
    Model,
    Observation,
    Opener,
    Pasteboard,
    Pointer,
    Pressure,
    QuitMode,
    Recording,
    Running,
    Scope,
    ScriptingDictionary,
    Session,
    Shape,
    Value,
    Written,
};
