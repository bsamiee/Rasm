import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Function, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Fact = (typeof FactScalar)['Type'] | { readonly _tag: 'list'; readonly values: readonly Fact[] } | { readonly _tag: 'record'; readonly fields: Readonly<Record<string, Fact>> };
type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | TypedRecord | Specifier | typeof Descriptor.Type | { readonly _tag?: never; readonly [term: string]: AppleEventValue };
interface OptionFromOptionalKeyLambda extends Struct.Lambda {
    <S extends Schema.Constraint>(self: S): Schema.OptionFromOptionalKey<S>;
    readonly '~lambda.out': this['~lambda.in'] extends Schema.Constraint ? Schema.OptionFromOptionalKey<this['~lambda.in']> : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String.check(Schema.makeFilter((key: string) => !Object.hasOwn(fields, key))), Schema.Json)]);
const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
class TypedRecord extends Schema.Opaque<TypedRecord>()(Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' })) {}
class Specifier extends Schema.Opaque<Specifier>()(
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Nested.annotate({ description: 'Selection data, or first, middle, last, any, or every for index form' }),
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
const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Running process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' });
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
const ContainerKind = Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'statusItem', 'menu', 'toolbar']).annotate({ description: 'Container kind, prompt for a dialog of an application that is not regular' });
const Label = Schema.Struct({ text: Schema.String, attribute: Schema.String.annotate({ description: 'Attribute the label was read from, AXHelp where siblings share a label, recognized where text recognition read it from the window image' }) });
const Evidence = Schema.Literals(['attribute', 'marked']).annotate({ description: 'How the selection is known: an attribute states it, or exactly one sibling carries a read-only value' });
const KeyName = Schema.Literals(['return', 'enter', 'tab', 'space', 'delete', 'forwardDelete', 'escape', 'home', 'end', 'pageUp', 'pageDown', 'left', 'right', 'down', 'up', 'help', 'f1', 'f2', 'f3', 'f4', 'f5', 'f6', 'f7', 'f8', 'f9', 'f10', 'f11', 'f12']);
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
    }),
) {}
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
        window: Schema.optionalKey(WindowID),
        frame: Schema.optionalKey(Frame),
        layer: Schema.optionalKey(Schema.Int.annotate({ description: 'Window server layer' })),
        modal: Schema.Boolean,
        asking: Schema.Boolean.annotate({ description: 'Requires an answer' }),
        focused: Schema.optionalKey(Schema.Boolean),
        parent: Schema.optionalKey(Ref.annotate({ description: 'Container this one sits in' })),
        default: Schema.optionalKey(Ref),
        cancel: Schema.optionalKey(Ref),
        close: Schema.optionalKey(Ref),
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
    tool: Schema.optionalKey(Schema.String.annotate({ description: 'Tool the pointer acts with, as the host names it, else the selected option of the one toolbar choice, the focused window first' })),
    shortcut: Schema.optionalKey(controlFacts.shortcut),
    help: Schema.optionalKey(controlFacts.help),
    holding: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Objects the pointer carries to place, as the host addresses them' })),
    operations: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Operations in progress, a modal tool state or dialog among them, in the host names, outermost first' })),
    cursor: Schema.optionalKey(Cursor),
    draggingFrame: Schema.optionalKey(Frame.annotate({ description: 'Frame of the image a drag session of the application shows, from its window at the dragging window level' })),
}).annotate({ description: 'Pointer of the application: tool with its shortcut and help, carried objects, and operations a host source or toolbar choice states, cursor and drag the system states' });
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
    search: { text: Schema.String.annotate({ description: 'Text the application matches against its elements' }), limit: Schema.Int.check(Schema.isGreaterThan(0)) },
    menus: {},
}).annotate({ description: 'Every window and the menu bar, one container, one menu level read on demand, rows of a multi, the containers of elements the application finds by text (AXUIElementsForSearchPredicate), or the menu bar with every level read so far' });
const Build = Schema.String.annotate({ description: 'CFBundleVersion of the instance read' });
const Instant = Schema.DateTimeUtcFromString;
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});
const Opener = Schema.Struct({ container: Locator, control: Locator });
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
        build: Build,
        container: Locator,
        kind: ContainerKind,
        title: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Title of a container holding no AXDocument, as a document window titles the document' })),
        modal: Schema.Boolean,
        layer: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Window server layer' })),
        size: Schema.OptionFromOptionalKey(Size),
        default: Schema.OptionFromOptionalKey(Locator),
        cancel: Schema.OptionFromOptionalKey(Locator),
        close: Schema.OptionFromOptionalKey(Locator),
        opened: Instant.annotate({ description: 'When the container last opened, the epoch of this read' }),
        at: Instant.annotate({ description: 'When this read was made' }),
        truncated: Schema.Boolean.annotate({ description: 'The read stopped at its node or time bound before every element of the container' }),
        opener: Schema.OptionFromOptionalKey(Opener.annotate({ description: 'Control whose step opened the container' })),
        documents: Schema.Array(Schema.String.annotate({ description: 'Path of a document open in the instance' })),
        conditions: Schema.Array(Condition),
        controls: Schema.Record(Locator, Shape),
        layout: Schema.Record(Locator, Frame.annotate({ description: 'Origin in its container and size, in points, absent where the container states no frame' })),
        values: Schema.Record(Locator, Value),
    }).annotate({ description: 'One read of a container: its static facts by locator, their layout and values, and the conditions it was read under' }),
) {}
const Window = Schema.Struct({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Schema.Int,
    onScreen: Schema.Boolean,
    container: Schema.optionalKey(Ref.annotate({ description: 'Container the window shows' })),
    kind: Schema.optionalKey(ContainerKind),
    document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument URL' })),
    asking: Schema.Boolean.annotate({ description: 'Requires an answer, or blocks its application at modal layer' }),
    presented: Schema.Boolean.annotate({ description: 'A container describes it, or it is on screen' }),
}).annotate({ description: 'Window server row, asking rows first' });
const ProcessStatus = Schema.Literals(['running', 'runnable', 'throttled', 'uninterruptible', 'sleeping', 'stopped']).annotate({
    description: 'Main thread state: running after time on a processor, runnable while waiting for one, throttled or suspended by the kernel under resource starvation, uninterruptible while waiting in the kernel on a page-in or disk read, sleeping while waiting on an event, stopped by a signal or suspension',
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
    pageins: Schema.Finite,
    swapins: Schema.Finite.annotate({ description: 'Pages swapped in from compressor segments' }),
    swapouts: Schema.Finite.annotate({ description: 'Pages swapped out to compressor segments' }),
    decompressions: Schema.Finite.annotate({ description: 'Pages decompressed' }),
    deviceUtilization: Schema.optionalKey(Schema.Int.annotate({ description: 'GPU Device Utilization %, the busiest accelerator, absent where the driver states none' })),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    diskTime: Schema.Finite.annotate({ description: 'Seconds block storage drivers spent reading and writing' }),
    availableCapacityForImportantUsage: Schema.optionalKey(Schema.Finite.annotate({ description: 'Bytes the startup volume holds for important work' })),
}).annotate({ description: 'host_statistics processor and paging counters, block storage statistics, and GPU utilization' });
class Running extends Schema.Opaque<Running>()(
    Schema.Struct({
        active: Schema.Boolean,
        status: Status,
        usage: Schema.optionalKey(Usage.annotate({ description: 'On turning unresponsive' })),
        usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
        windows: Schema.Array(Window),
    }),
) {}
const failure = { reason: Schema.String.annotate({ description: 'Failure case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) };
const Failed = open(failure);
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u))
    .pipe(Schema.brand('ProcessInstance'))
    .annotate({ description: 'Process ID and kernel start time as <pid>@<start>, unequal for a later process reusing the ID' });
const DiagnosticReport = open({
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
    exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type' })),
    signal: Schema.optionalKey(Schema.String),
    termination: Schema.optionalKey(open({ namespace: Schema.String, code: Schema.optionalKey(Schema.Finite), indicator: Schema.optionalKey(Schema.String) }).annotate({ description: 'Kernel termination reason, CODESIGNING with Launch Constraint Violation for a refused launch' })),
});
const reportFields = {
    report: Schema.optionalKey(DiagnosticReport),
    reportFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report read failure case: timedOut after 60 seconds, io, or cancelled' })),
};
const exitFields = {
    detail: Schema.optionalKey(Schema.Array(Schema.Literals(['memory', 'codeSigning', 'decryption'])).annotate({ description: 'Kernel exit detail: memory limit, invalid code signature, failed decryption' })),
    reporting: Schema.optionalKey(Schema.Boolean.annotate({ description: 'A crash report is on its way' })),
    ...reportFields,
};
const Exited = Schema.Union([
    open({ _tag: Schema.tag('status'), status: Schema.Int.annotate({ description: 'Exit status' }), ...exitFields }),
    open({ _tag: Schema.tag('signal'), signal: Schema.Int.annotate({ description: 'Terminating signal number' }), ...exitFields }),
    open({ _tag: Schema.tag('statusFailure'), statusFailure: Schema.String.annotate({ description: 'Exit status read failure' }), ...reportFields }),
]).pipe(Schema.toTaggedUnion('_tag'));
const States = Schema.Union([Schema.TaggedStruct('running', Running.fields), open({ _tag: Schema.tag('failed'), ...failure }), Schema.TaggedStruct('exited', { termination: Exited })])
    .annotate({ description: 'Running with every window, failed read, or exited with its cause' })
    .pipe(Schema.toTaggedUnion('_tag'));
const InstanceState = Object.assign(States, {
    Failed,
    Exited,
    Status,
    asking: States.matchOrElse({ running: ({ windows }) => windows.filter(Struct.get('asking')) }, Array.empty),
    launching: States.matchOrElse({ running: ({ status }) => Status.guards.launching(status) }, Function.constFalse),
    reports: States.matchOrElse({ running: ({ active, status, windows }) => active || Status.guards.unresponsive(status) || windows.some(Struct.get('asking')) }, Function.constTrue),
});
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
        logs: Schema.Array(Schema.Struct({ file: Schema.String, text: Schema.String })).annotate({ description: 'Newest first across folders, by the JSON timestamp of a line, else file modification time' }),
        unified: Schema.Result(
            Schema.Array(open({ timestamp: Schema.String, processID: Schema.optionalKey(Pid), messageType: Schema.optionalKey(Schema.String), subsystem: Schema.optionalKey(Schema.String), category: Schema.optionalKey(Schema.String), eventMessage: Schema.optionalKey(Schema.String) })).annotate({
                description: 'log show events, newest first',
            }),
            Schema.ErrorInstance(),
        ),
        unreadable: Schema.Array(Schema.String).annotate({ description: 'Report files, log folders, and log lines that failed to read, with cause' }),
    });
}
const Route = Schema.TaggedUnion({ process: {}, window: { number: WindowID }, focused: { number: WindowID }, system: {} }).annotate({
    description: 'Route its events took: process posts keys to the key window, window adds the routing fields of a background window, focused brackets the window route with focus and defocus records, system posts at the HID tap to the frontmost process',
});
const Presentation = Schema.Union([
    Schema.TaggedStruct('behind', {}).annotate({ description: 'Menus opened behind the frontmost application, its key focus never leaving it' }),
    Schema.TaggedStruct('forward', {}).annotate({ description: 'Application brought forward to open its menu, as no route opened it behind, and left there' }),
]).pipe(Schema.toTaggedUnion('_tag'));
const Delivery = Schema.Struct({
    window: Schema.optionalKey(WindowID.annotate({ description: 'Window the action reached' })),
    route: Schema.optionalKey(Route.annotate({ description: 'Absent where the action posted no event' })),
    presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the menu the action opened was presented, absent where it opened none' })),
});
class Act extends Schema.Opaque<Act>()(
    Schema.Struct({
        actions: Schema.NonEmptyArray(
            Schema.Union([
                Schema.Struct({ press: Schema.Struct({ ref: Ref, action: Schema.String.annotate({ description: 'Element action, AXPress unless named, AXRaise refused' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed('AXPress')), Schema.withConstructorDefault(Effect.succeed('AXPress'))) }) }),
                Schema.Struct({ set: Schema.Struct({ ref: Ref, value: Value.annotate({ description: 'Written to AXValue as CFString, CFNumber, or CFBoolean and read back, an unchanged value falling through to the element menu or list, AXIncrement or AXDecrement, then focus and typed keys' }) }) }),
                Schema.Struct({ choose: Schema.Struct({ ref: Ref, item: Schema.String.annotate({ description: 'Title of a menu or list item, or value of a combo box, written and read back before its menu, list, or keys are tried' }) }) }),
                Schema.Struct({
                    key: Schema.Struct({
                        ...KeyStep.fields,
                        ref: Schema.optionalKey(Ref.annotate({ description: 'Element the key goes to: its window made main and the element focused through Accessibility, else a window-routed click into it' })),
                    }).annotate({ description: 'Posted to the instance process with no activation, the focused window settling it where no ref names the element' }),
                }),
            ]),
        ).annotate({ description: 'Run in order until one fails, then the instance turns idle once' }),
    }),
) {
    static readonly Result = Schema.Struct({
        outcomes: Schema.Array(Schema.Result(Delivery, Schema.ErrorInstance())).annotate({ description: 'One per action run, the first failure ending the run' }),
        state: States.annotate({ description: 'State once the acted windows stop asking, bounded at 2 heartbeats over which the instance neither pages nor waits for a processor' }),
        usage: Schema.optionalKey(Schema.Result(UsageInterval, Schema.ErrorInstance()).annotate({ description: 'Growth of the instance usage over the actions and its cause, else the read failure; absent for actions a linked source runs' })),
        load: Schema.optionalKey(Schema.Result(Load, Schema.ErrorInstance()).annotate({ description: 'System load over the actions, else the read failure; absent for actions a linked source runs' })),
        written: Schema.optionalKey(Written.annotate({ description: 'Documents the instance holds open, by AXDocument and the open command, that FSEvents saw written during the actions, else the watch failure; absent for actions a linked source runs' })),
        diagnostics: Schema.optionalKey(Diagnostics.Result.annotate({ description: 'Error and fault lines of the instance and its crash and hang reports since the actions began; absent for actions a linked source runs' })),
    });
}
const Launch = Schema.Struct({
    url: Schema.URL.annotate({ description: 'Bundle' }),
    documents: Schema.Array(Schema.URL)
        .pipe(Schema.withConstructorDefault(Effect.succeed([])))
        .annotate({ description: 'Files the application opens' }),
    arguments: Schema.Array(Schema.String)
        .pipe(Schema.withConstructorDefault(Effect.succeed([])))
        .annotate({ description: 'Process arguments' }),
    environment: Schema.Record(Schema.String, Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed({}))),
    new: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Beside running instances' }),
    opening: Schema.Record(Schema.String, AppleEventValue)
        .pipe(Schema.withConstructorDefault(Effect.succeed({})))
        .annotate({ description: 'Parameters of the open command by name or code, sent with the documents to the instance once it turns idle behind the frontmost application' }),
    wait: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))).annotate({ description: 'Events continue until the instance exits' }),
});
const QuitMode = Schema.Literals(['asking', 'saving', 'discarding', 'forcing']).annotate({ description: 'Save option the quit event states where the scripting definition declares one on quit, forcing ending the process with no event' });
class Session extends Schema.Opaque<Session>()(
    Schema.Struct({
        onConsole: Schema.Boolean.annotate({ description: 'kCGSessionOnConsoleKey, false while another user session holds the screen' }),
        locked: Schema.Boolean.annotate({ description: 'The lock screen shows' }),
        asleep: Schema.Boolean.annotate({ description: 'The main display sleeps' }),
        secureInput: Schema.Boolean.annotate({ description: 'A process holds secure event input' }),
        space: Schema.Int.annotate({ description: 'Active Space' }),
    }).annotate({ description: 'Login session and screens; view and act refuse a session off the console, a locked screen, or a sleeping display' }),
) {}
const Pressure = Schema.Struct({
    memory: Schema.Literals(['normal', 'warning', 'critical']).annotate({ description: 'kern.memorystatus_vm_pressure_level' }),
    available: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 100 })).annotate({ description: 'kern.memorystatus_level, percent of memory available' }),
    thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]).annotate({ description: 'ProcessInfo thermalState' }),
    lowPower: Schema.Boolean.annotate({ description: 'Low Power Mode' }),
    advisory: Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])])).annotate({
        description: 'IOCopySystemLoadAdvisoryDetailed levels by key: CombinedLevel, the system verdict on running optional work now, and the UserLevel, BatteryLevel, and ThermalLevel constraints',
    }),
}).annotate({ description: 'System pressure on subscribe and each memory, thermal, power, or load advisory change, the raw value for a level this build names no case for' });
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
class InstanceSnapshot extends Schema.Class<InstanceSnapshot, { readonly brand: unique symbol }>('InstanceSnapshot')({
    pid: Pid,
    instance: ProcessInstance,
    identifier: LaunchedInstance.fields.identifier,
    url: Schema.URL.annotate({ description: 'Bundle' }),
    activationPolicy: ActivationPolicy,
    language: Schema.optionalKey(Schema.String.annotate({ description: 'Interface language of the instance, on the reply of watch' })),
    state: InstanceState,
    listening: Schema.Array(Schema.Int).annotate({ description: 'TCP ports the process listens on, read at each state, empty unless running or on a failed read' }),
    listeningFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Listening ports read failure' })),
}) {}
const LaunchEvent = Schema.Union([Schema.TaggedStruct('launched', LaunchedInstance.fields), Schema.TaggedStruct('state', InstanceSnapshot.fields), Schema.TaggedStruct('opened', { reply: AppleEventValue }), open({ _tag: Schema.tag('failed'), ...failure })])
    .annotate({ description: 'Launch events: launched first, then states, the reply of the open command once sent, ending idle or asking' })
    .pipe(Schema.toTaggedUnion('_tag'));
