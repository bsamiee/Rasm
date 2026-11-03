import { HostError, Requests } from '@rasm/automation/requests';
import { Link } from '@rasm/uxp/link';
import { Cause, Console, Effect, flow, Layer, Predicate, Record, Schema } from 'effect';
import type { Rpc } from 'effect/rpc';
import { answered, Envelope, Reply, type Request } from '#/contract';
import { ExtensionManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Cep {
    readonly evalScript: (script: string, callback: (reply: string) => void) => void;
    readonly getSystemPath: (type: string) => string;
    readonly getExtensionId: () => string;
}

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct({ evalScript: Schema.declare(Predicate.isFunction), getSystemPath: Schema.declare(Predicate.isFunction), getExtensionId: Schema.declare(Predicate.isFunction) });
const Panel = Schema.Struct({ __adobe_cep__: Schema.declare((host): host is Cep => Schema.is(Cep)(host)) });
const Answer = Schema.Union([Schema.Literal('EvalScript error.'), Schema.fromJsonString(Reply)]);

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { __adobe_cep__: cep } = yield* Schema.decodeUnknownEffect(Panel)(globalThis);
    const served = <Current extends Request>(rpc: Current) => {
        const [request, success] = [Schema.fromJsonString(Envelope(rpc)), Schema.toCodecJson(rpc.successSchema)];
        return Effect.fnUntraced(function* (payload: Current['payloadSchema']['Type']) {
            const sent = yield* Effect.orDie(Schema.encodeEffect(request)({ tag: rpc._tag, label: payload.label, payload }));
            const reply = yield* Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${sent.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')})`, flow(Effect.succeed, resume)));
            const answer = yield* Schema.decodeEffect(Answer)(reply).pipe(Effect.filterOrFail(Schema.is(Reply), () => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `${payload.label}: ${ExtensionManifest.script} ${ExtensionManifest.global}` })));
            return yield* answered(success, payload.label)(answer);
        });
    };
    // biome-ignore lint/nursery/noUnsafeTypeAssertion: each handler encodes its tag's payload and decodes its tag's success through that rpc's schemas, which a table built from the request map types as their union
    const layer = Requests.toLayer(Record.fromIterableWith(Requests.requests.values(), (rpc) => [rpc._tag, served(rpc)]) as unknown as { readonly [Current in Request as Current['_tag']]: Rpc.ToHandlerFn<Current, never> });
    return yield* Link.dial(Requests, layer, Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')))(cep.getExtensionId()).pipe(Layer.launch);
}).pipe(Effect.tapCause(flow(Cause.pretty, Console.error)), Effect.runFork);
