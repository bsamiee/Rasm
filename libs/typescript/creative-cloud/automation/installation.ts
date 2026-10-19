import { single } from '@rasm/search';
import { transformSync } from 'amaro';
import { Array, Duration, Effect, FileSystem, identity, Layer, type Option, Path, Result, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Face } from './face.ts';
import { AutomationError, Helper } from './helper.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMANDS = [
    { name: 'do script', code: 'K2  dosc', runtime: 'uxpscript' },
    { name: 'do javascript', code: 'miscDjxM', runtime: 'extendscript' },
    { name: 'DoScript', code: 'miscdosc', runtime: 'extendscript' },
    { name: 'do script', code: 'miscdosc', runtime: 'acrobat' },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Element = Schema.Struct({
    id: Schema.Int.annotate({ description: 'Window number of a window, else the helper-assigned number of the element while it stays open' }),
    kind: Schema.Literals(['window', 'dialog', 'sheet', 'menu']),
    role: Schema.String,
    title: Schema.OptionFromOptionalKey(Schema.String),
    text: Schema.Array(Schema.String).annotate({ description: 'Static text the element shows' }),
    buttons: Schema.Array(Schema.String).annotate({ description: 'Button labels press accepts' }),
});
const Status = Schema.Struct({
    responding: Schema.Boolean.annotate({ description: 'False when the application answered no accessibility request within the messaging timeout' }),
    frontWindow: Schema.OptionFromOptionalKey(Element.fields.id),
    elements: Schema.Array(Element),
});
const Window = Schema.Struct({ id: Schema.Finite.check(Schema.isUint32()), title: Schema.OptionFromOptionalKey(Schema.String) });
const Screenshot = Schema.Struct({ window: Window, windows: Schema.Array(Window), image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
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
    static readonly installed = Effect.flatMap(Helper, (client) => client.applications());
    static readonly located = (query: string) => Effect.flatMap(Installation.installed, (rows) => single('installed Adobe application', query)(rows, { names: (row) => [row.identifier, row.name, row.url.href], label: (row) => row.url.href }));

    // --- [LIFECYCLE]
    launch() {
        return Effect.flatMap(Helper, (client) => client.launch({ url: this.url }));
    }
    status() {
        return Effect.flatMap(Helper, (client) => client.status({ url: this.url }));
    }
    watch() {
        return Stream.unwrap(Effect.map(Helper, (client) => client.watch({ url: this.url })));
    }
    press(element: number, button: string) {
        return Effect.flatMap(Helper, (client) => client.press({ url: this.url, element, button }));
    }
    screenshot(window: Option.Option<number>) {
        return Effect.flatMap(Helper, (client) => client.screenshot({ application: this.url, window }));
    }

    // --- [DICTIONARY]
    definition(selection: Option.Option<{ readonly kind: string; readonly name: string }>) {
        return Effect.flatMap(Helper, (client) => client.definition({ application: this.url, selection }));
    }
    terms(kind: string) {
        return Effect.flatMap(Helper, (client) => client.terms({ application: this.url, kind }));
    }
    command(name: string, parameters: Schema.JsonObject, artifacts: readonly URL[]) {
        const { url } = this;
        return Effect.gen(function* () {
            const [client, files, path] = yield* Effect.all([Helper, FileSystem.FileSystem, Path.Path]);
            const reply = yield* client.execute({ application: url, command: name, arguments: parameters });
            const [read, failures] = yield* Effect.partition(artifacts, (artifact) => Effect.all({ uri: Effect.succeed(artifact.href), mimeType: client.contentType({ url: artifact }), data: Effect.flatMap(path.fromFileUrl(artifact), files.readFile) }, { concurrency: 'unbounded' }), { concurrency: 'unbounded' });
            return { reply, artifacts: read, unread: failures.map(Struct.get('message')) };
        });
    }

    // --- [SCRIPTS]
    language() {
        return Effect.flatMap(this.terms('command'), (declared) =>
            Effect.fromOption(
                Array.findFirst(COMMANDS, (row) => declared.some(({ name, code }) => name === row.name && code === row.code)),
                () => AutomationError.make({ reason: { _tag: 'Refused', failures: ['The dictionary declares no script command'] } }),
            ),
        );
    }
    script(code: string) {
        const { url, name: product } = this;
        const wrappers = {
            extendscript: (body: string) => `(function (body) {
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
        return encoded({ _tag: "Success", success: new Function(decodeURIComponent(body)).call(this) });
    } catch (error) {
        var known = error instanceof Error;
        var located = known && typeof error.number === "number" && typeof error.line === "number";
        return encoded({ _tag: "Failure", failure: { _tag: "Script", text: known ? error.message : String(error), error: located ? { number: error.number, line: error.line } : null } });
    }
}).call(this, "${body}")`,
            acrobat: (body: string) => `(function (body) {
    try {
        var value = new Function(decodeURIComponent(body)).call(this);
        return encodeURIComponent(JSON.stringify({ _tag: "Success", success: value === undefined ? null : value }));
    } catch (error) {
        return encodeURIComponent(JSON.stringify({ _tag: "Failure", failure: { _tag: "Script", text: String(error instanceof Error ? error.message : error), error: null } }));
    }
}).call(this, "${body}")`,
        };
        const scripted = Effect.filterOrFail(
            this.language(),
            (row): row is Exclude<(typeof COMMANDS)[number], { readonly runtime: 'uxpscript' }> => row.runtime !== 'uxpscript',
            () => AutomationError.make({ reason: { _tag: 'Refused', failures: [`${product} runs scripts in its extension, its dictionary script command keeps no context between calls`] } }),
        );
        return Effect.gen(function* () {
            const [client, { name, runtime }] = yield* Effect.all([Helper, scripted]);
            const reply = yield* client.execute({ application: url, command: name, arguments: { direct: wrappers[runtime](encodeURIComponent(code)) } });
            const text = yield* Schema.decodeUnknownEffect(Schema.String)(reply);
            const outcome = yield* Schema.decodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Result(Schema.Json, AutomationError.fields.reason.cases.Script))))(runtime === 'acrobat' ? decodeURIComponent(text) : text);
            return yield* Effect.fromResult(Result.mapError(outcome, (reason) => AutomationError.make({ reason })));
        });
    }
}
const Protocol = RpcGroup.make(
    Rpc.make('applications', { success: Schema.Array(Installation), error: AutomationError }),
    Rpc.make('application', { payload: { url: Schema.URL }, success: Installation, error: AutomationError }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Installation, error: AutomationError }),
    Rpc.make('status', { payload: { url: Schema.URL }, success: Status, error: AutomationError }),
    Rpc.make('watch', { payload: { url: Schema.URL }, success: Status, error: AutomationError, stream: true }),
    Rpc.make('press', { payload: { url: Schema.URL, element: Element.fields.id, button: Schema.String }, success: Status, error: AutomationError }),
    Rpc.make('definition', { payload: { application: Schema.URL, selection: Schema.OptionFromOptionalKey(Schema.Struct({ kind: Schema.String, name: Schema.String })) }, success: Schema.String, error: AutomationError }),
    Rpc.make('terms', { payload: { application: Schema.URL, kind: Schema.String }, success: Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: "Four-character code in the dictionary's quoted form" }) })), error: AutomationError }),
    Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.JsonObject }, success: Schema.Json, error: AutomationError }),
    Rpc.make('contentType', { payload: { url: Schema.URL }, success: Schema.String, error: AutomationError }),
    Rpc.make('screenshot', { payload: { application: Schema.URL, window: Schema.OptionFromOptionalKey(Window.fields.id) }, success: Screenshot, error: AutomationError }),
    Rpc.make('faces', { success: Schema.Array(Face), error: AutomationError }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const stripped = (code: string) => {
    const [open, close] = ['async function body() {\n', '\n}'];
    return Effect.try(() => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const helper = (executable: string) =>
    Layer.effect(Helper)(
        Effect.gen(function* () {
            const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
            return yield* RpcClient.make(Protocol, { disableTracing: true }).pipe(
                Effect.provideServiceEffect(RpcClient.Protocol, RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity })),
                Effect.provideServiceEffect(
                    Socket.Socket,
                    Socket.fromTransformStream(
                        Effect.gen(function* () {
                            const requests = new TransformStream<Uint8Array, Uint8Array>();
                            const child = yield* spawner.spawn(ChildProcess.make(executable, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' }));
                            return { readable: Stream.toReadableStream(child.stdout), writable: requests.writable };
                        }).pipe(
                            Effect.tapCause(Effect.logError),
                            Effect.mapError((cause) => Socket.SocketError.make({ reason: Socket.SocketOpenError.make({ kind: 'Unknown', cause }) })),
                        ),
                    ),
                ),
                Effect.provide(RpcSerialization.layerNdJsonRpc()),
            );
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { helper, Installation, Protocol, Screenshot, Status, stripped };
