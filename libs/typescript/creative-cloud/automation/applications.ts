import { Schema, SchemaTransformation, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Control extends Schema.Struct.Type<typeof controlFields> {
    readonly items?: readonly Control[];
}

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.annotate({ description: 'Running process ID' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID' });
const LongestSide = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Limit in pixels' });
const ElementID = Schema.String.annotate({ description: 'Base64 remote token, absent once element closes' });
const Frame = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin and size in screen points' });
const Window = Schema.Struct({
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
    windows: Schema.Array(Window).annotate({ description: 'Windows no element describes' }),
};
const usage = {
    usage: Schema.optionalKey(
        Schema.Struct({ cpu: Schema.Finite.annotate({ description: 'CPU seconds' }), footprint: Schema.Int.annotate({ description: 'Physical footprint bytes' }), threads: Schema.Int }).annotate({
            description: 'On read and on turning unresponsive',
        }),
    ),
};
const unresponsive = {
    process: Schema.Literals(['running', 'sleeping', 'stopped']).annotate({ description: 'Main thread state, sleeping while waiting, stopped by signal or debugger' }),
    windows: Schema.Array(Window).annotate({ description: 'Every CoreGraphics window, modal alerts included' }),
};
const exit = {
    status: Schema.optionalKey(Schema.Int),
    signal: Schema.optionalKey(Schema.Int),
    statusFailure: Schema.optionalKey(Schema.String),
    report: Schema.optionalKey(Schema.Struct({ url: Schema.URL, pid: Pid, bundleIdentifier: Schema.optionalKey(Schema.String), exception: Schema.optionalKey(Schema.String) })),
    reportFailure: Schema.optionalKey(Schema.String),
};
const State = Schema.Union([
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ launching: Schema.Struct({}) }), ...usage }) }).pipe(
        Schema.decodeTo(
            Schema.TaggedStruct('Launching', usage),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Launching' as const, ...Struct.pick(running, ['usage']) }),
                encode: (launching) => ({ running: { status: { launching: {} }, ...Struct.pick(launching, ['usage']) } }),
            }),
        ),
    ),
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ responding: Schema.Struct(elements) }), ...usage }) }).pipe(
        Schema.decodeTo(
            Schema.TaggedStruct('Responding', { ...elements, ...usage }),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Responding' as const, ...running.status.responding, ...Struct.pick(running, ['usage']) }),
                encode: (responding) => ({ running: { status: { responding: Struct.pick(responding, ['elements', 'windows']) }, ...Struct.pick(responding, ['usage']) } }),
            }),
        ),
    ),
    Schema.Struct({ running: Schema.Struct({ status: Schema.Struct({ unresponsive: Schema.Struct(unresponsive) }), ...usage }) }).pipe(
        Schema.decodeTo(
            Schema.TaggedStruct('Unresponsive', { ...unresponsive, ...usage }),
            SchemaTransformation.transform({
                decode: ({ running }) => ({ _tag: 'Unresponsive' as const, ...running.status.unresponsive, ...Struct.pick(running, ['usage']) }),
                encode: (value) => ({ running: { status: { unresponsive: Struct.pick(value, ['process', 'windows']) }, ...Struct.pick(value, ['usage']) } }),
            }),
        ),
    ),
    Schema.Struct({ failed: Schema.Struct({ reason: Schema.String }) }).pipe(Schema.decodeTo(Schema.TaggedStruct('Failed', { reason: Schema.String }), SchemaTransformation.transform({ decode: ({ failed }) => ({ _tag: 'Failed' as const, ...failed }), encode: (failed) => ({ failed: Struct.omit(failed, ['_tag']) }) }))),
    Schema.Struct({ exited: Schema.Struct(exit) }).pipe(Schema.decodeTo(Schema.TaggedStruct('Exited', exit), SchemaTransformation.transform({ decode: ({ exited }) => ({ _tag: 'Exited' as const, ...exited }), encode: (value) => ({ exited: Struct.omit(value, ['_tag']) }) }))),
])
    .annotate({ description: 'Elements and undescribed windows while responding, process state and every window when unresponsive past Accessibility timeout, failed on status read' })
    .pipe(Schema.toTaggedUnion('_tag'));
const Perform = Schema.Struct({
    actions: Schema.NonEmptyArray(
        Schema.Union([Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Element action except AXRaise' }) }) }), Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) })]),
    ).annotate({ description: 'Run in order until one fails' }),
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
    static readonly runs = (installation: Installation) => (instance: RunningInstance) => instance.url.href === installation.url.href && !State.isAnyOf(['Exited'])(instance.state);
}
class Screenshot extends Schema.Class<Screenshot, { readonly brand: unique symbol }>('Screenshot')({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent captures every on-screen window on display showing most' })),
    longestSide: Schema.OptionFromOptionalKey(LongestSide),
}) {
    static readonly Result = Schema.Struct({ windows: Schema.Array(WindowID) });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Installation, LongestSide, Perform, Pid, RunningInstance, Screenshot, State, Window };
