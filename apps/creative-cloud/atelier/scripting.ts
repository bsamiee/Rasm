import { Config, Context, Duration, Effect, identity, Layer, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Struct({ name: Schema.String, identifier: Schema.String, running: Schema.Boolean, scriptable: Schema.Boolean, url: Schema.URL, version: Schema.OptionFromOptionalKey(Schema.String) });
const Window = Schema.Struct({ id: Schema.Int, title: Schema.OptionFromOptionalKey(Schema.String), level: Schema.Literals(['normal', 'modal', 'other']), ...Struct.Record(['width', 'height'], Schema.Finite) });
const Size = Schema.Struct(Struct.Record(['width', 'height'], Schema.Int));
const Face = Schema.Struct(Struct.Record(['postscriptName', 'family', 'style', 'fullName', 'file'], Schema.String));

// --- [ERRORS] --------------------------------------------------------------------------

class ScriptingError extends Schema.TaggedError<ScriptingError, { readonly brand: unique symbol }>()('ScriptingError', { reason: Schema.TaggedUnion({ Refused: { failures: Schema.NonEmptyArray(Schema.String) }, Unpreviewable: { application: Schema.String } }) }) {
    override get message(): string {
        return ScriptingError.fields.reason.match(this.reason, {
            Refused: ({ failures }) => failures.join('\n'),
            Unpreviewable: ({ application }) => `${application} records no undo step for this command, run it without preview`,
        });
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
                Rpc.make('definition', { payload: { application: Schema.URL }, success: Schema.String, error: ScriptingError }),
                Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.JsonObject }, success: Schema.Json, error: ScriptingError }),
                Rpc.make('contentType', { payload: { url: Schema.URL }, success: Schema.OptionFromNullOr(Schema.String), error: ScriptingError }),
                Rpc.make('windows', { payload: { application: Schema.URL }, success: Schema.Array(Window), error: ScriptingError }),
                Rpc.make('capture', { payload: { window: Schema.Int, url: Schema.URL }, success: Size, error: ScriptingError }),
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

export { Application, Face, Scripting, ScriptingError, Size, Window };
