import { HostError } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';
import { type Cep, servePanel } from '@rasm/cep/panel';
import { Array, Effect, Function, flow, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Peer } from '#/peer';
import { Completion, Envelope, HostUrl, Requests } from '#/requests';

// --- [MODELS] --------------------------------------------------------------------------

const Sent = Function.memoize((rpc: Parameters<typeof Envelope>[0]) => Schema.fromJsonString(Schema.toCodecJson(Envelope(rpc))));
const Reply = Schema.fromJsonString(Completion);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = (cep: Cep) =>
    Effect.fnUntraced(function* <Tag extends Requests['_tag'], Payload extends Requests['payloadSchema'], Success extends Requests['successSchema']>(payload: Payload['Type'], { rpc }: { readonly rpc: Rpc.Rpc<Tag, Payload, Success, Requests['errorSchema']> }) {
        const sent = yield* Effect.orDie(Schema.encodeEffect(Sent(rpc))({ tag: rpc._tag, label: payload.label, payload }));
        const literal = sent.replaceAll('\u2028', String.raw`\u2028`).replaceAll('\u2029', String.raw`\u2029`);
        return yield* Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${literal})`, flow(Effect.succeed, resume))).pipe(
            Effect.filterOrFail(
                (reply) => reply !== 'EvalScript error.',
                () => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `${payload.label}: ${ExtensionManifest.script} ${ExtensionManifest.global}` }),
            ),
            Effect.flatMap(Schema.decodeEffect(Reply)),
            Effect.flatMap(Completion.answered(Schema.toCodecJson(rpc.successSchema), payload.label)),
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

servePanel({
    link: (cep) => Peer.dial(Requests, Requests.toLayer(Struct.Record(Array.map(Array.fromIterable(Requests.requests.values()), Struct.get('_tag')), served(cep))), Schema.decodeEffect(HostUrl)(cep.getSystemPath('hostApplication')))(cep.getExtensionId(), Schema.decodeEffect(HostUrl)(cep.getSystemPath('extension'))),
});