const containerFields = { container: Ref, kind: ContainerKind };
const controlFields = { control: Ref, kind: Kind };
const Change = Schema.TaggedUnion({
    started: { identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), name: Schema.optionalKey(Schema.String) },
    opened: { ...containerFields, title: Schema.optionalKey(Schema.String) },
    closed: { ...containerFields, title: Schema.optionalKey(Schema.String) },
    asking: { ...containerFields, title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), answers: Schema.Array(Schema.Struct({ ref: Ref, label: Schema.optionalKey(Schema.String) }).annotate({ description: 'Action control answering the container, with its label' })) },
    focused: controlFields,
    value: { ...controlFields, fact: Fact },
    selection: { ...controlFields, evidence: Evidence },
    moved: { ...containerFields, frame: Frame },
    frontmost: { active: Schema.Boolean },
    hidden: { hidden: Schema.Boolean },
    status: { status: Status },
    exited: { termination: Exited },
}).annotate({ description: 'Change of a selected instance: started, container opened, closed, or asking, focus, value, or selection of a control, a moved container, frontmost, hidden, status, exited with its crash report' });
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: ProcessInstance, sequence: Schema.Int.annotate({ description: 'Per instance, rising with each change' }), at: Schema.String.annotate({ description: 'ISO 8601 time the change was read' }), change: Change })) {}
const Report = Schema.Struct({ instance: ProcessInstance, identifier: Schema.optionalKey(LaunchedInstance.fields.identifier), language: Schema.String.annotate({ description: 'Interface language of the instance' }), observation: Observation });
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    executable: Schema.OptionFromOptionalKey(Schema.URL.annotate({ description: 'Main executable of the bundle' })),
    identifier: LaunchedInstance.fields.identifier,
    name: Schema.String,
    bundleName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleName, the name system and crash dialogs show' })),
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.Boolean,
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
    appIntents: Schema.Array(
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
    ).annotate({ description: 'App Intents actions from the Metadata.appintents of the bundle, its extensions, and the framework packages they include, which Shortcuts and Siri run' }),
}) {
    static readonly runs =
        ({ url }: Pick<Installation, 'url'>) =>
        (instance: InstanceSnapshot) =>
            instance.url.href === url.href && !InstanceState.isAnyOf(['exited'])(instance.state);
}
const Listing = Schema.Struct({
    installations: Schema.Array(Installation),
    unreadable: Schema.Array(Failed).annotate({ description: 'Bundles that failed to read, each with its cause, the bundle path as subject' }),
});
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
    static readonly Result = Schema.Array(
        Schema.Result(
            Schema.Struct({
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
            }),
            Schema.ErrorInstance(),
        ),
    ).annotate({ description: 'One image per window of a windows source in its order, else one' });
}
const Identifiers = { identifiers: Schema.optionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: "Bundle identifiers of the instances monitored, each matching the server's applications, every application the server matches when absent" })) };
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const ApplicationRpcs = RpcGroup.make(
    Rpc.make('installations', {
        payload: {
            identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
            prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every application the server matches when both are absent' })),
        },
        success: Listing.annotate({ description: 'Applications folders and running regular applications, on subscribe, each Spotlight update, and each regular launch or exit' }),
        stream: true,
    }),
    Rpc.make('installation', { payload: { url: Schema.URL.annotate({ description: 'Application bundle, listed or not' }) }, success: Installation, error: Failure }),
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
    Rpc.make('observations', { payload: Identifiers, success: Report.annotate({ description: 'Each container read whose static facts differ from the last sent, or that opened again' }), stream: true }),
    Rpc.make('session', { success: Session.annotate({ description: 'On subscribe and each change of console, lock, display sleep, or active Space' }), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('pasteboard', {
        payload: { text: Schema.optionalKey(Schema.String.annotate({ description: 'Plain text written as the only item before the read' })) },
        success: Pasteboard.annotate({ description: 'General pasteboard once the write, if any, landed' }),
        error: Failure,
    }),
    Rpc.make('watch', { payload: { instance: ProcessInstance }, success: InstanceSnapshot.annotate({ description: 'State of the instance, its Accessibility events, states, and observations streamed from this call on' }), error: Failure }),
    Rpc.make('model', {
        payload: {
            instance: ProcessInstance,
            scope: Scope,
            known: Schema.optionalKey(Schema.Struct({ build: Build, containers: Schema.Array(Locator) }).annotate({ description: 'Containers the interface map holds as read in this build, which the read checks in place of reading them whole' })),
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
    Rpc.make('launch', { payload: Launch.fields, success: LaunchEvent, error: Failure, stream: true }),
    Rpc.make('act', { payload: { instance: ProcessInstance, ...Act.fields }, success: Act.Result, error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, mode: QuitMode.pipe(Schema.withConstructorDefault(Effect.succeed('asking'))) }, success: InstanceState.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('dictionary', { payload: { url: Schema.URL }, success: ScriptingDictionary, error: Failure }),
    Rpc.make('execute', {
        payload: {
            instance: ProcessInstance,
            arguments: Schema.Record(Schema.String, AppleEventValue)
                .annotate({ description: 'Parameters of the script command by name or code, direct for direct parameter, accepts reply values' })
                .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        },
        success: Schema.Struct({ reply: AppleEventValue.annotate({ description: 'Reply' }), written: Written }),
        error: Failure,
    }),
    Rpc.make('capture', { payload: { instance: ProcessInstance, ...Capture.fields }, success: Capture.Result, error: Failure }),
    Rpc.make('record', {
        payload: { instance: ProcessInstance, ...Capture.fields, seconds: Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds recorded, the windows the application opens meanwhile on that display included' }) },
        success: Schema.Struct({
            movie: Schema.Uint8Array.annotate({ description: 'Movie of the source over the requested seconds, from SCRecordingOutput' }),
            type: Schema.String.annotate({ description: 'Uniform type identifier of the movie' }),
        }),
        error: Failure,
    }),
    Rpc.make('scale', {
        payload: { image: Schema.Uint8Array.annotate({ description: 'Image bytes in any format ImageIO reads' }), longestSide: LongestSide.annotate({ description: 'Longest side in pixels, never upscaled' }) },
        success: Schema.Struct({
            png: Schema.Uint8Array.annotate({ description: 'PNG scaled to the longest side' }),
            width: Schema.Int.annotate({ description: 'Width in pixels' }),
            height: Schema.Int.annotate({ description: 'Height in pixels' }),
        }),
        error: Failure,
    }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    Act,
    AppleEventValue,
    ApplicationRpcs,
    Build,
    Capture,
    Condition,
    Container,
    ContainerKind,
    Control,
    Cursor,
    Diagnostics,
    Event,
    Fact,
    Failure,
    Frame,
    Installation,
    InstanceSnapshot,
    InstanceState,
    Instant,
    Kind,
    Launch,
    KeyStep,
    LaunchedInstance,
    LaunchEvent,
    Locator,
    LongestSide,
    Model,
    Observation,
    Opener,
    Pasteboard,
    Pid,
    Pointer,
    Pressure,
    ProcessInstance,
    QuitMode,
    Ref,
    Report,
    Running,
    Scope,
    ScriptingDictionary,
    Session,
    Shape,
    Value,
    WindowID,
    Written,
};
