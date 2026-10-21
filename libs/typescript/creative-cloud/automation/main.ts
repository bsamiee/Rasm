import { Unmatched } from '@rasm/search';
import { transformSync } from 'amaro';
import { Array, Cause, Context, Duration, Effect, identity, Layer, Match, Option, Record, Schedule, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.annotate({ description: 'Process of a running instance' });
const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CoreGraphics window number of a window, dialog, sheet, or menu; status, press, and screenshot share it' });
const ElementAction = Schema.Union([
    Schema.String.annotate({ description: 'Action the element lists: an enabled control label, AXCloseButton, AXCancelButton, or AXDefaultButton for a window button, AXCancel for a menu' }),
    Schema.Struct({ field: Schema.String.annotate({ description: 'Label of a control the element lists' }), value: Schema.String.annotate({ description: 'Text the control receives' }) }),
]);
const Status = Schema.TaggedUnion({
    responding: {
        elements: Schema.Array(
            Schema.Struct({
                id: WindowID,
                role: Schema.String.annotate({ description: 'Accessibility role: AXWindow, AXSheet, or AXMenu' }),
                subrole: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Accessibility subrole: AXStandardWindow, AXDialog, AXSystemDialog, or AXFloatingWindow' })),
                title: Schema.OptionFromOptionalKey(Schema.String),
                text: Schema.Array(Schema.String).annotate({ description: 'Static text the element shows' }),
                controls: Schema.Array(
                    Schema.Struct({
                        role: Schema.String.annotate({ description: 'Accessibility role of a text field, text area, combo box, pop-up button, check box, or radio button' }),
                        label: Schema.OptionFromOptionalKey(Schema.String),
                        value: Schema.OptionFromOptionalKey(Schema.Union([Schema.String, Schema.Finite, Schema.Boolean]).annotate({ description: 'Text of a field or pop-up button, or check box and radio button state: 0 off, 1 on, 2 mixed' })),
                    }),
                ),
                actions: Schema.Array(Schema.String).annotate({ description: 'Actions press accepts' }),
                unreadable: Schema.Array(
                    Schema.Struct({
                        reads: Schema.Array(Schema.String).annotate({ description: 'Accessibility attributes or actions the read attempted' }),
                        role: Schema.OptionFromOptionalKey(Schema.String),
                        label: Schema.OptionFromOptionalKey(Schema.String),
                        error: Schema.Finite.check(Schema.isInt32()).annotate({ description: 'AXError code the host returned' }),
                    }),
                ).annotate({ description: 'Parts of the element the host could not read, so text, controls, and actions can be incomplete' }),
            }),
        ).annotate({ description: 'Open windows, dialogs, sheets, and menus, front to back' }),
    },
    unresponsive: {},
}).annotate({ description: 'Open elements of a responding application, or unresponsive when it answered no accessibility request within the messaging timeout' });
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
    }).annotate({ description: 'Record of a descriptor type other than a plain record: a range, comparison, logical, or application record type' }),
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property type by name or four-character code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or four-character code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String.annotate({ description: 'File URL' }) }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Class of the objects specified' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Schema.suspend(() => AppleEventValue).annotate({ description: 'Selector data: a property name, an index or first, middle, last, any, every, a name, or an id' }),
        from: Schema.optionalKey(Schema.suspend(() => AppleEventValue).annotate({ description: 'Container specifier, null or absent for the application' })),
    }).annotate({ description: 'Object specifier' }),
    Schema.TaggedStruct('opaque', { type: Schema.String.annotate({ description: 'Four-character descriptor type' }), data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({
        description: 'Non-record descriptor no other form holds exactly, reused unchanged',
    }),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => AppleEventValue),
    ).annotate({ description: 'Plain record by property name or four-character keyword' }),
]);
const Capture = Schema.Struct({
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'One window to capture; absent captures every on-screen window of the application in one image' })),
    pixels: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side of the image in pixels, a capture larger than it scales down and a smaller one keeps native size' })),
});
const Press = Schema.Struct({ element: WindowID, actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Actions performed in order, stopping at the first refusal' }) });
const Execute = Schema.Struct({
    command: Schema.String.annotate({ description: 'Command name or code from the application dictionary' }),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Dictionary parameters by name; direct supplies the direct parameter. Reuse native reply values unchanged' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'File URLs the command writes, returned with the reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
});
class InstalledFace extends Schema.Class<InstalledFace, { readonly brand: unique symbol }>('InstalledFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.String }) {}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    instances: Schema.Array(Pid).annotate({ description: 'Processes of the running instances of the bundle' }),
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
            description: 'Each named artifact file with its media type, or why it could not be read',
        }),
    });

    // --- [SCRIPTS]
    static readonly runtimes = {
        'K2  dosc': { _tag: 'Linked', runtime: 'uxpscript' },
        miscDjxM: { _tag: 'Dictionary', runtime: 'extendscript' },
        miscdosc: { _tag: 'Dictionary', runtime: 'javascript' },
    } as const;
    static readonly Script = Schema.Struct({ runtime: Schema.Literals(Object.values(Installation.runtimes).map(Struct.get('runtime'))).annotate({ description: 'Runtime that ran the source' }), result: Schema.OptionFromOptionalKey(Schema.Json) });
    script<A extends object, E, R>(pid: number, source: string, linked: Option.Option<(stripped: string) => Effect.Effect<A, E, R>>) {
        const { url, name } = this;
        return Effect.gen(function* () {
            const automation = yield* Automation;
            const declared = (yield* automation.terms({ url })).command.map(Struct.get('code'));
            const [code, row] = yield* Effect.fromOption(
                Array.findFirst(Record.toEntries(Installation.runtimes), ([each]) => declared.includes(each)),
                () => new Cause.NoSuchElementError(`${name} declares no script command`),
            );
            const ran = yield* Match.valueTags(row, {
                Linked: () => {
                    const [open, close] = ['async function body() {\n', '\n}'];
                    return Effect.flatMap(
                        Effect.all([Effect.fromOption(linked, () => new Cause.IllegalArgumentError(`${name} runs ${row.runtime} through a linked extension, and none is linked`)), Effect.try(() => transformSync(`${open}${source}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length))]),
                        ([call, stripped]) => call(stripped),
                    );
                },
                Dictionary: () =>
                    automation
                        .execute({
                            pid,
                            command: code,
                            arguments: {
                                direct: `(function (body) {
    function encoded(value) {
        var parts = [], key, at;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        if (value instanceof Array) { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (value.typename !== undefined) return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    try {
        var value = new Function(decodeURIComponent(body)).call(this);
        return encodeURIComponent(encoded({ _tag: "Success", success: value === undefined ? {} : { result: value } }));
    } catch (error) {
        var known = error instanceof Error;
        var thrown = known ? { name: error.name, message: error.message } : { message: String(error) };
        if (known && typeof error.number === "number" && typeof error.line === "number") thrown.cause = { number: error.number, line: error.line };
        return encodeURIComponent(encoded({ _tag: "Failure", failure: thrown }));
    }
}).call(this, "${encodeURIComponent(source)}")`,
                            },
                            artifacts: [],
                        })
                        .pipe(
                            Effect.map(Struct.get('reply')),
                            Effect.flatMap(Schema.decodeUnknownEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(Installation.Script.mapFields(Struct.pick(['result'])), Schema.ErrorInstance()))))))),
                            Effect.flatMap(Effect.fromResult),
                        ),
            });
            return { ...ran, runtime: row.runtime };
        });
    }
}
const Requests = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'Installed applications at subscription and after each launch or exit of an instance, or why discovery failed' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('statuses', {
        success: Schema.Struct({ pid: Pid, status: Schema.Result(Status, Schema.ErrorInstance()) }).annotate({ description: 'Status of a running instance when observation starts and after its windows, dialogs, sheets, or menus changed, or why it can no longer be observed' }),
        stream: true,
    }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Schema.Struct({ pid: Pid.annotate({ description: 'Process of the instance once it finished launching' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('status', { payload: { pid: Pid }, success: Status, error: Schema.ErrorInstance() }),
    Rpc.make('press', { payload: { pid: Pid, ...Press.fields }, error: Unmatched.Refusal }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Installation.Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Kind, name: Schema.String }, success: Schema.String, error: Unmatched.Refusal }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Installation.Executed, error: Unmatched.Refusal }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Capture.fields }, success: Screenshot, error: Unmatched.Refusal }),
    Rpc.make('faces', { success: Schema.Array(InstalledFace) }),
    Rpc.make('face', { payload: { name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText resolves it' }) }, success: InstalledFace, error: Unmatched }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/main/Automation', {
    make: Effect.gen(function* () {
        const client = yield* RpcClient.make(Requests);
        const [first, rest] = yield* Stream.peel(client.installations(), Sink.head());
        const installations = yield* Effect.flatMap(
            Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before listing any application')),
            SubscriptionRef.make,
        );
        yield* Effect.forkScoped(Stream.runForEach(rest, (listing) => SubscriptionRef.set(installations, listing)));
        return { ...client, installations, installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult) };
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Automation, Capture, ElementAction, Execute, Installation, InstalledFace, Kind, Pid, Press, Screenshot, Status, WindowID };
