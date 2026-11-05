import { Duration, Schema, SchemaTransformation, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Control extends Schema.Struct.Type<typeof controlFields> {
    readonly items?: readonly Control[];
}

// --- [MODELS] --------------------------------------------------------------------------

const Version = Schema.String.check(Schema.isPattern(/^\d+(?:\.\d+){0,3}$/u)).annotate({ description: 'major[.minor[.patch[.build]]]' });
const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Running process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const Seconds = Schema.Finite.pipe(Schema.decodeTo(Schema.Duration, SchemaTransformation.transform({ decode: Duration.seconds, encode: Duration.toSeconds })));
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' });
const ElementID = Schema.String.annotate({ description: 'Base64 element id, a remote token or a menu bar path, valid while status lists its element' });
const Frame = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin and size in screen points' });
const CoreGraphicsWindow = Schema.Struct({
    number: WindowID,
    title: Schema.optionalKey(Schema.String.annotate({ description: 'kCGWindowName, absent when unset or without Screen Recording grant' })),
    frame: Frame,
    layer: Schema.Int,
    onScreen: Schema.Boolean,
}).annotate({ description: 'CoreGraphics window, readable through capture' });
const controlFields = {
    id: ElementID,
    role: Schema.String,
    subrole: Schema.optionalKey(Schema.String),
    label: Schema.optionalKey(Schema.String),
    identifier: Schema.optionalKey(Schema.String),
    attribute: Schema.optionalKey(Schema.String),
    value: Schema.optionalKey(Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])),
    enabled: Schema.Boolean,
    actions: Schema.Array(Schema.String),
};
const Control: Schema.Codec<Control> = Schema.Struct({ ...controlFields, items: Schema.optionalKey(Schema.Array(Schema.suspend((): Schema.Codec<Control> => Control))) }).annotate({ identifier: 'Control' });
const elements = {
    elements: Schema.Array(
        Schema.Struct({
            id: ElementID,
            window: Schema.optionalKey(WindowID),
            role: Schema.String,
            subrole: Schema.optionalKey(Schema.String),
            modal: Schema.optionalKey(Schema.Boolean),
            identifier: Schema.optionalKey(Schema.String),
            description: Schema.optionalKey(Schema.String),
            title: Schema.optionalKey(Schema.String),
            document: Schema.optionalKey(Schema.URL.annotate({ description: 'AXDocument file' })),
            edited: Schema.optionalKey(Schema.Boolean.annotate({ description: 'AXEdited of close button' })),
            minimized: Schema.optionalKey(Schema.Boolean),
            actions: Schema.Array(Schema.String),
            requiresAnswer: Schema.Boolean.annotate({ description: 'Sheet, modal, or system dialog' }),
            detail: Schema.optionalKey(
                Schema.Struct({
                    frame: Schema.optionalKey(Frame),
                    main: Schema.optionalKey(Schema.Boolean),
                    focused: Schema.optionalKey(Schema.Boolean),
                    text: Schema.Array(Schema.String),
                    controls: Schema.Array(Control),
                    unreadable: Schema.Array(Schema.Struct({ reads: Schema.Array(Schema.String), role: Schema.optionalKey(Schema.String), label: Schema.optionalKey(Schema.String), error: Schema.Int.annotate({ description: 'AXError code' }) })),
                }).annotate({ description: 'Walked element contents' }),
            ),
        }),
    ),
    windows: Schema.Array(CoreGraphicsWindow).annotate({ description: 'Windows no element describes' }),
};
const usage = {
    usage: Schema.optionalKey(
        Schema.Struct({
            cpu: Seconds.annotate({ description: 'CPU time' }),
            footprint: Schema.ByteSizeFromNumber.annotate({ description: 'Physical footprint' }),
            threads: Schema.Int,
        }).annotate({ description: 'On read and on turning unresponsive' }),
    ),
    usageFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Usage read failure' })),
};
const Step = Schema.Struct({});
const next = {
    next: Schema.optionalKey(
        Schema.Union([
            Schema.Struct({ wait: Step }),
            Schema.Struct({ drive: Step }),
            Schema.Struct({
                answer: Schema.Struct({
                    id: ElementID,
                    buttons: Schema.Array(Schema.Struct({ id: ElementID, label: Schema.String })),
                    accept: Schema.optionalKey(ElementID),
                    discard: Schema.optionalKey(ElementID),
                    cancel: Schema.optionalKey(ElementID),
                }),
            }),
            Schema.Struct({ capture: Schema.Struct({ windows: Schema.Array(WindowID) }) }),
            Schema.Struct({ resume: Step }),
            Schema.Struct({ force: Step }),
            Schema.Struct({ grant: Step }),
            Schema.Struct({ report: Schema.Struct({ url: Schema.URL }) }),
        ]).annotate({ description: 'Action state calls for: wait, drive, answer element through one of its buttons, capture windows, resume stopped process, force quit, grant Accessibility, or read crash report' }),
    ),
};
const runningFields = {
    ...usage,
    hidden: Schema.optionalKey(Schema.Boolean),
    created: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Launch started the instance' })),
    ...next,
};
const launching = { windows: Schema.Array(CoreGraphicsWindow).annotate({ description: 'Every CoreGraphics window' }) };
const unresponsive = {
    process: Schema.Literals(['running', 'sleeping', 'stopped']).annotate({ description: 'Main thread state, sleeping while waiting, stopped by signal or debugger' }),
    windows: Schema.Array(CoreGraphicsWindow).annotate({ description: 'Every CoreGraphics window, modal alerts included' }),
};
const exit = {
    status: Schema.optionalKey(Schema.Int),
    signal: Schema.optionalKey(Schema.Int),
    detail: Schema.optionalKey(Schema.Array(Schema.Literals(['memory', 'codeSigning', 'decryption'])).annotate({ description: 'Exit cause flags' })),
    statusFailure: Schema.optionalKey(Schema.String),
    report: Schema.optionalKey(Schema.Struct({ url: Schema.URL, pid: Pid, bundleIdentifier: Schema.optionalKey(Schema.String), exception: Schema.optionalKey(Schema.String) })),
    reportFailure: Schema.optionalKey(Schema.String),
    ...next,
};
const failure = {
    reason: Schema.String.annotate({ description: 'Failure case' }),
    subject: Schema.String,
    message: Schema.optionalKey(Schema.String),
    pid: Schema.optionalKey(Pid.annotate({ description: 'Requested process not owning the element' })),
    error: Schema.optionalKey(Schema.Int.annotate({ description: 'AXError or errno code' })),
    attribute: Schema.optionalKey(Schema.String),
    reads: Schema.optionalKey(Schema.Array(Schema.String)),
    role: Schema.optionalKey(Schema.String),
    label: Schema.optionalKey(Schema.String),
    candidates: Schema.optionalKey(Schema.Array(Schema.String)),
    limit: Schema.optionalKey(Seconds),
    status: Schema.optionalKey(Schema.Int.annotate({ description: 'Apple event OSStatus' })),
    ...next,
};
const State = Schema.Union([
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ launching: Schema.Struct(launching) }), ...runningFields }) }).pipe(
        Schema.decodeTo(
            Schema.toType(Schema.TaggedStruct('Launching', { ...launching, ...runningFields })),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Launching' as const, ...running.status.launching, ...Struct.pick(running, Struct.keys(runningFields)) }),
                encode: (value) => ({ running: { status: { launching: Struct.pick(value, Struct.keys(launching)) }, ...Struct.pick(value, Struct.keys(runningFields)) } }),
            }),
        ),
    ),
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ responding: Schema.Struct(elements) }), ...runningFields }) }).pipe(
        Schema.decodeTo(
            Schema.toType(Schema.TaggedStruct('Responding', { ...elements, ...runningFields, answering: Schema.Boolean.annotate({ description: 'An element requires an answer' }) })),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Responding' as const, ...running.status.responding, answering: running.status.responding.elements.some(Struct.get('requiresAnswer')), ...Struct.pick(running, Struct.keys(runningFields)) }),
                encode: (value) => ({ running: { status: { responding: Struct.pick(value, Struct.keys(elements)) }, ...Struct.pick(value, Struct.keys(runningFields)) } }),
            }),
        ),
    ),
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ unresponsive: Schema.Struct(unresponsive) }), ...runningFields }) }).pipe(
        Schema.decodeTo(
            Schema.toType(Schema.TaggedStruct('Unresponsive', { ...unresponsive, ...runningFields })),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Unresponsive' as const, ...running.status.unresponsive, ...Struct.pick(running, Struct.keys(runningFields)) }),
                encode: (value) => ({ running: { status: { unresponsive: Struct.pick(value, Struct.keys(unresponsive)) }, ...Struct.pick(value, Struct.keys(runningFields)) } }),
            }),
        ),
    ),
    Schema.Struct({ failed: Schema.Struct(failure) }).pipe(Schema.decodeTo(Schema.toType(Schema.TaggedStruct('Failed', failure)), SchemaTransformation.transform({ decode: ({ failed }) => ({ _tag: 'Failed' as const, ...failed }), encode: (failed) => ({ failed: Struct.omit(failed, ['_tag']) }) }))),
    Schema.Struct({ exited: Schema.Struct(exit) }).pipe(Schema.decodeTo(Schema.TaggedStruct('Exited', exit), SchemaTransformation.transform({ decode: ({ exited }) => ({ _tag: 'Exited' as const, ...exited }), encode: (value) => ({ exited: Struct.omit(value, ['_tag']) }) }))),
])
    .annotate({ description: 'Elements and undescribed windows while responding, process state and every window when unresponsive past Accessibility timeout, failed on status read' })
    .pipe(Schema.toTaggedUnion('_tag'));
