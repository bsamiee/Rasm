import { Unmatched } from '@rasm/search';
import { Array, Effect, Function, Schema, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue =
    | null
    | boolean
    | number
    | string
    | readonly AppleEventValue[]
    | TypedRecord
    | Specifier
    | { readonly _tag: 'type'; readonly name: string }
    | { readonly _tag: 'enumerator'; readonly name: string }
    | { readonly _tag: 'date'; readonly date: string }
    | { readonly _tag: 'file'; readonly url: string }
    | { readonly _tag: 'opaque'; readonly type: string; readonly data: string; readonly value?: string }
    | { readonly _tag?: never; readonly [term: string]: AppleEventValue };

// --- [MODELS] --------------------------------------------------------------------------

const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
class TypedRecord extends Schema.Opaque<TypedRecord, unknown>()(Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' })) {}
class Specifier extends Schema.Opaque<Specifier, unknown>()(
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Nested.annotate({ description: 'Selection data, or first, middle, last, any, or every for index form' }),
        from: Schema.optionalKey(Nested.annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
) {}
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or enumerator, class, date, or file URL per parameter type' }),
    Schema.Array(Nested),
    TypedRecord,
    Specifier,
    Schema.Union([
        Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or code' }),
        Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or code' }),
        Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
        Schema.TaggedStruct('file', { url: Schema.String }),
        Schema.TaggedStruct('opaque', { type: Code, data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({ description: 'Descriptor no other case decodes, sent back unchanged' }),
    ]),
    Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)]).annotate({ description: 'Plain record by property name or code' }),
]);
const TermKind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Term = Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as scripting definition spells it' }) });
const Command = Term.mapFields(
    Struct.assign({
        parameters: Schema.Array(Term.mapFields(Struct.assign({ list: Schema.Boolean.annotate({ description: 'Takes a list' }), enumerators: Schema.Array(Term).annotate({ description: 'Enumerators of the enumeration typing it' }) }))),
    }),
);
class Execute extends Schema.Class<Execute, { readonly brand: unique symbol }>('Execute')({
    command: Schema.optionalKey(Schema.String.annotate({ description: 'Dictionary command name or code, get and set included, absent for the script command of the dictionary' })),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Parameters by name or code, direct for direct parameter, accepts reply values' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'Files command writes, returned with reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly Artifact = Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array });
    static readonly Result = Schema.Struct({
        reply: AppleEventValue,
        artifacts: Schema.Array(Schema.Result(Execute.Artifact, Schema.ErrorInstance())).annotate({ description: 'File contents with media type, or read error' }),
    });
}
class Terms extends Schema.Class<Terms, { readonly brand: unique symbol }>('Terms')(
    {
        ...Struct.Record(TermKind.literals, Schema.Array(Term)),
        command: Schema.Array(Command),
        script: Schema.OptionFromOptionalKey(Command.annotate({ description: 'Command a direct text parameter runs as source: the one taking a list of text arguments, else the do script event' })),
    },
    { description: 'Every declaration by kind, name, and code, commands with their parameters, and the script command' },
) {}

