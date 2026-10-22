import { Unmatched } from '@rasm/search';
import { transformSync } from 'amaro';
import { Cause, Context, Duration, Effect, identity, Layer, Option, Schedule, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.annotate({ description: 'Process of a running instance' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CGWindowID that status elements report and screenshot takes' });
const ElementID = Schema.String.annotate({ description: 'Base64 Accessibility remote token of an element, absent once the element closes' });
const ElementAction = Schema.Union([
    Schema.Struct({ perform: Schema.Struct({ element: ElementID, action: Schema.String.annotate({ description: 'Accessibility action the element lists, except AXRaise' }) }) }),
    Schema.Struct({ set: Schema.Struct({ element: ElementID, value: Schema.String.annotate({ description: 'Text written to AXValue' }) }) }),
]);
const Fields = Schema.Record(Schema.String, Schema.Json);
const State = Schema.Union([
    Schema.Struct({
        running: Schema.Struct({
            status: Schema.Union([Schema.Struct({ launching: Fields }), Schema.Struct({ responding: Schema.StructWithRest(Schema.Struct({ elements: Schema.Array(Schema.StructWithRest(Schema.Struct({ requiresAnswer: Schema.Boolean }), [Fields])) }), [Fields]) }), Schema.Struct({ unresponsive: Fields })]),
        }),
    }),
    Schema.Struct({ failed: Fields }),
    Schema.Struct({ exited: Fields }),
]).annotate({
    description: 'Instance state: running with open windows, sheets, and menus while responding, launching until launch finishes, or unresponsive when it answers no Accessibility request within the messaging timeout; why a status read failed; or how it exited',
});
const RunningInstance = Schema.Struct({ pid: Pid, url: Schema.URL.annotate({ description: 'Bundle the instance runs' }), state: State });
const Screenshot = Schema.Struct({ image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | { readonly [term: string]: AppleEventValue };
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or an enumerator, class, date, or file URL where the parameter type takes one' }),
    Schema.Array(Schema.suspend(() => AppleEventValue)),
    Schema.TaggedStruct('record', {
        type: Schema.String.annotate({ description: 'Four-character descriptor type' }),
        fields: Schema.Record(
            Schema.String,
            Schema.suspend(() => AppleEventValue),
        ).annotate({ description: 'Fields by property name or four-character keyword' }),
    }).annotate({ description: 'Record of a descriptor type other than a plain record' }),
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or four-character code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or four-character code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Class of the objects specified' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Schema.suspend(() => AppleEventValue).annotate({ description: 'Key data the form selects by, index form taking first, middle, last, any, or every' }),
        from: Schema.optionalKey(Schema.suspend(() => AppleEventValue).annotate({ description: 'Container specifier, null or absent for the application' })),
    }).annotate({ description: 'Object specifier' }),
    Schema.TaggedStruct('opaque', { type: Schema.String.annotate({ description: 'Four-character descriptor type' }), data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({
        description: 'Descriptor no other form holds exactly, passed back unchanged',
    }),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => AppleEventValue),
    ).annotate({ description: 'Plain record by property name or four-character keyword' }),
]);
const Capture = Schema.Struct({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: "Window to capture, absent for the application's on-screen windows on the display showing most of them" })),
    pixels: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest image side in pixels, larger captures scale down and smaller ones keep native size' })),
});
const Perform = Schema.Struct({ actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Actions on status element ids, run in order until one fails' }) });
const Execute = Schema.Struct({
    command: Schema.String.annotate({ description: 'Command name or code from the application dictionary, get and set included' }),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Parameters by name or code, direct for the direct parameter, reply values reused unchanged' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'File URLs the command writes, returned with the reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
});
const FaceName = Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, case-insensitive' });
class AvailableFace extends Schema.Class<AvailableFace, { readonly brand: unique symbol }>('AvailableFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.URL }) {}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    scriptable: Schema.Boolean,
}) {
    // --- [DISCOVERY]
    static readonly located = (query: string) => Automation.use(({ installed }) => Effect.flatMap(installed, (rows) => Unmatched.single('installed Adobe application', Option.some(query))(rows, (row) => [row.name])));

    // --- [DICTIONARY]
    static readonly Terms = Schema.Struct(Struct.Record(Kind.literals, Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as the scripting definition writes it' }) })))).annotate({ description: 'Every declaration of the scripting dictionary by kind, name, and code' });

    // --- [EVENTS]
    static readonly Executed = Schema.Struct({
        reply: AppleEventValue,
        artifacts: Schema.Array(Schema.Result(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8ArrayFromBase64 }), Schema.ErrorInstance())).annotate({
            description: 'Each artifact file with its media type, or why reading it failed',
        }),
    });

    // --- [SCRIPTS]
    static readonly Script = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
}
const Requests = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'Installed applications at subscription and on each Spotlight update, or why discovery failed' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('instances', {
        success: Schema.Array(RunningInstance).annotate({ description: 'Running instances with states at subscription and after each change, an exited instance listed until the next change' }),
        stream: true,
    }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Applications Launch Services opens the file with, default first' }) }),
    Rpc.make('status', { payload: { pid: Pid }, success: State, error: Unmatched.Refusal }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Schema.Struct({ pid: Pid.annotate({ description: 'Process of the instance once it finished launching' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, error: Unmatched.Refusal }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Installation.Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Kind, name: Schema.String }, success: Schema.String, error: Unmatched.Refusal }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Installation.Executed, error: Unmatched.Refusal }),
    Rpc.make('script', {
        payload: { pid: Pid, source: Schema.String.annotate({ description: 'Source the dictionary script command runs' }) },
        success: AppleEventValue,
        error: Unmatched.Refusal,
    }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Capture.fields }, success: Screenshot, error: Unmatched.Refusal }),
    Rpc.make('faces', { success: Schema.Array(AvailableFace) }),
    Rpc.make('face', { payload: { name: FaceName }, success: AvailableFace, error: Unmatched }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/main/Automation', {
    make: Effect.gen(function* () {
        const client = yield* RpcClient.make(Requests);
        const held = Effect.fnUntraced(function* <A, E, R>(changes: Stream.Stream<A, E, R>, subject: string) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)),
                SubscriptionRef.make,
            );
            yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)));
            return latest;
        });
        const [installations, instances] = yield* Effect.all([held(client.installations(), 'installations'), held(client.instances(), 'instances')]);
        return {
            ...client,
            installations,
            instances,
            installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult),
            script: (pid: number, source: string) =>
                client
                    .script({
                        pid,
                        source: `(function (body) {
    function encoded(value) {
        var parts = [], key, at, kind;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        kind = Object.prototype.toString.call(value);
        if (kind === "[object Array]") { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (kind !== "[object Object]") return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    var value = eval("(function () {" + decodeURIComponent(body) + "\\n})").call(this);
    return value === undefined ? "" : encodeURIComponent(encoded(value));
}).call(this, "${encodeURIComponent(source)}")`,
                    })
                    .pipe(
                        Effect.flatMap((reply) => (reply === null || reply === '' ? Effect.succeedNone : Effect.asSome(Schema.decodeUnknownEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Json))))(reply)))),
                        Effect.map((result) => ({ result })),
                    ),
        };
    }),
}) {
    static readonly spawned = (executable: string) =>
        Layer.effect(Automation, Automation.make).pipe(
            Layer.provide(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
            Layer.provide([
                Layer.effect(Socket.Socket)(
                    Socket.fromTransformStream(
                        Effect.gen(function* () {
                            const requests = new TransformStream<Uint8Array, Uint8Array>();
                            const child = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(executable, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' })));
                            return { readable: Stream.toReadableStream(child.stdout), writable: requests.writable };
                        }).pipe(Effect.mapError((cause) => Socket.SocketError.make({ reason: Socket.SocketOpenError.make({ kind: 'Unknown', cause }) }))),
                    ),
                ),
                RpcSerialization.layerNdJsonRpc(),
            ]),
        );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const strip = (source: string) => {
    const [open, close] = ['async function body() {\n', '\n}'];
    return Effect.try(() => transformSync(`${open}${source}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Automation, AvailableFace, Capture, ElementAction, Execute, FaceName, Installation, Kind, Perform, Pid, Screenshot, State, strip, WindowID };
