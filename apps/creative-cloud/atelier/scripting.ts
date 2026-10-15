import { Array, Config, Context, Duration, Effect, identity, Layer, Schema, Stream, Struct } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcClientError, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';

// --- [MODELS] --------------------------------------------------------------------------

const Refusal = Schema.Struct({ data: Schema.NonEmptyArray(Schema.String) });
const Application = Schema.Struct({ name: Schema.String, identifier: Schema.String, running: Schema.Boolean, scriptable: Schema.Boolean, url: Schema.URL, version: Schema.OptionFromOptionalKey(Schema.String) });
const Window = Schema.Struct({ id: Schema.Int, title: Schema.OptionFromOptionalKey(Schema.String), level: Schema.Literals(['normal', 'modal', 'other']), ...Struct.Record(['width', 'height'], Schema.Finite) });
const Face = Schema.Struct(Struct.Record(['postscriptName', 'family', 'style', 'fullName', 'file'], Schema.String));
const Sender = RpcGroup.make(
    Rpc.make('applications', { payload: { running: Schema.Boolean }, success: Schema.Array(Application), error: Refusal }),
    Rpc.make('application', { payload: { url: Schema.URL }, success: Application, error: Refusal }),
    Rpc.make('definition', { payload: { application: Schema.URL }, success: Schema.String, error: Refusal }),
    Rpc.make('execute', { payload: { application: Schema.URL, command: Schema.String, arguments: Schema.JsonObject }, success: Schema.Json, error: Refusal }),
    Rpc.make('contentType', { payload: { url: Schema.URL }, success: Schema.OptionFromNullOr(Schema.String), error: Refusal }),
    Rpc.make('windows', { payload: { application: Schema.URL }, success: Schema.Array(Window), error: Refusal }),
    Rpc.make('capture', { payload: { window: Schema.Int, url: Schema.URL, longest: Schema.Int }, success: Schema.Struct(Struct.Record(['width', 'height'], Schema.Int)), error: Refusal }),
    Rpc.make('faces', { success: Schema.Array(Face), error: Refusal }),
);

// --- [ERRORS] --------------------------------------------------------------------------

const ScriptingReason = Schema.TaggedUnion({
    Refused: { failures: Refusal.fields.data },
    Crashed: { cause: RpcClientError.RpcClientError },
    Missing: { subject: Schema.String, candidates: Schema.Array(Schema.String) },
    Unserved: { application: Schema.String },
    Unpreviewable: { application: Schema.String },
});
class ScriptingError extends Schema.TaggedError<ScriptingError>()('ScriptingError', { reason: ScriptingReason }) {
    override get message(): string {
        return ScriptingReason.match(this.reason, {
            Refused: ({ failures }) => Array.join(failures, '\n'),
            Crashed: ({ cause }) => `The Apple Events sender stopped (${cause.message}): a command in flight may have taken effect, check before repeating it`,
            Missing: ({ subject, candidates }) => `No ${subject}, closest: ${Array.join(candidates, ', ')}`,
            Unserved: ({ application }) => `${application} has no extension serving this tool: execute with run Command reaches it`,
            Unpreviewable: ({ application }) => `${application} records no undo step for this command, so preview cannot undo it`,
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Scripting extends Context.Service<Scripting>()('@rasm/atelier/Scripting', {
    make: Effect.gen(function* () {
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const executable = yield* Config.String('ATELIER_PATH');
        const client = yield* RpcClient.make(Sender, { disableTracing: true }).pipe(
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
                        Effect.mapError((cause) => new Socket.SocketError({ reason: new Socket.SocketOpenError({ kind: 'Unknown', cause }) })),
                    ),
                ),
            ),
            Effect.provide(RpcSerialization.layerNdJsonRpc()),
        );
        return {
            call: <A, R>(use: (sender: typeof client) => Effect.Effect<A, typeof Refusal.Type | RpcClientError.RpcClientError, R>): Effect.Effect<A, ScriptingError, R> =>
                use(client).pipe(
                    Effect.catchReason('RpcClientError', 'RpcClientDefect', Effect.die),
                    Effect.mapError((error) => new ScriptingError({ reason: Schema.is(RpcClientError.RpcClientError)(error) ? { _tag: 'Crashed', cause: error } : { _tag: 'Refused', failures: error.data } })),
                ),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Face, Scripting, ScriptingError, Window };