const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String.check(Schema.makeFilter((key: string) => !Object.hasOwn(fields, key))), Schema.Json)]);
const keyed = <const Key extends string, S extends Schema.Top>(key: Key, schema: S) => Schema.Struct({ _tag: Schema.tagDefaultOmit(key), ...Struct.Record([key], schema) });
const defaulted = <S extends Schema.Constraint & Schema.WithoutConstructorDefault>(schema: S, value: S['Encoded'] & S['~type.make.in']) => Schema.withConstructorDefault<Schema.withDecodingDefaultKey<S>>(Effect.succeed(value))(Schema.withDecodingDefaultKey<S>(Effect.succeed(value))(schema));
const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Running process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' });
const ElementID = Schema.String.annotate({ description: 'Base64 element id from a state, event, or node read, valid while its element exists; menu item ids name a path of positions from the menu bar' });
const Frame = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin and size in screen points' });
const Value = Schema.Union([Schema.Boolean, Schema.Finite, Schema.String]);
const Unreadable = Schema.Struct({
    reads: Schema.Array(Schema.String.annotate({ description: 'Attribute or call refused' })),
    role: Schema.optionalKey(Schema.String),
    label: Schema.optionalKey(Schema.String),
    error: Schema.Int.annotate({ description: 'AXError code' }),
});
class Node extends Schema.Opaque<Node, unknown>()(
    Schema.Struct({
        id: ElementID,
        role: Schema.optionalKey(Schema.String.annotate({ description: 'AXRole, AXApplication for the application, AXMenuBar for the menu bar' })),
        subrole: Schema.optionalKey(Schema.String),
        label: Schema.optionalKey(Schema.String.annotate({ description: 'AXTitle, AXDescription, or the AXValue of its AXTitleUIElement' })),
        identifier: Schema.optionalKey(Schema.String.annotate({ description: 'AXIdentifier' })),
        value: Schema.optionalKey(Value.annotate({ description: 'AXValue, the visible text of a text area' })),
        settable: Schema.Boolean.annotate({ description: 'AXValue accepts a set action' }),
        enabled: Schema.optionalKey(Schema.Boolean),
        actions: Schema.Array(Schema.String.annotate({ description: 'Action names a press action takes, AXRaise refused' })),
        frame: Schema.optionalKey(Frame),
        count: Schema.Int.annotate({ description: 'Children, visible rows of tables and outlines' }),
        children: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<Node> => Node)).annotate({ description: 'Absent below the depth read' })),
        unreadable: Schema.Array(Unreadable),
    }).annotate({ description: 'Accessibility element with its children to the depth read, the application root holding its windows and menu bar' }),
) {}
const Control = open({
    id: ElementID,
    role: Schema.String,
    label: Schema.optionalKey(Schema.String),
    attribute: Schema.optionalKey(Schema.String),
    value: Schema.optionalKey(Value),
    enabled: Schema.Boolean,
});
const Answers = open({ accept: Schema.optionalKey(Control), discard: Schema.optionalKey(Control), cancel: Schema.optionalKey(Control), buttons: Schema.Array(Control) }).annotate({
    description: 'Controls answering the element, absent with nothing to press: accept, discard, and cancel as the element marks them, buttons every labeled one, each pressed through act',
});
const Element = open({
    id: ElementID,
    role: Schema.String,
    subrole: Schema.optionalKey(Schema.String),
    modal: Schema.optionalKey(Schema.Boolean),
    identifier: Schema.optionalKey(Schema.String),
    title: Schema.optionalKey(Schema.String),
    document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument file URL' })),
    edited: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXEdited of close button' })),
    detail: Schema.optionalKey(open({ text: Schema.Array(Schema.String), controls: Schema.Array(Control) }).annotate({ description: 'Walked element contents' })),
    answers: Schema.optionalKey(Answers),
});
const Kind = Schema.Literals(['filePanel', 'sheet', 'alert', 'modal', 'system', 'menu', 'window', 'undescribed', 'blocking']).annotate({ description: 'Element kind, undescribed with no element, blocking for a new window on a blocked main thread' });
const Window = open({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Schema.Int,
    onScreen: Schema.Boolean,
    kind: Kind,
    asking: defaulted(Schema.Boolean, false).annotate({ description: 'Requires an answer' }),
    presented: defaulted(Schema.Boolean, true).annotate({ description: 'False for a window the application made before it first responded and no element describes' }),
    element: Schema.optionalKey(Element),
}).annotate({ description: 'Window server row, asking rows first, system prompts naming the instance among them' });
const Status = Schema.Literals(['launching', 'responding', 'unresponsive']);
const ProcessStatus = Schema.Literals(['running', 'runnable', 'sleeping', 'stopped']).annotate({ description: 'Main thread on unresponsive, running after time on a processor, runnable while waiting for one, sleeping while blocked, stopped by a signal' });
const Running = open({
    active: Schema.Boolean,
    status: Status,
    process: Schema.optionalKey(ProcessStatus),
    usage: Schema.optionalKey(open({ cpu: Schema.Finite.annotate({ description: 'CPU seconds' }), footprint: Schema.Finite.annotate({ description: 'Physical footprint bytes' }), threads: Schema.Int }).annotate({ description: 'On read and on turning unresponsive' })),
    usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
    menus: Schema.optionalKey(Schema.Array(Element)),
    windows: Schema.Array(Window),
});
const Failed = open({ reason: Schema.String.annotate({ description: 'Failure case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) });
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u))
    .pipe(Schema.brand('ProcessInstance'))
    .annotate({ description: 'Process ID and kernel start time as <pid>@<start>, unequal for a later process reusing the ID' });
const DiagnosticReport = open({
    url: Schema.URLFromString.annotate({ description: 'Report file in ~/Library/Logs/DiagnosticReports or /Library/Logs/DiagnosticReports' }),
    kind: Schema.Literals(['crash', 'hang', 'diag', 'spin']).annotate({ description: 'crash for an .ips crash or .crash file, else the text report extension' }),
    date: Schema.String.annotate({ description: 'ISO 8601 file modification time' }),
    pid: Pid,
    instance: Schema.optionalKey(ProcessInstance),
    name: Schema.optionalKey(Schema.String.annotate({ description: 'Process name' })),
    executable: Schema.optionalKey(Schema.String.annotate({ description: 'Executable path, redacted under /Users and /private/tmp in crash reports' })),
    service: Schema.optionalKey(Schema.String.annotate({ description: 'launchd label of the job the process ran as' })),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    event: Schema.optionalKey(Schema.String.annotate({ description: 'Text report Event line: hang, cpu usage, disk writes, wakeups' })),
    exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type' })),
    signal: Schema.optionalKey(Schema.String),
    termination: Schema.optionalKey(open({ namespace: Schema.String, code: Schema.optionalKey(Schema.Finite), indicator: Schema.optionalKey(Schema.String) }).annotate({ description: 'Kernel termination reason, CODESIGNING with Launch Constraint Violation for a refused launch' })),
});
const reported = {
    report: Schema.optionalKey(DiagnosticReport),
    reportFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report read failure case: timedOut after 60 seconds, io, or cancelled' })),
};
const ended = {
    detail: Schema.optionalKey(Schema.Array(Schema.Literals(['memory', 'codeSigning', 'decryption'])).annotate({ description: 'Kernel exit detail: memory limit, invalid code signature, failed decryption' })),
    reporting: Schema.optionalKey(Schema.Boolean.annotate({ description: 'A crash report is on its way' })),
    ...reported,
};
const Exited = Schema.Union([
    open({ _tag: Schema.tagDefaultOmit('Status'), status: Schema.Int.annotate({ description: 'Exit status' }), ...ended }),
    open({ _tag: Schema.tagDefaultOmit('Signal'), signal: Schema.Int.annotate({ description: 'Terminating signal number' }), ...ended }),
    open({ _tag: Schema.tagDefaultOmit('Unread'), statusFailure: Schema.String.annotate({ description: 'Exit status read failure' }), ...reported }),
]).pipe(Schema.toTaggedUnion('_tag'));
const States = Schema.Union([keyed('running', Running), keyed('failed', Failed), keyed('exited', Exited)])
    .annotate({ description: 'Running with every window, failed read, or exited with its cause' })
    .pipe(Schema.toTaggedUnion('_tag'));
const State = Object.assign(States, {
    Running,
    Failed,
    Exited,
    Window,
    asking: States.matchOrElse({ running: ({ running }) => running.windows.filter(Struct.get('asking')) }, Array.empty),
    launching: States.matchOrElse({ running: ({ running }) => running.status === 'launching' }, Function.constFalse),
    reports: States.matchOrElse({ running: ({ running }) => running.active || running.status === 'unresponsive' || running.windows.some(Struct.get('asking')) }, Function.constTrue),
});
const KeyName = Schema.Literals(['return', 'enter', 'tab', 'space', 'delete', 'forwardDelete', 'escape', 'home', 'end', 'pageUp', 'pageDown', 'left', 'right', 'down', 'up', 'help', 'f1', 'f2', 'f3', 'f4', 'f5', 'f6', 'f7', 'f8', 'f9', 'f10', 'f11', 'f12']);
const Modifier = Schema.Literals(['command', 'shift', 'option', 'control', 'function']);
const Act = Schema.Struct({
    actions: Schema.NonEmptyArray(
        Schema.Union([
            Schema.Struct({ press: Schema.Struct({ element: ElementID, action: defaulted(Schema.String, 'AXPress').annotate({ description: 'Element action, AXPress unless named, AXRaise refused' }) }) }),
            Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Value.annotate({ description: 'Written to AXValue as CFString, CFNumber, or CFBoolean and read back, an unchanged value falling through to the element menu or list, AXIncrement or AXDecrement, then focus and typed keys' }) }) }),
            Schema.Struct({ choose: Schema.Struct({ element: ElementID, item: Schema.String.annotate({ description: 'Title of a menu or list item, or value of a combo box, written and read back before its menu, list, or keys are tried' }) }) }),
            Schema.Struct({
                key: Schema.Struct({
                    key: Schema.Union([KeyName, Schema.String.annotate({ description: 'One character, posted with the key that types it on the current layout' })]),
                    modifiers: defaulted(Schema.Array(Modifier), []).annotate({ description: 'Held while the key posts' }),
                }).annotate({ description: 'Posted to the instance process with no activation, the focused window settling it' }),
            }),
        ]),
    ).annotate({ description: 'Run in order until one fails, then the instance settles once' }),
});
const Acted = Schema.Struct({
    outcomes: Schema.Array(Schema.Result(Schema.NullOr(WindowID).annotate({ description: 'Window the action reached' }), Schema.ErrorInstance())).annotate({ description: 'One per action run, the first failure ending the run' }),
    state: States.annotate({ description: 'State once the acted windows stop asking, bounded at 2 heartbeats' }),
});
const Launch = Schema.Struct({
    url: Schema.URL.annotate({ description: 'Bundle' }),
    documents: defaulted(Schema.Array(Schema.URL), []).annotate({ description: 'Files the application opens' }),
    arguments: defaulted(Schema.Array(Schema.String), []).annotate({ description: 'Process arguments' }),
    environment: defaulted(Schema.Record(Schema.String, Schema.String), {}),
    new: defaulted(Schema.Boolean, false).annotate({ description: 'Beside running instances' }),
});
const Pressure = Schema.Struct({ memory: Schema.Literals(['normal', 'warning', 'critical']), thermal: Schema.Literals(['nominal', 'fair', 'serious', 'critical', 'unknown']) }).annotate({ description: 'Kernel memory pressure level and thermal state, unknown for a state this build names no case for' });
const ActivationPolicy = Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'Regular in Dock, accessory for menu bar or helper process, prohibited for background only' });
const Launched = Schema.Struct({
    pid: Pid,
    instance: ProcessInstance,
    identifier: Schema.String.annotate({ description: 'Bundle identifier' }),
    created: Schema.Boolean.annotate({ description: 'Launch started the instance, false when it reused a running one' }),
    replaced: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Reused instance runs code replaced on disk since it started' })),
});
class RunningInstance extends Schema.Class<RunningInstance, { readonly brand: unique symbol }>('RunningInstance')({
    pid: Pid,
    instance: ProcessInstance,
    identifier: Launched.fields.identifier,
    url: Schema.URL.annotate({ description: 'Bundle' }),
    activationPolicy: ActivationPolicy,
    state: State,
    listening: Schema.Array(Schema.Int).annotate({ description: 'TCP ports the process listens on, read at each state, empty unless running or on a failed read' }),
    listeningFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Listening ports read failure' })),
}) {}
const Launching = Schema.Union([keyed('launched', Launched), keyed('state', RunningInstance), keyed('prompts', Schema.Array(RunningInstance)), keyed('failed', Failed)])
    .annotate({ description: 'Launch events: launched first, then states and system prompts, ending settled or asking' })
    .pipe(Schema.toTaggedUnion('_tag'));
