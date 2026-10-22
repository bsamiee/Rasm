import { Automation } from '@rasm/automation';
import { Effect, Layer, Schema } from 'effect';
import { RpcClient, RpcClientError, type RpcMessage, RpcSerialization } from 'effect/rpc';
import { InDesign } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const protocol = (execute: Automation['Service']['execute'], entry: URL, pid: number) =>
    RpcClient.Protocol.make(
        Effect.fnUntraced(function* (writeResponse) {
            const { makeUnsafe, codecFor } = yield* RpcSerialization.RpcSerialization;
            const parser = makeUnsafe();
            const refused = (cause: unknown) => RpcClientError.RpcClientError.make({ reason: RpcClientError.RpcClientDefect.make({ message: `do script of ${entry.href} in process ${pid}`, cause }) });
            const sent = Effect.fnUntraced(function* (clientId: number, request: RpcMessage.RequestEncoded) {
                const encoded = yield* Schema.decodeUnknownEffect(Schema.String)(parser.encode(request));
                const { reply } = yield* execute({ pid, command: 'do script', arguments: { direct: { _tag: 'file', url: entry.href }, language: 'uxpscript', 'with arguments': [encoded] }, artifacts: [] });
                const answer = yield* Schema.decodeUnknownEffect(Schema.String)(reply);
                // biome-ignore lint/nursery/noUnsafeTypeAssertion: the RpcClient decodes each message part through codecFor, as Effect's own protocols do with the parser's output
                const responses = parser.decode(answer) as readonly RpcMessage.FromServerEncoded[];
                yield* Effect.forEach(responses, (response) => writeResponse(clientId, response), { discard: true });
            }, Effect.mapError(refused));
            return {
                send: (clientId: number, request: RpcMessage.FromClientEncoded) => (request._tag === 'Request' ? sent(clientId, request) : Effect.void),
                supportsAck: false,
                supportsTransferables: false,
                codecFor,
            };
        }),
    );
const client = (entry: URL) =>
    Effect.map(
        Automation,
        ({ execute }) =>
            (pid: number) =>
                RpcClient.make(InDesign, { flatten: true }).pipe(Effect.provide(Layer.effect(RpcClient.Protocol)(protocol(execute, entry, pid)).pipe(Layer.provide(RpcSerialization.layerJson)))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { client };
