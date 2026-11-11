import { Unmatched } from '@rasm/search';
import { Array, Effect, flow, Record, Schema, String, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue =
    | null
    | boolean
    | number
    | string
    | readonly AppleEventValue[]
    | { readonly _tag: 'record'; readonly type: string; readonly fields: { readonly [term: string]: AppleEventValue } }
    | { readonly _tag: 'specifier'; readonly want: string; readonly form: typeof Form.Type; readonly seld: AppleEventValue; readonly from?: AppleEventValue }
    | typeof Leaf.Type
    | { readonly _tag?: never; readonly [term: string]: AppleEventValue };

// --- [MODELS] --------------------------------------------------------------------------

const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
const Form = Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']);
const Leaf = Schema.Union([
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
    Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class' }),
        form: Form,
        seld: Nested.annotate({ description: 'Selection data, or first, middle, last, any, or every for index form' }),
        from: Schema.optionalKey(Nested.annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
    Leaf,
    Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)]).annotate({ description: 'Plain record by property name or code' }),
]);
const TermKind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Term = Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as scripting definition spells it' }) });
class Execute extends Schema.Class<Execute, { readonly brand: unique symbol }>('Execute')({
    command: Schema.String.annotate({ description: 'Dictionary command name or code, get and set included' }),
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
    { ...Struct.Record(TermKind.literals, Schema.Array(Term)), command: Schema.Array(Term.mapFields(Struct.assign({ parameters: Schema.Array(Term) }))) },
    { description: 'Every declaration by kind, name, and code, commands with their parameters' },
) {
    static readonly script = Effect.fnUntraced(function* ({ command }: Terms, { events, direct, declared = {} }: { readonly events: Array.NonEmptyReadonlyArray<string>; readonly direct: AppleEventValue; readonly declared?: { readonly [code: string]: AppleEventValue } }) {
        const { name, parameters } = yield* Effect.fromOption(
            Array.findFirst(events, (event) => Array.findFirst(command, flow(Struct.get('code'), String.endsWith(event)))),
            () => Unmatched.make({ subject: `command with event ${events.join(' or ')}`, candidates: Array.map(command, Struct.get('name')) }),
        );
        const codes = new Set(Array.map(parameters, Struct.get('code')));
        return Execute.make({ command: name, arguments: { direct, ...Record.filter(declared, (_, code) => codes.has(code)) } });
    });
}

const Undeclared = Schema.Record(Schema.String, Schema.Json);
const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Undeclared]);
const keyed = <const Key extends string, S extends Schema.Top>(key: Key, schema: S) => Schema.Struct({ _tag: Schema.tagDefaultOmit(key), ...Struct.Record([key], schema) });
const defaulted = <S extends Schema.Constraint & Schema.WithoutConstructorDefault>(schema: S, value: S['Encoded'] & S['~type.make.in']) => Schema.withConstructorDefault<Schema.withDecodingDefaultKey<S>>(Effect.succeed(value))(Schema.withDecodingDefaultKey<S>(Effect.succeed(value))(schema));
const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Running process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' });
const ElementID = Schema.String.annotate({ description: 'Base64 element id, valid while state lists its element' });
const Frame = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin and size in screen points' });
const Control = open({
    id: ElementID,
    role: Schema.String,
    label: Schema.optionalKey(Schema.String),
    attribute: Schema.optionalKey(Schema.String),
    value: Schema.optionalKey(Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])),
    enabled: Schema.Boolean,
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
    answers: Schema.optionalKey(open({ accept: Schema.optionalKey(Control), discard: Schema.optionalKey(Control), cancel: Schema.optionalKey(Control), buttons: Schema.Array(Control) }).annotate({ description: 'Controls answering the element, absent with nothing to press' })),
});
const Window = open({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Schema.Int,
    onScreen: Schema.Boolean,
    kind: Schema.Literals(['filePanel', 'sheet', 'alert', 'modal', 'system', 'menu', 'window', 'undescribed', 'blocking']).annotate({ description: 'Element kind, undescribed with no element, blocking for a new window on a blocked main thread' }),
    asking: defaulted(Schema.Boolean, false).annotate({ description: 'Requires an answer' }),
    presented: defaulted(Schema.Boolean, true).annotate({ description: 'False for a window the application made before it first responded and no element describes' }),
    element: Schema.optionalKey(Element),
}).annotate({ description: 'Window server row, asking rows first, system prompts naming the instance among them' });
const Running = open({
    active: Schema.Boolean,
    status: Schema.Literals(['launching', 'responding', 'unresponsive']),
    process: Schema.optionalKey(Schema.Literals(['running', 'runnable', 'sleeping', 'stopped']).annotate({ description: 'Main thread on unresponsive, running after time on a processor, runnable while waiting for one, sleeping while blocked, stopped by a signal' })),
    usage: Schema.optionalKey(open({ cpu: Schema.Finite.annotate({ description: 'CPU seconds' }), footprint: Schema.Finite.annotate({ description: 'Physical footprint bytes' }), threads: Schema.Int }).annotate({ description: 'On read and on turning unresponsive' })),
    usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
    menus: Schema.optionalKey(Schema.Array(Element)),
    windows: Schema.Array(Window),
});
const Failed = open({ reason: Schema.String.annotate({ description: 'Failure case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) });
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u))
    .pipe(Schema.brand('ProcessInstance'))
    .annotate({ description: 'Process ID and kernel start time as <pid>@<start>, unequal for a later process reusing the ID' });
const CrashReport = open({
    url: Schema.URL.annotate({ description: 'Crash report file' }),
    pid: Pid,
    instance: Schema.optionalKey(ProcessInstance),
    executable: Schema.optionalKey(Schema.String.annotate({ description: 'Executable path, redacted under /Users and /private/tmp' })),
    service: Schema.optionalKey(Schema.String.annotate({ description: 'launchd label of the job the process ran as' })),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type' })),
    signal: Schema.optionalKey(Schema.String),
    termination: Schema.optionalKey(open({ namespace: Schema.String, code: Schema.optionalKey(Schema.Finite), indicator: Schema.optionalKey(Schema.String) }).annotate({ description: 'Kernel termination reason, CODESIGNING with Launch Constraint Violation for a refused launch' })),
});
const reported = {
    report: Schema.optionalKey(CrashReport),
    reportFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report read failure case' })),
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
    asking: States.matchOrElse({ running: ({ running }) => running.windows.filter(Struct.get('asking')) }, Array.empty<typeof Window.Type>),
});
const Perform = Schema.Struct({
    actions: Schema.NonEmptyArray(
        Schema.Union([
            Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Element action except AXRaise' }) }) }),
            Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) }),
            Schema.Struct({ key: Schema.Struct({ process: Pid, code: Schema.Int.annotate({ description: 'Virtual key code posted to the process, for a blocking window, a busy host runs it once its loop resumes' }) }) }),
        ]),
    ).annotate({ description: 'Run in order until one fails' }),
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
}) {}
const Launching = Schema.Union([keyed('launched', Launched), keyed('state', RunningInstance), keyed('prompts', Schema.Array(RunningInstance)), keyed('failed', Failed)])
    .annotate({ description: 'Launch events: launched first, then states and system prompts, ending settled or asking' })
    .pipe(Schema.toTaggedUnion('_tag'));
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    executable: Schema.OptionFromOptionalKey(Schema.URL.annotate({ description: 'Main executable of the bundle' })),
    identifier: Launched.fields.identifier,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.Boolean,
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    activationPolicy: ActivationPolicy.annotate({ description: 'From LSBackgroundOnly and LSUIElement' }),
}) {
    static readonly runs = (installation: Installation) => (instance: RunningInstance) => instance.url.href === installation.url.href && !State.isAnyOf(['exited'])(instance.state);
}
const Lifecycle = Schema.Struct({
    agents: Schema.Array(open({ label: Schema.String, program: Schema.URL.annotate({ description: 'Executable launchd starts' }), plist: Schema.URL, keepAlive: Schema.Boolean })).annotate({ description: 'Contents/Library/LaunchAgents declarations of the named applications' }),
    processes: Schema.Array(open({ pid: Pid, instance: ProcessInstance, executable: Schema.optionalKey(Schema.String), replaced: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Process runs other code than the file at executable holds' })) })).annotate({
        description: 'Processes executing a file of the named bundles, the named executable, or the named instance',
    }),
    reports: Schema.Array(CrashReport).annotate({ description: 'Newest crash reports of the named instance, bundle identifier, or agent label' }),
}).annotate({ description: 'watch-apps processes row, a keepAlive agent with no process at program is not running and reports hold its cause' });
class Screenshot extends Schema.Class<Screenshot, { readonly brand: unique symbol }>('Screenshot')({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent captures each presented window' })),
    longestSide: Schema.OptionFromOptionalKey(LongestSide),
}) {
    static readonly Result = Schema.Record(Schema.String.annotate({ description: 'Window number' }), Schema.Result(Schema.URL.annotate({ description: 'PNG file written' }), Schema.ErrorInstance()));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Execute, Installation, Launch, Launched, Launching, Lifecycle, LongestSide, Perform, Pid, Pressure, ProcessInstance, RunningInstance, Screenshot, State, TermKind, Terms, WindowID };
