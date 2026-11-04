import { HostError, Requests } from '@rasm/automation';
import { Peer } from '@rasm/uxp/peer';
import { Cause, Console, Effect, Function, flow, Layer, Predicate, Schema, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Completion, Envelope } from '#/contract';
import { ExtensionManifest } from '#/extension-manifest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Cep {
    readonly evalScript: (script: string, callback: (reply: string) => void) => void;
    readonly getSystemPath: (type: string) => string;
    readonly getExtensionId: () => string;
}

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct({ evalScript: Schema.declare(Predicate.isFunction), getSystemPath: Schema.declare(Predicate.isFunction), getExtensionId: Schema.declare(Predicate.isFunction) });
const Panel = Schema.Struct({ __adobe_cep__: Schema.declare((host): host is Cep => Schema.is(Cep)(host)) });
const Sent = Function.memoize((rpc: Parameters<typeof Envelope>[0]) => Schema.fromJsonString(Schema.toCodecJson(Envelope(rpc))));
const Reply = Schema.fromJsonString(Completion);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = (cep: Cep) =>
    Effect.fnUntraced(function* <Tag extends Requests['_tag'], Payload extends Requests['payloadSchema'], Success extends Requests['successSchema']>(payload: Payload['Type'], { rpc }: { readonly rpc: Rpc.Rpc<Tag, Payload, Success, Requests['errorSchema']> }) {
        const sent = yield* Effect.orDie(Schema.encodeEffect(Sent(rpc))({ tag: rpc._tag, label: payload.label, payload }));
        return yield* Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${sent.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')})`, flow(Effect.succeed, resume))).pipe(
            Effect.filterOrFail(
                (reply) => reply !== 'EvalScript error.',
                () => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `${payload.label}: ${ExtensionManifest.script} ${ExtensionManifest.global}` }),
            ),
            Effect.flatMap(Schema.decodeEffect(Reply)),
            Effect.flatMap(Completion.answered(Schema.toCodecJson(rpc.successSchema), payload.label)),
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { __adobe_cep__: cep } = yield* Schema.decodeUnknownEffect(Panel)(globalThis);
    const layer = Requests.toLayer(Struct.Record(Array.from(Requests.requests.values(), Struct.get('_tag')), served(cep)));
    return yield* Peer.dial(Requests, layer, Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')))(cep.getExtensionId()).pipe(Layer.launch);
}).pipe(Effect.tapCause(flow(Cause.pretty, Console.error)), Effect.runFork);
