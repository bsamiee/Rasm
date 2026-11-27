import { Unmatched } from '@rasm/search/unmatched';
import { Effect, Predicate, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = (typeof FactScalar)['Type'] | { readonly _tag: 'list'; readonly values: readonly Fact[] } | { readonly _tag: 'record'; readonly fields: Readonly<Record<string, Fact>> };
type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | TypedRecord | Specifier | typeof Descriptor.Type | { readonly _tag?: never; readonly [term: string]: AppleEventValue };
interface OptionFromOptionalKeyLambda extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const GRAPHEMES = new Intl.Segmenter(undefined, { granularity: 'grapheme' });
const REPLY_DEADLINE = 120_000;
const PLANE = 2_147_483_647;

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.check(Schema.isGreaterThan(0));
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u)).pipe(Schema.brand('ProcessInstance'));
const ProcessStatus = Schema.Literals(['running', 'runnable', 'throttled', 'uninterruptible', 'sleeping', 'stopped', 'traced']);
const Usage = Schema.Struct({
    cpu: Schema.Finite,
    runnable: Schema.Finite,
    pageWait: Schema.Finite,
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    footprint: Schema.Finite,
    threads: Schema.Int,
    runningThreads: Schema.Int,
    assertions: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    assertionsFailure: Schema.optionalKey(Schema.String),
});
const UsageInterval = Schema.Struct({
    cause: Schema.Literals(['running', 'runnable', 'paging', 'reading', 'writing', 'idle']),
    duration: Schema.Finite,
    ...Struct.pick(Usage.fields, ['cpu', 'runnable', 'pageWait', 'diskBytesRead', 'diskBytesWritten']),
    end: Usage,
});
const Load = Schema.Struct({
    idle: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }))),
    ...Struct.map({ pageins: Schema.Finite, swapins: Schema.Finite, swapouts: Schema.Finite, decompressions: Schema.Finite }, Schema.optionalKey),
    deviceUtilization: Schema.optionalKey(Schema.Int),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    diskTime: Schema.Finite,
    availableCapacity: Schema.Finite,
    availableMemory: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 100 })),
});
class Pressure extends Schema.Opaque<Pressure>()(
    Schema.Struct({
        memory: Schema.Union([Schema.Literals(['normal', 'warning', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
        thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]),
        lowPower: Schema.Boolean,
        advisory: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])]))),
    }),
) {}
const DiagnosticReport = Schema.Struct({
    url: Schema.URLFromString,
    kind: Schema.Literals(['crash', 'hang', 'diag', 'spin']),
    date: Schema.String,
    pid: Pid,
    launched: Schema.optionalKey(Schema.String),
    name: Schema.optionalKey(Schema.String),
    executable: Schema.optionalKey(Schema.String),
    coalition: Schema.optionalKey(Schema.String),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    event: Schema.optionalKey(Schema.String),
    exception: Schema.optionalKey(
        Schema.Struct({
            type: Schema.String,
            signal: Schema.optionalKey(Schema.String),
            subtype: Schema.optionalKey(Schema.String),
            message: Schema.optionalKey(Schema.String),
            rawCodes: Schema.Finite.pipe(Schema.Array, Schema.optionalKey),
        }),
    ),
    termination: Schema.optionalKey(
        Schema.Struct({
            namespace: Schema.String,
            code: Schema.optionalKey(Schema.Finite),
            indicator: Schema.optionalKey(Schema.String),
            byProc: Schema.optionalKey(Schema.String),
            reasons: Schema.String.pipe(Schema.Array, Schema.optionalKey),
            details: Schema.String.pipe(Schema.Array, Schema.optionalKey),
        }),
    ),
    applicationSpecificInformation: Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String))),
    virtualMemoryRegionInfo: Schema.optionalKey(Schema.String),
    faultingThread: Schema.Struct({ image: Schema.optionalKey(Schema.String), imageOffset: Schema.Finite, symbol: Schema.optionalKey(Schema.String), symbolLocation: Schema.optionalKey(Schema.Finite) }).pipe(Schema.Array, Schema.optionalKey),
});
const LogEntry = Schema.Struct({
    timestamp: Schema.String,
    messageType: Schema.Literals(['Debug', 'Info', 'Default', 'Error', 'Fault']),
    processID: Pid,
    processImagePath: Schema.String,
    senderImagePath: Schema.String,
    subsystem: Schema.String,
    category: Schema.String,
    eventMessage: Schema.String,
});
const ended = {
    detail: Schema.Literals(['memory', 'codeSigning', 'decryption']).pipe(Schema.Array, Schema.optionalKey),
    reporting: Schema.optionalKey(Schema.Boolean),
    report: Schema.optionalKey(DiagnosticReport),
    reportFailure: Schema.optionalKey(Schema.String),
};
const Exited = Schema.TaggedUnion({
    status: { status: Schema.Int, ...ended },
    signal: { signal: Schema.Int, ...ended },
    statusFailure: { statusFailure: Schema.String, ...ended },
});
class Diagnostics extends Schema.Opaque<Diagnostics>()(
    Schema.Struct({
        application: Schema.optionalKey(Schema.Struct({ url: Schema.URL, instance: Schema.optionalKey(ProcessInstance) })),
        folders: Schema.Array(Schema.Struct({ url: Schema.URL, glob: Schema.optionalKey(Schema.String) })).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        since: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0))),
        limit: Schema.Int.check(Schema.isGreaterThan(0)),
        find: Schema.optionalKey(Schema.String),
    }),
) {
    static readonly Result = Schema.Struct({
        reports: Schema.Array(DiagnosticReport),
        samples: Schema.Array(Schema.URLFromString),
        logs: Schema.Array(Schema.Struct({ file: Schema.String, text: Schema.String })),
        unified: Schema.Result(Schema.Array(LogEntry), Schema.ErrorInstance()),
        unreadable: Schema.Array(Schema.String),
    });
}
const WindowID = Schema.Finite.check(Schema.isUint32());
const Ref = Schema.Natural;
const Locator = Schema.String;
const Build = Schema.String;
const Instant = Schema.DateTimeUtcFromString;
const Point = Schema.Tuple([Schema.Finite, Schema.Finite]);
const Frame = Schema.Tuple([Point, Point]);
const Value = Schema.Union([Schema.Boolean, Schema.Finite, Schema.String]);
const FactScalar = Schema.Union([
    Schema.TaggedStruct('bool', { value: Schema.Boolean }),
    Schema.TaggedStruct('integer', { value: Schema.Int }),
    Schema.TaggedStruct('real', { value: Schema.Finite }),
    Schema.TaggedStruct('text', { value: Schema.String }),
    Schema.TaggedStruct('date', { value: Schema.String }),
    Schema.TaggedStruct('url', { value: Schema.String }),
    Schema.TaggedStruct('point', { value: Point }),
    Schema.TaggedStruct('size', { value: Point }),
    Schema.TaggedStruct('rect', { value: Frame }),
    Schema.TaggedStruct('color', { red: Schema.Finite, green: Schema.Finite, blue: Schema.Finite, alpha: Schema.Finite }),
    Schema.TaggedStruct('range', { location: Schema.Int, length: Schema.Int }),
    Schema.TaggedStruct('element', { id: Schema.Int }),
    Schema.TaggedStruct('refused', { error: Schema.Int }),
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
]);
const Kind = Schema.Literals(['action', 'toggle', 'choice', 'multi', 'row', 'number', 'measure', 'text', 'color', 'date', 'item', 'region']);
const ContainerKind = Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'statusItem', 'menu', 'toolbar', 'web']);
const TextRange = Schema.Struct({ location: Schema.Natural, length: Schema.Natural });
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
]);
const Label = Schema.Struct({ text: Schema.String, attribute: Schema.Literals(['AXTitle', 'AXDescription', 'recognized', 'AXUserInputLabels', 'AXFilename', 'AXTitleUIElement', 'AXValue', 'AXHelp', 'AXRoleDescription']) });
const HostLabel = Schema.Struct({ text: Schema.String });
const Evidence = Schema.Literals(['attribute', 'marked']);
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
]);
const Modifier = Schema.Literals(['command', 'shift', 'option', 'control', 'function']);
class KeyStep extends Schema.Opaque<KeyStep>()(
    Schema.Struct({
        key: Schema.Union([KeyName, Schema.String.check(Schema.makeFilter((text: string) => Iterator.from(GRAPHEMES.segment(text)).take(2).toArray().length === 1, { expected: 'one character or a special key name' })), Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 65_535 }))]),
        modifiers: Schema.Array(Modifier).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
    }),
) {}
const Presentation = Schema.TaggedUnion({ behind: {}, forward: {}, restored: { back: Schema.Boolean } });
const controlFacts = {
    subrole: Schema.String,
    label: Label,
    labels: Schema.Array(Schema.String),
    section: Schema.String,
    help: Schema.String,
    range: Schema.Struct({
        minimum: Schema.optionalKey(Schema.Finite),
        maximum: Schema.optionalKey(Schema.Finite),
        step: Schema.optionalKey(Schema.Finite),
        wraps: Schema.optionalKey(Schema.Boolean),
    }),
    mark: Schema.String,
    shortcut: KeyStep,
    keyEquivalent: Schema.String,
    separated: Schema.Boolean,
};
class Control extends Schema.Opaque<Control>()(
    Schema.Struct({
        ref: Ref,
        locator: Locator,
        kind: Kind,
        role: Schema.String,
        positional: Schema.Boolean,
        ...Struct.map(controlFacts, Schema.optionalKey),
        label: Schema.optionalKey(Schema.Union([Label, HostLabel])),
        value: Schema.optionalKey(Fact),
        enabled: Schema.optionalKey(Schema.Boolean),
        focused: Schema.optionalKey(Schema.Boolean),
        selected: Schema.optionalKey(Evidence),
        frame: Schema.optionalKey(Frame),
        options: Schema.Struct({ label: Schema.String, ref: Ref, ...Struct.map(Struct.pick(controlFacts, ['help', 'shortcut']), Schema.optionalKey) }).pipe(Schema.Array, Schema.optionalKey),
        opens: Schema.optionalKey(Ref),
        alternate: Schema.optionalKey(Ref),
        placeholder: Schema.optionalKey(Schema.String),
        settable: Schema.optionalKey(Schema.Boolean),
        level: Schema.optionalKey(Schema.Int),
        index: Schema.optionalKey(Schema.Natural),
        row: Schema.optionalKey(Ref),
        expanded: Schema.optionalKey(Schema.Boolean),
        actions: Schema.String.pipe(Schema.Array, Schema.optionalKey),
        alternateUIVisible: Schema.optionalKey(Schema.Boolean),
        disclosedBy: Schema.optionalKey(Ref),
        total: Schema.optionalKey(Schema.Natural),
        sort: Schema.optionalKey(Schema.Literals(['ascending', 'descending'])),
        cells: Schema.Struct({ column: Schema.optionalKey(Schema.String), text: Schema.String }).pipe(Schema.Array, Schema.optionalKey),
        column: Schema.optionalKey(Schema.String),
        scroll: Schema.optionalKey(Schema.Struct({ contentSize: Schema.optionalKey(Point), horizontalScrollBar: Schema.optionalKey(Schema.Finite), verticalScrollBar: Schema.optionalKey(Schema.Finite) })),
        selectedTextRanges: TextRange.pipe(Schema.Array, Schema.optionalKey),
        numberOfCharacters: Schema.optionalKey(Schema.Natural),
        insertionPointLineNumber: Schema.optionalKey(Schema.Natural),
        runs: Schema.Struct({ range: TextRange, text: Schema.String, attributes: Schema.Record(Schema.String, Fact), frame: Schema.optionalKey(Frame) }).pipe(Schema.Array, Schema.optionalKey),
        invalid: Schema.optionalKey(Schema.Literals(['true', 'grammar', 'spelling'])),
        required: Schema.optionalKey(Schema.Boolean),
        current: Schema.optionalKey(Schema.Literals(['true', 'page', 'step', 'location', 'date', 'time'])),
    }),
) {}
class Container extends Schema.Opaque<Container>()(
    Schema.Struct({
        ref: Ref,
        locator: Locator,
        kind: ContainerKind,
        title: Schema.optionalKey(Schema.String),
        help: Schema.optionalKey(controlFacts.help),
        document: Schema.optionalKey(Schema.String),
        edited: Schema.optionalKey(Schema.Boolean),
        fullScreen: Schema.optionalKey(Schema.Boolean),
        loaded: Schema.optionalKey(Schema.Boolean),
        loadingProgress: Schema.optionalKey(Schema.Finite),
        window: Schema.optionalKey(WindowID),
        frame: Schema.optionalKey(Frame),
        layer: Schema.optionalKey(Schema.Int),
        closed: Schema.Boolean,
        modal: Schema.Boolean,
        asking: Schema.Boolean,
        focused: Schema.optionalKey(Schema.Boolean),
        parent: Schema.optionalKey(Ref),
        default: Schema.optionalKey(Ref),
        cancel: Schema.optionalKey(Ref),
        close: Schema.optionalKey(Ref),
        text: Schema.Array(Schema.String),
        truncated: Schema.Boolean,
        unreadable: Schema.Boolean,
        controls: Schema.Array(Control),
    }),
) {}
const Cursor = Schema.TaggedUnion({ system: { name: Schema.String }, custom: { digest: Schema.String, hotSpot: Point, size: Point } });
const Pointer = Schema.Struct({
    tool: Schema.optionalKey(Schema.String),
    shortcut: Schema.optionalKey(controlFacts.shortcut),
    help: Schema.optionalKey(controlFacts.help),
    holding: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    operations: Schema.String.pipe(Schema.Array, Schema.optionalKey),
    cursor: Schema.optionalKey(Cursor),
    draggingFrame: Schema.optionalKey(Frame),
});
class Model extends Schema.Opaque<Model>()(Schema.Struct({ sequence: Schema.Int, containers: Schema.Array(Container), pointer: Schema.optionalKey(Pointer) })) {}
const Scope = Schema.TaggedUnion({
    application: {},
    container: { ref: Ref },
    menu: { ref: Ref },
    rows: { ref: Ref, from: Schema.Natural, count: Schema.Int.check(Schema.isGreaterThan(0)) },
    text: { ref: Ref },
    search: { text: Schema.String, key: Schema.optionalKey(SearchKey), limit: Schema.Int.check(Schema.isGreaterThan(0)) },
    menus: {},
});
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
    opens: Schema.OptionFromOptionalKey(Locator),
    alternate: Schema.OptionFromOptionalKey(Locator),
    parent: Schema.OptionFromOptionalKey(Locator),
});
class Observation extends Schema.Opaque<Observation>()(
    Schema.Struct({
        build: Schema.OptionFromOptionalKey(Build),
        container: Locator,
        kind: ContainerKind,
        title: Schema.OptionFromOptionalKey(Schema.String),
        modal: Schema.Boolean,
        layer: Schema.OptionFromOptionalKey(Container.fields.layer.schema),
        size: Schema.OptionFromOptionalKey(Point),
        default: Schema.OptionFromOptionalKey(Locator),
        cancel: Schema.OptionFromOptionalKey(Locator),
        close: Schema.OptionFromOptionalKey(Locator),
        opened: Instant,
        at: Instant,
        truncated: Schema.Boolean,
        opener: Schema.OptionFromOptionalKey(Opener),
        documents: Schema.Array(Schema.String),
        conditions: Schema.Array(Condition),
        controls: Schema.Record(Locator, Shape),
        layout: Schema.Record(Locator, Frame),
        values: Schema.Record(Locator, Value),
    }),
) {}
const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String.check(Schema.makeFilter((key: string) => !Object.hasOwn(fields, key))), Schema.Json)]);
const failure = { reason: Schema.String, subject: Schema.String, message: Schema.optionalKey(Schema.String) };
const Failed = open(failure);
const Window = Schema.Struct({
    number: WindowID,
    title: Schema.optionalKey(Schema.String),
    frame: Frame,
    layer: Container.fields.layer.schema,
    band: Schema.Literals(['normal', 'floating', 'modalPanel', 'utility', 'dock', 'mainMenu', 'status', 'popUpMenu', 'overlay', 'help', 'dragging', 'screenSaver', 'assistiveTechHigh', 'cursor']),
    parent: Schema.optionalKey(WindowID),
    spaces: Schema.Array(Schema.Int),
    onScreen: Schema.Boolean,
    hidden: Schema.Boolean,
    shared: Schema.Boolean,
    remote: Schema.optionalKey(WindowID),
    container: Schema.optionalKey(Ref),
    kind: Schema.optionalKey(Container.fields.kind),
    document: Schema.optionalKey(Container.fields.document.schema),
    asking: Schema.Boolean,
    presented: Schema.Boolean,
});
const Status = Schema.TaggedUnion({ launching: {}, responding: {}, unresponsive: { thread: ProcessStatus } });
class Running extends Schema.Opaque<Running>()(
    Schema.Struct({
        active: Schema.Boolean,
        status: Status,
        pendingEventAge: Schema.optionalKey(Schema.Finite),
        usage: Schema.optionalKey(UsageInterval),
        usageFailure: Schema.optionalKey(Schema.String),
        windows: Schema.Array(Window),
        statusItems: Schema.optionalKey(Schema.Natural),
        statusItemsFailure: Schema.optionalKey(Schema.String),
    }),
) {}
const InstanceState = Object.assign(Schema.Union([Schema.TaggedStruct('running', Running.fields), open({ _tag: Schema.tag('failed'), ...failure }), Schema.TaggedStruct('exited', { termination: Exited })]).pipe(Schema.toTaggedUnion('_tag')), { Exited, Status });
const QuitMode = Schema.Literals(['asking', 'saving', 'discarding', 'forcing']);
const Written = Schema.Result(Schema.Array(Schema.URL), Schema.ErrorInstance());
class Session extends Schema.Opaque<Session>()(Schema.Struct({ onConsole: Schema.Boolean, locked: Schema.Boolean, asleep: Schema.Boolean, secureInput: Schema.Boolean, spaces: Schema.Array(Schema.Int) })) {}
class Pasteboard extends Schema.Opaque<Pasteboard>()(
    Schema.Struct({
        locator: Container.fields.locator,
        changeCount: Schema.Int,
        types: Schema.Array(Schema.String),
        access: Schema.Union([Schema.Literals(['default', 'ask', 'alwaysAllow', 'alwaysDeny']), Schema.Int]),
        patterns: Schema.Array(Schema.String),
        contentType: Schema.optionalKey(Schema.String),
        content: Schema.optionalKey(Schema.TaggedUnion({ text: { value: Schema.String }, withheld: {}, concealed: {} })),
    }),
) {}
const LaunchedInstance = Schema.Struct({ pid: Pid, instance: ProcessInstance, identifier: Schema.String, created: Schema.Boolean, replaced: Schema.optionalKey(Schema.Boolean) });
const container = { container: Ref, kind: Container.fields.kind, title: Container.fields.title };
const control = { control: Ref, kind: Control.fields.kind };
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), name: Schema.optionalKey(Schema.String) },
    opened: container,
    closed: container,
    asking: {
        ...container,
        text: Container.fields.text,
        answers: Schema.Array(Schema.Struct({ ref: Ref, label: Schema.optionalKey(Schema.String) })),
        secure: Schema.optionalKey(Schema.Struct({ ref: Ref, label: Schema.optionalKey(Schema.String) })),
        presentation: Schema.optionalKey(Presentation),
        ...Struct.pick(Container.fields, ['default', 'cancel', 'close']),
    },
    focused: control,
    value: { ...control, fact: Fact },
    selection: { ...control, evidence: Control.fields.selected.schema },
    moved: { ...Struct.omit(container, ['title']), frame: Frame },
    frontmost: { active: Schema.Boolean },
    hidden: { hidden: Schema.Boolean },
    window: { change: Schema.Union([Schema.Literals(['created', 'destroyed', 'moved', 'resized', 'levelChanged', 'shown', 'hidden', 'addedToSpace', 'removedFromSpace']), Schema.String]), window: Window },
    status: { status: Status },
    announced: { text: Schema.String, priority: Schema.optionalKey(Schema.Literals(['low', 'medium', 'high'])) },
    dragging: {
        notification: Schema.Literals(['AXDraggingSourceDragBegan', 'AXDraggingSourceDragEnded', 'AXDraggingDestinationDropAllowed', 'AXDraggingDestinationDropNotAllowed', 'AXDraggingDestinationDragAccepted', 'AXDraggingDestinationDragNotAccepted']),
        control: Schema.optionalKey(Ref),
        kind: Schema.optionalKey(Control.fields.kind),
    },
    proven: { followed: Schema.Int, drawn: Schema.Boolean },
    logged: { entry: LogEntry },
    reported: { report: DiagnosticReport },
    sampled: { report: Schema.URLFromString },
    exited: { termination: Exited },
});
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: ProcessInstance, sequence: Schema.Int, at: Schema.String, change: Change })) {}
class InstanceSnapshot extends Schema.Class<InstanceSnapshot, { readonly brand: unique symbol }>('InstanceSnapshot')({
    ...Struct.pick(LaunchedInstance.fields, ['pid', 'instance', 'identifier']),
    name: Schema.optionalKey(Schema.String),
    url: Schema.URL,
    activationPolicy: Schema.Union([Schema.Literals(['regular', 'accessory', 'prohibited']), Schema.TemplateLiteral([Schema.Int])]),
    language: Schema.optionalKey(Schema.String),
    launchDate: Schema.optionalKey(Instant),
    usage: Schema.optionalKey(Usage),
    usageFailure: Schema.optionalKey(Schema.String),
    state: InstanceState,
    listening: Schema.Array(Schema.Int),
    listeningFailure: Schema.optionalKey(Schema.String),
}) {
    static readonly Watched = InstanceSnapshot.pipe(Schema.refine((snapshot): snapshot is InstanceSnapshot & { readonly language: string } => snapshot.language !== undefined, { expected: 'an instance snapshot stating its interface language' }));
}
const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
class TypedRecord extends Schema.Opaque<TypedRecord>()(Schema.TaggedStruct('record', { type: Schema.String, fields: Schema.Record(Schema.String, Nested) })) {}
class Specifier extends Schema.Opaque<Specifier>()(Schema.TaggedStruct('specifier', { want: Schema.String, form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']), seld: Nested, from: Schema.optionalKey(Nested) })) {}
const Descriptor = Schema.Union([
    Schema.TaggedStruct('type', { name: Schema.String }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }),
    Schema.TaggedStruct('date', { date: Schema.String }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('opaque', { type: Schema.String, data: Schema.String, value: Schema.optionalKey(Schema.String) }),
]);
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([Schema.Null, Schema.Boolean, Schema.Finite, Schema.String, Schema.Array(Nested), TypedRecord, Specifier, Descriptor, Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)])]);
const Term = Schema.Struct({ name: Schema.String, code: Schema.String });
const Command = Term.mapFields(Struct.assign({ parameters: Schema.Array(Term.mapFields(Struct.assign({ types: Schema.Array(Schema.String), list: Schema.Boolean, enumerators: Schema.Array(Term) }))) }));
const DictionaryKind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']);
class ScriptingDictionary extends Schema.Class<ScriptingDictionary, { readonly brand: unique symbol }>('ScriptingDictionary')({
    ...Struct.Record(DictionaryKind.literals, Schema.Array(Term)),
    command: Schema.Array(Command),
    script: Schema.OptionFromOptionalKey(Command),
    open: Schema.OptionFromOptionalKey(Command),
}) {
    static readonly Kind = DictionaryKind;
}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: InstanceSnapshot.fields.url,
    executable: Schema.OptionFromOptionalKey(Schema.URL),
    identifier: LaunchedInstance.fields.identifier,
    name: Schema.String,
    bundleName: Schema.OptionFromOptionalKey(Schema.String),
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String),
    scriptable: Schema.OptionFromOptionalKey(Schema.Boolean),
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String),
    activationPolicy: InstanceSnapshot.fields.activationPolicy,
    documentTypes: Schema.Struct({ role: Schema.optionalKey(Schema.String), extensions: Schema.Array(Schema.String) }).pipe(Schema.Array, Schema.OptionFromOptionalKey),
    urlSchemes: Schema.String.pipe(Schema.Array, Schema.OptionFromOptionalKey),
    exportedTypes: Schema.Struct({ identifier: Schema.String, extensions: Schema.Array(Schema.String) }).pipe(Schema.Array, Schema.OptionFromOptionalKey),
    multipleInstancesProhibited: Schema.OptionFromOptionalKey(Schema.Boolean),
    environment: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.String)),
    architectures: Schema.String.pipe(Schema.Array, Schema.OptionFromOptionalKey),
    quarantine: Schema.OptionFromOptionalKey(Schema.Struct({ agentName: Schema.optionalKey(Schema.String), type: Schema.optionalKey(Schema.String), originURL: Schema.optionalKey(Schema.String), timeStamp: Schema.optionalKey(Schema.String) })),
    claim: Schema.OptionFromOptionalKey(Schema.Struct({ prefixes: Schema.Array(Schema.String), server: Schema.String, scope: Schema.String })),
    appIntents: Schema.Struct({
        identifier: Schema.String,
        title: Schema.optionalKey(Schema.String),
        description: Schema.optionalKey(Schema.String),
        openAppWhenRun: Schema.Boolean,
        supportedModes: Schema.Array(Schema.Literals(['background', 'foreground', 'deferred', 'dynamic'])),
        isDiscoverable: Schema.Boolean,
        parameters: Schema.Array(Schema.Struct({ name: Schema.String, title: Schema.optionalKey(Schema.String), description: Schema.optionalKey(Schema.String), isOptional: Schema.Boolean })),
    }).pipe(Schema.Array, Schema.OptionFromOptionalKey),
    unread: Failed.pipe(Schema.Array, Schema.optionalKey),
}) {
    static readonly runs =
        ({ url }: Pick<Installation, 'url'>) =>
        (instance: InstanceSnapshot) =>
            instance.url.href === url.href && !InstanceState.isAnyOf(['exited'])(instance.state);
}
const LaunchEvent = Schema.Union([Schema.TaggedStruct('launched', LaunchedInstance.fields), Schema.TaggedStruct('state', InstanceSnapshot.fields), Schema.TaggedStruct('opened', { reply: AppleEventValue }), InstanceState.cases.failed]).pipe(Schema.toTaggedUnion('_tag'));
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0));
class Capture extends Schema.Opaque<Capture>()(
    Schema.Struct({
        source: Schema.TaggedUnion({ windows: { numbers: Schema.NonEmptyArray(WindowID) }, element: { ref: Ref }, region: { frame: Frame } }),
        longestSide: Schema.OptionFromOptionalKey(LongestSide),
    }),
) {
    static readonly Screenshot = Schema.Struct({
        png: Schema.Uint8Array,
        width: Schema.Int,
        height: Schema.Int,
        frame: Frame,
        windows: Schema.Array(WindowID),
        backingStore: Schema.Boolean,
        document: Schema.optionalKey(Schema.String),
        presentation: Schema.optionalKey(Presentation),
    });
    static readonly Result = Schema.Array(Schema.Result(Capture.Screenshot, Schema.ErrorInstance()));
}
class Recording extends Schema.Opaque<Recording>()(Schema.Struct({ ...Capture.fields, frames: Schema.Int.check(Schema.isGreaterThan(0)) })) {
    static readonly Marker = Schema.Union([Schema.TaggedStruct('step', { index: Schema.Int, at: Instant }), Schema.TaggedStruct('event', Event.fields)]);
    static readonly Result = Schema.Struct({
        keyFrames: Schema.Array(Schema.Struct({ at: Instant, changed: Schema.optionalKey(Frame), screenshot: Capture.Screenshot, markers: Schema.Array(Recording.Marker) })),
        presentation: Schema.optionalKey(Presentation),
    });
}
const Milliseconds = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: REPLY_DEADLINE }));
const PenPressure = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }));
const Tilt = Schema.Finite.check(Schema.isBetween({ minimum: -1, maximum: 1 }));
const Sample = Schema.Union([Point, Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure]), Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure, Tilt, Tilt])]);
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
const ranged = Schema.makeFilter(({ to, range }: { readonly to?: unknown; readonly range?: unknown }) => range === undefined || to === undefined || Predicate.isString(to), { expected: 'a range replaced by text alone' });
const timing = {
    hold: Schema.optionalKey(Milliseconds),
    count: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: REPLY_DEADLINE }))),
    every: Schema.optionalKey(Milliseconds),
    jitter: Schema.optionalKey(Milliseconds),
    edge: Schema.optionalKey(Schema.Literals(['down', 'up'])),
};
const input = <Position extends Schema.Top>(position: Position) =>
    Schema.Struct({
        pointer: position,
        path: position.pipe(Schema.Array, Schema.optionalKey),
        button: Schema.optionalKey(Schema.Literals(['left', 'right', 'middle', 'none'])),
        by: Schema.optionalKey(Point),
        unit: Schema.optionalKey(Schema.Literals(['pixel', 'line'])),
        modifiers: KeyStep.fields.modifiers,
        ...timing,
    }).check(paced, pointed);
