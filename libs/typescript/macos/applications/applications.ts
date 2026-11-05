import { Effect, Match, Predicate, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Cases<A> {
    readonly running: (running: typeof Running.Type) => A;
    readonly failed: (failed: typeof Failed.Type) => A;
    readonly exited: (exited: typeof Exited.Type) => A;
}

// --- [MODELS] --------------------------------------------------------------------------

const open = <const Fields extends Schema.Struct.Fields>(fields: Fields) => Schema.StructWithRest(Schema.Struct(fields), [Schema.Record(Schema.String, Schema.Json)]);
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
    process: Schema.optionalKey(Schema.Literals(['running', 'sleeping', 'stopped']).annotate({ description: 'Main thread state on unresponsive, sleeping while waiting, stopped by signal or debugger' })),
    usage: Schema.optionalKey(open({ cpu: Schema.Finite.annotate({ description: 'CPU seconds' }), footprint: Schema.Finite.annotate({ description: 'Physical footprint bytes' }), threads: Schema.Int }).annotate({ description: 'On read and on turning unresponsive' })),
    usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
    menus: Schema.optionalKey(Schema.Array(Element)),
    windows: Schema.Array(Window),
});
const Failed = open({ reason: Schema.String.annotate({ description: 'Failure case' }), subject: Schema.String, message: Schema.optionalKey(Schema.String) });
const Exited = open({
    status: Schema.optionalKey(Schema.Int),
    signal: Schema.optionalKey(Schema.Int),
    detail: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Exit cause flags' })),
    statusFailure: Schema.optionalKey(Schema.String),
    reporting: Schema.optionalKey(Schema.Boolean.annotate({ description: 'A crash report is on its way' })),
    report: Schema.optionalKey(open({ url: Schema.String.annotate({ description: 'Crash report file URL' }), exception: Schema.optionalKey(Schema.String) })),
    reportFailure: Schema.optionalKey(Schema.String),
});
const StateUnion = Schema.Union([Schema.Struct({ running: Running }), Schema.Struct({ failed: Failed }), Schema.Struct({ exited: Exited })]).annotate({ description: 'Running with every window, failed read, or exited with its cause' });
const match = <A>(state: typeof StateUnion.Type, cases: Cases<A>) =>
    Match.value(state).pipe(
        Match.when({ running: Match.defined }, ({ running }) => cases.running(running)),
        Match.when({ failed: Match.defined }, ({ failed }) => cases.failed(failed)),
        Match.when({ exited: Match.defined }, ({ exited }) => cases.exited(exited)),
        Match.exhaustive,
    );
const State = Object.assign(StateUnion, {
    Running,
    Failed,
    Exited,
    Window,
    match,
    asking: (state: typeof StateUnion.Type) => match<readonly (typeof Window.Type)[]>(state, { running: ({ windows }) => windows.filter(Struct.get('asking')), failed: () => [], exited: () => [] }),
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
const Pressure = Schema.Struct({ memory: Schema.Literals(['normal', 'warning', 'critical']), thermal: Schema.Literals(['nominal', 'fair', 'serious', 'critical']) }).annotate({ description: 'Kernel memory pressure level and thermal state' });
const Launched = Schema.Struct({
    pid: Pid,
    created: Schema.Boolean.annotate({ description: 'Launch started the instance, false when it reused a running one' }),
});
class RunningInstance extends Schema.Class<RunningInstance, { readonly brand: unique symbol }>('RunningInstance')({
    pid: Pid,
    identifier: Schema.String.annotate({ description: 'Bundle identifier' }),
    url: Schema.URL.annotate({ description: 'Bundle' }),
    activationPolicy: Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'Regular in Dock, accessory for menu bar or helper process' }),
    state: State,
}) {}
const Launching = Schema.Union([Schema.Struct({ launched: Launched }), Schema.Struct({ state: RunningInstance }), Schema.Struct({ prompts: Schema.Array(RunningInstance) }), Schema.Struct({ failed: Failed })]).annotate({
    description: 'Launch events: launched first, then states and system prompts, ending settled or asking',
});
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'CFBundleVersion' })),
    scriptable: Schema.Boolean,
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    agent: Schema.Boolean.annotate({ description: 'LSUIElement, runs without Dock icon' }),
}) {
    static readonly runs = (installation: Installation) => (instance: RunningInstance) => instance.url.href === installation.url.href && !Predicate.hasProperty(instance.state, 'exited');
}
class Screenshot extends Schema.Class<Screenshot, { readonly brand: unique symbol }>('Screenshot')({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent captures each presented window' })),
    longestSide: Schema.OptionFromOptionalKey(LongestSide),
}) {
    static readonly Result = Schema.Record(Schema.String.annotate({ description: 'Window number' }), Schema.Result(Schema.URL.annotate({ description: 'PNG file written' }), Schema.ErrorInstance()));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Installation, Launch, Launched, Launching, LongestSide, Perform, Pid, Pressure, RunningInstance, Screenshot, State, WindowID };
