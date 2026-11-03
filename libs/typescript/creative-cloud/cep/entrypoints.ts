import { HostError, Requests } from '@rasm/automation/requests';
import { Link } from '@rasm/uxp/link';
import { Cause, Console, Effect, flow, Layer, Match, Predicate, Record, Schema } from 'effect';
import type { Rpc } from 'effect/rpc';
import { answered, Envelope, Reply, type Request } from '#/contract';
import { ExtensionManifest } from '#/manifest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNEVALUATED = 'EvalScript error.';

// --- [MODELS] --------------------------------------------------------------------------

const Answer = Schema.Union([Schema.Literal(UNEVALUATED), Schema.fromJsonString(Reply)]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const extension = (link: ImportMetaEnv['LINK']): void => {
    Effect.gen(function* () {
        const { __adobe_cep__: cep } = yield* Schema.decodeUnknownEffect(
            Schema.Struct({
                __adobe_cep__: Schema.declare(
                    Schema.is(
                        Schema.Struct({
                            evalScript: Schema.declare(Predicate.isFunction),
                            getSystemPath: Schema.declare(Predicate.isFunction),
                            getExtensionId: Schema.declare(Predicate.isFunction),
                        }),
                    ),
                ),
            }),
        )(globalThis);
        const evaluated = (script: string) =>
            Effect.callback<unknown>((resume) => {
                cep.evalScript(script, flow(Effect.succeed, resume));
            });
        const served = <Current extends Request>(rpc: Current) => {
            const [request, success] = [Schema.fromJsonString(Envelope(rpc)), Schema.toCodecJson(rpc.successSchema)];
            return (payload: Current['payloadSchema']['Type']) =>
                Schema.encodeEffect(request)({ tag: rpc._tag, label: payload.label, payload }).pipe(
                    Effect.orDie,
                    Effect.flatMap((sent) => evaluated(`${ExtensionManifest.global}(${sent.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')})`)),
                    Effect.flatMap(Schema.decodeUnknownEffect(Answer)),
                    Effect.flatMap((answer) =>
                        Match.value(answer).pipe(
                            Match.when(UNEVALUATED, () => Effect.fail(HostError.refused(`${payload.label}: ${ExtensionManifest.script} defines no ${ExtensionManifest.global}`)())),
                            Match.orElse(answered(success, payload.label)),
                        ),
                    ),
                );
        };
        const handlers = Record.fromIterableWith(Requests.requests.values(), (rpc) => [rpc._tag, served(rpc)]);
        // biome-ignore lint/nursery/noUnsafeTypeAssertion: each handler encodes its tag's payload and decodes its tag's success through that rpc's schemas, which a table built from the request map types as their union
        const layer = Requests.toLayer(handlers as unknown as { readonly [Current in Request as Current['_tag']]: Rpc.ToHandlerFn<Current, never> });
        const identifier = yield* Schema.decodeUnknownEffect(Schema.String)(cep.getExtensionId());
        return yield* Layer.launch(Link.dial(Requests, layer, { application: Schema.decodeUnknownEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')) })({ ...link, extension: identifier }));
    }).pipe(Effect.tapCause(flow(Cause.pretty, Console.error)), Effect.runFork);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
