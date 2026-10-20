import { Unmatched } from '@rasm/search';
import { transformSync } from 'amaro';
import { Array, Cause, Context, Duration, Effect, identity, Layer, Match, Option, pipe, Record, Schedule, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CoreGraphics window number of a window, dialog, sheet, or menu; status, press, and screenshot share it' });
const ElementAction = Schema.Union([
    Schema.String.annotate({ description: 'Action the element lists: an enabled control label, AXCloseButton for the close button, AXCancel for a menu' }),
    Schema.Struct({ field: Schema.String.annotate({ description: 'Label of a control the element lists' }), value: Schema.String.annotate({ description: 'Text the control receives' }) }),
]);
const Status = Schema.TaggedUnion({
    responding: {
        frontWindow: Schema.OptionFromOptionalKey(WindowID),
        elements: Schema.Array(
            Schema.Struct({
                id: WindowID,
                kind: Schema.Literals(['window', 'dialog', 'sheet', 'menu']),
                role: Schema.String.annotate({ description: 'Accessibility role' }),
                subrole: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Accessibility subrole' })),
                title: Schema.OptionFromOptionalKey(Schema.String),
                text: Schema.Array(Schema.String).annotate({ description: 'Static text the element shows' }),
                controls: Schema.Array(
                    Schema.Struct({
                        role: Schema.String.annotate({ description: 'Accessibility role of a text field, text area, combo box, pop-up button, check box, or radio button' }),
                        label: Schema.OptionFromOptionalKey(Schema.String),
                        value: Schema.OptionFromOptionalKey(Schema.Union([Schema.String, Schema.Literals([0, 1, 2])]).annotate({ description: 'Text of a field or pop-up button, or check box and radio button state: 0 off, 1 on, 2 mixed' })),
                    }),
                ),
                actions: Schema.Array(Schema.String).annotate({ description: 'Actions press accepts: an enabled control label, AXCloseButton for the close button, AXCancel for a menu' }),
            }),
        ),
    },
    unresponsive: {},
}).annotate({ description: 'Front window and open elements of a responding application, or unresponsive when it answered no accessibility request within the messaging timeout' });
const Rect = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin and size in points as [[x, y], [width, height]]' });
const Window = Schema.Struct({ id: WindowID, title: Schema.OptionFromOptionalKey(Schema.String), layer: Schema.Int, frame: Rect, onScreen: Schema.Boolean });
const ScreenshotImage = Schema.Struct({ data: Schema.Uint8ArrayFromBase64, mimeType: Schema.String, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
const Screenshot = Schema.Struct({ window: Window, windows: Schema.Array(Window), sdrImage: Schema.OptionFromOptionalKey(ScreenshotImage), hdrImage: Schema.OptionFromOptionalKey(ScreenshotImage) });
const ScreenshotOptions = Schema.Struct({
    target: Schema.OptionFromOptionalKey(Schema.Union([WindowID, Schema.Literal('application')]).annotate({ description: 'Window id, or application for every window of the application on the display holding its front window; absent for the front window' })),
    sourceRect: Schema.OptionFromOptionalKey(Rect.annotate({ description: 'Captured part of the content in points' })),
    destinationRect: Schema.OptionFromOptionalKey(Rect.annotate({ description: 'Part of the output image the capture fills, in pixels' })),
    maxDimension: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side of the image in pixels, a capture larger than it scales down and a smaller one keeps native size' })),
    dynamicRange: Schema.Literals(['sdr', 'hdr', 'bothSDRAndHDR'])
        .annotate({ description: 'Images ScreenCaptureKit returns: hdr and bothSDRAndHDR return one sdrImage holding 16-bit extended sRGB values above 1.0, and hdrImage stays absent for window and application captures' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed('sdr' as const))),
    contentType: Schema.String.annotate({ description: 'Uniform type identifier ImageIO writes: public.tiff keeps extended range, public.png, public.heic, and public.jpeg clip values above 1.0' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed('public.png'))),
});
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
class InstalledFace extends Schema.Class<InstalledFace, { readonly brand: unique symbol }>('InstalledFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, fullName: Schema.String, file: Schema.String }) {}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    instances: Schema.Array(Schema.Int).annotate({ description: 'Process ids of the running instances of the bundle' }),
    pid: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Process of a running instance calls address, required when the bundle runs more than one' })),
    scriptable: Schema.Boolean,
}) {
    // --- [DISCOVERY]
    static readonly located = (query: string) => Automation.use(({ installed }) => Effect.flatMap(SubscriptionRef.get(installed), (rows) => Unmatched.single('installed Adobe application', Option.some(query))(rows, (row) => [row.name])));

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
        uxpscript: {
            commands: [['do script', 'K2  dosc']],
            route: {
                _tag: 'Linked',
                stripped: (source: string) => {
                    const [open, close] = ['async function body() {\n', '\n}'];
                    return Effect.try(() => transformSync(`${open}${source}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length));
                },
            },
        },
        extendscript: {
            commands: [
                ['do javascript', 'miscDjxM'],
                ['DoScript', 'miscdosc'],
            ],
            route: {
                _tag: 'Dictionary',
                encoder: `function encoded(value) {
        var parts = [], key, at;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        if (value instanceof Array) { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (value.typename !== undefined) return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }`,
            },
        },
        acrobat: { commands: [['do script', 'miscdosc']], route: { _tag: 'Dictionary', encoder: 'JSON.stringify' } },
    } as const;
    static readonly Script = Schema.Struct({ runtime: Schema.Literals(Record.keys(Installation.runtimes)).annotate({ description: 'Runtime that ran the source' }), result: Schema.OptionFromOptionalKey(Schema.Json) });
    script<A extends object, E, R>(source: string, linked: Option.Option<(stripped: string) => Effect.Effect<A, E, R>>) {
        const { url, pid, name } = this;
        return Automation.use((client) => client.terms({ url })).pipe(
            Effect.flatMap(({ command: declared }) =>
                pipe(
                    Record.toEntries(Installation.runtimes),
                    Array.flatMap(([runtime, { commands, route }]) => Array.map(commands, ([command, code]) => ({ runtime, command, code, route }))),
                    Array.findFirst(({ command, code }) => declared.some((term) => term.name === command && term.code === code)),
                    Effect.fromOption(() => new Cause.NoSuchElementError(`${name} declares no script command`)),
                ),
            ),
            Effect.flatMap(({ command, runtime, route }) =>
                Match.valueTags(route, {
                    Linked: ({ stripped }) =>
                        Effect.fromOption(linked, () => new Cause.IllegalArgumentError(`${name}'s ${command} keeps no context between calls, and no extension is linked`)).pipe(
                            Effect.flatMap((call) => Effect.flatMap(stripped(source), call)),
                            Effect.map(Struct.assign({ runtime })),
                        ),
                    Dictionary: ({ encoder }) =>
                        Automation.use((client) =>
                            client.execute({
                                url,
                                pid,
                                command,
                                arguments: {
                                    direct: `(function (body, encoded) {
    try {
        var value = new Function(decodeURIComponent(body)).call(this);
        return encodeURIComponent(encoded({ _tag: "Success", success: value === undefined ? {} : { result: value } }));
    } catch (error) {
        var known = error instanceof Error;
        var thrown = known ? { name: error.name, message: error.message } : { message: String(error) };
        if (known && typeof error.number === "number" && typeof error.line === "number") thrown.cause = { number: error.number, line: error.line };
        return encodeURIComponent(encoded({ _tag: "Failure", failure: thrown }));
    }
}).call(this, "${encodeURIComponent(source)}", ${encoder})`,
                                },
                                artifacts: [],
                            }),
                        ).pipe(
                            Effect.flatMap(({ reply }) => Schema.decodeUnknownEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(Installation.Script.mapFields(Struct.pick(['result'])), Schema.ErrorInstance()))))))(reply)),
                            Effect.flatMap(Effect.fromResult),
                            Effect.map(Struct.assign({ runtime })),
                        ),
                }),
            ),
        );
    }
}
const Refusal = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Target = Struct.pick(Installation.fields, ['url', 'pid']);
const Requests = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Array(Installation).annotate({ description: 'Installed applications at subscription and after each launch or exit of an instance' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('statuses', {
        success: Schema.Struct({ pid: Schema.Int, status: Schema.Result(Status, Schema.ErrorInstance()) }).annotate({ description: 'Status of a running instance when observation starts and after its windows, dialogs, sheets, or menus changed, or why it can no longer be observed' }),
        stream: true,
    }),
    Rpc.make('launch', { payload: { url: Target.url }, success: Schema.Struct({ pid: Schema.Int.annotate({ description: 'Process of the instance once it finished launching' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('activate', { payload: Target, error: Refusal }),
    Rpc.make('status', { payload: Target, success: Status, error: Refusal }),
    Rpc.make('press', { payload: { ...Target, element: WindowID, actions: Schema.NonEmptyArray(ElementAction).annotate({ description: 'Actions performed in order, stopping at the first refusal' }) }, error: Refusal }),
    Rpc.make('terms', { payload: { url: Target.url }, success: Installation.Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Target.url, selection: Schema.Struct({ kind: Kind, name: Schema.String }) }, success: Schema.String, error: Refusal }),
    Rpc.make('execute', { payload: { ...Target, command: Schema.String, arguments: Schema.Record(Schema.String, AppleEventValue), artifacts: Schema.Array(Schema.URL) }, success: Installation.Executed, error: Refusal }),
    Rpc.make('screenshot', { payload: { ...Target, ...ScreenshotOptions.fields }, success: Screenshot, error: Refusal }),
    Rpc.make('faces', { success: Schema.Array(InstalledFace) }),
    Rpc.make('face', { payload: { name: Schema.String.annotate({ description: 'PostScript name, full name, or family for its default face, in any case, as CoreText resolves it' }) }, success: InstalledFace, error: Unmatched }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/main/Automation', {
    make: Effect.gen(function* () {
        const client = yield* RpcClient.make(Requests);
        const [first, rest] = yield* Stream.peel(client.installations(), Sink.head());
        const installed = yield* Effect.flatMap(
            Effect.fromOption(first, () => new Cause.NoSuchElementError('installations ended before listing any application')),
            SubscriptionRef.make,
        );
        yield* Effect.forkScoped(Stream.runForEach(rest, (rows) => SubscriptionRef.set(installed, rows)));
        return { ...Struct.omit(client, ['installations']), installed };
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

export { AppleEventValue, Automation, ElementAction, Installation, InstalledFace, Kind, Screenshot, ScreenshotImage, ScreenshotOptions, Status, WindowID };