const happened = {
    instance: ProcessInstance,
    pid: Pid,
    at: Schema.String.annotate({ description: 'ISO 8601 time the change was read' }),
};
const Shown = open({
    number: WindowID,
    title: Schema.optionalKey(Schema.String),
    document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument file URL' })),
    subrole: Schema.optionalKey(Schema.String.annotate({ description: 'AXSubrole, AXStandardWindow for a document window' })),
    kind: Kind,
});
class Dialog extends Schema.Opaque<Dialog, unknown>()(
    open({
        window: WindowID,
        element: Schema.optionalKey(ElementID),
        kind: Kind,
        title: Schema.optionalKey(Schema.String),
        text: Schema.Array(Schema.String),
        answers: Schema.optionalKey(Answers),
    }),
) {}
const Event = Schema.Union([
    open({ _tag: Schema.tagDefaultOmit('started'), ...happened, started: Schema.Literal(true), identifier: Schema.optionalKey(Launched.fields.identifier), name: Schema.optionalKey(Schema.String) }),
    open({ _tag: Schema.tagDefaultOmit('opened'), ...happened, opened: Shown }),
    open({ _tag: Schema.tagDefaultOmit('closed'), ...happened, closed: Shown }),
    open({ _tag: Schema.tagDefaultOmit('asking'), ...happened, asking: Dialog }),
    open({ _tag: Schema.tagDefaultOmit('answered'), ...happened, answered: Dialog }),
    open({ _tag: Schema.tagDefaultOmit('status'), ...happened, status: Status, process: Schema.optionalKey(ProcessStatus) }),
    open({ _tag: Schema.tagDefaultOmit('exited'), ...happened, exited: Exited }),
])
    .annotate({ description: 'Change of a selected instance: started, window opened or closed, dialog asking or answered, status, exited with its crash report' })
    .pipe(Schema.toTaggedUnion('_tag'));
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    executable: Schema.OptionFromOptionalKey(Schema.URL.annotate({ description: 'Main executable of the bundle' })),
    identifier: Launched.fields.identifier,
    name: Schema.String,
    bundleName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleName, the name system and crash dialogs show' })),
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.Boolean,
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    activationPolicy: ActivationPolicy.annotate({ description: 'From LSBackgroundOnly and LSUIElement' }),
}) {
    static readonly runs =
        ({ url }: Pick<Installation, 'url'>) =>
        (instance: RunningInstance) =>
            instance.url.href === url.href && !State.isAnyOf(['exited'])(instance.state);
}
class Capture extends Schema.Class<Capture, { readonly brand: unique symbol }>('Capture')({
    source: Schema.OptionFromOptionalKey(
        Schema.Union([Schema.Struct({ windows: Schema.Struct({ numbers: Schema.NonEmptyArray(WindowID) }) }), Schema.Struct({ element: Schema.Struct({ id: ElementID.annotate({ description: 'Element cropped from its window' }) }) })]).annotate({
            description: 'Windows by number or one element, each presented window when absent',
        }),
    ),
    longestSide: Schema.OptionFromOptionalKey(LongestSide.annotate({ description: 'Longest side in pixels of the inline preview, absent for none' })),
}) {
    static readonly Result = Schema.Record(
        Schema.String.annotate({ description: 'Window number' }),
        Schema.Result(
            Schema.Struct({
                file: Schema.String.annotate({ description: 'Full-resolution PNG file URL in the output folder captures of the application' }),
                width: Schema.Int.annotate({ description: 'File width in pixels' }),
                height: Schema.Int.annotate({ description: 'File height in pixels' }),
                document: Schema.optionalKey(Schema.String.annotate({ description: 'AXDocument file URL of the window' })),
                preview: Schema.OptionFromOptionalKey(Schema.Uint8Array.annotate({ description: 'PNG scaled to the longest side' })),
            }),
            Schema.ErrorInstance(),
        ),
    );
}
class Diagnostics extends Schema.Class<Diagnostics, { readonly brand: unique symbol }>('Diagnostics')({
    application: Schema.optionalKey(Schema.Struct({ url: Schema.URL.annotate({ description: 'Bundle of the installation' }), instance: Schema.optionalKey(ProcessInstance) }).annotate({ description: 'Reports and unified log of the instance, else of the installation executable; absent reads the server itself' })),
    folders: defaulted(Schema.Array(Schema.Struct({ url: Schema.URL.annotate({ description: 'Folder of log files' }), glob: Schema.optionalKey(Schema.String.annotate({ description: 'fnmatch pattern of file names, every file when absent' })) })), []),
    since: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds back from now, else since the instance launched' })),
    limit: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Most reports, log lines, and unified log events returned' }),
}) {
    static readonly Result = Schema.Struct({
        reports: Schema.Array(DiagnosticReport).annotate({ description: 'Newest first' }),
        logs: Schema.Array(Schema.Struct({ file: Schema.String, text: Schema.String })).annotate({ description: 'Newest first across folders, by the JSON timestamp of a line, else file modification time' }),
        unified: Schema.Result(
            Schema.Array(open({ timestamp: Schema.String, processID: Schema.optionalKey(Pid), messageType: Schema.optionalKey(Schema.String), subsystem: Schema.optionalKey(Schema.String), category: Schema.optionalKey(Schema.String), eventMessage: Schema.optionalKey(Schema.String) })).annotate({
                description: 'log show events, newest first',
            }),
            Schema.ErrorInstance(),
        ),
        unreadable: Schema.Array(Schema.String).annotate({ description: 'Report files and log folders that failed to read, with cause' }),
    });
}
const Identifiers = { identifiers: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Bundle identifiers of the instances monitored, each admitted by the server' }) };
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Automation = RpcGroup.make(
    Rpc.make('installations', {
        payload: {
            identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
            prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every admitted application when both are absent' })),
        },
        success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }),
        stream: true,
    }),
    Rpc.make('instances', { payload: Identifiers, success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('events', { payload: Identifiers, success: Event, stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('nodes', {
        payload: {
            instance: ProcessInstance,
            root: Schema.optionalKey(ElementID.annotate({ description: 'Element read, the application when absent' })),
            depth: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Child levels read below the root' }),
        },
        success: Node,
        error: Failure,
    }),
    Rpc.make('launch', { payload: Launch.fields, success: Launching, error: Failure, stream: true }),
    Rpc.make('act', { payload: { instance: ProcessInstance, ...Act.fields }, success: Acted, error: Failure }),
    Rpc.make('quit', { payload: { instance: ProcessInstance, force: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))) }, success: State.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Failure }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: TermKind, name: Schema.String }, success: Schema.String, error: Failure }),
    Rpc.make('execute', { payload: { instance: ProcessInstance, ...Execute.fields }, success: Execute.Result, error: Failure }),
    Rpc.make('capture', { payload: { instance: ProcessInstance, ...Capture.fields }, success: Capture.Result, error: Failure }),
    Rpc.make('preview', {
        payload: { file: Schema.URL.annotate({ description: 'PNG file' }), longestSide: LongestSide.annotate({ description: 'Longest side in pixels of the preview, never upscaled' }) },
        success: Schema.Struct({
            width: Schema.Int.annotate({ description: 'File width in pixels' }),
            height: Schema.Int.annotate({ description: 'File height in pixels' }),
            preview: Schema.Uint8Array.annotate({ description: 'PNG scaled to the longest side' }),
        }),
        error: Failure,
    }),
    Rpc.make('diagnostics', { payload: Diagnostics.fields, success: Diagnostics.Result, error: Failure }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Act, Acted, AppleEventValue, Capture, Automation, Diagnostics, type Dialog, ElementID, Event, Execute, Installation, Launch, Launched, Launching, LongestSide, Node, Pid, Pressure, ProcessInstance, RunningInstance, State, TermKind, Terms, WindowID };
