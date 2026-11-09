import { HostError } from '@rasm/automation';
import { ExtensionManifest } from '@rasm/cep';
import { type Cep, servePanel } from '@rasm/cep/panel';
import { Array, Effect, Function, flow, Predicate, Schema, Struct } from 'effect';
import { Url } from 'effect/http';
import type { Rpc } from 'effect/rpc';
import { Peer } from '#/peer';
import { Completion, Envelope, HostUrl, Requests } from '#/requests';

// --- [TYPES] ---------------------------------------------------------------------------

interface Files {
    readonly readFile: (path: string) => unknown;
}

// --- [MODELS] --------------------------------------------------------------------------

const Sent = Function.memoize((rpc: Parameters<typeof Envelope>[0]) => Schema.fromJsonString(Schema.toCodecJson(Envelope(rpc))));
const Reply = Schema.fromJsonString(Completion);
const Engine = Schema.Struct({ cep: Schema.Struct({ fs: Schema.declare((held): held is Files => Schema.is(Schema.Struct({ readFile: Schema.declare(Predicate.isFunction) }))(held)) }) }).annotate({ description: 'File system the CEP engine sets on window' });
const Read = Schema.Struct({ err: Schema.Literal(0), data: Schema.String }).annotate({ description: 'window.cep.fs.readFile result without error' });

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
const key = Effect.fnUntraced(function* (cep: Cep) {
    const [{ cep: engine }, file] = yield* Effect.all([Effect.suspend(() => Schema.decodeUnknownEffect(Engine)(globalThis)), Effect.fromResult(Url.fromString(`../../${Peer.link.key}`, `${cep.getSystemPath('userData')}/`))]);
    const path = decodeURIComponent(file.pathname);
    return yield* Effect.try({ try: () => engine.fs.readFile(path), catch: HostError.refused(path) }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Read)), Effect.map(Struct.get('data')));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

servePanel({
    link: (cep) =>
        Peer.dial(Requests, Requests.toLayer(Struct.Record(Array.map(Array.fromIterable(Requests.requests.values()), Struct.get('_tag')), served(cep))), Schema.decodeEffect(HostUrl)(cep.getSystemPath('hostApplication')), key(cep))(cep.getExtensionId(), Schema.decodeEffect(HostUrl)(cep.getSystemPath('extension'))),
});
