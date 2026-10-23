import { Unmatched } from '@rasm/search';
import { Cause, Context, Duration, Effect, identity, Layer, Option, Schedule, Schema, Sink, Stream, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcClient, RpcClientError, type RpcMessage, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import type { Execute } from '#/apple-events';
import { Contract } from '#/contract';

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/automation', {
    make: Effect.gen(function* () {
        const client = yield* RpcClient.make(Contract);
        const held = Effect.fnUntraced(function* <A, E, R>(changes: Stream.Stream<A, E, R>, subject: string) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} stream ended before first value`)),
                SubscriptionRef.make,
            );
            yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)));
            return latest;
        });
        const [installations, instances] = yield* Effect.all([held(client.installations(), 'installations'), held(client.instances(), 'instances')]);
        const installed = Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult);
        return {
            ...client,
            installations,
            instances,
            installed,
            located: (query: string) => Effect.flatMap(installed, (rows) => Unmatched.single('installed Adobe application', Option.some(query))(rows, (row) => [row.name])),
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
                    .pipe(Effect.flatMap((reply) => (reply === null || reply === '' ? Effect.succeedNone : Effect.asSome(Schema.decodeUnknownEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Json))))(reply))))),
        };
    }),
}) {
    static readonly protocol = (pid: number, event: (message: string) => Pick<Execute, 'command' | 'arguments'>) =>
        Layer.effect(RpcClient.Protocol)(
            RpcClient.Protocol.make(
                Effect.fnUntraced(function* (writeResponse) {
                    const [{ execute }, { makeUnsafe, codecFor }] = yield* Effect.all([Automation, RpcSerialization.RpcSerialization]);
                    const parser = makeUnsafe();
                    const sent = Effect.fnUntraced(
                        function* (clientId: number, request: RpcMessage.RequestEncoded) {
                            const { reply } = yield* execute({ pid, ...event(yield* Schema.decodeUnknownEffect(Schema.String)(parser.encode(request))), artifacts: [] });
                            // biome-ignore lint/nursery/noUnsafeTypeAssertion: RpcClient decodes each part through codecFor
                            const responses = parser.decode(yield* Schema.decodeUnknownEffect(Schema.String)(reply)) as readonly RpcMessage.FromServerEncoded[];
                            yield* Effect.forEach(responses, (response) => writeResponse(clientId, response), { discard: true });
                        },
                        (effect, _, request) => Effect.mapError(effect, (cause) => RpcClientError.RpcClientError.make({ reason: RpcClientError.RpcClientDefect.make({ message: `${request.tag} request to process ${pid} failed`, cause }) })),
                    );
                    return {
                        send: (clientId: number, request: RpcMessage.FromClientEncoded) => (request._tag === 'Request' ? sent(clientId, request) : Effect.void),
                        supportsAck: false,
                        supportsTransferables: false,
                        codecFor,
                    };
                }),
            ),
        ).pipe(Layer.provide(RpcSerialization.layerJson));
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

export { Automation };