const Route = Schema.TaggedUnion({ source: { member: Schema.String, approximated: Schema.Array(Schema.String) }, process: {}, window: { number: WindowID }, focused: { number: WindowID }, system: {} });
const Rung = Schema.Literals(['source', 'accessibility', 'process', 'window', 'focused', 'system']);
const from = Schema.optionalKey(Rung.pick(['accessibility', 'process', 'window', 'focused', 'system']));
const Step = Schema.Union([
    Schema.Struct({ ...KeyStep.fields, at: Schema.optionalKey(Ref), ...timing, from }).check(paced),
    input(Schema.Union([Ref, Sample])).mapFields(Struct.assign({ windows: WindowID.pipe(Schema.Array, Schema.optionalKey), from }), { unsafePreserveChecks: true }),
    Schema.Struct({
        at: Ref,
        action: Schema.optionalKey(Schema.String.check(Schema.makeFilter((named: string) => named !== 'AXRaise', { expected: 'an action other than AXRaise, which takes focus from the frontmost application' }))),
        to: Schema.optionalKey(Schema.Union([Value, Frame])),
        range: Schema.optionalKey(Schema.Union([Schema.Tuple([Schema.Natural, Schema.Natural]), Schema.String])),
        from,
    }).check(ranged),
    Schema.Struct({ modifiers: Schema.NonEmptyArray(Modifier), hold: Milliseconds }),
]);
const Scrolled = Schema.Struct({
    horizontal: Schema.optionalKey(Schema.Finite),
    vertical: Schema.optionalKey(Schema.Finite),
    rows: Schema.optionalKey(Schema.Int),
    showing: Schema.Array(Schema.Struct({ index: Schema.Int, label: Schema.optionalKey(Schema.String) })),
});
const Delivery = Schema.Struct({
    proof: Schema.Literals(['answer', 'change', 'value', 'limit', 'posting', 'unanswered', 'exit', 'standing']),
    window: Schema.optionalKey(WindowID),
    route: Schema.optionalKey(Route),
    tried: Schema.Array(Schema.Struct({ rung: Rung, failure: Schema.String })),
    presentation: Schema.optionalKey(Presentation),
    pointer: Schema.optionalKey(Point),
    popUps: Schema.Array(Schema.Struct({ window: WindowID, closed: Schema.Boolean })),
    scrolled: Schema.optionalKey(Scrolled),
    resolved: Schema.optionalKey(Schema.Struct({ ref: Ref, effect: Schema.Literals(['pressed', 'selected', 'focused']) })),
    followed: Schema.optionalKey(Schema.Int),
    surface: Schema.optionalKey(Ref),
});
const Act = {
    Rung,
    Route,
    Input: input(Sample),
    Step,
    Result: Schema.Struct({
        outcomes: Schema.Array(Schema.Result(Delivery, Schema.ErrorInstance())),
        state: InstanceState,
        usage: Schema.optionalKey(Schema.Result(UsageInterval, Schema.ErrorInstance())),
        load: Schema.optionalKey(Schema.Result(Load, Schema.ErrorInstance())),
        written: Schema.optionalKey(Written),
        pasteboard: Schema.optionalKey(Schema.Result(Pasteboard, Schema.ErrorInstance())),
        recording: Schema.optionalKey(Schema.Result(Recording.Result, Schema.ErrorInstance())),
    }),
};
const Listing = Schema.Struct({ installations: Schema.Array(Installation), unreadable: Schema.Array(Failed) });
const Identifiers = { identifiers: Schema.String.pipe(Schema.NonEmptyArray, Schema.optionalKey) };
const Table = Schema.Array(Locator);

