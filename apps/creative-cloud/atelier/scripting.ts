import { Config, Context, Duration, Effect, identity, Layer, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Struct({ name: Schema.String, identifier: Schema.String, running: Schema.Boolean, scriptable: Schema.Boolean, url: Schema.URL, version: Schema.OptionFromOptionalKey(Schema.String) });
const Window = Schema.Struct({ id: Schema.Finite.check(Schema.isUint32()), title: Schema.OptionFromOptionalKey(Schema.String) });
const Screenshot = Schema.Struct({ window: Window, windows: Schema.Array(Window), image: Schema.Uint8ArrayFromBase64, width: Schema.Int.annotate({ description: 'Image width in pixels' }), height: Schema.Int.annotate({ description: 'Image height in pixels' }) });
const Face = Schema.Struct({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, fullName: Schema.String, file: Schema.String });

// --- [ERRORS] --------------------------------------------------------------------------

class ScriptingError extends Schema.TaggedError<ScriptingError, { readonly brand: unique symbol }>()('ScriptingError', { failures: Schema.NonEmptyArray(Schema.String) }) {
    override get message(): string {
        return this.failures.join('\n');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Scripting extends Context.Service<Scripting>()('@rasm/atelier/scripting', {
    make: Effect.gen(function* () {
        const [spawner, executable] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, Config.String('ATELIER_PATH')]);
        return yield* RpcClient.make(
            RpcGroup.make(
                Rpc.make('applications', { payload: { running: Schema.Boolean }, success: Schema.Array(Application), error: ScriptingError }),
                Rpc.make('application', { payload: { url: Schema.URL }, success: Application, error: ScriptingError }),
                Rpc.make('definition', { payload: { application: Schema.URL, selection: Schema.optionalKey(Schema.Struct({ kind: Schema.String, name: Schema.String })) }, success: Schema.String, error: ScriptingError }),
                Rpc.make('terms', { payload: { application: Schema.URL, kind: Schema.optionalKey(Schema.String) }, success: Schema.Array(Schema.String).pipe(Schema.mutable), error: ScriptingError }),
                Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.JsonObject }, success: Schema.Json, error: ScriptingError }),
                Rpc.make('contentType', { payload: { url: Schema.URL }, success: Schema.String, error: ScriptingError }),
                Rpc.make('screenshot', { payload: { application: Schema.URL, window: Schema.OptionFromOptionalKey(Window.fields.id) }, success: Screenshot, error: ScriptingError }),
                Rpc.make('faces', { success: Schema.Array(Face), error: ScriptingError }),
            ),
            { disableTracing: true },
        ).pipe(
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
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Face, Screenshot, Scripting };
