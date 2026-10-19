import { single } from '@rasm/search';
import { transformSync } from 'amaro';
import { Array, Cause, Duration, Effect, FileSystem, identity, Layer, Option, Path, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, type RpcClientError, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Status = Schema.Struct({
    responding: Schema.Boolean.annotate({ description: 'False when the application answered no accessibility request within the messaging timeout' }),
    frontWindow: Schema.OptionFromOptionalKey(Schema.Int),
    elements: Schema.Array(
        Schema.Struct({
            id: Schema.Int.annotate({ description: 'Window number of a window, else the helper-assigned number of the element while it stays open' }),
            kind: Schema.Literals(['window', 'dialog', 'sheet', 'menu']),
            role: Schema.String,
            title: Schema.OptionFromOptionalKey(Schema.String),
            text: Schema.Array(Schema.String).annotate({ description: 'Static text the element shows' }),
            buttons: Schema.Array(Schema.String).annotate({ description: 'Button labels press accepts' }),
        }),
    ),
});
const Window = Schema.Struct({ id: Schema.Finite.check(Schema.isUint32()), title: Schema.OptionFromOptionalKey(Schema.String) });
const Screenshot = Schema.Struct({ window: Window, windows: Schema.Array(Window), image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, fullName: Schema.String, file: Schema.String }) {
    static get installed() {
        return sent((client) => client.faces());
    }
    static readonly named = (name: string) => Effect.flatMap(Face.installed, (faces) => single('installed face', name)(faces, { names: (each) => [each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])], label: Struct.get('postscriptName') }));
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
        return sent((client) => client.installations());
    }
    static readonly located = (query: string) => Effect.flatMap(Installation.installed, (rows) => single('installed Adobe application', query)(rows, { names: (row) => [row.identifier, row.name, row.url.href], label: (row) => row.url.href }));

    // --- [LIFECYCLE]
    launch() {
        return sent((client) => client.launch({ url: this.url }));
    }
    status() {
        return sent((client) => client.status({ url: this.url }));
    }
    watch() {
        return Stream.unwrap(Effect.map(RpcClient.make(Protocol), (client) => client.watch({ url: this.url })));
    }

    // --- [ELEMENTS]
    press(element: number, button: string) {
        return sent((client) => client.press({ url: this.url, element, button }));
    }
    screenshot(window: Option.Option<number>) {
        return sent((client) => client.screenshot({ application: this.url, window }));
    }

    // --- [DICTIONARY]
    definition(selection: Option.Option<{ readonly kind: string; readonly name: string }>) {
        return sent((client) => client.definition({ application: this.url, selection }));
    }
    terms(kind: string) {
        return sent((client) => client.terms({ application: this.url, kind }));
    }
    command(name: string, parameters: Schema.JsonObject, artifacts: readonly URL[]) {
        const { url } = this;
        return sent((client) =>
            Effect.gen(function* () {
                const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const reply = yield* client.execute({ application: url, command: name, arguments: parameters });
                const [read, failures] = yield* Effect.partition(artifacts, (artifact) => Effect.all({ uri: Effect.succeed(artifact.href), mimeType: client.contentType({ url: artifact }), data: Effect.flatMap(path.fromFileUrl(artifact), files.readFile) }, { concurrency: 'unbounded' }), { concurrency: 'unbounded' });
                return { reply, artifacts: read, unread: failures.map(Struct.get('message')) };
            }),
        );
    }

    // --- [SCRIPTS]
    language() {
        const commands = [
            { name: 'do script', code: 'K2  dosc', runtime: 'uxpscript' },
            { name: 'do javascript', code: 'miscDjxM', runtime: 'extendscript' },
            { name: 'DoScript', code: 'miscdosc', runtime: 'extendscript' },
            { name: 'do script', code: 'miscdosc', runtime: 'acrobat' },
        ] as const;
        return Effect.flatMap(this.terms('command'), (declared) =>
            Effect.fromOption(
                Array.findFirst(commands, (row) => declared.some(({ name, code }) => name === row.name && code === row.code)),
                () => new Cause.NoSuchElementError(`${this.name} declares no script command`),
            ),
        );
    }
    script(code: string) {
        const encoders = {
            uxpscript: Option.none(),
            extendscript: Option.some(`function encoded(value) {
        var parts = [], key, at;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\\x00-\\x1f"\\\\]/g, function (c) { return "\\\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        if (value instanceof Array) { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (value.typename !== undefined) return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }`),
            acrobat: Option.some('JSON.stringify'),
        };
        const { url, name: product } = this;
        return Effect.flatMap(this.language(), ({ name, runtime }) =>
            Effect.gen(function* () {
                const encoder = yield* Effect.fromOption(encoders[runtime], () => new Cause.IllegalArgumentError(`${product} runs scripts in its extension, its dictionary script command keeps no context between calls`));
                const reply = yield* sent((client) =>
                    client.execute({
                        application: url,
                        command: name,
                        arguments: {
                            direct: `(function (body, encoded) {
    var outcome;
    try {
        var value = new Function(decodeURIComponent(body)).call(this);
        outcome = { _tag: "Success", success: value === undefined ? null : value };
    } catch (error) {
        var known = error instanceof Error;
        var thrown = known ? { name: error.name, message: error.message } : { message: String(error) };
        if (known && typeof error.number === "number" && typeof error.line === "number") thrown.cause = { number: error.number, line: error.line };
        outcome = { _tag: "Failure", failure: thrown };
    }
    return encodeURIComponent(encoded(outcome));
}).call(this, "${encodeURIComponent(code)}", ${encoder})`,
                        },
                    }),
                );
                const outcome = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(Schema.Json, Schema.ErrorInstance()))))(decodeURIComponent(yield* Schema.decodeUnknownEffect(Schema.String)(reply)));
                return { runtime, result: yield* Effect.fromResult(outcome) };
            }),
        );
    }
}
const Protocol = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Array(Installation), error: Schema.ErrorInstance() }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Installation, error: Schema.ErrorInstance() }),
    Rpc.make('status', { payload: { url: Schema.URL }, success: Status, error: Schema.ErrorInstance() }),
    Rpc.make('watch', { payload: { url: Schema.URL }, success: Status, error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('press', { payload: { url: Schema.URL, element: Status.fields.elements.value.fields.id, button: Schema.String }, success: Status, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { application: Schema.URL, selection: Schema.OptionFromOptionalKey(Schema.Struct({ kind: Schema.String, name: Schema.String })) }, success: Schema.String, error: Schema.ErrorInstance() }),
    Rpc.make('terms', { payload: { application: Schema.URL, kind: Schema.String }, success: Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: "Four-character code in the dictionary's quoted form" }) })), error: Schema.ErrorInstance() }),
    Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.JsonObject }, success: Schema.Json, error: Schema.ErrorInstance() }),
    Rpc.make('contentType', { payload: { url: Schema.URL }, success: Schema.String, error: Schema.ErrorInstance() }),
    Rpc.make('screenshot', { payload: { application: Schema.URL, window: Schema.OptionFromOptionalKey(Screenshot.fields.window.fields.id) }, success: Screenshot, error: Schema.ErrorInstance() }),
    Rpc.make('faces', { success: Schema.Array(Face), error: Schema.ErrorInstance() }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const sent = <A, E, R>(call: (client: RpcClient.RpcClient<RpcGroup.Rpcs<typeof Protocol>, RpcClientError.RpcClientError>) => Effect.Effect<A, E, R>) => Effect.scoped(Effect.flatMap(RpcClient.make(Protocol), call));
const stripped = (code: string) => {
    const [open, close] = ['async function body() {\n', '\n}'];
    return Effect.try(() => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const helper = (executable: string) =>
    RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity }).pipe(
        Layer.provide(
            Layer.effect(Socket.Socket)(
                Socket.fromTransformStream(
                    Effect.gen(function* () {
                        const requests = new TransformStream<Uint8Array, Uint8Array>();
                        const child = yield* (yield* ChildProcessSpawner.ChildProcessSpawner).spawn(ChildProcess.make(executable, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' }));
                        return { readable: Stream.toReadableStream(child.stdout), writable: requests.writable };
                    }).pipe(
                        Effect.tapCause(Effect.logError),
                        Effect.mapError((cause) => Socket.SocketError.make({ reason: Socket.SocketOpenError.make({ kind: 'Unknown', cause }) })),
                    ),
                ),
            ),
        ),
        Layer.provide(RpcSerialization.layerNdJsonRpc()),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face, helper, Installation, Protocol, Screenshot, Status, stripped };