const Perform = Schema.Struct({
    actions: Schema.NonEmptyArray(
        Schema.Union([Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Element action except AXRaise' }) }) }), Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) })]),
    ).annotate({ description: 'Run in order until one fails' }),
});
const Launched = Schema.Struct({
    pid: Pid.annotate({ description: 'Process ID once launch finishes' }),
    created: Schema.Boolean.annotate({ description: 'Launch started the instance, false when it reused a running one' }),
});
class RunningInstance extends Schema.Class<RunningInstance, { readonly brand: unique symbol }>('RunningInstance')({
    pid: Pid,
    url: Schema.URL.annotate({ description: 'Bundle' }),
    activationPolicy: Schema.Literals(['regular', 'accessory', 'prohibited']).annotate({ description: 'Regular in Dock, accessory for menu bar or helper process' }),
    state: State,
}) {}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    scriptable: Schema.Boolean,
    scriptingDefinitionFile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'OSAScriptingDefinition file in bundle Resources' })),
    agent: Schema.Boolean.annotate({ description: 'LSUIElement, runs without Dock icon' }),
}) {
    static readonly runs = (installation: Installation) => (instance: RunningInstance) => instance.url.href === installation.url.href && !State.guards.Exited(instance.state);
}
class Screenshot extends Schema.Class<Screenshot, { readonly brand: unique symbol }>('Screenshot')({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent captures on-screen windows of display holding most of their area' })),
    longestSide: Schema.OptionFromOptionalKey(LongestSide),
}) {
    static readonly Result = Schema.Struct({ windows: Schema.Array(WindowID) });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { CoreGraphicsWindow, Installation, Launched, LongestSide, Perform, Pid, RunningInstance, Screenshot, State, Version };