// --- [ERRORS] --------------------------------------------------------------------------

class ApplicationFailure extends Schema.TaggedError<ApplicationFailure, { readonly brand: unique symbol }>()('ApplicationFailure', {
    reason: Failed.schema.fields.reason,
    subject: Schema.String,
    message: Schema.String,
    grant: Schema.optionalKey(Schema.Literals(['accessibility', 'screenCapture', 'postEvent'])),
    settings: Schema.optionalKey(Schema.String),
}) {}
class AppleEventFailure extends Schema.TaggedError<AppleEventFailure, { readonly brand: unique symbol }>()('AppleEventFailure', { reason: Schema.String, subject: Schema.String, message: Schema.String, status: Schema.optionalKey(Schema.Int) }) {}
const Failure = Schema.Union([Unmatched, ApplicationFailure, AppleEventFailure, Schema.ErrorInstance()]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const ApplicationRpcs = RpcGroup.make(
    Rpc.make('installations', { payload: { identifiers: Schema.String.pipe(Schema.Array, Schema.optionalKey), prefix: Schema.optionalKey(Schema.String), name: Schema.optionalKey(Schema.String) }, success: Listing, stream: true }),
    Rpc.make('installation', { payload: { url: Installation.fields.url }, success: Installation, error: Failure }),
    Rpc.make('instances', { payload: Identifiers, success: Schema.Result(Schema.Array(InstanceSnapshot), Schema.ErrorInstance()), stream: true }),
    Rpc.make('events', { payload: Identifiers, success: Schema.Result(Event, Schema.ErrorInstance()), stream: true }),
    Rpc.make('observations', { payload: Identifiers, success: Schema.Struct({ instance: ProcessInstance, identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), language: Schema.String, observation: Observation }), stream: true }),
    Rpc.make('session', { success: Schema.Result(Session, Schema.ErrorInstance()), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()), stream: true }),
    Rpc.make('pasteboard', { payload: { text: Schema.optionalKey(Schema.String), content: Schema.Boolean }, success: Pasteboard, error: Failure }),
    Rpc.make('watch', { payload: { instance: ProcessInstance, locators: Schema.optionalKey(Table) }, success: InstanceSnapshot.Watched, error: Failure }),
    Rpc.make('locators', { payload: { instance: ProcessInstance }, success: Table, error: Failure }),
    Rpc.make('release', { payload: { instance: ProcessInstance } }),
    Rpc.make('model', {
        payload: { instance: ProcessInstance, scope: Scope, known: Schema.optionalKey(Schema.Struct({ build: Schema.OptionFromOptionalKey(Build), containers: Schema.Array(Locator), labels: Schema.Array(Schema.String) })) },
        success: Model,
        error: Failure,
    }),
    Rpc.make('intern', { payload: { instance: ProcessInstance, locators: Schema.Array(Locator) }, success: Schema.Array(Ref), error: Failure }),
    Rpc.make('handlers', { payload: { document: Schema.URL }, success: Listing, error: Failure }),
    Rpc.make('launch', {
        payload: {
            url: Installation.fields.url,
            documents: Schema.Array(Schema.URL).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
            arguments: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
            environment: Schema.Record(Schema.String, Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed({}))),
            new: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))),
            opening: Schema.Record(Schema.String, AppleEventValue).pipe(Schema.withConstructorDefault(Effect.succeed({}))),
            wait: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))),
        },
        success: LaunchEvent,
        error: Failure,
        stream: true,
    }),
    Rpc.make('act', {
        payload: {
            instance: ProcessInstance,
            steps: Schema.NonEmptyArray(Act.Step),
            record: Schema.optionalKey(Recording),
        },
        success: Act.Result,
        error: Failure,
    }),
    Rpc.make('activate', { payload: { instance: ProcessInstance }, success: Presentation, error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, mode: QuitMode.pipe(Schema.withConstructorDefault(Effect.succeed('asking'))) }, success: InstanceState, error: Failure }),
    Rpc.make('dictionary', { payload: { url: Installation.fields.url }, success: ScriptingDictionary, error: Failure }),
    Rpc.make('execute', {
        payload: { instance: ProcessInstance, arguments: Schema.Record(Schema.String, AppleEventValue).pipe(Schema.withConstructorDefault(Effect.succeed({}))) },
        success: Schema.Struct({ reply: AppleEventValue, written: Written }),
        error: Failure,
    }),
    Rpc.make('capture', {
        payload: { instance: ProcessInstance, ...Capture.fields, marks: Schema.Array(Schema.Struct({ frame: Frame, label: Schema.String })).pipe(Schema.withConstructorDefault(Effect.succeed([]))) },
        success: Capture.Result,
        error: Failure,
    }),
    Rpc.make('record', {
        payload: { instance: ProcessInstance, ...Recording.fields, seconds: Schema.Finite.check(Schema.isGreaterThan(0)), movie: Schema.optionalKey(Schema.URL) },
        success: Recording.Result,
        error: Failure,
    }),
    Rpc.make('scale', { payload: { image: Schema.Uint8Array, longestSide: LongestSide }, success: Schema.Struct(Struct.pick(Capture.Screenshot.fields, ['png', 'width', 'height'])), error: Failure }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    Act,
    AppleEventValue,
    AppleEventFailure,
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
    KeyStep,
    Kind,
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
