import { Unmatched } from '@rasm/search';
import { transformSync } from 'amaro';
import { Array, Cause, Context, Effect, FileSystem, identity, Layer, Match, Option, pipe, Record, Schedule, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, type RpcClientError, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const WindowID = Schema.Finite.check(Schema.isUint32()).annotate({ description: 'CoreGraphics window number of a window, dialog, sheet, or menu; status, press, and screenshot share it' });
const Status = Schema.Struct({
    responding: Schema.Boolean.annotate({ description: 'False when the application answered no accessibility request within the messaging timeout' }),
    frontWindow: Schema.OptionFromOptionalKey(WindowID),
    elements: Schema.Array(
        Schema.Struct({
            id: WindowID,
            kind: Schema.Literals(['window', 'dialog', 'sheet', 'menu']),
            role: Schema.String,
            title: Schema.OptionFromOptionalKey(Schema.String),
            text: Schema.Array(Schema.String).annotate({ description: 'Static text the element shows' }),
            controls: Schema.Array(
                Schema.Union([
                    Schema.Struct({ text: Schema.Struct({ kind: Schema.Literals(['field', 'popUp']), label: Schema.OptionFromOptionalKey(Schema.String), value: Schema.OptionFromOptionalKey(Schema.String) }) }),
                    Schema.Struct({ checkBox: Schema.Struct({ label: Schema.OptionFromOptionalKey(Schema.String), on: Schema.Boolean }) }),
                ]),
            ).annotate({ description: 'Text fields and pop-up buttons with their text, check boxes and radio buttons with their state' }),
            actions: Schema.Array(Schema.String).annotate({ description: 'Actions press accepts: an enabled control label, AXCloseButton for the close button, AXCancel for a menu' }),
        }),
    ),
});
const Window = Schema.Struct({ id: WindowID, title: Schema.OptionFromOptionalKey(Schema.String) });
const Screenshot = Schema.Struct({ window: Window, windows: Schema.Array(Window), image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
type Native = null | boolean | number | string | readonly Native[] | { readonly [term: string]: Native };
const Native: Schema.Codec<Native> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or an enumerator, class, date, or file URL where the parameter type takes one' }),
    Schema.Array(Schema.suspend(() => Native)),
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property type by name or four-character code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or four-character code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String.annotate({ description: 'File URL' }) }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Class of the objects specified' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Schema.suspend(() => Native).annotate({ description: 'Selector data: a property name, an index or first, middle, last, any, every, a name, or an id' }),
        from: Schema.optionalKey(Schema.suspend(() => Native).annotate({ description: 'Container specifier, null or absent for the application' })),
    }).annotate({ description: 'Object specifier' }),
    Schema.TaggedStruct('opaque', { type: Schema.String.annotate({ description: 'Four-character descriptor type' }), data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({
        description: 'Descriptor the dictionary does not model, reused unchanged',
    }),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => Native),
    ).annotate({ description: 'Record by property name' }),
]);
class InstalledFace extends Schema.Class<InstalledFace, { readonly brand: unique symbol }>('InstalledFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, fullName: Schema.String, file: Schema.String }) {
    static get installed() {
        return Automation.use((client) => client.faces());
    }
    static readonly named = (name: string) => Automation.use((client) => client.face({ name }));

    source() {
        return Effect.map(
            FileSystem.FileSystem.use((files) => files.readFile(this.file)),
            (data) => ({ data, postscriptName: Option.some(this.postscriptName) }),
        );
    }
}
class Installation extends Schema.Class<Installation, { readonly brand: unique symbol }>('Installation')({
    url: Schema.URL,
    identifier: Schema.String,
    name: Schema.String,
    version: Schema.OptionFromOptionalKey(Schema.String),
    channel: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Prerelease channel the bundle declares or its name states, absent for a release' })),
    running: Schema.OptionFromOptionalKey(Schema.Struct({ pid: Schema.Int, finishedLaunching: Schema.Boolean })),
    scriptable: Schema.Boolean,
}) {
    // --- [DISCOVERY]
    static get installed() {
        return Automation.use((client) => client.installations());
    }
    static readonly located = (query: string) => Effect.flatMap(Installation.installed, (rows) => Unmatched.single('installed Adobe application', Option.some(query))(rows, (row) => [row.url.href, row.identifier, row.name]));

    // --- [LIFECYCLE]
    launch() {
        return Automation.use((client) => client.launch({ application: this.url }));
    }
    status() {
        return Automation.use((client) => client.status({ application: this.url }));
    }
    watch() {
        return Stream.unwrap(Effect.map(Automation, (client) => client.watch({ application: this.url })));
    }

    // --- [ELEMENTS]
    press(element: typeof WindowID.Type, action: string) {
        return Automation.use((client) => client.press({ application: this.url, element, action }));
    }
    screenshot(window: Option.Option<typeof WindowID.Type>) {
        return Automation.use((client) => client.screenshot({ application: this.url, window }));
    }

    // --- [DICTIONARY]
    definition(selection: Option.Option<{ readonly kind: typeof Kind.Type; readonly name: string }>) {
        return Automation.use((client) => client.definition({ application: this.url, selection }));
    }
    terms(kind: typeof Kind.Type) {
        return Automation.use((client) => client.terms({ application: this.url, kind }));
    }

    // --- [EVENTS]
    static readonly Command = Schema.Struct({
        reply: Native,
        artifacts: Schema.Array(Schema.Result(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8ArrayFromBase64 }), Schema.ErrorInstance())).annotate({
            description: 'Each named artifact file with its media type, or why it could not be read',
        }),
    });
    command(name: string, parameters: Readonly<Record<string, Native>>, artifacts: readonly URL[]) {
        return Automation.use((client) => client.execute({ application: this.url, command: name, arguments: parameters, artifacts }));
    }

    // --- [SCRIPTS]
    static readonly runtimes = {
        uxpscript: {
            commands: [['do script', 'K2  dosc']],
            route: {
                _tag: 'Linked',
                stripped: (code: string) => {
                    const [open, close] = ['async function body() {\n', '\n}'];
                    return Effect.try(() => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length));
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
    runtime() {
        return Effect.flatMap(this.terms('command'), (declared) =>
            pipe(
                Record.toEntries(Installation.runtimes),
                Array.flatMap(([runtime, { commands, route }]) => Array.map(commands, ([command, code]) => ({ runtime, command, code, route }))),
                Array.findFirst(({ command, code }) => declared.some((term) => term.name === command && term.code === code)),
                Effect.fromOption(() => new Cause.NoSuchElementError(`${this.name} declares no script command`)),
            ),
        );
    }
    script<A extends object, E, R>(code: string, linked: Option.Option<(source: string) => Effect.Effect<A, E, R>>) {
        const installation = this;
        return Effect.gen(function* () {
            const { command, runtime, route } = yield* installation.runtime();
            return yield* Match.valueTags(route, {
                Linked: ({ stripped }) =>
                    Effect.fromOption(linked, () => new Cause.IllegalArgumentError(`${installation.name}'s ${command} keeps no context between calls, and no extension is linked`)).pipe(
                        Effect.flatMap((call) => Effect.flatMap(stripped(code), call)),
                        Effect.map(Struct.assign({ runtime })),
                    ),
                Dictionary: ({ encoder }) =>
                    installation
                        .command(
                            command,
                            {
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
}).call(this, "${encodeURIComponent(code)}", ${encoder})`,
                            },
                            [],
                        )
                        .pipe(
                            Effect.flatMap(({ reply }) => Schema.decodeUnknownEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(Installation.Script.mapFields(Struct.pick(['result'])), Schema.ErrorInstance()))))))(reply)),
                            Effect.flatMap(Effect.fromResult),
                            Effect.map(Struct.assign({ runtime })),
                        ),
            });
        });
    }
}
const Requests = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Array(Installation), error: Schema.ErrorInstance() }),
    Rpc.make('launch', { payload: { application: Schema.URL }, success: Installation, error: Schema.ErrorInstance() }),
    Rpc.make('status', { payload: { application: Schema.URL }, success: Status, error: Schema.ErrorInstance() }),
    Rpc.make('watch', { payload: { application: Schema.URL }, success: Status, error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('press', { payload: { application: Schema.URL, element: WindowID, action: Schema.String }, success: Status, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { application: Schema.URL, selection: Schema.OptionFromOptionalKey(Schema.Struct({ kind: Kind, name: Schema.String })) }, success: Schema.String, error: Schema.ErrorInstance() }),
    Rpc.make('terms', { payload: { application: Schema.URL, kind: Kind }, success: Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as the scripting definition writes it' }) })), error: Schema.ErrorInstance() }),
    Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.Record(Schema.String, Native), artifacts: Schema.Array(Schema.URL) }, success: Installation.Command, error: Schema.ErrorInstance() }),
    Rpc.make('screenshot', { payload: { application: Schema.URL, window: Schema.OptionFromOptionalKey(WindowID) }, success: Screenshot, error: Schema.ErrorInstance() }),
    Rpc.make('faces', { success: Schema.Array(InstalledFace), error: Schema.ErrorInstance() }),
    Rpc.make('face', { payload: { name: Schema.String }, success: InstalledFace, error: Schema.Union([Unmatched, Schema.ErrorInstance()]) }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation, RpcClient.FromGroup<typeof Requests, RpcClientError.RpcClientError>>()('@rasm/automation/main/Automation') {
    static readonly spawned = (executable: string) =>
        Layer.effect(Automation)(RpcClient.make(Requests)).pipe(
            Layer.provide(RpcClient.layerProtocolSocket({ retryPolicy: Schedule.recurs(0) })),
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

export { Automation, Installation, InstalledFace, Kind, Native, Screenshot, Status, WindowID };
